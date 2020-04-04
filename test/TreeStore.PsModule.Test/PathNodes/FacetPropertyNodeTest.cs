using CodeOwls.PowerShell.Paths;
using Moq;
using System;
using System.Linq;
using System.Management.Automation;
using TreeStore.Model;
using TreeStore.PsModule.PathNodes;
using Xunit;

namespace TreeStore.PsModule.Test.PathNodes
{
    public class FacetPropertyNodeTest : NodeTestBase
    {
        #region P2F node structure

        [Fact]
        public void FacetPropertyNode_has_Name_and_IsContainer()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            // ACT

            var result = new FacetPropertyNode(tag, tag.Facet.Properties.Single());

            // ASSERT

            Assert.Equal("p", result.Name);
            Assert.False(result.IsContainer);
        }

        #endregion P2F node structure

        #region IGetItem

        [Fact]
        public void FacetPropertyNodeValue_provides_Item()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            // ACT

            var result = new FacetPropertyNode(tag, tag.Facet.Properties.Single()).GetItem(this.ProviderContextMock.Object);

            // ASSERT

            Assert.Equal(tag.Facet.Properties.Single().Id, result.Property<Guid>("Id"));
            Assert.Equal(tag.Facet.Properties.Single().Name, result.Property<string>("Name"));
            Assert.Equal(TreeStoreItemType.FacetProperty, result.Property<TreeStoreItemType>("ItemType"));
            Assert.Equal(FacetPropertyTypeValues.String, result.Property<FacetPropertyTypeValues>("ValueType"));
            Assert.IsType<FacetPropertyNode.Item>(result.ImmediateBaseObject);
        }

        #endregion IGetItem

        #region IGetChildItems

        [Fact]
        public void FacetPropertyNode_has_no_children()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            // ACT

            var node = new FacetPropertyNode(tag, tag.Facet.Properties.Single());
            var result = node.GetChildNodes(this.ProviderContextMock.Object);

            // ASSERT

            Assert.Empty(result);
        }

        #endregion IGetChildItems

        #region IRemoveItem

        [Fact]
        public void FacetPropertyMode_removes_itself_from_Tag()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            this.ProviderContextMock
                .Setup(p => p.Force)
                .Returns(false);

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByTag(tag))
                .Returns(Enumerable.Empty<Entity>());

            this.PersistenceMock
                .Setup(m => m.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.Upsert(tag))
                .Returns(tag);

            // ACT

            new FacetPropertyNode(tag, tag.Facet.Properties.Single()).RemoveItem(this.ProviderContextMock.Object, "p");

            // ASSERT

            Assert.Empty(tag.Facet.Properties);
        }

        [Fact]
        public void FacetPropertyMode_removing_itself_fails_if_tag_is_assigned()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);
            var entity = DefaultEntity(WithAssignedTag(tag));

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.ProviderContextMock
                .Setup(p => p.Force)
                .Returns(false);

            this.ProviderContextMock
                .Setup(p => p.WriteError(It.IsAny<ErrorRecord>()));

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByTag(tag))
                .Returns(entity.Yield());

            // ACT

            new FacetPropertyNode(tag, tag.Facet.Properties.Single()).RemoveItem(this.ProviderContextMock.Object, "p");
        }

        [Fact]
        public void FacetPropertyMode_removes_forced_tag_if_assigned_and_has_properties()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);
            var entity = DefaultEntity(WithAssignedTag(tag));

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.ProviderContextMock
                .Setup(p => p.Force)
                .Returns(true);

            this.PersistenceMock
                .Setup(p => p.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.Upsert(tag))
                .Returns(tag);

            // ACT

            new FacetPropertyNode(tag, tag.Facet.Properties.Single()).RemoveItem(this.ProviderContextMock.Object, "p");

            // ASSERT

            Assert.Empty(tag.Facet.Properties);
        }

        #endregion IRemoveItem

        #region ICopyItem

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
                .CopyItem(this.ProviderContextMock.Object, "p", null, new TagNode(tag2));

            // ASSERT

            Assert.Single(tag2.Facet.Properties);
            Assert.Equal(tag.Facet.Properties.Single().Name, tag2.Facet.Properties.Single().Name);
            Assert.Equal(tag.Facet.Properties.Single().Type, tag2.Facet.Properties.Single().Type);
        }

        [Fact]
        public void FacetPropertyNode_copies_with_new_name()
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
                .CopyItem(this.ProviderContextMock.Object, "p", "pp", new TagNode(tag2));

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
                    "p", "p", new TagNode(tag2)));

            // ASSERT

            Assert.Equal("duplicate property name: p", result.Message);
        }

        [Theory]
        //[InlineData(null)]// prohibited by powershell
        [InlineData(nameof(FacetPropertyNode.Item.Id))]
        [InlineData(nameof(FacetPropertyNode.Item.Name))]
        [InlineData(nameof(FacetPropertyNode.Item.ValueType))]
        [InlineData(nameof(FacetPropertyNode.Item.ItemType))]
        public void FacetPropertyNode_copying_rejects_internal_names(string propertyName)
        {
            // ARRANGE

            var tag = DefaultTag(WithoutProperty, WithProperty("p", FacetPropertyTypeValues.String), WithProperty("pp", FacetPropertyTypeValues.Long));

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() =>
                new FacetPropertyNode(tag, tag.Facet.Properties.First()).CopyItem(this.ProviderContextMock.Object, "p", propertyName, Mock.Of<PathNode>()));

            // ASSERT

            Assert.Equal($"facetProperty(name='{propertyName}') wasn't copied: name is reserved", result.Message);
        }

        [Theory]
        [MemberData(nameof(InvalidNameChars))]
        public void FacetPropertyNode_copying_rejects_invalid_characters(char invalidChar)
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);
            var invalidName = new string("p".ToCharArray().Append(invalidChar).ToArray());
            var tagsContainer = new TagsNode();

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => new FacetPropertyNode(tag, tag.Facet.Properties.First())
                .CopyItem(this.ProviderContextMock.Object, "p", invalidName, Mock.Of<PathNode>()));

            // ASSERT

            Assert.Equal($"facetProperty(name='{invalidName}' wasn't created: it contains invalid characters", result.Message);
        }

        #endregion ICopyItem

        #region IRenameItem

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
        public void FacetPropertyNode_renaming_ignores_same_name()
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);

            // ACT

            new FacetPropertyNode(tag, tag.Facet.Properties.Single()).RenameItem(this.ProviderContextMock.Object, "p", "p");

            // ASSERT

            Assert.Equal("p", tag.Facet.Properties.Single().Name);
        }

        [Fact]
        public void FacetPropertyNode_renaming_rejects_duplicate_name()
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

        [Theory]
        //[InlineData(null)]// prohibited by powershell
        [InlineData(nameof(FacetPropertyNode.Item.Id))]
        [InlineData(nameof(FacetPropertyNode.Item.Name))]
        [InlineData(nameof(FacetPropertyNode.Item.ValueType))]
        [InlineData(nameof(FacetPropertyNode.Item.ItemType))]
        public void FacetPropertyNode_renaming_rejects_internal_names(string propertyName)
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty, WithProperty("pp", FacetPropertyTypeValues.Long));

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => new FacetPropertyNode(tag, tag.Facet.Properties.First())
                .RenameItem(this.ProviderContextMock.Object, "p", propertyName));

            // ASSERT

            Assert.Equal($"facetProperty(name='{propertyName}') wasn't renamed: name is reserved", result.Message);
        }

        [Theory]
        [MemberData(nameof(InvalidNameChars))]
        public void FacetPropertyNode_renaming_rejects_invalid_characters(char invalidChar)
        {
            // ARRANGE

            var tag = DefaultTag(WithDefaultProperty);
            var invalidName = new string("p".ToCharArray().Append(invalidChar).ToArray());
            var tagsContainer = new TagsNode();

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => new FacetPropertyNode(tag, tag.Facet.Properties.First())
                .RenameItem(this.ProviderContextMock.Object, "p", invalidName));

            // ASSERT

            Assert.Equal($"facetProperty(name='{invalidName}' wasn't created: it contains invalid characters", result.Message);
        }

        #endregion IRenameItem

        #region IGetItemProperty

        [Fact]
        public void FacetPropertyNode_retrieves_Cs_properties_with_values()
        {
            // ARRANGE

            var tag = DefaultTag(WithoutProperty, WithProperty("p", FacetPropertyTypeValues.String), WithProperty("pp", FacetPropertyTypeValues.Long));
            var facetProperty = tag.Facet.Properties.First();

            // ACT

            var result = new FacetPropertyNode(tag, facetProperty).GetItemProperties(this.ProviderContextMock.Object, Enumerable.Empty<string>());

            // ASSERT

            Assert.Equal(new[] { "Id", "Name", "ValueType", "ItemType" }, result.Select(r => r.Name));
            Assert.Equal(new object[] { facetProperty.Id, facetProperty.Name, facetProperty.Type, TreeStoreItemType.FacetProperty }, result.Select(r => r.Value));
        }

        [Theory]
        [InlineData("ID")]
        [InlineData("id")]
        public void FacetPropertyNode_retrieves_specifoed_Cs_properties_with_values(string propertyName)
        {
            // ARRANGE

            var tag = DefaultTag(WithoutProperty, WithProperty("p", FacetPropertyTypeValues.String), WithProperty("pp", FacetPropertyTypeValues.Long));
            var facetProperty = tag.Facet.Properties.First();

            // ACT

            var result = new FacetPropertyNode(tag, facetProperty).GetItemProperties(this.ProviderContextMock.Object, new[] { propertyName });

            // ASSERT

            Assert.Equal("Id", result.Single().Name);
            Assert.Equal(facetProperty.Id, result.Single().Value);
        }

        #endregion IGetItemProperty
    }
}