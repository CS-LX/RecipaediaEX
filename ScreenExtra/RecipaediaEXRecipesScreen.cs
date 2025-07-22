using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using Engine;
using Engine.Serialization;
using Game;
using ZLinq;

namespace RecipaediaEX.UI {
    public class RecipaediaEXRecipesScreen : Screen
    {
        //功能
        public List<Assembly> m_scannedAssemblies = [];
        public Dictionary<Type, (Type type, int order)> m_descriptorTypes = [];//所有配方类型-所有呈现界面类型
        public Dictionary<Type, RecipeDescriptor> m_descriptorsCache = [];//配方解析器类型-配方解析器实例的缓存
        public List<IRecipe> m_recipes = new();//当前展示的配方系列
        public int m_index = 0;//当前展示的配方Index
        public Stack<RecipesPresentation> m_presentations = new();//配方呈现的一级级的栈

        //UI
        public ButtonWidget m_prevRecipeButton;
        public ButtonWidget m_prevInStackButton;
        public ButtonWidget m_nextRecipeButton;
        public CanvasWidget m_recipeDescriptorsCanvas;
        public RecipeDescriptor m_currentRecipeDescriptor;

        public RecipaediaEXRecipesScreen() {
            XElement node = RecipaediaEXLoader.RequestScreenFile("RecipaediaEXRecipesScreen");
            LoadContents(this, node);
            m_prevRecipeButton = Children.Find<ButtonWidget>("PreviousRecipe");
            m_prevInStackButton = Children.Find<ButtonWidget>("PreviousRecipeInStack");
            m_nextRecipeButton = Children.Find<ButtonWidget>("NextRecipe");
            m_recipeDescriptorsCanvas = Children.Find<CanvasWidget>("RecipeDescriptors");
        }

        public override void Enter(object[] parameters) {
            IRecipaediaRecipeItem recipeItem = (IRecipaediaRecipeItem)parameters[0];
            m_recipes.AddRange(RecipaediaEXManager.Recipes.AsValueEnumerable().Where(x => recipeItem.Match(x)).ToArray());
            GetDescriptors();
        }

        public override void Update() {
            IRecipe currentRecipe = m_recipes[m_index];
            string nameSuffix = string.Format(LanguageControl.GetContentWidgets(nameof(RecipaediaRecipesScreen), 1), m_index + 1);
            //显示配方逻辑
            if (m_currentRecipeDescriptor == null) ShowCurrentDescriptor(m_descriptorTypes[currentRecipe.GetType()].type, currentRecipe, nameSuffix);

            //切换配方序号逻辑
            m_prevRecipeButton.IsEnabled = m_index > 0;
            m_nextRecipeButton.IsEnabled = m_index < m_recipes.Count - 1;
            if (m_prevRecipeButton.IsClicked) {
                m_index = MathUtils.Max(m_index - 1, 0);
                HideCurrentDescriptor();
            }
            if (m_nextRecipeButton.IsClicked) {
                m_index = MathUtils.Min(m_index + 1, m_recipes.Count - 1);
                HideCurrentDescriptor();
            }

            //切换上个配方/退出逻辑
            m_prevInStackButton.IsEnabled = m_presentations.Count > 0;
            if (Input.Back || Input.Cancel || m_prevInStackButton.IsClicked) {
                if (m_presentations.Count > 0) {
                    SwitchToPreviousRecipe();
                }
                else {
                    Exit();
                }
            }
            if (Children.Find<ButtonWidget>("TopBar.Back").IsClicked) {
                Exit();
            }
        }

