using Kosmograph.Desktop.Editors.ViewModel;
using Kosmograph.Model;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Editors.Test.ViewModel
{
    public class AssignedTagEditModelTest
    {
        private Tag DefaultTag() => new Tag("t", new Facet("f", new FacetProperty("p")));

        private (Tag, Dictionary<string, object>) DefaultAssignedTag()
        {
            var tmp = DefaultTag();
            return (tmp, new Dictionary<string, object>
            {
                { tmp.Facet.Properties.Single().Id.ToString(), 1 }
            });
        }

        [Fact]
        public void AssigedTagEditModel_mirrors_Model()
        {
            // ARRANGE

            var (model, values) = DefaultAssignedTag();

            // ACT

            var result = new AssignedTagEditModel(model, values);

            // ASSERT

            Assert.Equal(model, result.Model);
            Assert.Equal(1, result.Properties.Single().Value);
        }

        [Fact]
        public void AssigedTagEditModel_delays_changes_to_Model()
        {
            // ARRANGE

            var (model, values) = DefaultAssignedTag();
            var editModel = new AssignedTagEditModel(model, values);

            // ACT

            editModel.Properties.Single().Value = "changed";

            // ASSERT

            Assert.Equal("changed", editModel.Properties[0].Value);
            Assert.Equal(1, values[model.Facet.Properties.Single().Id.ToString()]);
        }

        [Fact]
        public void AssigedTagEditModel_commits_changes_to_Model()
        {
            // ARRANGE

            var (model, values) = DefaultAssignedTag();
            var editModel = new AssignedTagEditModel(model, values);

            editModel.Properties.Single().Value = "changed";

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal("changed", editModel.Properties[0].Value);
            Assert.Equal("changed", values[model.Facet.Properties.Single().Id.ToString()]);
        }

        [Fact]
        public void AssigedTagEditModel_reverts_changes_from_Model()
        {
            // ARRANGE

            var (model, values) = DefaultAssignedTag();
            var editModel = new AssignedTagEditModel(model, values);

            editModel.Properties.Single().Value = "changed";

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(1, editModel.Properties[0].Value);
            Assert.Equal(1, values[model.Facet.Properties.Single().Id.ToString()]);
        }
    }
}