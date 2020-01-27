using TreeStore.Model;
using Moq;
using TreeStore.PsModule.PathNodes;
using System;
using System.Linq;
using System.Management.Automation;
using Xunit;

namespace TreeStore.PsModule.Test.PathNodes
{
    public class EntityNodeTest : NodeTestBase
    {
        #region P2F node structure

        [Fact]
        public void EntityNode_has_name_and_ItemMode()
        {
            // ACT

            var result = new EntityNode(this.PersistenceMock.Object, DefaultEntity());

            // ASSERT

            Assert.Equal("e", result.Name);
            Assert.Equal("+", result.ItemMode);
        }

        [Fact]
        public void EntityNode_provides_Value()
        {
            // ACT

            var result = new EntityNode(this.PersistenceMock.Object, DefaultEntity()).GetItemProvider();

            // ASSERT

            Assert.Equal("e", result.Name);
            Assert.True(result.IsContainer);
        }

        [Fact]
        public void EntityNodeValue_provides_Item()
        {
            // ARRANGE

            var e = DefaultEntity();

            // ACT

            var result = new EntityNode(this.PersistenceMock.Object, e).GetItemProvider().GetItem() as EntityNode.Item;

            // ASSERT

            Assert.Equal(e.Id, result!.Id);
            Assert.Equal(e.Name, result!.Name);
            Assert.Equal(KosmographItemType.Entity, result!.ItemType);
        }

        #endregion P2F node structure

        [Fact]
        public void EntityNode_retrieves_assigned_tags()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

            this.ProviderContextMock
               .Setup(p => p.Persistence)
               .Returns(this.PersistenceMock.Object);

            // ACT

            var result = new EntityNode(this.PersistenceMock.Object, e).GetChildNodes(this.ProviderContextMock.Object);

            // ASSERT

            Assert.Single(result);
        }

        [Theory]
        [InlineData("t")]
        [InlineData("T")]
        public void EntityNode_resolves_tag_name_as_AssignedTagNode(string name)
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            // ACT

            var result = new EntityNode(this.PersistenceMock.Object, e).Resolve(this.ProviderContextMock.Object, name).Single();

            // ASSERT

