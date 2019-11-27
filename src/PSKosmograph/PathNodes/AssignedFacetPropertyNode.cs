using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using CodeOwls.PowerShell.Provider.PathNodes;
using Kosmograph.Model;
using System;
using System.Collections.Generic;

namespace PSKosmograph.PathNodes
{
    public class AssignedFacetPropertyNode : IPathNode
    {
        public class Value : LeafPathValue
        {
            public Value(Entity entity, FacetProperty property)
                : base(new Item(entity, property), property.Name)
            { }
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

            public string Name => this.assignedProperty.Name;

            public object? Value => this.entity.TryGetFacetProperty(this.assignedProperty).Item2;
        }

        private readonly Entity entity;
        private readonly FacetProperty assignedProperty;

        public AssignedFacetPropertyNode(Entity entity, FacetProperty property)
        {
            this.entity = entity;
            this.assignedProperty = property;
        }
        
        public string Name => this.assignedProperty.Name;

        public string ItemMode => ".";

        public IEnumerable<IPathNode> GetNodeChildren(IProviderContext providerContext)
        {
            throw new NotImplementedException();
        }

        public IPathValue GetNodeValue() => new Value(this.entity, this.assignedProperty);

        public IEnumerable<IPathNode> Resolve(IProviderContext providerContext, string name)
        {
            throw new NotImplementedException();
        }
    }
}