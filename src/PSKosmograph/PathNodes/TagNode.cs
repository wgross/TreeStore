using CodeOwls.PowerShell.Paths;
using CodeOwls.PowerShell.Paths.Extensions;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PSKosmograph.PathNodes
{
    public class TagNode : IPathNode,
        // Item capabilities
        INewItem, IRemoveItem, ICopyItem, IRenameItem,
        // Property capabilities
        INewItemProperty, IRenameItemProperty, IRemoveItemProperty, ICopyItemPropertySource, ICopyItemPropertyDestination, IMoveItemPropertySource, IMoveItemPropertyDestination
    {
        public class Item
        {
            private readonly Tag tag;

            public Item(Tag value)
            {
                this.tag = value;
            }

            public string Name
            {
                get => this.tag.Name;
                set => this.tag.Name = value;
            }

            public Guid Id => this.tag.Id;

            public KosmographItemType ItemType => KosmographItemType.Tag;
        }

        public class ItemProvider : IItemProvider
        {
            private readonly IKosmographPersistence model;
            private readonly Tag tag;

            public ItemProvider(IKosmographPersistence model, Tag tag)
            {
                this.model = model;
                this.tag = tag;
            }

            public string Name => this.tag.Name.MakeSafeForPath();

            public bool IsContainer => true;

            public object GetItem() => new Item(this.tag);

            #region IGetItemProperties

            public IEnumerable<PSPropertyInfo> GetItemProperties(IEnumerable<string> propertyNames)
            {
                IEnumerable<PSNoteProperty> tagProperties()
                {
                    yield return new PSNoteProperty(nameof(Item.Id), this.tag.Id);
                    yield return new PSNoteProperty(nameof(Item.Name), this.tag.Name);
                }

                var facetProperties = this.tag.Facet.Properties.Select(p => new PSFacetProperty(p.Name, p.Type, null));

                if (propertyNames.Any())
                    return tagProperties().Union(facetProperties).Where(p => propertyNames.Contains(p.Name, StringComparer.OrdinalIgnoreCase));
                else
                    return tagProperties().Union(facetProperties);
            }

            #endregion IGetItemProperties

            public void SetItemProperties(IEnumerable<PSPropertyInfo> properties)

            {
                IItemProvider.SetItemProperties(this, properties);
                this.model.Tags.Upsert(tag);
            }

            internal void AddProperty(IProviderContext providerContext, string name, FacetPropertyTypeValues type)
            {
                this.tag.Facet.AddProperty(new FacetProperty(name, type));

                providerContext.Persistence().Tags.Upsert(this.tag);
            }
        }

        private readonly IKosmographPersistence model;
        private readonly Tag tag;

        public TagNode(IKosmographPersistence model, Tag tag)
        {
            this.model = model;
            this.tag = tag;
        }

        #region IPathNode Members

        public string Name => this.tag.Name;

        public string ItemMode => "+";

        public IEnumerable<IPathNode> GetNodeChildren(IProviderContext providerContext)
            => this.tag.Facet.Properties.Select(p => new FacetPropertyNode(this.tag, p));

        public IItemProvider GetItemProvider() => new ItemProvider(this.model, this.tag);

        public IEnumerable<IPathNode> Resolve(IProviderContext providerContext, string? name)
        {
            if (name is null)
                return this.GetNodeChildren(providerContext);

            var facetProperty = this.tag.Facet.Properties.FirstOrDefault(p => name.Equals(p.Name));
            if (facetProperty is null)
                return Enumerable.Empty<IPathNode>();

            return new[] { new FacetPropertyNode(tag, facetProperty) };
        }

        #endregion IPathNode Members

        #region INewItem Members

        public class NewFacetPropertyParameters
        {
            [Parameter(Mandatory = true)]
            public FacetPropertyTypeValues ValueType { get; set; }
        }

        public IEnumerable<string> NewItemTypeNames => "FacetProperty".Yield();

        public object NewItemParameters => new NewFacetPropertyParameters();

        public IItemProvider? NewItem(IProviderContext providerContext, string newItemChildPath, string itemTypeName, object? newItemValue)
        {
            var facetProperty = providerContext.DynamicParameters switch
            {
                NewFacetPropertyParameters p => new FacetProperty(newItemChildPath, p.ValueType),
                _ => new FacetProperty(newItemChildPath)
            };

            this.tag.Facet.AddProperty(facetProperty);
            providerContext.Persistence().Tags.Upsert(this.tag);

            return this.GetNodeChildren(providerContext).Single(fp => fp.Name.Equals(newItemChildPath))?.GetItemProvider();
        }

        #endregion INewItem Members

        #region IRemoveItem Members

        public void RemoveItem(IProviderContext providerContext, string name, bool recurse)
        {
            if (recurse)
                providerContext.Persistence().Tags.Delete(this.tag);
            else if (!this.tag.Facet.Properties.Any())
                providerContext.Persistence().Tags.Delete(this.tag);
        }

        #endregion IRemoveItem Members

        #region ICopyItem Members

        public void CopyItem(IProviderContext providerContext, string sourceItemName, string destinationItemName, IItemProvider destinationContainer, bool recurse)
        {
            var newTag = new Tag(destinationItemName);
            this.tag.Facet.Properties.ForEach(p => newTag.Facet.AddProperty(new FacetProperty(p.Name, p.Type)));

            providerContext.Persistence().Tags.Upsert(newTag);
        }

        #endregion ICopyItem Members

        #region IRenameItem Members

        public void RenameItem(IProviderContext providerContext, string path, string newName)
        {
            if (this.tag.Name.Equals(newName))
                return;

            this.tag.Name = newName;
            providerContext.Persistence().Tags.Upsert(this.tag);
        }

        #endregion IRenameItem Members

        #region INewItemProperty Members

        public void NewItemProperty(IProviderContext providerContext, string propertyName, string propertyTypeName, object newItemValue)
        {
            this.tag.Facet.AddProperty(new FacetProperty(propertyName, FacetPropertyTypeValue(propertyTypeName)));

            providerContext.Persistence().Tags.Upsert(this.tag);
        }

        private static FacetPropertyTypeValues FacetPropertyTypeValue(string facetProperyTypeName)
        {
            if (Enum.TryParse(typeof(FacetPropertyTypeValues), facetProperyTypeName, out var type))
                return (FacetPropertyTypeValues)(type ?? FacetPropertyTypeValues.String);
            else return FacetPropertyTypeValues.String;
        }

        #endregion INewItemProperty Members

        #region IRenameItemProperty Members

        public void RenameItemProperty(IProviderContext providerContext, string sourcePropertyName, string destinationPropertyName)
        {
            this.tag.Facet.Properties.Single(p => p.Name.Equals("p", StringComparison.OrdinalIgnoreCase)).Name = destinationPropertyName;

            providerContext.Persistence().Tags.Upsert(this.tag);
        }

        #endregion IRenameItemProperty Members

        #region IRemoveItemPropery Members

        public void RemoveItemProperty(IProviderContext providerContext, string propertyName)
        {
            this.tag.Facet.RemoveProperty(
                this.tag.Facet.Properties.Single(p => p.Name.Equals("p", StringComparison.OrdinalIgnoreCase)));

            providerContext.Persistence().Tags.Upsert(this.tag);
        }

        #endregion IRemoveItemPropery Members

        #region ICopyItemProperty

        public void CopyItemProperty(IProviderContext providerContext, string sourceProperty, string destinationProperty, IItemProvider sourceItemProvider)
        {
            var sourcePSFacetProperty = sourceItemProvider.GetItemProperties(sourceProperty.Yield()).First();

            this.NewItemProperty(providerContext, destinationProperty, sourcePSFacetProperty.TypeNameOfValue, sourcePSFacetProperty.Value);
        }

        #endregion ICopyItemProperty

        #region IMoveItemProperty

        public void MoveItemProperty(IProviderContext providerContext, string sourceProperty, string destinationProperty, IItemProvider sourceItemProvider)
        {
            var sourcePSFacetProperty = sourceItemProvider.GetItemProperties(sourceProperty.Yield()).First();

            this.NewItemProperty(providerContext, destinationProperty, sourcePSFacetProperty.TypeNameOfValue, sourcePSFacetProperty.Value);
        }

        #endregion IMoveItemProperty
    }
}