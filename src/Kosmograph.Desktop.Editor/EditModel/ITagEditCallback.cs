using Kosmograph.Model;

namespace Kosmograph.Desktop.Editors.ViewModel
{
    public interface ITagEditCallback
    {
        string Validate(TagEditModel tag);

        void Rollback(Tag tag);

        void Commit(Tag tag);

        bool CanCommit(TagEditModel tag);
    }
}