using CodeOwls.PowerShell.Paths;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using TreeStore.Model;

namespace TreeStore.PsModule.PathNodes
{
    public sealed class TagsNode : ContainerNode, INewItem
    {
        public sealed class Item
        {
            public string Name => "Tags";
        }

        #region IPathNode

        public override string Name => "Tags";

        public override IEnumerable<PathNode> Resolve(IProviderContext providerContext, string? name)
        {
            if (string.IsNullOrEmpty(name))
                return this.GetChildNodes(providerContext);

            var tag = providerContext.Persistence().Tags.FindByName(name);

            if (tag is null)
                return Enumerable.Empty<PathNode>();

            return new[] { new TagNode(tag) };
        }

        #endregion IPathNode

        #region IGetItem

        public override PSObject GetItem(IProviderContext providerContext) => PSObject.AsPSObject(new Item());

        #endregion IGetItem

        #region IGetChildItem

        public override IEnumerable<PathNode> GetChildNodes(IProviderContext providerContext)
        {
            return providerContext
                .Persistence()
                .Tags.FindAll()
                .Select(t => new TagNode(t));
        }

        #endregion IGetChildItem

        #region INewItem

        public IEnumerable<string> NewItemTypeNames => "Tag".Yield();

        public PathNode NewItem(IProviderContext providerContext, string newItemChildPath, string? itemTypeName, object? newItemValue)
        {
            if (!newItemChildPath.EnsureValidName())
                throw new InvalidOperationException($"tag(name='{newItemChildPath}' wasn't created: it contains invalid characters");

            return new TagNode(providerContext
                .Persistence()
                .Tags
                .Upsert(new Tag(Path.GetFileName(newItemChildPath))));
        }

        #endregion INewItem
    }
}