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
            this.CommandBindings.Add(new CommandBinding(KosmographCommands.CreateEntity, this.CreateEntityExecuted, this.CreateEntityCanExceute));

            this.CommandBindings.Add(new CommandBinding(KosmographCommands.EditTag, this.EditTagExecuted, this.EditTagCanExceute));
            this.CommandBindings.Add(new CommandBinding(KosmographCommands.EditEntity, this.EditEntityExecuted, this.EditEntityCanExceute));
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
            var tag = new Tag("tag1", new Facet("facet", new FacetProperty("p")));
            model.Tags.Upsert(tag);
            model.Entities.Upsert(new Entity("entity", tag));

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

        #region Shared Workflows

        private void KosmographModelCommandCanExecute<T>(CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !(e.Parameter is null || e.Parameter.GetType() != typeof(T));
        }

        #endregion Shared Workflows

        #region CreateNewTagCommand

        private void CreateTagExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var viewModel = (KosmographViewModel)(e.Parameter);

            viewModel.CreateTagCommand.Execute(null);

            var dialog = new EditTagDialog { DataContext = viewModel.SelectedTag };
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                viewModel.Commit();
            }
            else
            {
                viewModel.Rollback();
            }
        }

        private void CreateTagCanExceute(object sender, CanExecuteRoutedEventArgs e) => this.KosmographModelCommandCanExecute<KosmographViewModel>(e);

        #endregion CreateNewTagCommand

        #region Edit Tag

        private void EditTagCanExceute(object sender, CanExecuteRoutedEventArgs e) => this.KosmographModelCommandCanExecute<EditTagViewModel>(e);

        private void EditTagExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var viewModel = (EditTagViewModel)e.Parameter;
            var dialog = new EditDialog { DataContext = viewModel };
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                viewModel.Commit();
            }
            else
            {
                viewModel.Rollback();
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

        #region Create Entity

        private void CreateEntityCanExceute(object sender, CanExecuteRoutedEventArgs e) => this.KosmographModelCommandCanExecute<EditEntityViewModel>(e);

        private void CreateEntityExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var viewModel = (KosmographViewModel)(e.Parameter);

            viewModel.CreateEntityCommand.Execute(null);

            var dialog = new EditTagDialog { DataContext = viewModel.SelectedEntity };
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                viewModel.Commit();
            }
            else
            {
                viewModel.Rollback();
            }
        }

        #endregion Create Entity

        #region Edit Entity

        private void entityListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            KosmographCommands.EditEntity.Execute(this.ViewModel.SelectedEntity, null);
        }

        private void EditEntityExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var viewModel = (EditEntityViewModel)e.Parameter;
            var dialog = new EditDialog { DataContext = viewModel };
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                viewModel.Commit();
            }
            else
            {
                viewModel.Rollback();
            }
        }

        private void EditEntityCanExceute(object sender, CanExecuteRoutedEventArgs e) => this.KosmographModelCommandCanExecute<EditEntityViewModel>(e);

        #endregion Edit Entity
    }
}