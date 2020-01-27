using TreeStore.Model;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace TreeStore.LiteDb
{
    public class FacetRepository : LiteDbRepositoryBase<Facet>
    {
        public FacetRepository(LiteRepository db) : base(db, "facets")
        {
        }
    }
}
