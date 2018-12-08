using Moq;
using System;
using Xunit;

namespace Kosmograph.Messaging.Test
{
    public class EntityMessagesTest : IDisposable
    {
        private readonly KosmographMessageBus messageBus = new KosmographMessageBus();
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);

        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        [Fact]
        public void MessageBus_notifies_of_added_entity()
        {
            // ARRANGE

            var entity = Mock.Of<IEntity>();

            var entityObserver = this.mocks.Create<IObserver<ChangedMessage<IEntity>>>();
            ChangedMessage<IEntity> entityChangeMessage = null;
            entityObserver
                .Setup(o => o.OnNext(It.IsAny<ChangedMessage<IEntity>>()))
                .Callback<ChangedMessage<IEntity>>(m => entityChangeMessage = m);

            this.messageBus.Entities.Subscribe(entityObserver.Object);

            // ACT

            this.messageBus.Entities.Added(entity);

            // ASSERT

            Assert.Equal(ChangeTypeValues.Added, entityChangeMessage.ChangeType);
            Assert.Same(entity, entityChangeMessage.Changed);
        }

        [Fact]
        public void MessageBus_notifies_of_modified_entity()
        {
            // ARRANGE

            var entity = Mock.Of<IEntity>();

            var entityObserver = this.mocks.Create<IObserver<ChangedMessage<IEntity>>>();
            ChangedMessage<IEntity> entityChangeMessage = null;
            entityObserver
                .Setup(o => o.OnNext(It.IsAny<ChangedMessage<IEntity>>()))
                .Callback<ChangedMessage<IEntity>>(m => entityChangeMessage = m);

            this.messageBus.Entities.Subscribe(entityObserver.Object);

            // ACT

            this.messageBus.Entities.Modified(entity);

            // ASSERT

            Assert.Equal(ChangeTypeValues.Modified, entityChangeMessage.ChangeType);
            Assert.Same(entity, entityChangeMessage.Changed);
        }

        [Fact]
        public void MessageBus_notifies_of_removed_entity()
        {
            // ARRANGE

            var entity = Mock.Of<IEntity>();

            var entityObserver = this.mocks.Create<IObserver<ChangedMessage<IEntity>>>();
            ChangedMessage<IEntity> entityChangeMessage = null;
            entityObserver
                .Setup(o => o.OnNext(It.IsAny<ChangedMessage<IEntity>>()))
                .Callback<ChangedMessage<IEntity>>(m => entityChangeMessage = m);

            this.messageBus.Entities.Subscribe(entityObserver.Object);

            // ACT

            this.messageBus.Entities.Removed(entity);

            // ASSERT

            Assert.Equal(ChangeTypeValues.Removed, entityChangeMessage.ChangeType);
            Assert.Same(entity, entityChangeMessage.Changed);
        }
    }
}