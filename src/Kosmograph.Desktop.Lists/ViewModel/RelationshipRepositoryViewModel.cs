using GalaSoft.MvvmLight.Command;
using Kosmograph.Messaging;
using Kosmograph.Model;
using System;
using System.Linq;

namespace Kosmograph.Desktop.Lists.ViewModel
{
    public class RelationshipRepositoryViewModel : RepositoryViewModelBase<RelationshipViewModel, IRelationship>, IObserver<ChangedMessage<ITag>>
    {
        private readonly IRelationshipRepository repository;
        private readonly IChangedMessageBus<ITag> tagMessaging;
        private readonly IDisposable tagSubscriptions;

        public RelationshipRepositoryViewModel(IRelationshipRepository repository, IChangedMessageBus<IRelationship> relationshipMessaging, IChangedMessageBus<ITag> tagMessaging)
            : base(relationshipMessaging)
        {
            this.repository = repository;
            this.tagMessaging = tagMessaging;
            this.tagSubscriptions = this.tagMessaging.Subscribe(this);
            this.DeleteCommand = new RelayCommand<RelationshipViewModel>(this.DeleteCommandExecuted);
        }

        public void FillAll()
        {
            foreach (var vm in this.repository.FindAll().Select(t => new RelationshipViewModel(t)))
                this.Add(vm);
        }

        public RelayCommand<RelationshipViewModel> DeleteCommand { get; }

        private void DeleteCommandExecuted(RelationshipViewModel viewModel)
        {
            this.repository.Delete(viewModel.Model);
        }

        override protected void OnRemoved(Guid id)
        {
            var existingRelationship = this.FirstOrDefault(e => e.Model.Id.Equals(id));
            if (existingRelationship is null)
                return;
            this.Remove(existingRelationship);
        }

        override protected void OnUpdated(Guid id)
        {
            try
            {
                var relationship = new RelationshipViewModel(this.repository.FindById(id));
                var indexOfRelationship = this.IndexOf(relationship);
                if (indexOfRelationship > -1)
                {
                    // setItem doesn work here most probably because Equals is overwritten to match the Model
                    // remove/add is ok anyway because the list has to be sorted because the name might have changed.
                    this.RemoveAt(indexOfRelationship);
                    this.Add(relationship);
                }
                else
                    this.Add(relationship);
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
            var affectedRelationshipIds =
                from e in this
                where e.Tags.Any(t => t.Tag.Id.Equals(value.Changed.Id))
                select e.Model.Id;

            foreach (var id in affectedRelationshipIds.ToArray())
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