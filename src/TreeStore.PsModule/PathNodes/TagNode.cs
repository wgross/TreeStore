using CodeOwls.PowerShell.Paths;
using CodeOwls.PowerShell.Paths.Extensions;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using TreeStore.Model;

namespace TreeStore.PsModule.PathNodes
{
    public class TagNode : PathNode,
        // Item capabilities
        INewItem, IRemoveItem, ICopyItem, IRenameItem
    {
        #region Item - to be used in powershell pipe

        public sealed class Item
        {
            public sealed class Property
            {
                private readonly FacetProperty property;

                protected internal Property(FacetProperty property)
                {
                    this.property = property;
                }

                public string Name => this.property.Name;

                public FacetPropertyTypeValues ValueType => this.property.Type;
            }

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

            public TreeStoreItemType ItemType => TreeStoreItemType.Tag;

            public Property[] Properties => this.tag.Facet.Properties.Select(p => new Property(p)).ToArray();
        }

        #endregion Item - to be used in powershell pipe

        public class ItemProvider : IItemProvider

        {
            private readonly ITreeStorePersistence model;
            private readonly Tag tag;

            public ItemProvider(ITreeStorePersistence model, Tag tag)
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
                    var item = (Item)GetItem();
                    yield return new PSNoteProperty(nameof(Item.Id), item.Id);
                    yield return new PSNoteProperty(nameof(Item.Name), item.Name);
                    yield return new PSNoteProperty(nameof(Item.Properties), item.Properties);
                }

                if (propertyNames.Any())
                    return tagProperties().Where(p => propertyNames.Contains(p.Name, StringComparer.OrdinalIgnoreCase));
                else
                    return tagProperties();
            }

            #endregion IGetItemProperties

            #region ISetItemProperties

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

            #endregion ISetItemProperties
        }

        private readonly ITreeStorePersistence model;
        private readonly Tag tag;

        public TagNode(ITreeStorePersistence model, Tag tag)
        {
            this.model = model;
            this.tag = tag;
        }

        #region IPathNode Members

        public override string Name => this.tag.Name;

        public override string ItemMode => "+";

        public override IEnumerable<PathNode> GetChildNodes(IProviderContext providerContext)
            => this.tag.Facet.Properties.Select(p => new FacetPropertyNode(this.tag, p));

        public override IItemProvider GetItemProvider() => new ItemProvider(this.model, this.tag);

        public override IEnumerable<PathNode> Resolve(IProviderContext providerContext, string? name)
        {
            if (name is null)
                return this.GetChildNodes(providerContext);

            var facetProperty = this.tag.Facet.Properties.FirstOrDefault(p => name.Equals(p.Name));
            if (facetProperty is null)
                return Enumerable.Empty<PathNode>();

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

            return this.GetChildNodes(providerContext).Single(fp => fp.Name.Equals(newItemChildPath))?.GetItemProvider();
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
    }
}