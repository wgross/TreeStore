using CodeOwls.PowerShell.Paths;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using TreeStore.Model;

namespace TreeStore.PsModule.PathNodes
{
    public class TagNode : ContainerNode,
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

            #region // Properties

            // todo: properties
            //   public string[] Properties => this.tag.Facet.Properties.Select(p => p.Name).ToArray();

            #endregion // Properties
        }

        #endregion Item - to be used in powershell pipe

        private readonly Tag tag;

        public TagNode(Tag tag)
        {
            this.tag = tag;
        }

        #region IGetItem

        public override PSObject GetItem(IProviderContext providerContext) => PSObject.AsPSObject(new Item(this.tag));

        #endregion IGetItem

        #region IPathNode

        public override string Name => this.tag.Name;

        public override IEnumerable<PathNode> GetChildNodes(IProviderContext providerContext)
            => this.tag.Facet.Properties.Select(p => new FacetPropertyNode(this.tag, p));

        public override IEnumerable<PathNode> Resolve(IProviderContext providerContext, string? name)
        {
            if (name is null)
                return this.GetChildNodes(providerContext);

            var facetProperty = this.tag.Facet.Properties.FirstOrDefault(p => name.Equals(p.Name));
            if (facetProperty is null)
                return Enumerable.Empty<PathNode>();

            return new[] { new FacetPropertyNode(tag, facetProperty) };
        }

        #endregion IPathNode

        #region INewItem

        public class NewFacetPropertyParameters
        {
            [Parameter(Mandatory = true)]
            public FacetPropertyTypeValues ValueType { get; set; }
        }

        public IEnumerable<string> NewItemTypeNames => "FacetProperty".Yield();

        public object NewItemParameters => new NewFacetPropertyParameters();

        public PathNode NewItem(IProviderContext providerContext, string newItemChildPath, string? itemTypeName, object? newItemValue)
        {
            if (!newItemChildPath.EnsureValidName())
                throw new InvalidOperationException($"facetProperty(name='{newItemChildPath}' wasn't created: it contains invalid characters");

            if (FacetPropertyNode.ForbiddenNames.Contains(newItemChildPath, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException($"facetProperty(name='{newItemChildPath}') wasn't created: name is reserved");

            if (this.tag.Facet.Properties.Any(p => p.Name.Equals(newItemChildPath, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"facetProperty(name='{newItemChildPath}') wasn't created: name is duplicate");

            var facetProperty = providerContext.DynamicParameters switch
            {
                NewFacetPropertyParameters p => new FacetProperty(newItemChildPath, p.ValueType),
                _ => new FacetProperty(newItemChildPath)
            };

            this.tag.Facet.AddProperty(facetProperty);
            providerContext.Persistence().Tags.Upsert(this.tag);

            return this.GetChildNodes(providerContext).Single(fp => fp.Name.Equals(newItemChildPath));
        }

        #endregion INewItem

        #region IRemoveItem

        public object RemoveItemParameters => new RuntimeDefinedParameterDictionary();

        public void RemoveItem(IProviderContext providerContext, string name)
        {
            var persistence = providerContext.Persistence();
            var deleteTag = (
                isUsed: persistence.Entities.FindByTag(this.tag).Any(),
                hasProperties: this.tag.Facet.Properties.Any()
            )
            switch
            {
                // If the tag is used and has properrties, deletion will cause data loss.
                (true, _) => this.RemoveItemInUse(providerContext),
                (_, true) => providerContext.Recurse,
                (_, false) => true
            };
            if (deleteTag)
                persistence.Tags.Delete(this.tag);
        }

        private bool RemoveItemInUse(IProviderContext providerContext)
        {
            if (providerContext.Force)
                return true;

            // item is in use but -Force isn't specified
            providerContext.WriteError(
                new ErrorRecord(new InvalidOperationException($"Can't delete tag(name='{this.tag.Name}'): It is used by at least one entity. Use -Force to delete anyway."),
                    errorId: "TagInUse",
                    errorCategory: ErrorCategory.InvalidOperation,
                    targetObject: this.GetItem(providerContext)));

            return false;
        }

        #endregion IRemoveItem

        #region ICopyItem

        public object CopyItemParameters => new RuntimeDefinedParameterDictionary();

        public void CopyItem(IProviderContext providerContext, string sourceItemName, string destinationItemName, PathNode destinationNode)
        {
            if (!destinationItemName.EnsureValidName())
                throw new InvalidOperationException($"tag(name='{destinationItemName}' wasn't created: it contains invalid characters");

            var newTag = new Tag(destinationItemName);
            this.tag.Facet.Properties.ForEach(p => newTag.Facet.AddProperty(new FacetProperty(p.Name, p.Type)));

            providerContext.Persistence().Tags.Upsert(newTag);
        }

        internal void AddProperty(IProviderContext providerContext, string name, FacetPropertyTypeValues type)
        {
            this.tag.Facet.AddProperty(new FacetProperty(name, type));

            providerContext.Persistence().Tags.Upsert(this.tag);
        }

        #endregion ICopyItem

        #region IRenameItem

        public object RenameItemParameters => new RuntimeDefinedParameterDictionary();

        public void RenameItem(IProviderContext providerContext, string path, string newName)
        {
            if (!newName.EnsureValidName())
                throw new InvalidOperationException($"tag(name='{newName}' wasn't created: it contains invalid characters");

            if (this.tag.Name.Equals(newName))
                return;

            this.tag.Name = newName;
            providerContext.Persistence().Tags.Upsert(this.tag);
        }

        #endregion IRenameItem
    }
}