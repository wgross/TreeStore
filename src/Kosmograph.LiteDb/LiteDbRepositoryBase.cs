using Kosmograph.Model.Base;
using LiteDB;

namespace Kosmograph.LiteDb
{
    public abstract class LiteDbRepositoryBase<T> where T : EntityBase
    {
        private readonly LiteRepository repository;
        private readonly string collectionName;

        static LiteDbRepositoryBase()
        {
            BsonMapper.Global
                .Entity<T>().Id(v => v.Id);
        }

        public LiteDbRepositoryBase(LiteRepository repository, string collectionName)
        {
            this.repository = repository;
            this.collectionName = collectionName;
        }

        public virtual void Upsert(T entity) => this.repository.Upsert(entity, collectionName);

        public T FindById(BsonValue id) => this.repository.SingleById<T>(id, collectionName);

        public object Delete(BsonValue id) => this.repository.Delete<T>(id, collectionName);
    }
}