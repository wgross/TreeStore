using Moq;
using System.Linq;
using System.Management.Automation;
using TreeStore.Model;
using Xunit;

namespace TreeStore.PsModule.Test
{
    [Collection("UsesPowershell")]
    public class KosmographFileSystemProviderTest
    {
        public MockRepository Mocks { get; } = new MockRepository(MockBehavior.Strict);

        public PowerShell PowerShell { get; }

        public Mock<ITreeStorePersistence> PersistenceMock { get; }

        public KosmographFileSystemProviderTest()
        {
            this.PowerShell = PowerShell.Create();
            this.PersistenceMock = this.Mocks.Create<ITreeStorePersistence>();

            // inject mock of TreeStore model
            TreeStoreCmdletProvider.NewTreeStorePersistence = _ => this.PersistenceMock.Object;

            this.PowerShell
              .AddStatement()
                  .AddCommand("Import-Module")
                  .AddArgument("./TreeStore.dll")
                  .Invoke();

            this.PowerShell.Commands.Clear();
        }

        [Fact]
        public void Powershell_creates_new_drive()
        {
            // ACT
            // create a drive with the treesor provider and give it the url

            this.PowerShell
                .AddStatement()
                    .AddCommand("New-TreeStoreDrive")
                        .AddParameter("Name", "kg")
                        .Invoke();

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
            Assert.Equal("TreeStore in memory", ((PSDriveInfo)drive.ImmediateBaseObject).Description);
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
                    .AddCommand("New-TreeStoreDrive")
                        .AddParameter("Name", "kg")
                        .Invoke();

            this.PersistenceMock.Setup(p => p.Dispose());

            // ACT
            // remove the drive

            this.PowerShell
                .AddStatement()
                    .AddCommand("Remove-PsDrive")
                    .AddParameter("Name", "kg")
                    .Invoke();

            // ASSERT
            // drive is no longer there and service was called.

            var drive = this.PowerShell
                .AddStatement()
                    .AddCommand("Get-PsDrive")
                        .AddParameter("Name", "kg")
                        .Invoke().SingleOrDefault();

            Assert.Null(drive);
            Assert.True(this.PowerShell.HadErrors);
        }
    }
}