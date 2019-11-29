using Kosmograph.LiteDb;
using Kosmograph.Messaging;
using Kosmograph.Model;
using System.IO;
using Xunit;

namespace PSKosmograph.Test
{
    public class KosmographDriveInfoTest : KosmographCmdletProviderTestBase
    {
        private readonly string _liteDbPath;

        public KosmographDriveInfoTest()
        {
            _liteDbPath = Path.GetTempFileName();
            KosmographCmdletProvider.NewKosmographPersistence = path => KosmographLiteDbPersistence.InFile(new KosmographMessageBus(), path);

            this.PowerShell
                .AddStatement()
                    .AddCommand("New-PsDrive")
                        .AddParameter("Name", "kgf")
                        .AddParameter("PsProvider", "Kosmograph")
                        .AddParameter("Root", _liteDbPath)
                        .Invoke();

            this.PowerShell.Commands.Clear();
        }

        [Fact]
        public void KosmographCmdletProvider_creates_LiteDb_file()
        {
            // ACT

            this.PowerShell.AddCommand("New-Item").AddParameter("Path", @"kgf:\Tags\t").Invoke();
            this.PowerShell.Commands.Clear();
            this.PowerShell.AddCommand("New-Item").AddParameter("Path", @"kgf:\Tags\t\p").AddParameter("ValueType", FacetPropertyTypeValues.Long).Invoke();
            this.PowerShell.Commands.Clear();
            this.PowerShell.AddCommand("New-Item").AddParameter("Path", @"kgf:\Entities\e").Invoke();
            this.PowerShell.Commands.Clear();
            this.PowerShell.AddCommand("New-Item").AddParameter("Path", @"kgf:\Entities\e\t").Invoke();
            this.PowerShell.Commands.Clear();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.True(File.Exists(_liteDbPath));
        }
    }
}