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
    }
}