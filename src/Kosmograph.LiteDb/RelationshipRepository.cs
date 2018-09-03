﻿using Kosmograph.Model;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmograph.LiteDb
{
    public class RelationshipRepository : LiteDbRepositoryBase<Relationship>
    {
        public RelationshipRepository(LiteRepository db) : base(db, "relationships")
        {
        }
    }
}