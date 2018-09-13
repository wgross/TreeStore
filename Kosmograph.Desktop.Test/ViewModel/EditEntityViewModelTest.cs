using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class EditEntityViewModelTest
    {
        private readonly Tag tag;
        private readonly EditTagViewModel editTag;

        public EditEntityViewModelTest()
        {
            this.tag = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            this.editTag = new EditTagViewModel(this.tag, delegate { });
        }

        [Fact]
        public void EditEntitViewModel_mirrors_Entity()
        {
            // ARRANGE

            var tag1 = new Tag("tag1", new Facet("f", new FacetProperty("p1")));
            var tag2 = new Tag("tag2", new Facet("f", new FacetProperty("p2")));
            var entity = new Entity("entity", tag1, tag2);

            entity.SetFacetProperty(tag1.Facet.Properties.Single(), 1);

            // ACT

            var editEntity = new EditEntityViewModel(entity, delegate { });

            // ASSERT

            Assert.Equal("entity", editEntity.Name);
            Assert.Equal("tag1", editEntity.Tags.ElementAt(0).Name);
            Assert.Equal("tag2", editEntity.Tags.ElementAt(1).Name);
            Assert.Equal("p1", editEntity.Tags.ElementAt(0).Properties.Single().Name);
            Assert.Equal("p2", editEntity.Tags.ElementAt(1).Properties.Single().Name);
            Assert.Equal(1, editEntity.Tags.ElementAt(0).Properties.Single().Value);
            Assert.Null(editEntity.Tags.ElementAt(1).Properties.Single().Value);
        }

        [Fact]
        public void EditEntityViewModel_delays_changes_to_model()
        {
            // ARRANGE

            var tag1 = new Tag("tag1", new Facet("f", new FacetProperty("p1")));
            var tag2 = new Tag("tag2", new Facet("f", new FacetProperty("p2")));
            var tag3 = new Tag("tag3", new Facet("f", new FacetProperty("p3")));
            var entity = new Entity("entity", tag1, tag2);

            entity.SetFacetProperty(tag1.Facet.Properties.Single(), 1);

            var editEntity = new EditEntityViewModel(entity, delegate { });

            // ACT

            editEntity.Name = "changed";
            editEntity.Tags.ElementAt(0).Properties.Single().Value = 2;
            editEntity.Tags.ElementAt(1).Properties.Single().Value = "value";
            editEntity.AssignTagCommand.Execute(tag3);
            editEntity.RemoveTagCommand.Execute(editEntity.Tags.First());

            // ASSERT

            Assert.Equal("changed", editEntity.Name);
            Assert.Equal("tag2", editEntity.Tags.ElementAt(0).Name);
            Assert.Equal("tag3", editEntity.Tags.ElementAt(1).Name);

            Assert.Equal("value", editEntity.Tags.ElementAt(0).Properties.Single().Value);
            Assert.Null(editEntity.Tags.ElementAt(1).Properties.Single().Value);

            Assert.Equal("entity", editEntity.Model.Name);
            Assert.Equal("tag1", editEntity.Model.Tags.ElementAt(0).Name);
            Assert.Equal("tag2", editEntity.Model.Tags.ElementAt(1).Name);

            Assert.Equal(1, editEntity.Model.TryGetFacetProperty(tag1.Facet.Properties.Single()).Item2);
            Assert.Null(editEntity.Model.TryGetFacetProperty(tag2.Facet.Properties.Single()).Item2);
            Assert.Null(editEntity.Model.TryGetFacetProperty(tag3.Facet.Properties.Single()).Item2);
        }

        [Fact]
        public void EditEntityViewModel_commits_changes_to_model()
        {
            // ARRANGE

            var tag1 = new Tag("tag1", new Facet("f", new FacetProperty("p1")));
            var tag2 = new Tag("tag2", new Facet("f", new FacetProperty("p2")));
            var tag3 = new Tag("tag3", new Facet("f", new FacetProperty("p3")));
            var entity = new Entity("entity", tag1, tag2);

            entity.SetFacetProperty(tag1.Facet.Properties.Single(), 1);

            var editEntity = new EditEntityViewModel(entity, delegate { });

            editEntity.Name = "changed";
            editEntity.Tags.ElementAt(0).Properties.Single().Value = 2;
            editEntity.Tags.ElementAt(1).Properties.Single().Value = "value";
            editEntity.AssignTagCommand.Execute(tag3);
            editEntity.RemoveTagCommand.Execute(editEntity.Tags.First());

            // ACT

            editEntity.Commit();

            // ASSERT

            Assert.Equal("changed", editEntity.Name);
            Assert.Equal("tag2", editEntity.Tags.ElementAt(0).Name);
            Assert.Equal("tag3", editEntity.Tags.ElementAt(1).Name);

            Assert.Equal("value", editEntity.Tags.ElementAt(0).Properties.Single().Value);
            Assert.Null(editEntity.Tags.ElementAt(1).Properties.Single().Value);

            Assert.Equal("changed", editEntity.Model.Name);
            Assert.Equal("tag2", editEntity.Model.Tags.ElementAt(0).Name);
            Assert.Equal("tag3", editEntity.Model.Tags.ElementAt(1).Name);

            Assert.Null(editEntity.Model.TryGetFacetProperty(tag1.Facet.Properties.Single()).Item2);
            Assert.Equal("value", editEntity.Model.TryGetFacetProperty(tag2.Facet.Properties.Single()).Item2);
            Assert.Null(editEntity.Model.TryGetFacetProperty(tag3.Facet.Properties.Single()).Item2);
        }

        [Fact]
        public void EditEntityViewModel_reverts_changes_at_view_model()
        {
            // ARRANGE

            var tag1 = new Tag("tag1", new Facet("f", new FacetProperty("p1")));
            var tag2 = new Tag("tag2", new Facet("f", new FacetProperty("p2")));
            var tag3 = new Tag("tag3", new Facet("f", new FacetProperty("p3")));
            var entity = new Entity("entity", tag1, tag2);

            entity.SetFacetProperty(tag1.Facet.Properties.Single(), 1);

            var editEntity = new EditEntityViewModel(entity, delegate { });

            editEntity.Name = "changed";
            editEntity.Tags.ElementAt(0).Properties.Single().Value = 2;
            editEntity.Tags.ElementAt(1).Properties.Single().Value = "value";
            editEntity.AssignTagCommand.Execute(tag3);
            editEntity.RemoveTagCommand.Execute(editEntity.Tags.First());

            // ACT

            editEntity.Rollback();

            // ASSERT

            Assert.Equal("entity", editEntity.Name);
            Assert.Equal("tag1", editEntity.Tags.ElementAt(1).Name);
            Assert.Equal("tag2", editEntity.Tags.ElementAt(0).Name);

            Assert.Equal(1, editEntity.Tags.ElementAt(1).Properties.Single().Value);
            Assert.Null(editEntity.Tags.ElementAt(0).Properties.Single().Value);

            Assert.Equal("tag1", editEntity.Model.Tags.ElementAt(0).Name);
            Assert.Equal("tag2", editEntity.Model.Tags.ElementAt(1).Name);

            Assert.Equal(1, editEntity.Model.TryGetFacetProperty(tag1.Facet.Properties.Single()).Item2);
            Assert.Null(editEntity.Model.TryGetFacetProperty(tag2.Facet.Properties.Single()).Item2);
        }
    }
}