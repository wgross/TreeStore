﻿using Kosmograph.Model;

namespace Kosmograph.Desktop.EditModel
{
    public interface ITagEditCallback
    {
        void Rollback(Tag tag);

        void Commit(Tag tag);

        bool CanCommit(TagEditModel tag);
    }
}