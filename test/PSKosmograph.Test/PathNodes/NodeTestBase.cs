using Kosmograph.Model;
using Moq;
using System;
using System.Linq;

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

        #region Default Tag

        protected Tag DefaultTag(params Action<Tag>[] setup)
        {
            var tmp = new Tag("t", new Facet("f", new FacetProperty("p", FacetPropertyTypeValues.String)));
            setup.ForEach(s => s(tmp));
            return tmp;
        }

        protected static void WithDefaultProperty(Tag tag)
        {
            tag.Facet.Properties.Clear();
            tag.Facet.AddProperty(new FacetProperty("p", FacetPropertyTypeValues.String));
        }

        protected static void WithoutProperty(Tag tag) => tag.Facet.Properties.Clear();

        protected static Action<Tag> WithProperty(string name, FacetPropertyTypeValues type)
        {
            return tag => tag.Facet.AddProperty(new FacetProperty(name, type));

        }

        #endregion Default Tag

        #region Default Entity

        protected Entity DefaultEntity(params Action<Entity>[] setup)
        {
            var tmp = new Entity("e");
            setup.ForEach(s => s(tmp));
            return tmp;
        }

        protected void WithDefaultTag(Entity entity) => entity.Tags.Add(DefaultTag(WithDefaultProperty));

        protected Action<Entity> WithDefaultPropertySet<V>(V value)
            => e => e.SetFacetProperty(e.Tags.First().Facet.Properties.First(), value);

        protected void WithoutTags(Entity entity) => entity.Tags.Clear();

        #endregion Default Entity
    }
}