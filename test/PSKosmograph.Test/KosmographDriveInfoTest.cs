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
            KosmographCmdletProvider.NewKosmographService = path => new KosmographLiteDbPersistence(new KosmographMessageBus(), File.Open(path, FileMode.OpenOrCreate));

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
        public void KosmographCmdletProvider_create_LiteDb_file()
        {
            // ACT

            var result = this.PowerShell
                .AddCommand("New-Item")
                .AddParameter("Name", "e")
                .Invoke()
                .Single();

            // ASSERT

            Assert.NotNull(result);
        }
    }
}