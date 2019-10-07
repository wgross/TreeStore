using Kosmograph.Model;

namespace PSKosmograph.Service
{
    public class KosmographItem
    {
        public KosmographItem(Tag tag)
        {
            this.Name = tag.Name;
        }

        public string Name { get; }

        public bool IsContainer => false;
    }
}