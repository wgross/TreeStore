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
    public class RelationshipRepositoryViewModelTest : IDisposable
    {
        private readonly RelationshipRepositoryViewModel repositoryViewModel;
        private readonly RelationshipMessageBus relationshipMessaging = new RelationshipMessageBus();
        private readonly TagMessageBus tagMessaging = new TagMessageBus();
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);
        private readonly Mock<IRelationshipRepository> repository;

        public RelationshipRepositoryViewModelTest()
        {
            this.repository = this.mocks.Create<IRelationshipRepository>();
            this.repositoryViewModel = new RelationshipRepositoryViewModel(this.repository.Object, this.relationshipMessaging, this.tagMessaging);
        }

        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        public Tag DefaultTag() => new Tag("tag", new Facet("facet", new FacetProperty("p")));

        public Entity DefaultEntity() => new Entity("entity", DefaultTag());

        public Relationship DefaultRelationship() => new Relationship("entity", from: DefaultEntity(), to: DefaultEntity(), tags: DefaultTag());

        [Fact]
        public void RelationshipRepositoryViewModel_subscribes_to_relationship_and_tag_changes()
        {
            // ARRANGE

            var tagMessagingMock = this.mocks.Create<IChangedMessageBus<ITag>>();
            tagMessagingMock
                .Setup(t => t.Subscribe(It.IsAny<RelationshipRepositoryViewModel>()))
                .Returns(Mock.Of<IDisposable>());

            var relationshipMessagingMock = this.mocks.Create<IChangedMessageBus<IRelationship>>();

            relationshipMessagingMock
                .Setup(r => r.Subscribe(It.IsAny<RelationshipRepositoryViewModel>()))
                .Returns(Mock.Of<IDisposable>());

            // ACT

            var tmp = new RelationshipRepositoryViewModel(this.repository.Object, relationshipMessagingMock.Object, tagMessagingMock.Object);
        }

        [Fact]
        public void RelationshipRepositoryViewModel_disposes_to_relationship_and_tag_subscriptions()
        {
            // ARRANGE

            var tagSubscription = this.mocks.Create<IDisposable>();
            tagSubscription
                .Setup(t => t.Dispose());

            var tagMessagingMock = this.mocks.Create<IChangedMessageBus<ITag>>();
            tagMessagingMock
                .Setup(t => t.Subscribe(It.IsAny<RelationshipRepositoryViewModel>()))
                .Returns(tagSubscription.Object);

            var relationshipSubscription = this.mocks.Create<IDisposable>();
            relationshipSubscription
                .Setup(t => t.Dispose());

            var relationshipMessagingMock = this.mocks.Create<IChangedMessageBus<IRelationship>>();

            relationshipMessagingMock
                .Setup(r => r.Subscribe(It.IsAny<RelationshipRepositoryViewModel>()))
                .Returns(relationshipSubscription.Object);

            var viewModel = new RelationshipRepositoryViewModel(this.repository.Object, relationshipMessagingMock.Object, tagMessagingMock.Object);

            // ACT

            viewModel.Dispose();
        }

        [Fact]
        public void RelationshipRepositoryViewModel_creates_TagViewModel_from_all_Tags()
        {
            // ARRANGE

            var entity = DefaultRelationship();

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
        public void RelationshipRepositoryViewModel_replaces_RelationshipViewModel_on_updated()
        {
            // ARRANGE

            var relationship = DefaultRelationship();

            this.repository
                .Setup(r => r.FindAll())
                .Returns(relationship.Yield());

            this.repository
                .Setup(r => r.FindById(relationship.Id))
                .Returns(relationship);

            this.repositoryViewModel.FillAll();
            var originalViewModel = this.repositoryViewModel.Single();

            bool existingRelationshipWasRemoved = false;
            bool existingRelationshipWasAdded = false;
            this.repositoryViewModel.CollectionChanged += (sender, e) =>
            {
                Assert.Same(this.repositoryViewModel, sender);
                if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.OfType<RelationshipViewModel>().Single().Model.Equals(relationship))
                {
                    existingRelationshipWasRemoved = true;
                }
                if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems.OfType<RelationshipViewModel>().Single().Model.Equals(relationship))
                {
                    existingRelationshipWasAdded = true;
                }
            };

            // ACT

            this.relationshipMessaging.Modified(relationship);

            // ASSERT

            Assert.True(existingRelationshipWasRemoved);
            Assert.True(existingRelationshipWasAdded);
            Assert.NotSame(originalViewModel, this.repositoryViewModel.Single());
            Assert.Equal(relationship, this.repositoryViewModel.Single().Model);
        }

        [Fact]
        public void RelationshipRepositoryViewModel_replacing_RelationshipViewModel_on_updated_removes_missing_Tag()
        {
            // ARRANGE

            var entity = DefaultRelationship();

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
                Assert.Equal(entity, e.OldItems.OfType<RelationshipViewModel>().Single().Model);
                collectionChanged = true;
            };

            // ACT

            this.relationshipMessaging.Modified(entity);

            // ASSERT

            Assert.True(collectionChanged);
            Assert.False(this.repositoryViewModel.Any());
        }

        [Fact]
        public void RelationshipRepositoryViewModel_removes_RelationshipViewModel_on_removed()
        {
            // ARRANGE

            var entity = DefaultRelationship();

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
                Assert.Equal(entity, e.OldItems.OfType<RelationshipViewModel>().Single().Model);
                collectionChanged = true;
            };

            // ACT

            this.relationshipMessaging.Removed(entity);

            // ASSERT

            Assert.True(collectionChanged);
            Assert.False(this.repositoryViewModel.Any());
        }

        [Fact]
        public void RelationshipRepositoryViewModel_adds_RelationshipViewModel_on_updated()
        {
            // ARRANGE

            var entity = DefaultRelationship();

            this.repository
                .Setup(r => r.FindById(entity.Id))
                .Returns(entity);

            bool collectionChanged = false;
            this.repositoryViewModel.CollectionChanged += (sender, e) =>
            {
                Assert.Same(this.repositoryViewModel, sender);
                Assert.Equal(entity, e.NewItems.OfType<RelationshipViewModel>().Single().Model);
                Assert.Null(e.OldItems);
                collectionChanged = true;
            };

            // ACT

            this.relationshipMessaging.Modified(entity);

            // ASSERT
            // same tag, but new view model instance

            Assert.True(collectionChanged);
            Assert.Equal(entity, this.repositoryViewModel.Single().Model);
        }

        [Fact]
        public void RelationshipRepositoryViewModel_adding_TagViewModel_on_update_ignores_missing_relationship()
        {
            // ARRANGE

            var entity = DefaultRelationship();

            this.repository
                .Setup(r => r.FindById(entity.Id))
                .Throws<InvalidOperationException>();

            bool collectionChanged = false;
            this.repositoryViewModel.CollectionChanged += (sender, e) =>
            {
                collectionChanged = true;
            };

            // ACT

            this.relationshipMessaging.Modified(entity);

            // ASSERT

            Assert.False(collectionChanged);
            Assert.False(this.repositoryViewModel.Any());
        }

        [Fact]
        public void RelationshipRepositoryViewModel_removes_Relationship()
        {
            // ARRANGE

            var relationship = DefaultRelationship();

            this.repository
                .Setup(r => r.FindAll())
                .Returns(relationship.Yield());

            // deleting a tag at the repos raises a changed event
            this.repository
                .Setup(r => r.Delete(relationship))
                .Callback(() => this.relationshipMessaging.Removed(relationship))
                .Returns(true);

            this.repositoryViewModel.FillAll();
            var originalViewModel = this.repositoryViewModel.Single();

            bool collectionChanged = false;
            this.repositoryViewModel.CollectionChanged += (sender, e) =>
            {
                Assert.Same(this.repositoryViewModel, sender);
                Assert.Null(e.NewItems);
                Assert.Equal(relationship, e.OldItems.OfType<RelationshipViewModel>().Single().Model);
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
        public void RelationshipRepositoryViewModel_updates_Relationship_with_modified_Tag()
        {
            // ARRANGE

            var relationship = DefaultRelationship();

            this.repository
                .Setup(r => r.FindAll())
                .Returns(relationship.Yield());

            this.repository
                .Setup(r => r.FindById(relationship.Id))
                .Returns(relationship);

            this.repositoryViewModel.FillAll();
            var originalViewModel = this.repositoryViewModel.Single();

            bool existingEntityWasRemoved = false;
            bool existingEntityWasAdded = false;
            this.repositoryViewModel.CollectionChanged += (sender, e) =>
            {
                Assert.Same(this.repositoryViewModel, sender);
                if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.OfType<RelationshipViewModel>().Single().Model.Equals(relationship))
                {
                    existingEntityWasRemoved = true;
                }
                if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems.OfType<RelationshipViewModel>().Single().Model.Equals(relationship))
                {
                    existingEntityWasAdded = true;
                }
            };

            // ACT

            this.tagMessaging.Modified(relationship.Tags.Single());

            // ASSERT

            Assert.True(existingEntityWasAdded);
            Assert.True(existingEntityWasRemoved);
            Assert.NotSame(originalViewModel, this.repositoryViewModel.Single());
            Assert.Equal(relationship, this.repositoryViewModel.Single().Model);
        }

        [Fact]
        public void RelationshipRepositoryViewModel_updates_Relationship_with_removed_Tag()
        {
            // ARRANGE

            var relationship = DefaultRelationship();

            this.repository
                .Setup(r => r.FindAll())
                .Returns(relationship.Yield());

            this.repository
                .Setup(r => r.FindById(relationship.Id))
                .Returns(relationship);

            this.repositoryViewModel.FillAll();
            var originalViewModel = this.repositoryViewModel.Single();

            bool existingEntityWasRemoved = false;
            bool existingEntityWasAdded = false;
            this.repositoryViewModel.CollectionChanged += (sender, e) =>
            {
                Assert.Same(this.repositoryViewModel, sender);
                if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.OfType<RelationshipViewModel>().Single().Model.Equals(relationship))
                {
                    existingEntityWasRemoved = true;
                }
                if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems.OfType<RelationshipViewModel>().Single().Model.Equals(relationship))
                {
                    existingEntityWasAdded = true;
                }
            };

            // ACT

            this.tagMessaging.Removed(relationship.Tags.Single());

            // ASSERT

            Assert.True(existingEntityWasAdded);
            Assert.True(existingEntityWasRemoved);
            Assert.NotSame(originalViewModel, this.repositoryViewModel.Single());
            Assert.Equal(relationship, this.repositoryViewModel.Single().Model);
        }
    }
}