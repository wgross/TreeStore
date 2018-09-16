using System;
using System.Collections.Generic;

namespace Kosmograph.Model
{
    public interface IRelationshipRepository
    {
        Relationship FindById(Guid id);

        IEnumerable<Relationship> FindAll();

        Relationship Upsert(Relationship tag);

        bool Delete(Guid id);
    }
}