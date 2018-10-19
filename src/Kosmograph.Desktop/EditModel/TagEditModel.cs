using GalaSoft.MvvmLight.Command;
using Kosmograph.Desktop.EditModel.Base;
using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System.Collections;
using System.Linq;
using System.Windows.Input;

namespace Kosmograph.Desktop.EditModel
{
    public class TagEditModel : NamedEditModelBase<TagViewModel, Tag>
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

        protected override void Commit()
        {
            this.Properties.Commit(
                onAdd: p => this.ViewModel.Properties.Add(p.ViewModel),
                onRemove: p => this.ViewModel.Properties.Remove(p.ViewModel));
            this.Properties.ForEach(p => p.CommitCommand.Execute(null));
            base.Commit();
            this.editCallback.Commit(this.ViewModel.Model);
        }

        protected override bool CanCommit()
        {
            if (this.HasErrors)
                return false;
            if (base.CanCommit())
                if (this.Properties.All(p => p.CommitCommand.CanExecute(null)))
                    return this.editCallback.CanCommit(this);
            return false;
        }

        protected override void Rollback()
        {
            this.Properties =
                new CommitableObservableCollection<FacetPropertyEditModel>(this.ViewModel.Properties.Select(p => new FacetPropertyEditModel(this, p)));
            base.Rollback();
            this.editCallback.Rollback(this.ViewModel.Model);
        }

        #region Implement Validate

        protected override void Validate()
        {
            this.HasErrors = false;
            this.NameError = this.editCallback.Validate(this);

            // validate the repo data
            if (!string.IsNullOrEmpty(this.NameError))
            {
                this.HasErrors = this.HasErrors || true;
                this.RaiseErrorsChanged(nameof(this.Name));
            }

            // validate the local data
            if (string.IsNullOrEmpty(this.Name))
            {
                this.HasErrors = true;
                this.NameError = "Tag name must not be empty";
                this.RaiseErrorsChanged(nameof(this.Name));
            }
        }

        #endregion Implement Validate

        override public IEnumerable GetErrors(string propertyName)
        {
            if (nameof(Name).Equals(propertyName))
            {
                //if (!string.IsNullOrEmpty(this.NameError))
                return this.NameError.Yield();
            }
            return Enumerable.Empty<string>();
        }
    }
}