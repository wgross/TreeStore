using Kosmograph.Model;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Editors.ViewModel
{
    public class TagRepositoryViewModelTest : IDisposable
    {
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);
        private readonly Mock<ITagRepository> model;
        private TagRepositoryViewModel viewModel;

        public TagRepositoryViewModelTest()
        {
            this.model = this.mocks.Create<ITagRepository>();
            this.viewModel = new TagRepositoryViewModel(this.model.Object);
        }

        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        [Fact]
        public void TagRepositoryViewModel_mirrors_Model()
        {
            // ARRANGE

            var tag = new Tag("t");
            this.model
                .Setup(r => r.FindAll())
                .Returns(tag.Yield());

            // ACT

            this.viewModel.FillAll();

            // ASSERT

            Assert.Single(this.viewModel);
            Assert.Equal(tag, this.viewModel.Single().Model);
        }

        [Fact]
        public void TagRepositoryViewModel_delays_created_Tag_from_Model()
        {
            // ACT

            this.viewModel.CreateCommand.Execute(null);

            // ASSERT

            Assert.Empty(this.viewModel);
            Assert.Equal("new tag", this.viewModel.Edited.Name);
        }

        [Fact]
        public void TagRepositoryViewModel_commits_created_Tag_to_Model()
        {
            // ARRANGE

            var tag = new Tag("t");

            this.model
                .Setup(r => r.Upsert(It.IsAny<Tag>()))
                .Returns<Tag>(t => t);

            // create new tag
            this.viewModel.CreateCommand.Execute(null);

            // ACT

            this.viewModel.Edited.CommitCommand.Execute(null);

            // ASSERT
            // tag is inserted in the list after commit

            Assert.Equal("new tag", this.viewModel.Single().Name);
            Assert.Null(this.viewModel.Edited);
        }

        [Fact]
        public void TagRepositoryViewModel_invalidates_editing_duplicate_tag_name_with_different_id()
        {
            // ARRANGE

            this.model
                .Setup(m => m.FindAll())
                .Returns(new[] { new Tag("t") });

            this.viewModel.FillAll();

            // create new tag
            this.viewModel.CreateCommand.Execute(null);

            // ACT

            this.viewModel.Edited.Name = "T";
            var result = this.viewModel.Edited.NameError;

            // ASSERT
            // commit cant be executed

            Assert.True(this.viewModel.Edited.HasErrors);
            Assert.Equal("Tag name must be unique", result);
        }

        [Fact]
        public void TagRepositoryViewModel_validates_editing_duplicate_tag_name_with_same_id()
        {
            // ARRANGE

            this.model
                .Setup(m => m.FindAll())
                .Returns(new[] { new Tag("t") });

            this.viewModel.FillAll();

            // create new tag
            this.viewModel.EditCommand.Execute(this.viewModel.Single());

            // ACT

            this.viewModel.Edited.Name = "T";
            var result = this.viewModel.Edited.NameError;

            // ASSERT
            // commit can be executed

            Assert.Null(result);
            Assert.True(this.viewModel.EditCommand.CanExecute(null));
            Assert.False(this.viewModel.Edited.HasErrors);
        }

        [Fact]
        public void TagRepositoryViewModel_forbids_adding_tag_with_duplicate_name()
        {
            // ARRANGE

            this.model
                .Setup(m => m.FindAll())
                .Returns(new[] { new Tag("t") });

            this.viewModel.FillAll();

            // create new tag
            this.viewModel.CreateCommand.Execute(null);

            // ACT

            this.viewModel.Edited.Name = "T";
            var result = this.viewModel.Edited.CommitCommand.CanExecute(null);

            // ASSERT
            // commit cant be executed

            Assert.False(result);
        }

        [Fact]
        public void TagRepositoryViewModel_reverts_created_Tag_at_Model()
        {
            // ARRANGE

            this.viewModel.CreateCommand.Execute(null);

            // ACT

            this.viewModel.Edited.RollbackCommand.Execute(null);

            // ASSERT
            // tag is forgotton

            Assert.Empty(this.viewModel);
            Assert.Null(this.viewModel.Edited);
        }

        [Fact]
        public void TagRepositoryViewModel_deletes_Tag_from_Model()
        {
            // ARRANGE

            var tag = new Tag("t");
            this.model
                .Setup(r => r.FindAll())
                .Returns(tag.Yield());

            this.model
                .Setup(r => r.Delete(tag))
                .Returns(true);

            this.viewModel.FillAll();

            // ACT

            this.viewModel.DeleteCommand.Execute(this.viewModel.Single());

            // ASSERT

            Assert.Empty(this.viewModel);
        }
    }
}