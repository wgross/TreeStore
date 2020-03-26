using Moq;
using System;
using System.Linq;
using TreeStore.Model;
using TreeStore.PsModule.PathNodes;
using Xunit;

namespace TreeStore.PsModule.Test.PathNodes
{
    public class EntitiesNodeTest : NodeTestBase
    {
        #region P2F node structure

        [Fact]
        public void EntitiesNode_has_name_and_IsContainer()
        {
            // ACT

            var result = new EntitiesNode();

            // ASSERT

            Assert.Equal("Entities", result.Name);
            Assert.True(result.IsContainer);
        }

        #endregion P2F node structure

        #region IGetItem

        [Fact]
        public void EntitiesNodeValue_provides_Item()
        {
            // ACT

            var result = new EntitiesNode().GetItem(this.ProviderContextMock.Object);

            // ASSERT

            Assert.IsType<EntitiesNode.Item>(result.ImmediateBaseObject);

            var resultValue = (EntitiesNode.Item)result.ImmediateBaseObject;

            Assert.Equal("Entities", resultValue.Name);
        }

        #endregion IGetItem

        #region Resolve

        [Theory]
        [InlineData("e")]
        [InlineData("E")]
        public void EntitiesNodeValue_retrieves_EntityNode_by_name_and_root_category(string name)
        {
            // ARRANGE

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            var rootCategory = DefaultCategory(AsRoot);
            this.CategoryRepositoryMock
                .Setup(r => r.Root())
                .Returns(rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByParentAndName(rootCategory, name))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(s => s.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, name))
                .Returns(DefaultEntity());

            // ACT

            var result = new EntitiesNode()
                .Resolve(this.ProviderContextMock.Object, name)
                .ToArray();

            // ASSERT

            Assert.IsType<EntityNode>(result.Single());
        }

        [Theory]
        [InlineData("c")]
        [InlineData("C")]
        public void EntitiesNodeValue_retrieves_CategoryNode_by_name_and_root_category(string name)
        {
            // ARRANGE

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            var rootCategory = DefaultCategory(AsRoot);
            this.CategoryRepositoryMock
                .Setup(r => r.Root())
                .Returns(rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByParentAndName(rootCategory, name))
                .Returns(DefaultCategory());

            // ACT

            var result = new EntitiesNode()
                .Resolve(this.ProviderContextMock.Object, name)
                .ToArray();

            // ASSERT

            Assert.IsType<CategoryNode>(result.Single());
        }

        [Fact]
        public void EntitiesNodeValue_retrieves_all_entities_and_categories_by_null_name()
        {
            // ARRANGE

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            var subCategory = DefaultCategory();
            var rootCategory = DefaultCategory(AsRoot, WithSubCategory(subCategory));
            this.CategoryRepositoryMock
                .Setup(r => r.Root())
                .Returns(rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByParent(rootCategory))
                .Returns(subCategory.Yield());

            this.PersistenceMock
                .Setup(s => s.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategory(rootCategory))
                .Returns(DefaultEntity().Yield());

            // ACT

            var result = new EntitiesNode()
                .Resolve(this.ProviderContextMock.Object, null)
                .ToArray();

            // ASSERT

            Assert.Equal(2, result.Length);
            Assert.IsType<CategoryNode>(result.First());
            Assert.IsType<EntityNode>(result.Last());
        }

        [Fact]
        public void EntitiesNodeValue_returns_null_on_unknown_name()
        {
            // ARRANGE

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
               .Setup(p => p.Categories)
               .Returns(this.CategoryRepositoryMock.Object);

            var rootCategory = DefaultCategory(AsRoot);
            this.CategoryRepositoryMock
                .Setup(r => r.Root())
                .Returns(rootCategory);

            this.CategoryRepositoryMock
               .Setup(r => r.FindByParentAndName(rootCategory, "unknown"))
               .Returns((Category?)null);

            this.PersistenceMock
                .Setup(s => s.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "unknown"))
                .Returns((Entity?)null);

            // ACT

            var result = new EntitiesNode()
                .Resolve(this.ProviderContextMock.Object, "unknown")
                .ToArray();

            // ASSERT

