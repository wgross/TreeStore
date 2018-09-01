using Kosmograph.Model;
using LiteDB;

namespace Kosmograph.LiteDb
{
    public abstract class LiteDbRepositoryBase<T> where T : EntityBase
    {
        private readonly LiteDatabase db;
        private readonly LiteCollection<T> collection;

        static LiteDbRepositoryBase()
        {
            BsonMapper.Global
                .Entity<T>().Id(v => v.Id);
        }

        public LiteDbRepositoryBase(LiteDatabase db, string collectionName)
        {
            this.db = db;
            this.collection = this.db.GetCollection<T>(collectionName);
        }

        public void Upsert(T entity) => this.collection.Upsert(entity);

        public T FindById(BsonValue id) => this.collection.FindById(id);

        public object Delete(BsonValue id) => this.collection.Delete(id);
    }
}