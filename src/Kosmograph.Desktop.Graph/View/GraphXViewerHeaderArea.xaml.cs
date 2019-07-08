using Kosmograph.Desktop.Graph.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kosmograph.Desktop.Graph.View
{
    /// <summary>
    /// Interaction logic for GraphXViewerHeaderArea.xaml
    /// </summary>
    public partial class GraphXViewerHeaderArea : UserControl
    {
        public GraphXViewerHeaderArea() => this.InitializeComponent();

        #region Request editing of a Tag

        public static readonly RoutedEvent EditTagEvent = EventManager.RegisterRoutedEvent(nameof(EditTag), RoutingStrategy.Bubble, typeof(EditTagByIdRoutedEventArgs), typeof(GraphXViewer));

        public event RoutedEventHandler EditTag
        {
            add
            {
                this.AddHandler(EditTagEvent, value);
            }
            remove
            {
                this.RemoveHandler(EditTagEvent, value);
            }
        }

        private void RaiseEditTagEvent(Guid tagId) => this.RaiseEvent(new EditTagByIdRoutedEventArgs(EditTagEvent, tagId));

        #endregion Request editing of a Tag

        private void EditTag_Clicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            var tag = sender.AsType<FrameworkElement>()?.DataContext.AsType<TagQueryViewModel>()?.TagQuery.Tag;
            if (tag is null)
                return;

            this.RaiseEditTagEvent(tag.Id);
        }
    }
}