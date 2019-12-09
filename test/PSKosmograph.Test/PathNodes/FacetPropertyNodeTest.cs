using Kosmograph.Model;
using Moq;
using PSKosmograph.PathNodes;
using System;
using System.Linq;
using Xunit;

namespace PSKosmograph.Test.PathNodes
{
    public class FacetPropertyNodeTest : NodeTestBase
    {
        [Fact]
        public void FacetPropertyNode_has_name_and_ItemMode()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            // ACT

            var result = new FacetPropertyNode(tag, tag.Facet.Properties.Single());

            // ASSERT

            Assert.Equal("p", result.Name);
            Assert.Equal("+", result.ItemMode);
        }

        [Fact]
        public void FacetPropertyNode_provides_Value()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            // ACT

            var result = new FacetPropertyNode(tag, tag.Facet.Properties.Single()).GetItemProvider();

            // ASSERT

            Assert.Equal("p", result.Name);
            Assert.False(result.IsContainer);
            Assert.IsType<FacetPropertyNode.ItemProvider>(result);
        }

        [Fact]
        public void FacetPropertyNodeValue_provides_Item()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            // ACT

            var result = new FacetPropertyNode(tag, tag.Facet.Properties.Single()).GetItemProvider().GetItem() as FacetPropertyNode.Item;

            // ASSERT

            Assert.Equal(tag.Facet.Properties.Single().Id, result!.Id);
            Assert.Equal("p", result!.Name);
            Assert.Equal(tag.Facet.Properties.Single().Type, result!.ValueType);
            Assert.Equal(KosmographItemType.FacetProperty, result!.ItemType);
        }

        [Fact]
        public void FacetPropertyNode_has_no_children()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            // ACT

            var node = new FacetPropertyNode(tag, tag.Facet.Properties.Single());
            var result = (
                children: node.GetNodeChildren(this.ProviderContextMock.Object),
                parameters: node.GetNodeChildrenParameters
            );

            // ASSERT

            Assert.Empty(result.children);
            Assert.Null(result.parameters);
        }

        [Fact]
        public void FacetPropertyMode_removes_itself_from_Tag()
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
                .Setup(r => r.Upsert(tag))
                .Returns(tag);

            // ACT

            new FacetPropertyNode(tag, tag.Facet.Properties.Single())
                .RemoveItem(this.ProviderContextMock.Object, "p", true);

            // ASSERT

            Assert.Empty(tag.Facet.Properties);
        }

        [Fact]
        public void FacetPropertyNodeItem_set_FacetProperty_name()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            // ACT

            var item = new FacetPropertyNode(tag, tag.Facet.Properties.Single()).GetItemProvider().GetItem() as FacetPropertyNode.Item;
            item!.Name = "changed";

            // ASSERT

            Assert.Equal("changed", tag.Facet.Properties.Single().Name);
        }

        [Fact]
        public void FacetPropertyNodeItem_set_FacetProperty_type()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            // ACT

            var item = new FacetPropertyNode(tag, tag.Facet.Properties.Single()).GetItemProvider().GetItem() as FacetPropertyNode.Item;
            item!.ValueType = Kosmograph.Model.FacetPropertyTypeValues.Bool;

            // ASSERT

            Assert.Equal(Kosmograph.Model.FacetPropertyTypeValues.Bool, tag.Facet.Properties.Single().Type);
        }

        [Fact]
        public void FacetPropertyNode_copies_to_tag_with_same_name()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);
            var tag2 = DefaultTag(WithoutProperty, t => t.Name = "tt");

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.Upsert(tag2))
                .Returns(tag2);

            // ACT

            new FacetPropertyNode(tag, tag.Facet.Properties.Single())
                .CopyItem(this.ProviderContextMock.Object, "p", null, new TagNode(this.PersistenceMock.Object, tag2).GetItemProvider(), false);

            // ASSERT

            Assert.Single(tag2.Facet.Properties);
            Assert.Equal(tag.Facet.Properties.Single().Name, tag2.Facet.Properties.Single().Name);
            Assert.Equal(tag.Facet.Properties.Single().Type, tag2.Facet.Properties.Single().Type);
        }

        [Fact]
        public void FacetPropertyNode_copies_to_tag_with_new_name()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);
            var tag2 = DefaultTag(WithoutProperty, t => t.Name = "tt");

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.Upsert(tag2))
                .Returns(tag2);

            // ACT

            new FacetPropertyNode(tag, tag.Facet.Properties.Single())
                .CopyItem(this.ProviderContextMock.Object, "p", "pp", new TagNode(this.PersistenceMock.Object, tag2).GetItemProvider(), false);

            // ASSERT

            Assert.Single(tag2.Facet.Properties);
            Assert.Equal("pp", tag2.Facet.Properties.Single().Name);
            Assert.Equal(tag.Facet.Properties.Single().Type, tag2.Facet.Properties.Single().Type);
        }

        [Fact]
        public void FacetPropertyNode_copying_to_tag_fails_on_duplicate_property_name()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);
            var tag2 = DefaultTag(WithDefaultProperty, t => t.Name = "tt");

            // ACT

            var result = Assert.Throws<InvalidOperationException>(()
                => new FacetPropertyNode(tag, tag.Facet.Properties.Single()).CopyItem(this.ProviderContextMock.Object,
                    "p", "p", new TagNode(this.PersistenceMock.Object, tag2).GetItemProvider(), false));

            // ASSERT

            Assert.Equal("duplicate property name: p", result.Message);
        }

        [Fact]
        public void FacetPropertyNode_renames_itself()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

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

            new FacetPropertyNode(tag, tag.Facet.Properties.Single()).RenameItem(this.ProviderContextMock.Object, "p", "pp");

            // ASSERT

            Assert.Equal("pp", renamedTag!.Facet.Properties.Single().Name);
        }

        [Fact]
        public void FacetPropertyNode_renaming_doesnt_store_identical_name()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            // ACT

            new FacetPropertyNode(tag, tag.Facet.Properties.Single()).RenameItem(this.ProviderContextMock.Object, "p", "p");

            // ASSERT

            Assert.Equal("p", tag.Facet.Properties.Single().Name);
        }

        [Fact]
        public void FacetPropertyNode_renaming_fails_on_duplicate_name()
        {
            // ARRANGE

            var tag = DefaultTag(WithoutProperty, WithProperty("p", FacetPropertyTypeValues.String), WithProperty("pp", FacetPropertyTypeValues.Long));

            // ACT
            // rename with already used name (different casing)

            var result = Assert.Throws<InvalidOperationException>(() => 
                new FacetPropertyNode(tag, tag.Facet.Properties.First()).RenameItem(this.ProviderContextMock.Object, "p", "PP"));

            // ASSERT

            Assert.Equal("rename failed: property name 'PP' must be unique.", result.Message);
        }
    }
}