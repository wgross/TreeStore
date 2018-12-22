using Kosmograph.Model.Base;
using LiteDB;
using System;
using System.Collections.Generic;

namespace Kosmograph.LiteDb
{
    public abstract class LiteDbRepositoryBase<T> where T : NamedBase
    {
        private readonly string collectionName;

        protected LiteRepository Repository { get; }

        static LiteDbRepositoryBase()
        {
            BsonMapper.Global
                .Entity<T>().Id(v => v.Id);
        }

        public LiteDbRepositoryBase(LiteRepository repository, string collectionName)
        {
            this.Repository = repository;
            this.collectionName = collectionName;
        }

        public virtual T Upsert(T entity)
        {
            this.Repository.Upsert(entity, collectionName);
            return entity;
        }

        public virtual T FindById(Guid id) => this.Repository.SingleById<T>(id, collectionName);

        public virtual IEnumerable<T> FindAll() => this.Repository.Query<T>(collectionName).ToEnumerable();

        public virtual bool Delete(T entity) => this.Repository.Delete<T>(entity.Id, collectionName);
    }
}