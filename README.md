# RecipaediaEX 对接文档

## 概述

**RecipaediaEX** 是一个专为《Survivalcraft》游戏设计的高级配方系统模组。该模组为游戏提供了一个全新的、可扩展的配方管理框架，旨在解决原版配方系统的局限性，并为模组开发者提供强大的扩展能力。

### 核心特性

- **可扩展配方系统**：支持自定义配方格式和读取逻辑，不再受限于传统的 .cr 文件格式
- **智能配方匹配**：提供高效的配方匹配算法，支持配方变换（旋转、翻转等）
- **工作站关联**：将配方与具体的工作台/熔炉等工作站（Crafter）关联，使配方系统更加直观
- **完整图鉴界面**：内置配方图鉴系统，支持分类管理和详细信息展示
- **模组友好**：提供丰富的接口和特性系统，便于其他模组集成和扩展

### 主要优势

相比原版配方系统，RecipaediaEX 提供了以下显著改进：

- **灵活的文件格式**：不再强制要求 .cr 文件作为配方，支持自定义配方文件格式
- **自定义解析逻辑**：不再强制要求配方条目有特定的属性结构（如 Result、Description 等）
- **工作站关联系统**：将配方与工作台/熔炉等工作站（Crafter）关联，使配方用途更加明确
- **高度可扩展**：支持模组开发者自定义配方类型、读取器和文件加载器

### 技术架构

RecipaediaEX 采用模块化设计，主要包含以下核心组件：

- **配方管理核心**：负责配方的加载、解析和匹配
- **配方读取器系统**：支持多种配方格式的自定义解析
- **文件加载器框架**：允许模组开发者自定义配方文件的加载逻辑
- **工作站管理系统**：将配方与游戏中的制造设施关联
- **用户界面系统**：提供完整的配方图鉴和浏览界面

### 适用版本

- **游戏版本**：Survivalcraft 2.4+
- **API 版本**：1.8+
- **模组版本**：2.0.0

## 逻辑层适配

### 逻辑层接口说明

RecipaediaEX 的逻辑层接口位于 `UI` 命名空间之外，主要负责配方的加载、解析、匹配和管理。以下是各接口的具体用途：

#### 核心接口

- **`IRecipe`**：配方的基础接口，定义配方的核心属性和匹配逻辑
- **`IRecipeReader`**：配方解析器接口，负责将 XML 数据转换为配方实例
- **`IRecipeFileLoader`**：配方文件加载器接口，负责从模组中读取配方文件
- **`ICrafter`**：工作站接口，用于标识哪些方块可以作为特定配方类型的工作站

#### 配方流向

配方的处理流程如下：

```
XML 配方文件 → IRecipeFileLoader → IRecipeReader → RecipaediaEXManager（配方容器）
```

1. **XML 配方文件**：模组提供的配方数据
2. **IRecipeFileLoader**：负责读取和解析配方文件
3. **IRecipeReader**：将 XML 数据转换为具体的配方实例
4. **RecipaediaEXManager**：管理所有配方的容器，提供配方查找和匹配功能

### 添加自定义配方

#### 1. 创建自定义配方类

首先，创建继承自 `IRecipe` 接口的配方类：

```csharp
public class CustomRecipe : IRecipe
{
    public string Description { get; set; }
    public string Message { get; set; }
    public int DisplayOrder { get; set; }
    public ValuesDictionary ExtraValues = new();
    
    // 自定义属性
    public int CustomProperty { get; set; }
    
    public bool Match(IRecipe actual)
    {
        // 实现配方匹配逻辑
        if (actual is CustomRecipe other)
        {
            return CustomProperty == other.CustomProperty;
        }
        return false;
    }
    
    public virtual T GetExtraValue<T>(string key, T defaultValue) => ExtraValues.GetValue(key, defaultValue);

    public void SetExtraValue<T>(string key, T value) => ExtraValues.SetValue(key, value);
}
```

