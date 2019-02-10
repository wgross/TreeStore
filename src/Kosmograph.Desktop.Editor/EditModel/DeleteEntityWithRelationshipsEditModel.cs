using Kosmograph.Desktop.Editors.ViewModel.Base;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Kosmograph.Desktop.Editors.ViewModel
{
    public class DeleteEntityWithRelationshipsEditModel : EditModelBase
    {
        private readonly Action<Entity, IEnumerable<Relationship>> committed;
        private readonly Action<Entity, IEnumerable<Relationship>> rollback;

        public DeleteEntityWithRelationshipsEditModel(Entity entity, IEnumerable<Relationship> relationships, Action<Entity, IEnumerable<Relationship>> onCommitted, Action<Entity, IEnumerable<Relationship>> onRollback)
        {
            this.Entity = entity;
            this.Relationships = new ObservableCollection<Relationship>(relationships);
            this.committed = onCommitted;
            this.rollback = onRollback;
        }

        public Entity Entity { get; set; }

        public ObservableCollection<Relationship> Relationships { get; set; }

        protected override void Commit()
        {
            this.committed(this.Entity, this.Relationships);
        }

        protected override void Rollback()
        {
            this.rollback(this.Entity, this.Relationships);
        }
    }
}