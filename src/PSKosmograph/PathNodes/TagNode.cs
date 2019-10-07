using CodeOwls.PowerShell.Paths.Extensions;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using CodeOwls.PowerShell.Provider.PathNodes;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PSKosmograph.PathNodes
{
    public class TagNode : IPathNode, INewItem
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
        }

        public class Value : IPathValue
        {
            private readonly IKosmographPersistence model;
            private readonly Tag tag;

            public Value(IKosmographPersistence model, Tag tag)
            {
                this.model = model;
                this.tag = tag;
            }

            public string Name => this.tag.Name.MakeSafeForPath();

            public bool IsCollection => true;

            public object Item => new Item(this.tag);

            public void SetItemProperties(IEnumerable<PSPropertyInfo> properties)
            {
                IPathValue.SetItemProperties(this, properties);
                this.model.Tags.Upsert(tag);
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

        public object? GetNodeChildrenParameters => null;

        public string Name => this.tag.Name;

        public string ItemMode => "+";

        public IEnumerable<IPathNode> GetNodeChildren(IProviderContext providerContext)
            => this.tag.Facet.Properties.Select(p => new FacetPropertyNode(p));

        public IPathValue GetNodeValue() => new Value(this.model, this.tag);

        public IEnumerable<IPathNode> Resolve(IProviderContext providerContext, string? name)
        {
            if (name is null)
                return this.GetNodeChildren(providerContext);

            var facetProperty = this.tag.Facet.Properties.FirstOrDefault(p => name.Equals(p.Name));
            if (facetProperty is null)
                return Enumerable.Empty<IPathNode>();

            return new[] { new FacetPropertyNode(facetProperty) };
        }

        #endregion IPathNode Members

        #region INewItem Members

        public class NewFacetPropertyParameters
        {
            [Parameter(Mandatory = true)]
            public FacetPropertyTypeValues ValueType { get; set; }
        }

        public IEnumerable<string> NewItemTypeNames => "FacetProperty".Yield();

        public object? NewItemParameters => new NewFacetPropertyParameters();

        public IPathValue? NewItem(IProviderContext providerContext, string newItemChildPath, string? itemTypeName, object? newItemValue)
        {
            var facetProperty = providerContext.DynamicParameters switch
            {
                NewFacetPropertyParameters p => new FacetProperty(newItemChildPath, p.ValueType),
                _ => new FacetProperty(newItemChildPath)
            };

            this.tag.Facet.AddProperty(facetProperty);
            providerContext.Persistence().Tags.Upsert(this.tag);

            return this.GetNodeChildren(providerContext).Single(fp => fp.Name.Equals(newItemChildPath))?.GetNodeValue();
        }

        #endregion INewItem Members
    }
}