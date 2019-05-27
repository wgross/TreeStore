using Kosmograph.Messaging;
using Moq;
using System;
using Xunit;

namespace Kosmograph.Model.Test
{
    public class ModelControllerTest
    {
        private readonly KosmographMessageBus messageBus;
        private readonly ModelController modelController;
        private MockRepository mocks = new MockRepository(MockBehavior.Strict);

        public ModelControllerTest()
        {
            this.messageBus = new KosmographMessageBus();
            this.modelController = new ModelController(this.messageBus);
        }

        private Tag DefaultTag() => new Tag("t");

        private Entity DefaultEntity() => new Entity("e");

        private Relationship DefaultRelationship() => new Relationship("r", DefaultEntity(), DefaultEntity());

        [Fact]
        public void ModelController_observes_all_repos()
        {
            // ARRANGE

            var tags = this.mocks.Create<IChangedMessageBus<ITag>>();
            ModelController tagSubsciber = null;
            tags
                .Setup(t => t.Subscribe(It.IsAny<IObserver<ChangedMessage<ITag>>>()))
                .Callback<IObserver<ChangedMessage<ITag>>>(m => tagSubsciber = (ModelController)m)
                .Returns(Mock.Of<IDisposable>());

            ModelController entitySubscriber = null;
            var entities = this.mocks.Create<IChangedMessageBus<IEntity>>();
            entities
                .Setup(t => t.Subscribe(It.IsAny<IObserver<ChangedMessage<IEntity>>>()))
                .Callback<IObserver<ChangedMessage<IEntity>>>(m => entitySubscriber = (ModelController)m)
                .Returns(Mock.Of<IDisposable>());

            ModelController relationshipSubscriber = null;
            var relationships = this.mocks.Create<IChangedMessageBus<IRelationship>>();

            relationships
                .Setup(t => t.Subscribe(It.IsAny<IObserver<ChangedMessage<IRelationship>>>()))
                .Callback<IObserver<ChangedMessage<IRelationship>>>(m => relationshipSubscriber = (ModelController)m)
                .Returns(Mock.Of<IDisposable>());

            // ACT

            var result = new ModelController(tags.Object, entities.Object, relationships.Object);

            // ASSERT

            Assert.Same(result, tagSubsciber);
            Assert.Same(result, entitySubscriber);
            Assert.Same(result, relationshipSubscriber);
        }

        [Fact]
        public void ModelController_raises_TagChanged_on_changed_message()
        {
            // ARRANGE

            var tag = DefaultTag();

            // ACT

            var result = default(Tag);
            this.modelController.TagChanged = t => result = t;
            this.messageBus.Tags.Modified(tag);

            // ASSERT

            Assert.Same(tag, result);
        }

        [Fact]
        public void ModelController_raises_TagRemoved_on_removed_message()
        {
            // ARRANGE

            var tag = DefaultTag();

            // ACT

            var result = default(Guid?);
            this.modelController.TagRemoved = id => result = id;
            this.messageBus.Tags.Removed(tag);

            // ASSERT

            Assert.Equal(tag.Id, result);
        }

        [Fact]
        public void ModelController_raises_EntityChanged_on_changed_message()
        {
            // ARRANGE

            var entity = DefaultEntity();

            // ACT

            var result = default(Entity);
            this.modelController.EntityChanged = t => result = t;
            this.messageBus.Entities.Modified(entity);

            // ASSERT

            Assert.Same(entity, result);
        }

        [Fact]
        public void ModelController_raises_EntityRemoved_on_removed_message()
        {
            // ARRANGE

            var entity = DefaultEntity();

            // ACT

            var result = default(Guid?);
            this.modelController.EntityRemoved = id => result = id;
            this.messageBus.Entities.Removed(entity);

            // ASSERT

            Assert.Equal(entity.Id, result);
        }

        [Fact]
        public void ModelController_raises_RelationshipChanged_on_changed_message()
        {
            // ARRANGE

            var relationship = DefaultRelationship();

            // ACT

            var result = default(Relationship);
            this.modelController.RelationshipChanged = t => result = t;
            this.messageBus.Relationships.Modified(relationship);

            // ASSERT

            Assert.Same(relationship, result);
        }

        [Fact]
        public void ModelController_raises_RelationshipRemoved_on_removed_message()
        {
            // ARRANGE

            var relationship = DefaultRelationship();

            // ACT

            var result = default(Guid?);
            this.modelController.RelationshipRemoved = id => result = id;
            this.messageBus.Relationships.Removed(relationship);

            // ASSERT

            Assert.Equal(relationship.Id, result);
        }
    }
}