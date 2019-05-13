using Kosmograph.Messaging;
using Kosmograph.Model.Base;
using System;
using System.Linq;

namespace Kosmograph.Desktop.Lists.ViewModel
{
    public abstract class TaggedRepositoryViewModelBase<TRepositoryItem, TModel, CO> : RepositoryViewModelBase<TRepositoryItem, CO>, IObserver<ChangedMessage<ITag>>
        where TRepositoryItem : TaggedViewModelBase<TModel>
        where TModel : TaggedBase
        where CO : IIdentifiable
    {
        private readonly IChangedMessageBus<ITag> tagMessaging;
        private IDisposable tagSubscription;

        public TaggedRepositoryViewModelBase(IChangedMessageBus<CO> messaging, IChangedMessageBus<ITag> tagMessaging)
            : base(messaging)
        {
            this.tagMessaging = tagMessaging;
            this.tagSubscription = this.tagMessaging.Subscribe(this);
        }

        void IObserver<ChangedMessage<ITag>>.OnCompleted()
        {
            throw new NotImplementedException();
        }

        void IObserver<ChangedMessage<ITag>>.OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        void IObserver<ChangedMessage<ITag>>.OnNext(ChangedMessage<ITag> value)
        {
            var affectedItems =
             from e in this
             where e.Tags.Any(t => t.Tag.Id.Equals(value.Changed.Id))
             select e.Model.Id;

            foreach (var id in affectedItems.ToArray())
                this.OnUpdated(id);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                this.tagSubscription?.Dispose();
                this.tagSubscription = null;
            }
        }
    }
}