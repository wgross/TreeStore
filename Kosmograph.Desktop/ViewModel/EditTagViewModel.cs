using GalaSoft.MvvmLight.Command;
using Kosmograph.Model;
using System;
using System.Linq;
using System.Windows.Input;

namespace Kosmograph.Desktop.ViewModel
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

        public CommitableObservableCollection<FacetPropertyEditModel> Properties { get; }

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

        public override void Rollback()
        {
            base.Rollback();
            this.rolledback(this.ViewModel.Model);
        }
    }
}