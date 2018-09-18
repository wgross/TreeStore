using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System.Collections.Generic;
using Xunit;

namespace Kosmograph.Desktop.Test.EditModel
{
    public class AssignedFacetPropertyEditModelTest
    {
        [Fact]
        public void AssigedFacetPropertyEditModel_mirrors_ViewModel()
        {
            // ARRANGE

            var model = new FacetProperty("p");
            var values = new Dictionary<string, object> { { model.Id.ToString(), 1 } };
            var viewModel = new AssignedFacetPropertyViewModel(model, values);

            // ACT

            var result = new AssignedFacetPropertyEditModel(viewModel);

            // ASSERT

            Assert.Equal("p", result.Name);
            Assert.Equal(1, result.Value);
        }

        [Fact]
        public void AssigedFacetPropertyEditModel_delays_change_from_ViewModel()
        {
            // ARRANGE

            var model = new FacetProperty("p");
            var values = new Dictionary<string, object> { { model.Id.ToString(), 1 } };
            var viewModel = new AssignedFacetPropertyViewModel(model, values);
            var editModel = new AssignedFacetPropertyEditModel(viewModel);

            // ACT

            editModel.Value = "value";

            // ASSERT

            Assert.Equal("value", editModel.Value);
            Assert.Equal(1, viewModel.Value);
        }

        [Fact]
        public void AssigedFacetPropertyEditModel_commits_change_to_ViewModel()
        {
            // ARRANGE

            var model = new FacetProperty("p");
            var values = new Dictionary<string, object> { { model.Id.ToString(), 1 } };
            var viewModel = new AssignedFacetPropertyViewModel(model, values);
            var editModel = new AssignedFacetPropertyEditModel(viewModel);
            editModel.Value = "value";

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal("value", editModel.Value);
            Assert.Equal("value", viewModel.Value);
        }

        [Fact]
        public void AssigedFacetPropertyEditModel_reverts_state_from_ViewModel()
        {
            // ARRANGE

            var model = new FacetProperty("p");
            var values = new Dictionary<string, object> { { model.Id.ToString(), 1 } };
            var viewModel = new AssignedFacetPropertyViewModel(model, values);
            var editModel = new AssignedFacetPropertyEditModel(viewModel);
            editModel.Value = "value";

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(1, editModel.Value);
            Assert.Equal(1, viewModel.Value);
        }
    }
}