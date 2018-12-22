using Kosmograph.Messaging;
using Kosmograph.Model;
using Moq;
using System;
using Xunit;

namespace Kosmograph.LiteDb.Test
{
    public class KosmographLiteDbPersistenceTest : IDisposable
    {
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);
        private readonly Mock<IKosmographMessageBus> kosmographMessaging;
        private readonly KosmographLiteDbPersistence kosmographPersistence;

        public KosmographLiteDbPersistenceTest()
        {
            this.kosmographMessaging = this.mocks.Create<IKosmographMessageBus>();
            this.kosmographPersistence = new KosmographLiteDbPersistence(this.kosmographMessaging.Object);
        }

        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        [Fact]
        public void KosmographLiteDbPersistence_removes_entity_with_relationship()
        {
            // ARRANGE

            var entityMessaging = this.mocks.Create<IChangedMessageBus<IEntity>>();
            entityMessaging
                .Setup(e => e.Modified(It.IsAny<IEntity>()));

            this.kosmographMessaging
                .Setup(m => m.Entities)
                .Returns(entityMessaging.Object);

            var relationshipMessaging = this.mocks.Create<IChangedMessageBus<IRelationship>>();
            relationshipMessaging
                .Setup(e => e.Modified(It.IsAny<IRelationship>()));

            this.kosmographMessaging
                .Setup(m => m.Relationships)
                .Returns(relationshipMessaging.Object);

            var entity1 = this.kosmographPersistence.Entities.Upsert(new Entity());
            var entity2 = this.kosmographPersistence.Entities.Upsert(new Entity()); ;
            var relationship = this.kosmographPersistence.Relationships.Upsert(new Relationship("r", entity1, entity2));

            // ACT

            entityMessaging
                .Setup(m => m.Removed(entity1));
            relationshipMessaging
                .Setup(m => m.Removed(relationship));

            this.kosmographPersistence.RemoveWithRelationship(entity1);
        }
    }
}