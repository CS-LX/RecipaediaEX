namespace RecipaediaEX.Overlay {
    /// <summary>自动摆放协议占位；Phase 2a 实现。</summary>
    public interface IRecipePlacementTarget {
        bool CanAccept(IRecipe recipe);
    }
}
