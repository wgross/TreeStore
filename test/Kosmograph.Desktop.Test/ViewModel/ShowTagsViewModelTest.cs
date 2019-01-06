using Kosmograph.Model;
using Xunit;

namespace Kosmograph.Desktop.ViewModel.Test
{
    public class ShowTagsViewModelTest
    {
        private readonly ShowTagsViewModel viewModel;

        public ShowTagsViewModelTest()
        {
            this.viewModel = new ShowTagsViewModel();
        }

        public Tag DefaultTag() => new Tag("tag", new Facet("facet", new FacetProperty("p")));

        [Fact]
        public void ShowTagsViewModel_can_starts_editing_of_Tag()
        {
            // ARRANGE

            var tag = DefaultTag();

            // ACT

            this.viewModel.EditCommand.CanExecute(tag);
        }

        [Fact]
        public void ShowTagsViewModel_starts_editing_of_Tag()
        {
            // ARRANGE

            var tag = DefaultTag();

            // ACT

            this.viewModel.EditCommand.Execute(tag);

            // ASSERT

            Assert.NotNull(this.viewModel.Edited);
        }
    }
}