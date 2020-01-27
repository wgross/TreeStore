using Xunit;

namespace TreeStore.Model.Test
{
    public class TagTest
    {
        [Fact]
        public void Tag_contains_facet_with_same_name()
        {
            // ACT

            var result = new Tag("tag");

            // ASSERT

            Assert.Equal("tag", result.Facet.Name);
        }
    }
}