using CodeOwls.PowerShell.Paths.Processors;
using CodeOwls.PowerShell.Provider;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Provider;
using TreeStore.LiteDb;
using TreeStore.Messaging;
using TreeStore.Model;

namespace TreeStore.PsModule
{
    [CmdletProvider(TreeStoreCmdletProvider.Id, ProviderCapabilities.None)]
    public class TreeStoreCmdletProvider : Provider
    {
        #region Construction and initialization of this instance

        /// <summary>
        /// The creation method is exposed as a delegate for testing purpose.
        /// </summary>
        public static Func<string, ITreeStorePersistence> NewTreeStorePersistence { get; set; }
            = driveRoot => CreateLiteDbPersistenceFromPath(driveRoot);

        /// <summary>
        /// The Default Creation method make an in memory model if the drive root is empty.
        /// </summary>
        /// <param name="driveRoot"></param>
        /// <returns></returns>
        private static ITreeStorePersistence CreateLiteDbPersistenceFromPath(string driveRoot) => string.IsNullOrEmpty(driveRoot)
                ? TreeStoreLiteDbPersistence.InMemory(TreeStoreMessageBus.Default)
                : TreeStoreLiteDbPersistence.InFile(TreeStoreMessageBus.Default, $"FileName={driveRoot};Mode=Shared");

        public TreeStoreCmdletProvider()
        {
        }

        public const string Id = "TreeStore";

        #endregion Construction and initialization of this instance

        private TreeStoreDriveInfo KosmographDriveInfo => (TreeStoreDriveInfo)this.PSDriveInfo;

        public TreeStoreDriveInfo GetTreeStoreDriveInfo(string path)
        {
            if (this.PSDriveInfo is { })
                return (TreeStoreDriveInfo)this.PSDriveInfo;

            var driveName = Drive.GetDriveName(path);
            return this.ProviderInfo
                .Drives
                .OfType<TreeStoreDriveInfo>()
                .Where(d => d.Name.Equals(driveName, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();
        }

        protected override IPathResolver PathResolver => new TreeStorePathResolver();

        protected override IProviderContext CreateContext(string path, bool recurse, bool resolveFinalNodeFilterItems)
        {
            return new TreeStoreProviderContext(this, path, this.GetTreeStoreDriveInfo(path), new TreeStorePathResolver(), this.DynamicParameters, recurse)
            {
                ResolveFinalNodeFilterItems = resolveFinalNodeFilterItems
            };
        }

        #region DriveCmdletProvider

        protected override PSDriveInfo NewDrive(PSDriveInfo drive)
        {
            return new TreeStoreDriveInfo(new PSDriveInfo(
                drive.Name,
                drive.Provider,
                root: $@"{drive.Name}:\", // show the "name:\" as root path
                drive.Description,
                drive.Credential),
                    persistence: NewTreeStorePersistence(drive.Root));
        }

        public ProviderInfo GetProviderInfo() => this.ProviderInfo;

        protected override PSDriveInfo RemoveDrive(PSDriveInfo drive)
        {
            if (drive is TreeStoreDriveInfo kgdrive)
            {
                kgdrive.Dispose();
            }
            return base.RemoveDrive(drive);
        }

        #endregion DriveCmdletProvider
    }
}