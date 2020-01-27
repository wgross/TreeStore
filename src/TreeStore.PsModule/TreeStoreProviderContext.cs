using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using TreeStore.Model;

namespace TreeStore.PsModule
{
    public sealed class TreeStoreProviderContext : ProviderContext, ITreeStoreProviderContext
    {
        public TreeStoreProviderContext(
            TreeStoreCmdletProvider provider,
            string path, TreeStoreDriveInfo drive, TreeStorePathResolver pathResolver,
            object dynamicParameters, bool recurse)
            : base(provider, path, drive, pathResolver, dynamicParameters, recurse)
        {
            this.Persistence = drive.Persistence;
        }

        public ITreeStorePersistence Persistence { get; }
    }

    public static class ProviderContextExtensions
    {
        public static ITreeStorePersistence Persistence(this IProviderContext ctc)
            => ((ITreeStoreProviderContext)ctc).Persistence;
    }
}