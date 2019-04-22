using Kosmograph.Desktop.Lists.ViewModel;
using Kosmograph.Model;
using Moq;
using System;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class KosmographViewModelTagsTest : KosmographViewModelTestBase, IDisposable
    {
        protected readonly Mock<ITagRepository> tagRepository;
        private readonly Mock<IRelationshipRepository> relationshipRepository;
        protected readonly Mock<IEntityRepository> entityRepository;
        protected readonly Mock<IKosmographPersistence> persistence;
        protected readonly Desktop.ViewModel.KosmographViewModel viewModel;

        public KosmographViewModelTagsTest()
        {
            this.relationshipRepository = this.Mocks.Create<IRelationshipRepository>();
            this.entityRepository = this.Mocks.Create<IEntityRepository>();
            this.tagRepository = this.Mocks.Create<ITagRepository>();
            this.persistence = this.Mocks.Create<IKosmographPersistence>();
            this.persistence
                .Setup(p => p.Entities)
                .Returns(this.entityRepository.Object);
            this.persistence
                .Setup(p => p.Tags)
                .Returns(this.tagRepository.Object);
            this.persistence
                .Setup(p => p.Relationships)
                .Returns(this.relationshipRepository.Object);
            this.viewModel = new Kosmograph.Desktop.ViewModel.KosmographViewModel(new Model.KosmographModel(this.persistence.Object));
        }

        public Lists.ViewModel.TagViewModel DefaultTagViewModel() => new Lists.ViewModel.TagViewModel(DefaultTag());

        public Lists.ViewModel.TagViewModel DefaultTagViewModel(Tag tag) => new Lists.ViewModel.TagViewModel(tag);

        [Fact]
        public void KosmographViewModel_provides_editor_for_existing_Tag()
        {
            // ARRANGE

            var tagViewModel = DefaultTagViewModel();

            // ACT
            // start editing of a tag

            this.viewModel.EditTagCommand.Execute(tagViewModel);

            // ASSERT
            // editor was created

            Assert.NotNull(this.viewModel.EditedTag);
            Assert.Equal(tagViewModel.Model, this.viewModel.EditedTag.Model);
        }

        [Fact]
        public void KosmographViewModel_saves_tag_on_commit()
        {
            // ARRANGE
            // edit a tag

            var tagViewModel = DefaultTagViewModel();
            this.viewModel.EditTagCommand.Execute(tagViewModel);
            this.tagRepository
                .Setup(r => r.Upsert(tagViewModel.Model))
                .Returns(tagViewModel.Model);

            // ACT
            // commit the tag upserts the tag in the DB

            this.viewModel.EditedTag.CommitCommand.Execute(null);

            // ASSERT
            // the tag is sent to the repo

            Assert.Null(this.viewModel.EditedTag);
        }

        [Fact]
        public void KosmographViewModel_clears_edited_tag_on_rollback()
        {
            // ARRANGE
            // edit a tag

            var tagViewModel = DefaultTagViewModel();
            this.viewModel.EditTagCommand.Execute(tagViewModel);

            // ACT
            // rollback the tag

            this.viewModel.EditedTag.RollbackCommand.Execute(null);

            // ASSERT
            // the tag is sent to the repo

            Assert.Null(this.viewModel.EditedTag);
        }

        [Fact]
        public void KosmographViewModel_invalidates_duplicate_tag_name_on_update()
        {
            // ARRANGE
            // edit a tag
            var tag1 = DefaultTag(t => t.Name = "t1");
            var tag2 = DefaultTag(t => t.Name = "t2");
            var tagViewModel = DefaultTagViewModel(tag2);
            this.viewModel.EditTagCommand.Execute(tagViewModel);
            this.tagRepository
                .Setup(r => r.FindByName("t1"))
                .Returns(tag1);

            // ACT
            // make a duplicate name and try to commit it

            this.viewModel.EditedTag.Name = tag1.Name;
            var result = this.viewModel.EditedTag.CommitCommand.CanExecute(null);

            // ASSERT
            // the tag is sent to the repo

            Assert.False(result);
        }

        [Fact]
        public void KosmographViewModel_provides_editor_for_new_Tag()
        {
            // ACT
            // start editing of a tag

            this.viewModel.CreateTagCommand.Execute(null);

            // ASSERT
            // editor with minimal tag was created

            Assert.NotNull(this.viewModel.EditedTag);
            Assert.Equal("new tag", this.viewModel.EditedTag.Name);
            Assert.Empty(this.viewModel.EditedTag.Properties);
        }

        [Fact]
        public void KosmographViewModel_saves_new_tag_on_commit()
        {
            // ARRANGE

            this.viewModel.CreateTagCommand.Execute(null);
            Tag createdTag = null;
            this.tagRepository
                .Setup(r => r.Upsert(It.IsAny<Tag>()))
                .Callback<Tag>(t => createdTag = t)
                .Returns<Tag>(t => t);

            // ACT
            // commit editor

            this.viewModel.EditedTag.CommitCommand.Execute(null);

            // ASSERT
            // editor is gone

            Assert.Null(this.viewModel.EditedTag);
            Assert.NotNull(createdTag);
        }

        [Fact]
        public void KosmographViewModel_clears_new_tag_on_rollback()
        {
            // ARRANGE

            this.viewModel.CreateTagCommand.Execute(null);

            // ACT
            // rollback editor

            this.viewModel.EditedTag.RollbackCommand.Execute(null);

            // ASSERT
            // editor is gone

            Assert.Null(this.viewModel.EditedTag);
        }

        [Fact]
        public void KosmographViewModel_invalidates_duplicate_tag_name_on_create()
        {
            // ARRANGE

            var tag1 = DefaultTag(t => t.Name = "t1");
            this.tagRepository
                .Setup(r => r.FindByName("t1"))
                .Returns(tag1);

            this.viewModel.CreateTagCommand.Execute(null);

            // ACT
            // commit editor

            this.viewModel.EditedTag.Name = "t1";
            var result = this.viewModel.EditedTag.CommitCommand.CanExecute(null);

            // ASSERT
            // editor is gone

            Assert.False(result);
        }

        [Fact]
        public void KosmographVIewModel_deletes_tag_at_model()
        {
            // ARRANGE

            var tag = DefaultTag(t => t.Name = "t1");
            this.tagRepository
                .Setup(r => r.Delete(tag))
                .Returns(true);

            // ACT

            this.viewModel.DeleteTagCommand.Execute(new TagViewModel(tag));
        }
    }
}