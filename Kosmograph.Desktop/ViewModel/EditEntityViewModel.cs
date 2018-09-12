using Kosmograph.Model;
using System;

namespace Kosmograph.Desktop.ViewModel
{
    public class EditEntityViewModel : EditNamedViewModel<Entity>
    {
        private readonly Action<Entity> committed;

        public EditEntityViewModel(Entity entity, Action<Entity> onEntityCommitted)
            : base(entity)
        {
            this.committed = onEntityCommitted;
        }

        public override void Commit()
        {
            base.Commit();
            this.committed(this.Model);
        }
    }
}