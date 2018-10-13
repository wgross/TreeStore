using GalaSoft.MvvmLight.Command;
using Kosmograph.Desktop.EditModel;
using Kosmograph.Model;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Kosmograph.Desktop.ViewModel
{
    public class TagRepositoryViewModel : RepositoryViewModel<TagViewModel, Tag>
    {
        private sealed class TagEditCallbackHandler : ITagEditCallback
        {
            private readonly TagRepositoryViewModel viewModel;
            private readonly Action<Tag> onCommit;

            public TagEditCallbackHandler(TagRepositoryViewModel viewModel, Action<Tag> onCommit)
            {
                this.viewModel = viewModel;
                this.onCommit = onCommit;
            }

            public bool CanCommit(TagEditModel tag)
            {
                throw new System.NotImplementedException();
            }

            public void Commit(Tag tag)
            {
                this.onCommit(tag);
            }

            public void Rollback(Tag tag)
            {
                this.viewModel.OnRollback(tag);
            }
        }

        public TagRepositoryViewModel(ITagRepository repository)
            : base(repository, m => new TagViewModel(m))
        {
            this.CreateCommand = new RelayCommand(this.CreateExecuted);
            this.EditCommand = new RelayCommand<TagViewModel>(this.EditExecuted);
        }

        #region Create Tag

        public ICommand CreateCommand { get; set; }

        private void CreateExecuted()
        {
            this.Edited = new TagEditModel(new TagViewModel(new Tag("new tag", new Facet())), new TagEditCallbackHandler(this, this.OnCreateCommitted));
        }

        public TagEditModel Edited
        {
            get => this.edited;
            private set
            {
                this.edited = value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Edited)));
            }
        }

        private TagEditModel edited;

        private void OnRollback(Tag obj)
        {
            this.Edited = null;
        }

        private void OnCreateCommitted(Tag entity)
        {
            this.Add(CreateViewModel(entity));
            this.Edited = null;
        }

        #endregion Create Tag

        #region Edit Tag

        public ICommand EditCommand { get; }

        private void EditExecuted(TagViewModel tag)
        {
            this.Edited = new TagEditModel(tag, new TagEditCallbackHandler(this, this.OnEditCommitted));
        }

        private void OnEditCommitted(Tag obj)
        {
            this.Edited = null;
        }

        #endregion Edit Tag
    }
}