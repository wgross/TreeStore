using Kosmograph.Model;

namespace Kosmograph.Desktop.Lists.ViewModel
{
    public class RelationshipViewModel : TaggedViewModelBase<Relationship>
    {
        public RelationshipViewModel(Relationship model)
            : base(model)
        {
            this.From = new EntityViewModel(this.Model.From);
            this.To = new EntityViewModel(this.Model.To);
        }

        public EntityViewModel From { get; }

        public EntityViewModel To { get; }
    }
}