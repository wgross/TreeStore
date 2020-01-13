using Elementary.Hierarchy.Generic;
using Kosmograph.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kosmograph.Model
{
    public class Category : FacetingEntityBase
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
            this.SubCategories = subcategories.Select(c =>
            {
                c.Parent = this;
                return c;
            }).ToList();
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

        public List<Category> SubCategories
        {
            get => this.subCategories;
            set
            {
                value.ForEach(c => c.Parent = this);
                this.subCategories = value;
            }
        }

        private List<Category> subCategories = new List<Category>();

        public Category? Parent { get; set; }

        public void AddSubCategory(Category subcategory)
        {
            subcategory.Parent = this;
            this.SubCategories = this.SubCategories.Union(new[] { subcategory }).ToList();
        }

        public Category FindSubCategory(Guid id) => this.DescendantsAndSelf(c => c.SubCategories, depthFirst: true, maxDepth: 10).FirstOrDefault(c => c.Id == id);

        public Category FindSubCategory(string name, IEqualityComparer<string> nameComparer)
            => this.Children<Category>(c => c.SubCategories).FirstOrDefault(c => nameComparer.Equals(c.Name, name));

        #endregion Category is hierarchical
    }
}