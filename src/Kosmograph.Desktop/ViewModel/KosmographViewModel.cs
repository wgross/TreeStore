using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Kosmograph.Desktop.Graph.ViewModel;
using Kosmograph.Desktop.Lists.ViewModel;
using Kosmograph.Messaging;
using Kosmograph.Model;
using System;
using System.Linq;

namespace Kosmograph.Desktop.ViewModel
{
    public partial class KosmographViewModel : ViewModelBase, IDisposable
    {
        private KosmographModel model;

        public KosmographViewModel(KosmographModel kosmographModel)
        {
            this.model = kosmographModel;
            this.Tags = new Lists.ViewModel.TagRepositoryViewModel(this.model.Tags, KosmographMessageBus.Default.Tags);
            this.Entities = new Lists.ViewModel.EntityRepositoryViewModel(this.model.Entities, KosmographMessageBus.Default.Entities, KosmographMessageBus.Default.Tags);
            this.Relationships = new Lists.ViewModel.RelationshipRepositoryViewModel(this.model.Relationships, KosmographMessageBus.Default.Relationships);
            this.Graph = new GraphXViewerViewModel(KosmographMessageBus.Default);
            //this.Entities = new EntityRepositoryViewModel(this.model.Entities, this.Tags.GetViewModel);
            //this.Relationships = new RelationshipRepositoryViewModel(this.model.Relationships, this.Entities.GetViewModel, this.Tags.GetViewModel);
            this.EditTagCommand = new RelayCommand<Lists.ViewModel.TagViewModel>(this.EditTagExecuted);
            this.EditEntityCommand = new RelayCommand<Lists.ViewModel.EntityViewModel>(this.EditEntityExecuted);
            this.EditRelationshipCommand = new RelayCommand<Lists.ViewModel.RelationshipViewModel>(this.EditRelationshipExecuted);
            this.CreateTagCommand = new RelayCommand(this.CreateTagExecuted);
            this.CreateEntityCommand = new RelayCommand(this.CreateEntityExecuted);
            this.CreateRelationshipCommand = new RelayCommand(this.CreateRelationshipExecuted);
            this.DeleteTagCommand = new RelayCommand<TagViewModel>(this.DeleteTagExecuted);
            this.DeleteEntityCommand = new RelayCommand<EntityViewModel>(this.DeleteEntityExecuted);
            this.DeleteRelationshipCommand = new RelayCommand<RelationshipViewModel>(this.DeleteRelationshipExecuted);
        }

        public void FillAll()
        {
            this.Tags.FillAll();
            this.Entities.FillAll();
            this.Relationships.FillAll();
            this.Graph.Show(this.Entities.Select(e => e.Model));
            this.Graph.Show(this.Relationships.Select(r => r.Model));
        }

        public KosmographModel Model => this.model;

        public GraphXViewerViewModel Graph { get; }

        #region IDisposable Members

        public void Dispose()
        {
            this.model.Dispose();
        }

        #endregion IDisposable Members
    }
}