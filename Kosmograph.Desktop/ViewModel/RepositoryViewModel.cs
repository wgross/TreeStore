using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Kosmograph.Desktop.ViewModel.Base;
using Kosmograph.Model;
using Kosmograph.Model.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Kosmograph.Desktop.ViewModel
{
    public class RepositoryViewModel<VM, M> : ObservableCollection<VM>
        where M : NamedItemBase
        where VM : NamedViewModelBase<M>
    {
        public class Deleted<M>
        {
            public Deleted(IEnumerable<M> deleted)
            {
                this.Items = deleted;
            }

            public IEnumerable<M> Items { get; }
        }

        private readonly IRepository<M> repository;
        private readonly Func<M, VM> newViewModel;

        private readonly IDictionary<Guid, VM> locals = new Dictionary<Guid, VM>();
        private bool filling;

        public RepositoryViewModel(IRepository<M> repository, Func<M, VM> newViewModel)
        {
            this.repository = repository;
            this.newViewModel = newViewModel;

            this.DeleteCommand = new RelayCommand<VM>(this.DeleteViewModelExecuted);
        }

        #region Delete an item

        public RelayCommand<VM> DeleteCommand { get; }

        private void DeleteViewModelExecuted(VM viewModel)
        {
            this.Remove(viewModel);
        }

        #endregion Delete an item

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var viewModel in e.NewItems.OfType<VM>())
                    {
                        if (!this.filling)
                            this.repository.Upsert(viewModel.Model);
                        this.locals[viewModel.Model.Id] = viewModel;
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    var deleted = new List<M>();
                    foreach (var viewModel in e.OldItems.OfType<VM>())
                    {
                        if (!this.filling)
                        {
                            if (this.repository.Delete(viewModel.Model.Id))
                            {
                                deleted.Add(viewModel.Model);
                                this.locals.Remove(viewModel.Model.Id);
                            }
                        }
                    }
                    Messenger.Default.Send(new GenericMessage<string>("hallo"));
                    break;
            }
            base.OnCollectionChanged(e);
        }

        public void FillAll()
        {
            try
            {
                this.filling = true;
                foreach (var m in this.repository.FindAll())
                    this.Add(this.newViewModel(m));
            }
            finally
            {
                this.filling = false;
            }
        }

        public VM GetViewModel(M tag) => this.locals.TryGetValue(tag.Id, out var viewModel) ? viewModel : throw new InvalidOperationException("model unknown");
    }

    public class TagRepositoryViewModel : RepositoryViewModel<TagViewModel, Tag>
    {
        public TagRepositoryViewModel(ITagRepository repository)
            : base(repository, m => new TagViewModel(m))
        {
        }
    }

    public class EntityRepositoryViewModel : RepositoryViewModel<EntityViewModel, Entity>
    {
        public EntityRepositoryViewModel(IEntityRepository model)
            : base(model, m => new EntityViewModel(m))
        {
        }
    }

    public class RelationshipRepositoryViewModel : RepositoryViewModel<RelationshipViewModel, Relationship>
    {
        public RelationshipRepositoryViewModel(IRelationshipRepository model)
            : base(model, m => new RelationshipViewModel(m))
        {
            //Messenger.Default.Register<GenericMessage<Deleted<Entity>>>(this, this.OnEntityDeleted);
            Messenger.Default.Register<GenericMessage<string>>(this, this.OnEntityDeleted);
        }

        private void OnEntityDeleted(GenericMessage<string> msg)
        {
            return;
        }

        private void OnEntityDeleted(GenericMessage<Deleted<Entity>> obj)
        {
            foreach (var relationship in this.Where(r => obj.Content.Items.Contains(r.From.Model) || obj.Content.Items.Contains(r.To.Model)))
                this.Remove(relationship);
        }
    }
}