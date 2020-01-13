using System.Collections.Generic;

namespace Kosmograph.Model
{
    public interface IEntityRepository : IRepository<Entity>
    {
        IEnumerable<Entity> FindByTag(Tag tag);

        IEnumerable<Entity> FindByCategory(Category category);

        Entity? FindByName(string name);

        Entity? FindByCategoryAndName(Category category, string name);
    }
}