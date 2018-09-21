using Kosmograph.Desktop.EditModel.Base;
using Kosmograph.Desktop.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Kosmograph.Desktop.EditModel
{
    public class DeleteEntityWithRelationshipsEditModel : EditModelBase
    {
        private readonly Action<EntityViewModel, IEnumerable<RelationshipViewModel>> committed;
        private readonly Action<EntityViewModel, IEnumerable<RelationshipViewModel>> rollback;
        private EntityViewModel entityViewModel;
        private IEnumerable<RelationshipViewModel> enumerable;
        private Action<EntityViewModel, IEnumerable<RelationshipViewModel>> onDeleteEntityCommited;

        public DeleteEntityWithRelationshipsEditModel(EntityViewModel entityViewModel, IEnumerable<RelationshipViewModel> relationships, Action<EntityViewModel, IEnumerable<RelationshipViewModel>> onCommitted, Action<EntityViewModel, IEnumerable<RelationshipViewModel>> onRollback)
        {
            this.Entity = entityViewModel;
            this.Relationships = new ObservableCollection<RelationshipViewModel>(relationships);
            this.committed = onCommitted;
            this.rollback = onRollback;
        }

        public EntityViewModel Entity { get; set; }

        public ObservableCollection<RelationshipViewModel> Relationships { get; set; }

        public override void Commit()
        {
            this.committed(this.Entity, this.Relationships);
        }

        public override void Rollback()
        {
            this.rollback(this.Entity, this.Relationships);
        }
    }
}