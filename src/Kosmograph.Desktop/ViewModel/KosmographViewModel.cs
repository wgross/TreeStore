using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Kosmograph.Desktop.EditModel;
using Kosmograph.Desktop.Lists.ViewModel;
using Kosmograph.Messaging;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Kosmograph.Desktop.ViewModel
{
    public partial class KosmographViewModel : ViewModelBase, IDisposable
    {
        private KosmographModel model;

        public KosmographViewModel(KosmographModel kosmographModel)
        {
            this.model = kosmographModel;
            this.Tags = new Lists.ViewModel.TagRepositoryViewModel(this.model.Tags, KosmographMessageBus.Default.Tags);
            this.Entities = new Lists.ViewModel.EntityRepositoryViewModel(this.model.Entities, KosmographMessageBus.Default.Entities);
            this.Relationships = new Lists.ViewModel.RelationshipRepositoryViewModel(this.model.Relationships, KosmographMessageBus.Default.Relationships);
            //this.Entities = new EntityRepositoryViewModel(this.model.Entities, this.Tags.GetViewModel);
            //this.Relationships = new RelationshipRepositoryViewModel(this.model.Relationships, this.Entities.GetViewModel, this.Tags.GetViewModel);
            this.EditTagCommand = new RelayCommand<Lists.ViewModel.TagViewModel>(this.EditTagExecuted);
            this.EditEntityCommand = new RelayCommand<Lists.ViewModel.EntityViewModel>(this.EditEntityExecuted);
            this.EditRelationshipCommand = new RelayCommand<Lists.ViewModel.RelationshipViewModel>(this.EditRelationshipExecuted);
            this.CreateTagCommand = new RelayCommand(this.CreateTagExecuted);
            this.CreateEntityCommand = new RelayCommand(this.CreateEntityExecuted);
            this.CreateRelationshipCommand = new RelayCommand(this.CreateRelationshipExecuted);
            this.DeleteTagCommand = new RelayCommand<TagViewModel>(this.DeleteTagExecuted);
            this.DeleteEntityCommand = new RelayCommand<EntityViewModel>(this.OnDeletingEntity);
        }

        public void FillAll()
        {
            this.Tags.FillAll();
            this.Entities.FillAll();
            this.Relationships.FillAll();
        }

        public KosmographModel Model => this.model;

        #region Delete Entity with Relastionships

        public ICommand DeleteEntityCommand { get; }

        private void OnDeletingEntity(EntityViewModel entityViewModel)
        {
            this.DeletingEntity = new DeleteEntityWithRelationshipsEditModel(entityViewModel,
                null, // this.Relationships.FindRelationshipByEntity(entityViewModel),
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

        public void Dispose()
        {
            this.model.Dispose();
        }

        #endregion Delete Entity with Relastionships
    }
}