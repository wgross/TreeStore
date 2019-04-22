using Kosmograph.Desktop.Lists.ViewModel;
using Kosmograph.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kosmograph.Desktop.Lists.View
{
    public partial class TagRepositoryView : UserControl
    {
        public TagRepositoryView()
        {
            this.InitializeComponent();
        }

        private TagRepositoryViewModel ViewModel => this.DataContext as TagRepositoryViewModel;

        #region Currently selected tag

        public TagViewModel SelectedItem
        {
            get => (TagViewModel)this.GetValue(SelectedItemProperty);
            set => this.SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(SelectedItem), propertyType: typeof(TagViewModel), ownerType: typeof(TagRepositoryView), typeMetadata: new PropertyMetadata(defaultValue: null));

        #endregion Currently selected tag

        //private void tagListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) =>

        //    this.ViewModel.Tags.EditCommand.Execute(this.tagListBox.SelectedItem);

        private void repositoryListBoxItem_MouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var tagViewModel = ((FrameworkElement)sender).DataContext as TagViewModel;
                if (tagViewModel is null)
                    return;

                DataObject data = new DataObject();
                data.SetData(typeof(Tag), tagViewModel.Model);

                DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Link);
            }
        }

        #region Map keyboard input to routed events

        private void repositoryListBox_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    this.RaiseDeleteTagEvent();
                    e.Handled = true;
                    break;

                case Key.Return:
                    this.RaiseEditTagEvent();
                    e.Handled = true;
                    break;

                case Key.Insert:
                    this.RaiseCreateTagEvent();
                    e.Handled = true;
                    break;
            }
        }

        #endregion Map keyboard input to routed events

        #region Map mouse events to routed events

        private void repositoryListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) => this.RaiseEditTagEvent();

        #endregion Map mouse events to routed events

        #region Request editing of a Tag

        public static readonly RoutedEvent EditTagEvent = EventManager.RegisterRoutedEvent(nameof(EditTag), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TagRepositoryView));

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

        private void RaiseEditTagEvent() => this.RaiseEvent(new RoutedEventArgs(EditTagEvent));

        #endregion Request editing of a Tag

        #region Request deletion of a Tag

        public static readonly RoutedEvent DeleteTagEvent = EventManager.RegisterRoutedEvent(nameof(DeleteTag), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TagRepositoryView));

        public event RoutedEventHandler DeleteTag
        {
            add
            {
                this.AddHandler(DeleteTagEvent, value);
            }
            remove
            {
                this.RemoveHandler(DeleteTagEvent, value);
            }
        }

        private void RaiseDeleteTagEvent() => this.RaiseEvent(new RoutedEventArgs(DeleteTagEvent));

        #endregion Request deletion of a Tag

        #region Request creation of a Tag

        public static readonly RoutedEvent CreateTagEvent = EventManager.RegisterRoutedEvent(nameof(CreateTag), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TagRepositoryView));

        public event RoutedEventHandler CreateTag
        {
            add
            {
                this.AddHandler(CreateTagEvent, value);
            }
            remove
            {
                this.RemoveHandler(CreateTagEvent, value);
            }
        }

        private void RaiseCreateTagEvent() => this.RaiseEvent(new RoutedEventArgs(CreateTagEvent));

        #endregion Request creation of a Tag
    }
}