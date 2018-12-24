using Kosmograph.Desktop.Lists.ViewModel;
using Kosmograph.Model;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Lists.Test.ViewModel
{
    public class AssignedTagViewModelTest
    {
        public Tag DefaultTag() => new Tag("t", new Facet("f", new FacetProperty("p")));

        [Fact]
        public void AssignedTagViewModel_mirrors_Model()
        {
            // ARRANGE

            var tag = DefaultTag();
            var tagViewModel = new TagViewModel(tag);
            var values = new Dictionary<string, object>
            {
                { tag.Facet.Properties.Single().Id.ToString(), 1 }
            };

            // ACT

            var result = new AssignedTagViewModel(tag, values);

            // ASSERT

            Assert.Equal(tag, result.Tag);
            Assert.Equal(1, result.Properties.Single().Value);
        }

        [Fact]
        public void AssignedTagViewModel_changes_Model()
        {
            // ARRANGE

            var tag = DefaultTag();
            var tagViewModel = new TagViewModel(tag);
            var values = new Dictionary<string, object>
            {
                { tag.Facet.Properties.Single().Id.ToString(), 1 }
            };
            var viewModel = new AssignedTagViewModel(tag, values);

            // ACT

            viewModel.Properties.Single().Value = "changed";

            // ASSERT

            Assert.Equal(tag, viewModel.Tag);
            Assert.Equal("changed", values[tag.Facet.Properties.Single().Id.ToString()]);
        }
    }
}