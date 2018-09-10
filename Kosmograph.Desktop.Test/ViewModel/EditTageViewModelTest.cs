using Elementary.Compare;
using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class EditTageViewModelTest
    {
        [Fact]
        public void EditTagViewModel_mirrors_model_Tag()
        {
            // ARRANGE

            var tag = new Tag("tag", new Facet("facet", new FacetProperty("p")));

            // ACT

            var result = new EditTagViewModel(tag);

            // ASSERT

            var comp = tag.DeepCompare(result);

            Assert.Empty(comp.Different);
            Assert.All(comp.Missing, m => Assert.EndsWith("Id", m));
        }
    }
}