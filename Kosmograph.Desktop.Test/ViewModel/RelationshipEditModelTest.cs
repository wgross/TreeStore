using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class RelationshipEditModelTest
    {
        [Fact]
        public void RelationshipEditModel_mirrors_RelationshipViewModel()
        {
            // ARRANGE

            var relationshipViewModel = new RelationshipViewModel(
                    new Relationship("r", new Entity(), new Entity(), new Tag()));

            // ACT

            var result = new RelationshipEditModel(relationshipViewModel, delegate { }, delegate { });

            // ASSERT

            Assert.Equal("r", result.Name);
            Assert.Equal(relationshipViewModel.From.Model, result.From.Model);
            Assert.Equal(relationshipViewModel.To.Model, result.To.Model);
            Assert.Equal(relationshipViewModel.Tags.Single().Model, result.Tags.Single().Model);
        }

        [Fact]
        public void RelationshipEditModel_delays_changes_to_RelationshipViewModel()
        {
            // ARRANGE

            var from = new Entity("e1");
            var to = new Entity("e2");
            var relationshipViewModel = new RelationshipViewModel(
                new Relationship("r", from, to, new Tag()));

            var fromChanged = new EntityViewModel(new Entity("e3"));
            var toChanged = new EntityViewModel(new Entity("e4"));

            bool rollbackCbCalled = false;
            var rollbackCB = new Action<Relationship>(r => rollbackCbCalled = true);

            Relationship commited = null;
            var commitCB = new Action<Relationship>(r => commited = r);

            var editRelationship = new RelationshipEditModel(relationshipViewModel, commitCB, rollbackCB);

            // ACT

            editRelationship.Name = "changed";
            editRelationship.From = fromChanged;
            editRelationship.To = toChanged;

            // ASSERT

            Assert.Null(commited);
            Assert.False(rollbackCbCalled);

            Assert.Equal("changed", editRelationship.Name);
            Assert.Equal("r", editRelationship.ViewModel.Name);
            Assert.Equal("r", editRelationship.ViewModel.Model.Name);

            Assert.Equal(fromChanged, editRelationship.From);
            Assert.Equal(from, editRelationship.ViewModel.From.Model);

            Assert.Equal(toChanged, editRelationship.To);
            Assert.Equal(to, editRelationship.ViewModel.Model.To);
        }

        [Fact]
        public void RelationshipEditModel_commits_changes_to_RelationshipViewModel()
        {
            // ARRANGE

            var relationshipViewModel = new RelationshipViewModel(
                new Relationship("r", new Entity("e1"), new Entity("e2"), new Tag()));

            var fromChanged = new EntityViewModel(new Entity("e3"));
            var toChanged = new EntityViewModel(new Entity("e4"));

            bool rollbackCbCalled = false;
            var rollbackCB = new Action<Relationship>(r => rollbackCbCalled = true);

            Relationship commited = null;
            var commitCB = new Action<Relationship>(r => commited = r);

            var editRelationship = new RelationshipEditModel(relationshipViewModel, commitCB, rollbackCB);

            editRelationship.Name = "changed";
            editRelationship.From = fromChanged;
            editRelationship.To = toChanged;

            // ACT

            editRelationship.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(relationshipViewModel.Model, commited);
            Assert.False(rollbackCbCalled);

            Assert.Equal("changed", editRelationship.Name);
            Assert.Equal("changed", editRelationship.ViewModel.Name);
            Assert.Equal("changed", editRelationship.ViewModel.Model.Name);

            Assert.Equal(fromChanged, editRelationship.From);
            Assert.Equal(fromChanged, editRelationship.ViewModel.From);
            Assert.Equal(fromChanged.Model, editRelationship.ViewModel.Model.From);

            Assert.Equal(toChanged, editRelationship.To);
            Assert.Equal(toChanged, editRelationship.ViewModel.To);
            Assert.Equal(toChanged.Model, editRelationship.ViewModel.Model.To);
        }
    }
}
;