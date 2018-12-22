using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Editor.Test.ViewModel
{
    public class KosmographViewModelTest : IDisposable
    {
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);
        private readonly Mock<IKosmographPersistence> persistence;
        private readonly Mock<ITagRepository> tagRepository;
        private readonly Mock<IEntityRepository> entityRepository;
        private readonly Mock<IRelationshipRepository> relationshipRepository;
        private readonly KosmographViewModel viewModel;

        public KosmographViewModelTest()
        {
            this.tagRepository = this.mocks.Create<ITagRepository>();
            this.entityRepository = this.mocks.Create<IEntityRepository>();
            this.relationshipRepository = this.mocks.Create<IRelationshipRepository>();
            this.persistence = this.mocks.Create<IKosmographPersistence>(MockBehavior.Loose);
            this.persistence
              .Setup(p => p.Tags)
              .Returns(this.tagRepository.Object);
            this.persistence
                .Setup(p => p.Entities)
                .Returns(this.entityRepository.Object);
            this.persistence
                .Setup(p => p.Relationships)
                .Returns(this.relationshipRepository.Object);

            this.viewModel = new KosmographViewModel(new KosmographModel(this.persistence.Object));
        }

        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        [Fact]
        public void KosmographViewModel_raise_property_changed_on_selected_item_change()
        {
            // ARRANGE

            this.persistence
               .Setup(p => p.Tags)
               .Returns(this.tagRepository.Object);

            var tag = new Tag();

            this.tagRepository
                .Setup(r => r.FindAll())
                .Returns(tag.Yield());

            var editTag = this.viewModel.Tags.Single();

            this.persistence
               .Setup(p => p.Entities)
               .Returns(this.entityRepository.Object);

            var entity = new Entity();

            this.entityRepository
                .Setup(r => r.FindAll())
                .Returns(entity.Yield());

            var editEntity = this.viewModel.Entities.Single();

            var properties = new List<string>();
            this.viewModel.PropertyChanged += (s, e) => properties.Add(e.PropertyName);

            // ACT

            this.viewModel.SelectedEntity = editEntity;
            this.viewModel.SelectedTag = editTag;

            // ASSERT

            Assert.Equal(new[] { nameof(KosmographViewModel.SelectedEntity), nameof(KosmographViewModel.SelectedTag) }, properties);
        }

        [Fact]
        public void KosmographViewModel_starts_delete_entity_with_relationships()
        {
            // ARRANGE

            var entity1 = new Entity();
            var entity2 = new Entity();
            this.entityRepository
                .Setup(r => r.FindAll())
                .Returns(new[] { entity1, entity2 });

            var relationship = new Relationship("r", entity1, entity2);
            this.relationshipRepository
                .Setup(r => r.FindAll())
                .Returns(relationship.Yield());

            this.viewModel.Entities.FillAll();
            this.viewModel.Relationships.FillAll();

            // ACT

            this.viewModel.DeleteEntityCommand.Execute(this.viewModel.Entities.First());

            // ASSERT

            Assert.NotNull(this.viewModel.DeletingEntity);
            Assert.Equal(entity1, this.viewModel.DeletingEntity.Entity.Model);
            Assert.Equal(relationship.Yield(), this.viewModel.DeletingEntity.Relationships.Select(r => r.Model));
        }

        [Fact]
        public void KosmographViewModel_commits_delete_entity_with_relationships()
        {
            // ARRANGE

            var entity1 = new Entity();
            var entity2 = new Entity();
            this.entityRepository
                .Setup(r => r.FindAll())
                .Returns(new[] { entity1, entity2 });

            var relationship = new Relationship("r", entity1, entity2);
            this.relationshipRepository
                .Setup(r => r.FindAll())
                .Returns(relationship.Yield());

            this.relationshipRepository
                .Setup(r => r.Delete(relationship))
                .Returns(true);

            this.entityRepository
                .Setup(r => r.Delete(entity1))
                .Returns(true);

            this.viewModel.Entities.FillAll();
            this.viewModel.Relationships.FillAll();

            this.viewModel.DeleteEntityCommand.Execute(this.viewModel.Entities.First());

            // ACT

            this.viewModel.DeletingEntity.CommitCommand.Execute(null);

            // ASSERT

            Assert.Null(this.viewModel.DeletingEntity);
        }

        [Fact]
        public void KosmographViewModel_reverts_delete_entity_with_relationships()
        {
            // ARRANGE

            var entity1 = new Entity();
            var entity2 = new Entity();
            this.entityRepository
                .Setup(r => r.FindAll())
                .Returns(new[] { entity1, entity2 });

            var relationship = new Relationship("r", entity1, entity2);
            this.relationshipRepository
                .Setup(r => r.FindAll())
                .Returns(relationship.Yield());

            this.viewModel.Entities.FillAll();
            this.viewModel.Relationships.FillAll();

            this.viewModel.DeleteEntityCommand.Execute(this.viewModel.Entities.First());

            // ACT

            this.viewModel.DeletingEntity.RollbackCommand.Execute(null);

            // ASSERT

            Assert.Null(this.viewModel.DeletingEntity);
        }

        [Fact]
        public void KosmographViewModel_disposes_model_on_disposing()
        {
            // ARRANGE

            this.persistence
                .Setup(p => p.Dispose());

            // ACT

            this.viewModel.Dispose();
        }
    }
}