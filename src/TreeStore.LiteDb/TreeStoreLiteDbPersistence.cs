using LiteDB;
using System.IO;
using TreeStore.Messaging;
using TreeStore.Model;

namespace TreeStore.LiteDb
{
    public class TreeStoreLiteDbPersistence : ITreeStorePersistence
    {
        private LiteRepository db;

        #region Create in Memory Storage

        public static TreeStoreLiteDbPersistence InMemory(ITreeStoreMessageBus messageBus) => new TreeStoreLiteDbPersistence(messageBus);

        private TreeStoreLiteDbPersistence(ITreeStoreMessageBus messageBus)
            : this(messageBus, new MemoryStream())
        {
            this.MessageBus = messageBus;
        }

        private TreeStoreLiteDbPersistence(ITreeStoreMessageBus messageBus, Stream storageStream)
           : this(messageBus, new LiteRepository(storageStream))
        {
        }

        #endregion Create in Memory Storage

        #region Create File based Storage

        public static TreeStoreLiteDbPersistence InFile(ITreeStoreMessageBus messageBus, string connectionString)
            => new TreeStoreLiteDbPersistence(messageBus, new LiteRepository(connectionString));

        #endregion Create File based Storage

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
            var traverser = new CategoryRemovalTraverser((CategoryRepository)this.Categories, (EntityRepository)this.Entities);

            if (recurse)
                return traverser.DeleteRecursively(category);

            return traverser.DeleteIfEmpty(category);
        }

        public void CopyCategory(Category source, Category destination, bool recurse)
        {
            var traverser = new CategoryCopyTraverser((CategoryRepository)this.Categories, (EntityRepository)this.Entities);

            if (recurse)
                traverser.CopyCategoryRecursively(source, destination);
            else
                traverser.CopyCategory(source, destination);
        }

        public void Dispose()
        {
            this.db?.Dispose();
            this.db = null;
        }
    }
}