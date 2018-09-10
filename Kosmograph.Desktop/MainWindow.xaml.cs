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
            InitializeComponent();
            this.Activated += this.MainWindow_Activated;
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            if (this.Model == null)
            {
                this.CreateNewModel();
                if (this.Model == null)
                    if (Application.Current != null)
                        Application.Current.Shutdown();
            }
        }

        private void CreateNewModel()
        {
            this.Model = new KosmographViewModel(new KosmographModel(new KosmographLiteDbPersistence()));
        }

        public KosmographViewModel Model
        {
            get
            {
                return this.DataContext as KosmographViewModel;
            }
            set
            {
                this.DataContext = value;
            }
        }

        #region CreateNewTagCommand

        private void CreateTagExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var data = new EditTagViewModel(new Tag("tag", new Facet("facet", new FacetProperty("p"))));
            var dialog = new EditTagDialog { DataContext = data };
            dialog.ShowDialog();
        }

        private void CreateTagCanExceute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !(this.Model is null);
        }

        #endregion CreateNewTagCommand
    }
}