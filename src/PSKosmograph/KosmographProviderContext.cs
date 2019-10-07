using CodeOwls.PowerShell.Paths.Processors;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using Kosmograph.Model;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace PSKosmograph
{
    public sealed class KosmographProviderContext : ProviderContext, IKosmographProviderContext
    {
        public KosmographProviderContext(
            CmdletProvider provider, string path, PSDriveInfo drive, IPathResolver pathProcessor, object dynamicParameters, bool recurse)
            : base(provider, path, drive, pathProcessor, dynamicParameters, recurse)
        {
            this.Persistence = ((KosmographCmdletProvider)provider).Persistence;
        }

        public IKosmographPersistence Persistence { get; }
    }

    public static class ProviderContextExtensions
    {
        

        public static IKosmographPersistence Persistence(this IProviderContext ctc)
            => ((IKosmographProviderContext)ctc).Persistence;
    }
}