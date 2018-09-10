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
            this.ViewModel = new KosmographViewModel(new KosmographModel(new KosmographLiteDbPersistence()));

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
            var data = new EditTagViewModel(new Tag("tag", new Facet("facet", new FacetProperty("p"))));
            var dialog = new EditTagDialog { DataContext = data };
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                viewModel.Tags.Add(data);
            }
        }

        private void CreateTagCanExceute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !(e.Parameter is null || e.Parameter.GetType() != typeof(KosmographViewModel));
        }

        #endregion CreateNewTagCommand
    }
}