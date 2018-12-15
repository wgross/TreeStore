using Kosmograph.Desktop.EditModel;
using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Editor.Test.EditModel
{
    public class DeleteEntityWithRelationshipsEditModelTest
    {
        [Fact]
        public void DeleteEntityWithRelationshipsEditModel_shows_Entity_and_Relationship()
        {
            // ARRANGE

            var entity1 = new Entity().ToViewModel();
            var entity2 = new Entity().ToViewModel();
            var relationship = new RelationshipViewModel(new Relationship(), entity1, entity2);

            // ACT

            var result = new DeleteEntityWithRelationshipsEditModel(entity1, relationship.Yield(), delegate { }, delegate { });

            // ASSERT

            Assert.Equal(entity1, result.Entity);
            Assert.Equal(relationship, result.Relationships.Single());
        }

        [Fact]
        public void DeleteEntityWithRelationshipsEditModel_commits_deletion_of_Entity_and_Relationship()
        {
            // ARRANGE

            var entity1 = new Entity().ToViewModel();
            var entity2 = new Entity().ToViewModel();
            var relationship = new RelationshipViewModel(new Relationship(), entity1, entity2);

            (EntityViewModel, IEnumerable<RelationshipViewModel>)? committed = null;
            var commitCB = new Action<EntityViewModel, IEnumerable<RelationshipViewModel>>((e, r) => committed = (e, r));
            var editModel = new DeleteEntityWithRelationshipsEditModel(entity1, relationship.Yield(), commitCB, delegate { });

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(entity1, committed.Value.Item1);
            Assert.Equal(relationship.Yield(), committed.Value.Item2);
        }

        [Fact]
        public void DeleteEntityWithRelationshipsEditModel_reverts_deletion_of_Entity_and_Relationship()
        {
            // ARRANGE

            var entity1 = new Entity().ToViewModel();
            var entity2 = new Entity().ToViewModel();
            var relationship = new RelationshipViewModel(new Relationship(), entity1, entity2);

            (EntityViewModel, IEnumerable<RelationshipViewModel>)? rolledback = null;
            var rollbackCb = new Action<EntityViewModel, IEnumerable<RelationshipViewModel>>((e, r) => rolledback = (e, r));
            var editModel = new DeleteEntityWithRelationshipsEditModel(entity1, relationship.Yield(), delegate { }, rollbackCb);

            // ACT

            editModel.RollbackCommand.Execute(null);

            // ASSERT

            Assert.Equal(entity1, rolledback.Value.Item1);
            Assert.Equal(relationship.Yield(), rolledback.Value.Item2);
        }
    }
}