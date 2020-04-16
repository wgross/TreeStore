using CodeOwls.PowerShell.Paths;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
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

        public object NewItemParameters => new RuntimeDefinedParameterDictionary();

        public PathNode NewItem(IProviderContext providerContext, string newItemName, string? itemTypeName, object? newItemValue)
        {
            Guard.Against.InvalidNameCharacters(newItemName, $"tag(name='{newItemName}' wasn't created");
            Guard.Against.InvalidReservedNodeNames(newItemName, $"tag(name='{newItemName}' wasn't created");

            return new TagNode(providerContext
                .Persistence()
                .Tags
                .Upsert(new Tag(Path.GetFileName(newItemName))));
        }

        #endregion INewItem
    }
}