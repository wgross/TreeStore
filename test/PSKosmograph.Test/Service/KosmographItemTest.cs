using PSKosmograph.Service;
using Xunit;

namespace PSKosmograph.Test.Service
{
    public class KosmographItemTest : KosmographItemTestBase
    {
        [Fact]
        public void KosmographItem_has_name()
        {
            // ACT
            // make item from Tag

            var result = new KosmographItem(DefaultTag());

            // ASSERT

            Assert.Equal("t", result.Name);
            Assert.False(result.IsContainer);
        }
    }
}