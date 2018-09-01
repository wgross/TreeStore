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

            Assert.Contains(subcategory, category.SubCategories);
            Assert.Equal(category, subcategory.Parent);
        }
    }
}