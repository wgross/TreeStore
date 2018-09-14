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

            //this.CommandBindings.Add(new CommandBinding(KosmographCommands.CreateTag, this.CreateTagExecuted, this.CreateTagCanExceute));
            //this.CommandBindings.Add(new CommandBinding(KosmographCommands.CreateEntity, this.CreateEntityExecuted, this.CreateEntityCanExceute));
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
                var oldModel = this.ViewModel;
                if (oldModel != null)
                    oldModel.PropertyChanged -= this.KosmographViewModel_PropertyChanged;

                value.PropertyChanged += this.KosmographViewModel_PropertyChanged;
                this.DataContext = value;
            }
        }

        private void KosmographViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(KosmographViewModel.EditedTag):
                    this.tagEditorControl.DataContext = this.ViewModel.EditedTag;
                    this.tagEditorControl.Visibility = Visibility.Visible;
                    //var editTagDialog = new EditDialog { DataContext = this.ViewModel.EditedTag };
                    //editTagDialog.ShowDialog();
                    break;

                case nameof(KosmographViewModel.EditedEntity):

                    this.entityEditorControl.DataContext = this.ViewModel.EditedEntity;
                    this.entityEditorControl.Visibility = Visibility.Visible;
                    //var editEntityDialog = new EditDialog { DataContext = this.ViewModel.EditedEntity };
                    //editEntityDialog.ShowDialog();
                    break;
            }
        }

        #region Event handler

        private void tagListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) => this.ViewModel.EditTagCommand.Execute(this.ViewModel.SelectedTag);

        private void entityListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) => this.ViewModel.EditEntityCommand.Execute(this.ViewModel.SelectedEntity);

        #endregion Event handler
    }
}