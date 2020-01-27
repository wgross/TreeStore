﻿using TreeStore.Model;
using Moq;
using TreeStore.PsModule.PathNodes;
using System.Linq;
using System.Management.Automation;
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

            var result = new TagNode(this.PersistenceMock.Object, DefaultTag());

            // ASSERT

            Assert.Equal("t", result.Name);
            Assert.Equal("+", result.ItemMode);
        }

        [Fact]
        public void TagNode_provides_Value()
        {
            // ACT

            var result = new TagNode(this.PersistenceMock.Object, DefaultTag()).GetItemProvider();

            // ASSERT

            Assert.Equal("t", result.Name);
            Assert.True(result.IsContainer);
        }

        [Fact]
        public void TagNodeValue_provides_Item()
        {
            // ARRANGE

            var tag = DefaultTag();

            // ACT

            var result = new TagNode(this.PersistenceMock.Object, tag).GetItemProvider().GetItem() as TagNode.Item;

            // ASSERT

            Assert.Equal("t", result!.Name);
            Assert.Equal(tag.Id, result!.Id);
            Assert.Equal(KosmographItemType.Tag, result!.ItemType);
        }

        #endregion P2F node structure

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

        #region IGetChildItem Members

        [Fact]
        public void TagNode_retrieves_FacetProperties_as_child_nodes()
        {
            // ACT

            var result = new TagNode(this.PersistenceMock.Object, DefaultTag()).GetChildNodes(this.ProviderContextMock.Object);

            // ASSERT

            Assert.Single(result);
        }

        #endregion IGetChildItem Members

        #region IGetItemProperty

        [Fact]
        public void TagNode_retrieves_FacetProperties_and_Csharp_properties()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            // ACT

            var result = new TagNode(this.PersistenceMock.Object, tag)
                .GetItemProvider().GetItemProperties(Enumerable.Empty<string>())
                .ToArray();

            // ASSERT

            Assert.Equal(3, result.Length);
        }

        [Theory]
        [InlineData("NAME")]
        [InlineData(nameof(Tag.Name))]
        public void TagNode_retrieves_single_Csharp_property_by_name(string propertyName)
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            // ACT

            var result = new TagNode(this.PersistenceMock.Object, tag)
                .GetItemProvider().GetItemProperties(propertyName.Yield())
                .Single();

            // ASSERT

            Assert.Equal(nameof(Tag.Name), result.Name);
            Assert.Equal(tag.Name, result.Value);
            Assert.Equal("System.String", result.TypeNameOfValue);
        }

        [Theory]
        [InlineData("P")]
        [InlineData("p")]
        public void TagNode_retrieves_single_FacetProperty_by_name(string propertyName)
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            // ACT

            var result = new TagNode(this.PersistenceMock.Object, tag)
                .GetItemProvider().GetItemProperties(propertyName.Yield())
                .Single();

            // ASSERT

            Assert.Equal(tag.Facet.Properties.Single().Name, result.Name);
            Assert.Null(result.Value);
            Assert.Equal(tag.Facet.Properties.Single().Type.ToString(), result.TypeNameOfValue);
        }

        [Fact]
        public void TagNode_retrieveing_single_FacetProperty_by_name_ignores_unknown_name()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            // ACT

            var result = new TagNode(this.PersistenceMock.Object, tag)
                .GetItemProvider().GetItemProperties("unknown".Yield());

            // ASSERT

            Assert.Empty(result);
        }

        #endregion IGetItemProperty

        #region ISetItemProperty

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

            new TagNode(this.PersistenceMock.Object, tag).GetItemProvider().SetItemProperties(new PSNoteProperty("Name", "changed").Yield());

            // ASSERT

            Assert.Equal("changed", tag.Name);
        }

        #endregion ISetItemProperty

        #region INewItem - replace with INewItemProperty!

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

            Assert.IsType<FacetPropertyNode.ItemProvider>(result);
            Assert.Equal("p", result!.Name);
            Assert.Equal(FacetPropertyTypeValues.Bool, ((FacetPropertyNode.Item)result.GetItem()).ValueType);
        }

        #endregion INewItem - replace with INewItemProperty!

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

            new TagNode(this.PersistenceMock.Object, tag)
                .CopyItem(this.ProviderContextMock.Object, "t", "tt", tagsContainer.GetItemProvider(), recurse: false);

            // ASSERT

            Assert.Equal("tt", createdTag!.Name);
            Assert.Equal("p", createdTag!.Facet.Properties.Single().Name);
            Assert.Equal(tag.Facet.Properties.Single().Type, createdTag!.Facet.Properties.Single().Type);
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

        #endregion IRenameItem

        #region INewItemProperty

        [Fact]
        public void TagsNode_creates_FacetProperty()
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
                .Setup(r => r.Upsert(It.Is<Tag>(t => t.Name.Equals("t"))))
                .Returns<Tag>(t => t);

            // ACT

            new TagNode(this.PersistenceMock.Object, tag)
                .NewItemProperty(this.ProviderContextMock.Object, propertyName: "p", propertyTypeName: "bool", newItemValue: null);
        }

        #endregion INewItemProperty

        #region IRenameItemProperty

        [Theory]
        [InlineData("p")]
        [InlineData("P")]
        public void TagNode_renames_FacetProperty(string sourcePropertyName)
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

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

            new TagNode(this.PersistenceMock.Object, tag)
                .RenameItemProperty(this.ProviderContextMock.Object, sourcePropertyName: sourcePropertyName, destinationPropertyName: "q");

            // ASSERT

            Assert.Equal("q", tag.Facet.Properties.Single().Name);
        }

        #endregion IRenameItemProperty

        #region IRemoveItemProperty

        [Theory]
        [InlineData("p")]
        [InlineData("P")]
        public void TagNode_removes_FacetProperty(string propertyName)
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

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

            new TagNode(this.PersistenceMock.Object, tag)
                .RemoveItemProperty(this.ProviderContextMock.Object, propertyName);

            // ASSERT

            Assert.Empty(tag.Facet.Properties);
        }

        #endregion IRemoveItemProperty

        #region ICopyItemProperty

        [Theory]
        [InlineData("p")]
        [InlineData("P")]
        public void TagNode_copies_facet_property_to_other_tag(string propertyName)
        {
            // ARRANGE

            var sourceTag = DefaultTag(WithDefaultProperty);
            var destinationTag = DefaultTag(WithoutProperty);

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.Upsert(destinationTag))
                .Returns<Tag>(t => t);

            var sourceTagNode = new TagNode(this.PersistenceMock.Object, sourceTag);

            // ACT

            new TagNode(this.PersistenceMock.Object, destinationTag)
                .CopyItemProperty(this.ProviderContextMock.Object, propertyName, "pp", sourceTagNode.GetItemProvider());

            // ASSERT

            Assert.Equal("pp", destinationTag.Facet.Properties.Single().Name);
            Assert.Equal(sourceTag.Facet.Properties.Single().Type, destinationTag.Facet.Properties.Single().Type);
            Assert.NotEqual(sourceTag.Facet.Properties.Single().Id, destinationTag.Facet.Properties.Single().Id);
        }

        #endregion ICopyItemProperty

        #region IMoveItemProperty

        [Theory]
        [InlineData("p")]
        [InlineData("P")]
        public void TagNode_moves_facet_property_to_other_tag(string propertyName)
        {
            // ARRANGE

            var sourceTag = DefaultTag(WithDefaultProperty);
            var destinationTag = DefaultTag(WithoutProperty);

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.Upsert(destinationTag))
                .Returns<Tag>(t => t);

            var sourceTagNode = new TagNode(this.PersistenceMock.Object, sourceTag);

            // ACT

            new TagNode(this.PersistenceMock.Object, destinationTag)
                .MoveItemProperty(this.ProviderContextMock.Object, propertyName, "pp", sourceTagNode.GetItemProvider());

            // ASSERT
            // move implements only the destination logic. The removal of the source property is implemented by the provider

            Assert.Equal("pp", destinationTag.Facet.Properties.Single().Name);
            Assert.Equal(FacetPropertyTypeValues.String, destinationTag.Facet.Properties.Single().Type);
            Assert.NotEqual(sourceTag.Facet.Properties.Single().Id, destinationTag.Facet.Properties.Single().Id);
        }

        #endregion IMoveItemProperty
    }
}