using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using TreeStore.Model;

namespace TreeStore.PsModule
{
    public interface ITreeStoreProviderContext : IProviderContext
    {
        ITreeStorePersistence Persistence { get; }
    }
}