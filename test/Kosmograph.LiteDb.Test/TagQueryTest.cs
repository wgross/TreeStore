using Kosmograph.Model;
using System;
using System.Linq;
using Xunit;

namespace Kosmograph.LiteDb.Test
{
    public class TagQueryTest : LiteDbTestBase
    {
        private readonly Tag tag;
        private readonly TagQuery tagQuery;

        private class EntityObserver : IObserver<Entity>
        {
            private readonly IDisposable entities;
            private readonly Action<Entity> onNext;

            public EntityObserver(IObservable<Entity> entities, Action<Entity> onNext)
            {
                this.entities = entities.Subscribe(this);
                this.onNext = onNext;
            }

            void IObserver<Entity>.OnCompleted() => throw new NotImplementedException();

            void IObserver<Entity>.OnError(Exception error) => throw new NotImplementedException();

            void IObserver<Entity>.OnNext(Entity value) => this.onNext(value);
        }

        public TagQueryTest()
        {
            this.tag = this.Persistence.Tags.Upsert(DefaultTag(t => t.Name = "query"));
            this.tagQuery = new TagQuery(new KosmographModel(this.Persistence), this.MessageBus, tag);
        }

        private Tag DefaultTag(Action<Tag> setup = null) => Setup(new Tag("t", new Facet("f", new FacetProperty("p"))), setup);

        private Entity DefaultEntity(Action<Entity> setup = null, params Tag[] tags) => Setup(new Entity("e", tags), setup);

        private Entity DefaultEntity(Action<Entity> setup = null) => Setup(new Entity("e", DefaultTag()), setup);

        private Relationship DefaultRelationship(Action<Relationship> setup = null, params Tag[] tags) => Setup(new Relationship("r", DefaultEntity(), DefaultEntity(), tags));

        [Fact]
        public void TagQuery_retrieves_all_matching_entities()
        {
            // ARRANGE

            var tag2 = this.Persistence.Tags.Upsert(DefaultTag(t => t.Name = "tag2"));
            var entity1 = this.Persistence.Entities.Upsert(DefaultEntity(tags: this.tag));
            var entity2 = this.Persistence.Entities.Upsert(DefaultEntity(tags: tag2));

            // ACT

            var result = this.tagQuery.GetEntities().ToArray();

            // ASSERT

            Assert.Equal(entity1, result.Single());
        }

        [Fact]
        public void TagQuery_retrieves_all_matching_relationships()
        {
            // ARRANGE

            var relationship1 = DefaultRelationship(tags: this.tag);
            var entity1 = this.Persistence.Entities.Upsert(relationship1.From);
            var entity2 = this.Persistence.Entities.Upsert(relationship1.To);
            this.Persistence.Relationships.Upsert(relationship1);

            var relationship2 = DefaultRelationship(tags: DefaultTag());
            var entity3 = this.Persistence.Entities.Upsert(relationship2.From);
            var entity4 = this.Persistence.Entities.Upsert(relationship2.To);
            this.Persistence.Relationships.Upsert(relationship2);

            // ACT

            var result = this.tagQuery.GetRelationships().ToArray();

            // ASSERT

            Assert.Equal(relationship1, result.Single());
        }

        [Fact]
        public void TagQuery_adds_newly_tagged_entity()
        {
            // ACT

            Entity result = null;
            this.tagQuery.Added = e => result = e;
            var entity1 = this.Persistence.Entities.Upsert(DefaultEntity(tags: this.tag));

            // ASSERT

            Assert.Equal(entity1, result);
        }

        [Fact]
        public void TagQuery_removes_untagged_entity()
        {
            // ARRANGE

            var entity1 = this.Persistence.Entities.Upsert(DefaultEntity(tags: this.tag));

            // ACT

            Guid result = Guid.Empty;
            this.tagQuery.Removed = e => result = e;

            bool addWasCalled = false;
            this.tagQuery.Added = e => addWasCalled = true;

            entity1.Tags.Clear();
            this.Persistence.Entities.Upsert(entity1);

            // ASSERT

            Assert.Equal(entity1.Id, result);
            Assert.False(addWasCalled);
        }

        [Fact]
        public void TagQuery_removes_deleted_entity()
        {
            // ARRANGE

            var entity1 = this.Persistence.Entities.Upsert(DefaultEntity(tags: this.tag));

            // ACT

            Guid result = Guid.Empty;
            this.tagQuery.Removed = e => result = e;

            bool addWasCalled = false;
            this.tagQuery.Added = e => addWasCalled = true;

            entity1.Tags.Clear();
            this.Persistence.Entities.Delete(entity1);

            // ASSERT

            Assert.Equal(entity1.Id, result);
            Assert.False(addWasCalled);
        }

        [Fact]
        public void TagQuery_doesnt_notify_add_for_known_entity()
        {
            // ARRANGE

            var entity1 = this.Persistence.Entities.Upsert(DefaultEntity(tags: this.tag));
            var snapshot = this.tagQuery.GetEntities();

            // ACT

            bool result = false;
            this.tagQuery.Added = e => result = true;
            this.Persistence.Entities.Upsert(DefaultEntity(tags: this.tag));

            // ASSERT

            Assert.False(result);
        }

        [Fact]
        public void TagQuery_knows_entity_after_notification()
        {
            // ACT

            int result = 0;
            this.tagQuery.Added = e => result++;
            var entity1 = this.Persistence.Entities.Upsert(DefaultEntity(tags: this.tag));
            this.Persistence.Entities.Upsert(entity1);

            // ASSERT

            Assert.Equal(1, result);
        }

        [Fact]
        public void TagQuery_doesnt_notify_delete_for_unknown_entity()
        {
            // ARRANGE

            var entity1 = this.Persistence.Entities.Upsert(DefaultEntity(tags: this.tag));
            var snapshot = this.tagQuery.GetEntities();
            var entity2 = this.Persistence.Entities.Upsert(DefaultEntity(tags: this.tag));

            // ACT

            bool result = false;
            this.tagQuery.Removed = e => result = true;

            this.Persistence.Entities.Delete(entity2);

            // ASSERT

            Assert.False(result);
        }
    }
}