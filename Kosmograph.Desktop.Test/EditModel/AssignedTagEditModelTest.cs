using Kosmograph.Desktop.EditModel;
using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.EditModel
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
            var viewModel = new AssignedTagViewModel(model, values);

            // ACT

            var result = new AssignedTagEditModel(viewModel);

            // ASSERT
            // facte propert yhandlung is tested elsewhere.

            Assert.Equal("t", result.Name);
            Assert.Single(result.Properties);
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
            var viewModel = new AssignedTagViewModel(model, values);
            var editModel = new AssignedTagEditModel(viewModel);

            // ACT

            editModel.Properties.Single().Value = "changed";

            // ASSERT

            Assert.Equal("changed", editModel.Properties[0].Value);
            Assert.Equal(1, editModel.ViewModel.Properties[0].Value);
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
            var viewModel = new AssignedTagViewModel(model, values);
            var editModel = new AssignedTagEditModel(viewModel);

            editModel.Properties.Single().Value = "changed";

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal("changed", editModel.Properties[0].Value);
            Assert.Equal("changed", editModel.ViewModel.Properties[0].Value);
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
            var viewModel = new AssignedTagViewModel(model, values);
            var editModel = new AssignedTagEditModel(viewModel);

            editModel.Properties.Single().Value = "changed";

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(1, editModel.Properties[0].Value);
            Assert.Equal(1, editModel.ViewModel.Properties[0].Value);
        }
    }
}