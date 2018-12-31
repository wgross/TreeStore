using GalaSoft.MvvmLight.Command;
using Kosmograph.Messaging;
using Kosmograph.Model;
using System;
using System.Linq;

namespace Kosmograph.Desktop.Lists.ViewModel
{
    public class RelationshipRepositoryViewModel : RepositoryViewModelBase<RelationshipViewModel, IRelationship>
    {
        private readonly IRelationshipRepository repository;

        public RelationshipRepositoryViewModel(IRelationshipRepository repository, IChangedMessageBus<IRelationship> messaging)
            : base(messaging)
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
                var entity = new RelationshipViewModel(this.repository.FindById(id));
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
                this.OnRemoved(id);
            }
        }
    }
}