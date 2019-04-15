using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using Moq;
using System;
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
            this.relationshipRepository = this.mocks.Create<IRelationshipRepository>();
            this.entityRepository = this.mocks.Create<IEntityRepository>();
            this.tagRepository = this.mocks.Create<ITagRepository>();
            this.persistence = this.mocks.Create<IKosmographPersistence>();
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
    }
}