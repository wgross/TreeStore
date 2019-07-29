using Kosmograph.Desktop.Editors.ViewModel;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Editors.Test.ViewModel
{
    public class AssignedTagEditModelTest
    {
        private Tag DefaultTag(Action<Tag> setup = null)
        {
            var tmp = new Tag("t", new Facet("f", new FacetProperty("p")));
            setup?.Invoke(tmp);
            return tmp;
        }

        private (Tag, Dictionary<string, object>) DefaultAssignedTag()
        {
            var tmp = DefaultTag();
            return (tmp, new Dictionary<string, object>
            {
                { tmp.Facet.Properties.Single().Id.ToString(), 1 }
            });
        }

        private (Tag, Dictionary<string, object>) DefaultAssignedTag(Tag tag)
        {
            return (tag, new Dictionary<string, object>
            {
                { tag.Facet.Properties.Single().Id.ToString(), 1 }
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
        public void AssigedTagEditModel_cant_commit_with_invalid_property_value()
        {
            // ARRANGE

            var (model, values) = DefaultAssignedTag(DefaultTag(t => t.Facet.Properties.Single().Type = FacetPropertyTypeValues.Bool));
            var editModel = new AssignedTagEditModel(model, values);

            editModel.Properties.ElementAt(0).Value = "v"; // assign invalid value

            // ACT

            var result = editModel.CommitCommand.CanExecute(null);

            // ASSERT

            Assert.False(result);
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