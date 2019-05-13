using Kosmograph.Desktop.Lists.ViewModel;
using Kosmograph.Messaging;
using Kosmograph.Model;
using Moq;
using System;
using System.Collections.Specialized;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Lists.Test.ViewModel
{
    public class EntityRepositoryViewModelTest : IDisposable
    {
        private MockRepository Mocks { get; } = new MockRepository(MockBehavior.Strict);
        private readonly EntityRepositoryViewModel repositoryViewModel;
        private readonly EntityMessageBus entityMessaging = new EntityMessageBus();
        private readonly TagMessageBus tagMessaging = new TagMessageBus();
        private readonly Mock<IEntityRepository> repository;

        public EntityRepositoryViewModelTest()
        {
            this.repository = this.Mocks.Create<IEntityRepository>();
            this.repositoryViewModel = new EntityRepositoryViewModel(this.repository.Object, this.entityMessaging, this.tagMessaging);
        }

        public void Dispose()
        {
            this.Mocks.VerifyAll();
        }

        public Tag DefaultTag() => new Tag("tag", new Facet("facet", new FacetProperty("p")));

        public Entity DefaultEntity() => new Entity("entity", DefaultTag());

        [Fact]
        public void EntityRepositoryViewModel_subscribes_to_entity_and_tag_changes()
        {
            // ARRANGE

            var tagMessagingMock = this.Mocks.Create<IChangedMessageBus<ITag>>();
            tagMessagingMock
                .Setup(t => t.Subscribe(It.IsAny<EntityRepositoryViewModel>()))
                .Returns(Mock.Of<IDisposable>());

            var entityMessagingMock = this.Mocks.Create<IChangedMessageBus<IEntity>>();

            entityMessagingMock
                .Setup(r => r.Subscribe(It.IsAny<EntityRepositoryViewModel>()))
                .Returns(Mock.Of<IDisposable>());

            // ACT

            var tmp = new EntityRepositoryViewModel(this.repository.Object, entityMessagingMock.Object, tagMessagingMock.Object);
        }

        [Fact]
        public void EntityRepositoryViewModel_disposes_relationship_and_tag_subscriptions()
        {
            // ARRANGE

            var tagSubscription = this.Mocks.Create<IDisposable>();
            tagSubscription
                .Setup(t => t.Dispose());

            var tagMessagingMock = this.Mocks.Create<IChangedMessageBus<ITag>>();
            tagMessagingMock
                .Setup(t => t.Subscribe(It.IsAny<EntityRepositoryViewModel>()))
                .Returns(tagSubscription.Object);

            var entitySubscription = this.Mocks.Create<IDisposable>();
            entitySubscription
                .Setup(t => t.Dispose());

            var entityMessagingMock = this.Mocks.Create<IChangedMessageBus<IEntity>>();

            entityMessagingMock
                .Setup(r => r.Subscribe(It.IsAny<EntityRepositoryViewModel>()))
                .Returns(entitySubscription.Object);

            var viewModel = new EntityRepositoryViewModel(this.repository.Object, entityMessagingMock.Object, tagMessagingMock.Object);

            // ACT

            viewModel.Dispose();
        }

        [Fact]
        public void EntityRepositoryViewModel_subscribes_entity_and_tags_events()
        {
            // ARRANGE

            var entityMessageBusMock = this.Mocks.Create<IChangedMessageBus<IEntity>>();

            entityMessageBusMock
                .Setup(m => m.Subscribe(It.IsAny<IObserver<ChangedMessage<IEntity>>>()))
                .Returns(Mock.Of<IDisposable>());

            var tagMessageBusMock = this.Mocks.Create<IChangedMessageBus<ITag>>();

            tagMessageBusMock
                .Setup(m => m.Subscribe(It.IsAny<IObserver<ChangedMessage<ITag>>>()))
                .Returns(Mock.Of<IDisposable>());

            // ACT

            var tmp = new EntityRepositoryViewModel(this.repository.Object, entityMessageBusMock.Object, tagMessageBusMock.Object);
        }

        [Fact]
        public void EntityRepositoryViewModel_creates_TagViewModel_from_all_Tags()
        {
            // ARRANGE

            var entity = DefaultEntity();

            this.repository
                .Setup(r => r.FindAll())
                .Returns(entity.Yield());

            // ACT

            this.repositoryViewModel.FillAll();

            // ASSERT
            // repos contains the single tag

            Assert.Equal(entity, this.repositoryViewModel.Single().Model);
        }

        [Fact]
        public void EntityRepositoryViewModel_replaces_EntityViewModel_on_updated()
        {
            // ARRANGE

            var entity = DefaultEntity();

            this.repository
                .Setup(r => r.FindAll())
                .Returns(entity.Yield());

            this.repository
                .Setup(r => r.FindById(entity.Id))
                .Returns(entity);

            this.repositoryViewModel.FillAll();
            var originalViewModel = this.repositoryViewModel.Single();

            bool existingEntityWasRemoved = false;
            bool existingEntityWasAdded = false;
            this.repositoryViewModel.CollectionChanged += (sender, e) =>
            {
                Assert.Same(this.repositoryViewModel, sender);
                if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.OfType<EntityViewModel>().Single().Model.Equals(entity))
                {
                    existingEntityWasRemoved = true;
                }
                if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems.OfType<EntityViewModel>().Single().Model.Equals(entity))
                {
                    existingEntityWasAdded = true;
                }
            };

            // ACT

            this.entityMessaging.Modified(entity);

            // ASSERT

            Assert.True(existingEntityWasAdded);
            Assert.True(existingEntityWasRemoved);
            Assert.NotSame(originalViewModel, this.repositoryViewModel.Single());
            Assert.Equal(entity, this.repositoryViewModel.Single().Model);
        }

        [Fact]
        public void EntityRepositoryViewModel_replacing_EntityViewModel_on_updated_removes_missing_Tag()
        {
            // ARRANGE

            var entity = DefaultEntity();

            this.repository
                .Setup(r => r.FindAll())
                .Returns(entity.Yield());

            this.repository
                .Setup(r => r.FindById(entity.Id))
                .Throws<InvalidOperationException>();

            this.repositoryViewModel.FillAll();
            var originalViewModel = this.repositoryViewModel.Single();

            bool collectionChanged = false;
            this.repositoryViewModel.CollectionChanged += (sender, e) =>
            {
                Assert.Same(this.repositoryViewModel, sender);
                Assert.Null(e.NewItems);
                Assert.Equal(entity, e.OldItems.OfType<EntityViewModel>().Single().Model);
                collectionChanged = true;
            };

            // ACT

            this.entityMessaging.Modified(entity);

            // ASSERT

            Assert.True(collectionChanged);
            Assert.False(this.repositoryViewModel.Any());
        }

        [Fact]
        public void EntityRepositoryViewModel_removes_EntityViewModel_on_removed()
        {
            // ARRANGE

            var entity = DefaultEntity();

            this.repository
                .Setup(r => r.FindAll())
                .Returns(entity.Yield());

            this.repositoryViewModel.FillAll();
            var originalViewModel = this.repositoryViewModel.Single();

            bool collectionChanged = false;
            this.repositoryViewModel.CollectionChanged += (sender, e) =>
            {
                Assert.Same(this.repositoryViewModel, sender);
                Assert.Null(e.NewItems);
                Assert.Equal(entity, e.OldItems.OfType<EntityViewModel>().Single().Model);
                collectionChanged = true;
            };

            // ACT

            this.entityMessaging.Removed(entity);

            // ASSERT

            Assert.True(collectionChanged);
            Assert.False(this.repositoryViewModel.Any());
        }

        [Fact]
        public void EntityRepositoryViewModel_adds_EntityViewModel_on_updated()
        {
            // ARRANGE

            var entity = DefaultEntity();

            this.repository
                .Setup(r => r.FindById(entity.Id))
                .Returns(entity);

            bool collectionChanged = false;
            this.repositoryViewModel.CollectionChanged += (sender, e) =>
            {
                Assert.Same(this.repositoryViewModel, sender);
                Assert.Equal(entity, e.NewItems.OfType<EntityViewModel>().Single().Model);
                Assert.Null(e.OldItems);
                collectionChanged = true;
            };

            // ACT

            this.entityMessaging.Modified(entity);

            // ASSERT
            // same tag, but new view model instance

            Assert.True(collectionChanged);
            Assert.Equal(entity, this.repositoryViewModel.Single().Model);
        }

        [Fact]
        public void EntityRepositoryViewModel_adding_EntityViewModel_on_updated_ignores_missing_entty()
        {
            // ARRANGE

            var entity = DefaultEntity();

            this.repository
                .Setup(r => r.FindById(entity.Id))
                .Throws<InvalidOperationException>();

            bool collectionChanged = false;
            this.repositoryViewModel.CollectionChanged += (sender, e) =>
            {
                collectionChanged = true;
            };

            // ACT

            this.entityMessaging.Modified(entity);

            // ASSERT

            Assert.False(collectionChanged);
            Assert.False(this.repositoryViewModel.Any());
        }

        [Fact]
        public void EntityRepositoryViewModel_removes_Entity()
        {
            // ARRANGE

            var entity = DefaultEntity();

            this.repository
                .Setup(r => r.FindAll())
                .Returns(entity.Yield());

            // deleting a tag at the repos raises a changed event
            this.repository
                .Setup(r => r.Delete(entity))
                .Callback(() => this.entityMessaging.Removed(entity))
                .Returns(true);

            this.repositoryViewModel.FillAll();
            var originalViewModel = this.repositoryViewModel.Single();

            bool collectionChanged = false;
            this.repositoryViewModel.CollectionChanged += (sender, e) =>
            {
                Assert.Same(this.repositoryViewModel, sender);
                Assert.Null(e.NewItems);
                Assert.Equal(entity, e.OldItems.OfType<EntityViewModel>().Single().Model);
                collectionChanged = true;
            };

            // ACT

            this.repositoryViewModel.DeleteCommand.Execute(this.repositoryViewModel.Single());

            // ASSERT
            // same tag, but new view model instance

            Assert.True(collectionChanged);
            Assert.False(this.repositoryViewModel.Any());
        }

        [Fact]
        public void EntityRepositoryViewModel_updates_Entity_with_modified_Tag()
        {
            // ARRANGE

            var entity = DefaultEntity();

            this.repository
                .Setup(r => r.FindAll())
                .Returns(entity.Yield());

            this.repository
                .Setup(r => r.FindById(entity.Id))
                .Returns(entity);

            this.repositoryViewModel.FillAll();
            var originalViewModel = this.repositoryViewModel.Single();

            bool existingEntityWasRemoved = false;
            bool existingEntityWasAdded = false;
            this.repositoryViewModel.CollectionChanged += (sender, e) =>
            {
                Assert.Same(this.repositoryViewModel, sender);
                if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.OfType<EntityViewModel>().Single().Model.Equals(entity))
                {
                    existingEntityWasRemoved = true;
                }
                if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems.OfType<EntityViewModel>().Single().Model.Equals(entity))
                {
                    existingEntityWasAdded = true;
                }
            };

            // ACT

            this.tagMessaging.Modified(entity.Tags.Single());

            // ASSERT

            Assert.True(existingEntityWasAdded);
            Assert.True(existingEntityWasRemoved);
            Assert.NotSame(originalViewModel, this.repositoryViewModel.Single());
            Assert.Equal(entity, this.repositoryViewModel.Single().Model);
        }

        [Fact]
        public void EntityRepositoryViewModel_updates_Entity_with_removed_Tag()
        {
            // ARRANGE

            var entity = DefaultEntity();

            this.repository
                .Setup(r => r.FindAll())
                .Returns(entity.Yield());

            this.repository
                .Setup(r => r.FindById(entity.Id))
                .Returns(entity);

            this.repositoryViewModel.FillAll();
            var originalViewModel = this.repositoryViewModel.Single();

            bool existingEntityWasRemoved = false;
            bool existingEntityWasAdded = false;
            this.repositoryViewModel.CollectionChanged += (sender, e) =>
            {
                Assert.Same(this.repositoryViewModel, sender);
                if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.OfType<EntityViewModel>().Single().Model.Equals(entity))
                {
                    existingEntityWasRemoved = true;
                }
                if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems.OfType<EntityViewModel>().Single().Model.Equals(entity))
                {
                    existingEntityWasAdded = true;
                }
            };

            // ACT

            this.tagMessaging.Removed(entity.Tags.Single());

            // ASSERT

            Assert.True(existingEntityWasAdded);
            Assert.True(existingEntityWasRemoved);
            Assert.NotSame(originalViewModel, this.repositoryViewModel.Single());
            Assert.Equal(entity, this.repositoryViewModel.Single().Model);
        }
    }
}