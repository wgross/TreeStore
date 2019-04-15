using GalaSoft.MvvmLight.Messaging;
using Kosmograph.Desktop.Editors.ViewModel;
using Kosmograph.Model;
using System;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Editors.Test.ViewModel
{
    public class EntityEditModelTest
    {
        private Tag DefaultTag(string name = "tag") => new Tag(name, new Facet("f", new FacetProperty("p")));

        private Entity DefaultEntity() => new Entity("entity", DefaultTag());

        private Entity DefaultEntity(Action<Entity> setup)
        {
            var tmp = new Entity("entity", DefaultTag());
            setup?.Invoke(tmp);
            return tmp;
        }

        [Fact]
        public void EntityEditModel_mirrors_Model()
        {
            // ARRANGE

            var model = DefaultEntity(e => e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), 1));

            // ACT

            var editModel = new EntityEditModel(model, delegate { }, delegate { });

            // ASSERT

            Assert.Equal("entity", editModel.Name);
            Assert.Equal("tag", editModel.Tags.ElementAt(0).Model.Name);
            Assert.Equal("p", editModel.Tags.ElementAt(0).Properties.Single().Model.Name);
            Assert.Equal(1, editModel.Tags.ElementAt(0).Properties.Single().Value);
        }

        [Fact]
        public void EntityEditModel_delays_changes_at_Model()
        {
            // ARRANGE

            var model = DefaultEntity();

            model.SetFacetProperty(model.Tags.Single().Facet.Properties.Single(), 1);

            var editModel = new EntityEditModel(model, delegate { }, delegate { });

            // ACT

            editModel.Name = "changed";
            editModel.Tags.Single().Properties.Single().Value = "changed";

            // ASSERT

            Assert.Equal("changed", editModel.Name);
            Assert.Equal("entity", model.Name);
            Assert.Equal("changed", editModel.Tags.Single().Properties.Single().Value);
            Assert.Equal(1, model.Values[model.Tags.Single().Facet.Properties.Single().Id.ToString()]);
        }

        [Fact]
        public void EntityEditModel_commits_changes_to_Model()
        {
            // ARRANGE

            var model = DefaultEntity();
            model.SetFacetProperty(model.Tags.Single().Facet.Properties.Single(), 1);

            Entity committed = null;
            var commitCB = new Action<Entity>(e => committed = e);

            Entity reverted = null;
            var revertCB = new Action<Entity>(e => reverted = e);

            var editModel = new EntityEditModel(model, commitCB, revertCB);

            editModel.Name = "changed";
            editModel.Tags.Single().Properties.Single().Value = "changed";

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(model, committed);
            Assert.Null(reverted);
            Assert.Equal("changed", editModel.Name);
            Assert.Equal("changed", model.Name);
            Assert.Equal("changed", editModel.Tags.Single().Properties.Single().Value);
            Assert.Equal("changed", model.Values[model.Tags.Single().Facet.Properties.Single().Id.ToString()]);
        }

        [Fact]
        public void EntityEditModel_commit_notifies()
        {
            // ARRANGE

            var model = DefaultEntity();

            model.SetFacetProperty(model.Tags.Single().Facet.Properties.Single(), 1);

            var editModel = new EntityEditModel(model, delegate { }, delegate { });

            // ACT

            EditModelCommitted result = null;
            var committedNotification = new Action<EditModelCommitted>(n => result = n);

            Messenger.Default.Register<EditModelCommitted>(this, committedNotification);

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.NotNull(result);
            Assert.Equal(typeof(Entity), result.Model.GetType());
        }

        [Fact]
        public void EntityEditModel_reverts_changes_from_Model()
        {
            // ARRANGE

            var model = DefaultEntity();
            model.SetFacetProperty(model.Tags.Single().Facet.Properties.Single(), 1);

            Entity reverted = null;
            var revertCB = new Action<Entity>(e => reverted = e);

            var editModel = new EntityEditModel(model, delegate { }, revertCB);

            editModel.Name = "changed";
            editModel.Tags.Single().Properties.Single().Value = "changed";

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(model, reverted);
            Assert.Equal("entity", editModel.Name);
            Assert.Equal("entity", model.Name);
            Assert.Equal(1, editModel.Tags.Single().Properties.Single().Value);
            Assert.Equal(1, model.Values[model.Tags.Single().Facet.Properties.Single().Id.ToString()]);
        }

        [Fact]
        public void EntityEditModel_delays_adding_tag_at_Model()
        {
            // ARRANGE

            var tag2 = DefaultTag("tag2");
            var model = DefaultEntity();
            var editModel = new EntityEditModel(model, delegate { }, delegate { });

            // ACT

            editModel.AssignTagCommand.Execute(tag2);

            // ASSERT

            Assert.Equal(2, editModel.Tags.Count());
            Assert.Single(model.Tags);
        }

        [Fact]
        public void EntityEditModel_invalidates_empty_name()
        {
            // ARRANGE

            var editModel = new EntityEditModel(DefaultEntity(), delegate { }, delegate { });

            // ACT

            editModel.Name = string.Empty;
            var result = editModel.CommitCommand.CanExecute(null);

            // ASSERT

            Assert.False(result);
            Assert.True(editModel.HasErrors);
            Assert.Equal("Name must not be empty", editModel.NameError);
        }

        [Fact]
        public void EntityEditModel_commits_adding_tag_at_Model()
        {
            // ARRANGE

            var tag2 = DefaultTag("tag2");
            var model = DefaultEntity();

            Entity committed = null;
            var commitCB = new Action<Entity>(e => committed = e);

            Entity reverted = null;
            var revertCB = new Action<Entity>(e => reverted = e);

            var editModel = new EntityEditModel(model, commitCB, revertCB);

            editModel.AssignTagCommand.Execute(tag2);

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(model, committed);
            Assert.Null(reverted);
            Assert.Equal(new[] { model.Tags.First(), tag2 }, model.Tags);
        }

        [Fact]
        public void EntityEditModel_reverts_adding_tag_at_Model()
        {
            // ARRANGE

            var tagViewModel = DefaultTag("tag2");
            var model = DefaultEntity();

            Entity reverted = null;
            var revertCB = new Action<Entity>(e => reverted = e);

            var editModel = new EntityEditModel(model, delegate { }, revertCB);

            editModel.AssignTagCommand.Execute(tagViewModel);

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(model, reverted);
            Assert.Single(editModel.Tags);
            Assert.Single(model.Tags);
        }

        [Fact]
        public void EntityEditModel_delays_removing_tag_at_Model()
        {
            // ARRANGE

            var model = DefaultEntity();
            var editModel = new EntityEditModel(model, delegate { }, delegate { });

            // ACT

            editModel.RemoveTagCommand.Execute(editModel.Tags.Single());

            // ASSERT

            Assert.Empty(editModel.Tags);
            Assert.Single(model.Tags);
        }

        [Fact]
        public void EntityEditModel_commits_removing_tag_at_Model()
        {
            // ARRANGE

            var model = new Entity("entity", new Tag("tag1", new Facet("f", new FacetProperty("p1"))));

            Entity committed = null;
            var commitCB = new Action<Entity>(e => committed = e);

            Entity reverted = null;
            var revertCB = new Action<Entity>(e => reverted = e);

            var editModel = new EntityEditModel(model, commitCB, revertCB);

            editModel.RemoveTagCommand.Execute(editModel.Tags.Single());

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Null(reverted);
            Assert.Equal(model, committed);
            Assert.Empty(editModel.Tags);
            Assert.Empty(model.Tags);
        }

        [Fact]
        public void EntityEditModel_reverts_removing_tag_from_Model()
        {
            // ARRANGE

            var model = DefaultEntity();
            model.SetFacetProperty(model.Tags.Single().Facet.Properties.Single(), 1);

            Entity reverted = null;
            var revertCB = new Action<Entity>(e => reverted = e);

            var editModel = new EntityEditModel(model, delegate { }, revertCB);

            editModel.RemoveTagCommand.Execute(editModel.Tags.Single());

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(model, reverted);
            Assert.Single(editModel.Tags);
            Assert.Single(model.Tags);
        }

        [Fact]
        public void EntityEditModel_rejects_duplicate_Tags()
        {
            // ARRANGE

            var tag = DefaultTag();
            var model = new Entity("entity", tag);
            var editModel = new EntityEditModel(model, delegate { }, delegate { });

            // ACT

            var result = editModel.AssignTagCommand.CanExecute(tag);

            // ASSERT

            Assert.False(result);
            Assert.Single(editModel.Tags);
            Assert.Single(model.Tags);
        }
    }
}