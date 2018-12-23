using Kosmograph.Messaging;
using Kosmograph.Model;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Lists.ViewModel.Test
{
    public class TagRepositoryViewModelTest : IDisposable
    {
        private readonly TagRepositoryViewModel repositoryViewModel;
        private readonly TagMessageBus tagMessaging = new TagMessageBus();
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);
        private readonly Mock<ITagRepository> tagRepository;

        public TagRepositoryViewModelTest()
        {
            this.tagRepository = this.mocks.Create<ITagRepository>();
            this.repositoryViewModel = new TagRepositoryViewModel(this.tagRepository.Object, this.tagMessaging);
        }

        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        [Fact]
        public void TagRepositoryViewModel_creates_TagViewModel_from_all_Tags()
        {
            // ARRANGE

            var tag = new Tag("tag");

            this.tagRepository
                .Setup(r => r.FindAll())
                .Returns(tag.Yield());

            // ACT

            this.repositoryViewModel.FillAll();

            // ASSERT
            // repos contains the single tag

            Assert.Equal(tag, this.repositoryViewModel.Single().Model);
        }

        [Fact]
        public void TagRepositoryViewModel_replaces_TagViewModel_on_updated()
        {
            // ARRANGE

            var tag = new Tag("tag");

            this.tagRepository
                .Setup(r => r.FindAll())
                .Returns(tag.Yield());

            this.tagRepository
                .Setup(r => r.FindById(tag.Id))
                .Returns(tag);

            this.repositoryViewModel.FillAll();
            var originalViewModel = this.repositoryViewModel.Single();

            bool collectionChanged = false;
            this.repositoryViewModel.CollectionChanged += (sender, e) =>
            {
                Assert.Same(this.repositoryViewModel, sender);
                Assert.Equal(tag, e.NewItems.OfType<TagViewModel>().Single().Model);
                Assert.Equal(tag, e.OldItems.OfType<TagViewModel>().Single().Model);
                collectionChanged = true;
            };

            // ACT

            this.tagMessaging.Modified(tag);

            // ASSERT
            // same tag, but new view model instance

            Assert.True(collectionChanged);
            Assert.NotSame(originalViewModel, this.repositoryViewModel.Single());
            Assert.Equal(tag, this.repositoryViewModel.Single().Model);
        }

        [Fact]
        public void TagRepositoryViewModel_replacing_TagViewModel_on_updated_removes_missing_Tag()
        {
            // ARRANGE

            var tag = new Tag("tag");

            this.tagRepository
                .Setup(r => r.FindAll())
                .Returns(tag.Yield());

            this.tagRepository
                .Setup(r => r.FindById(tag.Id))
                .Throws<InvalidOperationException>();

            this.repositoryViewModel.FillAll();
            var originalViewModel = this.repositoryViewModel.Single();

            bool collectionChanged = false;
            this.repositoryViewModel.CollectionChanged += (sender, e) =>
            {
                Assert.Same(this.repositoryViewModel, sender);
                Assert.Null(e.NewItems);
                Assert.Equal(tag, e.OldItems.OfType<TagViewModel>().Single().Model);
                collectionChanged = true;
            };

            // ACT

            this.tagMessaging.Modified(tag);

            // ASSERT
            // tag was removed instead

            Assert.True(collectionChanged);
            Assert.False(this.repositoryViewModel.Any());
        }

        [Fact]
        public void TagRepositoryViewModel_removes_TagViewModel_on_removed()
        {
            // ARRANGE

            var tag = new Tag("tag");

            this.tagRepository
                .Setup(r => r.FindAll())
                .Returns(tag.Yield());

            this.repositoryViewModel.FillAll();
            var originalViewModel = this.repositoryViewModel.Single();

            bool collectionChanged = false;
            this.repositoryViewModel.CollectionChanged += (sender, e) =>
            {
                Assert.Same(this.repositoryViewModel, sender);
                Assert.Null(e.NewItems);
                Assert.Equal(tag, e.OldItems.OfType<TagViewModel>().Single().Model);
                collectionChanged = true;
            };

            // ACT

            this.tagMessaging.Removed(tag);

            // ASSERT
            // same tag, but new view model instance

            Assert.True(collectionChanged);
            Assert.False(this.repositoryViewModel.Any());
        }

        [Fact]
        public void TagRepositoryViewModel_adds_TagViewModel_on_updated()
        {
            // ARRANGE

            var tag = new Tag("tag");

            this.tagRepository
                .Setup(r => r.FindById(tag.Id))
                .Returns(tag);

            bool collectionChanged = false;
            this.repositoryViewModel.CollectionChanged += (sender, e) =>
            {
                Assert.Same(this.repositoryViewModel, sender);
                Assert.Equal(tag, e.NewItems.OfType<TagViewModel>().Single().Model);
                Assert.Null(e.OldItems);
                collectionChanged = true;
            };

            // ACT

            this.tagMessaging.Modified(tag);

            // ASSERT
            // same tag, but new view model instance

            Assert.True(collectionChanged);
            Assert.Equal(tag, this.repositoryViewModel.Single().Model);
        }

        [Fact]
        public void TagRepositoryViewModel_adding_TagViewModel_on_updated_ignores_missing_tag()
        {
            // ARRANGE

            var tag = new Tag("tag");

            this.tagRepository
                .Setup(r => r.FindById(tag.Id))
                .Throws<InvalidOperationException>();

            bool collectionChanged = false;
            this.repositoryViewModel.CollectionChanged += (sender, e) =>
            {
                collectionChanged = true;
            };

            // ACT

            this.tagMessaging.Modified(tag);

            // ASSERT
            // same tag, but new view model instance

            Assert.False(collectionChanged);
            Assert.False(this.repositoryViewModel.Any());
        }
    }
}