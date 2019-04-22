using Kosmograph.Model;
using Moq;
using System;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public abstract class KosmographViewModelTestBase : IDisposable
    {
        protected MockRepository Mocks { get; } = new MockRepository(MockBehavior.Strict);

        protected Mock<IRelationshipRepository> RelationshipRepository { get; }
        protected Mock<IEntityRepository> EntityRepository { get; }
        protected Mock<ITagRepository> TagRepository { get; }
        protected Mock<IKosmographPersistence> Persistence { get; }
        protected Desktop.ViewModel.KosmographViewModel ViewModel { get; }

        public KosmographViewModelTestBase()
        {
            this.RelationshipRepository = this.Mocks.Create<IRelationshipRepository>();
            this.EntityRepository = this.Mocks.Create<IEntityRepository>();
            this.TagRepository = this.Mocks.Create<ITagRepository>();
            this.Persistence = this.Mocks.Create<IKosmographPersistence>();
            this.Persistence
                .Setup(p => p.Entities)
                .Returns(this.EntityRepository.Object);
            this.Persistence
                .Setup(p => p.Tags)
                .Returns(this.TagRepository.Object);
            this.Persistence
                .Setup(p => p.Relationships)
                .Returns(this.RelationshipRepository.Object);
            this.ViewModel = new Kosmograph.Desktop.ViewModel.KosmographViewModel(new Model.KosmographModel(this.Persistence.Object));
        }

        public void Dispose()
        {
            this.Mocks.VerifyAll();
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