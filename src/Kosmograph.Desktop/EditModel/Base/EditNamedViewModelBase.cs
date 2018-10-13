using Kosmograph.Model.Base;

namespace Kosmograph.Desktop.EditModel.Base
{
    public class EditNamedViewModelBase<T> : EditModelBase
            where T : NamedItemBase
    {
        public T Model { get; private set; }

        public EditNamedViewModelBase(T edited)
        {
            this.Model = edited;
        }

        protected override void Commit()
        {
            this.Model.Name = this.Name;
        }

        protected override void Rollback()
        {
            this.Name = this.Model.Name;
        }

        public string Name
        {
            get => this.name ?? this.Model.Name;
            set => this.Set(nameof(Name), ref this.name, value);
        }

        private string name;
    }
}