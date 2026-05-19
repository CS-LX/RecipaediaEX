using Game;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using System.Xml.Linq;
using XmlUtilities;
using ZLinq;

using RecipaediaEX;

namespace RecipaediaEX.Implementation {
    /// <summary>
    /// <para>默认的配方文件读取器，读取所有的.cr文件</para>
    /// <para>模组如果建立新的配方文件，尽可能避免.cr后缀名，避免配方意外地被其他模组交叉读取</para>
    /// <para>此读取器优先级是0</para>
    /// </summary>
    public abstract class XmlRecipesLoader : IRecipesLoader {
        public List<XElement> XElements { get; } = new();

        public virtual string ControlledFileExtension => ".cr";

        public virtual int Order => 0;

        public List<IRecipe> RecipeListFromLoader = new();

        public virtual void Initialize() {
            if (string.IsNullOrEmpty(ControlledFileExtension)) return;
            foreach (ModEntity modEntity in ModsManager.ModList) {
                modEntity.GetFiles(ControlledFileExtension, (_, stream) => {
                    XElement xElement = XmlUtils.LoadXmlFromStream(stream, Encoding.UTF8, true);
                    if ((xElement != null)) {
                        XElements.Add(xElement);
                    }
                });
            }
        }

        public virtual IEnumerable<IRecipe> GetRecipes() {
            RecipeListFromLoader.Clear();
            foreach (XElement xElement in XElements) {
                try {
                    LoadRecipeItems(xElement);
                }
                catch(Exception e) {
                    Engine.Log.Error($"[RecipaediaEX.XmlRecipesLoader]Error loading Recipe Item{xElement.ToString()}");
                    Engine.Log.Error(e);
                }
            }
            return RecipeListFromLoader;
        }

        /// <summary>
        /// 读取配方xml中Recipe开头的条目
        /// </summary>
        protected virtual void LoadRecipeItems(XElement recipesXml) {
            if (recipesXml == null) return;
            foreach (XElement element in recipesXml.Elements()) {
                if (element.Name.LocalName == "Recipe") {
                    RecipeListFromLoader.Add(ReadFormattedRecipe(element));
                }
                else {
                    LoadRecipeItems(element);
                }
            }
        }

        public virtual IRecipe ReadFormattedRecipe(XElement item) {
            float requiredHeatLevel = XmlUtils.GetAttributeValue<float>(item, "RequiredHeatLevel");
            FormattedRecipe craftingRecipe = requiredHeatLevel > 0 ? new OriginalSmeltingRecipe() : new OriginalCraftingRecipe();
            string attributeValue = XmlUtils.GetAttributeValue<string>(item, "Result");
            string desc = XmlUtils.GetAttributeValue<string>(item, "Description");
            if (desc.StartsWith("[")
                && desc.EndsWith("]")
                && LanguageControl.TryGetBlock(attributeValue, "CRDescription:" + desc.Substring(1, desc.Length - 2), out string r))
                desc = r;
            craftingRecipe.ResultValue = CraftingRecipesManager.DecodeResult(attributeValue);
            craftingRecipe.SetExtraValue(RecipeExtraKeys.MatchedResultBlockValues, new int[] { craftingRecipe.ResultValue });
            craftingRecipe.ResultCount = XmlUtils.GetAttributeValue<int>(item, "ResultCount");
            string attributeValue2 = XmlUtils.GetAttributeValue(item, "Remains", string.Empty);
            if (!string.IsNullOrEmpty(attributeValue2)) {
                craftingRecipe.RemainsValue = CraftingRecipesManager.DecodeResult(attributeValue2);
                craftingRecipe.RemainsCount = XmlUtils.GetAttributeValue<int>(item, "RemainsCount");
            }
            craftingRecipe.RequiredHeatLevel = requiredHeatLevel;
            craftingRecipe.RequiredPlayerLevel = XmlUtils.GetAttributeValue(item, "RequiredPlayerLevel", 1f);
            craftingRecipe.Description = desc;
            craftingRecipe.Message = XmlUtils.GetAttributeValue<string>(item, "Message", null);
            craftingRecipe.DisplayOrder = XmlUtils.GetAttributeValue<int>(item, "DisplayOrder", 0);
            Dictionary<char, string> dictionary = new Dictionary<char, string>();
            foreach (XAttribute item2 in from a in item.Attributes().AsValueEnumerable() where a.Name.LocalName.Length == 1 && char.IsLower(a.Name.LocalName[0]) select a) {
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
            string[] array = item.Value.Trim().Split(["\n"], StringSplitOptions.None);
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
                        craftingRecipe.Ingredients[j + (i * 6)] = text2;
                    }
                }
            }
            craftingRecipe.PreTransformIngredients();
            return craftingRecipe;
        }
    }
}