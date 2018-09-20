using Kosmograph.Desktop.EditModel;
using Kosmograph.Desktop.Test.ViewModel;
using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.EditModel
{
    public class RelationshipEditModelTest
    {
        [Fact]
        public void RelationshipEditModel_mirrors_ViewModel()
        {
            // ARRANGE

            var model = new Relationship("r", new Entity(), new Entity(), new Tag());
            var viewModel = new RelationshipViewModel(model, new EntityViewModel(model.From), new EntityViewModel(model.To), model.Tags.Single().ToViewModel());

            // ACT

            var result = new RelationshipEditModel(viewModel, delegate { }, delegate { });

            // ASSERT

            Assert.Equal("r", result.Name);
            Assert.Equal(viewModel.From.Model, result.From.Model);
            Assert.Equal(viewModel.To.Model, result.To.Model);
            Assert.Equal(viewModel.Tags.Single().Tag.Model, result.Tags.Single().ViewModel.Tag.Model);
        }

        [Fact]
        public void RelationshipEditModel_delays_changes_at_ViewModel()
        {
            // ARRANGE

            var entity1 = new EntityViewModel(new Entity());
            var entity2 = new EntityViewModel(new Entity());
            var model = new Relationship("r", new Entity(), new Entity(), new Tag());
            var viewModel = new RelationshipViewModel(model, new EntityViewModel(model.From), new EntityViewModel(model.To));
            var editModel = new RelationshipEditModel(viewModel, delegate { }, delegate { });

            // ACT

            editModel.Name = "changed";
            editModel.From = entity1;
            editModel.To = entity2;

            // ASSERT

            Assert.Equal("changed", editModel.Name);
            Assert.Equal("r", viewModel.Name);
            Assert.Equal("r", model.Name);

            Assert.NotEqual(editModel.From.Model, viewModel.From.Model);
            Assert.NotEqual(editModel.To.Model, viewModel.To.Model);
        }

        [Fact]
        public void RelationshipEditModel_commits_changes_to_ViewModel()
        {
            // ARRANGE

            var entity1 = new EntityViewModel(new Entity());
            var entity2 = new EntityViewModel(new Entity());
            var model = new Relationship("r", new Entity(), new Entity(), new Tag());
            var viewModel = new RelationshipViewModel(model, new EntityViewModel(model.From), new EntityViewModel(model.To));

            Relationship committed = null;
            var commitCB = new Action<Relationship>(r => committed = r);

            Relationship reverted = null;
            var revertCB = new Action<Relationship>(r => reverted = r);

            var editModel = new RelationshipEditModel(viewModel, commitCB, revertCB);

            editModel.Name = "changed";
            editModel.From = entity1;
            editModel.To = entity2;

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(model, committed);
            Assert.Null(reverted);

            Assert.Equal("changed", editModel.Name);
            Assert.Equal("changed", viewModel.Name);
            Assert.Equal("changed", model.Name);

            Assert.Equal(entity1.Model, viewModel.From.Model);
            Assert.Equal(entity1.Model, model.From);
            Assert.Equal(entity2.Model, viewModel.To.Model);
            Assert.Equal(entity2.Model, model.To);
        }

        [Fact]
        public void RelationshipEditModel_revert_changes_from_ViewModel()
        {
            // ARRANGE

            var entity1 = new EntityViewModel(new Entity());
            var entity2 = new EntityViewModel(new Entity());
            var model = new Relationship("r", new Entity(), new Entity(), new Tag());
            var viewModel = new RelationshipViewModel(model, new EntityViewModel(model.From), new EntityViewModel(model.To));

            Relationship reverted = null;
            var revertCB = new Action<Relationship>(r => reverted = r);

            var editModel = new RelationshipEditModel(viewModel, delegate { }, revertCB);

            editModel.Name = "changed";
            editModel.From = entity1;
            editModel.To = entity2;

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(model, reverted);

            Assert.Equal("r", editModel.Name);
            Assert.Equal("r", viewModel.Name);
            Assert.Equal("r", model.Name);

            Assert.Equal(model.From, editModel.From.Model);
            Assert.Equal(model.From, viewModel.From.Model);
            Assert.Equal(model.To, editModel.To.Model);
            Assert.Equal(model.To, viewModel.To.Model);
        }

        [Fact]
        public void RelationshipEditModel_delays_adding_Tag_at_ViewModel()
        {
            // ARRANGE

            var tagViewModel = new TagViewModel(new Tag("t2", Facet.Empty));
            var model = new Relationship("r", new Entity(), new Entity(), new Tag());
            var viewModel = new RelationshipViewModel(model, new EntityViewModel(model.From), new EntityViewModel(model.To), model.Tags.Single().ToViewModel());
            var editModel = new RelationshipEditModel(viewModel, delegate { }, delegate { });

            // ACT

            editModel.AssignTagCommand.Execute(tagViewModel);

            // ASSERT

            Assert.Equal(2, editModel.Tags.Count());
            Assert.Single(viewModel.Tags);
            Assert.Single(model.Tags);
        }

        [Fact]
        public void RelationshipEditModel_commits_added_Tag_to_ViewModel()
        {
            // ARRANGE

            var tagViewModel = new TagViewModel(new Tag("t2", Facet.Empty));
            var model = new Relationship("r", new Entity(), new Entity(), new Tag());
            var viewModel = new RelationshipViewModel(model, new EntityViewModel(model.From), new EntityViewModel(model.To), model.Tags.Single().ToViewModel());
            var editModel = new RelationshipEditModel(viewModel, delegate { }, delegate { });

            editModel.AssignTagCommand.Execute(tagViewModel);

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(2, editModel.Tags.Count());
            Assert.Equal(2, viewModel.Tags.Count());
            Assert.Equal(2, model.Tags.Count());
        }

        [Fact]
        public void RelationshipEditModel_reverts_added_Tag_from_ViewModel()
        {
            // ARRANGE

            var tagViewModel = new TagViewModel(new Tag("t2", Facet.Empty));
            var model = new Relationship("r", new Entity(), new Entity(), new Tag());
            var viewModel = new RelationshipViewModel(model, new EntityViewModel(model.From), new EntityViewModel(model.To), model.Tags.Single().ToViewModel());
            var editModel = new RelationshipEditModel(viewModel, delegate { }, delegate { });

            editModel.AssignTagCommand.Execute(tagViewModel);

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Single(editModel.Tags);
            Assert.Single(viewModel.Tags);
            Assert.Single(model.Tags);
        }

        [Fact]
        public void RelationshipEditModel_delays_removing_Tag_at_ViewModel()
        {
            // ARRANGE

            var tagViewModel = new TagViewModel(new Tag("t2", Facet.Empty));
            var model = new Relationship("r", new Entity(), new Entity(), new Tag());
            var viewModel = new RelationshipViewModel(model, new EntityViewModel(model.From), new EntityViewModel(model.To), model.Tags.Single().ToViewModel());
            var editModel = new RelationshipEditModel(viewModel, delegate { }, delegate { });

            // ACT

            editModel.RemoveTagCommand.Execute(editModel.Tags.Single());

            // ASSERT

            Assert.Empty(editModel.Tags);
            Assert.Single(viewModel.Tags);
            Assert.Single(model.Tags);
        }

        [Fact]
        public void RelationshipEditModel_commits_removed_Tag_to_ViewModel()
        {
            // ARRANGE

            var tagViewModel = new TagViewModel(new Tag("t2", Facet.Empty));
            var model = new Relationship("r", new Entity(), new Entity(), new Tag());
            var viewModel = new RelationshipViewModel(model, new EntityViewModel(model.From), new EntityViewModel(model.To), model.Tags.Single().ToViewModel());
            var editModel = new RelationshipEditModel(viewModel, delegate { }, delegate { });

            editModel.RemoveTagCommand.Execute(editModel.Tags.Single());

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Empty(editModel.Tags);
            Assert.Empty(viewModel.Tags);
            Assert.Empty(model.Tags);
        }

        [Fact]
        public void RelationshipEditModel_reverts_removed_Tag_from_ViewModel()
        {
            // ARRANGE

            var model = new Relationship("r", new Entity(), new Entity(), new Tag());
            var viewModel = new RelationshipViewModel(model, new EntityViewModel(model.From), new EntityViewModel(model.To), model.Tags.Single().ToViewModel());
            var editModel = new RelationshipEditModel(viewModel, delegate { }, delegate { });

            editModel.RemoveTagCommand.Execute(editModel.Tags.Single());

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Single(editModel.Tags);
            Assert.Single(viewModel.Tags);
            Assert.Single(model.Tags);
        }

        [Fact]
        public void RelationshipEditModel_rejects_duplicate_Tags()
        {
            // ARRANGE

            var tagModel = new Tag();
            var model = new Relationship("r", new Entity(), new Entity(), tagModel);
            var viewModel = new RelationshipViewModel(model, new EntityViewModel(model.From), new EntityViewModel(model.To), tagModel.ToViewModel());
            var editModel = new RelationshipEditModel(viewModel, delegate { }, delegate { });

            // ACT

            var result = editModel.AssignTagCommand.CanExecute(new TagViewModel(tagModel));

            // ASSERT

            Assert.False(result);
        }
    }
}