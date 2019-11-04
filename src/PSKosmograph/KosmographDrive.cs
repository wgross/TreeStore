using CodeOwls.PowerShell.Provider;
using Kosmograph.Model;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSKosmograph
{
    public class KosmographDriveInfo : Drive
    {
        public KosmographDriveInfo(PSDriveInfo driveInfo, IKosmographPersistence persistence) : base(driveInfo)
        {
            this.Persistence = persistence;
        }

        public IKosmographPersistence Persistence { get; }
        
        public string Database { get; set; }
    }
}