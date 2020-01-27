using TreeStore.Model;
using Moq;
using TreeStore.PsModule.PathNodes;
using System;
using System.Linq;
using Xunit;

namespace TreeStore.PsModule.Test.PathNodes
{
    public class CategoryNodeTest : NodeTestBase
    {
        #region P2F node structure

        [Fact]
        public void CategoryNode_has_name_and_ItemMode()
        {
            // ACT

            var result = new CategoryNode(this.PersistenceMock.Object, DefaultCategory());

            // ASSERT

            Assert.Equal("c", result.Name);
            Assert.Equal("d+~<  cmr ", result.ItemMode);
        }

        [Fact]
        public void CategoryNode_provides_Value()
        {
            // ACT

            var result = new CategoryNode(this.PersistenceMock.Object, DefaultCategory()).GetItemProvider();

            // ASSERT

            Assert.Equal("c", result.Name);
            Assert.True(result.IsContainer);
        }

        [Fact]
        public void CategoryNodeValue_provides_Item()
        {
            // ARRANGE

            var c = DefaultCategory();

            // ACT

            var result = new CategoryNode(this.PersistenceMock.Object, c).GetItemProvider().GetItem() as CategoryNode.Item;

            // ASSERT

            Assert.Equal(c.Id, result!.Id);
            Assert.Equal(c.Name, result!.Name);
            Assert.Equal(KosmographItemType.Category, result!.ItemType);
        }

        #endregion P2F node structure

        #region IGetChildItem

        [Fact]
        public void CategoryNode_retrieves_categories_and_entities_as_childnodes()
        {
            // ARRANGE

            var category = DefaultCategory(WithSubCategory(DefaultCategory(c => c.Name = "cc")));
            var entity = DefaultEntity(e => e.Category = category);

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            this.CategoryRepositoryMock
                .Setup(p => p.FindByCategory(category))
                .Returns(category.SubCategories);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategory(category))
                .Returns(entity.Yield());

            var node = new CategoryNode(this.PersistenceMock.Object, category);

            // ACT

            var result = node.GetChildNodes(this.ProviderContextMock.Object).ToArray();

            // ASSERT

            Assert.Equal(2, result.Length);
        }

        #endregion IGetChildItem

        #region Resolve

        [Theory]
        [InlineData("e")]
        [InlineData("E")]
        public void CategoryNode_resolves_child_name_as_Entity(string name)
        {
            // ARRANGE

            var category = DefaultCategory();
            var entity = DefaultEntity(e => e.Category = category);

            this.ProviderContextMock
                .Setup(p => p.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategory(category))
                .Returns(entity.Yield());

            var node = new CategoryNode(this.PersistenceMock.Object, category);

            // ACT

            var result = node.Resolve(this.ProviderContextMock.Object, name).Single();

            // ASSERT

            Assert.Equal(entity.Id, ((EntityNode.Item)result.GetItemProvider().GetItem()).Id);
        }

        [Theory]
        [InlineData("cc")]
        [InlineData("CC")]
        public void CategoryNode_resolves_child_name_as_Category(string name)
        {
            // ARRANGE

            var category = DefaultCategory(WithSubCategory(DefaultCategory(c => c.Name = "cc")));

            this.ProviderContextMock
               .Setup(p => p.Persistence)
               .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategory(category))
                .Returns(Enumerable.Empty<Entity>());

            var node = new CategoryNode(this.PersistenceMock.Object, category);

            // ACT

            var result = node.Resolve(this.ProviderContextMock.Object, name).Single();

            // ASSERT

