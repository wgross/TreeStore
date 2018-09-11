using Elementary.Compare;
using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class EditTagViewModelTest
    {
        [Fact]
        public void EditTagViewModel_mirrors_model_Tag()
        {
            // ARRANGE

            var tag = new Tag("tag", new Facet("facet", new FacetProperty("p")));

            // ACT

            var result = new EditTagViewModel(tag, delegate { });

            // ASSERT

            var comp = tag.DeepCompare(result);

            Assert.Empty(comp.Different);
        }

        [Fact]
        public void EditTagViewModel_delays_changes_at_Tag()
        {
            // ARRANGE

            var editTag = new EditTagViewModel(new Tag("tag", new Facet()), delegate { });

            // ACT

            editTag.Name = "changed";
            editTag.Facet.Properties.Add(new EditFacetPropertyViewModel(new FacetProperty("p")));

            //ASSERT

            Assert.Equal("tag", editTag.Model.Name);
            Assert.Empty(editTag.Model.Facet.Properties);
        }

        [Fact]
        public void EditTagViewModel_commits_changes_to_Tag()
        {
            // ARRANGE

            var tag = new Tag("tag", new Facet("p", new FacetProperty("p1")));

            Tag committedTag = null;
            var editTag = new EditTagViewModel(tag, t => committedTag = t);

            editTag.Name = "changed";
            editTag.Facet.Name = "changed";
            editTag.Facet.Properties.Add(new EditFacetPropertyViewModel(new FacetProperty("p2")));
            editTag.Facet.Properties.Remove(editTag.Facet.Properties.First());
            editTag.Facet.Properties.First().Name = "p2-changed";

            // ACT

            editTag.Commit();

            // ASSERT

            Assert.Equal("changed", editTag.Model.Name);
            Assert.Equal(tag, committedTag);
            Assert.Single(editTag.Model.Facet.Properties);
            Assert.Equal("p2-changed", editTag.Model.Facet.Properties.Single().Name);
        }
    }
}