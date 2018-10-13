using Kosmograph.Desktop.EditModel;
using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using Moq;
using System;
using System.ComponentModel;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.EditModel
{
    public class TagEditModelTest : IDisposable
    {
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);
        private readonly Mock<ITagEditCallback> tagEditCallback;

        public TagEditModelTest()
        {
            this.tagEditCallback = this.mocks.Create<ITagEditCallback>();
        }

        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        [Fact]
        public void TagEditModel_mirrors_ViewModel()
        {
            // ARRANGE

            var model = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            var viewModel = new TagViewModel(model);
            var editModel = new TagEditModel(viewModel, this.tagEditCallback.Object);

            // ASSERT

            Assert.Equal("tag", editModel.Name);
            Assert.Single(editModel.Properties);
        }

        [Fact]
        public void TagEditModel_delays_change_at_ViewModel()
        {
            // ARRANGE

            var p2 = new FacetProperty("p2");
            var model = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            var viewModel = new TagViewModel(model);
            var editModel = new TagEditModel(viewModel, this.tagEditCallback.Object);

            // ACT

            editModel.Name = "changed";

            // ASSERT

            Assert.Equal("changed", editModel.Name);
            Assert.Equal("tag", viewModel.Name);
            Assert.Equal("tag", model.Name);
        }

        [Fact]
        public void TagEditModel_commits_change_to_ViewModel()
        {
            // ARRANGE

            var p2 = new FacetProperty("p2");
            var model = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            var viewModel = new TagViewModel(model);

            this.tagEditCallback
                .Setup(cb => cb.Commit(model));

            var editModel = new TagEditModel(viewModel, this.tagEditCallback.Object);

            this.tagEditCallback
                .Setup(cb => cb.CanCommit(editModel))
                .Returns(true);

            editModel.Name = "changed";

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal("changed", editModel.Name);
            Assert.Equal("changed", viewModel.Name);
            Assert.Equal("changed", model.Name);
        }

        [Fact]
        public void TagEditModel_reverts_change_from_ViewModel()
        {
            // ARRANGE

            var p2 = new FacetProperty("p2");
            var model = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            var viewModel = new TagViewModel(model);

            this.tagEditCallback
                .Setup(cb => cb.Commit(model));

            this.tagEditCallback
                .Setup(cb => cb.Rollback(model));

            var editModel = new TagEditModel(viewModel, this.tagEditCallback.Object);

            this.tagEditCallback
                .Setup(cb => cb.CanCommit(editModel))
                .Returns(true);

            editModel.Name = "changed";

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal("tag", editModel.Name);
            Assert.Equal("tag", viewModel.Name);
            Assert.Equal("tag", model.Name);
        }

        [Fact]
        public void TagEditModel_delays_add_property_at_ViewModel()
        {
            // ARRANGE

            var model = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            var viewModel = new TagViewModel(model);
            var editModel = new TagEditModel(viewModel, this.tagEditCallback.Object);

            // ACT

            editModel.CreatePropertyCommand.Execute(null);

            // ASSERT

            Assert.Equal(2, editModel.Properties.Count());
            Assert.Equal("new property", editModel.Properties.ElementAt(1).Name);
            Assert.Single(viewModel.Properties);
            Assert.Single(model.Facet.Properties);
        }

        [Fact]
        public void TagEditModel_commits_add_property_at_ViewModel()
        {
            // ARRANGE

            var model = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            var viewModel = new TagViewModel(model);

            this.tagEditCallback
                .Setup(cb => cb.Commit(model));

            var editModel = new TagEditModel(viewModel, this.tagEditCallback.Object);

            this.tagEditCallback
                .Setup(cb => cb.CanCommit(editModel))
                .Returns(true);

            editModel.CreatePropertyCommand.Execute(null);

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(2, editModel.Properties.Count());
            Assert.Equal(2, viewModel.Properties.Count());
            Assert.Equal(2, model.Facet.Properties.Count());
            Assert.Equal("new property", editModel.Properties.ElementAt(1).Name);
            Assert.Equal("new property", viewModel.Properties.ElementAt(1).Name);
            Assert.Equal("new property", model.Facet.Properties.ElementAt(1).Name);
        }

        [Fact]
        public void TagEditModel_reverts_add_property_from_ViewModel()
        {
            // ARRANGE

            var model = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            var viewModel = new TagViewModel(model);

            this.tagEditCallback
                .Setup(cb => cb.Rollback(model));

            this.tagEditCallback
                .Setup(cb => cb.Commit(model));

            var editModel = new TagEditModel(viewModel, this.tagEditCallback.Object);

            this.tagEditCallback
                .Setup(cb => cb.CanCommit(editModel))
                .Returns(true);

            editModel.CreatePropertyCommand.Execute(null);

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal("tag", editModel.Name);
            Assert.Equal("tag", viewModel.Name);
            Assert.Equal("tag", model.Name);
            Assert.Single(editModel.Properties);
            Assert.Single(viewModel.Properties);
            Assert.Single(model.Facet.Properties);
        }

        [Fact]
        public void TagEditModel_invalidates_empty_tag_name()
        {
            // ARRANGE

            var model = new Tag("tag", new Facet("facet"));
            var viewModel = new TagViewModel(model);
            var editModel = new TagEditModel(viewModel, this.tagEditCallback.Object);

            DataErrorsChangedEventArgs args = null;
            void changed(object sender, DataErrorsChangedEventArgs args_) { args = args_; }

            editModel.ErrorsChanged += changed;

            // ACT

            editModel.Name = string.Empty;
            var result = editModel.CommitCommand.CanExecute(null);

            // ASSERT

            Assert.False(result);
            Assert.True(editModel.HasErrors);
            Assert.Equal("Tag name must not be empty", editModel.GetErrors(nameof(TagEditModel.Name)).Cast<string>().Single());
            Assert.Equal(nameof(TagEditModel.Name), args.PropertyName);
        }

        [Fact]
        public void TagEditModel_invalidates_empty_property_name()
        {
            // ARRANGE

            var model = new Tag("tag", new Facet("facet"));
            var viewModel = new TagViewModel(model);

            var editModel = new TagEditModel(viewModel, this.tagEditCallback.Object);

            editModel.CreatePropertyCommand.Execute(null);

            // ACT

            editModel.Properties.Single().Name = string.Empty;
            var result = editModel.CommitCommand.CanExecute(null);

            // ASSERT

            Assert.False(result);
        }

        [Fact]
        public void TagEditModel_invalidates_duplicate_property_name()
        {
            // ARRANGE

            var model = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            var viewModel = new TagViewModel(model);

            var editModel = new TagEditModel(viewModel, this.tagEditCallback.Object);

            editModel.CreatePropertyCommand.Execute(null);
            editModel.Properties.ElementAt(1).Name = "p";

            // ACT

            var result = editModel.CommitCommand.CanExecute(null);

            // ASSERT

            Assert.False(result);
        }

        [Fact]
        public void TagEditModel_delays_remove_property_at_ViewModel()
        {
            // ARRANGE

            var model = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            var viewModel = new TagViewModel(model);
            var editModel = new TagEditModel(viewModel, this.tagEditCallback.Object);

            // ACT

            editModel.RemovePropertyCommand.Execute(editModel.Properties.Single());

            // ASSERT

            Assert.Empty(editModel.Properties);
            Assert.Single(viewModel.Properties);
            Assert.Single(model.Facet.Properties);
        }

        [Fact]
        public void TagEditModel_commits_remove_property_to_ViewModel()
        {
            // ARRANGE

            var model = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            var viewModel = new TagViewModel(model);

            this.tagEditCallback
               .Setup(cb => cb.Commit(model));

            var editModel = new TagEditModel(viewModel, this.tagEditCallback.Object);

            this.tagEditCallback
                .Setup(cb => cb.CanCommit(editModel))
                .Returns(true);

            editModel.RemovePropertyCommand.Execute(editModel.Properties.Single());

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Empty(editModel.Properties);
            Assert.Empty(viewModel.Properties);
            Assert.Empty(model.Facet.Properties);
        }

        [Fact]
        public void TagEditModel_reverts_remove_property_at_ViewModel()
        {
            // ARRANGE

            var model = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            var viewModel = new TagViewModel(model);

            this.tagEditCallback
               .Setup(cb => cb.Commit(model));

            this.tagEditCallback
               .Setup(cb => cb.Rollback(model));

            var editModel = new TagEditModel(viewModel, this.tagEditCallback.Object);

            this.tagEditCallback
                .Setup(cb => cb.CanCommit(editModel))
                .Returns(true);

            editModel.CreatePropertyCommand.Execute(null);

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Single(editModel.Properties);
            Assert.Single(viewModel.Properties);
            Assert.Single(model.Facet.Properties);
        }
    }
}