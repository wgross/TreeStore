using QuickGraph;
using System;

namespace Kosmograph.Desktop.Graph.ViewModel
{
    public class KosmographVisualGraphModel : BidirectionalGraph<KosmographVisualVertexModel, KosmographVisualEdgeModel>
    {
        public readonly IdentityMap<Guid, object> IdentityMap = new IdentityMap<Guid, object>();

        protected override void OnVertexAdded(KosmographVisualVertexModel args)
        {
            base.OnVertexAdded(args);
            this.IdentityMap.Add(args.ModelId, args);
        }

        protected override void OnVertexRemoved(KosmographVisualVertexModel args)
        {
            base.OnVertexRemoved(args);
            this.IdentityMap.Remove(args.ModelId);
        }

        protected override void OnEdgeAdded(KosmographVisualEdgeModel args)
        {
            base.OnEdgeAdded(args);
            this.IdentityMap.Add(args.ModelId, args);
        }

        protected override void OnEdgeRemoved(KosmographVisualEdgeModel args)
        {
            base.OnEdgeRemoved(args);
            this.IdentityMap.Remove(args.ModelId);
        }
    }
}