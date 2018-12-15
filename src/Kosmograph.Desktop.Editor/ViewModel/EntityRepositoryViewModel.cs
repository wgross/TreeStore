using GalaSoft.MvvmLight.Command;
using Kosmograph.Desktop.EditModel;
using Kosmograph.Model;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Kosmograph.Desktop.ViewModel
{
    public class EntityRepositoryViewModel : RepositoryViewModel<EntityViewModel, Entity>
    {
        public EntityRepositoryViewModel(IEntityRepository model, Func<Tag, TagViewModel> newTagViewModel)
            : base(model, m => NewViewModel(m, newTagViewModel))
        {
            this.CreateCommand = new RelayCommand(this.CreateExecuted);
            this.EditCommand = new RelayCommand<EntityViewModel>(this.EditExecuted);
        }

        private static EntityViewModel NewViewModel(Entity model, Func<Tag, TagViewModel> newTagViewModel)
        {
            return new EntityViewModel(model, model.Tags.Select(newTagViewModel).ToArray());
        }

        #region Create Entity

        public EntityEditModel Edited
        {
            get => this.edited;
            private set
            {
                this.edited = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Edited)));
            }
        }

        private EntityEditModel edited;

        public ICommand CreateCommand { get; }

        private void CreateExecuted()
        {
            this.Edited = new EntityEditModel(new EntityViewModel(new Entity("new entity")), this.OnCreateCommitted, this.OnRollback);
        }

        private void OnRollback(Entity obj)
        {
            this.Edited = null;
        }

        private void OnCreateCommitted(Entity entity)
        {
            this.Add(CreateViewModel(entity));
            this.Edited = null;
        }

        #endregion Create Entity

        #region Edit Entity

        public ICommand EditCommand { get; }

        private void EditExecuted(EntityViewModel entity)
        {
            this.Edited = new EntityEditModel(entity, this.OnEditCommitted, this.OnRollback);
        }

        private void OnEditCommitted(Entity obj)
        {
            this.Edited = null;
        }

        #endregion Edit Entity
    }
}