            Assert.IsType<AssignedTagNode>(result);
        }

        [Fact]
        public void EntityNode_resolves_unkown_tag_name_as_empty_result()
        {
            // ARRANGE

            var e = DefaultEntity();

            // ACT

            var result = new EntityNode(this.PersistenceMock.Object, e).Resolve(this.ProviderContextMock.Object, "unknown");

            // ASSERT

            Assert.Empty(result);
        }

        [Fact]
        public void EntityNode_resolves_null_tag_name_as_all_child_nodes()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

            this.ProviderContextMock
                .Setup(p => p.Persistence)
                .Returns(this.PersistenceMock.Object);

            // ACT

            var result = new EntityNode(this.PersistenceMock.Object, e).Resolve(this.ProviderContextMock.Object, null);

            // ASSERT

            Assert.Single(result);
        }

        #region IRemoveItem

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EntityNode_removes_itself(bool recurse)
        {
            // ARRANGE

            var entity = DefaultEntity(WithoutTags);

            this.ProviderContextMock
              .Setup(c => c.Persistence)
              .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.Delete(entity))
                .Returns(true);

            // ACT

            new EntityNode(this.PersistenceMock.Object, entity).RemoveItem(this.ProviderContextMock.Object, "t", recurse);
        }

        [Fact]
        public void EntityNode_removes_itself_with_assigned_tags()
        {
            // ARRANGE

            var entity = DefaultEntity(WithDefaultTag);

            this.ProviderContextMock
              .Setup(c => c.Persistence)
              .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.Delete(entity))
                .Returns(true);

            // ACT

            new EntityNode(this.PersistenceMock.Object, entity)
                .RemoveItem(this.ProviderContextMock.Object, "t", recurse: true);
        }

        [Fact]
        public void EntityNode_removing_itself_with_assigned_tags_fails_gracefully_if_recurse_false()
        {
            // ARRANGE

            var entity = DefaultEntity(WithDefaultTag);

            // ACT

            new EntityNode(this.PersistenceMock.Object, entity)
                .RemoveItem(this.ProviderContextMock.Object, "t", recurse: false);
        }

        #endregion IRemoveItem

        [Fact]
        public void EntityNodeValue_sets_entity_name()
        {
            // ARRANGE

            var e = DefaultEntity();

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(e))
                .Returns(e);

            // ACT

            new EntityNode(this.PersistenceMock.Object, e).GetItemProvider().SetItemProperties(new PSNoteProperty("Name", "changed").Yield());

            // ASSERT

            Assert.Equal("changed", e.Name);
        }

        [Fact]
        public void EntityNode_provides_NewItemTypesNames()
        {
            // ARRANGE

            var e = DefaultEntity();

            // ACT

            var result = new EntityNode(this.PersistenceMock.Object, e).NewItemTypeNames;

            // ASSERT

            Assert.Equal("AssignedTag".Yield(), result);
        }

        [Fact]
        public void EntityNode_adds_tag_by_name()
        {
            // ARRANGE

            var e = DefaultEntity(WithoutTags);

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(e))
                .Returns(e);

            this.PersistenceMock
                .Setup(p => p.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(DefaultTag());

            // ACT

            var result = new EntityNode(this.PersistenceMock.Object, e).NewItem(this.ProviderContextMock.Object, "t", itemTypeName: null, newItemValue: null);

            // ASSERT

            Assert.IsType<AssignedTagNode.ItemProvider>(result);
        }

        [Fact]
        public void EntityNode_adding_tag_by_name_fails_for_unknown_tag()
        {
            // ARRANGE

            var e = DefaultEntity(WithoutTags);

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns((Tag?)null);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => new EntityNode(this.PersistenceMock.Object, e).NewItem(this.ProviderContextMock.Object, "t", itemTypeName: null, newItemValue: null));

            // ASSERT

            Assert.Equal($"tag(name='t') doesn't exist.", result.Message);
        }

        [Fact]
        public void EntityNode_adding_tag_twice_by_name_fails_gracefully()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);
            var tag = e.Tags.Single();

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(tag);

            // ACT

            var result = new EntityNode(this.PersistenceMock.Object, e).NewItem(this.ProviderContextMock.Object, "t", itemTypeName: null, newItemValue: null);

            // ASSERT

            Assert.IsType<AssignedTagNode.ItemProvider>(result);
        }

        #region ICopyItem

        [Fact]
        public void EntityNode_copies_itself_as_new_entity()
        {
            // ARRANGE

            this.ArrangeRootCategory(out var rootCategory);

            var entity = DefaultEntity(WithDefaultTag, WithEntityCategory(rootCategory));
            entity.SetFacetProperty(entity.Tags.Single().Facet.Properties.Single(), 1);

            this.CategoryRepositoryMock // destination name is unused
                .Setup(r => r.FindByCategoryAndName(rootCategory, "ee"))
                .Returns((Category?)null);

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock // detsinatin name is unused
                .Setup(r => r.FindByCategoryAndName(rootCategory, "ee"))
                .Returns((Entity?)null);

            Entity? createdEntity = null;
            this.EntityRepositoryMock
                .Setup(r => r.Upsert(It.IsAny<Entity>()))
                .Callback<Entity>(e => createdEntity = e)
                .Returns(entity);

            var entityContainer = new EntitiesNode();

            // ACT

            new EntityNode(this.PersistenceMock.Object, entity)
                .CopyItem(this.ProviderContextMock.Object, "e", "ee", entityContainer.GetItemProvider(), recurse: false);

            // ASSERT

            Assert.Equal("ee", createdEntity!.Name);
            Assert.Equal(rootCategory, createdEntity!.Category);
            Assert.Equal(entity.Tags.Single(), createdEntity!.Tags.Single());
            Assert.Equal(entity.Values.Single().Value, createdEntity!.Values.Single().Value);
            Assert.Equal(entity.Values.Single().Key, createdEntity!.Values.Single().Key);
        }

        [Theory]
        [InlineData("e-src", (string?)null, "e-src")]
        [InlineData("e-src", "e-dst", "e-dst")]
        public void EntityNode_copies_itself_to_category(string initialName, string destinationName, string resultName)
        {
            // ARRANGE

            this.ArrangeSubCategory(out var rootCategory, out var subCategory);

            this.CategoryRepositoryMock
                .Setup(r => r.FindById(subCategory.Id))
                .Returns(subCategory);

            var entity = DefaultEntity(WithDefaultTag);
            entity.SetFacetProperty(entity.Tags.Single().Facet.Properties.Single(), 1);

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

            Entity? createdEntity = null;
            this.EntityRepositoryMock
                .Setup(r => r.Upsert(It.IsAny<Entity>()))
                .Callback<Entity>(e => createdEntity = e)
                .Returns(entity);

            var entityContainer = new CategoryNode(this.PersistenceMock.Object, subCategory);

            // ACT

            new EntityNode(this.PersistenceMock.Object, entity)
                .CopyItem(this.ProviderContextMock.Object, initialName, destinationName, entityContainer.GetItemProvider(), recurse: false);

            // ASSERT

            Assert.Equal(resultName, createdEntity!.Name);
            Assert.Equal(subCategory, createdEntity!.Category);
            Assert.Equal(entity.Tags.Single(), createdEntity!.Tags.Single());
            Assert.Equal(entity.Values.Single().Value, createdEntity!.Values.Single().Value);
            Assert.Equal(entity.Values.Single().Key, createdEntity!.Values.Single().Key);
        }

        [Theory]
        [InlineData("e-src", (string?)null, "e-src")]
        [InlineData("e-src", "e-dst", "e-dst")]
        public void EntityNode_rejects_copying_itself_to_category_if_entity_name_already_exists(string initialName, string destinationName, string resultName)
        {
            // ARRANGE

            this.ProviderContextMock
                .Setup(p => p.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            var c = DefaultCategory();
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
                .Returns(DefaultEntity(e => e.Name = resultName));

            var categoryNode = new CategoryNode(this.PersistenceMock.Object, c);
            var e = DefaultEntity(e => e.Name = initialName);
            var node = new EntityNode(this.PersistenceMock.Object, e);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(
                () => node.CopyItem(this.ProviderContextMock.Object, initialName, destinationName, categoryNode.GetItemProvider(), recurse: false));

            // ASSERT

            Assert.Equal($"Destination container contains already an entity with name '{resultName}'", result.Message);
        }

        #endregion ICopyItem

        #region IRenameItem

        [Fact]
        public void EntityNode_renames_itself()
        {
            // ARRANGE

            var parentCategory = DefaultCategory(WithSubCategory(DefaultCategory()));
            var entity = DefaultEntity(WithEntityCategory(parentCategory));

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(p => p.FindByCategoryAndName(parentCategory, "ee"))
                .Returns((Entity?)null);

            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(parentCategory, "ee"))
                .Returns((Category?)null);

            Entity? renamedEntity = null;
            this.EntityRepositoryMock
                .Setup(r => r.Upsert(It.IsAny<Entity>()))
                .Callback<Entity>(t => renamedEntity = t)
                .Returns(entity);

            // ACT

            new EntityNode(this.PersistenceMock.Object, entity).RenameItem(this.ProviderContextMock.Object, "e", "ee");

            // ASSERT

            Assert.Equal("ee", renamedEntity!.Name);
        }

        [Theory]
        [InlineData("ee")]
        [InlineData("EE")]
        public void EntityNode_renaming_rejects_duplicate_entity_name(string existingName)
        {
            // ARRANGE

            var parentCategory = DefaultCategory(WithSubCategory(DefaultCategory()));
            var entity = DefaultEntity(WithEntityCategory(parentCategory));

            this.ProviderContextMock
              .Setup(c => c.Persistence)
              .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            var existingEntity = DefaultEntity(WithEntityCategory(parentCategory), e => e.Name = existingName);
            this.EntityRepositoryMock
                .Setup(p => p.FindByCategoryAndName(parentCategory, "ee"))
                .Returns(existingEntity);

            var entityNode = new EntityNode(this.PersistenceMock.Object, entity);

            // ACT

            entityNode.RenameItem(this.ProviderContextMock.Object, "e", "ee");

            // ASSERT

            Assert.Equal("e", entity.Name);
        }

        [Theory]
        [InlineData("cc")]
        [InlineData("CC")]
        public void EntityNode_renaming_rejects_duplicate_category_name(string existingName)
        {
            // ARRANGE

            var category = DefaultCategory(c => c.Name = "c");
            var parentCategory = DefaultCategory(WithSubCategory(DefaultCategory(c => c.Name = existingName)));
            var entity = DefaultEntity(WithEntityCategory(parentCategory));

            this.ProviderContextMock
              .Setup(c => c.Persistence)
              .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(p => p.FindByCategoryAndName(parentCategory, "cc"))
                .Returns((Entity?)null);

            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(parentCategory, "cc"))
                .Returns(category);

            var entityNode = new EntityNode(this.PersistenceMock.Object, entity);

            // ACT

            entityNode.RenameItem(this.ProviderContextMock.Object, "e", "cc");

            // ASSERT

            Assert.Equal("e", entity.Name);
        }

        #endregion IRenameItem

        #region Set facet property value

        [Fact]
        public void EntityNode_sets_facet_property_value()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(e))
                .Returns(e);

            // ACT

            new EntityNode(this.PersistenceMock.Object, e).GetItemProvider().SetItemProperties(new PSNoteProperty("T.P", 2).Yield());

            // ASSERT

            Assert.Equal(2, e.TryGetFacetProperty(e.Tags.Single().Facet.Properties.Single()).value);
        }

        [Theory]
        [InlineData("X.p")]
        [InlineData("t.X")]
        [InlineData(".p")]
        [InlineData("t.")]
        [InlineData("t.p.X")]
        [InlineData("p")]
        [InlineData("t")]
        //[InlineData((string?)null)]// blocked by powershell
        //[InlineData("")]// blocked by powershell
        public void EntityNode_setting_facet_property_value_ignores_invalid_name(string propertyName)
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(e))
                .Returns(e);

            // ACT

            new EntityNode(this.PersistenceMock.Object, e).GetItemProvider().SetItemProperties(new PSNoteProperty(propertyName, 2).Yield());

            // ASSERT

            Assert.False(e.TryGetFacetProperty(e.Tags.Single().Facet.Properties.Single()).exists);
        }

        #endregion Set facet property value

        #region IGetItemProperty

        [Fact]
        public void EntityValue_retrieves_entity_and_assigned_tags_properties()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

            // ACT

            var result = new EntityNode(this.PersistenceMock.Object, e).GetItemProvider().GetItemProperties(Enumerable.Empty<string>()).ToArray();

            // ASSERT

            Assert.Equal(new[] { "Name", "t.p" }, result.Select(r => r.Name));
            Assert.Equal(new object?[] { "e", (object?)null }, result.Select(r => r.Value));
        }

        [Theory]
        [InlineData("NAME")]
        [InlineData("T.P")]
        public void EntityValue_retrieves_entity_and_assigned_tags_properties_by_name(string propertyName)
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

            // ACT

            var result = new EntityNode(this.PersistenceMock.Object, e).GetItemProvider().GetItemProperties(propertyName.Yield()).ToArray();

            // ASSERT

            Assert.Equal(propertyName, result.Single().Name, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void EntityValue_retrieves_entity_and_assigned_tags_properties_by_name_ignores_unknown_names()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

            // ACT

            var result = new EntityNode(this.PersistenceMock.Object, e).GetItemProvider().GetItemProperties(new[] { "Name", "unknown" }).ToArray();

            // ASSERT

            Assert.Equal("Name", result.Single().Name);
        }

        #endregion IGetItemProperty

        #region IClearItemProperty

        [Theory]
        [InlineData("t.p")]
        [InlineData("T.P")]
        public void EntityNode_clears_assigned_tag_properties_by_name(string propertyName)
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), 1);

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
              .Setup(m => m.Entities)
              .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(e))
                .Returns<Entity>(e => e);

            // ACT

            new EntityNode(this.PersistenceMock.Object, e)
                .ClearItemProperty(this.ProviderContextMock.Object, propertyName.Yield());

            // ASSERT

            Assert.Empty(e.Values);
        }

        [Fact]
        public void EntityNode_clearing_assigned_tag_properties_ignores_unknown_name()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), 1);

            // ACT

            new EntityNode(this.PersistenceMock.Object, e)
                .ClearItemProperty(this.ProviderContextMock.Object, "unknown".Yield());

            // ASSERT

            Assert.Single(e.Values);
        }

        #endregion IClearItemProperty

        #region IMoveItem

        [Theory]
        [InlineData("e-src", (string?)null, "e-src")]
        [InlineData("e-src", "e-dst", "e-dst")]
        public void EntityNode_moves_itself_to_category(string initialName, string destinationName, string resultName)
        {
            // ARRANGE

            this.ProviderContextMock
                .Setup(p => p.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            var c = DefaultCategory();
            this.CategoryRepositoryMock
                .Setup(r => r.FindById(c.Id))
                .Returns(c);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(c, resultName))
                .Returns((Category?)null);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(c, resultName))
                .Returns((Entity?)null);

            var e = DefaultEntity(e => e.Name = initialName);
            this.EntityRepositoryMock
                .Setup(r => r.Upsert(e))
                .Returns<Entity>(e => e);

            var categoryNode = new CategoryNode(this.PersistenceMock.Object, c);
            var node = new EntityNode(this.PersistenceMock.Object, e);

            // ACT

            node.MoveItem(this.ProviderContextMock.Object, initialName, destinationName, categoryNode.GetItemProvider());

            // ASSERT

            Assert.Equal(c, e.Category);
            Assert.Equal(resultName, e.Name);
        }

        [Theory]
        [InlineData("e-src", (string?)null, "e-src")]
        [InlineData("e-src", "e-dst", "e-dst")]
        public void EntityNode_moves_itself_to_root_category(string initialName, string destinationName, string resultName)
        {
            // ARRANGE

            this.ArrangeSubCategory(out var rootCategory, out var subCategeory);

            this.ProviderContextMock
                .Setup(p => p.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            this.CategoryRepositoryMock
                .Setup(r => r.Root())
                .Returns(rootCategory);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, resultName))
                .Returns((Category?)null);

            this.EntityRepositoryMock
                .Setup(r => r.FindByCategoryAndName(rootCategory, resultName))
                .Returns((Entity?)null);

            var e = DefaultEntity(e => e.Name = initialName, WithEntityCategory(subCategeory));
            this.EntityRepositoryMock
                .Setup(r => r.Upsert(e))
                .Returns<Entity>(e => e);

            var entitiesNode = new EntitiesNode();
            var node = new EntityNode(this.PersistenceMock.Object, e);

            // ACT

            node.MoveItem(this.ProviderContextMock.Object, initialName, destinationName, entitiesNode.GetItemProvider());

            // ASSERT

            Assert.Equal(rootCategory, e.Category);
            Assert.Equal(resultName, e.Name);
        }

        [Theory]
        [InlineData("e-src", (string?)null, "e-src")]
        [InlineData("e-src", "e-dst", "e-dst")]
        public void EntityNode_rejects_moving_itself_to_category_if_entity_name_already_exists(string initialName, string destinationName, string resultName)
        {
            // ARRANGE

            this.ProviderContextMock
                .Setup(p => p.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            var c = DefaultCategory();
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
                .Returns(DefaultEntity(e => e.Name = resultName));

            var categoryNode = new CategoryNode(this.PersistenceMock.Object, c);
            var e = DefaultEntity(e => e.Name = initialName);
            var node = new EntityNode(this.PersistenceMock.Object, e);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(
                () => node.MoveItem(this.ProviderContextMock.Object, initialName, destinationName, categoryNode.GetItemProvider()));

            // ASSERT

            Assert.Equal($"Destination container contains already an entity with name '{resultName}'", result.Message);
        }

        [Fact]
        public void EntityNode_rejects_moving_itself_to_category_if_category_name_already_exists()
        {
            // ARRANGE

            this.ProviderContextMock
                .Setup(p => p.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Categories)
                .Returns(this.CategoryRepositoryMock.Object);

            var c = DefaultCategory();
            this.CategoryRepositoryMock
                .Setup(r => r.FindById(c.Id))
                .Returns(c);

            this.CategoryRepositoryMock
                .Setup(r => r.FindByCategoryAndName(c, "e"))
                .Returns(DefaultCategory());

            var e = DefaultEntity();
            var categoryNode = new CategoryNode(this.PersistenceMock.Object, c);
            var node = new EntityNode(this.PersistenceMock.Object, e);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(
                () => node.MoveItem(this.ProviderContextMock.Object, "e", null, categoryNode.GetItemProvider()));

            // ASSERT

            Assert.Equal($"Destination container contains already a category with name '{e.Name}'", result.Message);
        }

        #endregion IMoveItem
    }
}