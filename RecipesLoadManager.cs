using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Engine.Serialization;
using Game;
using XmlUtilities;
using ZLinq;

namespace RecipaediaEX {
    public static class RecipesLoadManager {
        public static Dictionary<ModEntity, IRecipeFileLoader> m_modToRecipeFileLoaders = new();
        static Dictionary<string, IRecipeFileLoader> m_modPackageNameToFileLoader = new();//mod包名对应读取器，一般作为中间结构使用
        static DefaultRecipeFileLoader m_defaultRecipeFileLoader;//默认的读取器
        public static List<Assembly> m_scannedAssemblies = new();

        public static List<XElement> RecipesItems;

        public static void Initialize() {
            RecipesItems = new List<XElement>();
            //获取读取器
            GetFileLoaders();
            //读取xml
            LoadCrX();
        }

        #region 内部方法
        /// <summary>
        /// 获取所有ModEntity对应的读取器
        /// </summary>
        static void GetFileLoaders() {
            //先实例化各读取器，
            foreach (Assembly item in TypeCache.LoadedAssemblies.AsValueEnumerable().Where(a => !TypeCache.IsKnownSystemAssembly(a))) {
                if (!m_scannedAssemblies.Contains(item)) {
                    foreach (TypeInfo definedType in item.DefinedTypes) {
                        RecipeFileLoaderAttribute customAttribute = definedType.GetCustomAttribute<RecipeFileLoaderAttribute>();
                        if (customAttribute != null) {
                            string modPackageName = customAttribute.TargetModPackageName;
                            IRecipeFileLoader newLoader = (IRecipeFileLoader)Activator.CreateInstance(definedType.AsType());
                            if (m_modPackageNameToFileLoader.TryGetValue(modPackageName, out IRecipeFileLoader existLoader)) {
                                m_modPackageNameToFileLoader[modPackageName] = existLoader.Order > newLoader.Order ? existLoader : newLoader;
                            }
                            else {
                                m_modPackageNameToFileLoader[modPackageName] = newLoader;
                            }
                        }
                    }
                    m_scannedAssemblies.Add(item);
                }
            }
            m_defaultRecipeFileLoader = new DefaultRecipeFileLoader();

            //再与ModEntity建立对应关系
            foreach (var modEntity in ModsManager.ModList) {
                if (m_modPackageNameToFileLoader.TryGetValue(modEntity.modInfo.PackageName, out IRecipeFileLoader loader)) {
                    m_modToRecipeFileLoaders[modEntity] = loader;
                }
                else {
                    m_modToRecipeFileLoaders[modEntity] = m_defaultRecipeFileLoader;
                }
            }
        }
        /// <summary>
        /// 读取配方Xml
        /// </summary>
        static void LoadCrX() {
            foreach (var modEntity in ModsManager.ModList) {
                IRecipeFileLoader loader = m_modToRecipeFileLoaders[modEntity];
                XElement xElement = loader.GetRecipesXml(modEntity);
                LoadRecipeItems(xElement);
            }
        }
        /// <summary>
        /// 读取配方xml中Recipe开头的条目
        /// </summary>
        static void LoadRecipeItems(XElement recipesXml) {
            foreach (XElement element in recipesXml.Elements()) {
                if (element.Name.LocalName == "Recipe") {
                    RecipesItems.Add(element);
                }
                else {
                    LoadRecipeItems(element);
                }
            }
        }
        #endregion

    }
}