using Kosmograph.Model;
using System;

namespace Kosmograph.Desktop.ViewModel
{
    public class RelationshipEditModel : NamedEditModelBase<RelationshipViewModel, Relationship>
    {
        private readonly Action<Relationship> onRelationshipCommitted;

        public RelationshipEditModel(RelationshipViewModel viewModel, Action<Relationship> onRelationshipCommitted, Action<Relationship> onRelatioshipRolledback)
            : base(viewModel)
        {
            this.From = viewModel.From;
            this.To = viewModel.To;
            this.Tags = new CommitableObservableCollection<TagViewModel>(viewModel.Tags);
            this.onRelationshipCommitted = onRelationshipCommitted;
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

        public CommitableObservableCollection<TagViewModel> Tags { get; set; }

        #region Implement Commit

        public override void Commit()
        {
            this.ViewModel.From = this.From;
            this.ViewModel.To = this.To;
            base.Commit();
            this.onRelationshipCommitted(this.ViewModel.Model);
        }

        #endregion Implement Commit
    }
}