#### 2. 创建配方读取器

实现 `IRecipeReader` 接口，并使用 `RecipeReaderAttribute` 特性标记：

```csharp
[RecipeReader([typeof(CustomRecipe)])]
public class CustomRecipeReader : IRecipeReader
{
    public IRecipe LoadRecipe(XElement item)
    {
        var recipe = new CustomRecipe
        {
            Description = XmlUtils.GetAttributeValue<string>(item, "Description"),
            Message = XmlUtils.GetAttributeValue<string>(item, "Message"),
            DisplayOrder = XmlUtils.GetAttributeValue<int>(item, "DisplayOrder", 0),
            CustomProperty = XmlUtils.GetAttributeValue<int>(item, "CustomProperty")
        };
        
        return recipe;
    }
}
```

#### 3. 创建配方文件加载器（可选）

如果需要自定义配方文件格式，可以实现 `IRecipeFileLoader` 接口：

```csharp
[RecipeFileLoader(TargetModPackageName = "your.mod.package")]
public class CustomRecipeFileLoader : IRecipeFileLoader
{
    public int Order => 0; // 优先级，数字越大优先级越高
    
    public XElement GetRecipesXml(ModEntity modEntity)
    {
        // 自定义配方文件读取逻辑
        string filePath = Path.Combine(modEntity.modInfo.DirectoryPath, "custom_recipes.xml");
        if (File.Exists(filePath))
        {
            return XElement.Load(filePath);
        }
        return null;
    }
}
```

#### 4. 实现工作站组件

在工作站实体中实现配方匹配和应用功能。可以参考 `ComponentsExtra` 命名空间下的实现：

```csharp
public class CustomCrafterComponent : Component, IUpdateable
{
    public UpdateOrder UpdateOrder => UpdateOrder.Default;
    
    public void Update(float dt)
    {
        // 检查配方匹配
        var actualRecipe = CreateActualRecipe();
        var matchedRecipe = RecipaediaEXManager.FindMatchingRecipe<CustomRecipe>(actualRecipe);
        
        if (matchedRecipe != null)
        {
            // 应用配方结果
            ApplyRecipeResult(matchedRecipe);
        }
    }
    
    private CustomRecipe CreateActualRecipe()
    {
        // 根据当前槽位内容创建实际配方
        return new CustomRecipe { /* 设置实际配方数据 */ };
    }
    
    private void ApplyRecipeResult(CustomRecipe recipe)
    {
        // 实现配方应用逻辑
    }
}
```

#### 5. 实现 ICrafter 接口

创建实现 `ICrafter` 接口的类，用于标识工作站：

```csharp
public class CustomCrafter : ICrafter
{
    public bool IsCrafter(int blockValue, Type recipeType)
    {
        // 判断该方块是否可以作为指定配方类型的工作站
        return recipeType == typeof(CustomRecipe) && 
               Terrain.ExtractContents(blockValue) == CustomBlockIndex;
    }
}
```

### 重要说明

- **配方匹配逻辑**：工作站的配方匹配、应用功能需要开发者自己实现，RecipaediaEX 只提供配方查找功能
- **Crafter 接口**：`ICrafter` 接口在逻辑层没有具体功能，主要用于 UI 层告诉玩家配方应该使用什么工作站
- **性能优化**：建议在配方匹配时使用或参考 `FormattedRecipe` 的 `PreTransformIngredients()` 方法预计算配方变换，提高匹配效率

## UI层适配

### UI层接口说明

RecipaediaEX 的 UI 层接口位于 `UI` 命名空间下，主要负责配方图鉴界面的展示和交互。UI 层采用分层结构设计，提供灵活的界面定制能力。

#### UI层结构

```
配方图鉴页面（RecipaediaEXScreen）
├── 条目详情页面（RecipaediaEXDescriptionScreen）
└── 配方一览页面（RecipaediaEXRecipesScreen）
```

