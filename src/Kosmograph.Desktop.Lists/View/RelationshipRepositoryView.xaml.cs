using Kosmograph.Desktop.Lists.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kosmograph.Desktop.Lists.View
{
    public partial class RelationshipRepositoryView : UserControl
    {
        public RelationshipRepositoryView()
        {
            this.InitializeComponent();
        }

        RelationshipRepositoryViewModel ViewModel => this.DataContext as RelationshipRepositoryViewModel;

        #region Map mouse events to routed events

        private void repositoryListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) => this.RaiseEditRelationshipEvent();

        private void repositoryListBoxItem_MouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var relationshipViewModel = ((FrameworkElement)sender).DataContext as RelationshipViewModel;
                if (relationshipViewModel is null)
                    return;

                DataObject data = new DataObject();
                data.SetData(typeof(RelationshipViewModel), relationshipViewModel);

                DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Link);
            }
        }

        #endregion Map mouse events to routed events

        #region Map keyboard input to routed events

        private void repositoryListBox_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    this.RaiseDeleteRelationshipEvent();
                    e.Handled = true;
                    break;

                case Key.Return:
                    this.RaiseEditRelationshipEvent();
                    e.Handled = true;
                    break;

                case Key.Insert:
                    this.RaiseCreateRelationshipEvent();
                    e.Handled = true;
                    break;
            }
        }

        #endregion Map keyboard input to routed events

        #region Currently selected relationship

        public RelationshipViewModel SelectedItem
        {
            get => (RelationshipViewModel)this.GetValue(SelectedItemProperty);
            set => this.SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(SelectedItem), propertyType: typeof(RelationshipViewModel), ownerType: typeof(RelationshipRepositoryView), typeMetadata: new PropertyMetadata(defaultValue: null));

        #endregion Currently selected relationship

        #region Request editing of a relationship

        public static readonly RoutedEvent EditRelationshipEvent = EventManager.RegisterRoutedEvent(nameof(EditRelationship), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RelationshipRepositoryView));

        public event RoutedEventHandler EditRelationship
        {
            add
            {
                this.AddHandler(EditRelationshipEvent, value);
            }
            remove
            {
                this.RemoveHandler(EditRelationshipEvent, value);
            }
        }

        private void RaiseEditRelationshipEvent() => this.RaiseEvent(new RoutedEventArgs(EditRelationshipEvent));

        #endregion Request editing of a relationship

        #region Request deletion of a Relationship

        public static readonly RoutedEvent DeleteRelationshipEvent = EventManager.RegisterRoutedEvent(nameof(DeleteRelationship), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RelationshipRepositoryView));

        public event RoutedEventHandler DeleteRelationship
        {
            add
            {
                this.AddHandler(DeleteRelationshipEvent, value);
            }
            remove
            {
                this.RemoveHandler(DeleteRelationshipEvent, value);
            }
        }

        private void RaiseDeleteRelationshipEvent() => this.RaiseEvent(new RoutedEventArgs(DeleteRelationshipEvent));

        #endregion Request deletion of a Relationship

        #region Request creation of an Relationship

        public static readonly RoutedEvent CreateRelationshipEvent = EventManager.RegisterRoutedEvent(nameof(CreateRelationship), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RelationshipRepositoryView));

        public event RoutedEventHandler CreateRelationship
        {
            add
            {
                this.AddHandler(CreateRelationshipEvent, value);
            }
            remove
            {
                this.RemoveHandler(CreateRelationshipEvent, value);
            }
        }

        private void RaiseCreateRelationshipEvent() => this.RaiseEvent(new RoutedEventArgs(CreateRelationshipEvent));

        #endregion Request creation of an Relationship
    }
}