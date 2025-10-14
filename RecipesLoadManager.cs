using Engine.Serialization;
using Game;
using RecipaediaEX.Implementation;
using RecipaediaEX.LoaderExtra.Loaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using XmlUtilities;
using ZLinq;

namespace RecipaediaEX {
    public static class RecipesLoadManager {
        public static List<IRecipesLoader> RecipesLoaders = new();
        public static bool DebugLogModToRecipeFileLoaders = true;
        public static List<Assembly> m_scannedAssemblies = new();

        public static void Initialize() {
            //获取读取器
            GetRecipesLoaders();
        }

        #region 内部方法
        /// <summary>
        /// 获取所有ModEntity对应的读取器
        /// </summary>
        static void GetRecipesLoaders() {
            //先实例化各读取器，
            foreach (Assembly item in TypeCache.LoadedAssemblies.AsValueEnumerable().Where(a => !TypeCache.IsKnownSystemAssembly(a))) {
                if (!m_scannedAssemblies.Contains(item)) {
                    foreach (TypeInfo definedType in item.DefinedTypes) {
                        if (typeof(IRecipesLoader).IsAssignableFrom(definedType) && !definedType.IsAbstract) {
                            try {
                                IRecipesLoader newLoader = (IRecipesLoader)Activator.CreateInstance(definedType.AsType());
                                RecipesLoaders.Add(newLoader);
                            }
                            catch (Exception ex) {
                                Engine.Log.Error($"[RecipaediaEX]Error Creating Instance of {definedType.FullName}");
                                Engine.Log.Error(ex);
                            }
                        }
                    }
                    m_scannedAssemblies.Add(item);
                }
            }
            RecipesLoaders.Sort((a, b) => a.Order.CompareTo(b.Order));
            if(DebugLogModToRecipeFileLoaders) {
                Engine.Log.Information($"[RecipaediaEX]读取到{RecipesLoaders.Count}个RecipesLoader");
                foreach (var loader in RecipesLoaders) {
                    Engine.Log.Information($"[RecipaediaEX] RecipeFileLoader {loader.GetType().FullName}");
                }
            }
        }
        #endregion

    }
}