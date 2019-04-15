using Kosmograph.Desktop.Lists.ViewModel;
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

        //private void tagListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) =>
        //    this.ViewModel.Tags.EditCommand.Execute(this.tagListBox.SelectedItem);

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
                data.SetData(typeof(EntityViewModel), entityViewModel);

                DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Link);
            }
        }

        #endregion Map mouse events to routed events

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
    }
}