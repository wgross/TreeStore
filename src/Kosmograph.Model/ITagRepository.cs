using System;
using System.Collections.Generic;

namespace Kosmograph.Model
{
    public interface ITagRepository
    {
        Tag FindById(Guid id);

        IEnumerable<Tag> FindAll();
    }
}