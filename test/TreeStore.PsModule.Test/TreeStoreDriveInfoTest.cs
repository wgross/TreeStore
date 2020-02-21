using System;
using System.IO;
using System.Management.Automation;
using TreeStore.Model;
using Xunit;

namespace TreeStore.PsModule.Test
{
    public class TreeStoreDriveInfoTest : IDisposable
    {
        private readonly string liteDbPath;

        public PowerShell PowerShell { get; }

        public TreeStoreDriveInfoTest()
        {
            this.liteDbPath = Path.GetTempFileName();

            this.PowerShell = PowerShell.Create();

            this.PowerShell
               .AddStatement()
                   .AddCommand("Import-Module")
                       .AddArgument("./TreeStore.dll")
                       .Invoke();

            this.PowerShell
                .AddStatement()
                    .AddCommand("New-TreeStoreDrive")
                        .AddParameter("Name", "kg")
                        .AddParameter("TreeStorePath", this.liteDbPath)
                        .Invoke();

            this.PowerShell.Commands.Clear();
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //File.Delete(this.liteDbPath);
                }

                disposedValue = true;
            }
        }

        public void Dispose() => this.Dispose(true);

        #endregion IDisposable Support

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
            this.PowerShell.AddCommand("Remove-PSDrive").AddParameter("Name", "kgf").Invoke();
            this.PowerShell.Commands.Clear();

            // ASSERT

            Assert.False(this.PowerShell.HadErrors);
            Assert.True(File.Exists(this.liteDbPath));
        }
    }
}