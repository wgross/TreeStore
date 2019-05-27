using Kosmograph.Messaging;
using Moq;
using System;

namespace Kosmograph.Model.Test
{
    public class ModelTestBase
    {
        protected MockRepository Mocks { get; } = new MockRepository(MockBehavior.Strict);

        protected KosmographMessageBus MessageBus { get; }

        protected Mock<IKosmographPersistence> Persistence { get; }

        public ModelTestBase()
        {
            this.MessageBus = new KosmographMessageBus();
            this.Persistence = this.Mocks.Create<IKosmographPersistence>();
        }

        protected T Setup<T>(T t, Action<T> setup = null)
        {
            setup?.Invoke(t);
            return t;
        }

        protected Tag DefaultTag(Action<Tag> setup = null) => Setup(new Tag("t", new Facet("f", new FacetProperty("p"))), setup);

        protected Entity DefaultEntity(Action<Entity> setup = null, params Tag[] tags) => Setup(new Entity("e", tags), setup);

        protected Entity DefaultEntity(Action<Entity> setup = null) => Setup(new Entity("e", DefaultTag()), setup);

        protected Relationship DefaultRelationship(Action<Relationship> setup = null, params Tag[] tags) => Setup(new Relationship("r", DefaultEntity(), DefaultEntity(), tags));
    }
}