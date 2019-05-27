using Kosmograph.Messaging;
using Moq;
using System;
using Xunit;

namespace Kosmograph.Model.Test
{
    public class TrackingModelControllerTest
    {
        private readonly KosmographMessageBus messageBus;
        private readonly TrackingModelController modelController;
        private MockRepository mocks = new MockRepository(MockBehavior.Strict);

        public TrackingModelControllerTest()
        {
            this.messageBus = new KosmographMessageBus();
            this.modelController = new TrackingModelController(this.messageBus);
        }

        private Tag DefaultTag() => new Tag("t");

        private Entity DefaultEntity() => new Entity("e");

        private Relationship DefaultRelationship() => new Relationship("r", DefaultEntity(), DefaultEntity());

        [Fact]
        public void ModelController_raises_TagAdded_on_first_changed_message()
        {
            // ARRANGE

            var tag = DefaultTag();

            // ACT

            var result = default(Tag);
            this.modelController.TagAdded = t => result = t;
            this.messageBus.Tags.Modified(tag);

            // ASSERT

            Assert.Same(tag, result);
        }

        [Fact]
        public void ModelController_raises_TagChanged_on_second_changed_message()
        {
            // ARRANGE

            var tag = DefaultTag();
            this.messageBus.Tags.Modified(tag);

            // ACT

            var result = default(Tag);
            this.modelController.TagChanged = t => result = t;
            this.messageBus.Tags.Modified(tag);

            // ASSERT

            Assert.Same(tag, result);
        }

        [Fact]
        public void ModelController_doesnt_raise_TagRemoved_on_second_removed_message()
        {
            // ARRANGE

            var tag = DefaultTag();
            this.messageBus.Tags.Modified(tag);
            this.messageBus.Tags.Removed(tag);

            // ACT

            var result = default(Guid?);
            this.modelController.TagRemoved = id => result = id;
            this.messageBus.Tags.Removed(tag);

            // ASSERT

            Assert.Null(result);
        }

        [Fact]
        public void ModelController_raises_EntityAdded_on_first_changed_message()
        {
            // ARRANGE

            var entity = DefaultEntity();

            // ACT

            var result = default(Entity);
            this.modelController.EntityAdded = e => result = e;
            this.messageBus.Entities.Modified(entity);

            // ASSERT

            Assert.Same(entity, result);
        }

        [Fact]
        public void ModelController_raises_EntityChanged_on_second_changed_message()
        {
            // ARRANGE

            var entity = DefaultEntity();
            this.messageBus.Entities.Modified(entity);

            // ACT

            var result = default(Entity);
            this.modelController.EntityChanged = t => result = t;
            this.messageBus.Entities.Modified(entity);

            // ASSERT

            Assert.Same(entity, result);
        }

        [Fact]
        public void ModelController_doesnt_raise_EntityRemoved_on_second_removed_message()
        {
            // ARRANGE

            var entity = DefaultEntity();
            this.messageBus.Entities.Modified(entity);
            this.messageBus.Entities.Removed(entity);

            // ACT

            var result = default(Guid?);
            this.modelController.EntityRemoved = id => result = id;
            this.messageBus.Entities.Removed(entity);

            // ASSERT

            Assert.Null(result);
        }

        [Fact]
        public void ModelController_raises_RelationshipAdded_on_first_changed_message()
        {
            // ARRANGE

            var relationship = DefaultRelationship();

            // ACT

            var result = default(Relationship);
            this.modelController.RelationshipAdded = t => result = t;
            this.messageBus.Relationships.Modified(relationship);

            // ASSERT

            Assert.Same(relationship, result);
        }

        [Fact]
        public void ModelController_raises_RelationshipChanged_on_second_changed_message()
        {
            // ARRANGE

            var entity = DefaultRelationship();
            this.messageBus.Relationships.Modified(entity);

            // ACT

            var result = default(Relationship);
            this.modelController.RelationshipChanged = t => result = t;
            this.messageBus.Relationships.Modified(entity);

            // ASSERT

            Assert.Same(entity, result);
        }

        [Fact]
        public void ModelController_doesnt_raise_RelationshipRemoved_on_second_removed_message()
        {
            // ARRANGE

            var relationship = DefaultRelationship();
            this.messageBus.Relationships.Modified(relationship);
            this.messageBus.Relationships.Removed(relationship);

            // ACT

            var result = default(Guid?);
            this.modelController.RelationshipRemoved = id => result = id;
            this.messageBus.Relationships.Removed(relationship);

            // ASSERT

            Assert.Null(result);
        }
    }
}