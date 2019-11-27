using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using CodeOwls.PowerShell.Provider.PathNodes;
using Kosmograph.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PSKosmograph.PathNodes
{
    public sealed class TagsNode : IPathNode, INewItem
    {
        private class Value : ContainerPathValue
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

        public string Name => "Tags";

        public string ItemMode => "+";

        public IEnumerable<IPathNode> GetNodeChildren(IProviderContext providerContext)
        {
            return providerContext
                .Persistence()
                .Tags.FindAll()
                .Select(t => new TagNode(providerContext.Persistence(), t));
        }

        public IPathValue GetNodeValue() => new Value();

        public IEnumerable<IPathNode> Resolve(IProviderContext providerContext, string? name)
        {
            if (string.IsNullOrEmpty(name))
                return this.GetNodeChildren(providerContext);

            var tag = providerContext.Persistence().Tags.FindByName(name);

            if (tag is null)
                return Enumerable.Empty<IPathNode>();

            return new[] { new TagNode(providerContext.Persistence(), tag) };
        }

        #endregion IPathNode Members

        #region INewItem Members

        public IEnumerable<string> NewItemTypeNames => "Tag".Yield();

        public IPathValue NewItem(IProviderContext providerContext, string newItemChildPath, string? itemTypeName, object? newItemValue)
            => new TagNode(providerContext.Persistence(), providerContext.Persistence().Tags.Upsert(new Tag(Path.GetFileName(newItemChildPath)))).GetNodeValue();

        #endregion INewItem Members
    }
}