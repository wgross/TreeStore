using Kosmograph.Model;
using Moq;
using System;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public abstract class KosmographViewModelTestBase : IDisposable
    {
        protected readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);

        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        protected Tag DefaultTag(Action<Tag> setup = null) => Setup(new Tag("tag", new Facet("facet", new FacetProperty("p"))), setup);

        protected Entity DefaultEntity(Action<Entity> setup = null) => Setup(new Entity("entity", DefaultTag()), setup);

        protected Relationship DefaultRelationship(Action<Relationship> setup = null)
            => Setup(new Relationship("relationship", DefaultEntity(e => e.Name = "e1"), DefaultEntity(e => e.Name = "e2")));

        protected T Setup<T>(T t, Action<T> setup = null)
        {
            setup?.Invoke(t);
            return t;
        }
    }
}