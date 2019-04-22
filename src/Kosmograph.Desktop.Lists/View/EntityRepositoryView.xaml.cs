using Kosmograph.Desktop.Lists.ViewModel;
using Kosmograph.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kosmograph.Desktop.Lists.View
{
    public partial class EntityRepositoryView : UserControl
    {
        public EntityRepositoryView()
        {
            this.InitializeComponent();
        }

        EntityRepositoryViewModel ViewModel => this.DataContext as EntityRepositoryViewModel;

        #region Map mouse events to routed events

        private void repositoryListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) => this.RaiseEditEntityEvent();

        private void repositoryListBoxItem_MouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var entityViewModel = ((FrameworkElement)sender).DataContext as EntityViewModel;
                if (entityViewModel is null)
                    return;

                DataObject data = new DataObject();
                data.SetData(typeof(Entity), entityViewModel.Model);

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
                    this.RaiseDeleteEntityEvent();
                    e.Handled = true;
                    break;

                case Key.Return:
                    this.RaiseEditEntityEvent();
                    e.Handled = true;
                    break;

                case Key.Insert:
                    this.RaiseCreateEntityEvent();
                    e.Handled = true;
                    break;
            }
        }

        #endregion Map keyboard input to routed events

        #region Currently selected entity

        public EntityViewModel SelectedItem
        {
            get => (EntityViewModel)this.GetValue(SelectedItemProperty);
            set => this.SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(SelectedItem), propertyType: typeof(EntityViewModel), ownerType: typeof(EntityRepositoryView), typeMetadata: new PropertyMetadata(defaultValue: null));

        #endregion Currently selected entity

        #region Request editing of an entity

        public static readonly RoutedEvent EditEntityEvent = EventManager.RegisterRoutedEvent(nameof(EditEntity), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(EntityRepositoryView));

        public event RoutedEventHandler EditEntity
        {
            add
            {
                this.AddHandler(EditEntityEvent, value);
            }
            remove
            {
                this.RemoveHandler(EditEntityEvent, value);
            }
        }

        private void RaiseEditEntityEvent() => this.RaiseEvent(new RoutedEventArgs(EditEntityEvent));

        #endregion Request editing of an entity

        #region Request deletion of an Entity

        public static readonly RoutedEvent DeleteEntityEvent = EventManager.RegisterRoutedEvent(nameof(DeleteEntity), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(EntityRepositoryView));

        public event RoutedEventHandler DeleteEntity
        {
            add
            {
                this.AddHandler(DeleteEntityEvent, value);
            }
            remove
            {
                this.RemoveHandler(DeleteEntityEvent, value);
            }
        }

        private void RaiseDeleteEntityEvent() => this.RaiseEvent(new RoutedEventArgs(DeleteEntityEvent));

        #endregion Request deletion of an Entity

        #region Request creation of an Entity

        public static readonly RoutedEvent CreateEntityEvent = EventManager.RegisterRoutedEvent(nameof(CreateEntity), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(EntityRepositoryView));

        public event RoutedEventHandler CreateEntity
        {
            add
            {
                this.AddHandler(CreateEntityEvent, value);
            }
            remove
            {
                this.RemoveHandler(CreateEntityEvent, value);
            }
        }

        private void RaiseCreateEntityEvent() => this.RaiseEvent(new RoutedEventArgs(CreateEntityEvent));

        #endregion Request creation of an Entity
    }
}