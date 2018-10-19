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
    public class RelationshipEditModel : NamedEditModelBase<RelationshipViewModel, Relationship>
    {
        private readonly Action<Relationship> onRelationshipCommitted;
        private readonly Action<Relationship> onRelationshipRollback;

        public RelationshipEditModel(RelationshipViewModel viewModel, Action<Relationship> onRelationshipCommitted, Action<Relationship> onRelationshipRollback)
            : base(viewModel)
        {
            this.From = viewModel.From;
            this.To = viewModel.To;
            this.tags = new Lazy<CommitableObservableCollection<AssignedTagEditModel>>(() => this.CreateAssignedTags());
            this.AssignTagCommand = new RelayCommand<TagViewModel>(this.AssignTagExcuted, this.AssignTagCanExecute);
            this.RemoveTagCommand = new RelayCommand<AssignedTagEditModel>(this.RemoveTagExecuted);
            this.onRelationshipCommitted = onRelationshipCommitted;
            this.onRelationshipRollback = onRelationshipRollback;
        }

        public EntityViewModel From
        {
            get => this.from;
            set
            {
                if (this.Set(nameof(From), ref this.from, value))
                    this.CommitCommand.RaiseCanExecuteChanged();
            }
        }

        private EntityViewModel from;

        public EntityViewModel To
        {
            get => this.to;
            set
            {
                if (this.Set(nameof(To), ref this.to, value))
                    this.CommitCommand.RaiseCanExecuteChanged();
            }
        }

        private EntityViewModel to;

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

        #region Implement Commit

        protected override void Commit()
        {
            this.ViewModel.From = this.From;
            this.ViewModel.To = this.To;
            this.Tags.Commit(onAdd: this.CommitAddedTag, onRemove: this.CommitRemovedTag);
            this.Tags.ForEach(t => t.CommitCommand.Execute(null));
            base.Commit();
            this.onRelationshipCommitted(this.ViewModel.Model);
            this.MessengerInstance.Send(new EditModelCommitted(viewModel: this.ViewModel));
        }

        protected override bool CanCommit() => !(this.From is null || this.To is null);

        private void CommitRemovedTag(AssignedTagEditModel tag)
        {
            this.ViewModel.Tags.Remove(tag.ViewModel);
        }

        private void CommitAddedTag(AssignedTagEditModel tag)
        {
            this.ViewModel.Tags.Add(tag.ViewModel);
        }

        protected override void Rollback()
        {
            this.From = this.ViewModel.From;
            this.To = this.ViewModel.To;
            this.Tags.Rollback();
            this.Tags.ForEach(t => t.Properties.ForEach(p => p.RollbackCommand.Execute(null)));
            base.Rollback();
            this.onRelationshipRollback(this.ViewModel.Model);
        }

        #endregion Implement Commit

        #region Implement Validate

        protected override void Validate()
        {
        }

        #endregion Implement Validate

        public override IEnumerable GetErrors(string propertyName) => Enumerable.Empty<string>();
    }
}