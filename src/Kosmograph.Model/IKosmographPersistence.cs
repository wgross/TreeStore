using System;

namespace Kosmograph.Model
{
    public interface IKosmographPersistence : IDisposable
    {
        ITagRepository Tags { get; }

        ICategoryRepository Categories { get; }

        IEntityRepository Entities { get; }

        IRelationshipRepository Relationships { get; }
    }
}