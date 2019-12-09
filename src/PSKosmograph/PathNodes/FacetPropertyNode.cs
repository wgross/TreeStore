using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using CodeOwls.PowerShell.Provider.Paths;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PSKosmograph.PathNodes
{
    public class FacetPropertyNode : IPathNode, IRemoveItem, ICopyItem, IRenameItem
    {
        public sealed class ItemProvider : LeafItemProvider
        {
            public ItemProvider(FacetPropertyNode node)
                : base(new Item(node.facetProperty), node.Name)
            {
            }
        }

        public sealed class Item
        {
            private readonly FacetProperty facetProperty;

            public Item(FacetProperty property)
            {
                this.facetProperty = property;
            }

            public Guid Id => this.facetProperty.Id;

            public string Name
            {
                get => this.facetProperty.Name;
                set => this.facetProperty.Name = value;
            }

            public FacetPropertyTypeValues ValueType
            {
                get => this.facetProperty.Type;
                set => this.facetProperty.Type = value;
            }

            public KosmographItemType ItemType => KosmographItemType.FacetProperty;
        }

        private readonly FacetProperty facetProperty;
        private readonly Tag tag;

        public FacetPropertyNode(Tag tag, FacetProperty facetProperty)
        {
            this.tag = tag;
            this.facetProperty = facetProperty;
        }

        public object GetNodeChildrenParameters => null;

        public string Name => this.facetProperty.Name;

        public string ItemMode => "+";

        public IEnumerable<IPathNode> GetNodeChildren(IProviderContext providerContext) => Enumerable.Empty<IPathNode>();

        public IItemProvider GetItemProvider() => new ItemProvider(this);

        public IEnumerable<IPathNode> Resolve(IProviderContext providerContext, string name)
        {
            throw new NotImplementedException();
        }

        #region IRemoveItem members

        public void RemoveItem(IProviderContext providerContext, string path, bool recurse)
        {
            this.tag.Facet.RemoveProperty(this.facetProperty);
            providerContext.Persistence().Tags.Upsert(this.tag);
        }

        #endregion IRemoveItem members

        #region ICopyItem Members

        public void CopyItem(IProviderContext providerContext, string sourceItemName, string? destinationItemName, IItemProvider destinationContainer, bool recurse)
        {
            if (destinationContainer is TagNode.ItemProvider destinationContainerNodeValue)
            {
                destinationContainerNodeValue.AddProperty(providerContext, destinationItemName ?? this.facetProperty.Name, this.facetProperty.Type);
            }
        }

        #endregion ICopyItem Members

        #region IRenameItem

        public void RenameItem(IProviderContext providerContext, string path, string newName)
        {
            if (this.facetProperty.Name.Equals(newName))
                return;

            if (this.tag.Facet.Properties.Any(p => p.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"rename failed: property name '{newName}' must be unique.");

            this.facetProperty.Name = newName;

            providerContext.Persistence().Tags.Upsert(this.tag);
        }

        #endregion IRenameItem
    }
}