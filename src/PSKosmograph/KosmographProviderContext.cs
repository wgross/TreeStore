using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using Kosmograph.Model;

namespace PSKosmograph
{
    public sealed class KosmographProviderContext : ProviderContext, IKosmographProviderContext
    {
        public KosmographProviderContext(
            KosmographCmdletProvider provider,
            string path, KosmographDriveInfo drive, KosmographPathResolver pathResolver,
            object dynamicParameters, bool recurse)
            : base(provider, path, drive, pathResolver, dynamicParameters, recurse)
        {
            this.Persistence = drive.Persistence;
        }

        public IKosmographPersistence Persistence { get; }
    }

    public static class ProviderContextExtensions
    {
        public static IKosmographPersistence Persistence(this IProviderContext ctc)
            => ((IKosmographProviderContext)ctc).Persistence;
    }
}