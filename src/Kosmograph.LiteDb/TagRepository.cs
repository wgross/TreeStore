using Kosmograph.Model;
using LiteDB;

namespace Kosmograph.LiteDb
{
    public class TagRepository : LiteDbRepositoryBase<Tag>
    {
        private readonly FacetRepository facets;

        public TagRepository(LiteRepository db) : base(db, "tags")
        {
        }

        public override void Upsert(Tag entity)
        {
            base.Upsert(entity);
        }
    }
}