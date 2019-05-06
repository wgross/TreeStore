using GalaSoft.MvvmLight.Command;
using Kosmograph.Messaging;
using Kosmograph.Model;
using System;
using System.Linq;

namespace Kosmograph.Desktop.Lists.ViewModel
{
    public class EntityRepositoryViewModel : RepositoryViewModelBase<EntityViewModel, IEntity>, IObserver<ChangedMessage<ITag>>
    {
        private readonly IEntityRepository repository;
        private readonly IChangedMessageBus<ITag> tagChanges;
        private readonly IDisposable tagSubscriptions;

        public EntityRepositoryViewModel(IEntityRepository repository, IChangedMessageBus<IEntity> entityChanges, IChangedMessageBus<ITag> tagChanges)
            : base(entityChanges)
        {
            this.repository = repository;
            this.tagChanges = tagChanges;
            this.tagSubscriptions = this.tagChanges.Subscribe(this);
            this.DeleteCommand = new RelayCommand<EntityViewModel>(this.DeleteCommandExecuted);
        }

        public void FillAll()
        {
            foreach (var vm in this.repository.FindAll().Select(t => new EntityViewModel(t)))
                this.Add(vm);
        }

        public RelayCommand<EntityViewModel> DeleteCommand { get; }

        private void DeleteCommandExecuted(EntityViewModel viewModel)
        {
            this.repository.Delete(viewModel.Model);
        }

        override protected void OnRemoved(Guid id)
        {
            var existingEntity = this.FirstOrDefault(e => e.Model.Id.Equals(id));
            if (existingEntity is null)
                return;
            this.Remove(existingEntity);
        }

        override protected void OnUpdated(Guid id)
        {
            try
            {
                var entity = new EntityViewModel(this.repository.FindById(id));
                var indexOfEntity = this.IndexOf(entity);
                if (indexOfEntity > -1)
                {
                    // setItem doesn work here most probably because Equals is overwritten to match the Model
                    // remove/add is ok anyway because the list has to be sorted because the name might have changed.
                    this.RemoveAt(indexOfEntity);
                    this.Add(entity);
                }
                else
                    this.Add(entity);
            }
            catch (InvalidOperationException)
            {
                // throw on missing tag by Litedb -> consider map to KG exception type
                // remove item from list
                this.OnRemoved(id);
            }
        }

        #region IObserver<ChangedMessage<ITag>> Members

        void IObserver<ChangedMessage<ITag>>.OnNext(ChangedMessage<ITag> value)
        {
            var affectedEntitiesIds =
                from e in this
                where e.Tags.Any(t => t.Tag.Id.Equals(value.Changed.Id))
                select e.Model.Id;

            foreach (var id in affectedEntitiesIds.ToArray())
                this.OnUpdated(id);
        }

        void IObserver<ChangedMessage<ITag>>.OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        void IObserver<ChangedMessage<ITag>>.OnCompleted()
        {
            throw new NotImplementedException();
        }

        #endregion IObserver<ChangedMessage<ITag>> Members
    }
}