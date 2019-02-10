using Kosmograph.Desktop.Editor.Test;
using Kosmograph.Desktop.Editors.ViewModel;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Editors.Test.ViewModel
{
    public class DeleteEntityWithRelationshipsEditModelTest
    {
        private Entity DefaultEntity(string name) => new Entity(name);

        private Relationship DefaultRelationship() => new Relationship("relationship", DefaultEntity("from"), DefaultEntity("to"));

        [Fact]
        public void DeleteEntityWithRelationshipsEditModel_shows_Entity_and_Relationship()
        {
            // ARRANGE

            var relationship = DefaultRelationship();

            // ACT

            var result = new DeleteEntityWithRelationshipsEditModel(relationship.From, relationship.Yield(), delegate { }, delegate { });

            // ASSERT

            Assert.Equal(relationship.From, result.Entity);
            Assert.Equal(relationship, result.Relationships.Single());
        }

        [Fact]
        public void DeleteEntityWithRelationshipsEditModel_commits_deletion_of_Entity_and_Relationship()
        {
            // ARRANGE

            var relationship = DefaultRelationship();

            (Entity, IEnumerable<Relationship>)? committed = null;
            var commitCB = new Action<Entity, IEnumerable<Relationship>>((e, r) => committed = (e, r));
            var editModel = new DeleteEntityWithRelationshipsEditModel(relationship.From, relationship.Yield(), commitCB, delegate { });

            // ACT

            editModel.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(relationship.From, committed.Value.Item1);
            Assert.Equal(relationship.Yield(), committed.Value.Item2);
        }

        [Fact]
        public void DeleteEntityWithRelationshipsEditModel_reverts_deletion_of_Entity_and_Relationship()
        {
            // ARRANGE

            var relationship = DefaultRelationship();

            (Entity, IEnumerable<Relationship>)? rolledback = null;
            var rollbackCb = new Action<Entity, IEnumerable<Relationship>>((e, r) => rolledback = (e, r));
            var editModel = new DeleteEntityWithRelationshipsEditModel(relationship.From, relationship.Yield(), delegate { }, rollbackCb);

            // ACT

            editModel.RollbackCommand.Execute(null);

            // ASSERT

            Assert.Equal(relationship.From, rolledback.Value.Item1);
            Assert.Equal(relationship.Yield(), rolledback.Value.Item2);
        }
    }
}