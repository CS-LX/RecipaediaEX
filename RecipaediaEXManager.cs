using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Engine;
using Engine.Serialization;
using Game;
using RecipaediaEX.Implementation;
using ZLinq;

namespace RecipaediaEX
{
    public static class RecipaediaEXManager
    {
        public static List<IRecipe> m_recipes = [];
        public static Dictionary<string, IRecipeReader> m_readers = [];
        public static List<Assembly> m_scannedAssembliesForReaders = [];

        /// <summary>
        /// 所有配方的集合
        /// </summary>
        public static List<IRecipe> Recipes => m_recipes;

        public static void Initialize() {
            m_recipes.Clear();
            m_readers.Clear();
            //获取所有配方解析器
            GetRecipeReaders();
            //读取所有mod中的.cr文件
            List<XElement> recipesItems = RecipesLoadManager.RecipesItems;
            //解析配方
            LoadRecipesData(recipesItems);
        }

        #region 内部方法
        /// <summary>
        /// 获取配方解析器
        /// </summary>
        static void GetRecipeReaders() {
            foreach (Assembly item in TypeCache.LoadedAssemblies.AsValueEnumerable().Where(a => !TypeCache.IsKnownSystemAssembly(a))) {
                if (!m_scannedAssembliesForReaders.Contains(item)) {
                    foreach (TypeInfo definedType in item.DefinedTypes) {
                        RecipeReaderAttribute customAttribute = definedType.GetCustomAttribute<RecipeReaderAttribute>();
                        if (customAttribute != null) {
                            Type[] types = customAttribute.Types;
                            foreach (var type in types) {
                                m_readers[type.FullName] = (IRecipeReader)Activator.CreateInstance(definedType.AsType());
                            }
                        }
                    }
                    m_scannedAssembliesForReaders.Add(item);
                }
            }
        }

        /// <summary>
        /// 解析配方条目xml
        /// </summary>
        /// <param name="items"></param>
        static void LoadRecipesData(List<XElement> items) {
            foreach (var item in items) {
                try {
                    bool flag = false;
                    //ModsManager.HookAction("OnCraftingRecipeDecode", modLoader =>
                    //{
                    //    modLoader.OnCraftingRecipeDecode(m_recipes, item, out flag);
                    //    return flag;
                    //});
                    if (flag == false) {
                        IRecipe craftingRecipe = ReadRecipeItem(item);
                        m_recipes.Add(craftingRecipe);
                    }
                }
                catch (Exception e) {
                    Log.Error(e);
                }
            }
        }

        /// <summary>
        /// 解析单个配方xml至IRecipe
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        static IRecipe ReadRecipeItem(XElement item) {
            bool hasAttributes = ModsManager.HasAttribute(item, (name) => name == "Type", out XAttribute xAttribute);
            bool isSmelting = ModsManager.HasAttribute(item, (name) => name == "RequiredHeatLevel", out XAttribute xAttribute2) && float.Parse(xAttribute2.Value) > 0;
            string type = !hasAttributes ? (isSmelting ? typeof(OriginalSmeltingRecipe).FullName : typeof(OriginalCraftingRecipe).FullName) : xAttribute.Value;
            return m_readers[type].LoadRecipe(item);
        }
        #endregion

        #region 对外方法
        /// <summary>
        /// 寻找完整的配方以获得产物
        /// </summary>
        /// <param name="actual">玩家实际放置在生产方块中的配方</param>
        /// <returns>符合条件的第一个配方，如果没有则返回null</returns>
        public static IRecipe FindMatchingRecipe(IRecipe actual) {
            return m_recipes.AsValueEnumerable().FirstOrDefault(x => x.Match(actual));
        }
        /// <summary>
        /// 寻找完整的配方以获得产物
        /// </summary>
        /// <param name="actual">玩家实际放置在生产方块中的配方</param>
        /// <typeparam name="T">配方类型</typeparam>
        /// <returns>符合条件的第一个配方，如果没有则返回null</returns>
        public static T FindMatchingRecipe<T>(IRecipe actual) where T : class, IRecipe {
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
            return m_recipes.AsValueEnumerable().Where(x => x.Match(actual)).ToArray();
        }
        #endregion
    }
}
