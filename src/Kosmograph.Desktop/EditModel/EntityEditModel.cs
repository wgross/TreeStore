using GalaSoft.MvvmLight.Command;
using Kosmograph.Desktop.EditModel.Base;
using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System;
using System.Collections;
using System.Linq;
using System.Windows.Input;

namespace Kosmograph.Desktop.EditModel
{
    public sealed class EntityEditModel : NamedEditModelBase<EntityViewModel, Entity>
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

        private AssignedTagEditModel CreateAssignedTag(TagViewModel tag) => new AssignedTagEditModel(new AssignedTagViewModel(tag, this.ViewModel.Model.Values));

        private AssignedTagEditModel CreateAssignedTag(AssignedTagViewModel tag) => new AssignedTagEditModel(tag);

        #endregion Collection of assigned tags

        #region Assign Tag command

        private bool AssignTagCanExecute(TagViewModel tag) => !this.Tags.Any(tvm => tvm.ViewModel.Tag.Model.Equals(tag.Model));

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

        protected override void Commit()
        {
            this.Tags.Commit(onAdd: this.CommitAddedTag, onRemove: this.CommitRemovedTag);
            this.Tags.ForEach(t => t.Properties.ForEach(p => p.CommitCommand.Execute(null)));
            base.Commit();
            this.committed(this.ViewModel.Model);
            this.MessengerInstance.Send(new EditModelCommitted(viewModel: this.ViewModel));
        }

        protected override bool CanCommit() => !this.HasErrors;

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

        protected override void Rollback()
        {
            this.Tags.Rollback();
            this.Tags.ForEach(t => t.Properties.ForEach(p => p.RollbackCommand.Execute(null)));
            base.Rollback();
            this.rolledback(this.ViewModel.Model);
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

            if (!string.IsNullOrEmpty(this.NameError))
                this.RaiseErrorsChanged(nameof(this.Name));
        }

        public override IEnumerable GetErrors(string propertyName)
        {
            if (!string.IsNullOrEmpty(this.NameError))
            {
                return this.NameError.Yield();
            }
            return Enumerable.Empty<string>();
        }

        #endregion Implement Validate
    }
}