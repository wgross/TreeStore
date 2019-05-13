using QuickGraph;
using System;

namespace Kosmograph.Desktop.Graph.ViewModel
{
    public class GraphViewModel : BidirectionalGraph<VertexViewModel, EdgeViewModel>
    {
        //public readonly IdentityMap<Guid, object> IdentityMap = new IdentityMap<Guid, object>();

        //protected override void OnVertexAdded(VertexViewModel args)
        //{
        //    base.OnVertexAdded(args);
        //    this.IdentityMap.Remove(args.ModelId);
        //    this.IdentityMap.Add(args.ModelId, args);
        //}

        //protected override void OnVertexRemoved(VertexViewModel args)
        //{
        //    base.OnVertexRemoved(args);
        //    this.IdentityMap.Remove(args.ModelId);
        //}

        //protected override void OnEdgeAdded(EdgeViewModel args)
        //{
        //    base.OnEdgeAdded(args);
        //    this.IdentityMap.Remove(args.ModelId);
        //    this.IdentityMap.Add(args.ModelId, args);
        //}

        //protected override void OnEdgeRemoved(EdgeViewModel args)
        //{
        //    base.OnEdgeRemoved(args);
        //    this.IdentityMap.Remove(args.ModelId);
        //}
    }
}