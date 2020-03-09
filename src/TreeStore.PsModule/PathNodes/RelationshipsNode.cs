using CodeOwls.PowerShell.Paths;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace TreeStore.PsModule.PathNodes
{
    public sealed class RelationshipsNode : ContainerNode
    {
        public sealed class Item
        {
            public string Name => "Relationships";
        }

        public override string Name => "Relationships";

        #region IGetItem

        public override PSObject GetItem(IProviderContext providerContext) => PSObject.AsPSObject(new Item());

        #endregion IGetItem

        #region IGetChildItem Members

        public override IEnumerable<PathNode> GetChildNodes(IProviderContext providerContext)
        {
            throw new NotImplementedException();
        }

        #endregion IGetChildItem Members

        public override IEnumerable<PathNode> Resolve(IProviderContext providerContext, string name)
        {
            throw new NotImplementedException();
        }
    }
}