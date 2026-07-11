using System;
using System.Collections.Concurrent;

namespace RecipaediaEX.Events {
    /// <summary>
    /// RecipaediaEX 拦截总线：与 <see cref="RecipaediaEventBus"/> 的通知通道互补，
    /// 用于在操作执行前由依赖模组否决（返回 <see langword="false"/>）。
    /// </summary>
    public static class RecipaediaInterceptBus {
        static readonly ConcurrentDictionary<Type, IAnyInterceptChannel> m_channelsByType = new();

        static InterceptChannel<T> Channel<T>() => (InterceptChannel<T>)m_channelsByType.GetOrAdd(typeof(T), _ => new InterceptChannel<T>());

        /// <summary>获取指定上下文类型的拦截发布端。</summary>
        public static IInterceptPublisher<T> GetPublisher<T>() => Channel<T>();

        /// <summary>获取指定上下文类型的拦截订阅端。</summary>
        public static IInterceptSubscriber<T> GetSubscriber<T>() => Channel<T>();

        /// <summary>在唯一出口调用：任一订阅方否决则返回 <see langword="false"/>。</summary>
        public static bool TryProceed<T>(T context) => Channel<T>().TryProceed(context);

        #region P0 — 生产运行时

        /// <summary>产物格取出之前。</summary>
        public static IInterceptSubscriber<CrafterOutputRemovingContext> CrafterOutputRemoving => GetSubscriber<CrafterOutputRemovingContext>();

        /// <summary>熔炉写入产物格之前。</summary>
        public static IInterceptSubscriber<CrafterOutputProducingContext> CrafterOutputProducing => GetSubscriber<CrafterOutputProducingContext>();

        /// <summary>熔炉消耗燃料之前。</summary>
        public static IInterceptSubscriber<FurnaceFuelConsumingContext> FurnaceFuelConsuming => GetSubscriber<FurnaceFuelConsumingContext>();

        /// <summary>合成助手 <c>+</c> 方案就绪、执行搬运之前。</summary>
        public static IInterceptSubscriber<RecipePlacementPlanBuildingContext> RecipePlacementPlanBuilding => GetSubscriber<RecipePlacementPlanBuildingContext>();

        /// <summary>合成助手 <c>+</c> 即将扣背包填格之前。</summary>
        public static IInterceptSubscriber<RecipePlacementExecutingContext> RecipePlacementExecuting => GetSubscriber<RecipePlacementExecutingContext>();

        #endregion

        #region P1 — 合成助手 Overlay

        /// <summary>合成助手打开或重新显示之前。</summary>
        public static IInterceptSubscriber<CraftingOverlayOpeningContext> CraftingOverlayOpening => GetSubscriber<CraftingOverlayOpeningContext>();

        /// <summary>合成助手隐藏或销毁之前。</summary>
        public static IInterceptSubscriber<CraftingOverlayClosingContext> CraftingOverlayClosing => GetSubscriber<CraftingOverlayClosingContext>();

        /// <summary>按 Recipaedia 键打开全屏图鉴之前。</summary>
        public static IInterceptSubscriber<OpenFullRecipaediaNavigatingContext> OpenFullRecipaediaNavigating => GetSubscriber<OpenFullRecipaediaNavigatingContext>();

        /// <summary>助手搜索框应用查询之前。</summary>
        public static IInterceptSubscriber<OverlaySearchApplyingContext> OverlaySearchApplying => GetSubscriber<OverlaySearchApplyingContext>();

        /// <summary>助手展示配方预览弹层之前。</summary>
        public static IInterceptSubscriber<OverlayRecipePreviewShowingContext> OverlayRecipePreviewShowing => GetSubscriber<OverlayRecipePreviewShowingContext>();

        #endregion
    }
}
