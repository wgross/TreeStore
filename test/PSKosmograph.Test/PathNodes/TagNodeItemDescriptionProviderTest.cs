//using PSKosmograph.PathNodes;
//using System.ComponentModel;
//using Xunit;

//namespace PSKosmograph.Test.PathNodes
//{
//    public class TagNodeItemDescriptionProviderTest : NodeTestBase
//    {
//        private readonly TagNodeItemDescriptionProvider provider;

//        public TagNodeItemDescriptionProviderTest()
//        {
//            this.provider = new TagNodeItemDescriptionProvider();
//        }

//        [Fact]
//        public void TagNodeItemDescriptionProvider_provides_TagNodeItemTypeDescriptor()
//        {
//            // ARRANGE

//            var tag = DefaultTag();
//            var node = new TagNode(tag);

//            // ACT
//            var result = this.provider.GetTypeDescriptor(node.GetNodeValue().Item);

//            // ASSERT

//            Assert.NotNull(result);
//            Assert.IsType<TagNodeTypeDescritor>(result);
//        }

//        [Fact]
//        public void TagNodeItemTypeDescriptor_provides_Facet_properties()
//        {
//            // ARRANGE

//            var tag = DefaultTag();
//            var node = new TagNode(tag);
//            var typeDescriptor = this.provider.GetTypeDescriptor(node.GetNodeValue().Item);

//            // ACT

//            var result = typeDescriptor.GetProperties();

//            // ASSERT

//            Assert.Equal(3, result.Count);
//        }
//    }
//}