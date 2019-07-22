using GalaSoft.MvvmLight.Command;
using Kosmograph.Desktop.Editors.ViewModel.Base;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Kosmograph.Desktop.Editors.ViewModel
{
    public sealed class EntityEditModel : NamedEditModelBase<Entity>
    {
        private readonly Action<Entity> committed;
        private readonly Action<Entity> rolledback;

        public EntityEditModel(Entity edited, Action<Entity> onEntityCommitted, Action<Entity> onEntityRolledback)
            : base(edited)
        {
            this.tags = new Lazy<CommitableObservableCollection<AssignedTagEditModel>>(() => this.CreateAssignedTags());
            this.AssignTagCommand = new RelayCommand<Tag>(this.AssignTagExcuted, this.AssignTagCanExecute);
            this.RemoveTagCommand = new RelayCommand<AssignedTagEditModel>(this.RemoveTagExecuted);
            this.committed = onEntityCommitted ?? delegate { };
            this.rolledback = onEntityRolledback ?? delegate { };
        }

        #region Collection of assigned tags

        private Lazy<CommitableObservableCollection<AssignedTagEditModel>> tags;

        public CommitableObservableCollection<AssignedTagEditModel> Tags => this.tags.Value;

        private CommitableObservableCollection<AssignedTagEditModel> CreateAssignedTags() => new CommitableObservableCollection<AssignedTagEditModel>(this.Model.Tags.Select(this.CreateAssignedTag));

        private AssignedTagEditModel CreateAssignedTag(Tag tag) => new AssignedTagEditModel(tag, this.Model.Values);

        #endregion Collection of assigned tags

        #region Assign Tag command

        private bool AssignTagCanExecute(Tag tag) => !this.Tags.Any(tvm => tvm.Model.Equals(tag));

        private void AssignTagExcuted(Tag tag)
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

        protected override void Commit()
        {
            this.Tags.Commit(onAdd: this.CommitAddedTag, onRemove: this.CommitRemovedTag);
            this.Tags.ForEach(t => t.Properties.ForEach(p => p.CommitCommand.Execute(null)));
            base.Commit();
            this.committed(this.Model);
            this.MessengerInstance.Send(new EditModelCommitted(model: this.Model));
        }

        protected override bool CanCommit() => this.AllProperties.Aggregate(true, (ok, p) => !p.HasErrors && ok) && base.CanCommit();

        private IEnumerable<AssignedFacetPropertyEditModel> AllProperties => this.Tags.SelectMany(t => t.Properties);

        private void CommitRemovedTag(AssignedTagEditModel tag)
        {
            this.Model.Tags.Remove(tag.Model);
            //tag.ViewModel.Properties.ToList().ForEach(p => this.ViewModel.Remove(p.Id.ToString()));
        }

        private void CommitAddedTag(AssignedTagEditModel tag)
        {
            this.Model.Tags.Add(tag.Model);
        }

        #endregion Commit changes to Model

        #region Rollback changes

        protected override void Rollback()
        {
            this.Tags.Rollback();
            this.Tags.ForEach(t => t.Properties.ForEach(p => p.RollbackCommand.Execute(null)));
            base.Rollback();
            this.rolledback(this.Model);
        }

        #endregion Rollback changes

        #region Implement Validate

        protected override void Validate()
        {
            this.HasErrors = false;

            if (string.IsNullOrEmpty(this.Name))
            {
                this.NameError = "Name must not be empty";
                this.HasErrors = true;
            }
            else this.NameError = null;

            // this has side effect to the editor
            this.CommitCommand.RaiseCanExecuteChanged();
        }

        #endregion Implement Validate
    }
}