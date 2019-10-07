using Kosmograph.Model;
using Moq;
using System;

namespace PSKosmograph.Test.PathNodes
{
    public abstract class NodeTestBase : IDisposable
    {
        protected MockRepository Mocks { get; } = new MockRepository(MockBehavior.Strict);

        protected Mock<IKosmographProviderContext> ProviderContextMock { get; }

        protected Mock<IKosmographProviderService> ProviderServiceMock { get; }

        protected Mock<IKosmographPersistence> PersistenceMock { get; }

        protected Mock<IEntityRepository> EntityRepositoryMock { get; }

        protected Mock<ITagRepository> TagRepositoryMock { get; }

        public NodeTestBase()
        {
            this.ProviderContextMock = this.Mocks.Create<IKosmographProviderContext>();
            this.ProviderServiceMock = this.Mocks.Create<IKosmographProviderService>();
            this.PersistenceMock = this.Mocks.Create<IKosmographPersistence>();
            this.EntityRepositoryMock = this.Mocks.Create<IEntityRepository>();
            this.TagRepositoryMock = this.Mocks.Create<ITagRepository>();
        }

        public void Dispose() => this.Mocks.VerifyAll();

        protected Tag DefaultTag(Action<Tag>? setup = null)
        {
            var tmp = new Tag("t", new Facet("f", new FacetProperty("p")));
            setup?.Invoke(tmp);
            return tmp;
        }

        protected Entity DefaultEntity(Action<Entity>? setup = null)
        {
            var tmp = new Entity("e", DefaultTag());

            setup?.Invoke(tmp);
            return tmp;
        }
    }
}