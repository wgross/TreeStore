using Kosmograph.Desktop.EditModel;
using Kosmograph.Desktop.Test.ViewModel;
using Kosmograph.Model;
using Moq;
using System;
using System.ComponentModel;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.EditModel

{
    public class FacetPropertyEditModelTest : IDisposable
    {
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);
        private readonly Mock<ITagEditCallback> tagEditCallback;

        public FacetPropertyEditModelTest()
        {
            this.tagEditCallback = this.mocks.Create<ITagEditCallback>();
        }

        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        [Fact]
        public void FacetPropertyEditModel_mirrors_ViewModel()
        {
            // ARRANGE

            var model = new Tag("", new Facet("", new FacetProperty("p")));
            var viewModel = model.ToViewModel();

            // ACT

            var result = new TagEditModel(viewModel, this.tagEditCallback.Object).Properties.Single();

            // ASSERT

            Assert.Equal("p", result.Name);
        }

        [Fact]
        public void FacetPropertyEditModel_delays_changes_of_ViewModel()
        {
            // ARRANGE

            var model = new Tag("", new Facet("", new FacetProperty("p")));
            var viewModel = model.ToViewModel();
            var editModel = new TagEditModel(viewModel, this.tagEditCallback.Object);

            // ACT

            editModel.Properties.Single().Name = "changed";

            // ASSERT

            Assert.Equal("changed", editModel.Properties.Single().Name);
            Assert.Equal("p", viewModel.Properties.Single().Name);
            Assert.Equal("p", model.Facet.Properties.Single().Name);
        }

        [Fact]
        public void FacetPropertyEditModel_trims_property_name()
        {
            // ARRANGE

            var model = new Tag("", new Facet("", new FacetProperty("p")));
            var viewModel = model.ToViewModel();
            var editModel = new TagEditModel(viewModel, this.tagEditCallback.Object);

            // ACT

            editModel.Properties.Single().Name = " changed \t";

            // ASSERT

            Assert.Equal("changed", editModel.Properties.Single().Name);
        }

        [Fact]
        public void FacetPropertyEditModel_rejects_duplicate_name()
        {
            // ARRANGE

            var model = new Tag("", new Facet("", new FacetProperty("p"), new FacetProperty("q")));
            var viewModel = model.ToViewModel();
            var editModel = new TagEditModel(viewModel, this.tagEditCallback.Object);

            DataErrorsChangedEventArgs args = null;
            void changed(object sender, DataErrorsChangedEventArgs args_) { args = args_; }

            editModel.Properties.ElementAt(1).ErrorsChanged += changed;

            // ACT

            editModel.Properties.ElementAt(1).Name = editModel.Properties.ElementAt(0).Name;

            // ASSERT

            Assert.True(editModel.Properties.ElementAt(1).HasErrors);
            Assert.Equal("Property name must be unique", editModel.Properties.ElementAt(1).GetErrors(nameof(FacetPropertyEditModel.Name)).Cast<string>().Single());
            Assert.Equal(nameof(FacetPropertyEditModel.Name), args.PropertyName);
        }

        [Fact]
        public void FacetPropertyEditModel_rejects_empty_name()
        {
            // ARRANGE

            var model = new Tag("", new Facet("", new FacetProperty("p"), new FacetProperty("q")));
            var viewModel = model.ToViewModel();
            var editModel = new TagEditModel(viewModel, this.tagEditCallback.Object);

            DataErrorsChangedEventArgs args = null;
            void changed(object sender, DataErrorsChangedEventArgs args_) { args = args_; }

            editModel.Properties.ElementAt(1).ErrorsChanged += changed;

            // ACT

            editModel.Properties.ElementAt(1).Name = "";

            // ASSERT

            Assert.True(editModel.Properties.ElementAt(1).HasErrors);
            Assert.Equal("Property name must not be empty", editModel.Properties.ElementAt(1).GetErrors(nameof(FacetPropertyEditModel.Name)).Cast<string>().Single());
            Assert.Equal(nameof(FacetPropertyEditModel.Name), args.PropertyName);
        }

        [Fact]
        public void FacetPropertyEditModel_commits_changes_to_ViewModel()
        {
            // ARRANGE

            var model = new Tag("", new Facet("", new FacetProperty("p")));
            var viewModel = model.ToViewModel();
            var editModel = new TagEditModel(viewModel, this.tagEditCallback.Object);

            editModel.Properties.Single().Name = "changed";

            // ACT

            editModel.Properties.Single().CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal("changed", editModel.Properties.Single().Name);
            Assert.Equal("changed", viewModel.Properties.Single().Name);
            Assert.Equal("changed", model.Facet.Properties.Single().Name);
        }

        [Fact]
        public void FacetPropertyEditModel_reverts_changes_to_ViewModel()
        {
            // ARRANGE

            var model = new Tag("", new Facet("", new FacetProperty("p")));
            var viewModel = model.ToViewModel();
            var editModel = new TagEditModel(viewModel, this.tagEditCallback.Object);

            editModel.Properties.Single().Name = "changed";

            // ACT

            editModel.Properties.Single().RollbackCommand.Execute(null);
            editModel.Properties.Single().CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal("p", editModel.Properties.Single().Name);
            Assert.Equal("p", viewModel.Properties.Single().Name);
            Assert.Equal("p", model.Facet.Properties.Single().Name);
        }
    }
}