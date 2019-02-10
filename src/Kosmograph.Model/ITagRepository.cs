namespace Kosmograph.Model
{
    public interface ITagRepository : IRepository<Tag>
    {
        Tag FindByName(string name);
    }
}