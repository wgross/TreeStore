using Kosmograph.Messaging;
using Kosmograph.Model;
using LiteDB;
using System.IO;

namespace Kosmograph.LiteDb
{
    public class KosmographLiteDbPersistence : IKosmographPersistence
    {
        private readonly LiteRepository db;
        private readonly IKosmographMessageBus messageBus;

        public KosmographLiteDbPersistence(IKosmographMessageBus messageBus)
            : this(new MemoryStream())
        {
            this.messageBus = messageBus;
        }

        public KosmographLiteDbPersistence(Stream storageStream)
           : this(new LiteRepository(storageStream))
        {
        }

        public KosmographLiteDbPersistence(LiteRepository db)
        {
            this.db = db;
            this.Categories = new CategoryRepository(db);
        }

        public ITagRepository Tags => new TagRepository(db, messageBus.Tags);

        public ICategoryRepository Categories { get; private set; }

        public IEntityRepository Entities => new EntityRepository(db, this.messageBus.Entities);

        public IRelationshipRepository Relationships => new RelationshipRepository(db, this.messageBus.Relationships);

        public void Dispose()
        {
            this.db.Dispose();
        }
    }
}