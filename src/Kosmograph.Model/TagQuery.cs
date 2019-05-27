using Kosmograph.Messaging;
using System.Linq;

namespace Kosmograph.Model
{
    public class TagQuery : TrackingModelController
    {
        private readonly KosmographModel kosmographModel;

        public Tag Tag { get; }

        public TagQuery(KosmographModel kosmographModel, IKosmographMessageBus messageBus, Tag tag)
            : base(messageBus)
        {
            this.kosmographModel = kosmographModel;
            this.Tag = tag;
        }

        public void StartQuery()
        {
            this.kosmographModel
                .Entities
                .FindByTag(this.Tag)
                .ToList()
                .ForEach(e => this.AddEntity(e));

            this.kosmographModel
               .Relationships
               .FindByTag(this.Tag)
               .ToList()
               .ForEach(r => this.AddRelationship(r));
        }

        private void AddRelationship(Relationship added) => this.OnChangingRelationship(added);

        private void AddEntity(Entity entity) => this.OnChangingEntity(entity);

        protected override void OnAddedEntity(Entity entity)
        {
            if (entity.Tags.Contains(this.Tag))
                base.OnAddedEntity(entity);
        }

        protected override void OnChangingEntity(Entity changed)
        {
            if (changed.Tags.Contains(this.Tag))
                base.OnChangingEntity(changed);
            else
                RemoveEntity(changed);
        }

        private void RemoveEntity(Entity entity) => this.OnRemovingEntity(entity.Id);

        protected override void OnAddedRelationship(Relationship relationship)
        {
            if (relationship.Tags.Contains(this.Tag))
                base.OnAddedRelationship(relationship);
        }

        protected override void OnChangingRelationship(Relationship changed)
        {
            if (changed.Tags.Contains(this.Tag))
                base.OnChangingRelationship(changed);
            else
                this.RemoveRelationship(changed);
        }

        private void RemoveRelationship(Relationship changed) => this.OnRemovingRelationship(changed.Id);
    }
}