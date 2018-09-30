using Kosmograph.Model;
using LiteDB;

namespace Kosmograph.LiteDb
{
    public class TagRepository : LiteDbRepositoryBase<Tag>, ITagRepository
    {
        public const string CollectionName = "tags";

        private readonly FacetRepository facets;

        public TagRepository(LiteRepository repo) : base(repo, CollectionName)
        {
            repo.Database
                .GetCollection(CollectionName)
                .EnsureIndex(field: nameof(Tag.Name), expression: $"LOWER($.{nameof(Tag.Name)})", unique: true);
        }
    }
}