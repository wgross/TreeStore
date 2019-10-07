using PSKosmograph.Service;
using Xunit;

namespace PSKosmograph.Test.Service
{
    public sealed class KosmographContainerTest
    {
        [Fact]
        public void KosmographContainer_has_name()
        {
            // ACT

            var container = new KosmographContainer("name");

            // ASSERT

            Assert.Equal("name", container.Name);
            Assert.True(container.IsContainer);
        }
    }
}