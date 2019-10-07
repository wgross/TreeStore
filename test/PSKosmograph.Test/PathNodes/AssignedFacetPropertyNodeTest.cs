using PSKosmograph.PathNodes;
using System.Linq;
using Xunit;

namespace PSKosmograph.Test.PathNodes
{
    public class AssignedFacetPropertyNodeTest : NodeTestBase
    {
        [Fact]
        public void AssignedFacetPropertyNode_has_name_and_ItemMode()
        {
            // ARRANGE

            var e = DefaultEntity();

            // ACT

            var result = new AssignedFacetPropertyNode(e, e.Tags.Single().Facet.Properties.Single());

            // ASSERT

            Assert.Equal("p", result.Name);
            Assert.Equal(".", result.ItemMode);
            Assert.Null(result.GetNodeChildrenParameters);
        }

        [Fact]
        public void AssignedFacetPropertyNode_provides_Value()
        {
            // ARRANGE

            var e = DefaultEntity();

            // ACT

            var result = new AssignedFacetPropertyNode(e, e.Tags.Single().Facet.Properties.Single()).GetNodeValue();

            // ASSERT

            Assert.Equal("p", result.Name);
            Assert.False(result.IsCollection);
        }

        [Fact]
        public void AssignedFacetPropertyNodeValue_provides_Item()
        {
            // ARRANGE

            var e = DefaultEntity();
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), 2);

            // ACT

            var result = new AssignedFacetPropertyNode(e,
                e.Tags.Single().Facet.Properties.Single()).GetNodeValue().Item as AssignedFacetPropertyNode.Item;

            // ASSERT

            Assert.Equal("p", result!.Name);
            Assert.Equal(2, result!.Value);
        }
    }
}