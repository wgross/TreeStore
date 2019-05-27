using Kosmograph.Model;

namespace Kosmograph.Desktop.Graph.ViewModel
{
    public class TagQueryViewModel
    {
        private readonly TagQuery tagQuery;

        public TagQueryViewModel(TagQuery tagQuery)
        {
            this.tagQuery = tagQuery;
        }

        public string Name => this.tagQuery.Tag.Name;
    }
}