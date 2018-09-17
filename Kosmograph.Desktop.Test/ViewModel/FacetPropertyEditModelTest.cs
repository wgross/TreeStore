using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class FacetPropertyEditModelTest
    {
        private readonly FacetProperty property;
        private readonly Facet facet;
        private readonly Tag tag;
        private readonly TagEditModel editTag;

        public FacetPropertyEditModelTest()
        {
            //this.property = new FacetProperty("p1");
            //this.facet = new Facet("f", this.property);
            //this.tag = new Tag("tag", facet);
            //this.editTag = new TagEditModel(new TagViewModel(tag), delegate { });
        }

        [Fact]
        public void FacetPropertyEditModel_mirrors_FacetPropertyViewModel()
        {
            // ARRANGE

            var property = new FacetPropertyViewModel(new FacetProperty("p"));

            // ACT

            var result = new FacetPropertyEditModel(property);

            // ASSERT

            Assert.Equal("p", result.Name);
        }

        [Fact]
        public void FacetPropertyEditModel_delays_changes_of_FacetPropertyViewModel()
        {
            // ARRANGE

            var property = new FacetPropertyViewModel(new FacetProperty("p"));
            var editProperty = new FacetPropertyEditModel(property);

            // ACT

            editProperty.Name = "changed";


            // ASSERT

            Assert.Equal("p", property.Name);
        }

        [Fact]
        public void FacetPropertyEditModel_commits_changes_to_FacetPropertyViewModel()
        {
            // ARRANGE

            var property = new FacetPropertyViewModel(new FacetProperty("p"));
            var editProperty = new FacetPropertyEditModel(property);
            editProperty.Name = "changed";

            // ACT

            editProperty.Commit();

            // ASSERT

            Assert.Equal("changed", property.Name);
        }
    }
}