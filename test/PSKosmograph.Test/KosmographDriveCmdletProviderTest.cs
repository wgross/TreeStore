using System.Linq;
using System.Management.Automation;
using Xunit;

namespace PSKosmograph.Test
{

    [Collection("UsesPowershell")]
    public class KosmographFileSystemProviderTest
    {
        private readonly KosmographCmdletProvider provider;
        private readonly PowerShell powershell;

        public KosmographFileSystemProviderTest()
        {
            provider = new KosmographCmdletProvider();
            powershell = PowerShell.Create();

            this.powershell
                .AddStatement()
                    .AddCommand("Import-Module")
                    .AddArgument("./PSKosmograph.dll")
                    .Invoke();
        }

        [Fact]
        public void Powershell_creates_new_drive()
        {
            // ACT
            // create a drive with the treesor provider and give it the url

            this.powershell
                .AddStatement()
                    .AddCommand("New-PsDrive")
                        .AddParameter("Name", "kg")
                        .AddParameter("PsProvider", "Kosmograph")
                        .AddParameter("Root", @"");

            var result = this.powershell.Invoke();

            // ASSERT
            // drive was created

            Assert.False(this.powershell.HadErrors);

            var drive = this.powershell
                .AddStatement()
                    .AddCommand("Get-PsDrive")
                        .AddParameter("Name", "kg")
                        .Invoke().Single();

            Assert.NotNull(drive);
            Assert.Equal("kg", ((PSDriveInfo)drive.ImmediateBaseObject).Name);
            Assert.Equal("", ((PSDriveInfo)drive.ImmediateBaseObject).Root);
        }

        [Fact]
        public void Powershell_removes_drive()
        {
            // ARRANGE
            // import the module and create a drive

            this.powershell
                .AddStatement()
                    .AddCommand("New-PsDrive")
                        .AddParameter("Name", "kg")
                        .AddParameter("PsProvider", "Kosmograph")
                        .AddParameter("Root", @"");

            // ACT
            // remove the drive

            this.powershell
                .AddStatement()
                    .AddCommand("Remove-PsDrive")
                    .AddParameter("Name", "kg");

            var result = this.powershell.Invoke().Single();

            // ASSERT
            // drive is no longer there and service was called.

            Assert.False(this.powershell.HadErrors);

            Assert.NotNull(result);
            Assert.Equal("kg", ((PSDriveInfo)result.ImmediateBaseObject).Name);
            Assert.Equal("", ((PSDriveInfo)result.ImmediateBaseObject).Root);

            // fectcing drive fails

            var drive = this.powershell
                .AddStatement()
                    .AddCommand("Get-PsDrive")
                        .AddParameter("Name", "kg")
                        .Invoke().Single();

            Assert.True(this.powershell.HadErrors);
        }
    }
}