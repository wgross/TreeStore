using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using CodeOwls.PowerShell.Provider.PathNodes;
using Kosmograph.Model;
using System.Collections.Generic;
using System.Linq;

namespace PSKosmograph.PathNodes
{
    public sealed class EntitiesNode : IPathNode, INewItem
    {
        public sealed class Value : ContainerPathValue
        {
            public Value()
                : base(new Item(), name: "Entities")
            { }
        }

        public sealed class Item
        {
        }

        #region IPathNode Members

        public object? GetNodeChildrenParameters => null;

        public string Name => "Entities";

        public string ItemMode => "+";

        public IEnumerable<IPathNode> GetNodeChildren(IProviderContext providerContext) => providerContext
            .Persistence()
            .Entities.FindAll()
            .Select(e => new EntityNode(providerContext.Persistence(), e));

        public IPathValue GetNodeValue() => new Value();

        public IEnumerable<IPathNode> Resolve(IProviderContext providerContext, string? name)
        {
            if (string.IsNullOrEmpty(name))
                return this.GetNodeChildren(providerContext);

            var entity = providerContext.Persistence().Entities.FindByName(name);
            if (entity is null)
                return Enumerable.Empty<IPathNode>();
            return new EntityNode(providerContext.Persistence(), entity).Yield();
        }

        #endregion IPathNode Members

        #region NewItem Members

        public IEnumerable<string> NewItemTypeNames => "Entity".Yield();

        public object? NewItemParameters => null;

        public IPathValue NewItem(IProviderContext providerContext, string newItemName, string itemTypeName, object newItemValue)
            => new EntityNode(providerContext.Persistence(), providerContext.Persistence().Entities.Upsert(new Entity(newItemName))).GetNodeValue();

        #endregion NewItem Members
    }
}