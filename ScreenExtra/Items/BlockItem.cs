using System;
using System.Collections.Generic;
using System.Globalization;
using Game;
using RecipaediaEX.UI;
using ZLinq;

namespace RecipaediaEX.Implementation {
    public class BlockItem : IRecipaediaItem, IRecipaediaDescriptionItem, IRecipaediaRecipeItem {
        public int m_blockValue;
        public int m_order;
        public Block m_block;
        public int m_recipesCount;

        public BlockItem(Block block, int order, int blockValue) {
            m_block = block;
            m_blockValue = blockValue;
            m_order = order;
            m_recipesCount = RecipaediaEXManager.Recipes.AsValueEnumerable().Count(Match);
        }

        public object Value => m_blockValue;
        public string DetailScreenName => "RecipaediaDescription";
        public string RecipeScreenName => "RecipaediaRecipes";


        public string Name => m_block.GetDisplayName(null, m_blockValue);
        public Widget Icon => new BlockIconWidget { Value = m_blockValue, HorizontalAlignment = WidgetAlignment.Center, VerticalAlignment = WidgetAlignment.Center, };
        public string Description => m_block.GetDescription(m_blockValue);
        public Dictionary<string, string> GetProperties() {
            int value = m_blockValue;
            var dictionary = new Dictionary<string, string>();
            int num = Terrain.ExtractContents(value);
            Block block = BlocksManager.Blocks[num];
            if (block.GetEmittedLightAmount(value) > 0) {
                dictionary.Add("Luminosity", block.GetEmittedLightAmount(value).ToString());
            }
            if (block.GetFuelFireDuration(value) > 0f) {
                dictionary.Add("Fuel Value", block.GetFuelFireDuration(value).ToString());
            }
            dictionary.Add("Is Stackable", (block.GetMaxStacking(value) > 1) ? string.Format(LanguageControl.Get(nameof(RecipaediaDescriptionScreen), 1), block.GetMaxStacking(value).ToString()) : LanguageControl.No);
            dictionary.Add("Is Flammable", (block.GetFireDuration(value) > 0f) ? LanguageControl.Yes : LanguageControl.No);
            if (block.GetNutritionalValue(value) > 0f) {
                dictionary.Add("Nutrition", block.GetNutritionalValue(value).ToString());
            }
            if (block.GetRotPeriod(value) > 0) {
                dictionary.Add("Max Storage Time", string.Format(LanguageControl.Get(nameof(RecipaediaDescriptionScreen), 2), $"{2 * block.GetRotPeriod(value) * 60f / 1200f:0.0}"));
            }
            if (block.GetBlockDigMethod(value) != 0) {
                dictionary.Add("Digging Method", LanguageControl.Get("DigMethod", block.GetBlockDigMethod(value).ToString()));
                dictionary.Add("Digging Resilience", block.GetDigResilience(value).ToString());
            }
            if (block.GetExplosionResilience(value) > 0f) {
                dictionary.Add("Explosion Resilience", block.GetExplosionResilience(value).ToString());
            }
            if (block.GetExplosionPressure(value) > 0f) {
                dictionary.Add("Explosive Power", block.GetExplosionPressure(value).ToString());
            }
            bool flag = false;
            if (block.GetMeleePower(value) > 1f) {
                dictionary.Add("Melee Power", block.GetMeleePower(value).ToString());
                flag = true;
            }
            if (block.GetMeleePower(value) > 1f) {
                dictionary.Add("Melee Hit Ratio", $"{100f * block.GetMeleeHitProbability(value):0}%");
                flag = true;
            }
            if (block.GetProjectilePower(value) > 1f) {
                dictionary.Add("Projectile Power", block.GetProjectilePower(value).ToString());
                flag = true;
            }
            if (block.GetShovelPower(value) > 1f) {
                dictionary.Add("Shoveling", block.GetShovelPower(value).ToString());
                flag = true;
            }
            if (block.GetHackPower(value) > 1f) {
                dictionary.Add("Hacking", block.GetHackPower(value).ToString());
                flag = true;
            }
            if (block.GetQuarryPower(value) > 1f) {
                dictionary.Add("Quarrying", block.GetQuarryPower(value).ToString());
                flag = true;
            }
            if (flag && block.GetDurability(value) > 0) {
                dictionary.Add("Durability", block.GetDurability(value).ToString());
            }
            if (block.DefaultExperienceCount > 0f) {
                dictionary.Add("Experience Orbs", block.DefaultExperienceCount.ToString());
            }
            if (block.CanWear(value)) {
                ClothingData clothingData = block.GetClothingData(value);
                dictionary.Add("Can Be Dyed", clothingData.CanBeDyed ? LanguageControl.Yes : LanguageControl.No);
                dictionary.Add("Armor Protection", $"{(int)(clothingData.ArmorProtection * 100f)}%");
                dictionary.Add("Armor Durability", clothingData.Sturdiness.ToString());
                dictionary.Add("Insulation", $"{clothingData.Insulation:0.0} clo");
                dictionary.Add("Movement Speed", $"{clothingData.MovementSpeedFactor * 100f:0}%");
            }
#if DEBUG
            if (GameManager.Project != null
                && block.BlockIndex > 0) {
                dictionary.Add("Dynamic Index", block.BlockIndex.ToString());
            }
#endif
            ModsManager.HookAction(
                "EditBlockDescriptionScreen",
                loader => {
                    loader.EditBlockDescriptionScreen(dictionary);
                    loader.EditBlockDescriptionScreen(dictionary, value);
                    return false;
                }
            );
            Dictionary<string, string> translatedTexts = dictionary.AsValueEnumerable().Select(pair => new KeyValuePair<string, string>(LanguageControl.Get(RecipaediaDescriptionScreen.fName, pair.Key), pair.Value)).ToDictionary();
            return translatedTexts;
        }
        public bool RecipesButtonEnabled => m_recipesCount > 0;
        public string RecipesButtonText => m_recipesCount > 0 ? $"{m_recipesCount} {((m_recipesCount == 1) ? LanguageControl.Get(nameof(RecipaediaScreen), 1) : LanguageControl.Get(nameof(RecipaediaScreen), 2))}" : LanguageControl.Get(nameof(RecipaediaScreen), 3);
        public bool DetailsButtonEnabled => true;
        public string DetailsButtonText => LanguageControl.Get("ContentWidgets", nameof(RecipaediaScreen), "1");


        public bool Match(IRecipe recipe) {
            if (recipe is not FormattedRecipe formattedRecipe) return false;
            return m_blockValue == formattedRecipe.ResultValue;
        }
        public bool IsIngredient(IRecipe recipe) {
            if (recipe is not FormattedRecipe formattedRecipe) return false;

            int data = Terrain.ExtractData(m_blockValue);
            string actualIngredient = m_block.GetCraftingId(m_blockValue) + ":" + data.ToString(CultureInfo.InvariantCulture);
            foreach (var ingredient in formattedRecipe.Ingredients) {
                if (CraftingRecipesManager.CompareIngredients(ingredient, actualIngredient)) return true;
            }
            return false;
        }
    }
}