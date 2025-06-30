using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Engine;
using Engine.Serialization;
using Game;

namespace RecipaediaEX
{
    public static class RecipaediaEXManager
    {
        public static List<IRecipe> m_recipes = [];
        public static List<IRecipe> Recipes => m_recipes;

        public static Dictionary<string, IRecipeReader> m_readers = [];
        public static List<Assembly> m_scannedAssemblies = [];

        public static void Initialize() {
            m_recipes.Clear();
            m_readers.Clear();
            //获取所有配方解析器
            GetRecipeReaders();
            //读取所有mod中的.cr文件
            XElement source = null;
            foreach (ModEntity modEntity in ModsManager.ModList) {
                modEntity.LoadCr(ref source);
            }
            //解析配方
            LoadRecipesData(source);
        }

        /// <summary>
        /// 获取配方解析器
        /// </summary>
        static void GetRecipeReaders() {
            foreach (Assembly item in TypeCache.LoadedAssemblies.Where((Assembly a) => !TypeCache.IsKnownSystemAssembly(a))) {
                if (!m_scannedAssemblies.Contains(item)) {
                    foreach (TypeInfo definedType in item.DefinedTypes) {
                        RecipeReaderAttribute customAttribute = definedType.GetCustomAttribute<RecipeReaderAttribute>();
                        if (customAttribute != null) {
                            Type[] types = customAttribute.Types;
                            foreach (var type in types) {
                                m_readers[type.FullName] = (IRecipeReader)Activator.CreateInstance(definedType.AsType());
                            }
                        }
                    }
                    m_scannedAssemblies.Add(item);
                }
            }
        }

        /// <summary>
        /// 解析整个配方系列xml
        /// </summary>
        /// <param name="item"></param>
        static void LoadRecipesData(XElement item) {
            try {
                if (ModsManager.HasAttribute(item, (name) => { return name == "Result"; }, out XAttribute xAttribute) == false) {
                    foreach (XElement xElement in item.Elements()) {
                        LoadRecipesData(xElement);
                    }
                    return;
                }

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

        static IRecipe ReadRecipeItem(XElement item) {
            string type = string.Empty;
            type = ModsManager.HasAttribute(item, (name) => name == "Reader", out XAttribute xAttribute) == false ? typeof(OriginalCraftingRecipe).FullName : xAttribute.Value;
            return m_readers[type].LoadRecipe(item);
        }
    }
}
