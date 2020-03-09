using CodeOwls.PowerShell.Paths;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using TreeStore.Model;

namespace TreeStore.PsModule.PathNodes
{
    /// <summary>
    /// Represents an assigned facet propertyt.
    /// Value propertyvan be cane be set, get and cleared.
    /// </summary>
    public class AssignedFacetPropertyNode : LeafNode,
        IGetItem,
        ISetItemProperty, IGetItemProperty, IClearItemProperty
    {
        public class Item
        {
            private readonly Entity entity;
            private readonly FacetProperty assignedProperty;

            public Item(Entity entity, FacetProperty property)
            {
                this.entity = entity;
                this.assignedProperty = property;
            }

            public string Name => this.assignedProperty.Name;

            public object? Value
            {
                get => this.entity.TryGetFacetProperty(this.assignedProperty).Item2;
                set => this.entity.SetFacetProperty<object?>(this.assignedProperty, value);
            }

            public FacetPropertyTypeValues ValueType => this.assignedProperty.Type;

            public TreeStoreItemType ItemType => TreeStoreItemType.AssignedFacetProperty;
        }

        private readonly ITreeStorePersistence model;
        private readonly Entity entity;
        private readonly FacetProperty assignedProperty;

        public AssignedFacetPropertyNode(ITreeStorePersistence model, Entity entity, FacetProperty property)
        {
            this.model = model;
            this.entity = entity;
            this.assignedProperty = property;
        }

        public override string Name => this.assignedProperty.Name;

        public override IEnumerable<PathNode> Resolve(IProviderContext providerContext, string name) => throw new NotImplementedException();

        #region IGetItem

        public override PSObject GetItem(IProviderContext providerContext) => PSObject.AsPSObject(new Item(this.entity, this.assignedProperty));

        #endregion IGetItem

        #region IGetChildItem Members

        public override IEnumerable<PathNode> GetChildNodes(IProviderContext providerContext)
        {
            throw new NotImplementedException();
        }

        #endregion IGetChildItem Members

        #region ISetItemProperty

        public void SetItemProperties(IProviderContext providerContext, IEnumerable<PSPropertyInfo> properties)
        {
            // only the value property can be changed
            var valueProperty = properties.FirstOrDefault(p => p.Name.Equals(nameof(Item.Value), StringComparison.OrdinalIgnoreCase));
            if (valueProperty is null)
                return;

            if (!this.assignedProperty.CanAssignValue(valueProperty.Value?.ToString() ?? null))
                throw new InvalidOperationException($"value='{valueProperty.Value}' cant be assigned to property(name='{this.assignedProperty.Name}', type='{this.assignedProperty.Type}')");

            // change value and update database
            this.entity.SetFacetProperty(this.assignedProperty, valueProperty.Value);

            providerContext.Persistence().Entities.Upsert(this.entity);
        }

        #endregion ISetItemProperty

        #region IClearItemProperty

        public void ClearItemProperty(IProviderContext providerContext, IEnumerable<string> propertyNames)
        {
            // only the value property can be changed
            var valueProperty = propertyNames.FirstOrDefault(pn => pn.Equals(nameof(Item.Value), StringComparison.OrdinalIgnoreCase));
            if (valueProperty is null)
                return;

            // change value and update database
            this.entity.Values.Remove(this.assignedProperty.Id.ToString());
            providerContext.Persistence().Entities.Upsert(this.entity);
        }

        #endregion IClearItemProperty
    }
}