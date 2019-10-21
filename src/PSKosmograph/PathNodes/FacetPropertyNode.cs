﻿using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using CodeOwls.PowerShell.Provider.PathNodes;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PSKosmograph.PathNodes
{
    public class FacetPropertyNode : IPathNode, IRemoveItem
    {
        public sealed class Value : LeafPathValue
        {
            public Value(FacetPropertyNode node)
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

        public IPathValue GetNodeValue() => new Value(this);

        public IEnumerable<IPathNode> Resolve(IProviderContext providerContext, string name)
        {
            throw new NotImplementedException();
        }

        #region IRemoveItem members

        public object? RemoveItemParameters => null;

        public void RemoveItem(IProviderContext providerContext, string path, bool recurse)
        {
            this.tag.Facet.RemoveProperty(this.facetProperty);
            providerContext.Persistence().Tags.Upsert(this.tag);
        }

        #endregion IRemoveItem members
    }
}