using Moq;
using System;
using Xunit;

namespace TreeStore.Messaging.Test
{
    public class TagMessagesTest : IDisposable
    {
        private readonly TreeStoreMessageBus messageBus = new TreeStoreMessageBus();
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);

        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        [Fact]
        public void MessageBus_notifies_of_modified_tag()
        {
            // ARRANGE

            var tag = Mock.Of<ITag>();

            var tagObserver = this.mocks.Create<IObserver<ChangedMessage<ITag>>>();
            ChangedMessage<ITag> tagChangeMessage = null;
            tagObserver
                .Setup(o => o.OnNext(It.IsAny<ChangedMessage<ITag>>()))
                .Callback<ChangedMessage<ITag>>(m => tagChangeMessage = m);

            this.messageBus.Tags.Subscribe(tagObserver.Object);

            // ACT

            this.messageBus.Tags.Modified(tag);

            // ASSERT

            Assert.Equal(ChangeTypeValues.Modified, tagChangeMessage.ChangeType);
            Assert.Same(tag, tagChangeMessage.Changed);
        }

        [Fact]
        public void MessageBus_notifies_of_removed_tag()
        {
            // ARRANGE

            var tag = Mock.Of<ITag>();

            var tagObserver = this.mocks.Create<IObserver<ChangedMessage<ITag>>>();
            ChangedMessage<ITag> tagChangeMessage = null;
            tagObserver
                .Setup(o => o.OnNext(It.IsAny<ChangedMessage<ITag>>()))
                .Callback<ChangedMessage<ITag>>(m => tagChangeMessage = m);

            this.messageBus.Tags.Subscribe(tagObserver.Object);

            // ACT

            this.messageBus.Tags.Removed(tag);

            // ASSERT

            Assert.Equal(ChangeTypeValues.Removed, tagChangeMessage.ChangeType);
            Assert.Same(tag, tagChangeMessage.Changed);
        }
    }
}