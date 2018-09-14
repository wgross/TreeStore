using GalaSoft.MvvmLight.Command;
using Kosmograph.Model;
using System;
using System.Linq;
using System.Windows.Input;

namespace Kosmograph.Desktop.ViewModel
{
    public class EditEntityViewModel : EditNamedViewModelBase<Entity>
    {
        private readonly Action<Entity> committed;
        private readonly Action<Entity> rolledback;

        public EditEntityViewModel(Entity entity, Action<Entity> onEntityCommitted, Action<Entity> onEntityRolledback)
            : base(entity)
        {
            this.tags = new Lazy<CommitableObservableCollection<AssignedTagViewModel>>(() => this.CreateAssignedTags());
            this.AssignTagCommand = new RelayCommand<Tag>(this.AssigneTagExcuted);
            this.RemoveTagCommand = new RelayCommand<AssignedTagViewModel>(this.RemoveTagExecuted);
            this.committed = onEntityCommitted ?? delegate { };
            this.rolledback = onEntityRolledback ?? delegate { };
        }

        #region Collection of assigned tags

        private Lazy<CommitableObservableCollection<AssignedTagViewModel>> tags;

        public CommitableObservableCollection<AssignedTagViewModel> Tags => this.tags.Value;

        private CommitableObservableCollection<AssignedTagViewModel> CreateAssignedTags() => new CommitableObservableCollection<AssignedTagViewModel>(this.Model.Tags.Select(this.CreateAssignedTag));

        private AssignedTagViewModel CreateAssignedTag(Tag tag) => new AssignedTagViewModel(tag, this.Model.Values);

        #endregion Collection of assigned tags

        #region Assign Tag command

        private void AssigneTagExcuted(Tag tag)
        {
            this.Tags.Add(this.CreateAssignedTag(tag));
        }

        public ICommand AssignTagCommand { get; set; }

        #endregion Assign Tag command

        #region Remove Tag command

        public ICommand RemoveTagCommand { get; set; }

        private void RemoveTagExecuted(AssignedTagViewModel assignedTag)
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
            this.committed(this.Model);
        }

        private void CommitRemovedTag(AssignedTagViewModel tag)
        {
            this.Model.Tags.Remove(tag.Model);
            tag.Model.Facet.Properties.ForEach(p => this.Model.Values.Remove(p.Id.ToString()));
        }

        private void CommitAddedTag(AssignedTagViewModel tag)
        {
            this.Model.Tags.Add(tag.Model);
        }

        #endregion Commit changes to Model

        #region Rollback changes

        public override void Rollback()
        {
            this.Tags.Rollback();
            this.Tags.ForEach(t => t.Properties.ForEach(p => p.Rollback()));
            base.Rollback();
            this.rolledback(this.Model);
        }

        #endregion Rollback changes
    }
}