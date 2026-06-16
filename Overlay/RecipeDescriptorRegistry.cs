using System;
using System.Collections.Generic;
using System.Reflection;
using Engine.Serialization;
using Game;
using RecipaediaEX.UI;
using ZLinq;

namespace RecipaediaEX.Overlay {
    public static class RecipeDescriptorRegistry {
        static readonly List<Assembly> m_scannedAssemblies = [];
        static readonly Dictionary<Type, (Type type, int order)> m_descriptorTypes = [];

        public static void EnsureScanned() {
            foreach (Assembly assembly in TypeCache.LoadedAssemblies.AsValueEnumerable().Where(a => !TypeCache.IsKnownSystemAssembly(a))) {
                if (m_scannedAssemblies.Contains(assembly)) continue;
                foreach (TypeInfo definedType in assembly.DefinedTypes) {
                    Type descriptorType = definedType.AsType();
                    if (!descriptorType.IsAssignableTo(typeof(RecipeDescriptor))) continue;
                    RecipeDescriptorAttribute? recipeDescriptorAttribute = definedType.GetCustomAttribute<RecipeDescriptorAttribute>();
                    if (recipeDescriptorAttribute == null) continue;

                    foreach (Type recipeType in recipeDescriptorAttribute.RecipeTypes) {
                        int newOrder = recipeDescriptorAttribute.Order;
                        string newName = descriptorType.Name;
                        if (!m_descriptorTypes.TryGetValue(recipeType, out (Type type, int order) existing)) {
                            m_descriptorTypes[recipeType] = (descriptorType, newOrder);
                            continue;
                        }

                        int oldOrder = existing.order;
                        string oldName = existing.type.Name;
                        bool shouldReplace = newOrder > oldOrder || (newOrder == oldOrder && string.Compare(newName, oldName, StringComparison.Ordinal) > 0);
                        if (shouldReplace) m_descriptorTypes[recipeType] = (descriptorType, newOrder);
                    }
                }
                m_scannedAssemblies.Add(assembly);
            }
        }

        public static bool TryGetDescriptorType(Type recipeType, out Type descriptorType) {
            EnsureScanned();
            if (m_descriptorTypes.TryGetValue(recipeType, out (Type type, int order) entry)) {
                descriptorType = entry.type;
                return true;
            }
            descriptorType = null!;
            return false;
        }

        public static RecipeDescriptor? CreateDescriptor(Type descriptorType, IRecipaediaRecipeNavigator navigator) {
            ConstructorInfo? ctor = descriptorType.GetConstructor([typeof(IRecipaediaRecipeNavigator)]);
            if (ctor == null)
                throw new InvalidOperationException($"类型 {descriptorType.Name} 必须只有一个 IRecipaediaRecipeNavigator 参数的构造函数");

            return (RecipeDescriptor)ctor.Invoke([navigator]);
        }

        public static RecipeDescriptor? CreateDescriptor(IRecipaediaRecipeNavigator navigator, IRecipe recipe) {
            if (!TryGetDescriptorType(recipe.GetType(), out Type descriptorType)) return null;
            return CreateDescriptor(descriptorType, navigator);
        }
    }
}
