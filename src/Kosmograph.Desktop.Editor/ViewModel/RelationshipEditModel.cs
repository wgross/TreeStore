using GalaSoft.MvvmLight.Command;
using Kosmograph.Desktop.Editors.ViewModel.Base;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Kosmograph.Desktop.Editors.ViewModel
{
    public class RelationshipEditModel : NamedEditModelBase<Relationship>
    {
        private readonly Action<Relationship> onRelationshipCommitted;
        private readonly Action<Relationship> onRelationshipRollback;

        public RelationshipEditModel(Relationship edited, Action<Relationship> onRelationshipCommitted, Action<Relationship> onRelationshipRollback)
            : base(edited)
        {
            this.From = edited.From;
            this.To = edited.To;
            this.tags = new Lazy<CommitableObservableCollection<AssignedTagEditModel>>(() => this.CreateAssignedTags());
            this.AssignTagCommand = new RelayCommand<Tag>(this.AssignTagExcuted, this.AssignTagCanExecute);
            this.RemoveTagCommand = new RelayCommand<AssignedTagEditModel>(this.RemoveTagExecuted);
            this.onRelationshipCommitted = onRelationshipCommitted;
            this.onRelationshipRollback = onRelationshipRollback;
        }

        public Entity From
        {
            get => this.from;
            set
            {
                if (this.Set(nameof(From), ref this.from, value))
                    this.CommitCommand.RaiseCanExecuteChanged();
            }
        }

        private Entity from;

        public Entity To
        {
            get => this.to;
            set
            {
                if (this.Set(nameof(To), ref this.to, value))
                    this.CommitCommand.RaiseCanExecuteChanged();
            }
        }

        private Entity to;

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

        #region Implement Commit

        protected override void Commit()
        {
            this.Model.From = this.From;
            this.Model.To = this.To;
            this.Tags.Commit(onAdd: this.CommitAddedTag, onRemove: this.CommitRemovedTag);
            this.Tags.ForEach(t => t.CommitCommand.Execute(null));
            base.Commit();
            this.onRelationshipCommitted(this.Model);
            this.MessengerInstance.Send(new EditModelCommitted(model: this.Model));
        }

        protected override bool CanCommit() => this.HasEntities && this.AllProperties.Aggregate(true, (ok, p) => !p.HasErrors && ok);

        private bool HasEntities => !(this.From is null || this.To is null);

        private IEnumerable<AssignedFacetPropertyEditModel> AllProperties => this.Tags.SelectMany(t => t.Properties);

        private void CommitRemovedTag(AssignedTagEditModel tag)
        {
            this.Model.Tags.Remove(tag.Model);
        }

        private void CommitAddedTag(AssignedTagEditModel tag)
        {
            this.Model.Tags.Add(tag.Model);
        }

        protected override void Rollback()
        {
            this.From = this.Model.From;
            this.To = this.Model.To;
            this.Tags.Rollback();
            this.Tags.ForEach(t => t.Properties.ForEach(p => p.RollbackCommand.Execute(null)));
            base.Rollback();
            this.onRelationshipRollback(this.Model);
        }

        #endregion Implement Commit

        #region Implement Validate

        protected override void Validate()
        {
        }

        #endregion Implement Validate
    }
}