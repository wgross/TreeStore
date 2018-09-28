using Kosmograph.Desktop.Dialogs;
using Kosmograph.Desktop.Graph;
using Kosmograph.Desktop.ViewModel;
using Kosmograph.LiteDb;
using Kosmograph.Model;
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
            this.Loaded += this.MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel == null)
            {
                this.CreateNewModel();
                if (this.ViewModel == null)
                    if (Application.Current != null)
                        Application.Current.Shutdown();
            }
            var wnd = new GraphWindow
            {
                DataContext = this.DataContext
            };
            wnd.Show();
        }

        private void CreateNewModel()
        {
            var model = new KosmographModel(new KosmographLiteDbPersistence());
            var tag1 = model.Tags.Upsert(new Tag("tag1", new Facet("facet", new FacetProperty("p1"))));
            var tag2 = model.Tags.Upsert(new Tag("tag2", new Facet("facet", new FacetProperty("p2"))));
            var entity1 = model.Entities.Upsert(new Entity("entity1", tag1));
            var entity2 = model.Entities.Upsert(new Entity("entity2", tag2));
            var entity3 = model.Entities.Upsert(new Entity("entity3", tag1));
            var relationship1 = model.Relationships.Upsert(new Relationship("relationship1", entity1, entity2, tag1));
            var relationship2 = model.Relationships.Upsert(new Relationship("relationship2", entity2, entity3, tag1));
            var viewModel = new KosmographViewModel(model);
            viewModel.FillAll();
            this.ViewModel = viewModel;

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
                value.PropertyChanged += this.Value_PropertyChanged;
                this.DataContext = value;
            }
        }

        public DeleteEntityWithRelationshipsDialog DeleteEntityWithRelationshipsDialog { get; private set; }

        private void Value_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(KosmographViewModel.DeletingEntity)))
            {
                if (this.ViewModel.DeletingEntity is null)
                {
                    this.DeleteEntityWithRelationshipsDialog?.Close();
                    this.DeleteEntityWithRelationshipsDialog = null;
                }
                else
                {
                    this.DeleteEntityWithRelationshipsDialog = new DeleteEntityWithRelationshipsDialog
                    {
                        DataContext = this.ViewModel.DeletingEntity,
                        Owner = this
                    };
                    this.DeleteEntityWithRelationshipsDialog.ShowDialog();
                }
            }
        }
    }
}