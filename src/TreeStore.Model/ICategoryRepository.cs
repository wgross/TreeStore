using System;
using System.Collections.Generic;

namespace TreeStore.Model
{
    public interface ICategoryRepository
    {
        Category Root();

        Category FindById(Guid id);

        Category Upsert(Category entity);

        Category? FindByParentAndName(Category category, string name);

        IEnumerable<Category> FindByParent(Category category);
    }
}