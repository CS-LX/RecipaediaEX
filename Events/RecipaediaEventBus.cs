using System;
using System.Collections.Concurrent;

namespace RecipaediaEX.Events {
    /// <summary>
    /// RecipaediaEX 侧可扩展事件总线：按事件类型维护独立 <see cref="EventChannel{T}"/>，
    /// 便于其它模组在不改 RX 源码的情况下订阅自定义载荷类型。
    /// </summary>
    public static class RecipaediaEventBus {
        static readonly ConcurrentDictionary<Type, IEventChannel> s_channelsByType = new();

        /// <summary>按类型获取或创建通道（用于自定义事件类型）。</summary>
        static EventChannel<T> Channel<T>() => (EventChannel<T>)s_channelsByType.GetOrAdd(typeof(T), _ => new EventChannel<T>());

        /// <summary>获取当前类型的发布通道</summary>
        public static IPublisher<T> GetPublisher<T>() => Channel<T>();

        /// <summary>获取当前类型的接收通道</summary>
        public static ISubscriber<T> GetSubscriber<T>() => Channel<T>();

        /// <summary>内置：静态配方总表重建完成。</summary>
        public static ISubscriber<RecipesResetEvent> RecipesReset => GetSubscriber<RecipesResetEvent>();

        /// <summary>内置：配方匹配成功（静态表或动态链）。</summary>
        public static ISubscriber<RecipeMatchedEvent> RecipeMatched => GetSubscriber<RecipeMatchedEvent>();

        /// <summary>内置：扩展工作台预览配方变化。</summary>
        public static ISubscriber<CraftingRecipeChangedEvent> CraftingRecipeChanged => GetSubscriber<CraftingRecipeChangedEvent>();

        /// <summary>内置：扩展熔炉激活冶炼配方变化。</summary>
        public static ISubscriber<SmeltingRecipeChangedEvent> SmeltingRecipeChanged => GetSubscriber<SmeltingRecipeChangedEvent>();

        /// <summary>内置：熔炉冶炼完成并写入产物格。</summary>
        public static ISubscriber<CrafterOutputProducedEvent> CrafterOutputProduced => GetSubscriber<CrafterOutputProducedEvent>();

        /// <summary>内置：扩展工作台/熔炉产物格取出。</summary>
        public static ISubscriber<CrafterOutputRemovedEvent> CrafterOutputRemoved => GetSubscriber<CrafterOutputRemovedEvent>();

        /// <summary>内置：扩展熔炉消耗燃料。</summary>
        public static ISubscriber<FurnaceFuelUsedEvent> FurnaceFuelUsed => GetSubscriber<FurnaceFuelUsedEvent>();
    }
}
