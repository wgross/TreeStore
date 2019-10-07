using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using CodeOwls.PowerShell.Provider.PathNodes;
using System;
using System.Collections.Generic;

namespace PSKosmograph.PathNodes
{
    public sealed class RelationshipsNode : IPathNode
    {
        public sealed class Value : ContainerPathValue
        {
            public Value()
                : base(new Item(), "Relationships")
            { }
        }

        public sealed class Item
        {
        }

        public object GetNodeChildrenParameters => throw new NotImplementedException();

        public string ItemMode => "+";

        public string Name => "Relationships";

        public IEnumerable<IPathNode> GetNodeChildren(IProviderContext providerContext)
        {
            throw new NotImplementedException();
        }

        public IPathValue GetNodeValue() => new Value();

        public IEnumerable<IPathNode> Resolve(IProviderContext providerContext, string name)
        {
            throw new NotImplementedException();
        }
    }
}