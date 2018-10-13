using GalaSoft.MvvmLight.Command;
using Kosmograph.Desktop.EditModel.Base;
using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Kosmograph.Desktop.EditModel
{
    public class TagEditModel : NamedEditModelBase<TagViewModel, Tag>, INotifyDataErrorInfo
    {
        private readonly ITagEditCallback editCallback;

        public TagEditModel(TagViewModel viewModel, ITagEditCallback editCallback)
            : base(viewModel)
        {
            this.editCallback = editCallback;

            this.Properties =
                new CommitableObservableCollection<FacetPropertyEditModel>(viewModel.Properties.Select(p => new FacetPropertyEditModel(this, p)));

            this.CreatePropertyCommand = new RelayCommand(this.CreatePropertyExecuted);
            this.RemovePropertyCommand = new RelayCommand<FacetPropertyEditModel>(this.RemovePropertyExecuted);
        }

        #region Facet has observable collection of facet properties

        public CommitableObservableCollection<FacetPropertyEditModel> Properties { get; private set; }

        #endregion Facet has observable collection of facet properties

        #region Commands

        private void CreatePropertyExecuted()
        {
            this.Properties.Add(new FacetPropertyEditModel(this, new FacetPropertyViewModel(new FacetProperty("new property"))));
        }

        public ICommand CreatePropertyCommand { get; }

        private void RemovePropertyExecuted(FacetPropertyEditModel property)
        {
            this.Properties.Remove(property);
        }

        public ICommand RemovePropertyCommand { get; }

        #endregion Commands

        public override void Commit()
        {
            this.Properties.Commit(
                onAdd: p => this.ViewModel.Properties.Add(p.ViewModel),
                onRemove: p => this.ViewModel.Properties.Remove(p.ViewModel));
            this.Properties.ForEach(p => p.Commit());
            base.Commit();
            this.editCallback.Commit(this.ViewModel.Model);
        }

        protected override bool CanCommit()
        {
            if (this.Properties.Any(p => string.IsNullOrEmpty(p.Name)))
                return false; // no empty names
            if (this.Properties.Count() != this.Properties.Select(p => p.Name).Distinct().Count())
                return false;
            if (this.HasErrors)
                return false;
            return base.CanCommit() && this.editCallback.CanCommit(this);
        }

        public override void Rollback()
        {
            this.Properties =
                new CommitableObservableCollection<FacetPropertyEditModel>(this.ViewModel.Properties.Select(p => new FacetPropertyEditModel(this, p)));
            base.Rollback();
            this.editCallback.Rollback(this.ViewModel.Model);
        }

        #region Implement Validate

        private string nameError;

        protected override void Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                this.nameError = "Tag name must not be empty";
            }
            else
            {
                this.nameError = null;
            }
            if (!string.IsNullOrEmpty(this.nameError))
                this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(this.Name)));
        }

        #endregion Implement Validate

        #region INotifyDataErrorInfo

        public bool HasErrors => !string.IsNullOrEmpty(this.nameError);

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            if (nameof(Name).Equals(propertyName))
            {
                if (!string.IsNullOrEmpty(this.nameError))
                    return this.nameError.Yield();
            }
            return Enumerable.Empty<string>();
        }

        #endregion INotifyDataErrorInfo
    }
}