using TreeStore.Model.Base;
using System;
using System.Collections.Generic;

namespace TreeStore.Model
{
    public interface IRepository<T> where T : NamedBase
    {
        T FindById(Guid id);

        IEnumerable<T> FindAll();

        T Upsert(T instance);

        bool Delete(T instance);
    }
}