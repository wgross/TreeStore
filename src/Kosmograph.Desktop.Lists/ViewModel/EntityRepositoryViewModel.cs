using Kosmograph.Messaging;
using Kosmograph.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Kosmograph.Desktop.Lists.ViewModel
{
    public class EntityRepositoryViewModel : ObservableCollection<EntityViewModel>, IObserver<ChangedMessage<IEntity>>
    {
        private readonly IEntityRepository repository;
        private readonly IChangedMessageBus<IEntity> messaging;

        public EntityRepositoryViewModel(IEntityRepository repository, IChangedMessageBus<IEntity> messaging)
        {
            this.repository = repository;
            this.messaging = messaging;
            this.messaging.Subscribe(this);
        }

        public void FillAll()
        {
            foreach (var vm in this.repository.FindAll().Select(t => new EntityViewModel(t)))
                this.Add(vm);
        }

        #region IObserver<ChangedMessage<IEntity>>

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(ChangedMessage<IEntity> value)
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
            var existingEntity = this.FirstOrDefault(e => e.Model.Id.Equals(id));
            if (existingEntity is null)
                return;
            this.Remove(existingEntity);
        }

        private void Update(Guid id)
        {
            try
            {
                var entity = new EntityViewModel(this.repository.FindById(id));
                var indexOfTag = this.IndexOf(entity);
                if (indexOfTag > -1)
                    this.SetItem(indexOfTag, entity);
                else
                    this.Add(entity);
            }
            catch (InvalidOperationException)
            {
                // throw on missing tag by Litedb -> consider map to KG exception type
                // remove item from list
                this.Remove(id);
            }
        }

        #endregion IObserver<ChangedMessage<IEntity>>
    }
}