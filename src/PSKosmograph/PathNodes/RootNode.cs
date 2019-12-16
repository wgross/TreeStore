using CodeOwls.PowerShell.Paths;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using Kosmograph.Model;
using System.Collections.Generic;
using System.Linq;

namespace PSKosmograph.PathNodes
{
    public sealed class RootNode : PathNode
    {
        private class Value : ContainerItemProvider
        {
            public Value()
               : base(new Item(), string.Empty)
            {
            }
        }

        public sealed class Item
        {
        }

        public override string Name => string.Empty;

        public override string ItemMode => "+";

        public override IEnumerable<PathNode> GetNodeChildren(IProviderContext providerContext)
            => new PathNode[] { new TagsNode(), new EntitiesNode(), new RelationshipsNode() };

        public override IItemProvider GetItemProvider() => new Value();

        public override IEnumerable<PathNode> Resolve(IProviderContext providerContext, string? name)
        {
            if (name is null)
                return this.GetNodeChildren(providerContext);

            switch (name)
            {
                case "Tags":
                    return new TagsNode().Yield();

                case "Entities":
                    return new EntitiesNode().Yield();

                case "Relationships":
                    return new RelationshipsNode().Yield();
            }
            return Enumerable.Empty<PathNode>();
        }
    }
}