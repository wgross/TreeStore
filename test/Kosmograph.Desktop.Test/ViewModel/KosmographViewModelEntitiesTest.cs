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
        public void KosmographViewModel_provides_editor_for_existing_Entity()
        {
            // ARRANGE

            var entityViewModel = DefaultEntityViewModel();

            // ACT
            // start editing of a entity

            this.ViewModel.EditEntityCommand.Execute(entityViewModel);

            // ASSERT
            // editor was created

            Assert.NotNull(this.ViewModel.EditedEntity);
            Assert.Equal(entityViewModel.Model, this.ViewModel.EditedEntity.Model);
        }

        [Fact]
        public void KosmographViewModel_saves_entity_on_commit()
        {
            // ARRANGE
            // edit a entity

            var entityViewModel = DefaultEntityViewModel();
            this.ViewModel.EditEntityCommand.Execute(entityViewModel);
            this.EntityRepository
                .Setup(r => r.Upsert(entityViewModel.Model))
                .Returns(entityViewModel.Model);

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

            var entityViewModel = DefaultEntityViewModel();
            this.ViewModel.EditEntityCommand.Execute(entityViewModel);

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

            var entity = DefaultEntityViewModel();

            this.EntityRepository
                .Setup(r => r.Delete(entity.Model))
                .Returns(true);

            // ACT

            this.ViewModel.DeleteEntityCommand.Execute(entity);
        }

        [Fact]
        public void KosmographViewModel_deletes_unkown_entity()
        {
            // ARRANGE

            var entity = DefaultEntityViewModel();

            this.EntityRepository
                .Setup(r => r.Delete(entity.Model))
                .Returns(false);

            this.RelationshipRepository
                .Setup(r => r.FindByEntity(entity.Model))
                .Returns(new Relationship[0]);

            // ACT

            this.ViewModel.DeleteEntityCommand.Execute(entity);

            // ASSERT

            Assert.Null(this.ViewModel.DeletingEntity);
        }

        [Fact]
        public void KosmographViewModel_deleting_entity_with_relationships_shows_dialog()
        {
            // ARRANGE

            var entity = DefaultEntityViewModel();

            this.EntityRepository
                .Setup(r => r.Delete(entity.Model))
                .Returns(false);

            var relationship = DefaultRelationship();
            this.RelationshipRepository
                .Setup(r => r.FindByEntity(entity.Model))
                .Returns(relationship.Yield());

            // ACT

            this.ViewModel.DeleteEntityCommand.Execute(entity);

            // ASSERT

            Assert.NotNull(this.ViewModel.DeletingEntity);
            Assert.Same(entity.Model, this.ViewModel.DeletingEntity.Entity);
            Assert.Same(relationship, this.ViewModel.DeletingEntity.Relationships.Single());
        }

        [Fact]
        public void KosmographViewModel_deletes_entity_with_relationships_on_committing_deletion_dialog()
        {
            // ARRANGE

            var relWasDeleted = false;

            var entity = DefaultEntityViewModel();

            this.EntityRepository
                .Setup(r => r.Delete(entity.Model))
                .Returns(relWasDeleted);

            var relationship = DefaultRelationship();
            this.RelationshipRepository
                .Setup(r => r.FindByEntity(entity.Model))
                .Returns(relationship.Yield());

            this.RelationshipRepository
                .Setup(r => r.Delete(new[] { relationship }))
                .Callback(() => relWasDeleted = true);

            this.ViewModel.DeleteEntityCommand.Execute(entity);

            // ACT

            this.ViewModel.DeletingEntity.CommitCommand.Execute(null);

            // ASSERT

            this.EntityRepository.Verify(r => r.Delete(entity.Model), Times.Exactly(2));
            Assert.Null(this.ViewModel.DeletingEntity);
        }

        [Fact]
        public void KosmographViewModel_deleting_entity_with_relationships_is_rolledback()
        {
            // ARRANGE

            var entity = DefaultEntityViewModel();

            this.EntityRepository
                .Setup(r => r.Delete(entity.Model))
                .Returns(false);

            var relationship = DefaultRelationship();
            this.RelationshipRepository
                .Setup(r => r.FindByEntity(entity.Model))
                .Returns(relationship.Yield());

            this.ViewModel.DeleteEntityCommand.Execute(entity);

            // ACT

            this.ViewModel.DeletingEntity.RollbackCommand.Execute(null);

            // ASSERT

            Assert.Null(this.ViewModel.DeletingEntity);
        }
    }
}