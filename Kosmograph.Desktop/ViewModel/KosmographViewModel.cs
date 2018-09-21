using GalaSoft.MvvmLight;
using Kosmograph.Model;

namespace Kosmograph.Desktop.ViewModel
{
    public class KosmographViewModel : ViewModelBase
    {
        private KosmographModel model;

        public KosmographViewModel(KosmographModel kosmographModel)
        {
            this.model = kosmographModel;
            this.Tags = new TagRepositoryViewModel(this.model.Tags);
            this.Entities = new EntityRepositoryViewModel(this.model.Entities, this.Tags.GetViewModel);
            this.Relationships = new RelationshipRepositoryViewModel(this.model.Relationships, this.Entities.GetViewModel, this.Tags.GetViewModel);
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
    }
}