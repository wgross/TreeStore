using Elementary.Hierarchy.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using TreeStore.Model.Base;

namespace TreeStore.Model
{
    public class Category : FacetingEntityBase, ICloneable
    {
        public Category()
            : this(string.Empty, new Facet())
        { }

        public Category(string name)
            : this(name, new Facet())
        { }

        public Category(string name, Facet ownFacet, params Category[] subcategories)
            : base(name, ownFacet)
        {
            foreach (var c in subcategories)
                c.Parent = this;
        }

        #region Category owns a facet

        public IEnumerable<Facet> Facets()
        {
            if (this.Parent is null)
                return this.Facet.Yield();
            return this.Facet.Yield().Union(this.Parent.Facets());
        }

        #endregion Category owns a facet

        #region Category is hierarchical

        public Category? Parent { get; set; }

        public void AddSubCategory(Category subcategory)
        {
            subcategory.Parent = this;
        }

        #region Category has a unique within the parenet catagory

        public string UniqueName
        {
            get => $"{this.Name.ToLower()}_{this.Parent?.Id.ToString() ?? "<root>"}";
            set { }
        }

        #endregion Category has a unique within the parenet catagory

        public void DetachSubCategory(Category subcategory)
        {
            subcategory.Parent = null;
        }

        public object Clone() => new Category(this.Name);

        #endregion Category is hierarchical
    }
}