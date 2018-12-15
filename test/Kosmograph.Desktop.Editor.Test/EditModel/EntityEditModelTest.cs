using GalaSoft.MvvmLight.Messaging;
using Kosmograph.Desktop.EditModel;
using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Editor.Test.EditModel
{
    public class EntityEditModelTest
    {
        private readonly Tag tag;

        public EntityEditModelTest()
        {
            this.tag = new Tag("tag", new Facet("facet", new FacetProperty("p")));
        }

        [Fact]
        public void EntityEditModel_mirrors_ViewModel()
        {
            // ARRANGE

            var model = new Entity("entity", new Tag("tag1", new Facet("f", new FacetProperty("p1"))));
            model.SetFacetProperty(model.Tags.Single().Facet.Properties.Single(), 1);

            var viewModel = new EntityViewModel(model, model.Tags.Single().ToViewModel());

            // ACT

            var editModel = new EntityEditModel(viewModel, delegate { }, delegate { });

            // ASSERT

            Assert.Equal("entity", editModel.Name);
            Assert.Equal("tag1", editModel.Tags.ElementAt(0).ViewModel.Tag.Name);
            Assert.Equal("p1", editModel.Tags.ElementAt(0).Properties.Single().ViewModel.Property.Name);
            Assert.Equal(1, editModel.Tags.ElementAt(0).Properties.Single().Value);
        }

        [Fact]
        public void EntityEditModel_delays_changes_at_ViewModel()
        {
            // ARRANGE

            var model = new Entity("entity", new Tag("tag1", new Facet("f", new FacetProperty("p1"))));
            model.SetFacetProperty(model.Tags.Single().Facet.Properties.Single(), 1);

            var viewModel = new EntityViewModel(model, model.Tags.Single().ToViewModel());
            var editModel = new EntityEditModel(viewModel, delegate { }, delegate { });

            // ACT

            editModel.Name = "changed";
            editModel.Tags.Single().Properties.Single().Value = "changed";

            // ASSERT

            Assert.Equal("changed", editModel.Name);
            Assert.Equal("entity", viewModel.Name);
            Assert.Equal("entity", model.Name);

            Assert.Equal("changed", editModel.Tags.Single().Properties.Single().Value);
            Assert.Equal(1, viewModel.Tags.Single().Properties.Single().Value);
            Assert.Equal(1, model.Values[model.Tags.Single().Facet.Properties.Single().Id.ToString()]);
        }

        [Fact]
        public void EntityEditModel_commits_changes_to_ViewModel()
        {
            // ARRANGE

            var model = new Entity("entity", new Tag("tag1", new Facet("f", new FacetProperty("p1"))));
            model.SetFacetProperty(model.Tags.Single().Facet.Properties.Single(), 1);

            var viewModel = new EntityViewModel(model, model.Tags.Single().ToViewModel());

            Entity committed = null;
            var commitCB = new Action<Entity>(e => committed = e);

            Entity reverted = null;
            var revertCB = new Action<Entity>(e => reverted = e);

            var editModel = new EntityEditModel(viewModel, commitCB, revertCB);

            editModel.Name = "changed";
            editModel.Tags.Single().Properties.Single().Value = "changed";

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(model, committed);
            Assert.Null(reverted);

            Assert.Equal("changed", editModel.Name);
            Assert.Equal("changed", viewModel.Name);
            Assert.Equal("changed", model.Name);

            Assert.Equal("changed", editModel.Tags.Single().Properties.Single().Value);
            Assert.Equal("changed", viewModel.Tags.Single().Properties.Single().Value);
            Assert.Equal("changed", model.Values[model.Tags.Single().Facet.Properties.Single().Id.ToString()]);
        }

        [Fact]
        public void EntityEditModel_commit_notifies()
        {
            // ARRANGE

            var model = new Entity("entity", new Tag("tag1", new Facet("f", new FacetProperty("p1"))));
            model.SetFacetProperty(model.Tags.Single().Facet.Properties.Single(), 1);

            var viewModel = new EntityViewModel(model, model.Tags.Single().ToViewModel());

            var editModel = new EntityEditModel(viewModel, delegate { }, delegate { });

            // ACT

            EditModelCommitted result = null;
            var committedNotification = new Action<EditModelCommitted>(n => result = n);

            Messenger.Default.Register<EditModelCommitted>(this, committedNotification);

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.NotNull(result);
            Assert.Equal(typeof(EntityViewModel), result.ViewModel.GetType());
            Assert.Equal(viewModel, result.TryGetViewModel<EntityViewModel>().Item2);
        }

        [Fact]
        public void EntityEditModel_reverts_changes_from_ViewModel()
        {
            // ARRANGE

            var model = new Entity("entity", new Tag("tag1", new Facet("f", new FacetProperty("p1"))));
            model.SetFacetProperty(model.Tags.Single().Facet.Properties.Single(), 1);

            var viewModel = new EntityViewModel(model, model.Tags.Single().ToViewModel());

            Entity reverted = null;
            var revertCB = new Action<Entity>(e => reverted = e);

            var editModel = new EntityEditModel(viewModel, delegate { }, revertCB);

            editModel.Name = "changed";
            editModel.Tags.Single().Properties.Single().Value = "changed";

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(model, reverted);

            Assert.Equal("entity", editModel.Name);
            Assert.Equal("entity", viewModel.Name);
            Assert.Equal("entity", model.Name);

            Assert.Equal(1, editModel.Tags.Single().Properties.Single().Value);
            Assert.Equal(1, viewModel.Tags.Single().Properties.Single().Value);
            Assert.Equal(1, model.Values[model.Tags.Single().Facet.Properties.Single().Id.ToString()]);
        }

        [Fact]
        public void EntityEditModel_delays_adding_tag_at_ViewModel()
        {
            // ARRANGE

            var tagViewModel = new TagViewModel(new Tag("t2"));

            var model = new Entity("entity");
            var viewModel = new EntityViewModel(model);
            var editModel = new EntityEditModel(viewModel, delegate { }, delegate { });

            // ACT

            editModel.AssignTagCommand.Execute(tagViewModel);

            // ASSERT

            Assert.Single(editModel.Tags);
            Assert.Empty(viewModel.Tags);
            Assert.Empty(model.Tags);
        }

        [Fact]
        public void EntityEditModel_invalidates_empty_name()
        {
            // ARRANGE

            var tagViewModel = new TagViewModel(new Tag("t2"));

            var model = new Entity("entity");
            var viewModel = new EntityViewModel(model);
            var editModel = new EntityEditModel(viewModel, delegate { }, delegate { });

            // ACT

            editModel.Name = string.Empty;
            var result = editModel.CommitCommand.CanExecute(null);

            // ASSERT

            Assert.False(result);
            Assert.True(editModel.HasErrors);
            Assert.Equal("Name must not be empty", editModel.NameError);
        }

        [Fact]
        public void EntityEditModel_commits_adding_tag_at_ViewModel()
        {
            // ARRANGE

            var tagViewModel = new TagViewModel(new Tag("t2"));

            var model = new Entity("entity");
            var viewModel = new EntityViewModel(model);

            Entity committed = null;
            var commitCB = new Action<Entity>(e => committed = e);

            Entity reverted = null;
            var revertCB = new Action<Entity>(e => reverted = e);

            var editModel = new EntityEditModel(viewModel, commitCB, revertCB);

            editModel.AssignTagCommand.Execute(tagViewModel);

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(model, committed);
            Assert.Null(reverted);

            Assert.Single(editModel.Tags);
            Assert.Single(viewModel.Tags);
            Assert.Single(model.Tags);
        }

        [Fact]
        public void EntityEditModel_reverts_adding_tag_at_ViewModel()
        {
            // ARRANGE

            var tagViewModel = new TagViewModel(new Tag("t2"));

            var model = new Entity("entity");
            var viewModel = new EntityViewModel(model);

            Entity reverted = null;
            var revertCB = new Action<Entity>(e => reverted = e);

            var editModel = new EntityEditModel(viewModel, delegate { }, revertCB);

            editModel.AssignTagCommand.Execute(tagViewModel);

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(model, reverted);

            Assert.Empty(editModel.Tags);
            Assert.Empty(viewModel.Tags);
            Assert.Empty(model.Tags);
        }

        [Fact]
        public void EntityEditModel_delays_removing_tag_at_ViewModel()
        {
            // ARRANGE

            var model = new Entity("entity", new Tag("tag1", new Facet("f", new FacetProperty("p1"))));
            var viewModel = new EntityViewModel(model, model.Tags.Single().ToViewModel());
            var editModel = new EntityEditModel(viewModel, delegate { }, delegate { });

            // ACT

            editModel.RemoveTagCommand.Execute(editModel.Tags.Single());

            // ASSERT

            Assert.Empty(editModel.Tags);
            Assert.Single(viewModel.Tags);
            Assert.Single(model.Tags);
        }

        [Fact]
        public void EntityEditModel_commits_removing_tag_at_ViewModel()
        {
            // ARRANGE

            var model = new Entity("entity", new Tag("tag1", new Facet("f", new FacetProperty("p1"))));
            var viewModel = new EntityViewModel(model, model.Tags.Single().ToViewModel());

            Entity committed = null;
            var commitCB = new Action<Entity>(e => committed = e);

            Entity reverted = null;
            var revertCB = new Action<Entity>(e => reverted = e);

            var editModel = new EntityEditModel(viewModel, commitCB, revertCB);

            editModel.RemoveTagCommand.Execute(editModel.Tags.Single());

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Null(reverted);
            Assert.Equal(model, committed);

            Assert.Empty(editModel.Tags);
            Assert.Empty(viewModel.Tags);
            Assert.Empty(model.Tags);
        }

        [Fact]
        public void EntityEditModel_reverts_removing_tag_from_ViewModel()
        {
            // ARRANGE

            var model = new Entity("entity", new Tag("tag1", new Facet("f", new FacetProperty("p1"))));
            model.SetFacetProperty(model.Tags.Single().Facet.Properties.Single(), 1);

            var viewModel = new EntityViewModel(model, model.Tags.Single().ToViewModel());

            Entity reverted = null;
            var revertCB = new Action<Entity>(e => reverted = e);

            var editModel = new EntityEditModel(viewModel, delegate { }, revertCB);

            editModel.RemoveTagCommand.Execute(editModel.Tags.Single());

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(model, reverted);

            Assert.Single(editModel.Tags);
            Assert.Single(viewModel.Tags);
            Assert.Single(model.Tags);
        }

        [Fact]
        public void EntityEditModel_rejects_duplicate_Tags()
        {
            // ARRANGE

            var tag1 = new Tag("tag1", new Facet("f", new FacetProperty("p1")));
            var model = new Entity("entity", tag1);
            var viewModel = new EntityViewModel(model, model.Tags.Single().ToViewModel());
            var editModel = new EntityEditModel(viewModel, delegate { }, delegate { });

            // ACT

            var result = editModel.AssignTagCommand.CanExecute(tag1);

            // ASSERT

            Assert.False(result);
            Assert.Single(editModel.Tags);
            Assert.Single(model.Tags);
        }
    }
}