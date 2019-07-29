using Kosmograph.Desktop.Lists.ViewModel;
using Kosmograph.Model;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class KosmographViewModelEntitiesTest : KosmographViewModelTestBase
    {
        private Lists.ViewModel.EntityViewModel DefaultEntityViewModel(Entity entity) => Setup(new Lists.ViewModel.EntityViewModel(entity));

        private Lists.ViewModel.EntityViewModel DefaultEntityViewModel(Action<EntityViewModel> setup = null) => Setup(new Lists.ViewModel.EntityViewModel(DefaultEntity()));

        [Fact]
        public void KosmographViewModel_provides_editor_for_existing_entity()
        {
            // ARRANGE

            var entity = DefaultEntity();

            this.EntityRepository
                .Setup(r => r.FindById(entity.Id))
                .Returns(entity);

            // ACT
            // start editing of a entity

            this.ViewModel.EditEntityByIdCommand.Execute(entity.Id);

            // ASSERT
            // editor was created

            Assert.NotNull(this.ViewModel.EditedEntity);
            Assert.Equal(entity, this.ViewModel.EditedEntity.Model);
        }

        [Fact]
        public void KosmographViewModel_saves_entity_on_commit()
        {
            // ARRANGE
            // edit an entity

            var entity = DefaultEntity();

            this.EntityRepository
                .Setup(r => r.FindById(entity.Id))
                .Returns(entity);

            this.ViewModel.EditEntityByIdCommand.Execute(entity.Id);

            // save the entity

            this.EntityRepository
                .Setup(r => r.Upsert(entity))
                .Returns(entity);

            // ACT
            // committing the entity upserts the entity to the DB

            this.ViewModel.EditedEntity.CommitCommand.Execute(null);

            // ASSERT
            // the entity was sent to the repo

            Assert.Null(this.ViewModel.EditedEntity);
        }

        [Fact]
        public void KosmographViewModel_clears_edited_entity_on_rollback()
        {
            // ARRANGE
            // edit an entity

            var entity = DefaultEntity();

            this.EntityRepository
                .Setup(r => r.FindById(entity.Id))
                .Returns(entity);

            this.ViewModel.EditEntityByIdCommand.Execute(entity.Id);

            // ACT
            // rollback the entity

            this.ViewModel.EditedEntity.RollbackCommand.Execute(null);

            // ASSERT
            // the entity is sent to the repo

            Assert.Null(this.ViewModel.EditedEntity);
        }

        [Fact]
        public void KosmographViewModel_provides_editor_for_new_Entity()
        {
            // ACT
            // start editing of a entities

            this.ViewModel.CreateEntityCommand.Execute(null);

            // ASSERT
            // editor with minimal entity was created

            Assert.NotNull(this.ViewModel.EditedEntity);
            Assert.Equal("new entity", this.ViewModel.EditedEntity.Name);
            Assert.Empty(this.ViewModel.EditedEntity.Tags);
        }

        [Fact]
        public void KosmographViewModel_saves_new_entity_on_commit()
        {
            // ARRANGE

            this.ViewModel.CreateEntityCommand.Execute(null);
            Entity createdEntity = null;
            this.EntityRepository
                .Setup(r => r.Upsert(It.IsAny<Entity>()))
                .Callback<Entity>(e => createdEntity = e)
                .Returns<Entity>(e => e);

            // ACT
            // commit editor

            this.ViewModel.EditedEntity.CommitCommand.Execute(null);

            // ASSERT
            // editor is gone

            Assert.Null(this.ViewModel.EditedEntity);
            Assert.NotNull(createdEntity);
        }

        [Fact]
        public void KosmographViewModel_clears_new_entity_on_rollback()
        {
            // ARRANGE

            this.ViewModel.CreateEntityCommand.Execute(null);

            // ACT
            // rollback editor

            this.ViewModel.EditedEntity.RollbackCommand.Execute(null);

            // ASSERT
            // editor is gone

            Assert.Null(this.ViewModel.EditedEntity);
        }

        [Fact]
        public void KosmographViewModel_deletes_entity_without_relationships()
        {
            // ARRANGE

            var entity = DefaultEntity();

            this.EntityRepository
                 .Setup(r => r.FindById(entity.Id))
                 .Returns(entity);

            this.EntityRepository
                .Setup(r => r.Delete(entity))
                .Returns(true);

            // ACT

            this.ViewModel.DeleteEntityByIdCommand.Execute(entity.Id);
        }

        [Fact]
        public void KosmographViewModel_deletes_unkown_entity()
        {
            // ARRANGE

            var entity = DefaultEntity();

            this.EntityRepository
                 .Setup(r => r.FindById(entity.Id))
                 .Returns((Entity)null);

            // ACT

            this.ViewModel.DeleteEntityByIdCommand.Execute(entity.Id);

            // ASSERT

            Assert.Null(this.ViewModel.DeletingEntity);
        }

        [Fact]
        public void KosmographViewModel_deleting_entity_with_relationships_shows_dialog()
        {
            // ARRANGE

            var entity = DefaultEntity();

            this.EntityRepository
                 .Setup(r => r.FindById(entity.Id))
                 .Returns(entity);

            this.EntityRepository
                .Setup(r => r.Delete(entity))
                .Returns(false);

            var relationship = DefaultRelationship();
            this.RelationshipRepository
                .Setup(r => r.FindByEntity(entity))
                .Returns(relationship.Yield());

            // ACT

            this.ViewModel.DeleteEntityByIdCommand.Execute(entity.Id);

            // ASSERT

            Assert.NotNull(this.ViewModel.DeletingEntity);
            Assert.Same(entity, this.ViewModel.DeletingEntity.Entity);
            Assert.Same(relationship, this.ViewModel.DeletingEntity.Relationships.Single());
        }

        [Fact]
        public void KosmographViewModel_deletes_entity_with_relationships_on_committing_deletion_dialog()
        {
            // ARRANGE

            var relWasDeleted = false;

            var entity = DefaultEntity();

            this.EntityRepository
                 .Setup(r => r.FindById(entity.Id))
                 .Returns(entity);

            this.EntityRepository
                .Setup(r => r.Delete(entity))
                .Returns(relWasDeleted);

            var relationship = DefaultRelationship();
            this.RelationshipRepository
                .Setup(r => r.FindByEntity(entity))
                .Returns(relationship.Yield());

            this.RelationshipRepository
                .Setup(r => r.Delete(new[] { relationship }))
                .Callback(() => relWasDeleted = true);

            this.ViewModel.DeleteEntityByIdCommand.Execute(entity.Id);

            // ACT

            this.ViewModel.DeletingEntity.CommitCommand.Execute(null);

            // ASSERT

            this.EntityRepository.Verify(r => r.Delete(entity), Times.Exactly(2));

            Assert.Null(this.ViewModel.DeletingEntity);
        }

        [Fact]
        public void KosmographViewModel_deleting_entity_with_relationships_is_rolledback()
        {
            // ARRANGE

            var entity = DefaultEntity();

            this.EntityRepository
                 .Setup(r => r.FindById(entity.Id))
                 .Returns(entity);

            this.EntityRepository
                .Setup(r => r.Delete(entity))
                .Returns(false);

            var relationship = DefaultRelationship();
            this.RelationshipRepository
                .Setup(r => r.FindByEntity(entity))
                .Returns(relationship.Yield());

            this.ViewModel.DeleteEntityByIdCommand.Execute(entity.Id);

            // ACT

            this.ViewModel.DeletingEntity.RollbackCommand.Execute(null);

            // ASSERT

            Assert.Null(this.ViewModel.DeletingEntity);
        }
    }
}