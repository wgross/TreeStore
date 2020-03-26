using Moq;
using System;
using System.Linq;
using TreeStore.Model;
using TreeStore.PsModule.PathNodes;
using Xunit;

namespace TreeStore.PsModule.Test.PathNodes
{
    public class TagNodeTest : NodeTestBase
    {
        #region P2F node structure

        [Fact]
        public void TagNode_has_name_and_ItemMode()
        {
            // ACT

            var result = new TagNode(DefaultTag());

            // ASSERT

            Assert.Equal("t", result.Name);
        }

        [Fact]
        public void TagNode_provides_Value()
        {
            // ACT

            var result = new TagNode(DefaultTag());

            // ASSERT

            Assert.Equal("t", result.Name);
            Assert.True(result.IsContainer);
        }

        #endregion P2F node structure

        #region IGetItem

        [Fact]
        public void TagNodeValue_provides_Item()
        {
            // ARRANGE

            var tag = DefaultTag();

            // ACT

            var result = new TagNode(tag).GetItem(this.ProviderContextMock.Object);

            // ASSERT

            Assert.Equal(tag.Id, result.Property<Guid>("Id"));
            Assert.Equal(tag.Name, result.Property<string>("Name"));
            Assert.Equal(TreeStoreItemType.Tag, result.Property<TreeStoreItemType>("ItemType"));
            //todo: properties //Assert.Equal("p", result.Property<string[]>("Properties").Single());
            Assert.IsType<TagNode.Item>(result.ImmediateBaseObject);
        }

        #endregion IGetItem

        [Fact]
        public void TagNode_resolves_null_name_as_all_child_nodes()
        {
            // ACT

            var result = new TagNode(DefaultTag()).Resolve(this.ProviderContextMock.Object, name: null);

            // ASSERT

            Assert.Single(result);
        }

        [Fact]
        public void TagNode_resolves_property_name_as_FacetPropertyNode()
        {
            // ACT

            var result = new TagNode(DefaultTag()).Resolve(this.ProviderContextMock.Object, name: "p");

            // ASSERT

            Assert.Single(result);
        }

        [Fact]
        public void TagNode_resolves_unknown_property_name_as_empty_enumerable()
        {
            // ACT

            var result = new TagNode(DefaultTag()).Resolve(this.ProviderContextMock.Object, name: "unknown");

            // ASSERT

            Assert.Empty(result);
        }

        #region IGetChildItem

        [Fact]
        public void TagNode_retrieves_FacetProperties_as_child_nodes()
        {
            // ACT

            var result = new TagNode(DefaultTag()).GetChildNodes(this.ProviderContextMock.Object);

            // ASSERT

            Assert.Single(result);
        }

        #endregion IGetChildItem

        #region IGetItemProperty

        [Fact]
        public void TagNode_retrieves_properties_with_values()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            // ACT

            var result = new TagNode(tag).GetItemProperties(this.ProviderContextMock.Object, Enumerable.Empty<string>());

            // ASSERT

            //todo: properties //Assert.Equal(new[] { "Name", "Id", "ItemType", "Properties" }, result.Select(p => p.Name));
            Assert.Equal(new[] { "Name", "Id", "ItemType" }, result.Select(p => p.Name));
        }

        [Theory]
        [InlineData("NAME")]
        [InlineData(nameof(Tag.Name))]
        public void TagNode_retrieves_specified_property_with_value(string propertyName)
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            // ACT

            var result = new TagNode(tag).GetItemProperties(this.ProviderContextMock.Object, propertyName.Yield());

            // ASSERT

