using TreeStore.Messaging;
using System;

namespace TreeStore.Model
{
    public interface ITreeStorePersistence : IDisposable
    {
        ITreeStoreMessageBus MessageBus { get; }

        ITagRepository Tags { get; }

        ICategoryRepository Categories { get; }

        IEntityRepository Entities { get; }

        IRelationshipRepository Relationships { get; }

        bool DeleteCategory(Category category, bool recurse);
    }
}