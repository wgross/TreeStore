using TreeStore.Model.Base;
using LiteDB;
using System;
using System.Collections.Generic;

namespace TreeStore.LiteDb
{
    public abstract class LiteDbRepositoryBase
    {
        public string CollectionName { get; }

        /// <summary>
        /// Provides low oleve access to underlying the lite db.
        /// </summary>
        public LiteRepository LiteRepository { get; }

        public LiteCollection<BsonDocument> LiteCollection() => this.LiteRepository.Database.GetCollection(this.CollectionName);

        public LiteCollection<T> LiteCollection<T>() => this.LiteRepository.Database.GetCollection<T>(this.CollectionName);

        protected LiteDbRepositoryBase(LiteRepository repository, string collectionName)
        {
            this.LiteRepository = repository;
            this.CollectionName = collectionName;
        }
    }

    public abstract class LiteDbRepositoryBase<T> : LiteDbRepositoryBase
        where T : NamedBase
    {
        static LiteDbRepositoryBase()
        {
            // mapping from liteDb _id property is always to public Id property
            BsonMapper
                .Global
                .Entity<T>().Id(v => v.Id);
        }

        public LiteDbRepositoryBase(LiteRepository repository, string collectionName)
            : base(repository, collectionName)
        { }

        public virtual T Upsert(T entity)
        {
            this.LiteRepository.Upsert(entity, CollectionName);
            return entity;
        }

        public virtual T FindById(Guid id) => this.LiteRepository.SingleById<T>(id, this.CollectionName);

        public virtual IEnumerable<T> FindAll() => this.LiteRepository.Query<T>(this.CollectionName).ToEnumerable();

        public virtual bool Delete(T entity) => this.LiteRepository.Delete<T>(entity.Id, this.CollectionName);
    }
}