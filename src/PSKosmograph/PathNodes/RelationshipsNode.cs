using CodeOwls.PowerShell.Paths;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using System;
using System.Collections.Generic;

namespace PSKosmograph.PathNodes
{
    public sealed class RelationshipsNode : PathNode
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

        public override string ItemMode => "+";

        public override string Name => "Relationships";

        public override IEnumerable<PathNode> GetNodeChildren(IProviderContext providerContext)
        {
            throw new NotImplementedException();
        }

        public override IItemProvider GetItemProvider() => new ItemProvider();

        public override IEnumerable<PathNode> Resolve(IProviderContext providerContext, string name)
        {
            throw new NotImplementedException();
        }
    }
}