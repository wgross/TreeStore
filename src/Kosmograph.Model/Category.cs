﻿using Kosmograph.Model.Base;
using System.Collections.Generic;
using System.Linq;

namespace Kosmograph.Model
{
    public class Category : FacetedEntityBase
    {
        public Category()
            : this(string.Empty, Facet.Empty)
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

        public Category Parent { get; set; }

        public void AddSubCategory(Category subcategory)
        {
            subcategory.Parent = this;
            this.SubCategories = this.SubCategories.Union(new[] { subcategory }).ToList();
        }

        #endregion Category is hierarchical
    }
}