- **配方图鉴页面**：包含方块列表以及切换列表的功能
- **条目详情页面**：显示条目的详细信息和属性
- **配方一览页面**：展示与条目相关的所有配方

#### 核心接口

- **`IRecipaediaCategoryProvider`**：分类提供器接口，负责提供图鉴分类数据
- **`IRecipaediaCategory`**：分类配置接口，定义分类的基本信息和条目管理
- **`IRecipaediaItem`**：图鉴条目接口，定义条目的基本属性和界面跳转
- **`IRecipaediaDescriptionItem`**：详情条目接口，用于详情页面的展示
- **`IRecipaediaRecipeItem`**：配方条目接口，用于配方页面的展示
- **`RecipeDescriptor`**：配方预览器接口，用于自定义配方的UI展示

### 添加自定义UI元素

#### 1. 创建自定义分类（可选）

首先，创建分类提供器，实现 `IRecipaediaCategoryProvider` 接口：

```csharp
public class CustomCategoryProvider : IRecipaediaCategoryProvider
{
    public IEnumerable<IRecipaediaCategory> GetCategories()
    {
        yield return new CustomCategory("CustomItems", "自定义物品");
        yield return new CustomCategory("CustomBlocks", "自定义方块");
    }
}
```

然后，创建具体的分类类，实现 `IRecipaediaCategory` 接口：

```csharp
public class CustomCategory : IRecipaediaCategory
{
    private readonly string m_id;
    private readonly string m_displayName;
    private readonly List<IRecipaediaItem> m_items;

    public string Id => m_id;
    public string DisplayName => m_displayName;
    
    public CustomCategory(string id, string displayName)
    {
        m_id = id;
        m_displayName = displayName;
        m_items = new List<IRecipaediaItem>();
        
        // 添加分类下的条目
        PopulateItems();
    }

    public IEnumerable<IRecipaediaItem> GetItems() => m_items;
    
    public Func<IRecipaediaItem, Widget> ItemWidgetFactory => CreateItemWidget;

    private void PopulateItems()
    {
        // 添加自定义条目
        m_items.Add(new CustomItem("CustomItem1", "自定义物品1"));
        m_items.Add(new CustomItem("CustomItem2", "自定义物品2"));
    }

    private Widget CreateItemWidget(IRecipaediaItem item)
    {
        if (item is CustomItem customItem)
        {
            // 创建自定义的条目Widget
            var widget = new ContainerWidget();
            var label = new LabelWidget
            {
                Text = customItem.DisplayName,
                Font = ContentManager.Get<BitmapFont>("Fonts/Pericles")
            };
            widget.Children.Add(label);
            return widget;
        }
        return null;
    }
}
```

#### 2. 创建自定义条目

创建实现 `IRecipaediaItem` 接口的条目类：

```csharp
public class CustomItem : IRecipaediaItem
{
    public string ItemId { get; }
    public string DisplayName { get; }

    public CustomItem(string itemId, string displayName)
    {
        ItemId = itemId;
        DisplayName = displayName;
    }

    public object Value => ItemId;
    public string DetailScreenName => "RecipaediaDescription";
    public string RecipeScreenName => "RecipaediaRecipes";
    public bool RecipesButtonEnabled => true;
    public string RecipesButtonText => "查看配方";
    public bool DetailsButtonEnabled => true;
    public string DetailsButtonText => "查看详情";
}
```

#### 3. 自定义详情界面

如果需要自定义详情界面，可以实现 `IRecipaediaDescriptionItem` 接口：

