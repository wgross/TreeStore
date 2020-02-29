﻿using CodeOwls.PowerShell.Paths;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using TreeStore.Model;

namespace TreeStore.PsModule.PathNodes
{
    public class AssignedTagNode : PathNode, IRemoveItem, IClearItemProperty
    {
        private readonly ITreeStorePersistence model;
        private readonly Entity entity;
        private readonly Tag assignedTag;

        public AssignedTagNode(ITreeStorePersistence model, Entity entity, Tag tag)
        {
            this.model = model;
            this.entity = entity;
            this.assignedTag = tag;
        }

        public class ItemProvider : IItemProvider
        {
            private readonly ITreeStorePersistence model;
            private readonly Entity entity;
            private readonly Tag assignedTag;

            public ItemProvider(ITreeStorePersistence model, Entity entity, Tag assignedTag)
            {
                this.model = model;
                this.entity = entity;
                this.assignedTag = assignedTag;
            }

            public string Name => this.assignedTag.Name;

            public bool IsContainer => true;

            public object GetItem() => new Item(this.entity, this.assignedTag);

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

        #region Item - to be used in powershell pipe

        public sealed class Property
        {
            private readonly FacetProperty property;

            internal Property(string name, object? value, FacetPropertyTypeValues type)
            {
                this.Name = name;
                this.Value = value;
                this.ValueType = type;
            }

            public string Name { get; }

            public object? Value { get; }

            public FacetPropertyTypeValues ValueType { get; }
        }

        public sealed class Item
        {
            private readonly Tag assignedTag;
            private readonly Entity entity;

            public Item(Entity entity, Tag tag)
            {
                this.assignedTag = tag;
                this.entity = entity;
            }

            public Guid Id => this.assignedTag.Id;

            public string Name => this.assignedTag.Name;

            public TreeStoreItemType ItemType => TreeStoreItemType.AssignedTag;

            private Property[]? properties = null;

            public Property[] Properties => this.properties ??= this.SelectAssignedProperties();

            private Property[] SelectAssignedProperties() => this.assignedTag
                .Facet
                .Properties
                .Select(p =>
                {
                    var (hasValue, value) = this.entity.TryGetFacetProperty(p);
                    if (hasValue)
                        return new Property($"{p.Name}", value, p.Type);
                    else
                        return new Property($"{p.Name}", null, p.Type);
                })
                .ToArray();
        }

        #endregion Item - to be used in powershell pipe

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