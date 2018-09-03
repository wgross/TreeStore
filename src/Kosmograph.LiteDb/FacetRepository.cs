using Kosmograph.Model;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmograph.LiteDb
{
    public class FacetRepository : LiteDbRepositoryBase<Facet>
    {
        public FacetRepository(LiteRepository db) : base(db, "facets")
        {
        }
    }
}
