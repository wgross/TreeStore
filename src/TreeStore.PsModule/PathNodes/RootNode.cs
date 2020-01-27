using CodeOwls.PowerShell.Paths;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using System.Collections.Generic;
using System.Linq;
using TreeStore.Model;

namespace TreeStore.PsModule.PathNodes
{
    public sealed class RootNode : PathNode, IGetChildItem
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

        public override IItemProvider GetItemProvider() => new Value();

        public override IEnumerable<PathNode> Resolve(IProviderContext providerContext, string? name)
        {
            if (name is null)
                return this.GetChildNodes(providerContext);

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

        #region IGetChildItem Members

        public override IEnumerable<PathNode> GetChildNodes(IProviderContext providerContext)
            => new PathNode[] { new TagsNode(), new EntitiesNode() };

        //todo: reintroduce relationships
        // => new PathNode[] { new TagsNode(), new EntitiesNode(), new RelationshipsNode() };

        #endregion IGetChildItem Members
    }
}