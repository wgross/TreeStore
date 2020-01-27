namespace TreeStore.Model
{
    public interface ITagRepository : IRepository<Tag>
    {
        Tag? FindByName(string name);
    }
}