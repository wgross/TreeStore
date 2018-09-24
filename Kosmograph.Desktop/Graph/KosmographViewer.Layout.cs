using Microsoft.Msagl.Core;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Miscellaneous;
using System;

namespace Kosmograph.Desktop.Graph
{
    public partial class KosmographViewer
    {
        public event EventHandler LayoutStarted;

        public event EventHandler LayoutComplete;

        public CancelToken CancelToken { get; set; }

        public bool RunLayoutAsync { get; set; }

        public bool NeedToCalculateLayout
        {
            get { return needToCalculateLayout; }
            set { needToCalculateLayout = value; }
        }

        private GeometryGraph geometryGraphUnderLayout;

        private void LayoutGraph()
        {
            if (this.NeedToCalculateLayout)
            {
                try
                {
                    LayoutHelpers.CalculateLayout(this.geometryGraphUnderLayout, this.Graph.LayoutAlgorithmSettings, this.CancelToken);

                    //if (MsaglFileToSave != null)
                    //{
                    //    drawingGraph.Write(MsaglFileToSave);
                    //    Console.WriteLine("saved into {0}", MsaglFileToSave);
                    //    Environment.Exit(0);
                    //}
                }
                catch (OperationCanceledException)
                {
                    //swallow this exception
                }
            }
        }

        private void PostLayoutStep()
        {
            this.GraphCanvasShow();
            this.PushDataFromLayoutGraphToFrameworkElements();
            this.backgroundWorker = null; //this will signal that we are not under layout anymore
            this.GraphChanged.Invoke(this, null);
            this.SetInitialTransform();
        }
    }
}