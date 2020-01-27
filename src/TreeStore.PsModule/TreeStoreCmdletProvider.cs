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

        static TreeStoreCmdletProvider()
        {
            NewTreeStorePersistence = _ => TreeStoreLiteDbPersistence.InMemory(new TreeStoreMessageBus());
        }

        /// <summary>
        /// The creation method is exposed as a delegfate for testing purpose.
        /// </summary>
        public static Func<string, ITreeStorePersistence> NewTreeStorePersistence { get; set; }
            = _ => throw new InvalidOperationException($"{NewTreeStorePersistence} is uninitalized");

        private static ITreeStorePersistence CreateLiteDbPersistenceFromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return TreeStoreLiteDbPersistence.InMemory(TreeStoreMessageBus.Default);
            }
            else
            {
                return TreeStoreLiteDbPersistence.InFile(TreeStoreMessageBus.Default, path);
            }
        }

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
            //return new KosmographDriveInfo(drive, NewKosmographService(drive.Root));

            var persistence = NewTreeStorePersistence?.Invoke(drive.Root);
            if (persistence is null)
                throw new ArgumentNullException(nameof(NewTreeStorePersistence));

            return new TreeStoreDriveInfo(new PSDriveInfo(drive.Name, drive.Provider, $@"{drive.Name}:\", drive.Description, drive.Credential), persistence);
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