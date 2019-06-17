using Kosmograph.Desktop.Lists.ViewModel;
using Kosmograph.Model;
using Moq;
using System;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class KosmographViewModelTagsTest : KosmographViewModelTestBase, IDisposable
    {
        public Lists.ViewModel.TagViewModel DefaultTagViewModel() => new Lists.ViewModel.TagViewModel(DefaultTag());

        public Lists.ViewModel.TagViewModel DefaultTagViewModel(Tag tag) => new Lists.ViewModel.TagViewModel(tag);

        [Fact]
        public void KosmographViewModel_provides_editor_for_existing_Tag()
        {
            // ARRANGE

            var tagViewModel = DefaultTagViewModel();

            // ACT
            // start editing of a tag

            this.ViewModel.EditTagCommand.Execute(tagViewModel);

            // ASSERT
            // editor was created

            Assert.NotNull(this.ViewModel.EditedTag);
            Assert.Equal(tagViewModel.Model, this.ViewModel.EditedTag.Model);
        }

        [Fact]
        public void KosmographViewModel_saves_tag_on_commit()
        {
            // ARRANGE
            // edit a tag

            var tagViewModel = DefaultTagViewModel();
            this.ViewModel.EditTagCommand.Execute(tagViewModel);
            this.TagRepository
                .Setup(r => r.Upsert(tagViewModel.Model))
                .Returns(tagViewModel.Model);

            // ACT
            // commit the tag upserts the tag in the DB

            this.ViewModel.EditedTag.CommitCommand.Execute(null);

            // ASSERT
            // the tag is sent to the repo

            Assert.Null(this.ViewModel.EditedTag);
        }

        [Fact]
        public void KosmographViewModel_clears_edited_tag_on_rollback()
        {
            // ARRANGE
            // edit a tag

            var tagViewModel = DefaultTagViewModel();
            this.ViewModel.EditTagCommand.Execute(tagViewModel);

            // ACT
            // rollback the tag

            this.ViewModel.EditedTag.RollbackCommand.Execute(null);

            // ASSERT
            // the tag is sent to the repo

            Assert.Null(this.ViewModel.EditedTag);
        }

        [Fact]
        public void KosmographViewModel_invalidates_duplicate_tag_name_on_update()
        {
            // ARRANGE
            // edit a tag
            var tag1 = DefaultTag(t => t.Name = "t1");
            var tag2 = DefaultTag(t => t.Name = "t2");
            var tagViewModel = DefaultTagViewModel(tag2);
            this.ViewModel.EditTagCommand.Execute(tagViewModel);
            this.TagRepository
                .Setup(r => r.FindByName("t1"))
                .Returns(tag1);

            // ACT
            // make a duplicate name and try to commit it

            this.ViewModel.EditedTag.Name = tag1.Name;
            var result = this.ViewModel.EditedTag.CommitCommand.CanExecute(null);

            // ASSERT
            // the tag is sent to the repo

            Assert.False(result);
        }

        [Fact]
        public void KosmographViewModel_provides_editor_for_new_Tag()
        {
            // ACT
            // start editing of a tag

            this.ViewModel.CreateTagCommand.Execute(null);

            // ASSERT
            // editor with minimal tag was created

            Assert.NotNull(this.ViewModel.EditedTag);
            Assert.Equal("new tag", this.ViewModel.EditedTag.Name);
            Assert.Empty(this.ViewModel.EditedTag.Properties);
        }

        [Fact]
        public void KosmographViewModel_saves_new_tag_on_commit()
        {
            // ARRANGE

            this.ViewModel.CreateTagCommand.Execute(null);
            Tag createdTag = null;
            this.TagRepository
                .Setup(r => r.Upsert(It.IsAny<Tag>()))
                .Callback<Tag>(t => createdTag = t)
                .Returns<Tag>(t => t);

            // ACT
            // commit editor

            this.ViewModel.EditedTag.CommitCommand.Execute(null);

            // ASSERT
            // editor is gone

            Assert.Null(this.ViewModel.EditedTag);
            Assert.NotNull(createdTag);
        }

        [Fact]
        public void KosmographViewModel_clears_new_tag_on_rollback()
        {
            // ARRANGE

            this.ViewModel.CreateTagCommand.Execute(null);

            // ACT
            // rollback editor

            this.ViewModel.EditedTag.RollbackCommand.Execute(null);

            // ASSERT
            // editor is gone

            Assert.Null(this.ViewModel.EditedTag);
        }

        [Fact]
        public void KosmographViewModel_invalidates_duplicate_tag_name_on_create()
        {
            // ARRANGE

            var tag1 = DefaultTag(t => t.Name = "t1");
            this.TagRepository
                .Setup(r => r.FindByName("t1"))
                .Returns(tag1);

            this.ViewModel.CreateTagCommand.Execute(null);

            // ACT
            // commit editor

            this.ViewModel.EditedTag.Name = "t1";
            var result = this.ViewModel.EditedTag.CommitCommand.CanExecute(null);

            // ASSERT
            // editor is gone

            Assert.False(result);
        }

        [Fact]
        public void KosmographVIewModel_deletes_tag_at_model()
        {
            // ARRANGE

            var tag = DefaultTag(t => t.Name = "t1");
            this.TagRepository
                .Setup(r => r.Delete(tag))
                .Returns(true);

            // ACT

            this.ViewModel.DeleteTagCommand.Execute(new TagViewModel(tag));
        }
    }
}