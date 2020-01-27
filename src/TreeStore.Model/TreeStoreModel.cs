using TreeStore.Messaging;
using System;

namespace TreeStore.Model
{
    public class TreeStoreModel : IDisposable
    {
        private ITreeStorePersistence? persistence;

        public TreeStoreModel(ITreeStorePersistence persistence)
        {
            this.persistence = persistence;
        }

        public ITreeStoreMessageBus MessageBus => this.persistence!.MessageBus;

        public ITagRepository Tags => this.persistence!.Tags;

        public IEntityRepository Entities => this.persistence!.Entities;

        public IRelationshipRepository Relationships => this.persistence!.Relationships;

        public Category RootCategory() => this.persistence!.Categories.Root();
        
        public void Dispose()
        {
            this.persistence?.Dispose();
            this.persistence = null;
        }
    }
}