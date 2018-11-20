using System;
using Xunit;

namespace Kosmograph.Desktop.Test
{
    public class IdentityMapTest
    {
        [Fact]
        public void IdentityMap_maps_sourceId_to_targetId()
        {
            // ARRANGE

            var sourceId = Guid.NewGuid();
            var targetId = 1;
            var map = new IdentityMap<Guid, int>();

            // ACT

            map.Add(sourceId, targetId);

            // ASSERT

            Assert.True(map.TryGetTarget(sourceId, out var readTargetId));
            Assert.Equal(1, readTargetId);
        }

        [Fact]
        public void IdentityMap_removes_mapping_sourceId_to_targetId()
        {
            // ARRANGE

            var sourceId = Guid.NewGuid();
            var targetId = 1;
            var map = new IdentityMap<Guid, int>();
            map.Add(sourceId, targetId);

            // ACT

            var result = map.Remove(sourceId);

            // ASSERT

            Assert.True(result);
            Assert.False(map.TryGetTarget(sourceId, out var readTargetId));
        }
    }
}