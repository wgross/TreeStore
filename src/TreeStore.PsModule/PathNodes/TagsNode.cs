using CodeOwls.PowerShell.Paths;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using TreeStore.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TreeStore.PsModule.PathNodes
{
    public sealed class TagsNode : PathNode, INewItem
    {
        private class Value : ContainerItemProvider
        {
            public Value()
                 : base(new Item(), "Tags")
            {
            }
        }

        public sealed class Item
        {
            public string Name => "Tags";
        }

        #region IPathNode Members

        public override string Name => "Tags";

        public override string ItemMode => "+";

        public override IItemProvider GetItemProvider() => new Value();

        public override IEnumerable<PathNode> Resolve(IProviderContext providerContext, string? name)
        {
            if (string.IsNullOrEmpty(name))
                return this.GetChildNodes(providerContext);

            var tag = providerContext.Persistence().Tags.FindByName(name);

            if (tag is null)
                return Enumerable.Empty<PathNode>();

            return new[] { new TagNode(providerContext.Persistence(), tag) };
        }

        #endregion IPathNode Members

        #region IGetChildItem Members

        public override IEnumerable<PathNode> GetChildNodes(IProviderContext providerContext)
        {
            return providerContext
                .Persistence()
                .Tags.FindAll()
                .Select(t => new TagNode(providerContext.Persistence(), t));
        }

        #endregion IGetChildItem Members

        #region INewItem Members

        public IEnumerable<string> NewItemTypeNames => "Tag".Yield();

        public IItemProvider NewItem(IProviderContext providerContext, string newItemChildPath, string? itemTypeName, object? newItemValue)
            => new TagNode(providerContext.Persistence(), providerContext.Persistence().Tags.Upsert(new Tag(Path.GetFileName(newItemChildPath)))).GetItemProvider();

        #endregion INewItem Members
    }
}