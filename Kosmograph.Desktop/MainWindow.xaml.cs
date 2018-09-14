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

        #region Event handler

        private void tagListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) => this.ViewModel.EditTagCommand.Execute(this.ViewModel.SelectedTag);

        private void entityListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) => this.ViewModel.EditEntityCommand.Execute(this.ViewModel.SelectedEntity);

        #endregion Event handler

        private void tagListBox_MouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (this.ViewModel.SelectedTag is null)
                    return;

                DataObject data = new DataObject();
                data.SetData(typeof(Tag), this.ViewModel.SelectedTag.Model);

                DragDrop.DoDragDrop(this, data, DragDropEffects.Link);
            }
        }
    }
}