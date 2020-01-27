using System;
using System.Collections.Generic;

namespace TreeStore.Model
{
    public interface ICategoryRepository
    {
        Category Root();

        Category FindById(Guid id);

        Category Upsert(Category entity);

        Category? FindByCategoryAndName(Category category, string name);

        IEnumerable<Category> FindByCategory(Category category);
    }
}