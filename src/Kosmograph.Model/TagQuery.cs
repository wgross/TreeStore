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
            if (this.ContainsEntity(changed.Id))
            {
                if (!changed.Tags.Contains(this.Tag))
                {
                    // its a removal only if the entity is known and no longer contains the tag
                    RemoveEntity(changed);
                }
            }
            else if (changed.Tags.Contains(this.Tag))
            {
                // a yet unkonw entity having the query tags is
                // considered an "add" and is propagted to the tracking controller
                base.OnChangingEntity(changed);
            }
        }

        private void RemoveEntity(Entity entity) => this.OnRemovingEntity(entity.Id);

        protected override void OnAddedRelationship(Relationship relationship)
        {
            if (relationship.Tags.Contains(this.Tag))
                base.OnAddedRelationship(relationship);
        }

        protected override void OnChangingRelationship(Relationship changed)
        {
            if (ContainsRelationship(changed.Id))
            {
                if (!changed.Tags.Contains(this.Tag))
                {
                    // its a removal only if the relationship is known and no longer contains the tag
                    this.RemoveRelationship(changed);
                }
            }
            else if (changed.Tags.Contains(this.Tag))
            {
                // a yet unkonw relationship having the query tags is
                // considered an "add" and is propagted to the tracking controller
                base.OnChangingRelationship(changed);
            }
        }

        private void RemoveRelationship(Relationship changed) => this.OnRemovingRelationship(changed.Id);
    }
}