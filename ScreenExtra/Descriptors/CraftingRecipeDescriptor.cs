using System.Xml.Linq;
using Engine;
using Game;
using RecipaediaEX.UI;
using ZLinq;

namespace RecipaediaEX.Implementation {
    [RecipeDescriptor([typeof(OriginalCraftingRecipe)])]
    public class CraftingRecipeDescriptor : RecipeDescriptor {

		public LabelWidget m_nameWidget;

		public LabelWidget m_descriptionWidget;

		public GridPanelWidget m_gridWidget;

		public BlockRecipeSlotWidget m_resultWidget;

		public  CraftingRecipeDescriptor(RecipaediaEXRecipesScreen belongingScreen) : base(belongingScreen) {
            XElement node = RecipaediaEXLoader.RequestWidgetFile("Descriptors/CraftingRecipeDescriptor");
            LoadContents(this, node);
            m_nameWidget = Children.Find<LabelWidget>("CraftingRecipeDescriptor.Name");
            m_descriptionWidget = Children.Find<LabelWidget>("CraftingRecipeDescriptor.Description");
            m_gridWidget = Children.Find<GridPanelWidget>("CraftingRecipeDescriptor.Ingredients");
            m_resultWidget = Children.Find<BlockRecipeSlotWidget>("CraftingRecipeDescriptor.Result");
            for (int i = 0; i < m_gridWidget.RowsCount; i++) {
                for (int j = 0; j < m_gridWidget.ColumnsCount; j++) {
                    var widget = new BlockRecipeSlotWidget();
                    m_gridWidget.Children.Add(widget);
                    m_gridWidget.SetWidgetCell(widget, new Point2(j, i));
                }
            }
		}

        public override void Show(IRecipe recipe, string nameSuffix) {
            if (recipe is not OriginalCraftingRecipe craftingRecipe) return;

            Block block = BlocksManager.Blocks[Terrain.ExtractContents(craftingRecipe.ResultValue)];
            m_nameWidget.Text = block.GetDisplayName(null, craftingRecipe.ResultValue) + ((!string.IsNullOrEmpty(nameSuffix)) ? nameSuffix : string.Empty);
            m_descriptionWidget.Text = craftingRecipe.Description;
            m_nameWidget.IsVisible = true;
            m_descriptionWidget.IsVisible = true;
            foreach (var widget in m_gridWidget.Children) {
                var child = (BlockRecipeSlotWidget)widget;
                Point2 widgetCell = m_gridWidget.GetWidgetCell(child);
                string ingredient = craftingRecipe.Ingredients[widgetCell.X + (widgetCell.Y * 3)];
                if (!string.IsNullOrEmpty(ingredient)) {
                    CraftingRecipesManager.DecodeIngredient(ingredient, out string craftingId, out int? data);
                    var blocksByCraftingId = BlocksManager.FindBlocksByCraftingId(craftingId).AsValueEnumerable();
                    child.SetIngredients(blocksByCraftingId.Select(x => new BlockItem(x, 0, Terrain.MakeBlockValue(x.BlockIndex, 0, data.GetValueOrDefault(0)))).OfType<IRecipaediaItem>().ToArray(), m_belongingScreen);
                }
            }
            m_resultWidget.SetResult(new BlockItem(BlocksManager.Blocks[Terrain.ExtractContents(craftingRecipe.ResultValue)], 0, craftingRecipe.ResultValue), m_belongingScreen, craftingRecipe.ResultCount);
        }

        public override void Hide() {
            m_nameWidget.IsVisible = false;
            m_descriptionWidget.IsVisible = false;
            foreach (var widget in m_gridWidget.Children) {
                var child2 = (BlockRecipeSlotWidget)widget;
                child2.ClearContents();
            }
            m_resultWidget.ClearContents();
        }
    }
}