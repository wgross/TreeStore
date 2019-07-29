using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Kosmograph.Desktop.Graph.ViewModel;
using Kosmograph.Desktop.Lists.ViewModel;
using Kosmograph.Messaging;
using Kosmograph.Model;
using System;

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
            this.Relationships = new Lists.ViewModel.RelationshipRepositoryViewModel(this.model.Relationships, KosmographMessageBus.Default.Relationships, KosmographMessageBus.Default.Tags);
            this.Graph = new GraphXViewerViewModel(kosmographModel);
            this.EditTagByIdCommand = new RelayCommand<Guid>(this.EditTagByIdExecuted);
            this.EditEntityByIdCommand = new RelayCommand<Guid>(this.EditEntityByIdExecuted);
            this.EditRelationshipByIdCommand = new RelayCommand<Guid>(this.EditRelationshipByIdExecuted);
            this.CreateTagCommand = new RelayCommand(this.CreateTagExecuted);
            this.CreateEntityCommand = new RelayCommand(this.CreateEntityExecuted);
            this.CreateRelationshipCommand = new RelayCommand(this.CreateRelationshipExecuted);
            this.DeleteTagByIdCommand = new RelayCommand<Guid>(this.DeleteTagByIdExecuted);
            this.DeleteEntityByIdCommand = new RelayCommand<Guid>(this.DeleteEntityByIdExecuted);
            this.DeleteRelationshipByIdCommand = new RelayCommand<Guid>(this.DeleteRelationshipByIdExecuted);
        }

        public void FillAll()
        {
            this.Tags.FillAll();
            this.Entities.FillAll();
            this.Relationships.FillAll();
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