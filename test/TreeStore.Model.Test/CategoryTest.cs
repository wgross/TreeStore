using System.Linq;
using Xunit;

namespace TreeStore.Model.Test
{
    public class CategoryTest
    {
        // todo: category name is unique for parent node

        #region Category hierarchy structure

        [Fact]
        public void Category_has_no_parent()
        {
            // ACT

            var result = new Category();

            // ASSERT

            Assert.Null(result.Parent);
        }

        [Fact]
        public void Category_corrects_Parent_for_ctor_subcategories()
        {
            // ACT

            var category = new Category();
            var result = new Category("cat", new Facet(), category);

            // ASSERT

            Assert.Same(result, category.Parent);
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
        public void Category_clones_shallow_with_new_id()
        {
            // ARRANGE

            var category = new Category();
            var subcategory = new Category();
            category.AddSubCategory(subcategory);

            // ACT

            var result = (Category)category.Clone();

            // ASSERT

            Assert.Equal(category.Name, result.Name);
            Assert.NotEqual(category.Id, result.Id);
        }
    }
}