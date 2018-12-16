using Kosmograph.Messaging;
using Moq;

namespace Kosmograph.LiteDb.Test
{
    public class RepositoryTestBase
    {
        protected readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);
        private readonly Mock<IKosmographMessageBus> messageBus;
        protected readonly KosmographLiteDbPersistence persistence;

        public RepositoryTestBase()
        {
            this.messageBus = this.mocks.Create<IKosmographMessageBus>();
            this.persistence = new KosmographLiteDbPersistence(this.messageBus.Object);
        }
    }
}