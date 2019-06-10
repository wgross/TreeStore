using Kosmograph.Messaging;
using Moq;
using System;
using Xunit;

namespace Kosmograph.Model.Test
{
    public class ModelControllerTest : ModelTestBase
    {
        private readonly ModelController modelController;

        public ModelControllerTest()
        {
            // this.MessageBus = new KosmographMessageBus();
            this.modelController = new ModelController(this.MessageBus);
        }

        [Fact]
        public void ModelController_observes_all_repos()
        {
            // ARRANGE

            var tags = this.Mocks.Create<IChangedMessageBus<ITag>>();
            ModelController tagSubsciber = null;
            tags
                .Setup(t => t.Subscribe(It.IsAny<IObserver<ChangedMessage<ITag>>>()))
                .Callback<IObserver<ChangedMessage<ITag>>>(m => tagSubsciber = (ModelController)m)
                .Returns(Mock.Of<IDisposable>());

            ModelController entitySubscriber = null;
            var entities = this.Mocks.Create<IChangedMessageBus<IEntity>>();
            entities
                .Setup(t => t.Subscribe(It.IsAny<IObserver<ChangedMessage<IEntity>>>()))
                .Callback<IObserver<ChangedMessage<IEntity>>>(m => entitySubscriber = (ModelController)m)
                .Returns(Mock.Of<IDisposable>());

            ModelController relationshipSubscriber = null;
            var relationships = this.Mocks.Create<IChangedMessageBus<IRelationship>>();

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
        public void ModelController_dispose_stops_observing_all_repos()
        {
            // ARRANGE

            IDisposable make_disposable()
            {
                var tmp = this.Mocks.Create<IDisposable>();
                tmp.Setup(d => d.Dispose());
                return tmp.Object;
            }

            var tags = this.Mocks.Create<IChangedMessageBus<ITag>>();
            ModelController tagSubsciber = null;
            tags
                .Setup(t => t.Subscribe(It.IsAny<IObserver<ChangedMessage<ITag>>>()))
                .Callback<IObserver<ChangedMessage<ITag>>>(m => tagSubsciber = (ModelController)m)
                .Returns(make_disposable());

            ModelController entitySubscriber = null;
            var entities = this.Mocks.Create<IChangedMessageBus<IEntity>>();
            entities
                .Setup(t => t.Subscribe(It.IsAny<IObserver<ChangedMessage<IEntity>>>()))
                .Callback<IObserver<ChangedMessage<IEntity>>>(m => entitySubscriber = (ModelController)m)
                .Returns(make_disposable());

            ModelController relationshipSubscriber = null;
            var relationships = this.Mocks.Create<IChangedMessageBus<IRelationship>>();

            relationships
                .Setup(t => t.Subscribe(It.IsAny<IObserver<ChangedMessage<IRelationship>>>()))
                .Callback<IObserver<ChangedMessage<IRelationship>>>(m => relationshipSubscriber = (ModelController)m)
                .Returns(make_disposable());

            var controller = new ModelController(tags.Object, entities.Object, relationships.Object);

            // ACT

            controller.Dispose();
        }

        [Fact]
        public void ModelController_raises_TagChanged_on_changed_message()
        {
            // ARRANGE

            var tag = DefaultTag();

            // ACT

            var result = default(Tag);
            this.modelController.TagChanged = t => result = t;
            this.MessageBus.Tags.Modified(tag);

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
            this.MessageBus.Tags.Removed(tag);

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
            this.MessageBus.Entities.Modified(entity);

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
            this.MessageBus.Entities.Removed(entity);

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
            this.MessageBus.Relationships.Modified(relationship);

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
            this.MessageBus.Relationships.Removed(relationship);

            // ASSERT

            Assert.Equal(relationship.Id, result);
        }
    }
}