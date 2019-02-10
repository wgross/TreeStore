using GalaSoft.MvvmLight.Command;
using Kosmograph.Model;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Kosmograph.Desktop.Editors.ViewModel
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

            public bool HasError { get; private set; }

            public bool CanCommit(TagEditModel tag)
            {
                return !this.HasError;
            }

            public void Commit(Tag tag)
            {
                this.onCommit(tag);
            }

            public void Rollback(Tag tag)
            {
                this.viewModel.OnRollback(tag);
            }

            public string Validate(TagEditModel tag)
            {
                var possibleDuplicate = this.viewModel.FindByName(tag.Name);
                if (possibleDuplicate is null || possibleDuplicate.Model.Equals(tag.Model))
                {
                    this.HasError = false;
                    return null;
                }
                else
                {
                    this.HasError = true;
                    return "Tag name must be unique";
                }
            }
        }

        public TagRepositoryViewModel(ITagRepository repository)
            : base(repository, m => new TagViewModel(m))
        {
            this.CreateCommand = new RelayCommand(this.CreateExecuted);
            this.EditCommand = new RelayCommand<Tag>(this.EditExecuted);
        }

        #region Create Tag

        public ICommand CreateCommand { get; set; }

        private void CreateExecuted()
        {
            this.Edited = new TagEditModel(new Tag("new tag", new Facet()), new TagEditCallbackHandler(this, this.OnCreateCommitted));
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

        private void EditExecuted(Tag tag)
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