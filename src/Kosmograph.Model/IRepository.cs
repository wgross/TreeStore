using Kosmograph.Model.Base;
using System;
using System.Collections.Generic;

namespace Kosmograph.Model
{
    public interface IRepository<T> where T : NamedItemBase
    {
        T FindById(Guid id);

        IEnumerable<T> FindAll();

        T Upsert(T tag);

        bool Delete(Guid id);
    }
}