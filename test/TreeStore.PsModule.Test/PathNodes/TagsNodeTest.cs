using Moq;
using System;
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
        public void TagsNode_provides_Name_and_IsContainer()
        {
            // ACT

            var result = new TagsNode();

            // ASSERT

            Assert.Equal("Tags", result.Name);
            Assert.True(result.IsContainer);
        }

        #endregion P2F node structure

        #region IGetItem

        [Fact]
        public void TagsNodeValue_provides_Item()
        {
            // ACT

            var result = new TagsNode().GetItem(this.ProviderContextMock.Object);

            // ASSERT

            Assert.IsType<TagsNode.Item>(result.ImmediateBaseObject);

            var resultValue = (TagsNode.Item)result.ImmediateBaseObject;

            Assert.Equal("Tags", resultValue.Name);
        }

        #endregion IGetItem

        #region IGetChildItems

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

        #endregion IGetChildItems

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

        #region INewItem

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
        public void TagsNode_creates_Tag(string itemTypeName)
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
                .NewItem(this.ProviderContextMock.Object, newItemName: "t", itemTypeName: itemTypeName, newItemValue: null);

            // ASSERT

            Assert.IsType<TagNode>(result);
        }

        [Theory]
        [MemberData(nameof(InvalidNameChars))]
        public void TagsNode_creating_tag_rejects_invalid_characters(char invalidChar)
        {
            // ARRAMGE

            var invalidName = new string(@"Tags\t".ToCharArray().Append(invalidChar).ToArray());

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => new TagsNode()
                .NewItem(this.ProviderContextMock.Object, newItemName: invalidName, itemTypeName: null, newItemValue: null));

            // ASSERT

            Assert.Equal($"tag(name='{invalidName}' wasn't created: it contains invalid characters", result.Message);
        }

        [Theory]
        [MemberData(nameof(InvalidNodeNames))]
        public void TagsNode_creating_tag_rejects_reserved_name(string nodeName)
        {
            // ARRANGE

            var node = new TagsNode();

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => node.NewItem(this.ProviderContextMock.Object,
                newItemName: nodeName, itemTypeName: nameof(TreeStoreItemType.Entity), newItemValue: null!));

            // ASSERT

            Assert.Equal($"tag(name='{nodeName}' wasn't created: Name '{nodeName}' is reserved for future use.", result.Message);
        }

        #endregion INewItem
    }
}