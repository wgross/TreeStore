using GalaSoft.MvvmLight.Command;
using Kosmograph.Messaging;
using Kosmograph.Model;
using System;
using System.Linq;

namespace Kosmograph.Desktop.Lists.ViewModel
{
    public class RelationshipRepositoryViewModel : TaggedRepositoryViewModelBase<RelationshipViewModel, Relationship, IRelationship>
    {
        private readonly IRelationshipRepository repository;

        public RelationshipRepositoryViewModel(IRelationshipRepository repository, IChangedMessageBus<IRelationship> relationshipMessaging, IChangedMessageBus<ITag> tagMessaging)
            : base(relationshipMessaging, tagMessaging)
        {
            this.repository = repository;
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
    }
}