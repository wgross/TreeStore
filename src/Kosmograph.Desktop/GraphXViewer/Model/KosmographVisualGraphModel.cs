using QuickGraph;
using System;

namespace Kosmograph.Desktop.GraphXViewer.Model
{
    public class KosmographVisualGraphModel : BidirectionalGraph<KosmographVisualVertexModel, KosmographVisualEdgeModel>
    {
        public readonly IdentityMap<Guid, KosmographVisualVertexModel> IdentityMap = new IdentityMap<Guid, KosmographVisualVertexModel>();

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
    }
}