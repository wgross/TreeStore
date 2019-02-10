using GalaSoft.MvvmLight.Command;
using Kosmograph.Messaging;
using Kosmograph.Model;
using System;
using System.Linq;

namespace Kosmograph.Desktop.Lists.ViewModel
{
    public class TagRepositoryViewModel : RepositoryViewModelBase<TagViewModel, ITag>
    {
        private readonly ITagRepository repository;

        public TagRepositoryViewModel(ITagRepository repository, IChangedMessageBus<ITag> messaging)
            : base(messaging)
        {
            this.repository = repository;
            this.DeleteCommand = new RelayCommand<TagViewModel>(this.DeleteCommandExecuted);
        }

        public void FillAll()
        {
            foreach (var vm in this.repository.FindAll().Select(t => new TagViewModel(t)))
                this.Add(vm);
        }

        #region Provide command for the view

        public RelayCommand<TagViewModel> DeleteCommand { get; }

        private void DeleteCommandExecuted(TagViewModel viewModel)
        {
            this.repository.Delete(viewModel.Model);
        }

        #endregion Provide command for the view

        override protected void OnRemoved(Guid id)
        {
            var existingTag = this.FirstOrDefault(t => t.Model.Id.Equals(id));
            if (existingTag is null)
                return;
            this.Remove(existingTag);
        }

        override protected void OnUpdated(Guid id)
        {
            try
            {
                var tag = new TagViewModel(this.repository.FindById(id));
                var indexOfTag = this.IndexOf(tag);
                if (indexOfTag > -1)
                    this.SetItem(indexOfTag, tag);
                else
                    this.Add(tag);
            }
            catch (InvalidOperationException)
            {
                // throw on missing tag by Litedb -> consider map to KG exception type
                // remove item from list
                this.OnRemoved(id);
            }
        }

        public TagViewModel FindByName(string name)
        {
            throw new NotImplementedException();
        }
    }
}