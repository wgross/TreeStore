﻿using Moq;
using System;
using System.Linq;
using TreeStore.Model;
using TreeStore.PsModule.PathNodes;
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

            var result = new CategoryNode(DefaultCategory());

            // ASSERT

            Assert.Equal("c", result.Name);
        }

        [Fact]
        public void CategoryNode_provides_Value()
        {
            // ACT

            var result = new CategoryNode(DefaultCategory());

            // ASSERT

            Assert.Equal("c", result.Name);
            Assert.True(result.IsContainer);
        }

        #endregion P2F node structure

        #region IGetItem

        [Fact]
        public void CategoryNode_provides_Item()
        {
            // ARRANGE

            var c = DefaultCategory();

            // ACT

            var result = new CategoryNode(c).GetItem(this.ProviderContextMock.Object);

            // ASSERT

            Assert.Equal(c.Id, result.Property<Guid>("Id"));
            Assert.Equal(c.Name, result.Property<string>("Name"));
            Assert.Equal(TreeStoreItemType.Category, result.Property<TreeStoreItemType>("ItemType"));
            Assert.IsType<CategoryNode.Item>(result.ImmediateBaseObject);
        }

        #endregion IGetItem

        #region IGetItemProperties

        [Fact]
        public void CategoryNode_retrieves_all_properties()
        {
            // ARRANGE

            var c = DefaultCategory();

            // ACT

            var result = new CategoryNode(c).GetItemProperties(this.ProviderContextMock.Object, Enumerable.Empty<string>()).ToArray();

            // ASSERT

            Assert.Equal(new[] { "Id", "Name", "ItemType" }, result.Select(r => r.Name));
        }

        [Fact]
        public void CategoryNode_retrieves_specified_property()
        {
            // ARRANGE

            var c = DefaultCategory();

            // ACT

            var result = new CategoryNode(c).GetItemProperties(this.ProviderContextMock.Object, "NAME".Yield()).ToArray();

            // ASSERT

            Assert.Equal("Name", result.Single().Name, StringComparer.OrdinalIgnoreCase);
        }

        #endregion IGetItemProperties

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

            var node = new CategoryNode(category);

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

            var node = new CategoryNode(category);

            // ACT

            var result = node.Resolve(this.ProviderContextMock.Object, name).Single();

            // ASSERT

            Assert.Equal(entity.Id, ((EntityNode.Item)result.GetItem(this.ProviderContextMock.Object).ImmediateBaseObject).Id);
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

            var node = new CategoryNode(category);

            // ACT

            var result = node.Resolve(this.ProviderContextMock.Object, name).Single();

            // ASSERT

            Assert.Equal(category.SubCategories.Single().Id, ((CategoryNode.Item)result.GetItem(this.ProviderContextMock.Object).ImmediateBaseObject).Id);
        }

        #endregion Resolve

        #region INewItem

        [Fact]
        public void CategoryNode_provides_NewItemTypesNames()
        {
            // ARRANGE

            var node = new CategoryNode(DefaultCategory());

            // ACT

            var result = node.NewItemTypeNames;

            // ASSERT

            Assert.Equal(new[]
            {
                nameof(TreeStoreItemType.Category),
                nameof(TreeStoreItemType.Entity)
            }, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(nameof(TreeStoreItemType.Entity))]
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

            var node = new CategoryNode(category);

            // ACT

            var result = node.NewItem(this.ProviderContextMock.Object, newItemName: "Entity", itemTypeName: itemTypeName, newItemValue: null!);

            // ASSERT

            Assert.IsType<EntityNode>(result);
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

            var node = new CategoryNode(category);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => node.NewItem(this.ProviderContextMock.Object, newItemName: "Entity", itemTypeName: nameof(TreeStoreItemType.Entity), newItemValue: null!));

            // ASSERT

            Assert.Equal($"Name is already used by and item of type '{nameof(TreeStoreItemType.Entity)}'", result.Message);
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

            var node = new CategoryNode(category);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => node.NewItem(this.ProviderContextMock.Object, newItemName: "Entity", itemTypeName: nameof(TreeStoreItemType.Entity), newItemValue: null!));

            // ASSERT

            Assert.Equal($"Name is already used by and item of type '{nameof(TreeStoreItemType.Category)}'", result.Message);
        }

        [Fact]
        public void CategoryNode_creates_category()
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

            var node = new CategoryNode(category);

            // ACT

            var result = node.NewItem(this.ProviderContextMock.Object, newItemName: "c", itemTypeName: nameof(TreeStoreItemType.Category), newItemValue: null!);

            // ASSERT

            Assert.IsType<CategoryNode>(result);
        }

        [Fact]
        public void CategoryNode_creating_category_fails_on_duplicate_entity_name()
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

            var node = new CategoryNode(category);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => node.NewItem(this.ProviderContextMock.Object, newItemName: "c", itemTypeName: nameof(TreeStoreItemType.Category), newItemValue: null!));

            // ASSERT

            Assert.Equal($"Name is already used by and item of type '{nameof(TreeStoreItemType.Entity)}'", result.Message);
        }

        [Fact]
        public void CategoryNode_creating_category_fails_on_duplicate_category_name()
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

            var node = new CategoryNode(category);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => node.NewItem(this.ProviderContextMock.Object, newItemName: "c", itemTypeName: nameof(TreeStoreItemType.Category), newItemValue: null!));

            // ASSERT

            Assert.Equal($"Name is already used by and item of type '{nameof(TreeStoreItemType.Category)}'", result.Message);
        }

        [Theory]
        [MemberData(nameof(InvalidNameChars))]
        public void CategoryNode_creating_rejects_invalid_name_chararcters(char invalidChar)
        {
            // ARRANGE

            var category = DefaultCategory(c => c.Name = "c");
            var invalidName = new string("p".ToCharArray().Append(invalidChar).ToArray());
            var node = new CategoryNode(category);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => node.NewItem(this.ProviderContextMock.Object, invalidName, itemTypeName: nameof(TreeStoreItemType.Category), newItemValue: null));

            // ASSERT

            Assert.Equal($"category(name='{invalidName}' wasn't created: it contains invalid characters", result.Message);
        }

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

            new CategoryNode(subCategory).CopyItem(this.ProviderContextMock.Object, "c", "cc", entityContainer);

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

            var categoryContainer = new CategoryNode(subCategory);

            // ACT

            new CategoryNode(category)
                .CopyItem(this.ProviderContextMock.Object, initialName, destinationName, categoryContainer);

            // ASSERT

            Assert.Equal(resultName, createdCatagory!.Name);
            Assert.Equal(subCategory, createdCatagory!.Parent);
        }

        [Theory]
        [InlineData("e-src", (string?)null, "e-src")]
        [InlineData("e-src", "e-dst", "e-dst")]
        public void CategoryNode_copying_fails_if_entity_name_already_exists(string initialName, string destinationName, string resultName)
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
            var node = new CategoryNode(category);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(
                () => node.CopyItem(this.ProviderContextMock.Object, initialName, destinationName, entitiesNode));

            // ASSERT

            Assert.Equal($"Destination container contains already an entity with name '{resultName}'", result.Message);
        }

        [Theory]
        [InlineData("e-src", (string?)null, "e-src")]
        [InlineData("e-src", "e-dst", "e-dst")]
        public void CategoryNode_copying_fails_if_category_name_already_exists(string initialName, string destinationName, string resultName)
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
            var node = new CategoryNode(category);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(
                () => node.CopyItem(this.ProviderContextMock.Object, initialName, destinationName, entitiesNode));

            // ASSERTS

            Assert.Equal($"Destination container contains already a category with name '{resultName}'", result.Message);
        }

        [Theory]
        [MemberData(nameof(InvalidNameChars))]
        public void CategoryNode_copying_rejects_invalid_name_chararcters(char invalidChar)
        {
            // ARRANGE

            var category = DefaultCategory(c => c.Name = "c");
            var invalidName = new string("p".ToCharArray().Append(invalidChar).ToArray());
            var entitiesNode = new EntitiesNode();
            var node = new CategoryNode(category);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => node.CopyItem(this.ProviderContextMock.Object, "c", invalidName, entitiesNode));

            // ASSERT

            Assert.Equal($"category(name='{invalidName}' wasn't created: it contains invalid characters", result.Message);
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

            var categoryNode = new CategoryNode(category);

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

            var categoryNode = new CategoryNode(category);

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

            var categoryNode = new CategoryNode(category);

            // ACT

            categoryNode.RenameItem(this.ProviderContextMock.Object, "c", "cc");

            // ASSERT

            Assert.Equal("c", category.Name);
        }

        [Theory]
        [MemberData(nameof(InvalidNameChars))]
        public void CategoryNode_renaming_rejects_invalid_name_chararcters(char invalidChar)
        {
            // ARRANGE

            var category = DefaultCategory(c => c.Name = "c");
            var invalidName = new string("p".ToCharArray().Append(invalidChar).ToArray());
            var entitiesNode = new EntitiesNode();
            var node = new CategoryNode(category);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => node.RenameItem(this.ProviderContextMock.Object, "c", invalidName));

            // ASSERT

            Assert.Equal($"category(name='{invalidName}' wasn't renamed: it contains invalid characters", result.Message);
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

            this.ProviderContextMock
                .Setup(p => p.Recurse)
                .Returns(recurse);

            this.PersistenceMock
                .Setup(r => r.DeleteCategory(category, recurse))
                .Returns(true);

            var node = new CategoryNode(category);

            // ACT

            node.RemoveItem(this.ProviderContextMock.Object, "c");
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

            var categoryNode = new CategoryNode(c);
            var node = new CategoryNode(c2);

            // ACT

            node.MoveItem(this.ProviderContextMock.Object, "", destinationName, categoryNode);

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

            var categoryNode = new CategoryNode(c);
            var node = new CategoryNode(c2);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(
                () => node.MoveItem(this.ProviderContextMock.Object, "", destinationName, categoryNode));

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

            var categoryNode = new CategoryNode(c);
            var node = new CategoryNode(c2);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(
                () => node.MoveItem(this.ProviderContextMock.Object, "", destinationName, categoryNode));

            // ASSERT

            Assert.Equal($"Destination container contains already a category with name '{resultName}'", result.Message);
        }

        #endregion IMoveItem
    }
}