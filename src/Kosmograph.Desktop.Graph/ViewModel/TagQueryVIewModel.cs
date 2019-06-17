using Kosmograph.Model;

namespace Kosmograph.Desktop.Graph.ViewModel
{
    public class TagQueryViewModel
    {
        public TagQuery TagQuery { get; }

        public TagQueryViewModel(TagQuery tagQuery)
        {
            this.TagQuery = tagQuery;
        }

        public string Name => this.TagQuery.Tag.Name;
    }
}