using GalaSoft.MvvmLight.Command;
using Kosmograph.Desktop.Editors.ViewModel.Base;
using Kosmograph.Model;
using System.Linq;
using System.Windows.Input;

namespace Kosmograph.Desktop.Editors.ViewModel
{
    public sealed class TagEditModel : NamedEditModelBase<Tag>
    {
        private readonly ITagEditCallback editCallback;

        public TagEditModel(Tag edited, ITagEditCallback editCallback)
            : base(edited)
        {
            this.editCallback = editCallback;

            this.Properties = new CommitableObservableCollection<FacetPropertyEditModel>(edited.Facet.Properties.Select(CreateFacetPropertyEditModel));
            this.CreatePropertyCommand = new RelayCommand(this.CreatePropertyExecuted);
            this.RemovePropertyCommand = new RelayCommand<FacetPropertyEditModel>(this.RemovePropertyExecuted);
        }

        #region Facet has observable collection of facet properties

        public CommitableObservableCollection<FacetPropertyEditModel> Properties { get; private set; }

        private FacetPropertyEditModel CreateFacetPropertyEditModel(FacetProperty property)
        {
            var tmp = new FacetPropertyEditModel(this, property);
            tmp.PropertyChanged += this.FacetPropertyEditModel_PropertyChanged;

            return tmp;
        }

        private void FacetPropertyEditModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(FacetPropertyEditModel.Name)))
                this.Validate();
            if (e.PropertyName.Equals(nameof(FacetPropertyEditModel.Type)))
                this.Validate();
        }

        #endregion Facet has observable collection of facet properties

        #region Commands

        private void CreatePropertyExecuted()
        {
            this.Properties.Add(new FacetPropertyEditModel(this, new FacetProperty("new property")));
            this.Validate();
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
                onAdd: p => this.Model.Facet.Properties.Add(p.Model),
                onRemove: p => this.Model.Facet.Properties.Remove(p.Model));
            this.Properties.ForEach(p => p.CommitCommand.Execute(null));
            base.Commit();
            this.editCallback.Commit(this.Model);
        }

        protected override bool CanCommit() => base.CanCommit() && this.editCallback.CanCommit(this);

        protected override void Rollback()
        {
            this.Properties =
                new CommitableObservableCollection<FacetPropertyEditModel>(this.Model.Facet.Properties.Select(p => new FacetPropertyEditModel(this, p)));
            base.Rollback();
            this.editCallback.Rollback(this.Model);
        }

        #region Implement Validate

        public override void Validate()
        {
            // validate base class rules
            base.Validate();
            // validate the facte properties rules
            this.Properties.ForEach(p => p.Validate());
            // collect all error states
            this.HasErrors = this.Properties.Aggregate(this.HasErrors, (hasErrors, p) => p.HasErrors || hasErrors);
            // and call the extsrnal model validator
            var result = this.editCallback.Validate(this);
            if (!string.IsNullOrEmpty(result))
            {
                this.NameError = result;
                this.HasErrors = true;
            }
            this.CommitCommand.RaiseCanExecuteChanged();
        }

        #endregion Implement Validate
    }
}