            Assert.Equal(nameof(Tag.Name), result.Single().Name);
            Assert.Equal(tag.Name, result.Single().Value);
        }

        [Fact]
        public void TagNode_retrieveing_single_FacetProperty_by_name_ignores_unknown_name()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            // ACT

            var result = new TagNode(tag).GetItemProperties(this.ProviderContextMock.Object, "unknown".Yield());

            // ASSERT

            Assert.Empty(result);
        }

        #endregion IGetItemProperty

        #region INewItem

        [Fact]
        public void TagNode_provides_NewItemTypeNames()
        {
            // ARRANGE

            var tag = DefaultTag();

            // ACT

            var result = new TagNode(tag).NewItemTypeNames;

            // ASSERT

            Assert.Equal(result, "FacetProperty".Yield());
        }

        [Fact]
        public void TagNode_provides_NewItemParameter()
        {
            // ARRANGE

            var tag = DefaultTag();

            // ACT

            var result = new TagNode(tag).NewItemParameters;

            // ASSERT

            Assert.IsType<TagNode.NewFacetPropertyParameters>(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("FacetProperty")]
        public void TagNode_creates_FacetProperty(string itemTypeName)
        {
            // ARRANGE

            var tag = DefaultTag(WithoutProperty);

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

            var result = new TagNode(tag)
                .NewItem(this.ProviderContextMock.Object, newItemChildPath: "p", itemTypeName: itemTypeName, newItemValue: null);

            // ASSERT

            Assert.IsType<FacetPropertyNode>(result);
            Assert.Equal("p", result!.Name);
            Assert.Equal(FacetPropertyTypeValues.Bool, ((FacetPropertyNode.Item)result.GetItem(this.ProviderContextMock.Object).ImmediateBaseObject).ValueType);
        }

        [Theory]
        //[InlineData(null)]// prohibited by powershell
        [InlineData(nameof(FacetPropertyNode.Item.Id))]
        [InlineData(nameof(FacetPropertyNode.Item.Name))]
        [InlineData(nameof(FacetPropertyNode.Item.ValueType))]
        [InlineData(nameof(FacetPropertyNode.Item.ItemType))]
        public void TagNode_creating_FacetProperty_rejects_reserved_names(string propertyName)
        {
            // ARRANGE

            var tag = DefaultTag(WithoutProperty);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => new TagNode(tag)
                .NewItem(this.ProviderContextMock.Object, newItemChildPath: propertyName, itemTypeName: null, newItemValue: null));

            // ASSERT

            Assert.Equal($"facetProperty(name='{propertyName}') wasn't created: name is reserved", result.Message);
        }

        [Theory]
        [InlineData("p")]
        [InlineData("P")]
        public void TagNode_creating_FacetProperty_rejects_duplicate_name(string propertyName)
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => new TagNode(tag)
                .NewItem(this.ProviderContextMock.Object, newItemChildPath: propertyName, itemTypeName: null, newItemValue: null));

            // ASSERT

            Assert.Equal($"facetProperty(name='{propertyName}') wasn't created: name is duplicate", result.Message);
        }

        [Theory]
        [MemberData(nameof(InvalidNameChars))]
        public void TagNode_creating_Tag_rejects_invalid_characters(char invalidChar)
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);
            var invalidName = new string("p".ToCharArray().Append(invalidChar).ToArray());

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => new TagNode(tag)
                .NewItem(this.ProviderContextMock.Object, newItemChildPath: invalidName, itemTypeName: null, newItemValue: null));

            // ASSERT

            Assert.Equal($"facetProperty(name='{invalidName}' wasn't created: it contains invalid characters", result.Message);
        }

        #endregion INewItem

        #region IRemoveItem

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

            this.ProviderContextMock
                .Setup(p => p.Recurse)
                .Returns(recurse);

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.Delete(tag))
                .Returns(true);

            // ACT

            new TagNode(tag)
                .RemoveItem(this.ProviderContextMock.Object, "t");
        }

        [Fact]
        public void TagNode_removes_itself_with_properties()
        {
            // ARRANGE

            var tag = DefaultTag(WithoutProperty);

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.ProviderContextMock
                .Setup(p => p.Recurse)
                .Returns(true);

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.Delete(tag))
                .Returns(true);

            // ACT

            new TagNode(tag).RemoveItem(this.ProviderContextMock.Object, "t");

            // ASSERT

            Assert.Empty(tag.Facet.Properties);
        }

        [Fact]
        public void TagNode_removing_itself_with_properties_fails_gracefully_if_recurse_false()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            this.ProviderContextMock
                .Setup(p => p.Recurse)
                .Returns(false);

            // ACT

            new TagNode(tag).RemoveItem(this.ProviderContextMock.Object, "t");

            // ASSERT

            Assert.Single(tag.Facet.Properties);
        }

        #endregion IRemoveItem

        #region ICopyItem

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

            new TagNode(tag)
                .CopyItem(this.ProviderContextMock.Object, "t", "tt", tagsContainer);

            // ASSERT

            Assert.Equal("tt", createdTag!.Name);
            Assert.Equal("p", createdTag!.Facet.Properties.Single().Name);
            Assert.Equal(tag.Facet.Properties.Single().Type, createdTag!.Facet.Properties.Single().Type);
        }

        [Theory]
        [MemberData(nameof(InvalidNameChars))]
        public void TagNode_copying_itself_rejects_invalid_characters(char invalidChar)
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);
            var invalidName = new string("t".ToCharArray().Append(invalidChar).ToArray());
            var tagsContainer = new TagsNode();

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => new TagNode(tag)
                .CopyItem(this.ProviderContextMock.Object, "t", invalidName, tagsContainer));

            // ASSERT

            Assert.Equal($"tag(name='{invalidName}' wasn't created: it contains invalid characters", result.Message);
        }

        #endregion ICopyItem

        #region IRenameItem

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

            new TagNode(tag).RenameItem(this.ProviderContextMock.Object, "t", "tt");

            // ASSERT

            Assert.Equal("tt", renamedTag!.Name);
        }

        [Fact]
        public void TagNode_renaming_doesnt_store_identical_name()
        {
            // ARRANGE

            var tag = DefaultTag();

            // ACT

            new TagNode(tag).RenameItem(this.ProviderContextMock.Object, "t", "t");

            // ASSERT

            Assert.Equal("t", tag.Name);
        }

        [Theory]
        [MemberData(nameof(InvalidNameChars))]
        public void TagNode_renaming_rejects_invalid_characters(char invalidChar)
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);
            var invalidName = new string("t".ToCharArray().Append(invalidChar).ToArray());
            var tagsContainer = new TagsNode();

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => new TagNode(tag)
                .RenameItem(this.ProviderContextMock.Object, "t", invalidName));

            // ASSERT

            Assert.Equal($"tag(name='{invalidName}' wasn't created: it contains invalid characters", result.Message);
        }

        #endregion IRenameItem
    }
}