using Kosmograph.LiteDb;
using Kosmograph.Messaging;
using Kosmograph.Model;
using Moq;
using System;
using System.Management.Automation;
using Xunit;

namespace PSKosmograph.Test
{
    [Collection("UsesPowershell")]
    public class KosmographCmdletProviderTestBase : IDisposable
    {
        public MockRepository Mocks { get; }

        public KosmographModel Model { get; }

        public Mock<IKosmographPersistence> PersistenceMock { get; }

        public Mock<ITagRepository> TagRepositoryMock { get; }

        public Mock<IEntityRepository> EntityRepositoryMock { get; }
        public Mock<IRelationshipRepository> RelationshipRepositoryMock { get; }

        public PowerShell PowerShell { get; }

        public KosmographCmdletProviderTestBase()
        {
            this.Mocks = new MockRepository(MockBehavior.Strict);
            this.Model = this.CreateModel();
            this.PersistenceMock = this.Mocks.Create<IKosmographPersistence>();
            this.TagRepositoryMock = this.Mocks.Create<ITagRepository>();
            this.EntityRepositoryMock = this.Mocks.Create<IEntityRepository>();
            this.RelationshipRepositoryMock = this.Mocks.Create<IRelationshipRepository>();

            KosmographCmdletProvider.NewKosmographService = () => this.PersistenceMock.Object;

            this.PowerShell = PowerShell.Create();

            this.PowerShell
                .AddStatement()
                    .AddCommand("Import-Module")
                    .AddArgument("./PSKosmograph.dll")
                    .Invoke();

            this.PowerShell
                .AddStatement()
                    .AddCommand("New-PsDrive")
                        .AddParameter("Name", "kg")
                        .AddParameter("PsProvider", "Kosmograph")
                        .AddParameter("Root", @"")
                        .Invoke();

            this.PowerShell.Commands.Clear();
        }

        public void Dispose() => this.Mocks.VerifyAll();

        protected KosmographModel CreateModel()
        {
            var model = new KosmographModel(new KosmographLiteDbPersistence(KosmographMessageBus.Default));
            var tag1 = model.Tags.Upsert(new Tag("t1", new Facet("facet",
                new FacetProperty("text", FacetPropertyTypeValues.String),
                new FacetProperty("long", FacetPropertyTypeValues.Long))));
            var tag2 = model.Tags.Upsert(new Tag("t2", new Facet("facet", new FacetProperty("p2"))));
            var entity1 = model.Entities.Upsert(new Entity("e1", tag1));
            var entity2 = model.Entities.Upsert(new Entity("e2"));
            //var entity3 = model.Entities.Upsert(new Entity("entity3"));
            //var entity4 = model.Entities.Upsert(new Entity("entity4", tag1));
            var relationship1 = model.Relationships.Upsert(new Relationship("relationship1", entity1, entity2, tag2));
            // var relationship2 = model.Relationships.Upsert(new Relationship("relationship2", entity2, entity3, tag2));
            return model;
        }

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