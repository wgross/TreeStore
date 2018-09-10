using Kosmograph.Model;
using LiteDB;

namespace Kosmograph.LiteDb
{
    public class TagRepository : LiteDbRepositoryBase<Tag>, ITagRepository
    {
        public const string CollectionName = "tags";

        private readonly FacetRepository facets;

        public TagRepository(LiteRepository db) : base(db, CollectionName)
        {

        }
    }
}