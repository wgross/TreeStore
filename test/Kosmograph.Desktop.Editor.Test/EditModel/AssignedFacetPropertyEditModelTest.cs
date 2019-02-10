using Kosmograph.Desktop.Editors.ViewModel;
using Kosmograph.Model;
using System.Collections.Generic;
using Xunit;

namespace Kosmograph.Desktop.Editor.Test.ViewModel
{
    public class AssignedFacetPropertyEditModelTest
    {
        private (FacetProperty, Dictionary<string, object>) DefaultFacetProperty()
        {
            var tmp = new FacetProperty("p");
            return (tmp, new Dictionary<string, object> { { tmp.Id.ToString(), 1 } });
        }

        [Fact]
        public void AssigedFacetPropertyEditModel_mirrors_Model()
        {
            // ARRANGE

            var (model, values) = DefaultFacetProperty();

            // ACT

            var result = new AssignedFacetPropertyEditModel(model, values);

            // ASSERT

            Assert.Equal("p", result.Model.Name);
            Assert.Equal(1, result.Value);
        }

        [Fact]
        public void AssigedFacetPropertyEditModel_delays_change_from_Model()
        {
            // ARRANGE

            var (model, values) = DefaultFacetProperty();
            var editModel = new AssignedFacetPropertyEditModel(model, values);

            // ACT

            editModel.Value = "value";

            // ASSERT

            Assert.Equal("value", editModel.Value);
            Assert.Equal(1, values[model.Id.ToString()]);
        }

        [Fact]
        public void AssigedFacetPropertyEditModel_commits_change_to_Model()
        {
            // ARRANGE

            var (model, values) = DefaultFacetProperty();
            var editModel = new AssignedFacetPropertyEditModel(model, values);
            editModel.Value = "value";

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal("value", editModel.Value);
            Assert.Equal(1, values[model.Id.ToString()]);
        }

        [Fact]
        public void AssigedFacetPropertyEditModel_reverts_state_from_Model()
        {
            // ARRANGE

            var (model, values) = DefaultFacetProperty();
            var editModel = new AssignedFacetPropertyEditModel(model, values);
            editModel.Value = "value";

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(1, editModel.Value);
            Assert.Equal(1, values[model.Id.ToString()]);
        }
    }
}