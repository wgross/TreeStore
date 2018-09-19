using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
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

            var tag = new Tag("t", Facet.Empty);
            this.model
                .Setup(r => r.FindAll())
                .Returns(tag.Yield());

            // ACT

            this.viewModel.FillAll();

            // ASSERT

            Assert.Single(this.viewModel);
            Assert.Equal(tag, this.viewModel.Single().Model);
        }

        //[Fact]
        //public void TagRepositoryViewModel_delays_created_Tag_from_Model()
        //{
        //    // ARRANGE

        //    var tag = new Tag("t", Facet.Empty);
        //    this.tagRepository
        //        .Setup(r => r.FindAll())
        //        .Returns(tag.Yield());

        //    // ACT

        //    this.viewModel.CreateTagCommand.Execute(null);

        //    // ASSERT

        //    Assert.Single(this.viewModel.Tags);
        //    Assert.Equal("new tag", this.viewModel.EditedTag.ViewModel.Model.Name);
        //}

        //[Fact]
        //public void KosmographViewModel_commits_created_Tag_to_KosmographModel()
        //{
        //    // ARRANGE

        //    this.persistence
        //      .Setup(p => p.Tags)
        //      .Returns(this.tagRepository.Object);

        //    var tag = new Tag("t", Facet.Empty);
        //    this.tagRepository
        //        .Setup(r => r.FindAll())
        //        .Returns(tag.Yield());

        //    this.tagRepository
        //        .Setup(r => r.Upsert(It.IsAny<Tag>()))
        //        .Returns<Tag>(t => t);

        //    // create new tag
        //    this.viewModel.CreateTagCommand.Execute(null);

        //    // ACT

        //    this.viewModel.EditedTag.CommitCommand.Execute(null);

        //    // ASSERT
        //    // tag is inserted in the list after commit

        //    Assert.Equal(2, this.viewModel.Tags.Count);
        //    Assert.Equal("new tag", this.viewModel.Tags.ElementAt(1).Name);
        //    Assert.Null(this.viewModel.EditedTag);
        //    Assert.Equal(this.viewModel.Tags.ElementAt(1), this.viewModel.SelectedTag);
        //}

        //[Fact]
        //public void KosmographViewModel_reverts_created_Tag_at_KosmographViewModel()
        //{
        //    // ARRANGE

        //    this.persistence
        //      .Setup(p => p.Tags)
        //      .Returns(this.tagRepository.Object);

        //    var tag = new Tag("t", Facet.Empty);
        //    this.tagRepository
        //        .Setup(r => r.FindAll())
        //        .Returns(tag.Yield());

        //    // create new tag
        //    this.viewModel.CreateTagCommand.Execute(null);

        //    // ACT

        //    this.viewModel.EditedTag.RollbackCommand.Execute(null);

        //    // ASSERT
        //    // tag is forgotton

        //    Assert.Single(this.viewModel.Tags);
        //    Assert.Null(this.viewModel.EditedTag);
        //}

        [Fact]
        public void KosmographViewModel_deletes_Tag_from_Model()
        {
            // ARRANGE

            var tag = new Tag("t", Facet.Empty);
            this.model
                .Setup(r => r.FindAll())
                .Returns(tag.Yield());

            this.model
                .Setup(r => r.Delete(tag.Id))
                .Returns(true);

            this.viewModel.FillAll();

            // ACT

            this.viewModel.DeleteCommand.Execute(this.viewModel.Single());

            // ASSERT

            Assert.Empty(this.viewModel);
        }
    }
}