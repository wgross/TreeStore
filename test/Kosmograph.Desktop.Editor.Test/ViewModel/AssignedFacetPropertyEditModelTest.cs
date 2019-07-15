using Kosmograph.Desktop.Editors.ViewModel;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using Xunit;

namespace Kosmograph.Desktop.Editor.Test.ViewModel
{
    public class AssignedFacetPropertyEditModelTest
    {
        private FacetProperty DefaultFacetProperty(Action<FacetProperty> setup = null)
        {
            var tmp = new FacetProperty("p");
            setup?.Invoke(tmp);
            return tmp;
        }

        private (FacetProperty model, Dictionary<string, object> values) DefaultAssignedFacetPropertyValue(FacetProperty p = null)
        {
            var tmp = p ?? DefaultFacetProperty();
            return (tmp, new Dictionary<string, object> { { tmp.Id.ToString(), "string" } });
        }

        [Fact]
        public void AssigedFacetPropertyEditModel_mirrors_Model()
        {
            // ARRANGE

            var (model, values) = DefaultAssignedFacetPropertyValue();

            // ACT

            var result = new AssignedFacetPropertyEditModel(model, values);

            // ASSERT

            Assert.Equal("p", result.Model.Name);
            Assert.Equal("string", result.Value);
        }

        [Fact]
        public void AssigedFacetPropertyEditModel_delays_change_from_Model()
        {
            // ARRANGE

            var (model, values) = DefaultAssignedFacetPropertyValue();
            var editModel = new AssignedFacetPropertyEditModel(model, values);

            // ACT

            editModel.Value = "value";

            // ASSERT

            Assert.Equal("value", editModel.Value);
            Assert.Equal("string", values[model.Id.ToString()]);
        }

        [Fact]
        public void AssigedFacetPropertyEditModel_invalidates_incompatible_value()
        {
            // ARRANGE

            var (model, values) = DefaultAssignedFacetPropertyValue(DefaultFacetProperty(p => p.Type = FacetPropertyTypeValues.Bool));
            var editModel = new AssignedFacetPropertyEditModel(model, values);

            // ACT
            // not parsable to bool

            editModel.Value = "value";
            var result = editModel.CommitCommand.CanExecute(null);

            // ASSERT

            Assert.False(result);
            Assert.Equal("value", editModel.Value);
            Assert.Equal("Value must be of type 'Bool'", editModel.ValueError);
            Assert.Equal("string", values[model.Id.ToString()]);
        }

        [Fact]
        public void AssigedFacetPropertyEditModel_commits_change_to_Model()
        {
            // ARRANGE

            var (model, values) = DefaultAssignedFacetPropertyValue();
            var editModel = new AssignedFacetPropertyEditModel(model, values);
            editModel.Value = "value";

            // ACT

            // var result = editModel.CommitCommand;

            var result = editModel.CommitCommand.CanExecute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.True(result);
            Assert.False(editModel.HasErrors);
            Assert.True(string.IsNullOrEmpty(editModel.ValueError));
            Assert.Equal("value", editModel.Value);
            Assert.Equal("value", values[model.Id.ToString()]);
        }

        [Fact]
        public void AssigedFacetPropertyEditModel_reverts_state_from_Model()
        {
            // ARRANGE

            var (model, values) = DefaultAssignedFacetPropertyValue();
            var editModel = new AssignedFacetPropertyEditModel(model, values);
            editModel.Value = "value";

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal("string", editModel.Value);
            Assert.Equal("string", values[model.Id.ToString()]);
        }
    }
}