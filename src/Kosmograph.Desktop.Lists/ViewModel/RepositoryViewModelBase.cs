using Kosmograph.Messaging;
using System;
using System.Collections.ObjectModel;

namespace Kosmograph.Desktop.Lists.ViewModel
{
    public abstract class RepositoryViewModelBase<VM, CO> : ObservableCollection<VM>,
        IDisposable,
        IObserver<ChangedMessage<CO>> where CO : IIdentifiable
    {
        private IDisposable subscription;

        public RepositoryViewModelBase(IChangedMessageBus<CO> messaging)
        {
            this.subscription = messaging.Subscribe(this);
        }

        #region IObserver<ChangedMessage<CO>> Members

        void IObserver<ChangedMessage<CO>>.OnCompleted()
        {
            throw new NotImplementedException();
        }

        void IObserver<ChangedMessage<CO>>.OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        void IObserver<ChangedMessage<CO>>.OnNext(ChangedMessage<CO> value)
        {
            switch (value.ChangeType)
            {
                case ChangeTypeValues.Modified:
                    this.OnUpdated(value.Changed.Id);
                    break;

                case ChangeTypeValues.Removed:
                    this.OnRemoved(value.Changed.Id);
                    break;
            }
        }

        protected abstract void OnUpdated(Guid id);

        protected abstract void OnRemoved(Guid id);

        #endregion IObserver<ChangedMessage<CO>> Members

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.Clear();
                    this.subscription?.Dispose();
                }

                this.subscription = null;
                this.disposedValue = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        #endregion IDisposable Support
    }
}