using CodeOwls.PowerShell.Provider;
using Kosmograph.Model;
using System;
using System.Management.Automation;

namespace PSKosmograph
{
    public class KosmographDriveInfo : Drive, IDisposable
    {
        public KosmographDriveInfo(PSDriveInfo driveInfo, IKosmographPersistence persistence) : base(driveInfo)
        {
            this.Persistence = persistence;
        }

        public IKosmographPersistence Persistence { get; }

        public string Database { get; set; }

        #region IDisposable Support

        public void Dispose() => this.Persistence.Dispose();

        #endregion IDisposable Support
    }
}