using Kosmograph.Model;
using Moq;
using System.Linq;
using System.Management.Automation;
using Xunit;

namespace PSKosmograph.Test
{
    [Collection("UsesPowershell")]
    public class KosmographFileSystemProviderTest
    {
        public MockRepository Mocks { get; } = new MockRepository(MockBehavior.Strict);

        public PowerShell PowerShell { get; }

        public Mock<IKosmographPersistence> PersistenceMock { get; }

        public KosmographFileSystemProviderTest()
        {
            this.PersistenceMock = this.Mocks.Create<IKosmographPersistence>();
            KosmographCmdletProvider.NewKosmographPersistence = _ => this.PersistenceMock.Object;

            this.PowerShell = PowerShell.Create();

            this.PowerShell
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
        public void Powershell_removing_drive_disposes_persistence()
        {
            // ARRANGE
            // import the module and create a drive

            this.PowerShell
                .AddStatement()
                    .AddCommand("New-PsDrive")
                        .AddParameter("Name", "kg")
                        .AddParameter("PsProvider", "Kosmograph")
                        .AddParameter("Root", @"");

            this.PersistenceMock.Setup(p => p.Dispose());

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

            // fecthing drive fails

            var drive = this.PowerShell
                .AddStatement()
                    .AddCommand("Get-PsDrive")
                        .AddParameter("Name", "kg")
                        .Invoke().Single();

            Assert.True(this.PowerShell.HadErrors);
        }
    }
}