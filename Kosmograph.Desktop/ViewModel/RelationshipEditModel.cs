using GalaSoft.MvvmLight.Command;
using Kosmograph.Model;
using System;
using System.Linq;
using System.Windows.Input;

namespace Kosmograph.Desktop.ViewModel
{
    public class RelationshipEditModel : NamedEditModelBase<RelationshipViewModel, Relationship>
    {
        private readonly Action<Relationship> onRelationshipCommitted;
        private readonly Action<Relationship> onRelationshipRolledback;

        public RelationshipEditModel(RelationshipViewModel viewModel, Action<Relationship> onRelationshipCommitted, Action<Relationship> onRelatioshipRolledback)
            : base(viewModel)
        {
            this.From = viewModel.From;
            this.To = viewModel.To;
            this.tags = new Lazy<CommitableObservableCollection<AssignedTagEditModel>>(() => this.CreateAssignedTags());
            this.AssignTagCommand = new RelayCommand<Tag>(this.AssignTagExcuted, this.AssignTagCanExecute);
            this.RemoveTagCommand = new RelayCommand<AssignedTagEditModel>(this.RemoveTagExecuted);
            this.onRelationshipCommitted = onRelationshipCommitted;
            this.onRelationshipRolledback = onRelatioshipRolledback;
        }

        public EntityViewModel From
        {
            get => this.from;
            set => this.Set(nameof(From), ref this.from, value);
        }

        private EntityViewModel from;

        public EntityViewModel To
        {
            get => this.to;
            set => this.Set(nameof(To), ref this.to, value);
        }

        private EntityViewModel to;

        #region Collection of assigned tags

        private Lazy<CommitableObservableCollection<AssignedTagEditModel>> tags;

        public CommitableObservableCollection<AssignedTagEditModel> Tags => this.tags.Value;

        private CommitableObservableCollection<AssignedTagEditModel> CreateAssignedTags() => new CommitableObservableCollection<AssignedTagEditModel>(this.ViewModel.Model.Tags.Select(this.CreateAssignedTag));

        private AssignedTagEditModel CreateAssignedTag(Tag tag) => new AssignedTagEditModel(tag, this.ViewModel.Model.Values);

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

        public override void Commit()
        {
            this.ViewModel.From = this.From;
            this.ViewModel.To = this.To;
            base.Commit();
            this.onRelationshipCommitted(this.ViewModel.Model);
        }

        private void CommitRemovedTag(AssignedTagEditModel tag)
        {
            //tag.Model.Facete.Tags.Remove(tag.Model);
            //tag.Model.Facet.Properties.ForEach(p => this.Model.Values.Remove(p.Id.ToString()));
        }

        private void CommitAddedTag(AssignedTagEditModel tag)
        {
            //this.Model.Tags.Add(tag.Model);
        }

        public override void Rollback()
        {
            this.onRelationshipCommitted(this.ViewModel.Model);
        }
        #endregion Implement Commit
    }
}