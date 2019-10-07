﻿using CodeOwls.PowerShell.Paths.Processors;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using CodeOwls.PowerShell.Provider.PathNodes;
using Kosmograph.Model;
using PSKosmograph.PathNodes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PSKosmograph
{
    public sealed class KosmographPathResolver : IPathResolver
    {
        public IEnumerable<IPathNode> ResolvePath(IProviderContext providerContext, string path)
        {
            IPathNode node = new RootNode();
            foreach (var pathItem in path.Split("\\/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                node = node.Resolve(providerContext, pathItem).SingleOrDefault();
                if (node is null)
                    return Enumerable.Empty<IPathNode>();
            }
            return node.Yield();
        }
    }
}