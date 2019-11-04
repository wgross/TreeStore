using System.Management.Automation;
using Xunit;

namespace PSKosmograph.Test
{
    public class KosmographDriveInfoTest : KosmographCmdletProviderTestBase
    {
        [Fact]
        public void KosmographDriveInfo_recplicates_PSDriveInfo()
        {
            // ARRANGE

            var provider = new KosmographCmdletProvider();

            // ACT

            //var result = new KosmographDriveInfo(
            //    new PSDriveInfo("kg", provider.GetProviderInfo(), @"kg:\", strimngh., null), this.PersistenceMock.Object);

            // ASSERT

            // Assert.Same(this.Service.Object, result.Service);
        }
    }
}