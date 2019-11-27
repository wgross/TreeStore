using Kosmograph.Messaging;
using Kosmograph.Model;
using LiteDB;
using System.IO;
using FileMode = System.IO.FileMode;

namespace Kosmograph.LiteDb
{
    public class KosmographLiteDbPersistence : IKosmographPersistence
    {
        private LiteRepository db;

        public static KosmographLiteDbPersistence InMemory(IKosmographMessageBus messageBus) => new KosmographLiteDbPersistence(messageBus);

        private KosmographLiteDbPersistence(IKosmographMessageBus messageBus)
            : this(messageBus, new MemoryStream())
        {
            this.MessageBus = messageBus;
        }

        public static KosmographLiteDbPersistence InFile(IKosmographMessageBus messageBus, string path) => new KosmographLiteDbPersistence(messageBus, File.Open(path, FileMode.OpenOrCreate));

        private KosmographLiteDbPersistence(IKosmographMessageBus messageBus, Stream storageStream)
           : this(messageBus, new LiteRepository(storageStream))
        {
        }

        private KosmographLiteDbPersistence(IKosmographMessageBus messageBus, LiteRepository db)
        {
            this.db = db;
            this.Categories = new CategoryRepository(db);
            this.MessageBus = messageBus;
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