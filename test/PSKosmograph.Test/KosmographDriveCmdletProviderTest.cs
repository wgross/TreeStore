using Kosmograph.LiteDb;
using Kosmograph.Messaging;
using System.Linq;
using System.Management.Automation;
using Xunit;

namespace PSKosmograph.Test
{
    [Collection("UsesPowershell")]
    public class KosmographFileSystemProviderTest
    {
        public KosmographFileSystemProviderTest()
        {
            KosmographCmdletProvider.NewKosmographService = _ => new KosmographLiteDbPersistence(KosmographMessageBus.Default);

            this.PowerShell = PowerShell.Create();

            this.PowerShell
              .AddStatement()
                  .AddCommand("Import-Module")
                  .AddArgument("./PSKosmograph.dll")
                  .Invoke();
        }

        public PowerShell PowerShell { get; }

        [Fact]
        public void Powershell_creates_new_drive()
        {
            // ACT
            // create a drive with the treesor provider and give it the url

            this.PowerShell
                .AddStatement()
                    .AddCommand("New-PsDrive")
                        .AddParameter("Name", "kg")
                        .AddParameter("PsProvider", "Kosmograph")
                        .AddParameter("Root", @"c:\in\memory\db");

            var result = this.PowerShell.Invoke();

            // ASSERT
            // drive was created

            Assert.False(this.PowerShell.HadErrors);

            var drive = this.PowerShell
                .AddStatement()
                    .AddCommand("Get-PsDrive")
                        .AddParameter("Name", "kg")
                        .Invoke().Single();

            Assert.NotNull(drive);
            Assert.Equal("kg", ((PSDriveInfo)drive.ImmediateBaseObject).Name);
            Assert.Equal(@"kg:\", ((PSDriveInfo)drive.ImmediateBaseObject).Root);
            Assert.Null(((PSDriveInfo)drive.ImmediateBaseObject).Description);
            Assert.Equal("", ((PSDriveInfo)drive.ImmediateBaseObject).CurrentLocation);
            Assert.Null(((PSDriveInfo)drive.ImmediateBaseObject).DisplayRoot);
        }

        [Fact]
        public void Powershell_removes_drive()
        {
            // ARRANGE
            // import the module and create a drive

            this.PowerShell
                .AddStatement()
                    .AddCommand("New-PsDrive")
                        .AddParameter("Name", "kg")
                        .AddParameter("PsProvider", "Kosmograph")
                        .AddParameter("Root", @"");

            // ACT
            // remove the drive

            this.PowerShell
                .AddStatement()
                    .AddCommand("Remove-PsDrive")
                    .AddParameter("Name", "kg");

            var result = this.PowerShell.Invoke().Single();

            // ASSERT
            // drive is no longer there and service was called.

            Assert.False(this.PowerShell.HadErrors);

            Assert.NotNull(result);
            Assert.Equal("kg", ((PSDriveInfo)result.ImmediateBaseObject).Name);
            Assert.Equal(@"kg:\", ((PSDriveInfo)result.ImmediateBaseObject).Root);

            // fectcing drive fails

            var drive = this.PowerShell
                .AddStatement()
                    .AddCommand("Get-PsDrive")
                        .AddParameter("Name", "kg")
                        .Invoke().Single();

            Assert.True(this.PowerShell.HadErrors);
        }
    }
}