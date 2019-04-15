using GalaSoft.MvvmLight.Messaging;
using Kosmograph.Desktop.EditModel;
using Kosmograph.Desktop.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Kosmograph.Desktop.MsaglGraph
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Kosmograph.Desktop.MsaglGraph"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:Kosmograph.Desktop.MsaglGraph;assembly=Kosmograph.Desktop.MsaglGraph"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:KosmographControl/>
    ///
    /// </summary>
    public partial class KosmographControl : Canvas
    {
        public KosmographViewer MsaglGraphViewer { get; }

        static KosmographControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(KosmographControl), new FrameworkPropertyMetadata(typeof(KosmographControl)));
        }

        public KosmographControl()
        {
            this.MsaglGraphViewer = new KosmographViewer(this);
            this.Loaded += this.KosmographControl_Loaded;
            Messenger.Default.Register<EditModelCommitted>(this, this.EditModelCommitted);
        }
        
        private void KosmographControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.AddKosmographNodesAndEdges((KosmographViewModel)this.DataContext);
        }

        public void Layout() => this.MsaglGraphViewer.UpdateGraphLayout();
    }
}