using System.Collections.Generic;
using RecipaediaEX.UI;

namespace RecipaediaEX.Search {
    public interface IRecipaediaSearchContributor {
        void EnrichItem(IRecipaediaItem item, ItemSearchDocument doc);
        IEnumerable<ISearchFilterDefinition> GetFilterDefinitions() => [];
    }

    public interface ISearchFilterDefinition {
        string TokenPrefix { get; }
        string DisplayName { get; }
    }
}