            Assert.Equal(category.SubCategories.Single().Id, ((CategoryNode.Item)result.GetItemProvider().GetItem()).Id);
        }

        #endregion Resolve

        #region INewItem

        [Fact]
        public void CategoryNode_provides_NewItemTypesNames()
        {
            // ARRANGE

            var node = new CategoryNode(this.PersistenceMock.Object, DefaultCategory());

            // ACT

            var result = node.NewItemTypeNames;

            // ASSERT

            Assert.Equal(new[]
            {
                nameof(KosmographItemType.Category),
                nameof(KosmographItemType.Entity)
            }, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(nameof(KosmographItemType.Entity))]
        public void CategoryNode_creates_EntityNodeValue(string itemTypeName)
        {
            // ARRANGE

            //todo: create entity with tag
            //this.ProviderContextMock
            //    .Setup(c => c.DynamicParameters)
            //    .Returns((object?)null);

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            var category = DefaultCategory();
            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(category, "Entity"))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(category, "Entity"))
                .Returns((Entity?)null);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(It.Is<Entity>(t => t.Name.Equals("Entity"))))
                .Returns<Entity>(e => e);

            var node = new CategoryNode(this.PersistenceMock.Object, category);

            // ACT

            var result = node.NewItem(this.ProviderContextMock.Object, newItemName: "Entity", itemTypeName: itemTypeName, newItemValue: null!);

            // ASSERT

            Assert.IsType<EntityNode.ItemProvider>(result);
        }

        [Fact]
        public void CategoryNode_rejects_creating_EntityNodeValue_with_duplicate_entity_name()
        {
            // ARRANGE

            //todo: create entity with tag
            //this.ProviderContextMock
            //    .Setup(c => c.DynamicParameters)
            //    .Returns((object?)null);

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            var category = DefaultCategory();
            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(category, "Entity"))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(category, "Entity"))
                .Returns(DefaultEntity());

            var node = new CategoryNode(this.PersistenceMock.Object, category);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => node.NewItem(this.ProviderContextMock.Object, newItemName: "Entity", itemTypeName: nameof(KosmographItemType.Entity), newItemValue: null!));

            // ASSERT

            Assert.Equal($"Name is already used by and item of type '{nameof(KosmographItemType.Entity)}'", result.Message);
        }

        [Fact]
        public void CategoryNode_rejects_creating_EntityNodeValue_with_duplicate_category_name()
        {
            // ARRANGE

            //todo: create entity with tag
            //this.ProviderContextMock
            //    .Setup(c => c.DynamicParameters)
            //    .Returns((object?)null);

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            var category = DefaultCategory();
            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(category, "Entity"))
                .Returns(DefaultCategory());

            var node = new CategoryNode(this.PersistenceMock.Object, category);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => node.NewItem(this.ProviderContextMock.Object, newItemName: "Entity", itemTypeName: nameof(KosmographItemType.Entity), newItemValue: null!));

            // ASSERT

            Assert.Equal($"Name is already used by and item of type '{nameof(KosmographItemType.Category)}'", result.Message);
        }

        [Fact]
        public void CategoriesNode_creates_CategoryNodeValue()
        {
            // ARRANGE

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            var category = DefaultCategory();
            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(category, "c"))
                .Returns((Category?)null);

            this.CategoryRepositoryMock
                .Setup(r => r.Upsert(It.Is<Category>(c => c.Name.Equals("c"))))
                .Returns<Category>(c => c);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(category, "c"))
                .Returns((Entity?)null);

            var node = new CategoryNode(this.PersistenceMock.Object, category);

            // ACT

            var result = node.NewItem(this.ProviderContextMock.Object, newItemName: "c", itemTypeName: nameof(KosmographItemType.Category), newItemValue: null!);

            // ASSERT

            Assert.IsType<CategoryNode.ItemProvider>(result);
        }

        [Fact]
        public void CategoriesNode_rejects_creating_CategoryNodeValue_with_duplicate_entity_name()
        {
            // ARRANGE

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            var category = DefaultCategory();
            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(category, "c"))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(category, "c"))
                .Returns(DefaultEntity());

            var node = new CategoryNode(this.PersistenceMock.Object, category);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => node.NewItem(this.ProviderContextMock.Object, newItemName: "c", itemTypeName: nameof(KosmographItemType.Category), newItemValue: null!));

            // ASSERT

            Assert.Equal($"Name is already used by and item of type '{nameof(KosmographItemType.Entity)}'", result.Message);
        }

        [Fact]
        public void CategoriesNode_rejects_creating_CategoryNodeValue_with_duplicate_category_name()
        {
            // ARRANGE

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            var category = DefaultCategory();
            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(category, "c"))
                .Returns(DefaultCategory());

            var node = new CategoryNode(this.PersistenceMock.Object, category);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => node.NewItem(this.ProviderContextMock.Object, newItemName: "c", itemTypeName: nameof(KosmographItemType.Category), newItemValue: null!));

            // ASSERT

            Assert.Equal($"Name is already used by and item of type '{nameof(KosmographItemType.Category)}'", result.Message);
        }

        //todo: crerate entity with duplicate entity name

        #endregion INewItem

        #region ICopyItem

        [Fact]
        public void CategoryNode_copies_itself_as_new_category()
        {
            // ARRANGE

            this.ArrangeSubCategory(out var rootCategory, out var subCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.Root())
                .Returns(rootCategory);

            this.CategoryRepositoryMock // destination name is unused
                .Setup(r => r.FindByCategoryAndName(rootCategory, "cc"))
                .Returns((Category?)null);

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock // destination name is unused
                .Setup(r => r.FindByCategoryAndName(rootCategory, "cc"))
                .Returns((Entity?)null);

            Category? createdCategory = null;
            this.CategoryRepositoryMock
                .Setup(r => r.Upsert(It.IsAny<Category>()))
                .Callback<Category>(c => createdCategory = c)
                .Returns<Category>(c => c);

            var entityContainer = new EntitiesNode();

            // ACT

            new CategoryNode(this.PersistenceMock.Object, subCategory)
                .CopyItem(this.ProviderContextMock.Object, "c", "cc", entityContainer.GetItemProvider(), recurse: false);

            // ASSERT

            Assert.Equal("cc", createdCategory!.Name);
            Assert.Equal(rootCategory, createdCategory!.Parent);
        }

        [Theory]
        [InlineData("e-src", (string?)null, "e-src")]
        [InlineData("e-src", "e-dst", "e-dst")]
        public void CategoryNode_copies_itself_to_category(string initialName, string destinationName, string resultName)
        {
            // ARRANGE

            this.ArrangeSubCategory(out var rootCategory, out var subCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindById(subCategory.Id))
                .Returns(subCategory);

            var category = DefaultCategory(c => rootCategory.AddSubCategory(c));

            this.CategoryRepositoryMock // destination name is unused
                .Setup(r => r.FindByCategoryAndName(subCategory, resultName))
                .Returns((Category?)null);

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock // detsinatin name is unused
                .Setup(r => r.FindByCategoryAndName(subCategory, resultName))
                .Returns((Entity?)null);

            Category? createdCatagory = null;
            this.CategoryRepositoryMock
                .Setup(r => r.Upsert(It.IsAny<Category>()))
                .Callback<Category>(c => createdCatagory = c)
                .Returns<Category>(c => c);

            var categoryContainer = new CategoryNode(this.PersistenceMock.Object, subCategory);

            // ACT

            new CategoryNode(this.PersistenceMock.Object, category)
                .CopyItem(this.ProviderContextMock.Object, initialName, destinationName, categoryContainer.GetItemProvider(), recurse: false);

            // ASSERT

            Assert.Equal(resultName, createdCatagory!.Name);
            Assert.Equal(subCategory, createdCatagory!.Parent);
        }

        [Theory]
        [InlineData("e-src", (string?)null, "e-src")]
        [InlineData("e-src", "e-dst", "e-dst")]
        public void CategoryNode_rejects_copying_itself_to_category_if_entity_name_already_exists(string initialName, string destinationName, string resultName)
        {
            // ARRANGE

            this.ArrangeSubCategory(out var rootCategory, out var subCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.Root())
                .Returns(rootCategory);

            this.ProviderContextMock
                .Setup(p => p.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, resultName))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, resultName))
                .Returns(DefaultEntity(e => e.Name = resultName));

            var entitiesNode = new EntitiesNode();
            var category = DefaultCategory(c => subCategory.AddSubCategory(c));
            var node = new CategoryNode(this.PersistenceMock.Object, category);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(
                () => node.CopyItem(this.ProviderContextMock.Object, initialName, destinationName, entitiesNode.GetItemProvider(), recurse: false));

            // ASSERT

            Assert.Equal($"Destination container contains already an entity with name '{resultName}'", result.Message);
        }

        [Theory]
        [InlineData("e-src", (string?)null, "e-src")]
        [InlineData("e-src", "e-dst", "e-dst")]
        public void CategoryNode_rejects_copying_itself_to_category_if_category_name_already_exists(string initialName, string destinationName, string resultName)
        {
            // ARRANGE

            this.ArrangeSubCategory(out var rootCategory, out var subCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.Root())
                .Returns(rootCategory);

            this.ProviderContextMock
                .Setup(p => p.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, resultName))
                .Returns(DefaultCategory());

            var entitiesNode = new EntitiesNode();
            var category = DefaultCategory(c => subCategory.AddSubCategory(c));
            var node = new CategoryNode(this.PersistenceMock.Object, category);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(
                () => node.CopyItem(this.ProviderContextMock.Object, initialName, destinationName, entitiesNode.GetItemProvider(), recurse: false));

            // ASSERT

            Assert.Equal($"Destination container contains already a category with name '{resultName}'", result.Message);
        }

        #endregion ICopyItem

        #region IRenameItem

        [Fact]
        public void CategoryNode_renames_itself()
        {
            // ARRANGE

            var category = DefaultCategory();
            var parentCategory = DefaultCategory(WithSubCategory(category));

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            Category? renamedCategory = null;
            this.CategoryRepositoryMock
                .Setup(r => r.Upsert(It.IsAny<Category>()))
                .Callback<Category>(c => renamedCategory = c)
                .Returns(category);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(p => p.FindByCategoryAndName(parentCategory, "cc"))
                .Returns((Entity?)null);

            var categoryNode = new CategoryNode(this.PersistenceMock.Object, category);

            // ACT

            categoryNode.RenameItem(this.ProviderContextMock.Object, "c", "cc");

            // ASSERT

            Assert.Equal("cc", renamedCategory!.Name);
        }

        [Theory]
        [InlineData("e")]
        [InlineData("E")]
        public void CategoryNode_renaming_rejects_duplicate_entity_name(string existingName)
        {
            // ARRANGE

            var category = DefaultCategory();
            var parentCategory = DefaultCategory(WithSubCategory(category));

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            var entity = DefaultEntity(e => e.Name = existingName);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(p => p.FindByCategoryAndName(parentCategory, "e"))
                .Returns(entity);

            var categoryNode = new CategoryNode(this.PersistenceMock.Object, category);

            // ACT

            categoryNode.RenameItem(this.ProviderContextMock.Object, "c", "e");

            // ASSERT

            Assert.Equal("c", category.Name);
        }

        [Theory]
        [InlineData("cc")]
        [InlineData("CC")]
        public void CategoryNode_renaming_rejects_duplicate_category_name(string existingName)
        {
            // ARRANGE

            var category = DefaultCategory(c => c.Name = "c");
            var parentCategory = DefaultCategory(
                WithSubCategory(category),
                WithSubCategory(DefaultCategory(c => c.Name = existingName)));

            var categoryNode = new CategoryNode(this.PersistenceMock.Object, category);

            // ACT

            categoryNode.RenameItem(this.ProviderContextMock.Object, "c", "cc");

            // ASSERT

            Assert.Equal("c", category.Name);
        }

        #endregion IRenameItem

        #region IRemoveItem

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CategoryNode_removes_itself(bool recurse)
        {
            // ARRANGE

            var category = DefaultCategory();

            this.ProviderContextMock
              .Setup(c => c.Persistence)
              .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(r => r.DeleteCategory(category, recurse))
                .Returns(true);

            var node = new CategoryNode(this.PersistenceMock.Object, category);

            // ACT

            node.RemoveItem(this.ProviderContextMock.Object, "c", recurse);
        }

        #endregion IRemoveItem

        #region IMoveItem

        [Theory]
        [InlineData("c-src", (string?)null, "c-src")]
        [InlineData("c-src", "c-dst", "c-dst")]
        public void CategoryNode_moves_itself_to_category(string initialName, string destinationName, string resultName)
        {
            // ARRANGE

            this.ProviderContextMock
                .Setup(p => p.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            var c = DefaultCategory();
            var c2 = DefaultCategory(c => c.Name = initialName);
            this.CategoryRepositoryMock
                .Setup(r => r.FindById(c.Id))
                .Returns(c);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(c, resultName))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(c, resultName))
                .Returns((Entity?)null);

            this.CategoryRepositoryMock
                .Setup(r => r.Upsert(c))
                .Returns<Category>(c => c);

            var categoryNode = new CategoryNode(this.PersistenceMock.Object, c);
            var node = new CategoryNode(this.PersistenceMock.Object, c2);

            // ACT

            node.MoveItem(this.ProviderContextMock.Object, "", destinationName, categoryNode.GetItemProvider());

            // ASSERT

            Assert.Equal(c, c2.Parent);
            Assert.Contains(c2, c.SubCategories);
            Assert.Equal(resultName, c2.Name);
        }

        [Theory]
        [InlineData("c-src", (string?)null, "c-src")]
        [InlineData("c-src", "c-dst", "c-dst")]
        public void CategoryNode_rejects_moving_itself_to_category_if_entity_name_already_exists(string initialName, string destinationName, string resultName)
        {
            // ARRANGE

            this.ProviderContextMock
                .Setup(p => p.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            var c = DefaultCategory();
            var c2 = DefaultCategory(c => c.Name = initialName);
            this.CategoryRepositoryMock
                .Setup(r => r.FindById(c.Id))
                .Returns(c);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(c, resultName))
                .Returns((Category?)null);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(c, resultName))
                .Returns(DefaultEntity());

            var categoryNode = new CategoryNode(this.PersistenceMock.Object, c);
            var node = new CategoryNode(this.PersistenceMock.Object, c2);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(
                () => node.MoveItem(this.ProviderContextMock.Object, "", destinationName, categoryNode.GetItemProvider()));

            // ASSERT

            Assert.Equal($"Destination container contains already an entity with name '{resultName}'", result.Message);
        }

        [Theory]
        [InlineData("c-src", (string?)null, "c-src")]
        [InlineData("c-src", "c-dst", "c-dst")]
        public void CategoryNode_rejects_moving_itself_to_category_if_category_name_already_exists(string initialName, string destinationName, string resultName)
        {
            // ARRANGE

            this.ProviderContextMock
                .Setup(p => p.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            var c = DefaultCategory();
            var c2 = DefaultCategory(c => c.Name = initialName);
            this.CategoryRepositoryMock
                .Setup(r => r.FindById(c.Id))
                .Returns(c);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(c, resultName))
                .Returns(DefaultCategory());

            var categoryNode = new CategoryNode(this.PersistenceMock.Object, c);
            var node = new CategoryNode(this.PersistenceMock.Object, c2);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(
                () => node.MoveItem(this.ProviderContextMock.Object, "", destinationName, categoryNode.GetItemProvider()));

            // ASSERT

            Assert.Equal($"Destination container contains already a category with name '{resultName}'", result.Message);
        }

        #endregion IMoveItem
    }
}