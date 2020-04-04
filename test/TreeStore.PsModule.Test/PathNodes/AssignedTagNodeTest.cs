using Moq;
using System;
using System.Linq;
using System.Management.Automation;
using TreeStore.Model;
using TreeStore.PsModule.PathNodes;
using Xunit;

namespace TreeStore.PsModule.Test.PathNodes
{
    public class AssignedTagNodeTest : NodeTestBase
    {
        #region P2F node structure

        [Fact]
        public void AssignedTagNode_has_name_and_IsContainer()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);

            // ACT

            var result = new AssignedTagNode(e, e.Tags.Single());

            // ASSERT

            Assert.Equal("t", result.Name);
            Assert.False(result.IsContainer);
        }

        #endregion P2F node structure

        #region IGetItem

        [Fact]
        public void AssignedTagNode_provides_Item()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), "1");

            // ACT

            var result = new AssignedTagNode(e, e.Tags.Single()).GetItem(this.ProviderContextMock.Object);

            // ASSERT

            Assert.Equal(e.Tags.Single().Id, result.Property<Guid>("Id"));
            Assert.Equal("t", result.Property<string>("Name"));
            Assert.Equal(TreeStoreItemType.AssignedTag, result.Property<TreeStoreItemType>("ItemType"));
            Assert.Equal("1", result.Property<string>("p"));
            //todo: properties // Assert.Equal("p", result.Property<string[]>("Properties").Single());
            Assert.IsType<AssignedTagNode.Item>(result.ImmediateBaseObject);
        }

        #endregion IGetItem

        #region IRemoveItem

        [Fact]
        public void AssignedTagNode_removes_itself()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);

            this.ProviderContextMock
                .Setup(p => p.Force)
                .Returns(true);

            this.ProviderContextMock
                .Setup(p => p.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(e))
                .Returns(e);

            // ACT

            new AssignedTagNode(e, e.Tags.Single()).RemoveItem(this.ProviderContextMock.Object, "t");

            // ARRANGE

            Assert.Empty(e.Tags);
        }

        [Fact]
        public void AssignedTagNode_removes_itself_with_values()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag, WithDefaultPropertySet(value: "test"));

            this.ProviderContextMock
               .Setup(p => p.Force)
               .Returns(true);

            this.ProviderContextMock
                .Setup(p => p.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(e))
                .Returns(e);

            // ACT

            new AssignedTagNode(e, e.Tags.Single()).RemoveItem(this.ProviderContextMock.Object, "t");

            // ARRANGE
            // property values aren't child items.

            Assert.Empty(e.Tags);
            Assert.Empty(e.Values);
        }

        [Fact]
        public void AssignedTagNode_removing_itself_with_properties_have_values_if_not_forced()
        {
            var entity = DefaultEntity(WithAssignedDefaultTag, WithDefaultPropertySet(value: "test"));

            this.ProviderContextMock
                .Setup(p => p.Force)
                .Returns(false);

            this.ProviderContextMock
                .Setup(p => p.WriteError(It.IsAny<ErrorRecord>()));

            // ACT

            new AssignedTagNode(entity, entity.Tags.Single()).RemoveItem(this.ProviderContextMock.Object, "t");
        }

        #endregion IRemoveItem

        #region IGetItemProperties

        [Fact]
        public void AssignedTagNode_retrieves_properties_with_values()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), "1");

            // ACT

            var result = new AssignedTagNode(e, e.Tags.Single()).GetItemProperties(this.ProviderContextMock.Object, propertyNames: Enumerable.Empty<string>());

            // ASSERT

            //todo: properties //Assert.Equal(new[] { "p", "Name", "ItemType", "Properties" }, result.Select(p => p.Name));
            Assert.Equal(new[] { "p", "Id", "Name", "ItemType" }, result.Select(p => p.Name));
            //todo: properties Assert.Equal(new object[] { "1", "t", TreeStoreItemType.AssignedTag, new string[] { "p" } }, result.Select(p => p.Value));
            Assert.Equal(new object[] { "1", e.Tags.Single().Id, "t", TreeStoreItemType.AssignedTag }, result.Select(p => p.Value));
        }

        [Theory]
        [InlineData("NAME")]
        [InlineData("P")]
        public void AssignedTagNode_retrieves_specified_property_with_value(string propertyName)
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), "1");

            // ACT

            var result = new AssignedTagNode(e, e.Tags.Single()).GetItemProperties(this.ProviderContextMock.Object, propertyNames: new[] { propertyName });

            // ASSERT

            Assert.Single(result);
        }

        [Fact]
        public void AssignedTagNode_rertrieving_property_provides_parameter_with_completer()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);

            // ACT

            var result = (RuntimeDefinedParameterDictionary)new AssignedTagNode(e, e.Tags.Single()).GetItemPropertyParameters;

            // ASSERT

            Assert.True(result.TryGetValue("TreeStorePropertyName", out var parameter));
            Assert.Single(parameter!.Attributes.Where(a => a.GetType().Equals(typeof(ParameterAttribute))));

            var resultValidateSet = (ValidateSetAttribute)parameter!.Attributes.Single(a => a.GetType().Equals(typeof(ValidateSetAttribute)));

            Assert.Equal(new[] { "p", "Name" }, resultValidateSet.ValidValues);
        }

        #endregion IGetItemProperties

        #region ISetItemProperties

        [Theory]
        [InlineData("p")]
        [InlineData("P")]
        public void AssignedTagNode_sets_facet_property_value(string propertyName)
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

            new AssignedTagNode(e, e.Tags.Single())
                .SetItemProperties(this.ProviderContextMock.Object, new PSNoteProperty(propertyName, "2").Yield());

            // ASSERT

            Assert.Equal("2", e.TryGetFacetProperty(e.Tags.Single().Facet.Properties.Single()).value);
        }

        [Fact]
        public void AssignedTagNode_setting_facet_property_value_rejects_wrong_type()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag, e => e.Tags.Single().Facet.Properties.Single().Type = FacetPropertyTypeValues.Bool);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(
                () => new AssignedTagNode(e, e.Tags.Single()).SetItemProperties(this.ProviderContextMock.Object, new PSNoteProperty("p", 2).Yield()));

            // ASSERT

            Assert.NotNull(result);
            Assert.False(e.TryGetFacetProperty(e.Tags.Single().Facet.Properties.Single()).hasValue);
        }

        [Fact]
        public void AssignedTagNode_setting_facet_property_provides_parameter_with_completer()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);

            // ACT

            var result = (RuntimeDefinedParameterDictionary)new AssignedTagNode(e, e.Tags.Single()).SetItemPropertyParameters;

            // ASSERT

            Assert.True(result.TryGetValue("TreeStorePropertyName", out var parameter));
            Assert.Single(parameter!.Attributes.Where(a => a.GetType().Equals(typeof(ParameterAttribute))));

            var resultValidateSet = (ValidateSetAttribute)parameter!.Attributes.Single(a => a.GetType().Equals(typeof(ValidateSetAttribute)));

            Assert.Equal("p", resultValidateSet.ValidValues.Single());
        }

        #endregion ISetItemProperties

        #region IClearItemProperty

        [Theory]
        [InlineData("p")]
        [InlineData("P")]
        public void AssignedTagNode_clears_facet_property_value(string propertyName)
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

            new AssignedTagNode(e, e.Tags.Single())
                .ClearItemProperty(this.ProviderContextMock.Object, propertyName.Yield());

            // ASSERT

            Assert.Empty(e.Values);
        }

        [Fact]
        public void AssignedTagNode_clearing_facet_property_value_ignores_unknown_name()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), "1");

            // ACT

            new AssignedTagNode(e, e.Tags.Single())
                .ClearItemProperty(this.ProviderContextMock.Object, "unknown".Yield());

            // ASSERT

            Assert.Single(e.Values);
        }

        [Fact]
        public void AssignedTagNode_clearing_facet_property_provides_parameter_with_completer()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);

            // ACT

            var result = (RuntimeDefinedParameterDictionary)new AssignedTagNode(e, e.Tags.Single()).ClearItemPropertyParameters;

            // ASSERT

            Assert.True(result.TryGetValue("TreeStorePropertyName", out var parameter));
            Assert.Single(parameter!.Attributes.Where(a => a.GetType().Equals(typeof(ParameterAttribute))));

            var resultValidateSet = (ValidateSetAttribute)parameter!.Attributes.Single(a => a.GetType().Equals(typeof(ValidateSetAttribute)));

            Assert.Equal("p", resultValidateSet.ValidValues.Single());
        }

        #endregion IClearItemProperty

        #region ToFormattedString

        [Fact]
        public void AssignedTagNode_provides_formatted_string_view()
        {
            // ARRANGE

            var e = DefaultEntity(
                e => e.Id = Guid.Parse("4faacbce-d42d-4b3c-9a5f-706533d731ed"),
                WithAssignedTag(DefaultTag(
                    t => t.Name = "long_tag_name",
                    WithDefaultProperty,
                    WithProperty("long_property_name", FacetPropertyTypeValues.Long),
                    WithProperty("no_value", FacetPropertyTypeValues.DateTime)
                )));

            e.SetFacetProperty("long_tag_name", "p", "test");
            e.SetFacetProperty("long_tag_name", "long_property_name", 1);

            var item = (AssignedTagNode.Item)new AssignedTagNode(e, e.Tags.Single()).GetItem(this.ProviderContextMock.Object).ImmediateBaseObject;

            // ACT

            var result = item.ToFormattedString();

            // ASSERT

            Assert.Equal(FormattedEntity, result);
        }

        public string FormattedEntity =>
@"long_tag_name
  p                  : test
  long_property_name : 1
  no_value           : <no value>
";

        #endregion ToFormattedString
    }
}