using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using Kosmograph.Model;

namespace PSKosmograph
{
    public interface IKosmographProviderContext : IProviderContext
    {
        IKosmographPersistence Persistence { get; }
    }
}