using Moq;
using System.Linq;
using TreeStore.Model;
using TreeStore.PsModule.PathNodes;
using Xunit;

namespace TreeStore.PsModule.Test.PathNodes
{
    public class TagsNodeTest : NodeTestBase
    {
        private readonly Mock<ITagRepository> tagsRepository;

        public TagsNodeTest()
        {
            this.tagsRepository = this.Mocks.Create<ITagRepository>();
        }

        #region P2F node structure

        [Fact]
        public void TagsNode_has_name_and_ItemMode()
        {
            // ACT

            var result = new TagsNode();

            // ASSERT

            Assert.Equal("Tags", result.Name);
        }

        [Fact]
        public void TagsNode_provides_Value()
        {
            // ACT

            var result = new TagsNode();

            // ASSERT

            Assert.Equal("Tags", result.Name);
            Assert.True(result.IsContainer);
        }

        [Fact]
        public void TagsNodeValue_provides_Item()
        {
            // ACT

            var result = new TagsNode().GetItem();

            // ASSERT

            Assert.IsType<TagsNode.Item>(result.ImmediateBaseObject);

            var resultValue = (TagsNode.Item)result.ImmediateBaseObject;

            Assert.Equal("Tags", resultValue.Name);
        }

        #endregion P2F node structure

        [Fact]
        public void TagsNode_retrieves_Tags_as_child_nodes()
        {
            // ARRANGE

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(s => s.Tags)
                .Returns(this.tagsRepository.Object);

            this.tagsRepository
                .Setup(r => r.FindAll())
                .Returns(new[] { DefaultTag() });

            // ACT

            var result = new TagsNode()
                .GetChildNodes(this.ProviderContextMock.Object)
                .ToArray();

            // ASSERT

            Assert.IsType<TagNode>(result.Single());
        }

        [Fact]
        public void TagsNode_resolves_null_name_as_all_child_nodes()
        {
            // ARRANGE

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(s => s.Tags)
                .Returns(this.tagsRepository.Object);

            this.tagsRepository
                .Setup(r => r.FindAll())
                .Returns(new[] { DefaultTag() });

            // ACT

            var result = new TagsNode()
                .Resolve(this.ProviderContextMock.Object, null)
                .ToArray();

            // ASSERT

            Assert.IsType<TagNode>(result.Single());
        }

        [Fact]
        public void TagsNode_retrieves_TagNode_by_name()
        {
            // ARRANGE

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(s => s.Tags)
                .Returns(this.tagsRepository.Object);

            this.tagsRepository
                .Setup(r => r.FindByName("t"))
                .Returns(DefaultTag());

            // ACT

            var result = new TagsNode()
                .Resolve(this.ProviderContextMock.Object, "t")
                .ToArray();

            // ASSERT

            Assert.IsType<TagNode>(result.Single());
        }

        [Fact]
        public void TagsNode_resolves_unknown_tag_as_empty_enumerable()
        {
            // ARRANGE

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(s => s.Tags)
                .Returns(this.tagsRepository.Object);

            this.tagsRepository // Tag name is unkown
                .Setup(r => r.FindByName("t"))
                .Returns((Tag?)null);

            // ACT

            var result = new TagsNode()
                .Resolve(this.ProviderContextMock.Object, "t")
                .ToArray();

            // ASSERT

            Assert.Empty(result);
        }

        [Fact]
        public void TagsNode_provides_NewItemTypeNames()
        {
            // ACT

            var result = new TagsNode().NewItemTypeNames;

            // ASSERT

            Assert.Equal("Tag".Yield(), result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("Tag")]
        public void TagsNode_creates_TagNodeValue(string itemTypeName)
        {
            // ARRANGE

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.Upsert(It.Is<Tag>(t => t.Name.Equals("t"))))
                .Returns<Tag>(t => t);

            // ACT

            var result = new TagsNode()
                .NewItem(this.ProviderContextMock.Object, newItemChildPath: @"Tags\t", itemTypeName: itemTypeName, newItemValue: null);

            // ASSERT

            Assert.IsType<TagNode>(result);
        }
    }
}