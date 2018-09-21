using Kosmograph.Desktop.EditModel;
using Kosmograph.Model;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class FacetPropertyEditModelTest
    {
        [Fact]
        public void FacetPropertyEditModel_mirrors_ViewModel()
        {
            // ARRANGE

            var model = new FacetProperty("p");
            var viewModel = model.ToViewModel();

            // ACT

            var result = new FacetPropertyEditModel(viewModel);

            // ASSERT

            Assert.Equal("p", result.Name);
        }

        [Fact]
        public void FacetPropertyEditModel_delays_changes_of_ViewModel()
        {
            // ARRANGE

            var model = new FacetProperty("p");
            var viewModel = model.ToViewModel();
            var editModel = new FacetPropertyEditModel(viewModel);

            // ACT

            editModel.Name = "changed";

            // ASSERT

            Assert.Equal("p", viewModel.Name);
        }

        [Fact]
        public void FacetPropertyEditModel_commits_changes_to_ViewModel()
        {
            // ARRANGE

            var model = new FacetProperty("p");
            var viewModel = model.ToViewModel();
            var editModel = new FacetPropertyEditModel(viewModel);

            editModel.Name = "changed";

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal("changed", editModel.Name);
            Assert.Equal("changed", viewModel.Name);
            Assert.Equal("changed", model.Name);
        }

        [Fact]
        public void FacetPropertyEditModel_reverts_changes_to_ViewModel()
        {
            // ARRANGE

            var model = new FacetProperty("p");
            var viewModel = model.ToViewModel();
            var editModel = new FacetPropertyEditModel(viewModel);
            editModel.Name = "changed";

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal("p", editModel.Name);
            Assert.Equal("p", viewModel.Name);
            Assert.Equal("p", model.Name);
        }
    }
}