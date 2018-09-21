using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Kosmograph.Desktop.EditModel;
using Kosmograph.Desktop.ViewModel.Base;
using Kosmograph.Model;
using Kosmograph.Model.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

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

        public VM GetViewModel(M model) => this.locals.TryGetValue(model.Id, out var viewModel) ? viewModel : throw new InvalidOperationException("model unknown");

        public VM CreateViewModel(M model) => this.newViewModel(model);
    }

    public class TagRepositoryViewModel : RepositoryViewModel<TagViewModel, Tag>
    {
        public TagRepositoryViewModel(ITagRepository repository)
            : base(repository, m => new TagViewModel(m))
        {
            this.CreateCommand = new RelayCommand(this.CreateExecuted);
            this.EditCommand = new RelayCommand<TagViewModel>(this.EditExecuted);
        }

        #region Create Tag

        public ICommand CreateCommand { get; set; }

        private void CreateExecuted()
        {
            this.Edited = new TagEditModel(new TagViewModel(new Tag("new tag", new Facet())), this.OnCreateCommitted, this.OnRollback);
        }

        public TagEditModel Edited
        {
            get => this.edited;
            private set
            {
                this.edited = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Edited)));
            }
        }

        private TagEditModel edited;

        private void OnRollback(Tag obj)
        {
            this.Edited = null;
        }

        private void OnCreateCommitted(Tag entity)
        {
            this.Add(CreateViewModel(entity));
            this.Edited = null;
        }

        #endregion Create Tag

        #region Edit Tag

        public ICommand EditCommand { get; }

        private void EditExecuted(TagViewModel tag)
        {
            this.Edited = new TagEditModel(tag, this.OnEditCommitted, this.OnRollback);
        }

        private void OnEditCommitted(Tag obj)
        {
            this.Edited = null;
        }

        #endregion Edit Tag
    }

    public class EntityRepositoryViewModel : RepositoryViewModel<EntityViewModel, Entity>
    {
        public EntityRepositoryViewModel(IEntityRepository model, Func<Tag, TagViewModel> newTagViewModel)
            : base(model, m => NewViewModel(m, newTagViewModel))
        {
            this.CreateCommand = new RelayCommand(this.CreateExecuted);
            this.EditCommand = new RelayCommand<EntityViewModel>(this.EditExecuted);
        }

        private static EntityViewModel NewViewModel(Entity model, Func<Tag, TagViewModel> newTagViewModel)
        {
            return new EntityViewModel(model, model.Tags.Select(newTagViewModel).ToArray());
        }

        #region Create Entity

        public EntityEditModel Edited
        {
            get => this.edited;
            private set
            {
                this.edited = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Edited)));
            }
        }

        private EntityEditModel edited;

        public ICommand CreateCommand { get; }

        private void CreateExecuted()
        {
            this.Edited = new EntityEditModel(new EntityViewModel(new Entity("new entity")), this.OnCreateCommitted, this.OnRollback);
        }

        private void OnRollback(Entity obj)
        {
            this.Edited = null;
        }

        private void OnCreateCommitted(Entity entity)
        {
            this.Add(CreateViewModel(entity));
            this.Edited = null;
        }

        #endregion Create Entity

        #region Edit Entity

        public ICommand EditCommand { get; }

        private void EditExecuted(EntityViewModel entity)
        {
            this.Edited = new EntityEditModel(entity, this.OnEditCommitted, this.OnRollback);
        }

        private void OnEditCommitted(Entity obj)
        {
            this.Edited = null;
        }

        #endregion Edit Entity
    }

    public class RelationshipRepositoryViewModel : RepositoryViewModel<RelationshipViewModel, Relationship>
    {
        public RelationshipRepositoryViewModel(IRelationshipRepository model, Func<Entity, EntityViewModel> newEntityViewModel, Func<Tag, TagViewModel> newTagViewModel)
            : base(model, m => NewViewModel(m, newEntityViewModel, newTagViewModel))
        {
            this.CreateCommand = new RelayCommand(this.CreateExecuted);
            this.EditCommand = new RelayCommand<RelationshipViewModel>(this.EditExecuted);
        }

        private static RelationshipViewModel NewViewModel(Relationship model, Func<Entity, EntityViewModel> newEntityViewModel, Func<Tag, TagViewModel> newTagViewModel)

        {
            return new RelationshipViewModel(model,
                model.To is null ? null : newEntityViewModel(model.To),
                model.From is null ? null : newEntityViewModel(model.From),
                model.Tags.Select(newTagViewModel).ToArray());
        }

        #region Create relationship

        public ICommand CreateCommand { get; set; }

        private void CreateExecuted()
        {
            this.Edited = new RelationshipEditModel(this.CreateViewModel(new Relationship("new relationship", from: null, to: null)),
                this.OnCreateCommitted, this.OnRollBack);
        }

        private void OnRollBack(Relationship obj)
        {
            this.Edited = null;
        }

        private void OnCreateCommitted(Relationship relationship)
        {
            this.Add(CreateViewModel(relationship));
            this.Edited = null;
        }

        public RelationshipEditModel Edited
        {
            get => this.edited;
            private set
            {
                this.edited = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Edited)));
            }
        }

        private RelationshipEditModel edited;

        #endregion Create relationship

        #region Edit Relationship

        public ICommand EditCommand { get; set; }

        private void EditExecuted(RelationshipViewModel viewModel)
        {
            this.Edited = new RelationshipEditModel(viewModel, this.OnEditCommitted, this.OnRollBack);
        }

        private void OnEditCommitted(Relationship relationship)
        {
            this.Edited = null;
        }

        #endregion Edit Relationship
    }
}