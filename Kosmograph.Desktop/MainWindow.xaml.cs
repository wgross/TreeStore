using Kosmograph.Desktop.Dialogs;
using Kosmograph.Desktop.ViewModel;
using Kosmograph.LiteDb;
using Kosmograph.Model;
using System;
using System.Windows;
using System.Windows.Input;

namespace Kosmograph.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.Activated += this.MainWindow_Activated;
            this.CommandBindings.Add(new CommandBinding(KosmographCommands.CreateTag, this.CreateTagExecuted, this.CreateTagCanExceute));
            this.CommandBindings.Add(new CommandBinding(KosmographCommands.EditTag, this.EditTagExecuted, this.EditTagCanExceute));
            this.CommandBindings.Add(new CommandBinding(KosmographCommands.DeleteTag, this.DeleteTagExecuted, this.DeleteTagCanExecute));
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            if (this.ViewModel == null)
            {
                this.CreateNewModel();
                if (this.ViewModel == null)
                    if (Application.Current != null)
                        Application.Current.Shutdown();
            }
        }

        private void CreateNewModel()
        {
            var model = new KosmographModel(new KosmographLiteDbPersistence());
            model.Tags.Upsert(new Tag("tag1", new Facet("facet", new FacetProperty("p"))));

            this.ViewModel = new KosmographViewModel(model);

            CommandManager.InvalidateRequerySuggested();
        }

        public KosmographViewModel ViewModel
        {
            get
            {
                return (KosmographViewModel)this.DataContext;
            }
            set
            {
                this.DataContext = value;
            }
        }

        #region CreateNewTagCommand

        private void CreateTagExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var viewModel = (KosmographViewModel)(e.Parameter);
            var tag = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            var data = new EditTagViewModel(tag, delegate { });

            viewModel.Tags.Add(data);

            var dialog = new EditTagDialog { DataContext = data };
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                viewModel.Commit();
            }
            else
            {
                viewModel.Rollback();
            }
        }

        private void CreateTagCanExceute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !(e.Parameter is null || e.Parameter.GetType() != typeof(KosmographViewModel));
        }

        #endregion CreateNewTagCommand

        #region Edit Tag

        private void EditTagCanExceute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !(e.Parameter is null || e.Parameter.GetType() != typeof(EditTagViewModel));
        }

        private void EditTagExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var viewModel = (EditTagViewModel)e.Parameter;
            var dialog = new EditTagDialog { DataContext = viewModel };
            if (dialog.ShowDialog().GetValueOrDefault())
            {
            }
        }

        private void tagListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            KosmographCommands.EditTag.Execute(this.ViewModel.SelectedTag, null);
        }

        #endregion Edit Tag

        #region Delete Tag

        private void DeleteTagExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var viewModel = (KosmographViewModel)(e.Parameter);
            viewModel.Tags.Remove(viewModel.SelectedTag);
        }

        private void DeleteTagCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !(e.Parameter is null || e.Parameter.GetType() != typeof(KosmographViewModel));
        }

        #endregion Delete Tag
    }
}