using Game;

namespace RecipaediaEX.Events {
    /// <summary>
    /// 非合成 Host 场景下按 Recipaedia 键、即将发布 <see cref="OpenFullRecipaediaRequestedEvent"/> 之前。
    /// </summary>
    public readonly struct OpenFullRecipaediaNavigatingContext {
        public OpenFullRecipaediaNavigatingContext(ComponentGui componentGui) {
            ComponentGui = componentGui;
        }

        public ComponentGui ComponentGui { get; }
    }
}
