using Kosmograph.Messaging;
using Kosmograph.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kosmograph.Desktop.Lists.ViewModel
{
    public class TagRepositoryViewModel : ObservableCollection<TagViewModel>, IObserver<ChangedMessage<ITag>>
    {
        private readonly ITagRepository repository;
        private readonly IChangedMessageBus<ITag> messaging;

        public TagRepositoryViewModel(ITagRepository repository, IChangedMessageBus<ITag> messaging)
        {
            this.repository = repository;
            this.messaging = messaging;
            this.messaging.Subscribe(this);
        }

        public void FillAll()
        {
            foreach (var vm in this.repository.FindAll().Select(t => new TagViewModel(t)))
                this.Add(vm);
        }

        #region IObserver<ChangedMessage<ITag>> Members

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(ChangedMessage<ITag> value)
        {
            switch (value.ChangeType)
            {
                case ChangeTypeValues.Modified:
                    this.Update(value.Changed.Id);
                    break;

                case ChangeTypeValues.Removed:
                    this.Remove(value.Changed.Id);
                    break;
            }
        }

        private void Remove(Guid id)
        {
            var existingTag = this.FirstOrDefault(t => t.Model.Id.Equals(id));
            if (existingTag is null)
                return;
            this.Remove(existingTag);
        }

        private void Update(Guid id)
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

        #endregion IObserver<ChangedMessage<ITag>> Members
    }
}