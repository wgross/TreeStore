using CodeOwls.PowerShell.Paths.Processors;
using CodeOwls.PowerShell.Provider;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using Kosmograph.LiteDb;
using Kosmograph.Messaging;
using Kosmograph.Model;
using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace PSKosmograph
{
    [CmdletProvider(KosmographCmdletProvider.Id, ProviderCapabilities.None)]
    public class KosmographCmdletProvider : Provider
    {
        #region Construction and initialization of this instance

        static KosmographCmdletProvider()
        {
            NewKosmographPersistence = _ => KosmographLiteDbPersistence.InMemory(new KosmographMessageBus());
        }

        /// <summary>
        /// The creation method is exposed as a delegfate for testing purpose.
        /// </summary>
        public static Func<string, IKosmographPersistence> NewKosmographPersistence { get; set; }
            = _ => throw new InvalidOperationException($"{NewKosmographPersistence} is uninitalized");

        private static IKosmographPersistence CreateLiteDbPersistenceFromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return KosmographLiteDbPersistence.InMemory(KosmographMessageBus.Default);
            }
            else
            {
                return KosmographLiteDbPersistence.InFile(KosmographMessageBus.Default, path);
            }
        }

        public KosmographCmdletProvider()
        {
        }

        public const string Id = "Kosmograph";

        #endregion Construction and initialization of this instance

        private KosmographDriveInfo KosmographDriveInfo => (KosmographDriveInfo)this.PSDriveInfo;

        public KosmographDriveInfo GetKosmographDriveInfo(string path)
        {
            if (this.PSDriveInfo is { })
                return (KosmographDriveInfo)this.PSDriveInfo;

            var driveName = Drive.GetDriveName(path);
            return this.ProviderInfo
                .Drives
                .OfType<KosmographDriveInfo>()
                .Where(d => d.Name.Equals(driveName, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();
        }

        protected override IPathResolver PathResolver => new KosmographPathResolver();

        protected override IProviderContext CreateContext(string path, bool recurse, bool resolveFinalNodeFilterItems)
        {
            return new KosmographProviderContext(this, path, this.GetKosmographDriveInfo(path), new KosmographPathResolver(), this.DynamicParameters, recurse)
            {
                ResolveFinalNodeFilterItems = resolveFinalNodeFilterItems
            };
        }

        #region DriveCmdletProvider

        protected override PSDriveInfo NewDrive(PSDriveInfo drive)
        {
            //return new KosmographDriveInfo(drive, NewKosmographService(drive.Root));

            var persistence = NewKosmographPersistence?.Invoke(drive.Root);
            if (persistence is null)
                throw new ArgumentNullException(nameof(NewKosmographPersistence));

            return new KosmographDriveInfo(new PSDriveInfo(drive.Name, drive.Provider, $@"{drive.Name}:\", drive.Description, drive.Credential), persistence);
        }

        public ProviderInfo GetProviderInfo() => this.ProviderInfo;

        protected override PSDriveInfo RemoveDrive(PSDriveInfo drive)
        {
            if (drive is KosmographDriveInfo kgdrive)
            {
                kgdrive.Dispose();
            }
            return base.RemoveDrive(drive);
        }

        #endregion DriveCmdletProvider
    }
}