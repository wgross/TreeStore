using CodeOwls.PowerShell.Paths;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSKosmograph.PathNodes
{
    public class AssignedFacetPropertyNode : PathNode
    {
        public class ItemProvider : LeafItemProvider, IItemProvider
        {
            private readonly IKosmographPersistence model;
            private readonly Entity entity;

            public ItemProvider(IKosmographPersistence model, Entity entity, FacetProperty property)
                : base(new Item(entity, property), property.Name)
            {
                this.model = model;
                this.entity = entity;
            }

            public void SetItemProperties(IEnumerable<PSPropertyInfo> properties)
            {
                IItemProvider.SetItemProperties(this, properties);
                this.model.Entities.Upsert(this.entity);
            }
        }

        public class Item
        {
            private readonly Entity entity;
            private readonly FacetProperty assignedProperty;

            public Item(Entity entity, FacetProperty property)
            {
                this.entity = entity;
                this.assignedProperty = property;
            }

            public Guid Id => this.assignedProperty.Id;

            public string Name => this.assignedProperty.Name;

            public object? Value
            {
                get => this.entity.TryGetFacetProperty(this.assignedProperty).Item2;
                set => this.entity.SetFacetProperty<object?>(this.assignedProperty, value);
            }

            public FacetPropertyTypeValues ValueType => this.assignedProperty.Type;

            public KosmographItemType ItemType => KosmographItemType.AssignedFacetProperty;
        }

        private readonly IKosmographPersistence model;
        private readonly Entity entity;
        private readonly FacetProperty assignedProperty;

        public AssignedFacetPropertyNode(IKosmographPersistence model, Entity entity, FacetProperty property)
        {
            this.model = model;
            this.entity = entity;
            this.assignedProperty = property;
        }

        public override string Name => this.assignedProperty.Name;

        public override string ItemMode => ".";

        public override IEnumerable<PathNode> GetNodeChildren(IProviderContext providerContext)
        {
            throw new NotImplementedException();
        }

        public override IItemProvider GetItemProvider() => new ItemProvider(this.model, this.entity, this.assignedProperty);

        public override IEnumerable<PathNode> Resolve(IProviderContext providerContext, string name) => throw new NotImplementedException();
    }
}