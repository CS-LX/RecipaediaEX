using System;
using System.Collections.Generic;
using Engine;
using GameEntitySystem;
using RecipaediaEX.Events;
using ZLinq;

namespace RecipaediaEX {
    public static class RecipaediaEXManager
    {
        public static List<IRecipe> m_recipes = [];

        /// <summary>
        /// 所有配方的集合
        /// </summary>
        public static List<IRecipe> Recipes => m_recipes;

        public static void Initialize() {
            Recipes.Clear();
            foreach (IRecipesLoader loader in RecipesLoadManager.RecipesLoaders) {
                try {
                    loader.Initialize();
                }
                catch(Exception e){
                    Log.Error($"[RecipaediaEX] {loader.GetType().FullName} initialize failed!");
                    Log.Error(e);
                }
            }
            ResetRecipes();
        }

        /// <summary>
        /// 进入存档后，由于方块ID变动，因此需要提醒所有mod重置配方表
        /// </summary>
        public static void ResetRecipes() {
            Recipes.Clear();
            foreach (IRecipesLoader loader in RecipesLoadManager.RecipesLoaders) {
                try {
                    IEnumerable<IRecipe> recipesInLoader = loader.GetRecipes();
                    m_recipes.AddRange(recipesInLoader);
                }
                catch (Exception e) {
                    Log.Error($"[RecipaediaEX] {loader.GetType().FullName} GetRecipes failed!");
                    Log.Error(e);
                }
            }
            Recipes.RemoveAll(r => r == null);
            RecipaediaEventBus.GetPublisher<RecipesResetEvent>().Publish(new RecipesResetEvent(Recipes.Count));
        }

        static void PublishRecipeMatched(IRecipe actual, IRecipe matched, bool fromDynamicLoader) {
            Project? project = actual.GetExtraValue<Project>(RecipeExtraKeys.Project, null);
            RecipaediaEventBus.GetPublisher<RecipeMatchedEvent>().Publish(
                new RecipeMatchedEvent(actual, matched, fromDynamicLoader, project));
        }

        #region 对外方法
        /// <summary>
        /// 寻找完整的配方以获得产物
        /// </summary>
        /// <param name="actual">玩家实际放置在生产方块中的配方</param>
        /// <returns>符合条件的第一个配方，如果没有则返回null</returns>
        public static IRecipe FindMatchingRecipe(IRecipe actual) {
            IRecipe recipe = m_recipes.AsValueEnumerable().Where(x => x.Match(actual)).OrderBy(x => x.MatchPriority).FirstOrDefault();
            if (recipe != null) {
                PublishRecipeMatched(actual, recipe, fromDynamicLoader: false);
            }
            return recipe;
        }
        /// <summary>
        /// 寻找动态配方
        /// </summary>
        public static IRecipe FindDynamicRecipe(IRecipe actual, Project project) {
            return RecipesLoadManager.DynamicRecipeLoaders.AsValueEnumerable().Select(dynamicRecipeLoader => dynamicRecipeLoader.GetDynamicRecipe(actual, project)).FirstOrDefault(dynamicRecipe => dynamicRecipe != null);
        }
        /// <summary>
        /// 寻找完整的配方以获得产物
        /// </summary>
        /// <param name="actual">玩家实际放置在生产方块中的配方</param>
        /// <typeparam name="T">配方类型</typeparam>
        /// <returns>符合条件的第一个配方，如果没有则返回null</returns>
        public static T FindMatchingRecipe<T>(IRecipe actual) where T : class, IRecipe {
            Project project = actual.GetExtraValue<Project>(RecipeExtraKeys.Project, null);
            if (project != null) {
                IRecipe dynamicRecipe = FindDynamicRecipe(actual, project);
                if (dynamicRecipe != null) {
                    PublishRecipeMatched(actual, dynamicRecipe, fromDynamicLoader: true);
                    return dynamicRecipe as T;
                }
            }
            IRecipe recipe = FindMatchingRecipe(actual);
            if (recipe == null) return null;
            return recipe as T;
        }

        /// <summary>
        /// 尝试寻找完整的配方以获得产物
        /// </summary>
        /// <param name="actual">玩家实际放置在生产方块中的配方</param>
        /// <param name="recipe">符合条件的第一个配方，如果没有则返回nul</param>
        /// <typeparam name="T">配方类型</typeparam>
        /// <returns>找没找到</returns>
        public static bool TryFindMatchingRecipe<T>(IRecipe actual, out T recipe) where T : class, IRecipe  {
            recipe = FindMatchingRecipe<T>(actual);
            if (recipe == null) {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 寻找一系列完整的配方以获得产物
        /// </summary>
        /// <param name="actual">玩家实际放置在生产方块中的配方</param>
        /// <returns>符合条件的所有配方</returns>
        public static IRecipe[] FindMatchingRecipes(IRecipe actual) {
            return m_recipes.AsValueEnumerable().Where(x => x.Match(actual)).OrderBy(x => x.MatchPriority).ToArray();
        }
        #endregion
    }
}
