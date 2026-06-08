using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Game;
using RecipaediaEX.Search;

namespace RecipaediaEX.UI {
    public class RecipaediaSearchFilterDialog : Dialog {
        public const string fName = "RecipaediaSearchFilterDialog";

        public Action<RecipaediaSearchFilterState> m_applyHandler;
        public RecipaediaSearchFilterState m_state = new();

        TextBoxWidget m_nameTextBox;
        CheckboxWidget m_hasRecipeCheckbox;
        CheckboxWidget m_canUseCheckbox;
        ButtonWidget m_itemTypeButton;
        TextBoxWidget m_packTextBox;
        TextBoxWidget m_modTextBox;
        TextBoxWidget m_crafterTextBox;
        TextBoxWidget m_recipeTypeTextBox;
        TextBoxWidget m_ingredientTextBox;
        TextBoxWidget m_productTextBox;
        TextBoxWidget m_excludeTextBox;
        ButtonWidget m_resetButton;
        ButtonWidget m_cancelButton;
        ButtonWidget m_applyButton;

        public RecipaediaSearchFilterDialog(RecipaediaSearchFilterState initialState, Action<RecipaediaSearchFilterState> applyHandler) {
            m_applyHandler = applyHandler;
            m_state = CloneState(initialState);
            XElement node = RecipaediaEXLoader.RequestDialogFile("RecipaediaSearchFilterDialog");
            LoadContents(this, node);

            m_nameTextBox = Children.Find<TextBoxWidget>("RecipaediaSearchFilterDialog.Name");
            m_hasRecipeCheckbox = Children.Find<CheckboxWidget>("RecipaediaSearchFilterDialog.HasRecipe");
            m_canUseCheckbox = Children.Find<CheckboxWidget>("RecipaediaSearchFilterDialog.CanUse");
            m_itemTypeButton = Children.Find<ButtonWidget>("RecipaediaSearchFilterDialog.ItemType");
            m_packTextBox = Children.Find<TextBoxWidget>("RecipaediaSearchFilterDialog.Pack");
            m_modTextBox = Children.Find<TextBoxWidget>("RecipaediaSearchFilterDialog.Mod");
            m_crafterTextBox = Children.Find<TextBoxWidget>("RecipaediaSearchFilterDialog.Crafter");
            m_recipeTypeTextBox = Children.Find<TextBoxWidget>("RecipaediaSearchFilterDialog.RecipeType");
            m_ingredientTextBox = Children.Find<TextBoxWidget>("RecipaediaSearchFilterDialog.Ingredient");
            m_productTextBox = Children.Find<TextBoxWidget>("RecipaediaSearchFilterDialog.Product");
            m_excludeTextBox = Children.Find<TextBoxWidget>("RecipaediaSearchFilterDialog.Exclude");
            m_resetButton = Children.Find<ButtonWidget>("RecipaediaSearchFilterDialog.Reset");
            m_cancelButton = Children.Find<ButtonWidget>("RecipaediaSearchFilterDialog.Cancel");
            m_applyButton = Children.Find<ButtonWidget>("RecipaediaSearchFilterDialog.Apply");

            LoadStateToControls();
        }

        public override void Update() {
            if (m_itemTypeButton.IsClicked) {
                List<string> types = ["", "block", "custom"];
                DialogsManager.ShowDialog(
                    this,
                    new ListSelectionDialog(
                        LanguageControl.GetContentWidgets(fName, 4),
                        types,
                        50f,
                        GetItemTypeLabel,
                        item => {
                            m_state.ItemType = (string)item;
                            UpdateItemTypeButton();
                        }
                    )
                );
            }
            if (m_resetButton.IsClicked) {
                m_state = new RecipaediaSearchFilterState();
                LoadStateToControls();
            }
            if (m_cancelButton.IsClicked || Input.Cancel) {
                Dismiss(null);
            }
            if (m_applyButton.IsClicked || Input.Ok) {
                ReadControlsToState();
                Dismiss(m_state);
            }
        }

        void LoadStateToControls() {
            m_nameTextBox.Text = m_state.NameText ?? string.Empty;
            m_hasRecipeCheckbox.IsChecked = m_state.HasRecipe;
            m_canUseCheckbox.IsChecked = m_state.CanUse;
            UpdateItemTypeButton();
            m_packTextBox.Text = m_state.PackId ?? string.Empty;
            m_modTextBox.Text = m_state.ModName ?? string.Empty;
            m_crafterTextBox.Text = m_state.CrafterName ?? string.Empty;
            m_recipeTypeTextBox.Text = m_state.RecipeType ?? string.Empty;
            m_ingredientTextBox.Text = m_state.IngredientName ?? string.Empty;
            m_productTextBox.Text = m_state.ProductName ?? string.Empty;
            m_excludeTextBox.Text = m_state.ExcludeText ?? string.Empty;
        }

        void ReadControlsToState() {
            m_state.NameText = m_nameTextBox.Text?.Trim() ?? string.Empty;
            m_state.HasRecipe = m_hasRecipeCheckbox.IsChecked;
            m_state.CanUse = m_canUseCheckbox.IsChecked;
            m_state.PackId = m_packTextBox.Text?.Trim() ?? string.Empty;
            m_state.ModName = m_modTextBox.Text?.Trim() ?? string.Empty;
            m_state.CrafterName = m_crafterTextBox.Text?.Trim() ?? string.Empty;
            m_state.RecipeType = m_recipeTypeTextBox.Text?.Trim() ?? string.Empty;
            m_state.IngredientName = m_ingredientTextBox.Text?.Trim() ?? string.Empty;
            m_state.ProductName = m_productTextBox.Text?.Trim() ?? string.Empty;
            m_state.ExcludeText = m_excludeTextBox.Text?.Trim() ?? string.Empty;
        }

        void UpdateItemTypeButton() {
            m_itemTypeButton.Text = GetItemTypeLabel(m_state.ItemType ?? string.Empty);
        }

        static string GetItemTypeLabel(object item) {
            string value = item?.ToString() ?? string.Empty;
            return value switch {
                "block" => LanguageControl.GetContentWidgets(fName, "block"),
                "custom" => LanguageControl.GetContentWidgets(fName, "custom"),
                _ => LanguageControl.GetContentWidgets(fName, 5),
            };
        }

        static RecipaediaSearchFilterState CloneState(RecipaediaSearchFilterState state) => new() {
            NameText = state.NameText,
            HasRecipe = state.HasRecipe,
            CanUse = state.CanUse,
            ItemType = state.ItemType,
            PackId = state.PackId,
            ModName = state.ModName,
            CrafterName = state.CrafterName,
            RecipeType = state.RecipeType,
            IngredientName = state.IngredientName,
            ProductName = state.ProductName,
            ExcludeText = state.ExcludeText,
        };

        void Dismiss(RecipaediaSearchFilterState result) {
            DialogsManager.HideDialog(this);
            if (result != null) m_applyHandler?.Invoke(result);
        }
    }
}