            Assert.Empty(result);
        }

        #endregion Resolve

        #region INewItem

        [Fact]
        public void EntitiesNode_provides_NewItemTypesNames()
        {
            // ACT

            var result = new EntitiesNode().NewItemTypeNames;

            // ASSERT

            Assert.Equal(new[]
            {
                nameof(TreeStoreItemType.Category),
                nameof(TreeStoreItemType.Entity)
            }, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("Entity")]
        public void EntitiesNode_creates_EntityNodeValue(string itemTypeName)
        {
            // ARRANGE

            this.ProviderContextMock
                .Setup(c => c.DynamicParameters)
                .Returns((object?)null);

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            Entity newEntity = null;
            this.EntityRepositoryMock
                .Setup(r => r.Upsert(It.IsAny<Entity>()))
                .Callback<Entity>(e => newEntity = e)
                .Returns<Entity>(e => e);

            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            var rootCategory = DefaultCategory(AsRoot);
            this.CategoryRepositoryMock
                .Setup(r => r.Root())
                .Returns(rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByParentAndName(rootCategory, "Entity"))
                .Returns((Category?)null);

            var node = new EntitiesNode();

            // ACT

            var result = node.NewItem(this.ProviderContextMock.Object, newItemName: "Entity", itemTypeName: itemTypeName, newItemValue: null!);

            // ASSERT

            Assert.IsType<EntityNode>(result);
            Assert.Equal("Entity", newEntity!.Name);
            Assert.Equal(rootCategory, newEntity!.Category);
        }

        [Fact]
        public void EntitiesNode_provides_new_item_parameters()
        {
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

            Entity? entity = null;
            this.EntityRepositoryMock
                .Setup(r => r.Upsert(It.Is<Entity>(t => t.Name.Equals("ee"))))
                .Callback<Entity>(e => entity = e)
                .Returns<Entity>(e => e);

            this.PersistenceMock
                .Setup(m => m.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            var rootCategory = DefaultCategory(AsRoot);
            this.CategoryRepositoryMock
                .Setup(r => r.Root())
                .Returns(rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByParentAndName(rootCategory, "ee"))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(p => p.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(tag);

            var node = new EntitiesNode();

            // ACT

            var result = node.NewItem(this.ProviderContextMock.Object, newItemName: "ee", itemTypeName: nameof(TreeStoreItemType.Entity), newItemValue: null!);

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

            Entity? entity = null;
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

            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            var rootCategory = DefaultCategory(AsRoot);
            this.CategoryRepositoryMock
                .Setup(r => r.Root())
                .Returns(rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByParentAndName(rootCategory, "ee"))
                .Returns((Category?)null);

            // ACT

            var result = new EntitiesNode()
                .NewItem(this.ProviderContextMock.Object, newItemName: "ee", itemTypeName: nameof(TreeStoreItemType.Entity), newItemValue: null!);

            // ASSERT

            Assert.Equal(tag, entity!.Tags.Single());
        }

        [Fact]
        public void EntitiesNode_creates_CategoryNodeValue()
        {
            // ARRANGE

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            var rootCategory = DefaultCategory(AsRoot);
            this.CategoryRepositoryMock
                .Setup(r => r.Root())
                .Returns(rootCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.Upsert(It.Is<Category>(c => c.Name.Equals("c"))))
                .Returns<Category>(c => c);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, "c"))
                .Returns((Entity?)null);

            var node = new EntitiesNode();

            // ACT

            var result = node
                .NewItem(this.ProviderContextMock.Object, newItemName: "c", itemTypeName: nameof(TreeStoreItemType.Category), newItemValue: null!);

            // ASSERT

            Assert.IsType<CategoryNode>(result);
        }

        [Theory]
        [InlineData(nameof(TreeStoreItemType.AssignedFacetProperty))]
        [InlineData(nameof(TreeStoreItemType.AssignedTag))]
        [InlineData(nameof(TreeStoreItemType.FacetProperty))]
        [InlineData(nameof(TreeStoreItemType.Relationship))]
        [InlineData(nameof(TreeStoreItemType.Tag))]
        [InlineData("unknown")]
        [InlineData("")]
        public void EntitiesNode_creating_item_rejects_invalid_item_type(string itemTypeName)
        {
            // ARRANGE

            var node = new EntitiesNode();

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => node.NewItem(this.ProviderContextMock.Object,
                newItemName: "c", itemTypeName: itemTypeName, newItemValue: null!));

            // ASSERT

            Assert.Equal($"ItemType '{itemTypeName}' not allowed in the context", result.Message);
        }

        [Theory]
        [InlineData("name")]
        [InlineData("NAME")]
        public void EntitiesNode_creating_item_rejects_category_with_same_name_as_entity(string newItemName)
        {
            // ARRANGE

            var node = new EntitiesNode();

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.PersistenceMock
               .Setup(p => p.Categories)
               .Returns(this.CategoryRepositoryMock.Object);

            var rootCategory = DefaultCategory(AsRoot);
            this.CategoryRepositoryMock
                .Setup(r => r.Root())
                .Returns(rootCategory);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, newItemName))
                .Returns(DefaultEntity(e => e.Name = newItemName, WithEntityCategory(rootCategory)));

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => node.NewItem(this.ProviderContextMock.Object,
                newItemName: newItemName, itemTypeName: nameof(TreeStoreItemType.Category), newItemValue: null!));

            // ASSERT

            Assert.Equal("Name is already used by and item of type 'Entity'", result.Message);
        }

        [Theory]
        [InlineData("name")]
        [InlineData("NAME")]
        public void EntitiesNode_creating_item_rejects_entity_with_same_name_as_category(string newItemName)
        {
            // ARRANGE

            var node = new EntitiesNode();

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
               .Setup(p => p.Categories)
               .Returns(this.CategoryRepositoryMock.Object);

            var subCategory = DefaultCategory(c => c.Name = newItemName);
            var rootCategory = DefaultCategory(AsRoot, WithSubCategory(subCategory));
            this.CategoryRepositoryMock
                .Setup(r => r.Root())
                .Returns(rootCategory);

            this.CategoryRepositoryMock
              .Setup(r => r.FindByParentAndName(rootCategory, newItemName))
              .Returns(subCategory);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => node.NewItem(this.ProviderContextMock.Object,
                newItemName: newItemName, itemTypeName: nameof(TreeStoreItemType.Entity), newItemValue: null!));

            // ASSERT

            Assert.Equal("Name is already used by and item of type 'Category'", result.Message);
        }

        [Theory]
        [MemberData(nameof(InvalidNameChars))]
        public void EntitesNode_creating_item_rejects_invalid_chars(char invalidChar)
        {
            // ARRANGE

            var invalidName = new string("p".ToCharArray().Append(invalidChar).ToArray());
            var node = new EntitiesNode();

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => node.NewItem(this.ProviderContextMock.Object,
                newItemName: invalidName, itemTypeName: nameof(TreeStoreItemType.Entity), newItemValue: null!));

            // ASSERT

            Assert.Equal($"entity(name='{invalidName}' wasn't created: it contains invalid characters", result.Message);
        }

        #endregion INewItem
    }
}