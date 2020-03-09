using CodeOwls.PowerShell.Paths;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using TreeStore.Model;

namespace TreeStore.PsModule.PathNodes
{
    public class FacetPropertyNode : LeafNode,
        IRemoveItem, ICopyItem, IRenameItem,
        IGetItemProperty
    {
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

            public FacetPropertyTypeValues ValueType => this.facetProperty.Type;

            public TreeStoreItemType ItemType => TreeStoreItemType.FacetProperty;
        }

        private readonly FacetProperty facetProperty;
        private readonly Tag tag;

        public FacetPropertyNode(Tag tag, FacetProperty facetProperty)
        {
            this.tag = tag;
            this.facetProperty = facetProperty;
        }

        public override string Name => this.facetProperty.Name;

        public override IEnumerable<PathNode> Resolve(IProviderContext providerContext, string name)
        {
            throw new NotImplementedException();
        }

        #region IGetItem

        public override PSObject GetItem(IProviderContext providerContext) => PSObject.AsPSObject(new Item(this.facetProperty));

        #endregion IGetItem

        #region IRemoveItem Members

        public void RemoveItem(IProviderContext providerContext, string path)
        {
            this.tag.Facet.RemoveProperty(this.facetProperty);
            providerContext.Persistence().Tags.Upsert(this.tag);
        }

        #endregion IRemoveItem Members

        #region ICopyItem Members

        public void CopyItem(IProviderContext providerContext, string sourceItemName, string? destinationItemName, PathNode destinationNode)
        {
            if (destinationNode is TagNode destinationContainerNodeValue)
            {
                destinationContainerNodeValue.AddProperty(providerContext, destinationItemName ?? this.facetProperty.Name, this.facetProperty.Type);
            }
        }

        #endregion ICopyItem Members

        #region IRenameItem Members

        public void RenameItem(IProviderContext providerContext, string path, string newName)
        {
            if (this.facetProperty.Name.Equals(newName))
                return;

            if (this.tag.Facet.Properties.Any(p => p.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"rename failed: property name '{newName}' must be unique.");

            this.facetProperty.Name = newName;

            providerContext.Persistence().Tags.Upsert(this.tag);
        }

        #endregion IRenameItem Members
    }
}