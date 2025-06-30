# 新配方系统前置mod开发文档

### 接口说明：

##### IRecipe

表示一个配方

- Description：配方的描述 【属性】

- Message：配方无法合成时向玩家提供的消息 【属性】

- DisplayOrder：在配方表中的显示顺序，DisplayOrder越小，配方越靠前 【属性】

- Match(IRecipe other)：是否与其他配方匹配 【方法】

接口代码如下：

```csharp
    /// <summary>
    /// 表示一个配方
    /// </summary>
    public interface IRecipe {
        /// <summary>
        /// 配方的解释
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// 配方无法合成时向玩家提供的消息
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 在配方表中的显示顺序，DisplayOrder越小，配方越靠前
        /// </summary>
        public int DisplayOrder { get; }

        /// <summary>
        /// 是否与其他配方匹配
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Match(IRecipe other);
    }
```

---

##### IRecipeReader

将xml条目读取为配方实例的解析器

此解析器一定要打上RecipeReader标签（指定它解析出的配方是哪种）！

- LoadRecipe(XElement item)：从xml条目中加载配方 【方法】

xml条目：指Recipe标签开头的条目，例子如下

```xml
    <Recipe Result="MarbleBlock" ResultCount="1" RequiredHeatLevel="1" a="limestone" b="sand" Description="[0]">
      "ab"
    </Recipe>
```

接口代码如下：

```csharp
    /// <summary>
    /// 将xml条目读取为配方实例的解析器
    /// </summary>
    public interface IRecipeReader {
        /// <summary>
        /// <para>从xml条目中加载配方</para>
        /// <para>xml条目如下：</para>
        /// <para>&lt;Recipe Result="MarbleBlock" ResultCount="1" RequiredHeatLevel="1" a="limestone" b="sand" Description="[0]"&gt;</para>
        ///      "ab"
        /// <para>&lt;/Recipe&gt;</para>
        /// </summary>
        /// <param name="item">xml条目</param>
        /// <returns></returns>
        public IRecipe LoadRecipe(XElement item);
    }t item);
    }
```

---

##### RecipeReaderAttribute

给IRecipeReader的标签，表示这个Reader读取的配方的类型

代码如下：

```csharp
    /// <summary>
    /// 给IRecipeReader的标签，表示这个Reader读取的配方的类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RecipeReaderAttribute : Attribute {
        public Type m_type;
        public Type Type => m_type;

        public RecipeReaderAttribute(Type type) {
            m_type = type;
        }
    }
```

### 样例：

如果将原版的配方系统换成这个新的配方系统，则逻辑如下：

##### CraftingRecipeReader.cs

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Game;
using XmlUtilities;

namespace RecipaediaEX {
    [RecipeReader(typeof(CraftingRecipe))]
    public class CraftingRecipeReader : IRecipeReader {
        public IRecipe LoadRecipe(XElement item) {
            var craftingRecipe = new CraftingRecipe();
			string attributeValue = XmlUtils.GetAttributeValue<string>(item, "Result");
            string desc = XmlUtils.GetAttributeValue<string>(item, "Description");
            if (desc.StartsWith("[")
                && desc.EndsWith("]")
                && LanguageControl.TryGetBlock(attributeValue, "CRDescription:" + desc.Substring(1, desc.Length - 2), out var r))
                desc = r;
            craftingRecipe.ResultValue = CraftingRecipesManager.DecodeResult(attributeValue);
            craftingRecipe.ResultCount = XmlUtils.GetAttributeValue<int>(item, "ResultCount");
            string attributeValue2 = XmlUtils.GetAttributeValue(item, "Remains", string.Empty);
            if (!string.IsNullOrEmpty(attributeValue2)) {
                craftingRecipe.RemainsValue = CraftingRecipesManager.DecodeResult(attributeValue2);
                craftingRecipe.RemainsCount = XmlUtils.GetAttributeValue<int>(item, "RemainsCount");
            }
            craftingRecipe.RequiredHeatLevel = XmlUtils.GetAttributeValue<float>(item, "RequiredHeatLevel");
            craftingRecipe.RequiredPlayerLevel = XmlUtils.GetAttributeValue(item, "RequiredPlayerLevel", 1f);
            craftingRecipe.Description = desc;
            craftingRecipe.Message = XmlUtils.GetAttributeValue<string>(item, "Message", null);
            craftingRecipe.DisplayOrder = XmlUtils.GetAttributeValue<int>(item, "DisplayOrder", 0);
            var dictionary = new Dictionary<char, string>();
            foreach (XAttribute item2 in from a in item.Attributes() where a.Name.LocalName.Length == 1 && char.IsLower(a.Name.LocalName[0]) select a) {
                CraftingRecipesManager.DecodeIngredient(item2.Value, out string craftingId, out int? data);
                if (BlocksManager.FindBlocksByCraftingId(craftingId).Length == 0) {
                    throw new InvalidOperationException($"Block with craftingId \"{item2.Value}\" not found.");
                }
                if (data.HasValue
                    && (data.Value < 0 || data.Value > 262143)) {
                    throw new InvalidOperationException($"Data in recipe ingredient \"{item2.Value}\" must be between 0 and 0x3FFFF.");
                }
                dictionary.Add(item2.Name.LocalName[0], item2.Value);
            }
            string[] array = item.Value.Trim().Split(new string[] { "\n" }, StringSplitOptions.None);
            for (int i = 0; i < array.Length; i++) {
                int num = array[i].IndexOf('"');
                int num2 = array[i].LastIndexOf('"');
                if (num < 0
                    || num2 < 0
                    || num2 <= num) {
                    throw new InvalidOperationException("Invalid recipe line.");
                }
                string text = array[i].Substring(num + 1, num2 - num - 1);
                for (int j = 0; j < text.Length; j++) {
                    char c = text[j];
                    if (char.IsLower(c)) {
                        string text2 = dictionary[c];
                        craftingRecipe.Ingredients[j + (i * 3)] = text2;
                    }
                }
            }

			return craftingRecipe;
        }
    }
}
```

##### CraftingRecipe.cs

```csharp
using Game;

namespace RecipaediaEX {
    public class CraftingRecipe : IRecipe {
        public const int MaxSize = 3;

        public int ResultValue;

        public int ResultCount;

        public int RemainsValue;

        public int RemainsCount;

        public float RequiredHeatLevel;

        public float RequiredPlayerLevel;

        public string[] Ingredients = new string[9];

        public string Description;

        public string Message;

        /// <summary>
        /// 在配方表中的显示顺序，DisplayOrder越小，配方越靠前
        /// </summary>
        public int DisplayOrder = 0;

        string IRecipe.Description => Description;
        string IRecipe.Message => Message;
        int IRecipe.DisplayOrder => DisplayOrder;

        public bool Match(IRecipe other) {
            if (other == null || other is not CraftingRecipe craftingRecipe) return false;

            return CraftingRecipesManager.MatchRecipe(Ingredients, craftingRecipe.Ingredients);
        }
    }
}
```


