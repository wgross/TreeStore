using Kosmograph.Messaging;
using System;

namespace Kosmograph.Model
{
    public interface IKosmographPersistence : IDisposable
    {
        IKosmographMessageBus MessageBus { get; }

        ITagRepository Tags { get; }

        ICategoryRepository Categories { get; }

        IEntityRepository Entities { get; }

        IRelationshipRepository Relationships { get; }

        bool DeleteCategory(Category category, bool recurse);
    }
}