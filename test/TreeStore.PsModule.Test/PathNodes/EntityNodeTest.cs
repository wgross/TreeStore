using Moq;
using System;
using System.Linq;
using System.Management.Automation;
using TreeStore.Model;
using TreeStore.PsModule.PathNodes;
using Xunit;

namespace TreeStore.PsModule.Test.PathNodes
{
    public class EntityNodeTest : NodeTestBase
    {
        #region PathNode

        [Fact]
        public void EntityNode_has_name_and_ItemMode()
        {
            // ACT

            var result = new EntityNode(DefaultEntity());

            // ASSERT

            Assert.Equal("e", result.Name);
        }

        [Fact]
        public void EntityNode_provides_Name_and_IsContainer()
        {
            // ACT

            var result = new EntityNode(DefaultEntity());

            // ASSERT

            Assert.Equal("e", result.Name);
            Assert.True(result.IsContainer);
        }

        #endregion PathNode

        [Fact]
        public void EntityNode_retrieves_assigned_tags()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);

            // ACT

            var result = new EntityNode(e).GetChildNodes(this.ProviderContextMock.Object);

            // ASSERT

            Assert.Single(result);
        }

        [Theory]
        [InlineData("t")]
        [InlineData("T")]
        public void EntityNode_resolves_tag_name_as_AssignedTagNode(string name)
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);

            // ACT

            var result = new EntityNode(e).Resolve(this.ProviderContextMock.Object, name).Single();

            // ASSERT

            Assert.IsType<AssignedTagNode>(result);
        }

        [Fact]
        public void EntityNode_resolves_unkown_tag_name_as_empty_result()
        {
            // ARRANGE

            var e = DefaultEntity();

            // ACT

            var result = new EntityNode(e).Resolve(this.ProviderContextMock.Object, "unknown");

            // ASSERT

            Assert.Empty(result);
        }

        [Fact]
        public void EntityNode_resolves_null_tag_name_as_all_child_nodes()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);

            // ACT

            var result = new EntityNode(e).Resolve(this.ProviderContextMock.Object, null);

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

            this.ProviderContextMock
                .Setup(p => p.Recurse)
                .Returns(recurse);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.Delete(entity))
                .Returns(true);

            // ACT

            new EntityNode(entity).RemoveItem(this.ProviderContextMock.Object, "t");
        }

        [Fact]
        public void EntityNode_removes_itself_with_assigned_tags()
        {
            // ARRANGE

            var entity = DefaultEntity(WithAssignedDefaultTag);

            this.ProviderContextMock
              .Setup(c => c.Persistence)
              .Returns(this.PersistenceMock.Object);

            this.ProviderContextMock
                .Setup(p => p.Recurse)
                .Returns(true);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.Delete(entity))
                .Returns(true);

            // ACT

            new EntityNode(entity).RemoveItem(this.ProviderContextMock.Object, "t");
        }

        [Fact]
        public void EntityNode_removing_itself_with_assigned_tags_fails_gracefully_if_recurse_false()
        {
            // ARRANGE

            var entity = DefaultEntity(WithAssignedDefaultTag);

            this.ProviderContextMock
                .Setup(p => p.Recurse)
                .Returns(false);

            // ACT

            new EntityNode(entity).RemoveItem(this.ProviderContextMock.Object, "t");
        }

        #endregion IRemoveItem

        #region INewItem

        [Fact]
        public void EntityNode_provides_NewItemTypesNames()
        {
            // ARRANGE

            var e = DefaultEntity();

            // ACT

            var result = new EntityNode(e).NewItemTypeNames;

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

            var result = new EntityNode(e).NewItem(this.ProviderContextMock.Object, "t", itemTypeName: null, newItemValue: null);

            // ASSERT

            Assert.IsType<AssignedTagNode>(result);
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

            var result = Assert.Throws<InvalidOperationException>(() => new EntityNode(e).NewItem(this.ProviderContextMock.Object, "t", itemTypeName: null, newItemValue: null));

            // ASSERT

            Assert.Equal($"tag(name='t') doesn't exist.", result.Message);
        }

        [Fact]
        public void EntityNode_adding_tag_twice_by_name_fails_gracefully()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);
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

            var result = new EntityNode(e).NewItem(this.ProviderContextMock.Object, "t", itemTypeName: null, newItemValue: null);

            // ASSERT

            Assert.IsType<AssignedTagNode>(result);
        }

        #endregion INewItem

        #region IGetItem

        [Fact]
        public void EntityNode_provides_Item()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), "1");

            // ACT

            var result = new EntityNode(e).GetItem(this.ProviderContextMock.Object);

            // ASSERT

            Assert.Equal(e.Id, result.Property<Guid>("Id"));
            Assert.Equal(e.Name, result.Property<string>("Name"));
            Assert.Equal(TreeStoreItemType.Entity, result.Property<TreeStoreItemType>("ItemType"));
            Assert.Equal("t.p", result.Property<string[]>("Properties").Single());
            Assert.IsType<EntityNode.Item>(result.ImmediateBaseObject);

            var assignedTag = result.Property<PSObject>("t");

            Assert.IsType<AssignedTagNode.Item>(assignedTag.ImmediateBaseObject);
            Assert.Equal("1", assignedTag.Property<string>("p"));
        }

        #endregion IGetItem

        #region ICopyItem

        [Fact]
        public void EntityNode_copies_itself_as_new_entity()
        {
            // ARRANGE

            this.ArrangeRootCategory(out var rootCategory);

            var entity = DefaultEntity(WithAssignedDefaultTag, WithEntityCategory(rootCategory));
            entity.SetFacetProperty(entity.Tags.Single().Facet.Properties.Single(), "1");

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

            new EntityNode(entity).CopyItem(this.ProviderContextMock.Object, "e", "ee", entityContainer);

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

            var entity = DefaultEntity(WithAssignedDefaultTag);
            entity.SetFacetProperty(entity.Tags.Single().Facet.Properties.Single(), "1");

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

            var entityContainer = new CategoryNode(subCategory);

            // ACT

            new EntityNode(entity).CopyItem(this.ProviderContextMock.Object, initialName, destinationName, entityContainer);

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
        public void EntityNode_copying_rejects_category_if_entity_name_already_exists(string initialName, string destinationName, string resultName)
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

            var categoryNode = new CategoryNode(c);
            var e = DefaultEntity(e => e.Name = initialName);
            var node = new EntityNode(e);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(
                () => node.CopyItem(this.ProviderContextMock.Object, initialName, destinationName, categoryNode));

            // ASSERT

            Assert.Equal($"Destination container contains already an entity with name '{resultName}'", result.Message);
        }

        [Theory]
        [MemberData(nameof(InvalidNameChars))]
        public void EntityNode_copying_rejects_invalid_name_chararcters(char invalidChar)
        {
            // ARRANGE

            var entity = DefaultEntity(WithAssignedDefaultTag, WithEntityCategory(DefaultCategory()));
            var entityContainer = new EntitiesNode();
            var invalidName = new string("p".ToCharArray().Append(invalidChar).ToArray());
            var node = new EntityNode(entity);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => node.CopyItem(this.ProviderContextMock.Object, "e", invalidName, entityContainer));

            // ASSERT

            Assert.Equal($"entity(name='{invalidName}' wasn't copied: it contains invalid characters", result.Message);
        }

        #endregion ICopyItem

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

            var categoryNode = new CategoryNode(c);
            var node = new EntityNode(e);

            // ACT

            node.MoveItem(this.ProviderContextMock.Object, initialName, destinationName, categoryNode);

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
            var node = new EntityNode(e);

            // ACT

            node.MoveItem(this.ProviderContextMock.Object, initialName, destinationName, entitiesNode);

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

            var categoryNode = new CategoryNode(c);
            var e = DefaultEntity(e => e.Name = initialName);
            var node = new EntityNode(e);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(
                () => node.MoveItem(this.ProviderContextMock.Object, initialName, destinationName, categoryNode));

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
            var categoryNode = new CategoryNode(c);
            var node = new EntityNode(e);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(
                () => node.MoveItem(this.ProviderContextMock.Object, "e", null, categoryNode));

            // ASSERT

            Assert.Equal($"Destination container contains already a category with name '{e.Name}'", result.Message);
        }

        #endregion IMoveItem

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

            new EntityNode(entity).RenameItem(this.ProviderContextMock.Object, "e", "ee");

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

            var entityNode = new EntityNode(entity);

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

            var entityNode = new EntityNode(entity);

            // ACT

            entityNode.RenameItem(this.ProviderContextMock.Object, "e", "cc");

            // ASSERT

            Assert.Equal("e", entity.Name);
        }

        [Theory]
        [MemberData(nameof(InvalidNameChars))]
        public void EntityNode_renaming_rejects_invalid_name_chararcters(char invalidChar)
        {
            // ARRANGE

            var category = DefaultCategory(c => c.Name = "c");
            var entity = DefaultEntity(WithEntityCategory(category));
            var invalidName = new string("p".ToCharArray().Append(invalidChar).ToArray());
            var node = new EntityNode(entity);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => node.RenameItem(this.ProviderContextMock.Object, "e", invalidName));

            // ASSERT

            Assert.Equal($"entity(name='{invalidName}' wasn't renamed: it contains invalid characters", result.Message);
        }

        #endregion IRenameItem

        #region ISetItemProperty

        [Fact]
        public void EntityNode_sets_facet_property_value()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(e))
                .Returns(e);

            // ACT

            new EntityNode(e).SetItemProperties(this.ProviderContextMock.Object, new PSNoteProperty("T.P", "2").Yield());

            // ASSERT

            Assert.Equal("2", e.TryGetFacetProperty(e.Tags.Single().Facet.Properties.Single()).value);
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

            var e = DefaultEntity(WithAssignedDefaultTag);

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(e))
                .Returns(e);

            // ACT

            new EntityNode(e).SetItemProperties(this.ProviderContextMock.Object, new PSNoteProperty(propertyName, 2).Yield());

            // ASSERT

            Assert.False(e.TryGetFacetProperty(e.Tags.Single().Facet.Properties.Single()).exists);
        }

        [Fact]
        public void EntityNode_setting_facet_property_provides_parameter_with_completer()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);

            // ACT

            var result = (RuntimeDefinedParameterDictionary)new EntityNode(e).SetItemPropertyParameters;

            // ASSERT

            Assert.True(result.TryGetValue("TreeStorePropertyName", out var parameter));
            Assert.Single(parameter!.Attributes.Where(a => a.GetType().Equals(typeof(ParameterAttribute))));

            var resultValidateSet = (ValidateSetAttribute)parameter!.Attributes.Single(a => a.GetType().Equals(typeof(ValidateSetAttribute)));

            Assert.Equal("t.p", resultValidateSet.ValidValues.Single());
        }

        #endregion ISetItemProperty

        #region IGetItemProperty

        [Fact]
        public void EntityNode_retrieves_all_properties()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), "1");

            // ACT

            var result = new EntityNode(e).GetItemProperties(this.ProviderContextMock.Object, Enumerable.Empty<string>()).ToArray();

            // ASSERT
            // name and faceto property hav eben retreved

            Assert.Equal(new[] { "t", "Name", "Id", "ItemType", "Properties" }, result.Select(r => r.Name));
        }

        [Theory]
        [InlineData("NAME")]
        [InlineData("T")]
        public void EntityNode_retrieves_specified_property(string propertyName)
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);

            // ACT

            var result = new EntityNode(e).GetItemProperties(this.ProviderContextMock.Object, propertyName.Yield()).ToArray();

            // ASSERT

            Assert.Equal(propertyName, result.Single().Name, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void EntityNode_retrieving_specified_property_ignores_unknown_name()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);

            // ACT

            var result = new EntityNode(e).GetItemProperties(this.ProviderContextMock.Object, new[] { "Name", "unknown" }).ToArray();

            // ASSERT

            Assert.Equal("Name", result.Single().Name);
        }

        [Fact]
        public void EntityNode_retrieving_property_provides_parameter_with_completer()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);

            // ACT

            var result = (RuntimeDefinedParameterDictionary)new EntityNode(e).GetItemPropertyParameters;

            // ASSERT

            Assert.True(result.TryGetValue("TreeStorePropertyName", out var parameter));
            Assert.Single(parameter!.Attributes.Where(a => a.GetType().Equals(typeof(ParameterAttribute))));

            var resultValidateSet = (ValidateSetAttribute)parameter!.Attributes.Single(a => a.GetType().Equals(typeof(ValidateSetAttribute)));

            Assert.Equal(new[] { "t.p", "Id", "Name" }, resultValidateSet.ValidValues);
        }

        #endregion IGetItemProperty

        #region IClearItemProperty

        [Theory]
        [InlineData("t.p")]
        [InlineData("T.P")]
        public void EntityNode_clears_facet_properties_by_name(string propertyName)
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), "1");

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

            new EntityNode(e)
                .ClearItemProperty(this.ProviderContextMock.Object, propertyName.Yield());

            // ASSERT

            Assert.Empty(e.Values);
        }

        [Fact]
        public void EntityNode_clearing_facet_properties_ignores_unknown_name()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), "1");

            // ACT

            new EntityNode(e).ClearItemProperty(this.ProviderContextMock.Object, "unknown".Yield());

            // ASSERT

            Assert.Single(e.Values);
        }

        [Fact]
        public void EntityNode_clearing_facet_property_provides_parameter_with_completer()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);

            // ACT

            var result = (RuntimeDefinedParameterDictionary)new EntityNode(e).ClearItemPropertyParameters;

            // ASSERT

            Assert.True(result.TryGetValue("TreeStorePropertyName", out var parameter));
            Assert.Single(parameter!.Attributes.Where(a => a.GetType().Equals(typeof(ParameterAttribute))));

            var resultValidateSet = (ValidateSetAttribute)parameter!.Attributes.Single(a => a.GetType().Equals(typeof(ValidateSetAttribute)));

            Assert.Equal("t.p", resultValidateSet.ValidValues.Single());
        }

        #endregion IClearItemProperty

        #region ToFormattedString

        [Fact]
        public void EntityNode_provides_formatted_string_view()
        {
            // ARRANGE

            var e = DefaultEntity(
                e => e.Id = Guid.Parse("4faacbce-d42d-4b3c-9a5f-706533d731ed"),
                WithAssignedDefaultTag,
                WithAssignedTag(DefaultTag(
                    t => t.Name = "long_tag_name",
                    t =>
                    {
                        t.Facet.Properties.Single().Name = "long_property_name";
                        t.Facet.Properties.Single().Type = FacetPropertyTypeValues.Long;
                    }
                )));

            e.SetFacetProperty("t", "p", "test");
            e.SetFacetProperty("long_tag_name", "long_property_name", 1);

            var item = (EntityNode.Item)new EntityNode(e).GetItem(this.ProviderContextMock.Object).ImmediateBaseObject;

            // ACT

            var result = item.ToFormattedString();

            // ASSERT

            Assert.Equal(FormattedEntity, result);
        }

        public string FormattedEntity =>
@"e
  t
    p                  : test
  long_tag_name
    long_property_name : 1
";

        #endregion ToFormattedString
    }
}