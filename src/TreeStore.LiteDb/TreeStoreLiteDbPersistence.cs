using TreeStore.Messaging;
using TreeStore.Model;
using LiteDB;
using System.IO;
using FileMode = System.IO.FileMode;

namespace TreeStore.LiteDb
{
    public class TreeStoreLiteDbPersistence : ITreeStorePersistence
    {
        private LiteRepository db;

        public static TreeStoreLiteDbPersistence InMemory(ITreeStoreMessageBus messageBus) => new TreeStoreLiteDbPersistence(messageBus);

        private TreeStoreLiteDbPersistence(ITreeStoreMessageBus messageBus)
            : this(messageBus, new MemoryStream())
        {
            this.MessageBus = messageBus;
        }

        public static TreeStoreLiteDbPersistence InFile(ITreeStoreMessageBus messageBus, string path) => new TreeStoreLiteDbPersistence(messageBus, File.Open(path, FileMode.OpenOrCreate));

        private TreeStoreLiteDbPersistence(ITreeStoreMessageBus messageBus, Stream storageStream)
           : this(messageBus, new LiteRepository(storageStream))
        {
        }

        private TreeStoreLiteDbPersistence(ITreeStoreMessageBus messageBus, LiteRepository db)
        {
            this.db = db;
            this.Categories = new CategoryRepository(db);
            this.MessageBus = messageBus;
        }

        public ITreeStoreMessageBus MessageBus { get; }

        public ITagRepository Tags => new TagRepository(db, MessageBus.Tags);

        public ICategoryRepository Categories { get; private set; }

        public IEntityRepository Entities => new EntityRepository(db, this.MessageBus.Entities);

        public IRelationshipRepository Relationships => new RelationshipRepository(db, this.MessageBus.Relationships);

        public bool DeleteCategory(Category category, bool recurse)
        {
            return true;
        }

        public void Dispose()
        {
            this.db?.Dispose();
            this.db = null;
        }
    }
}