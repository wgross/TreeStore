using Kosmograph.Messaging;
using System;

namespace Kosmograph.Model
{
    public class KosmographModel : IDisposable
    {
        private IKosmographPersistence persistence;

        public KosmographModel(IKosmographPersistence persistence)
        {
            this.persistence = persistence;
        }

        public IKosmographMessageBus MessageBus => this.persistence.MessageBus;

        public ITagRepository Tags => this.persistence.Tags;

        public IEntityRepository Entities => this.persistence.Entities;

        public IRelationshipRepository Relationships => this.persistence.Relationships;

        public Category RootCategory() => this.persistence.Categories.Root();

        public void Dispose()
        {
            this.persistence?.Dispose();
            this.persistence = null;
        }
    }
}