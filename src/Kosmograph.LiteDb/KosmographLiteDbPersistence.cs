using Kosmograph.Model;
using LiteDB;
using System.IO;

namespace Kosmograph.LiteDb
{
    public class KosmographLiteDbPersistence : IKosmographPersistence
    {
        private readonly LiteRepository db;

        public KosmographLiteDbPersistence()
            : this(new MemoryStream())
        {
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

        public ITagRepository Tags => new TagRepository(db);

        public ICategoryRepository Categories { get; private set; }

        public IEntityRepository Entities => new EntityRepository(db);

        public IRelationshipRepository Relationships => new RelationshipRepository(db);
    }
}