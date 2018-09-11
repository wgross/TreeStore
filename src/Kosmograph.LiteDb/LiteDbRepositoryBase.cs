using Kosmograph.Model.Base;
using LiteDB;
using System;
using System.Collections.Generic;

namespace Kosmograph.LiteDb
{
    public abstract class LiteDbRepositoryBase<T> where T : EntityBase
    {
        protected readonly LiteRepository repository;
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

        public virtual T Upsert(T entity)
        {
            this.repository.Upsert(entity, collectionName);
            return entity;
        }

        public virtual T FindById(Guid id) => this.repository.SingleById<T>(id, collectionName);

        public virtual IEnumerable<T> FindAll() => this.repository.Query<T>(collectionName).ToEnumerable();

        public bool Delete(Guid id) => this.repository.Delete<T>(id, collectionName);
    }
}