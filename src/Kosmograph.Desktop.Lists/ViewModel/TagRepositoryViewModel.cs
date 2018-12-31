using Kosmograph.Messaging;
using Kosmograph.Model;
using System;
using System.Linq;

namespace Kosmograph.Desktop.Lists.ViewModel
{
    public class TagRepositoryViewModel : RepositoryViewModelBase<TagViewModel, ITag>
    {
        private readonly ITagRepository repository;
        private readonly IChangedMessageBus<ITag> messaging;

        public TagRepositoryViewModel(ITagRepository repository, IChangedMessageBus<ITag> messaging)
            : base(messaging)
        {
            this.repository = repository;
        }

        public void FillAll()
        {
            foreach (var vm in this.repository.FindAll().Select(t => new TagViewModel(t)))
                this.Add(vm);
        }

        override protected void Remove(Guid id)
        {
            var existingTag = this.FirstOrDefault(t => t.Model.Id.Equals(id));
            if (existingTag is null)
                return;
            this.Remove(existingTag);
        }

        override protected void Update(Guid id)
        {
            try
            {
                var tag = new TagViewModel(this.repository.FindById(id));
                var indexOfTag = this.IndexOf(tag);
                if (indexOfTag > -1)
                    this.SetItem(indexOfTag, tag);
                else
                    this.Add(tag);
            }
            catch (InvalidOperationException)
            {
                // throw on missing tag by Litedb -> consider map to KG exception type
                // remove item from list
                this.Remove(id);
            }
        }
    }
}