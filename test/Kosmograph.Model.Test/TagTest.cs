using Xunit;

namespace Kosmograph.Model.Test
{
    public class TagTest
    {
        [Fact]
        public void Tag_contains_facet_with_empty_name()
        {
            // ACT

            var result = new Tag();

            // ASSERT

            Assert.Empty(result.Facet.Name);
        }
    }
}