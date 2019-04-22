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
        private readonly Mock<IRelationshipRepository> relationshipRepository;
        private readonly Mock<IEntityRepository> entityRepository;
        private readonly Mock<ITagRepository> tagRepository;
        private readonly Mock<IKosmographPersistence> persistence;
        private readonly Desktop.ViewModel.KosmographViewModel viewModel;

        private Lists.ViewModel.EntityViewModel DefaultEntityViewModel(Entity entity) => Setup(new Lists.ViewModel.EntityViewModel(entity));

        private Lists.ViewModel.EntityViewModel DefaultEntityViewModel(Action<EntityViewModel> setup = null) => Setup(new Lists.ViewModel.EntityViewModel(DefaultEntity()));

        public KosmographViewModelEntitiesTest()
        {
            this.relationshipRepository = this.Mocks.Create<IRelationshipRepository>();
            this.entityRepository = this.Mocks.Create<IEntityRepository>();
            this.tagRepository = this.Mocks.Create<ITagRepository>();
            this.persistence = this.Mocks.Create<IKosmographPersistence>();
            this.persistence
                .Setup(p => p.Entities)
                .Returns(this.entityRepository.Object);
            this.persistence
                .Setup(p => p.Tags)
                .Returns(this.tagRepository.Object);
            this.persistence
                .Setup(p => p.Relationships)
                .Returns(this.relationshipRepository.Object);
            this.viewModel = new Kosmograph.Desktop.ViewModel.KosmographViewModel(new Model.KosmographModel(this.persistence.Object));
        }

        [Fact]
        public void KosmographViewModel_provides_editor_for_existing_Entity()
        {
            // ARRANGE

            var entityViewModel = DefaultEntityViewModel();

            // ACT
            // start editing of a entity

            this.viewModel.EditEntityCommand.Execute(entityViewModel);

            // ASSERT
            // editor was created

            Assert.NotNull(this.viewModel.EditedEntity);
            Assert.Equal(entityViewModel.Model, this.viewModel.EditedEntity.Model);
        }

        [Fact]
        public void KosmographViewModel_saves_entity_on_commit()
        {
            // ARRANGE
            // edit a entity

            var entityViewModel = DefaultEntityViewModel();
            this.viewModel.EditEntityCommand.Execute(entityViewModel);
            this.entityRepository
                .Setup(r => r.Upsert(entityViewModel.Model))
                .Returns(entityViewModel.Model);

            // ACT
            // committing the entity upserts the entity to the DB

            this.viewModel.EditedEntity.CommitCommand.Execute(null);

            // ASSERT
            // the entity was sent to the repo

            Assert.Null(this.viewModel.EditedEntity);
        }

        [Fact]
        public void KosmographViewModel_clears_edited_entity_on_rollback()
        {
            // ARRANGE
            // edit an entity

            var entityViewModel = DefaultEntityViewModel();
            this.viewModel.EditEntityCommand.Execute(entityViewModel);

            // ACT
            // rollback the entity

            this.viewModel.EditedEntity.RollbackCommand.Execute(null);

            // ASSERT
            // the entity is sent to the repo

            Assert.Null(this.viewModel.EditedEntity);
        }

        [Fact]
        public void KosmographViewModel_provides_editor_for_new_Entity()
        {
            // ACT
            // start editing of a entities

            this.viewModel.CreateEntityCommand.Execute(null);

            // ASSERT
            // editor with minimal entity was created

            Assert.NotNull(this.viewModel.EditedEntity);
            Assert.Equal("new entity", this.viewModel.EditedEntity.Name);
            Assert.Empty(this.viewModel.EditedEntity.Tags);
        }

        [Fact]
        public void KosmographViewModel_saves_new_entity_on_commit()
        {
            // ARRANGE

            this.viewModel.CreateEntityCommand.Execute(null);
            Entity createdEntity = null;
            this.entityRepository
                .Setup(r => r.Upsert(It.IsAny<Entity>()))
                .Callback<Entity>(e => createdEntity = e)
                .Returns<Entity>(e => e);

            // ACT
            // commit editor

            this.viewModel.EditedEntity.CommitCommand.Execute(null);

            // ASSERT
            // editor is gone

            Assert.Null(this.viewModel.EditedEntity);
            Assert.NotNull(createdEntity);
        }

        [Fact]
        public void KosmographViewModel_clears_new_entity_on_rollback()
        {
            // ARRANGE

            this.viewModel.CreateEntityCommand.Execute(null);

            // ACT
            // rollback editor

            this.viewModel.EditedEntity.RollbackCommand.Execute(null);

            // ASSERT
            // editor is gone

            Assert.Null(this.viewModel.EditedEntity);
        }

        [Fact]
        public void KosmographViewModel_deletes_entity_without_relationships()
        {
            // ARRANGE

            var entity = DefaultEntityViewModel();

            this.entityRepository
                .Setup(r => r.Delete(entity.Model))
                .Returns(true);

            // ACT

            this.viewModel.DeleteEntityCommand.Execute(entity);
        }

        [Fact]
        public void KosmographViewModel_deletes_unkown_entity()
        {
            // ARRANGE

            var entity = DefaultEntityViewModel();

            this.entityRepository
                .Setup(r => r.Delete(entity.Model))
                .Returns(false);

            this.relationshipRepository
                .Setup(r => r.FindByEntity(entity.Model))
                .Returns(new Relationship[0]);

            // ACT

            this.viewModel.DeleteEntityCommand.Execute(entity);

            // ASSERT

            Assert.Null(this.viewModel.DeletingEntity);
        }

        [Fact]
        public void KosmographViewModel_deleting_entity_with_relationships_shows_dialog()
        {
            // ARRANGE

            var entity = DefaultEntityViewModel();

            this.entityRepository
                .Setup(r => r.Delete(entity.Model))
                .Returns(false);

            var relationship = DefaultRelationship();
            this.relationshipRepository
                .Setup(r => r.FindByEntity(entity.Model))
                .Returns(relationship.Yield());

            // ACT

            this.viewModel.DeleteEntityCommand.Execute(entity);

            // ASSERT

            Assert.NotNull(this.viewModel.DeletingEntity);
            Assert.Same(entity.Model, this.viewModel.DeletingEntity.Entity);
            Assert.Same(relationship, this.viewModel.DeletingEntity.Relationships.Single());
        }

        [Fact]
        public void KosmographViewModel_deletes_entity_with_relationships_on_committing_deletion_dialog()
        {
            // ARRANGE

            var relWasDeleted = false;

            var entity = DefaultEntityViewModel();

            this.entityRepository
                .Setup(r => r.Delete(entity.Model))
                .Returns(relWasDeleted);

            var relationship = DefaultRelationship();
            this.relationshipRepository
                .Setup(r => r.FindByEntity(entity.Model))
                .Returns(relationship.Yield());

            this.relationshipRepository
                .Setup(r => r.Delete(new[] { relationship }))
                .Callback(() => relWasDeleted = true);

            this.viewModel.DeleteEntityCommand.Execute(entity);

            // ACT

            this.viewModel.DeletingEntity.CommitCommand.Execute(null);

            // ASSERT

            this.entityRepository.Verify(r => r.Delete(entity.Model), Times.Exactly(2));
            Assert.Null(this.viewModel.DeletingEntity);
        }

        [Fact]
        public void KosmographViewModel_deleting_entity_with_relationships_is_rolledback()
        {
            // ARRANGE

            var entity = DefaultEntityViewModel();

            this.entityRepository
                .Setup(r => r.Delete(entity.Model))
                .Returns(false);

            var relationship = DefaultRelationship();
            this.relationshipRepository
                .Setup(r => r.FindByEntity(entity.Model))
                .Returns(relationship.Yield());

            this.viewModel.DeleteEntityCommand.Execute(entity);

            // ACT

            this.viewModel.DeletingEntity.RollbackCommand.Execute(null);

            // ASSERT

            Assert.Null(this.viewModel.DeletingEntity);
        }
    }
}