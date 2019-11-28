using Kosmograph.Model;
using Moq;
using PSKosmograph.PathNodes;
using System.Linq;
using System.Management.Automation;
using Xunit;

namespace PSKosmograph.Test.PathNodes
{
    public class TagNodeTest : NodeTestBase
    {
        [Fact]
        public void TagNode_has_name_and_ItemMode()
        {
            // ACT

            var result = new TagNode(this.PersistenceMock.Object, DefaultTag());

            // ASSERT

            Assert.Equal("t", result.Name);
            Assert.Equal("+", result.ItemMode);
        }

        [Fact]
        public void TagNode_provides_Value()
        {
            // ACT

            var result = new TagNode(this.PersistenceMock.Object, DefaultTag()).GetNodeValue();

            // ASSERT

            Assert.Equal("t", result.Name);
            Assert.True(result.IsCollection);
        }

        [Fact]
        public void TagNodeValue_provides_Item()
        {
            // ARRANGE

            var tag = DefaultTag();

            // ACT

            var result = new TagNode(this.PersistenceMock.Object, tag).GetNodeValue().Item as TagNode.Item;

            // ASSERT

            Assert.Equal("t", result!.Name);
            Assert.Equal(tag.Id, result!.Id);
            Assert.Equal(KosmographItemType.Tag, result!.ItemType);
        }

        [Fact]
        public void TagNode_retrieves_FacetProperties_as_child_nodes()
        {
            // ACT

            var result = new TagNode(this.PersistenceMock.Object, DefaultTag()).GetNodeChildren(this.ProviderContextMock.Object);

            // ASSERT

            Assert.Single(result);
        }

        [Fact]
        public void TagNode_resolves_null_name_as_all_child_nodes()
        {
            // ACT

            var result = new TagNode(this.PersistenceMock.Object, DefaultTag()).Resolve(this.ProviderContextMock.Object, name: null);

            // ASSERT

            Assert.Single(result);
        }

        [Fact]
        public void TagNode_resolves_property_name_as_FacetPropertyNode()
        {
            // ACT

            var result = new TagNode(this.PersistenceMock.Object, DefaultTag()).Resolve(this.ProviderContextMock.Object, name: "p");

            // ASSERT

            Assert.Single(result);
        }

        [Fact]
        public void TagNode_resolves_unknown_property_name_as_empty_enumerable()
        {
            // ACT

            var result = new TagNode(this.PersistenceMock.Object, DefaultTag()).Resolve(this.ProviderContextMock.Object, name: "unknown");

            // ASSERT

            Assert.Empty(result);
        }

        [Fact]
        public void TagNodeValue_sets_Tag_name()
        {
            // ARRANGE

            var tag = DefaultTag();

            this.PersistenceMock
                .Setup(p => p.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.Upsert(tag))
                .Returns(tag);

            // ACT

            new TagNode(this.PersistenceMock.Object, tag).GetNodeValue().SetItemProperties(new PSNoteProperty("Name", "changed").Yield());

            // ASSERT

            Assert.Equal("changed", tag.Name);
        }

        [Fact]
        public void TagNode_provides_NewItemTypeNames()
        {
            // ARRANGE

            var tag = DefaultTag();

            // ACT

            var result = new TagNode(this.PersistenceMock.Object, tag).NewItemTypeNames;

            // ASSERT

            Assert.Equal(result, "FacetProperty".Yield());
        }

        [Fact]
        public void TagNode_provides_NewItemParameter()
        {
            // ARRANGE

            var tag = DefaultTag();

            // ACT

            var result = new TagNode(this.PersistenceMock.Object, tag).NewItemParameters;

            // ASSERT

            Assert.IsType<TagNode.NewFacetPropertyParameters>(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("FacetProperty")]
        public void TagNode_creates_new_FacetProperty(string itemTypeName)
        {
            // ARRANGE

            var tag = DefaultTag(t => t.Facet.Properties.Clear());

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.ProviderContextMock
                .Setup(c => c.DynamicParameters)
                .Returns(new TagNode.NewFacetPropertyParameters
                {
                    ValueType = FacetPropertyTypeValues.Bool
                });

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.Upsert(tag))
                .Returns(tag);

            // ACT

            var result = new TagNode(this.PersistenceMock.Object, tag)
                .NewItem(this.ProviderContextMock.Object, newItemChildPath: @"p", itemTypeName: itemTypeName, newItemValue: null);

            // ASSERT

            Assert.IsType<FacetPropertyNode.Value>(result);
            Assert.Equal("p", result!.Name);
            Assert.Equal(FacetPropertyTypeValues.Bool, ((FacetPropertyNode.Item)result.Item).ValueType);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TagNode_removes_itself(bool recurse)
        {
            // ARRANGE

            var tag = DefaultTag(WithoutProperty);

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.Delete(tag))
                .Returns(true);

            // ACT

            new TagNode(this.PersistenceMock.Object, tag)
                .RemoveItem(this.ProviderContextMock.Object, "t", recurse);
        }

        [Fact]
        public void TagNode_removes_itself_with_properties()
        {
            // ARRANGE

            var tag = DefaultTag(WithoutProperty);

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.Delete(tag))
                .Returns(true);

            // ACT

            var node = new TagNode(this.PersistenceMock.Object, tag);
            node.RemoveItem(this.ProviderContextMock.Object, "t", recurse: true);

            // ASSERT

            Assert.Empty(tag.Facet.Properties);
        }

        [Fact]
        public void TagNode_removing_itself_with_properties_fails_gracefully_if_recurse_false()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            // ACT

            new TagNode(this.PersistenceMock.Object, tag)
                .RemoveItem(this.ProviderContextMock.Object, "t", recurse: false);

            // ASSERT

            Assert.Single(tag.Facet.Properties);
        }

        [Fact]
        public void TagNode_copies_itself_as_new_tag()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            Tag? createdTag = null;
            this.TagRepositoryMock
                .Setup(r => r.Upsert(It.IsAny<Tag>()))
                .Callback<Tag>(t => createdTag = t)
                .Returns(tag);

            var tagsContainer = new TagsNode();

            // ACT

            new TagNode(this.PersistenceMock.Object, tag)
                .CopyItem(this.ProviderContextMock.Object, "t", "tt", tagsContainer.GetNodeValue(), recurse: false);

            // ASSERT

            Assert.Equal("tt", createdTag!.Name);
            Assert.Equal("p", createdTag!.Facet.Properties.Single().Name);
            Assert.Equal(tag.Facet.Properties.Single().Type, createdTag!.Facet.Properties.Single().Type);
        }

        [Fact]
        public void TagNode_renames_itself()
        {
            // ARRANGE

            var tag = DefaultTag();

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            Tag? renamedTag = null;
            this.TagRepositoryMock
                .Setup(r => r.Upsert(It.IsAny<Tag>()))
                .Callback<Tag>(t => renamedTag = t)
                .Returns(tag);

            // ACT

            new TagNode(this.PersistenceMock.Object, tag).RenameItem(this.ProviderContextMock.Object, "t", "tt");

            // ASSERT

            Assert.Equal("tt", renamedTag!.Name);
        }

        [Fact]
        public void TagNode_renaming_doesnt_store_identical_name()
        {
            // ARRANGE

            var tag = DefaultTag();

            // ACT

            new TagNode(this.PersistenceMock.Object, tag).RenameItem(this.ProviderContextMock.Object, "t", "t");

            // ASSERT

            Assert.Equal("t", tag.Name);
        }
    }
}