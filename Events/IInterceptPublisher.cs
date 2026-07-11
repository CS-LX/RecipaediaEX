namespace RecipaediaEX.Events {
    /// <summary>
    /// 拦截通道发布端：在宿主逻辑执行前询问已注册拦截器是否允许继续。
    /// </summary>
    public interface IInterceptPublisher<in T> {
        /// <summary>
        /// 按注册顺序（及优先级）依次调用拦截器。
        /// 任一拦截器否决则立即返回 <see langword="false"/>，否则返回 <see langword="true"/>。
        /// </summary>
        bool TryProceed(T context);
    }
}
