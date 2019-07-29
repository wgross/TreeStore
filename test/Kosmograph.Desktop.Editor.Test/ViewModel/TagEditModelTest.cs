using Kosmograph.Desktop.Editors.ViewModel;
using Kosmograph.Model;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Editors.Test.ViewModel
{
    public class TagEditModelTest : IDisposable
    {
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);
        private readonly Mock<ITagEditCallback> tagEditCallback;

        private Tag DefaultTag() => new Tag("tag", new Facet("facet", new FacetProperty("p")));

        public TagEditModelTest()
        {
            this.tagEditCallback = this.mocks.Create<ITagEditCallback>();
        }

        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        [Fact]
        public void TagEditModel_mirrors_Model()
        {
            // ARRANGE

            var model = DefaultTag();
            var editModel = new TagEditModel(model, this.tagEditCallback.Object);

            // ASSERT

            Assert.Equal("tag", editModel.Name);
            Assert.Single(editModel.Properties);
        }

        [Fact]
        public void TagEditModel_delays_change_at_Model()
        {
            // ARRANGE

            var p2 = new FacetProperty("p2");
            var model = DefaultTag();
            var editModel = new TagEditModel(model, this.tagEditCallback.Object);

            this.tagEditCallback
                .Setup(cb => cb.Validate(editModel))
                .Returns((string)null);

            // ACT

            editModel.Name = "changed";

            // ASSERT

            Assert.Equal("changed", editModel.Name);
            Assert.Equal("tag", model.Name);
        }

        [Fact]
        public void TagEditModel_commits_change_to_Model()
        {
            // ARRANGE

            var p2 = new FacetProperty("p2");
            var model = new Tag("tag", new Facet("facet", new FacetProperty("p")));

            this.tagEditCallback
                .Setup(cb => cb.Commit(model));

            var editModel = new TagEditModel(model, this.tagEditCallback.Object);

            this.tagEditCallback
                .Setup(cb => cb.Validate(editModel))
                .Returns((string)null);

            this.tagEditCallback
                .Setup(cb => cb.CanCommit(editModel))
                .Returns(true);

            editModel.Name = "changed";

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal("changed", editModel.Name);
            Assert.Equal("changed", model.Name);
        }

        [Fact]
        public void TagEditModel_reverts_change_from_Model()
        {
            // ARRANGE

            var p2 = new FacetProperty("p2");
            var model = DefaultTag();

            this.tagEditCallback
                .Setup(cb => cb.Commit(model));

            this.tagEditCallback
                .Setup(cb => cb.Rollback(model));

            var editModel = new TagEditModel(model, this.tagEditCallback.Object);

            this.tagEditCallback
                .Setup(cb => cb.Validate(editModel))
                .Returns((string)null);

            this.tagEditCallback
                .Setup(cb => cb.CanCommit(editModel))
                .Returns(true);

            editModel.Name = "changed";

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal("tag", editModel.Name);
            Assert.Equal("tag", model.Name);
        }

        [Fact]
        public void TagEditModel_delays_add_property_at_Model()
        {
            // ARRANGE

            var model = DefaultTag();
            var editModel = new TagEditModel(model, this.tagEditCallback.Object);

            this.tagEditCallback
                .Setup(cb => cb.Validate(editModel))
                .Returns((string)null);

            // ACT

            editModel.CreatePropertyCommand.Execute(null);

            // ASSERT

            Assert.Equal(2, editModel.Properties.Count());
            Assert.Equal("new property", editModel.Properties.ElementAt(1).Name);
            Assert.Single(model.Facet.Properties);
        }

        [Fact]
        public void TagEditModel_commits_add_property_at_Model()
        {
            // ARRANGE

            var model = DefaultTag();

            this.tagEditCallback
                .Setup(cb => cb.Commit(model));

            var editModel = new TagEditModel(model, this.tagEditCallback.Object);

            this.tagEditCallback
                .Setup(cb => cb.CanCommit(editModel))
                .Returns(true);

            editModel.CreatePropertyCommand.Execute(null);

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(2, editModel.Properties.Count());
            Assert.Equal(2, model.Facet.Properties.Count());
            Assert.Equal("new property", editModel.Properties.ElementAt(1).Name);
            Assert.Equal("new property", model.Facet.Properties.ElementAt(1).Name);
        }

        [Fact]
        public void TagEditModel_reverts_add_property_from_Model()
        {
            // ARRANGE

            var model = DefaultTag();

            this.tagEditCallback
                .Setup(cb => cb.Rollback(model));

            var editModel = new TagEditModel(model, this.tagEditCallback.Object);

            this.tagEditCallback
                .Setup(cb => cb.Validate(editModel))
                .Returns((string)null);

            editModel.CreatePropertyCommand.Execute(null);

            // ACT

            editModel.RollbackCommand.Execute(null);

            // ASSERT

            Assert.Equal("tag", editModel.Name);
            Assert.Equal("tag", model.Name);
            Assert.Single(editModel.Properties);
            Assert.Single(model.Facet.Properties);
        }

        [Fact]
        public void TagEditModel_invalidates_empty_tag_name()
        {
            // ARRANGE

            var model = DefaultTag();
            var editModel = new TagEditModel(model, this.tagEditCallback.Object);

            this.tagEditCallback
                .Setup(cb => cb.Validate(editModel))
                .Returns((string)null);

            // ACT

            editModel.Name = string.Empty;
            var result = editModel.CommitCommand.CanExecute(null);

            // ASSERT

            Assert.False(result);
            Assert.True(editModel.HasErrors);
            Assert.Equal("Name must not be empty", editModel.NameError); ;
        }

        [Fact]
        public void TagEditModel_invalidates_empty_property_name()
        {
            // ARRANGE

            var model = DefaultTag();
            var editModel = new TagEditModel(model, this.tagEditCallback.Object);

            this.tagEditCallback
              .Setup(cb => cb.Validate(editModel))
              .Returns((string)null);

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

            var model = DefaultTag();
            var editModel = new TagEditModel(model, this.tagEditCallback.Object);

            this.tagEditCallback
                .Setup(cb => cb.Validate(editModel))
                .Returns((string)null);

            editModel.CreatePropertyCommand.Execute(null);
            editModel.Properties.ElementAt(1).Name = "p";

            this.tagEditCallback
                .Setup(cb => cb.CanCommit(editModel))
                .Returns(true);

            // ACT

            var result = editModel.CommitCommand.CanExecute(null);

            // ASSERT

            Assert.False(result);
        }

        [Fact]
        public void TagEditModel_delays_remove_property_at_Model()
        {
            // ARRANGE

            var model = DefaultTag();
            var editModel = new TagEditModel(model, this.tagEditCallback.Object);

            // ACT

            editModel.RemovePropertyCommand.Execute(editModel.Properties.Single());

            // ASSERT

            Assert.Empty(editModel.Properties);
            Assert.Single(model.Facet.Properties);
        }

        [Fact]
        public void TagEditModel_commits_remove_property_from_Model()
        {
            // ARRANGE

            var model = DefaultTag();

            this.tagEditCallback
               .Setup(cb => cb.Commit(model));

            var editModel = new TagEditModel(model, this.tagEditCallback.Object);

            this.tagEditCallback
                .Setup(cb => cb.CanCommit(editModel))
                .Returns(true);

            editModel.RemovePropertyCommand.Execute(editModel.Properties.Single());

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Empty(editModel.Properties);
            Assert.Empty(model.Facet.Properties);
        }

        [Fact]
        public void TagEditModel_reverts_remove_property_at_Model()
        {
            // ARRANGE

            var model = DefaultTag();

            this.tagEditCallback
               .Setup(cb => cb.Rollback(model));

            var editModel = new TagEditModel(model, this.tagEditCallback.Object);

            this.tagEditCallback
               .Setup(cb => cb.Validate(editModel))
               .Returns((string)null);

            editModel.CreatePropertyCommand.Execute(null);

            // ACT

            editModel.RollbackCommand.Execute(null);

            // ASSERT

            Assert.Single(editModel.Properties);
            Assert.Single(model.Facet.Properties);
        }
    }
}