using Kosmograph.Desktop.Graph.ViewModel;
using Kosmograph.Messaging;
using Kosmograph.Model;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Graph.Test
{
    public class GraphXViewerViewModelTest : IDisposable
    {
        private MockRepository Mocks { get; } = new MockRepository(MockBehavior.Strict);

        private readonly GraphXViewerViewModel viewModel;
        private readonly IKosmographMessageBus messageBus = new KosmographMessageBus();
        private readonly Mock<IGraphCallback> graphCallback;

        public GraphXViewerViewModelTest()
        {
            this.graphCallback = this.Mocks.Create<IGraphCallback>();
            this.viewModel = new GraphXViewerViewModel(this.messageBus);
            this.viewModel.GraphCallback = this.graphCallback.Object;
        }

        public void Dispose()
        {
            this.Mocks.VerifyAll();
        }

        [Fact]
        public void GraphXViewerViewModel_subscribes_to_message_bus()
        {
            // ARRANGE

            var messageBusMock = this.Mocks.Create<IKosmographMessageBus>();

            var messageBusEntitiesMock = this.Mocks.Create<IChangedMessageBus<IEntity>>();
            messageBusMock
                .Setup(m => m.Entities)
                .Returns(messageBusEntitiesMock.Object);

            var messageBusRelationshipMock = this.Mocks.Create<IChangedMessageBus<IRelationship>>();
            messageBusMock
                .Setup(m => m.Relationships)
                .Returns(messageBusRelationshipMock.Object);

            messageBusEntitiesMock
                .Setup(m => m.Subscribe(It.IsAny<IObserver<ChangedMessage<IEntity>>>()))
                .Returns(Mock.Of<IDisposable>());

            messageBusRelationshipMock
                .Setup(m => m.Subscribe(It.IsAny<IObserver<ChangedMessage<IRelationship>>>()))
                .Returns(Mock.Of<IDisposable>());

            // ACT

            var tmp = new GraphXViewerViewModel(messageBusMock.Object);
        }

        [Fact]
        public void GraphXViewerViewModel_adds_entity_to_show()
        {
            // ARRANGE

            var entity = new Entity();

            this.graphCallback
                .Setup(c => c.Add(It.IsAny<VertexViewModel>()));

            // ACT

            this.viewModel.Show(entity.Yield());
        }

        [Fact]
        public void GraphXViewerViewModel_adds_entity_on_added_message()
        {
            // ARRANGE

            this.graphCallback
                .Setup(c => c.Add(It.IsAny<VertexViewModel>()));

            var entity = new Entity();

            // ACT
            // send removal through message bus
            this.messageBus.Entities.Modified(entity);

            // ASSERT

            Assert.Single(this.viewModel.Entities);
        }

        [Fact]
        public void GraphXViewerViewModel_removes_entity_on_delete_message()
        {
            // ARRANGE

            this.graphCallback
                .Setup(c => c.Add(It.IsAny<VertexViewModel>()));

            var entity = new Entity();
            this.viewModel.Show(entity.Yield());

            // ACT
            // send removal through message bus
            this.messageBus.Entities.Removed(entity);

            // ASSERT

            Assert.Empty(this.viewModel.Entities);
        }

        [Fact]
        public void GraphXViewerViewModel_adds_relationship_to_show()
        {
            // ARRANGE

            var entity = new Relationship();

            // ACT

            this.viewModel.Show(entity.Yield());

            // ASSERT

            Assert.Equal(entity, this.viewModel.Relationships.Single());
        }

        [Fact]
        public void GraphXViewerViewModel_removes_relationship_on_delete_message()
        {
            // ARRANGE

            var relationship = new Relationship();
            this.viewModel.Show(relationship.Yield());

            // ACT

            this.messageBus.Relationships.Removed(relationship);

            // ASSERT

            Assert.Empty(this.viewModel.Relationships);
        }

        [Fact]
        public void GraphXViewerViewModel_updates_relationship_on_modified_message()
        {
            // ARRANGE

            var relationship = new Relationship();
            this.viewModel.Show(relationship.Yield());

            this.graphCallback
              .Setup(c => c.Add(It.IsAny<EdgeViewModel>()));
            this.graphCallback
              .Setup(c => c.Remove(It.IsAny<EdgeViewModel>()));

            // ACT

           this.messageBus.Relationships.Modified(relationship);
        }
    }
}