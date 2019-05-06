using Kosmograph.Model;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class KosmographViewModelGraphTest : KosmographViewModelTestBase
    {
        [Fact]
        public void KosmographViewModel_add_all_entities_and_relationships_to_graph_view()
        {
            // ARRANGE

            var relationship = DefaultRelationship();

            EntityRepository
                .Setup(r => r.FindAll())
                .Returns(new[] { relationship.From, relationship.To });
            RelationshipRepository
                .Setup(r => r.FindAll())
                .Returns(relationship.Yield());
            TagRepository
                .Setup(r => r.FindAll())
                .Returns(relationship.Tags.Concat(relationship.From.Tags).Concat(relationship.To.Tags));

            // ACT

            ViewModel.FillAll();

            // ASSERT

            Assert.Equal(new[] { relationship.From, relationship.To }, ViewModel.Graph.Entities);
            Assert.Equal(new[] { relationship }, ViewModel.Graph.Relationships);
        }
    }
}