```csharp
public class CustomDescriptionItem : CustomItem, IRecipaediaDescriptionItem
{
    public CustomDescriptionItem(string itemId, string displayName) 
        : base(itemId, displayName) { }

    public string Name => DisplayName;
    
    public Widget Icon
    {
        get
        {
            // 创建自定义图标
            var icon = new BlockIconWidget
            {
                Value = GetCustomBlockValue(),
                Size = new Vector2(48, 48)
            };
            return icon;
        }
    }
    
    public string Description => $"这是 {DisplayName} 的详细描述信息。";
    
    public Dictionary<string, string> GetProperties()
    {
        return new Dictionary<string, string>
        {
            { "物品ID", ItemId },
            { "显示名称", DisplayName },
            { "创建时间", DateTime.Now.ToString() }
        };
    }
    
    private int GetCustomBlockValue()
    {
        // 返回对应的方块值
        return Terrain.MakeBlockValue(/* 自定义方块索引 */);
    }
}
```

#### 4. 自定义配方图鉴界面

如果需要自定义配方图鉴界面，可以实现 `IRecipaediaRecipeItem` 接口：

```csharp
public class CustomRecipeItem : CustomItem, IRecipaediaRecipeItem
{
    public CustomRecipeItem(string itemId, string displayName) 
        : base(itemId, displayName) { }

    public bool Match(IRecipe recipe)
    {
        // 判断该条目是否匹配指定的配方
        if (recipe is CustomRecipe customRecipe)
        {
            // 实现自定义的匹配逻辑
            return customRecipe.Description.Contains(ItemId);
        }
        return false;
    }

    public bool IsIngredient(IRecipe recipe)
    {
        // 判断该条目是否是配方的原料
        if (recipe is CustomRecipe customRecipe)
        {
            // 实现自定义的原料判断逻辑
            return customRecipe.Ingredients.Any(ingredient => 
                ingredient != null && ingredient.Contains(ItemId));
        }
        return false;
    }
}
```

#### 5. 自定义配方预览界面

创建自定义的配方预览器，继承 `RecipeDescriptor` 类：

```csharp
[RecipeDescriptor([typeof(CustomRecipe)], Order = 0)]
public class CustomRecipeDescriptor : RecipeDescriptor
{
    private LabelWidget m_nameLabel;
    private LabelWidget m_descriptionLabel;
    private ButtonWidget m_craftButton;

    public CustomRecipeDescriptor(RecipaediaEXRecipesScreen belongingScreen) 
        : base(belongingScreen) { }

    public override void Show(IRecipe recipe, string nameSuffix)
    {
        if (recipe is CustomRecipe customRecipe)
        {
            // 显示配方信息
            m_nameLabel.Text = $"自定义配方 {nameSuffix}";
            m_descriptionLabel.Text = customRecipe.Description;
            m_craftButton.IsEnabled = true;
        }
    }

    public override void Hide()
    {
        // 清理界面状态
        m_nameLabel.Text = string.Empty;
        m_descriptionLabel.Text = string.Empty;
        m_craftButton.IsEnabled = false;
    }

    public override CrafterButtonWidget GetCrafterButton(IRecipe recipe)
    {
        // 返回自定义的工作站按钮
        var button = new CrafterButtonWidget
        {
            Text = "使用自定义工作站",
            Size = new Vector2(120, 40)
        };
        
        // 设置按钮点击事件
        button.Click += (sender, e) =>
        {
            // 处理工作站选择逻辑
            HandleCrafterSelection(recipe);
        };
        
        return button;
    }

    private void HandleCrafterSelection(IRecipe recipe)
    {
        // 实现工作站选择逻辑
        // 可以打开工作站选择界面或直接使用默认工作站
    }
}
```

### 使用预设界面

如果使用 RecipaediaEX 预设的界面，需要实现相应的接口：

- **使用预设详情界面**：实现 `IRecipaediaDescriptionItem` 接口
- **使用预设配方界面**：实现 `IRecipaediaRecipeItem` 接口

### 重要说明

- **分类提供器**：必须有无参构造函数，在游戏中以单例形式存在
- **条目Widget**：通过 `ItemWidgetFactory` 自定义条目的显示样式
- **配方预览器**：使用 `RecipeDescriptorAttribute` 特性标记，支持优先级设置
- **界面跳转**：通过 `DetailScreenName` 和 `RecipeScreenName` 指定跳转的界面名称