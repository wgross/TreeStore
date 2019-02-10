using GalaSoft.MvvmLight.Messaging;
using Kosmograph.Desktop.Editors.ViewModel;
using Kosmograph.Model;
using System;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Editors.Test.ViewModel
{
    public class RelationshipEditModelTest
    {
        private Tag DefaultTag() => new Tag("tag");

        private Entity DefaultEntity() => new Entity();

        private Entity DefaultEntity(string name) => new Entity(name);

        private Relationship DefaultRelationship() => new Relationship("r", DefaultEntity("entity1"), DefaultEntity("entity2"), DefaultTag());

        [Fact]
        public void RelationshipEditModel_mirrors_Model()
        {
            // ARRANGE

            var model = new Relationship("r", new Entity(), new Entity(), new Tag());

            // ACT

            var result = new RelationshipEditModel(model, delegate { }, delegate { });

            // ASSERT

            Assert.Equal("r", result.Name);
            Assert.Equal(model.From, result.From);
            Assert.Equal(model.To, result.To);
            Assert.Equal(model.Tags.Single(), result.Tags.Single().Model);
        }

        [Fact]
        public void RelationshipEditModel_delays_changes_at_Model()
        {
            // ARRANGE

            var model = DefaultRelationship();
            var editModel = new RelationshipEditModel(model, delegate { }, delegate { });

            // ACT

            editModel.Name = "changed";
            editModel.From = model.From;
            editModel.To = model.To;

            // ASSERT

            Assert.Equal("changed", editModel.Name);
            Assert.Equal("r", model.Name);
            Assert.NotEqual(model.From, editModel.From);
            Assert.NotEqual(model.To, editModel.To);
        }

        [Fact]
        public void RelationshipEditModel_commits_changes_to_Model()
        {
            // ARRANGE

            var model = DefaultRelationship();

            Relationship committed = null;
            var commitCB = new Action<Relationship>(r => committed = r);

            Relationship reverted = null;
            var revertCB = new Action<Relationship>(r => reverted = r);

            var editModel = new RelationshipEditModel(model, commitCB, revertCB);

            editModel.Name = "changed";
            editModel.From = DefaultEntity();
            editModel.To = DefaultEntity();

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(model, committed);
            Assert.Null(reverted);

            Assert.Equal("changed", editModel.Name);
            Assert.Equal("changed", model.Name);
            Assert.Empty(model.From.Name);
            Assert.Empty(model.To.Name);
        }

        [Fact]
        public void RelationshipEditModel_commit_notifies()
        {
            // ARRANGE

            var model = DefaultRelationship();
            var editModel = new RelationshipEditModel(model, delegate { }, delegate { });

            // ACT

            EditModelCommitted result = null;
            var committedNotification = new Action<EditModelCommitted>(n => result = n);

            Messenger.Default.Register<EditModelCommitted>(this, committedNotification);

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.NotNull(result);
            Assert.Equal(typeof(RelationshipViewModel), result.ViewModel.GetType());
        }

        [Fact]
        public void RelationshipEditModel_revert_changes_from_Model()
        {
            // ARRANGE

            var model = DefaultRelationship();

            Relationship reverted = null;
            var revertCB = new Action<Relationship>(r => reverted = r);

            var editModel = new RelationshipEditModel(model, delegate { }, revertCB);

            editModel.Name = "changed";
            editModel.From = DefaultEntity();
            editModel.To = DefaultEntity();

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(model, reverted);
            Assert.Equal("r", editModel.Name);
            Assert.Equal("r", model.Name);
            Assert.Equal("entity1", editModel.From.Name);
            Assert.Equal("entity2", editModel.To.Name);
        }

        [Fact]
        public void RelationshipEditModel_cant_commit_with_a_null_entity()
        {
            // ARRANGE

            var model = DefaultRelationship();

            Relationship reverted = null;
            var revertCB = new Action<Relationship>(r => reverted = r);

            var editModel = new RelationshipEditModel(model, delegate { }, revertCB);

            editModel.To = null;

            // ACT

            var result = editModel.CommitCommand.CanExecute(null);

            // ASSERT

            Assert.False(result);
        }

        [Fact]
        public void RelationshipEditModel_delays_adding_Tag_at_Model()
        {
            // ARRANGE

            var tagViewModel = new TagViewModel(new Tag("t2"));
            var model = DefaultRelationship();
            var editModel = new RelationshipEditModel(model, delegate { }, delegate { });

            // ACT

            editModel.AssignTagCommand.Execute(tagViewModel);

            // ASSERT

            Assert.Equal(2, editModel.Tags.Count());
            Assert.Single(model.Tags);
        }

        [Fact]
        public void RelationshipEditModel_commits_added_Tag_to_Model()
        {
            // ARRANGE

            var tagViewModel = new TagViewModel(new Tag("t2"));
            var model = DefaultRelationship();
            var editModel = new RelationshipEditModel(model, delegate { }, delegate { });

            editModel.AssignTagCommand.Execute(tagViewModel);

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(2, editModel.Tags.Count());
            Assert.Equal(2, model.Tags.Count());
        }

        [Fact]
        public void RelationshipEditModel_reverts_added_Tag_from_Model()
        {
            // ARRANGE

            var tagViewModel = new TagViewModel(new Tag("t2"));
            var model = DefaultRelationship();
            var editModel = new RelationshipEditModel(model, delegate { }, delegate { });

            editModel.AssignTagCommand.Execute(tagViewModel);

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Single(editModel.Tags);
            Assert.Single(model.Tags);
        }

        [Fact]
        public void RelationshipEditModel_delays_removing_Tag_at_Model()
        {
            // ARRANGE

            var tagViewModel = new TagViewModel(new Tag("t2"));
            var model = DefaultRelationship();
            var editModel = new RelationshipEditModel(model, delegate { }, delegate { });

            // ACT

            editModel.RemoveTagCommand.Execute(editModel.Tags.Single());

            // ASSERT

            Assert.Empty(editModel.Tags);
            Assert.Single(model.Tags);
        }

        [Fact]
        public void RelationshipEditModel_commits_removed_Tag_to_Model()
        {
            // ARRANGE

            var tagViewModel = new TagViewModel(new Tag("t2"));
            var model = DefaultRelationship();
            var editModel = new RelationshipEditModel(model, delegate { }, delegate { });

            editModel.RemoveTagCommand.Execute(editModel.Tags.Single());

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Empty(editModel.Tags);
            Assert.Empty(model.Tags);
        }

        [Fact]
        public void RelationshipEditModel_reverts_removed_Tag_from_Model()
        {
            // ARRANGE

            var model = DefaultRelationship();
            var editModel = new RelationshipEditModel(model, delegate { }, delegate { });

            editModel.RemoveTagCommand.Execute(editModel.Tags.Single());

            // ACT

            editModel.RollbackCommand.Execute(null);
            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Single(editModel.Tags);
            Assert.Single(model.Tags);
        }

        [Fact]
        public void RelationshipEditModel_rejects_duplicate_Tags()
        {
            // ARRANGE

            var tagModel = new Tag();
            var model = DefaultRelationship();
            var editModel = new RelationshipEditModel(model, delegate { }, delegate { });

            // ACT

            var result = editModel.AssignTagCommand.CanExecute(new TagViewModel(tagModel));

            // ASSERT

            Assert.False(result);
        }
    }
}
;