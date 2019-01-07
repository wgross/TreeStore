using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Kosmograph.Messaging;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Kosmograph.Desktop.Editors.ViewModel
{
    public class KosmographViewModel : ViewModelBase, IDisposable, IObserver<ChangedMessage<ITag>>
    {
        private KosmographModel model;

        public KosmographViewModel(KosmographModel kosmographModel)
        {
            this.model = kosmographModel;
            this.Tags = new TagRepositoryViewModel(this.model.Tags);
            this.Entities = new EntityRepositoryViewModel(this.model.Entities, this.Tags.GetViewModel);
            this.Relationships = new RelationshipRepositoryViewModel(this.model.Relationships, this.Entities.GetViewModel, this.Tags.GetViewModel);
            this.DeleteEntityCommand = new RelayCommand<EntityViewModel>(this.OnDeletingEntity);
        }

        public KosmographViewModel(IKosmographMessageBus kosmographMessaging, KosmographModel kosmographModel)
        {
            this.kosmographMessaging = kosmographMessaging;
            this.kosmographMessaging.Tags.Subscribe(this);
            this.kosmographModel = kosmographModel;
        }

        public void FillAll()
        {
            this.Tags.FillAll();
            this.Entities.FillAll();
            this.Relationships.FillAll();
        }

        public KosmographModel Model => this.model;

        public TagRepositoryViewModel Tags { get; }

        public EntityRepositoryViewModel Entities { get; }

        public RelationshipRepositoryViewModel Relationships { get; }

        public TagViewModel SelectedTag
        {
            get => this.selectedTag;
            set => this.Set(nameof(SelectedTag), ref this.selectedTag, value);
        }

        private TagViewModel selectedTag;

        public EntityViewModel SelectedEntity
        {
            get => this.selectedEntity;
            set => this.Set(nameof(SelectedEntity), ref this.selectedEntity, value);
        }

        private EntityViewModel selectedEntity;

        public RelationshipViewModel SelectedRelationship
        {
            get => this.selectedRelationship;
            set => this.Set(nameof(SelectedRelationship), ref this.selectedRelationship, value);
        }

        public RelationshipViewModel selectedRelationship;

        #region Delete Entity with Relastionships

        public ICommand DeleteEntityCommand { get; }

        private void OnDeletingEntity(EntityViewModel entityViewModel)
        {
            this.DeletingEntity = new DeleteEntityWithRelationshipsEditModel(entityViewModel,
                this.Relationships.FindRelationshipByEntity(entityViewModel),
                this.OnDeleteEntityCommited,
                this.OnDeleteEntityRollback);
        }

        private void OnDeleteEntityRollback(EntityViewModel arg1, IEnumerable<RelationshipViewModel> arg2)
        {
            this.DeletingEntity = null;
        }

        private void OnDeleteEntityCommited(EntityViewModel entity, IEnumerable<RelationshipViewModel> relationships)
        {
            relationships.ToList().ForEach(r => this.Relationships.DeleteCommand.Execute(r));
            this.Entities.DeleteCommand.Execute(entity);
            this.DeletingEntity = null;
        }

        public DeleteEntityWithRelationshipsEditModel DeletingEntity
        {
            get => this.deletingEntity;
            set
            {
                this.Set(nameof(DeletingEntity), ref this.deletingEntity, value);
            }
        }

        private DeleteEntityWithRelationshipsEditModel deletingEntity;
        private IKosmographMessageBus kosmographMessaging;
        private KosmographModel kosmographModel;

        public void Dispose()
        {
            this.model.Dispose();
        }

        #endregion Delete Entity with Relastionships

        #region Observe changes in tags persistence

        public void OnNext(ChangedMessage<ITag> value)
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        #endregion Observe changes in tags persistence
    }
}