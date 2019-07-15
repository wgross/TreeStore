using Kosmograph.Model.Base;

namespace Kosmograph.Desktop.Editors.ViewModel.Base
{
    public abstract class NamedEditModelBase<M> : EditModelBase
            where M : NamedBase
    {
        public M Model { get; }

        public NamedEditModelBase(M edited)
        {
            this.Model = edited;
        }

        protected override void Commit()
        {
            this.Model.Name = this.Name;
            this.name = null;
        }

        protected override void Rollback()
        {
            // nullifying the name avoid validating in case of rollback
            this.name = null;
        }

        public string Name
        {
            get => this.name ?? this.Model.Name;
            set
            {
                if (this.Set(nameof(Name), ref this.name, value?.Trim()))
                    this.Validate();
            }
        }

        private string name;

        public string NameError
        {
            get => this.nameError;
            protected set => this.Set(nameof(NameError), ref this.nameError, value?.Trim());
        }

        private string nameError;

        #region Validate data and indicate error

        protected abstract void Validate();

        public bool HasErrors { get; protected set; }

        #endregion Validate data and indicate error
    }
}