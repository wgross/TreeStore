using Kosmograph.Messaging;
using Kosmograph.Model;
using LiteDB;
using System.IO;

namespace Kosmograph.LiteDb
{
    public class KosmographLiteDbPersistence : IKosmographPersistence
    {
        private LiteRepository db;

        public KosmographLiteDbPersistence(IKosmographMessageBus messageBus)
            : this(messageBus, new MemoryStream())
        {
            this.MessageBus = messageBus;
        }

        public KosmographLiteDbPersistence(IKosmographMessageBus messageBus, Stream storageStream)
           : this(messageBus, new LiteRepository(storageStream))
        {
        }

        private KosmographLiteDbPersistence(IKosmographMessageBus messageBus, LiteRepository db)
        {
            this.db = db;
            this.Categories = new CategoryRepository(db);
        }

        public IKosmographMessageBus MessageBus { get; }

        public ITagRepository Tags => new TagRepository(db, MessageBus.Tags);

        public ICategoryRepository Categories { get; private set; }

        public IEntityRepository Entities => new EntityRepository(db, this.MessageBus.Entities);

        public IRelationshipRepository Relationships => new RelationshipRepository(db, this.MessageBus.Relationships);

        public void Dispose()
        {
            this.db?.Dispose();
            this.db = null;
        }
    }
}