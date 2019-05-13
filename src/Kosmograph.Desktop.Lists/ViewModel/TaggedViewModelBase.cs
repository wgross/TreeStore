using Kosmograph.Model.Base;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kosmograph.Desktop.Lists.ViewModel
{
    public abstract class TaggedViewModelBase<TModel> : NamedViewModelBase<TModel>
        where TModel : TaggedBase
    {
        public TaggedViewModelBase(TModel model) : base(model)
        {
            this.Tags = new ObservableCollection<AssignedTagViewModel>(this.Model.Tags.Select(t => new AssignedTagViewModel(t, model.Values)));
        }

        public ObservableCollection<AssignedTagViewModel> Tags { get; }
    }
}