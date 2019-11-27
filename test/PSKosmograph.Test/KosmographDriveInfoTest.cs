using Kosmograph.LiteDb;
using Kosmograph.Messaging;
using System.IO;
using System.Linq;
using Xunit;

namespace PSKosmograph.Test
{
    public class KosmographDriveInfoTest : KosmographCmdletProviderTestBase
    {
        public KosmographDriveInfoTest()
        {
            var path = Path.GetTempFileName();
            KosmographCmdletProvider.NewKosmographPersistence = path => KosmographLiteDbPersistence.InFile(new KosmographMessageBus(), path);

            this.PowerShell
                .AddStatement()
                    .AddCommand("New-PsDrive")
                        .AddParameter("Name", "kgl")
                        .AddParameter("PsProvider", "Kosmograph")
                        .AddParameter("Root", path)
                        .Invoke();

            this.PowerShell.Commands.Clear();
        }

        [Fact]
        public void KosmographCmdletProvider_creates_LiteDb_file()
        {
            // ACT

            this.PowerShell
                .AddCommand("New-Item")
                    .AddParameter("Path", @"kgl:\Entities\e");

            var result = this.PowerShell.Invoke().Single();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.NotNull(result);
        }
    }
}