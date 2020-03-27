using LiteDB;
using System;
using TreeStore.Model;

namespace TreeStore.LiteDb
{
    public class FacetRepository : LiteDbRepositoryBase<Facet>
    {
        public FacetRepository(LiteRepository db) : base(db, "facets")
        {
        }

        protected override ILiteCollection<Facet> IncludeRelated(ILiteCollection<Facet> from)
        {
            throw new NotImplementedException();
        }
    }
}