using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Game;
using XmlUtilities;

namespace RecipaediaEX {
    [RecipeReader([typeof(OriginalCraftingRecipe), typeof(SmeltingRecipeWidget)])]
    public class FormattedRecipeReader : IRecipeReader {
        public IRecipe LoadRecipe(XElement item) {
            float requiredHeatLevel = XmlUtils.GetAttributeValue<float>(item, "RequiredHeatLevel");
            FormattedRecipe craftingRecipe = requiredHeatLevel > 0 ? new OriginalSmeltingRecipe() : new OriginalCraftingRecipe();
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
            craftingRecipe.RequiredHeatLevel = requiredHeatLevel;
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