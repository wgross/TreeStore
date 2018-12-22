using Moq;
using System;
using Xunit;

namespace Kosmograph.Messaging.Test
{
    public class RelationshipMessagesTest : IDisposable
    {
        private readonly KosmographMessageBus messageBus = new KosmographMessageBus();
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);

        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        [Fact]
        public void MessageBus_notifies_of_modified_relationship()
        {
            // ARRANGE

            var relationship = Mock.Of<IRelationship>();

            var relationshipObserver = this.mocks.Create<IObserver<ChangedMessage<IRelationship>>>();
            ChangedMessage<IRelationship> relationshipChangeMessage = null;
            relationshipObserver
                .Setup(o => o.OnNext(It.IsAny<ChangedMessage<IRelationship>>()))
                .Callback<ChangedMessage<IRelationship>>(m => relationshipChangeMessage = m);

            this.messageBus.Relationships.Subscribe(relationshipObserver.Object);

            // ACT

            this.messageBus.Relationships.Modified(relationship);

            // ASSERT

            Assert.Equal(ChangeTypeValues.Modified, relationshipChangeMessage.ChangeType);
            Assert.Same(relationship, relationshipChangeMessage.Changed);
        }

        [Fact]
        public void MessageBus_notifies_of_removed_relationship()
        {
            // ARRANGE

            var relationship = Mock.Of<IRelationship>();

            var relationshipObserver = this.mocks.Create<IObserver<ChangedMessage<IRelationship>>>();
            ChangedMessage<IRelationship> relationshipChangeMessage = null;
            relationshipObserver
                .Setup(o => o.OnNext(It.IsAny<ChangedMessage<IRelationship>>()))
                .Callback<ChangedMessage<IRelationship>>(m => relationshipChangeMessage = m);

            this.messageBus.Relationships.Subscribe(relationshipObserver.Object);

            // ACT

            this.messageBus.Relationships.Removed(relationship);

            // ASSERT

            Assert.Equal(ChangeTypeValues.Removed, relationshipChangeMessage.ChangeType);
            Assert.Same(relationship, relationshipChangeMessage.Changed);
        }
    }
}