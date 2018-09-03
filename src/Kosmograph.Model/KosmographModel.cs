namespace Kosmograph.Model
{
    public class KosmographModel
    {
        private IKosmographPersistence persistence;

        public KosmographModel(IKosmographPersistence persistence)
        {
            this.persistence = persistence;
        }

        public Category RootCategory() => this.persistence.Categories.Root();
    }
}