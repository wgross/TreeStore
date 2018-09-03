using System;

namespace Kosmograph.Model
{
    public interface ICategoryRepository
    {
        Category Root();

        Category FindById(Guid id);

        Category Upsert(Category entity);

        bool Delete(Guid id);
    }
}