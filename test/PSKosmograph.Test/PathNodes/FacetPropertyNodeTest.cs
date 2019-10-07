using PSKosmograph.PathNodes;
using System.Linq;
using Xunit;

namespace PSKosmograph.Test.PathNodes
{
    public class FacetPropertyNodeTest : NodeTestBase
    {
        [Fact]
        public void FacetPropertyNode_has_name_and_ItemMode()
        {
            // ARRANGE

            var tag = DefaultTag();

            // ACT

            var result = new FacetPropertyNode(tag.Facet.Properties.Single());

            // ASSERT

            Assert.Equal("p", result.Name);
            Assert.Equal("+", result.ItemMode);
            Assert.Null(result.GetNodeChildrenParameters);
        }

        [Fact]
        public void FacetPropertyNode_provides_Value()
        {
            // ARRANGE

            var tag = DefaultTag();

            // ACT

            var result = new FacetPropertyNode(tag.Facet.Properties.Single()).GetNodeValue();

            // ASSERT

            Assert.Equal("p", result.Name);
            Assert.False(result.IsCollection);
            Assert.IsType<FacetPropertyNode.Value>(result);
        }

        [Fact]
        public void FacetPropertyNodeValue_provides_Item()
        {
            // ARRANGE

            var tag = DefaultTag();

            // ACT

            var result = new FacetPropertyNode(tag.Facet.Properties.Single()).GetNodeValue().Item as FacetPropertyNode.Item;

            // ASSERT

            Assert.Equal(tag.Facet.Properties.Single().Id, result!.Id);
            Assert.Equal("p", result!.Name);
            Assert.Equal(tag.Facet.Properties.Single().Type, result!.ValueType);
        }

        [Fact]
        public void FacetPropertyNodeItem_set_FacetProperty_name()
        {
            // ARRANGE

            var tag = DefaultTag();

            // ACT

            var item = new FacetPropertyNode(tag.Facet.Properties.Single()).GetNodeValue().Item as FacetPropertyNode.Item;
            item!.Name = "changed";

            // ASSERT

            Assert.Equal("changed", tag.Facet.Properties.Single().Name);
        }

        [Fact]
        public void FacetPropertyNodeItem_set_FacetProperty_type()
        {
            // ARRANGE

            var tag = DefaultTag();

            // ACT

            var item = new FacetPropertyNode(tag.Facet.Properties.Single()).GetNodeValue().Item as FacetPropertyNode.Item;
            item!.ValueType = Kosmograph.Model.FacetPropertyTypeValues.Bool;

            // ASSERT

            Assert.Equal(Kosmograph.Model.FacetPropertyTypeValues.Bool, tag.Facet.Properties.Single().Type);
        }
    }
}