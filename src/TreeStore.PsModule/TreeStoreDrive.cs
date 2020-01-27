using CodeOwls.PowerShell.Provider;
using TreeStore.Model;
using System;
using System.Management.Automation;

namespace TreeStore.PsModule
{
    public class TreeStoreDriveInfo : Drive, IDisposable
    {
        public TreeStoreDriveInfo(PSDriveInfo driveInfo, ITreeStorePersistence persistence) : base(driveInfo)
        {
            this.Persistence = persistence;
        }

        public ITreeStorePersistence Persistence { get; }

        public string Database { get; set; }

        #region IDisposable Support

        public void Dispose() => this.Persistence.Dispose();

        #endregion IDisposable Support
    }
}