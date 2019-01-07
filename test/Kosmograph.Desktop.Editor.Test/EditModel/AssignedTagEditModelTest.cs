using Kosmograph.Desktop.Editor.Test;
using Kosmograph.Desktop.Editors.ViewModel;
using Kosmograph.Model;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Editors.Test.ViewModel
{
    public class AssignedTagEditModelTest
    {
        [Fact]
        public void AssigedTagEditModel_mirrors_ViewModel()
        {
            // ARRANGE

            var model = new Tag("t", new Facet("f", new FacetProperty("p")));

            var values = new Dictionary<string, object>
            {
                { model.Facet.Properties.Single().Id.ToString(), 1 }
            };

            var tagViewModel = model.ToViewModel();
            var viewModel = new AssignedTagViewModel(tagViewModel, values);

            // ACT

            var result = new AssignedTagEditModel(viewModel);

            // ASSERT

            Assert.Equal(model, result.ViewModel.Tag.Model);
            Assert.Equal(1, result.Properties.Single().Value);
        }

        [Fact]
        public void AssigedTagEditModel_delays_changes_to_ViewModel()
        {
            // ARRANGE

            var model = new Tag("t", new Facet("f", new FacetProperty("p")));
            var values = new Dictionary<string, object>
            {
                { model.Facet.Properties.Single().Id.ToString(), 1 }
            };
            var viewModel = new AssignedTagViewModel(model.ToViewModel(), values);
            var editModel = new AssignedTagEditModel(viewModel);

            // ACT

            editModel.Properties.Single().Value = "changed";

            // ASSERT

            Assert.Equal("changed", editModel.Properties[0].Value);
            Assert.Equal(1, viewModel.Properties[0].Value);
            Assert.Equal(1, values[model.Facet.Properties.Single().Id.ToString()]);
        }

        [Fact]
        public void AssigedTagEditModel_commits_changes_to_ViewModel()
        {
            // ARRANGE

            var model = new Tag("t", new Facet("f", new FacetProperty("p")));
            var values = new Dictionary<string, object>
            {
                { model.Facet.Properties.Single().Id.ToString(), 1 }
            };
            var viewModel = new AssignedTagViewModel(model.ToViewModel(), values);
            var editModel = new AssignedTagEditModel(viewModel);

            editModel.Properties.Single().Value = "changed";

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal("changed", editModel.Properties[0].Value);
            Assert.Equal("changed", viewModel.Properties[0].Value);
            Assert.Equal("changed", values[model.Facet.Properties.Single().Id.ToString()]);
        }

        [Fact]
        public void AssigedTagEditModel_reverts_changes_from_ViewModel()
        {
            // ARRANGE

            var model = new Tag("t", new Facet("f", new FacetProperty("p")));
            var values = new Dictionary<string, object>
            {
                { model.Facet.Properties.Single().Id.ToString(), 1 }
            };
            var viewModel = new AssignedTagViewModel(model.ToViewModel(), values);
            var editModel = new AssignedTagEditModel(viewModel);

            editModel.Properties.Single().Value = "changed";

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(1, editModel.Properties[0].Value);
            Assert.Equal(1, viewModel.Properties[0].Value);
            Assert.Equal(1, values[model.Facet.Properties.Single().Id.ToString()]);
        }
    }
}