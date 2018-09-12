using GalaSoft.MvvmLight.Command;
using Kosmograph.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;

namespace Kosmograph.Desktop.ViewModel
{
    public class EditFacetPropertyViewModel : EditNamedViewModel<FacetProperty>
    {
        public EditFacetPropertyViewModel(FacetProperty property)
            : base(property)
        {
        }
    }

    public class EditFacetViewModel : EditNamedViewModel<Facet>
    {
        #region Construction and Initialization of this instance

        private readonly List<(NotifyCollectionChangedAction, FacetProperty)> propertiesChanges;

        public EditFacetViewModel(Facet facet)
            : base(facet)
        {
            this.Properties = new ObservableCollection<EditFacetPropertyViewModel>(this.Model.Properties.Select(p => new EditFacetPropertyViewModel(p)));
            this.Properties.CollectionChanged += this.Properties_CollectionChanged;
            this.propertiesChanges = new List<(NotifyCollectionChangedAction, FacetProperty)>();
            this.CreatePropertyCommand = new RelayCommand<string>(this.CreatePropertyExecuted);
            this.RemovePropertyCommand = new RelayCommand<EditFacetPropertyViewModel>(this.RemovePropertyExecuted);
        }

        #endregion Construction and Initialization of this instance

        #region Facet has observable collection of facet properties

        private void Properties_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    this.propertiesChanges.AddRange(e.NewItems.OfType<EditFacetPropertyViewModel>().Select(fp => (NotifyCollectionChangedAction.Add, fp.Model)));
                    break;

                case NotifyCollectionChangedAction.Remove:
                    this.propertiesChanges.AddRange(e.OldItems.OfType<EditFacetPropertyViewModel>().Select(fp => (NotifyCollectionChangedAction.Remove, fp.Model)));
                    break;
            }
        }

        public ObservableCollection<EditFacetPropertyViewModel> Properties { get; }

        #endregion Facet has observable collection of facet properties

        #region commit changes to underlying model

        public override void Commit()
        {
            this.CommitProperties();
            base.Commit();
        }

        private void CommitProperties()
        {
            foreach (var (type, instance) in this.propertiesChanges)
            {
                switch (type)
                {
                    case NotifyCollectionChangedAction.Add:
                        this.Model.Properties.Add(instance);
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        this.Model.Properties.Remove(instance);
                        break;
                }
            }

            foreach (var property in this.Properties)
                property.Commit();
        }

        #endregion commit changes to underlying model

        #region Commands

        private void CreatePropertyExecuted(string name)
        {
            this.Properties.Add(new EditFacetPropertyViewModel(new FacetProperty(name ?? string.Empty)));
        }

        public ICommand CreatePropertyCommand { get; }

        private void RemovePropertyExecuted(EditFacetPropertyViewModel property)
        {
            this.Properties.Remove(property);
        }

        public ICommand RemovePropertyCommand { get; }

        #endregion Commands
    }
}