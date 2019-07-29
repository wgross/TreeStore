using Kosmograph.Model;
using System;
using System.Windows.Input;

namespace Kosmograph.Desktop.ViewModel
{
    public partial class KosmographViewModel
    {
        public Lists.ViewModel.TagRepositoryViewModel Tags { get; }

        private Lists.ViewModel.TagViewModel selectedTag;

        public Lists.ViewModel.TagViewModel SelectedTag
        {
            get => this.selectedTag;
            set => this.Set(nameof(SelectedTag), ref this.selectedTag, value);
        }

        #region Edit Tag

        private sealed class TagEditCallbackHandler : Editors.ViewModel.ITagEditCallback
        {
            private readonly ITagRepository tagRepository;
            private readonly Action<Tag> onCommit;
            private readonly Action<Tag> onRollback;

            public TagEditCallbackHandler(ITagRepository tagRepository, Action<Tag> onCommit, Action<Tag> onRollback)
            {
                this.tagRepository = tagRepository;
                this.onCommit = onCommit;
                this.onRollback = onRollback;
            }

            public bool HasError { get; private set; }

            public bool CanCommit(Editors.ViewModel.TagEditModel tag) => !this.HasError;

            public void Commit(Tag tag) => this.onCommit(tag);

            public void Rollback(Tag tag) => this.onRollback(tag);

            public string Validate(Editors.ViewModel.TagEditModel tag)
            {
                var possibleDuplicate = this.tagRepository.FindByName(tag.Name);
                if (possibleDuplicate is null || possibleDuplicate.Equals(tag.Model))
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

        public ICommand EditTagByIdCommand { get; }

        private Editors.ViewModel.TagEditModel editedTag;

        public Editors.ViewModel.TagEditModel EditedTag
        {
            get => this.editedTag;
            set => this.Set(nameof(EditedTag), ref this.editedTag, value);
        }

        private void EditTagByIdExecuted(Guid tagId) => this.EditTag(this.Model.Tags.FindById(tagId));

        private void EditTag(Tag tag) => this.EditedTag = new Editors.ViewModel.TagEditModel(tag, new TagEditCallbackHandler(this.model.Tags, this.OnEditTagCommitted, this.EditTagRollback));

        private void EditTagRollback(Tag obj) => this.EditedTag = null;

        private void OnEditTagCommitted(Tag obj)
        {
            this.model.Tags.Upsert(obj);
            this.EditedTag = null;
        }

        #endregion Edit Tag

        #region Create Tag

        public ICommand CreateTagCommand { get; set; }

        private void CreateTagExecuted()
        {
            this.EditedTag = new Editors.ViewModel.TagEditModel(new Tag("new tag", new Facet()), new TagEditCallbackHandler(this.model.Tags, this.CreateTagCommitted, this.CreateTagRollback));
        }

        private void CreateTagRollback(Tag obj) => this.EditedTag = null;

        private void CreateTagCommitted(Tag tag)
        {
            this.model.Tags.Upsert(tag);
            this.EditedTag = null;
        }

        #endregion Create Tag

        #region Delete Tag

        public ICommand DeleteTagByIdCommand { get; }

        private void DeleteTagByIdExecuted(Guid tagId) => this.DeleteTag(this.Model.Tags.FindById(tagId));

        private void DeleteTag(Tag tag) => this.Model.Tags.Delete(tag);

        #endregion Delete Tag
    }
}