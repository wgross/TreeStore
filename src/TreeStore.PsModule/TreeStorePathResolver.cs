using CodeOwls.PowerShell.Paths;
using CodeOwls.PowerShell.Paths.Processors;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using TreeStore.Model;
using TreeStore.PsModule.PathNodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TreeStore.PsModule
{
    public sealed class TreeStorePathResolver : IPathResolver
    {
        public IEnumerable<PathNode> ResolvePath(IProviderContext providerContext, string path)
        {
            var normalizedPath = path;
            if (providerContext.Drive is { })
                if (normalizedPath.StartsWith(providerContext.Drive.Root))
                {
                    normalizedPath = normalizedPath.Remove(0, providerContext.Drive.Root.Length);
                }

            PathNode node = new RootNode();
            foreach (var pathItem in normalizedPath.Split("\\/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                node = node.Resolve(providerContext, pathItem).SingleOrDefault();
                if (node is null)
                    return Enumerable.Empty<PathNode>();
            }
            return node.Yield();
        }
    }
}