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
            var model = new KosmographModel(new KosmographLiteDbPersistence());
            var tag1 = model.Tags.Upsert(new Tag("tag1", new Facet("facet", new FacetProperty("p1"))));
            var tag2 = model.Tags.Upsert(new Tag("tag2", new Facet("facet", new FacetProperty("p2"))));
            var entity1 = model.Entities.Upsert(new Entity("entity1", tag1));
            var entity2 = model.Entities.Upsert(new Entity("entity2", tag2));
            var relationship = model.Relationships.Upsert(new Relationship("relationship1", entity1, entity2, tag1));
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