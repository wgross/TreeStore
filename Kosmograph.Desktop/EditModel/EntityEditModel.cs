using GalaSoft.MvvmLight.Command;
using Kosmograph.Desktop.EditModel.Base;
using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System;
using System.Linq;
using System.Windows.Input;

namespace Kosmograph.Desktop.EditModel
{
    public class EntityEditModel : NamedEditModelBase<EntityViewModel, Entity>
    {
        private readonly Action<Entity> committed;
        private readonly Action<Entity> rolledback;

        public EntityEditModel(EntityViewModel model, Action<Entity> onEntityCommitted, Action<Entity> onEntityRolledback)
            : base(model)
        {
            this.tags = new Lazy<CommitableObservableCollection<AssignedTagEditModel>>(() => this.CreateAssignedTags());
            this.AssignTagCommand = new RelayCommand<TagViewModel>(this.AssignTagExcuted, this.AssignTagCanExecute);
            this.RemoveTagCommand = new RelayCommand<AssignedTagEditModel>(this.RemoveTagExecuted);
            this.committed = onEntityCommitted ?? delegate { };
            this.rolledback = onEntityRolledback ?? delegate { };
        }

        #region Collection of assigned tags

        private Lazy<CommitableObservableCollection<AssignedTagEditModel>> tags;

        public CommitableObservableCollection<AssignedTagEditModel> Tags => this.tags.Value;

        private CommitableObservableCollection<AssignedTagEditModel> CreateAssignedTags() => new CommitableObservableCollection<AssignedTagEditModel>(this.ViewModel.Tags.Select(this.CreateAssignedTag));

        private AssignedTagEditModel CreateAssignedTag(TagViewModel tag) => new AssignedTagEditModel(new AssignedTagViewModel(tag.Model, this.ViewModel.Model.Values));

        private AssignedTagEditModel CreateAssignedTag(AssignedTagViewModel tag) => new AssignedTagEditModel(tag);

        #endregion Collection of assigned tags

        #region Assign Tag command

        private bool AssignTagCanExecute(TagViewModel tag) => !this.Tags.Any(tvm => tvm.ViewModel.Model.Equals(tag.Model));

        private void AssignTagExcuted(TagViewModel tag)
        {
            this.Tags.Add(this.CreateAssignedTag(tag));
        }

        public ICommand AssignTagCommand { get; set; }

        #endregion Assign Tag command

        #region Remove Tag command

        public ICommand RemoveTagCommand { get; set; }

        private void RemoveTagExecuted(AssignedTagEditModel assignedTag)
        {
            this.Tags.Remove(assignedTag);
        }

        #endregion Remove Tag command

        #region Commit changes to Model

        public override void Commit()
        {
            this.Tags.Commit(onAdd: this.CommitAddedTag, onRemove: this.CommitRemovedTag);
            this.Tags.ForEach(t => t.Properties.ForEach(p => p.Commit()));
            base.Commit();
            this.committed(this.ViewModel.Model);
        }

        private void CommitRemovedTag(AssignedTagEditModel tag)
        {
            this.ViewModel.Tags.Remove(tag.ViewModel);
            //tag.ViewModel.Properties.ToList().ForEach(p => this.ViewModel.Remove(p.Id.ToString()));
        }

        private void CommitAddedTag(AssignedTagEditModel tag)
        {
            this.ViewModel.Tags.Add(tag.ViewModel);
        }

        #endregion Commit changes to Model

        #region Rollback changes

        public override void Rollback()
        {
            this.Tags.Rollback();
            this.Tags.ForEach(t => t.Properties.ForEach(p => p.Rollback()));
            base.Rollback();
            this.rolledback(this.ViewModel.Model);
        }

        #endregion Rollback changes
    }
}