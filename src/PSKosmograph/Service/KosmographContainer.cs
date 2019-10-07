namespace PSKosmograph.Service
{
    public sealed class KosmographContainer
    {
        public KosmographContainer(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public bool IsContainer => true;
    }
}