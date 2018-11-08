using System;
using System.Windows;
using System.Windows.Input;

namespace Kosmograph.Desktop.Graph
{
    public partial class KosmographViewerWindow : Window
    {
        public static ICommand Layout = new RoutedUICommand("Layout", "Layout", typeof(KosmographViewerWindow));

        public KosmographViewerWindow()
        {
            this.InitializeComponent();
            this.Loaded += this.KosmographViewerWindow_Loaded;
        }

        private void KosmographViewerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.CommandBindings.Add(new CommandBinding(Layout, this.Layout_Executed));
        }

        private void Layout_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.viewerControl.Layout();
        }
    }
}