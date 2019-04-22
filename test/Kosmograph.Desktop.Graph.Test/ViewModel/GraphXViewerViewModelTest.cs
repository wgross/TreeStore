using Kosmograph.Desktop.Graph.ViewModel;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Kosmograph.Desktop.Graph.Test
{
    public class GraphXViewerViewModelTest
    {
        private readonly GraphXViewerViewModel viewModel;

        public GraphXViewerViewModelTest()
        {
            this.viewModel = new GraphXViewerViewModel();
        }

        [Fact]
        public void GraphXViewerViewModel_adds_entity_to_show()
        {
            // ARRANGE

            var entity = new Entity();
            // ACT

            this.viewModel.Show(entity.Yield());

            // ASSERT

            Assert.Equal(entity, this.viewModel.Entities.Single());
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
    }
}
