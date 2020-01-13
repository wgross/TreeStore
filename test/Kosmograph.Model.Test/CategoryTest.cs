using System;
using System.Linq;
using Xunit;

namespace Kosmograph.Model.Test
{
    public class CategoryTest
    {
        // todo: category name is unique for parent node

        #region Category hierarchy structure

        [Fact]
        public void Category_has_no_sub_categories()
        {
            // ACT

            var result = new Category();

            // ASSERT

            Assert.Empty(result.SubCategories);
        }

        [Fact]
        public void Category_corrects_Parent_for_ctor_subcategories()
        {
            // ACT

            var result = new Category("cat", new Facet(), new Category());

            // ASSERT

            Assert.Single(result.SubCategories);
            Assert.Equal(result, result.SubCategories.Single().Parent);
        }

        [Fact]
        public void Category_corrects_Parent_for_assigned_subcategories()
        {
            // ARRANGE

            var result = new Category();

            // ACT

            result.SubCategories = new Category().Yield().ToList();

            // ASSERT

            Assert.Single(result.SubCategories);
            Assert.Equal(result, result.SubCategories.Single().Parent);
        }

        [Fact]
        public void Category_finds_subcategory_by_id()
        {
            // ARRANGE

            var searchCat = new Category();
            var cat = new Category("cat", new Facet(), new Category("cat2", new Facet(), searchCat));

            // ACT

            var result = cat.FindSubCategory(searchCat.Id);

            // ASSERT

            Assert.Equal(searchCat, result);
        }

        [Theory]
        [InlineData("cat2")]
        [InlineData("Cat2")]
        public void Categeory_finds_child_by_name(string name)
        {
            // ARRANGE

            var searchCat = new Category("cat2", new Facet());
            var cat = new Category("cat", new Facet(), searchCat);

            // ACT

            var result = cat.FindSubCategory(name, StringComparer.OrdinalIgnoreCase);

            // ASSERT

            Assert.Equal(searchCat, result);
        }

        [Fact]
        public void Category_finding_subcategory_by_id_returns_null_on_missing_subcategory()
        {
            // ARRANGE

            var searchCat = new Category();
            var cat = new Category("cat", new Facet(), new Category("cat2", new Facet(), searchCat));

            // ACT

            var result = cat.FindSubCategory(Guid.NewGuid());

            // ASSERT

            Assert.Null(result);
        }

        #endregion Category hierarchy structure

        [Fact]
        public void Category_assigns_Facet()
        {
            // ARRANGE

            var facet = new Facet();
            var category = new Category();

            // ACT

            category.AssignFacet(facet);

            // ASSERT

            Assert.Same(facet, category.Facet);
        }

        [Fact]
        public void Category_adds_subcategory()
        {
            // ARRANGE

            var category = new Category();
            var subcategory = new Category();

            // ACT

            category.AddSubCategory(subcategory);

            // ASSERT

            Assert.Equal(subcategory, category.SubCategories.Single());
            Assert.Equal(category, subcategory.Parent);
        }

        [Fact]
        public void Category_adding_subcategory_ignores_duplicate()
        {
            // ARRANGE

            var category = new Category();
            var subcategory = new Category();
            category.AddSubCategory(subcategory);

            // ACT

            category.AddSubCategory(subcategory);

            // ASSERT

            Assert.Equal(subcategory, category.SubCategories.Single());
            Assert.Equal(category, subcategory.Parent);
        }

        [Fact]
        public void Category_yields_own_Facet()
        {
            // ARRANGE

            var facet1 = new Facet();
            var category = new Category("cat", facet1);

            // ACT

            var result = category.Facets().ToArray();

            // ASSERT

            Assert.Equal(new[] { facet1 }, result);
        }

        [Fact]
        public void Category_aggregates_ancestors_facets()
        {
            // ARRANGE

            var facet1 = new Facet();
            var facet2 = new Facet();
            var category = new Category("cat", facet1, new Category("cat", facet2));

            // ACT

            var result = category.SubCategories.Single().Facets().ToArray();

            // ASSERT
            // facets in ancestor order

            Assert.Equal(new[] { facet2, facet1 }, result);
        }
    }
}