using Kosmograph.Model;
using Moq;
using PSKosmograph.PathNodes;
using System.Linq;
using Xunit;

namespace PSKosmograph.Test.PathNodes
{
    public class EntitiesNodeTest : NodeTestBase
    {
        private readonly Mock<IEntityRepository> entitesRepository;

        public EntitiesNodeTest()
        {
            this.entitesRepository = this.Mocks.Create<IEntityRepository>();
        }

        [Fact]
        public void EntitiesNode_has_name_and_ItemMode()
        {
            // ACT

            var result = new EntitiesNode();

            // ASSERT

            Assert.Equal("Entities", result.Name);
            Assert.Equal("+", result.ItemMode);
        }

        [Fact]
        public void EntitiesNode_provides_Value()
        {
            // ACT

            var result = new EntitiesNode().GetNodeValue();

            // ASSERT

            Assert.Equal("Entities", result.Name);
            Assert.True(result.IsCollection);
        }

        [Fact]
        public void EntitiesNodeValue_provides_Item()
        {
            // ACT

            var result = new EntitiesNode().GetNodeValue().Item as EntitiesNode.Item;

            // ASSERT

            Assert.Equal("Entities", result!.Name);
            Assert.NotNull(result);
        }

        [Fact]
        public void EntitiesNodeValue_retrieves_EntityNode_by_name()
        {
            // ARRANGE

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(s => s.Entities)
                .Returns(this.entitesRepository.Object);

            this.entitesRepository
                .Setup(r => r.FindByName("e"))
                .Returns(DefaultEntity());

            // ACT

            var result = new EntitiesNode()
                .Resolve(this.ProviderContextMock.Object, "e")
                .ToArray();

            // ASSERT

            Assert.IsType<EntityNode>(result.Single());
        }

        [Fact]
        public void EntitiesNodeValue_retrieves_all_EntityNode_by_null_name()
        {
            // ARRANGE

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(s => s.Entities)
                .Returns(this.entitesRepository.Object);

            this.entitesRepository
                .Setup(r => r.FindAll())
                .Returns(DefaultEntity().Yield());

            // ACT

            var result = new EntitiesNode()
                .Resolve(this.ProviderContextMock.Object, null)
                .ToArray();

            // ASSERT

            Assert.IsType<EntityNode>(result.Single());
        }

        [Fact]
        public void EntitiesNodeValue_returns_null_on_unknown_name()
        {
            // ARRANGE

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(s => s.Entities)
                .Returns(this.entitesRepository.Object);

            this.entitesRepository
                .Setup(r => r.FindByName("unknown"))
                .Returns((Entity?)null);

            // ACT

            var result = new EntitiesNode()
                .Resolve(this.ProviderContextMock.Object, "unknown")
                .ToArray();

            // ASSERT

            Assert.Empty(result);
        }

        [Fact]
        public void EntitiesNode_provides_NewItemTypesNames()
        {
            // ACT

            var result = new EntitiesNode().NewItemTypeNames;

            // ASSERT

            Assert.Equal("Entity".Yield(), result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("Entity")]
        public void EntitiesNode_creates_EntityNodeValue(string itemTypeName)
        {
            // ARRANGE

            var entity = DefaultEntity();

            this.ProviderContextMock
                .Setup(c => c.DynamicParameters)
                .Returns((object?)null);

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(It.Is<Entity>(t => t.Name.Equals("Entity"))))
                .Returns<Entity>(e => e);

            // ACT

            var result = new EntitiesNode()
                .NewItem(this.ProviderContextMock.Object, newItemName: "Entity", itemTypeName: itemTypeName, newItemValue: null!);

            // ASSERT

            Assert.IsType<EntityNode.Value>(result);
        }

        [Fact]
        public void EntitiesNode_provides_new_item_parameters()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

            // ACT

            var result = new EntitiesNode().NewItemParameters;

            // ASSERT

            Assert.IsType<EntitiesNode.NewItemParametersDefinition>(result);
            Assert.Empty(((EntitiesNode.NewItemParametersDefinition)result!).Tags);
        }

        [Fact]
        public void EntitiesNode_creates_EntityNodeValue_with_tags_attached()
        {
            // ARRANGE

            var tag = DefaultTag();

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.ProviderContextMock
                .Setup(c => c.DynamicParameters)
                .Returns(new EntitiesNode.NewItemParametersDefinition
                {
                    Tags = new[] { "t" }
                });

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            Entity entity = null;
            this.EntityRepositoryMock
                .Setup(r => r.Upsert(It.Is<Entity>(t => t.Name.Equals("ee"))))
                .Callback<Entity>(e => entity = e)
                .Returns<Entity>(e => e);

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(tag);

            // ACT

            var result = new EntitiesNode()
                .NewItem(this.ProviderContextMock.Object, newItemName: "ee", itemTypeName: "any", newItemValue: null!);

            // ASSERT

            Assert.Equal(tag, entity!.Tags.Single());
        }

        [Fact]
        public void EntitiesNode_creatîng_EntityNodeValue_with_tags_attached_ignores_unkown_tags()
        {
            // ARRANGE

            var tag = DefaultTag();

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.ProviderContextMock
                .Setup(c => c.DynamicParameters)
                .Returns(new EntitiesNode.NewItemParametersDefinition
                {
                    Tags = new[] { "t", "unknown" }
                });

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            Entity entity = null;
            this.EntityRepositoryMock
                .Setup(r => r.Upsert(It.Is<Entity>(t => t.Name.Equals("ee"))))
                .Callback<Entity>(e => entity = e)
                .Returns<Entity>(e => e);

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(tag);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("unknown"))
                .Returns((Tag?)null);

            // ACT

            var result = new EntitiesNode()
                .NewItem(this.ProviderContextMock.Object, newItemName: "ee", itemTypeName: "any", newItemValue: null!);

            // ASSERT

            Assert.Equal(tag, entity!.Tags.Single());
        }
    }
}