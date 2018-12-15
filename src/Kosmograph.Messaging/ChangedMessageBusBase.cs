using System;
using System.Reactive.Subjects;

namespace Kosmograph.Messaging
{
    public abstract class ChangedMessageBusBase<T> : IChangedMessageBus<T>
    {
        private readonly Subject<ChangedMessage<T>> observableSubject = new Subject<ChangedMessage<T>>();

        public void Added(T changed) => Send(ChangeTypeValues.Added, changed);

        public void Modified(T changed) => Send(ChangeTypeValues.Modified, changed);

        public void Removed(T changed) => this.Send(ChangeTypeValues.Removed, changed);

        public IDisposable Subscribe(IObserver<ChangedMessage<T>> observer) => this.observableSubject.Subscribe(observer);

        private void Send(ChangeTypeValues changeType, T changed) => this.observableSubject.OnNext(new ChangedMessage<T>(changeType, changed));
    }
}