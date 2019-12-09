using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using CodeOwls.PowerShell.Provider.Paths;
using System;
using System.Collections.Generic;

namespace PSKosmograph.PathNodes
{
    public sealed class RelationshipsNode : IPathNode
    {
        public sealed class ItemProvider : ContainerItemProvider
        {
            public ItemProvider()
                : base(new Item(), "Relationships")
            { }
        }

        public sealed class Item
        {
            public string Name => "Relationships";
        }

        public object GetNodeChildrenParameters => throw new NotImplementedException();

        public string ItemMode => "+";

        public string Name => "Relationships";

        public IEnumerable<IPathNode> GetNodeChildren(IProviderContext providerContext)
        {
            throw new NotImplementedException();
        }

        public IItemProvider GetItemProvider() => new ItemProvider();

        public IEnumerable<IPathNode> Resolve(IProviderContext providerContext, string name)
        {
            throw new NotImplementedException();
        }
    }
}