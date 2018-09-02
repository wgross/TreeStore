using System.Linq;
using Xunit;

namespace Kosmograph.Model.Test
{
    public class CategoryTest
    {
        [Fact]
        public void Category_has_no_sub_categories()
        {
            // ACT

            var result = new Category();

            // ASSERT

            Assert.Empty(result.SubCategories);
        }

        [Fact]
        public void Category_assigns_Facet()
        {
            // ARRANGE

            var facet = new Facet();
            var category = new Category();

            // ACT

            category.AssignFacet(facet);

            // ASSERT

            Assert.Same(facet, category.OwnFacet);
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

            Assert.Contains(subcategory, category.SubCategories);
            Assert.Equal(category, subcategory.Parent);
        }

        [Fact]
        public void Catagorr_yields_own_Facet()
        {
            // ARRANGE

            var facet1 = new Facet();
            var category = new Category(facet1);

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
            var category = new Category(facet1, new Category(facet2));

            // ACT

            var result = category.SubCategories.Single().Facets().ToArray();

            // ASSERT
            // facets in ancestor order

            Assert.Equal(new[] { facet2, facet1 }, result);
        }
    }
}