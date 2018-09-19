using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
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
            this.persistence = this.mocks.Create<IKosmographPersistence>(MockBehavior.Loose);
            this.tagRepository = this.mocks.Create<ITagRepository>();
            this.entityRepository = this.mocks.Create<IEntityRepository>();
            this.relationshipRepository = this.mocks.Create<IRelationshipRepository>();
            this.viewModel = new KosmographViewModel(new KosmographModel(this.persistence.Object));
        }

        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        [Fact]
        public void KosmographViewModel_notifies_Relationships_of_deleted_Entity()
        {
            // ARRANGE

            this.persistence
               .Setup(p => p.Entities)
               .Returns(this.entityRepository.Object);

            var entity1 = new Entity("e");
            this.entityRepository
                .Setup(r => r.FindAll())
                .Returns(entity1.Yield());

            this.entityRepository
               .Setup(r => r.Delete(entity1.Id))
               .Returns(true);

            this.persistence
              .Setup(p => p.Relationships)
              .Returns(this.relationshipRepository.Object);

            var relationship = new Relationship("r", entity1, entity1);
            this.relationshipRepository
                .Setup(r => r.FindAll())
                .Returns(relationship.Yield());

            this.relationshipRepository
                .Setup(r => r.Delete(relationship.Id))
                .Returns(true);

            var viewModel = new KosmographViewModel(new KosmographModel(this.persistence.Object));
            viewModel.Entities.FillAll();
            viewModel.Relationships.FillAll();

            // ACT

            viewModel.Entities.DeleteCommand.Execute(viewModel.Entities.Single());

            // ASSERT

            Assert.Empty(this.viewModel.Relationships);
        }

        //[Fact]
        //public void KosmographViewModel_delays_created_Tag_to_KosmographModel()
        //{
        //    // ARRANGE

        //    this.persistence
        //      .Setup(p => p.Tags)
        //      .Returns(this.tagRepository.Object);

        //    var tag = new Tag("t", Facet.Empty);
        //    this.tagRepository
        //        .Setup(r => r.FindAll())
        //        .Returns(tag.Yield());

        //    // ACT

        //    this.viewModel.CreateTagCommand.Execute(null);

        //    // ASSERT

        //    Assert.Single(this.viewModel.Tags);
        //    Assert.Equal("new tag", this.viewModel.EditedTag.ViewModel.Model.Name);
        //}

        //[Fact]
        //public void KosmographViewModel_commits_created_Tag_to_KosmographModel()
        //{
        //    // ARRANGE

        //    this.persistence
        //      .Setup(p => p.Tags)
        //      .Returns(this.tagRepository.Object);

        //    var tag = new Tag("t", Facet.Empty);
        //    this.tagRepository
        //        .Setup(r => r.FindAll())
        //        .Returns(tag.Yield());

        //    this.tagRepository
        //        .Setup(r => r.Upsert(It.IsAny<Tag>()))
        //        .Returns<Tag>(t => t);

        //    // create new tag
        //    this.viewModel.CreateTagCommand.Execute(null);

        //    // ACT

        //    this.viewModel.EditedTag.CommitCommand.Execute(null);

        //    // ASSERT
        //    // tag is inserted in the list after commit

        //    Assert.Equal(2, this.viewModel.Tags.Count);
        //    Assert.Equal("new tag", this.viewModel.Tags.ElementAt(1).Name);
        //    Assert.Null(this.viewModel.EditedTag);
        //    Assert.Equal(this.viewModel.Tags.ElementAt(1), this.viewModel.SelectedTag);
        //}

        //[Fact]
        //public void KosmographViewModel_reverts_created_Tag_at_KosmographViewModel()
        //{
        //    // ARRANGE

        //    this.persistence
        //      .Setup(p => p.Tags)
        //      .Returns(this.tagRepository.Object);

        //    var tag = new Tag("t", Facet.Empty);
        //    this.tagRepository
        //        .Setup(r => r.FindAll())
        //        .Returns(tag.Yield());

        //    // create new tag
        //    this.viewModel.CreateTagCommand.Execute(null);

        //    // ACT

        //    this.viewModel.EditedTag.RollbackCommand.Execute(null);

        //    // ASSERT
        //    // tag is forgotton

        //    Assert.Single(this.viewModel.Tags);
        //    Assert.Null(this.viewModel.EditedTag);
        //}

        //[Fact]
        //public void KosmographViewModel_commits_deleted_Tag_to_KosmographModel()
        //{
        //    // ARRANGE

        //    this.persistence
        //      .Setup(p => p.Tags)
        //      .Returns(this.tagRepository.Object);

        //    var tag = new Tag("t", Facet.Empty);
        //    this.tagRepository
        //        .Setup(r => r.FindAll())
        //        .Returns(tag.Yield());

        //    this.tagRepository
        //        .Setup(r => r.Delete(tag.Id))
        //        .Returns(true);

        //    // ACT

        //    // remove tag
        //    this.viewModel.Tags.DeleteCommand.Execute(this.viewModel.Tags.Single());

        //    // ASSERT
        //    // tag is inserted in the list after commit

        //    Assert.Empty(this.viewModel.Tags);
        //    Assert.Null(this.viewModel.SelectedTag);
        //    Assert.Null(this.viewModel.EditedTag);
        //}

        //[Fact]
        //public void KosmographViewModel_delays_created_Entity_to_KosmographModel()
        //{
        //    // ARRANGE

        //    this.persistence
        //      .Setup(p => p.Entities)
        //      .Returns(this.entityRepository.Object);

        //    var entity = new Entity("e");
        //    this.entityRepository
        //        .Setup(r => r.FindAll())
        //        .Returns(entity.Yield());

        //    // ACT

        //    this.viewModel.CreateEntityCommand.Execute(null);

        //    // ASSERT

        //    Assert.Single(this.viewModel.Entities);
        //    Assert.Equal("new entity", this.viewModel.EditedEntity.Name);
        //}

        //[Fact]
        //public void KosmographViewModel_commits_created_Entity_to_KosmographModel()
        //{
        //    // ARRANGE

        //    this.persistence
        //       .Setup(p => p.Entities)
        //       .Returns(this.entityRepository.Object);

        //    var entity = new Entity("e");
        //    this.entityRepository
        //        .Setup(r => r.FindAll())
        //        .Returns(entity.Yield());

        //    this.entityRepository
        //        .Setup(r => r.Upsert(It.IsAny<Entity>()))
        //        .Returns<Entity>(e => e);

        //    // create new entity
        //    this.viewModel.CreateEntityCommand.Execute(null);

        //    // ACT

        //    this.viewModel.EditedEntity.CommitCommand.Execute(null);

        //    // ASSERT
        //    // tag is inserted in the list after commit

        //    Assert.Equal(2, this.viewModel.Entities.Count);
        //    Assert.Equal("new entity", this.viewModel.Entities.ElementAt(1).Name);
        //    Assert.Null(this.viewModel.EditedEntity);
        //    Assert.Equal(this.viewModel.Entities.ElementAt(1), this.viewModel.SelectedEntity);
        //}

        //[Fact]
        //public void KosmographViewModel_reverts_created_Entity_at_KosmographViewModel()
        //{
        //    // ARRANGE

        //    this.persistence
        //       .Setup(p => p.Entities)
        //       .Returns(this.entityRepository.Object);

        //    var entity = new Entity("e");
        //    this.entityRepository
        //        .Setup(r => r.FindAll())
        //        .Returns(entity.Yield());

        //    // create new entity
        //    this.viewModel.CreateEntityCommand.Execute(null);

        //    // ACT

        //    this.viewModel.EditedEntity.RollbackCommand.Execute(null);

        //    // ASSERT
        //    // entiy is forgotton

        //    Assert.Single(this.viewModel.Entities);
        //    Assert.Null(this.viewModel.EditedEntity);
        //}

        //[Fact]
        //public void KosmographViewModel_commits_deleted_Entity_to_KosmographModel()
        //{
        //    // ARRANGE

        //    this.persistence
        //       .Setup(p => p.Entities)
        //       .Returns(this.entityRepository.Object);

        //    var entity = new Entity("e");
        //    this.entityRepository
        //        .Setup(r => r.FindAll())
        //        .Returns(entity.Yield());

        //    this.entityRepository
        //        .Setup(r => r.Delete(entity.Id))
        //        .Returns(true);

        //    // ACT

        //    // remove tag
        //    this.viewModel.Entities.DeleteCommand.Execute(this.viewModel.Entities.Single());

        //    // ASSERT
        //    // tag is inserted in the list after commit

        //    Assert.Empty(this.viewModel.Entities);
        //    Assert.Null(this.viewModel.SelectedEntity);
        //    Assert.Null(this.viewModel.EditedEntity);
        //}

        [Fact]
        public void KosmographViewModel_writes_modified_Tag_to_KosmographModel()
        {
            // ARRANGE

            this.persistence
               .Setup(p => p.Tags)
               .Returns(this.tagRepository.Object);

            var tag = new Tag();

            this.tagRepository
                .Setup(r => r.FindAll())
                .Returns(tag.Yield());

            this.tagRepository
                .Setup(r => r.Upsert(tag))
                .Returns(tag);

            this.viewModel.EditTagCommand.Execute(this.viewModel.Tags.Single());

            // ACT

            this.viewModel.EditedTag.Name = "changed";
            this.viewModel.EditedTag.CommitCommand.Execute(null);

            // ASSERT

            Assert.Null(this.viewModel.EditedTag);
        }

        [Fact]
        public void KosmographViewModel_writes_modified_Entity_to_KosmographModel()
        {
            // ARRANGE

            this.persistence
               .Setup(p => p.Entities)
               .Returns(this.entityRepository.Object);

            var entity = new Entity();

            this.entityRepository
                .Setup(r => r.FindAll())
                .Returns(entity.Yield());

            this.entityRepository
                .Setup(r => r.Upsert(entity))
                .Returns(entity);

            this.viewModel.EditEntityCommand.Execute(this.viewModel.Entities.Single());

            // ACT

            this.viewModel.EditedEntity.Name = "changed";
            this.viewModel.EditedEntity.CommitCommand.Execute(null);

            // ASSERT

            Assert.Null(this.viewModel.EditedEntity);
        }

        [Fact]
        public void KosmographViewModel_writes_Relationship_to_KosmographModel()
        {
            // ARRANGE

            this.persistence
               .Setup(p => p.Relationships)
               .Returns(this.relationshipRepository.Object);

            var relationship = new Relationship();

            this.relationshipRepository
                .Setup(r => r.FindAll())
                .Returns(relationship.Yield());

            this.relationshipRepository
                .Setup(r => r.Upsert(relationship))
                .Returns(relationship);

            this.viewModel.EditRelationshipCommand.Execute(this.viewModel.Relationships.Single());

            // ACT

            this.viewModel.EditedRelationship.Name = "changed";
            this.viewModel.EditedRelationship.CommitCommand.Execute(null);

            // ASSERT

            Assert.Null(this.viewModel.EditedRelationship);
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
    }
}