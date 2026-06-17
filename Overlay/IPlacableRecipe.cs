using System.Collections.Generic;

namespace RecipaediaEX.Overlay {
    public interface IPlacableRecipe {
        IReadOnlyList<PlacementRequirement> GetPlacementRequirements();
    }
}
