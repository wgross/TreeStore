using GalaSoft.MvvmLight.Command;
using Kosmograph.Desktop.EditModel.Base;
using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System;
using System.Linq;
using System.Windows.Input;

namespace Kosmograph.Desktop.EditModel
{
    public class TagEditModel : NamedEditModelBase<TagViewModel, Tag>
    {
        private readonly Action<Tag> committed;
        private readonly Action<Tag> rolledback;

        public TagEditModel(TagViewModel tag, Action<Tag> committed = null, Action<Tag> rolledback = null)
            : base(tag)
        {
            this.Properties =
                new CommitableObservableCollection<FacetPropertyEditModel>(tag.Properties.Select(p => new FacetPropertyEditModel(p)));

            this.CreatePropertyCommand = new RelayCommand(this.CreatePropertyExecuted);
            this.RemovePropertyCommand = new RelayCommand<FacetPropertyEditModel>(this.RemovePropertyExecuted);

            this.committed = committed ?? delegate { };
            this.rolledback = rolledback ?? delegate { };
        }

        #region Facet has observable collection of facet properties

        public CommitableObservableCollection<FacetPropertyEditModel> Properties { get; private set; }

        #endregion Facet has observable collection of facet properties

        #region Commands

        private void CreatePropertyExecuted()
        {
            this.Properties.Add(new FacetPropertyEditModel(new FacetPropertyViewModel(new FacetProperty("new property"))));
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
            this.committed(this.ViewModel.Model);
        }

        protected override bool CanCommit()
        {
            if (this.Properties.Any(p => string.IsNullOrEmpty(p.Name)))
                return false; // no empty names
            if (this.Properties.Count() != this.Properties.Select(p => p.Name).Distinct().Count())
                return false;
            return base.CanCommit();
        }

        public override void Rollback()
        {
            this.Properties =
                new CommitableObservableCollection<FacetPropertyEditModel>(this.ViewModel.Properties.Select(p => new FacetPropertyEditModel(p)));
            base.Rollback();
            this.rolledback(this.ViewModel.Model);
        }
    }
}