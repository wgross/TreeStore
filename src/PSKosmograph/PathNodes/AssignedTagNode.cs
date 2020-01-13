using CodeOwls.PowerShell.Paths;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PSKosmograph.PathNodes
{
    public class AssignedTagNode : PathNode, IRemoveItem, IClearItemProperty
    {
        private readonly IKosmographPersistence model;
        private readonly Entity entity;
        private readonly Tag assignedTag;

        public AssignedTagNode(IKosmographPersistence model, Entity entity, Tag tag)
        {
            this.model = model;
            this.entity = entity;
            this.assignedTag = tag;
        }

        public class ItemProvider : IItemProvider
        {
            private readonly IKosmographPersistence model;
            private readonly Entity entity;
            private readonly Tag assignedTag;

            public ItemProvider(IKosmographPersistence model, Entity entity, Tag assignedTag)
            {
                this.model = model;
                this.entity = entity;
                this.assignedTag = assignedTag;
            }

            public string Name => this.assignedTag.Name;

            public bool IsContainer => true;

            public object GetItem() => new Item(this.assignedTag);

            public IEnumerable<PSPropertyInfo> GetItemProperties(IEnumerable<string> propertyNames)
            {
                var assignedTagProperties = this.assignedTag.Facet.Properties.AsEnumerable();

                if (propertyNames != null && propertyNames.Any())
                    assignedTagProperties = assignedTagProperties.Where(p => propertyNames.Contains(p.Name));

                return assignedTagProperties.Select(p =>
                {
                    (_, object? value) = this.entity.TryGetFacetProperty(p);
                    return new PSNoteProperty(p.Name, value);
                });
            }

            public void SetItemProperties(IEnumerable<PSPropertyInfo> properties)
            {
                foreach (var p in properties)
                {
                    var fp = this.assignedTag.Facet.Properties.SingleOrDefault(fp => fp.Name.Equals(p.Name));
                    if (fp is null)
                        continue;
                    if (!fp.CanAssignValue(p.Value?.ToString() ?? null))
                        throw new InvalidOperationException($"value='{p.Value}' cant be assigned to property(name='{p.Name}', type='{fp.Type}')");

                    this.entity.SetFacetProperty(fp, p.Value);
                }
                this.model.Entities.Upsert(this.entity);
            }
        }

        public class Item
        {
            private readonly Tag assignedTag;

            public Item(Tag tag)
            {
                this.assignedTag = tag;
            }

            public Guid Id => this.assignedTag.Id;

            public string Name => this.assignedTag.Name;

            public KosmographItemType ItemType => KosmographItemType.AssignedTag;
        }

        public override string Name => this.assignedTag.Name;

        public override string ItemMode => "+";

        public override IItemProvider GetItemProvider() => new ItemProvider(this.model, this.entity, this.assignedTag);

        public override IEnumerable<PathNode> Resolve(IProviderContext providerContext, string? name)
        {
            if (name is null)
                return this.GetChildNodes(providerContext);

            var property = this.assignedTag.Facet.Properties.SingleOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (property is null)
                return Enumerable.Empty<PathNode>();

            return new AssignedFacetPropertyNode(providerContext.Persistence(), this.entity, property).Yield();
        }

        #region IGetChildItem Members

        public override IEnumerable<PathNode> GetChildNodes(IProviderContext providerContext)
            => this.assignedTag.Facet.Properties.Select(p => new AssignedFacetPropertyNode(this.model, this.entity, p));

        #endregion IGetChildItem Members

        #region IRemoveItem Members

        public void RemoveItem(IProviderContext providerContext, string path, bool recurse)
        {
            this.entity.Tags.Remove(this.assignedTag);
            providerContext.Persistence().Entities.Upsert(this.entity);
        }

        #endregion IRemoveItem Members

        #region IClearItemProperty

        #region IClearItem Members

        public void ClearItemProperty(IProviderContext providerContext, IEnumerable<string> propertyToClear)
        {
            foreach (var propertyName in propertyToClear)
            {
                var (_, property) = this.GetAssignedFacetProperty(propertyName);
                if (property is null)
                    return;
                this.entity.Values.Remove(property.Id.ToString());
            }
            providerContext.Persistence().Entities.Upsert(this.entity);
        }

        private (Tag? tag, FacetProperty? propperty) GetAssignedFacetProperty(string name)
        {
            if (string.IsNullOrEmpty(name))
                return (null, null);

            var facetProperty = this.assignedTag.Facet.Properties.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (facetProperty is null)
                return (assignedTag, null);

            return (assignedTag, facetProperty);
        }

        #endregion IClearItem Members

        #endregion IClearItemProperty
    }
}