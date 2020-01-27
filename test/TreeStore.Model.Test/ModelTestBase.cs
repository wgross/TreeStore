﻿using TreeStore.Messaging;
using Moq;
using System;

namespace TreeStore.Model.Test
{
    public class ModelTestBase : IDisposable
    {
        protected MockRepository Mocks { get; } = new MockRepository(MockBehavior.Strict);

        protected TreeStoreMessageBus MessageBus { get; }

        protected Mock<ITreeStorePersistence> Persistence { get; }

        public ModelTestBase()
        {
            this.MessageBus = new TreeStoreMessageBus();
            this.Persistence = this.Mocks.Create<ITreeStorePersistence>();
        }

        public void Dispose() => this.Mocks.VerifyAll();

        protected T Setup<T>(T t, Action<T> setup = null)
        {
            setup?.Invoke(t);
            return t;
        }

        protected TreeStoreModel NewModel()
        {
            this.Persistence.Setup(p => p.MessageBus).Returns(this.MessageBus);
            return new TreeStoreModel(this.Persistence.Object);
        }

        protected Tag DefaultTag(Action<Tag> setup = null) => Setup(new Tag("t", new Facet("f", new FacetProperty("p"))), setup);

        protected Entity DefaultEntity(Action<Entity> setup = null, params Tag[] tags) => Setup(new Entity("e", tags), setup);

        protected Entity DefaultEntity(Action<Entity> setup = null) => Setup(new Entity("e", DefaultTag()), setup);

        protected Relationship DefaultRelationship(Action<Relationship> setup = null) => DefaultRelationship(DefaultEntity(), DefaultEntity(), setup);

        protected Relationship DefaultRelationship(Entity from, Entity to, Action<Relationship> setup = null) => Setup(new Relationship("r", from, to), setup);
    }
}