using LiteDB;

namespace TreeStore.LiteDb
{
    public sealed class LiteRepositoryAdapater
    {
        public LiteRepositoryAdapater(LiteRepository liteDb)
        {
            this.LiteRepository = liteDb;
            this.Entities = this.LiteRepository.Database.GetCollection("entities");
            this.Tags = this.LiteRepository.Database.GetCollection("tags");
            this.Categories = this.LiteRepository.Database.GetCollection("categories");
        }

        public LiteRepository LiteRepository { get; }

        public LiteCollection<BsonDocument> Entities { get; }

        public LiteCollection<BsonDocument> Tags { get; }

        public LiteCollection<BsonDocument> Categories { get; }
    }
}