        public void GetDescriptors() {
            foreach (Assembly item in TypeCache.LoadedAssemblies.AsValueEnumerable().Where(a => !TypeCache.IsKnownSystemAssembly(a))) {
                if (!m_scannedAssemblies.Contains(item)) {
                    foreach (TypeInfo definedType in item.DefinedTypes) {
                        Type descriptorType = definedType.AsType();
                        if (!descriptorType.IsAssignableTo(typeof(RecipeDescriptor))) continue;//跳过不是继承RecipeDescriptor的
                        var recipeDescriptorAttribute = definedType.GetCustomAttribute<RecipeDescriptorAttribute>();
                        if (recipeDescriptorAttribute == null) continue;

                        foreach (var recipeType in recipeDescriptorAttribute.RecipeTypes) {
                            int newOrder = recipeDescriptorAttribute.Order;
                            string newName = descriptorType.Name;
                            if (!m_descriptorTypes.TryGetValue(recipeType, out var existing)) {
                                m_descriptorTypes[recipeType] = (descriptorType, newOrder);
                                continue;
                            }

                            int oldOrder = existing.order;
                            string oldName = existing.type.Name;
                            //遇到一个配方有重复的呈现界面时，优先级高的覆盖优先级低的；若优先级相同，则类名字典序后的覆盖字典序前的
                            bool shouldReplace = newOrder > oldOrder || (newOrder == oldOrder && string.Compare(newName, oldName, StringComparison.Ordinal) > 0);
                            if (shouldReplace) m_descriptorTypes[recipeType] = (descriptorType, newOrder);
                        }
                    }
                    m_scannedAssemblies.Add(item);
                }
            }
        }

        public void SwitchToNewRecipe(List<IRecipe> recipes, int index) {//切换至新配方，并且向配方堆栈里将旧配方压入
            m_presentations.Push(new RecipesPresentation(m_recipes, m_index));
            m_recipes = recipes;
            m_index = index;
        }

        public void SwitchToPreviousRecipe() {//将配方堆栈内的上级配方弹出
            RecipesPresentation previousPresentation = m_presentations.Pop();
            m_recipes = previousPresentation.m_recipes;
            m_index = previousPresentation.m_index;
        }

        public void ShowCurrentDescriptor(Type descriptorType, IRecipe recipe, string nameSuffix = null) {
            if (m_currentRecipeDescriptor != null) HideCurrentDescriptor();

            if (!m_descriptorsCache.TryGetValue(descriptorType, out RecipeDescriptor recipeDescriptor)) {
                recipeDescriptor = CreateDescriptor(descriptorType, this);
                m_descriptorsCache[descriptorType] = recipeDescriptor;
                m_recipeDescriptorsCanvas.Children.Add(recipeDescriptor);
            }

            recipeDescriptor.Show(recipe, nameSuffix);
            recipeDescriptor.IsVisible = true;
            recipeDescriptor.HorizontalAlignment = WidgetAlignment.Center;
            recipeDescriptor.VerticalAlignment = WidgetAlignment.Center;
            m_currentRecipeDescriptor = recipeDescriptor;
        }

        public void HideCurrentDescriptor() {
            if (m_currentRecipeDescriptor == null) return;

            m_currentRecipeDescriptor.Hide();
            m_currentRecipeDescriptor.IsVisible = false;
            m_currentRecipeDescriptor = null;
        }

        /// <summary>
        /// 运用工厂方法创建配方呈现界面，规范构造函数
        /// </summary>
        /// <param name="descriptorType">配方展示界面的类型</param>
        /// <param name="belongingScreen">配方展示界面所在的Screen</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">写的配方呈现界面没有/不只有一个 RecipaediaEXRecipesScreen 参数的构造函数</exception>
        public RecipeDescriptor CreateDescriptor(Type descriptorType, RecipaediaEXRecipesScreen belongingScreen){
            ConstructorInfo ctor = descriptorType.GetConstructor([typeof(RecipaediaEXRecipesScreen)]);
            if (ctor == null)
                throw new InvalidOperationException($"类型 {descriptorType.Name} 必须只有一个 RecipaediaEXRecipesScreen 参数的构造函数");

            return (RecipeDescriptor)ctor.Invoke([belongingScreen]);
        }

        public void Exit() {
            m_presentations.Clear();
            m_recipes.Clear();
            m_index = 0;
            HideCurrentDescriptor();
            ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
        }

        public struct RecipesPresentation(List<IRecipe> mRecipes, int mIndex) {
            public List<IRecipe> m_recipes = mRecipes;
            public int m_index = mIndex;
        }
    }
}
