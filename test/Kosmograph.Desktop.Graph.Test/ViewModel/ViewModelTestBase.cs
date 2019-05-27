using Kosmograph.Messaging;
using Kosmograph.Model;
using Moq;
using System;

namespace Kosmograph.Desktop.Graph.Test.ViewModel
{
    public class ViewModelTestBase
    {
        protected MockRepository Mocks { get; } = new MockRepository(MockBehavior.Strict);

        protected KosmographMessageBus MessageBus { get; }

        protected Mock<IKosmographPersistence> Persistence { get; }

        public ViewModelTestBase()
        {
            this.MessageBus = new KosmographMessageBus();
            this.Persistence = this.Mocks.Create<IKosmographPersistence>();
        }

        public void Dispose()
        {
            this.Mocks.VerifyAll();
        }

        protected T Setup<T>(T t, Action<T> setup = null)
        {
            setup?.Invoke(t);
            return t;
        }

        protected Tag DefaultTag(Action<Tag> setup = null) => Setup(new Tag("t", new Facet("f", new FacetProperty("p"))), setup);

        protected Entity DefaultEntity(Action<Entity> setup = null, params Tag[] tags) => Setup(new Entity("e", tags), setup);

        protected Entity DefaultEntity(Action<Entity> setup = null) => Setup(new Entity("e", DefaultTag()), setup);

        protected Relationship DefaultRelationship(Entity from, Entity to, Action<Relationship> setup = null, params Tag[] tags) => Setup(new Relationship("r", from, to, tags), setup);

        protected Relationship DefaultRelationship(Action<Relationship> setup = null, params Tag[] tags) => DefaultRelationship(DefaultEntity(), DefaultEntity(), setup, tags);
    }
}