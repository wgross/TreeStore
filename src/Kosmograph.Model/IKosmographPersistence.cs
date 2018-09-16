namespace Kosmograph.Model
{
    public interface IKosmographPersistence
    {
        ITagRepository Tags { get; }

        ICategoryRepository Categories { get; }

        IEntityRepository Entities { get; }

        IRelationshipRepository Relationships { get; }
    }
}