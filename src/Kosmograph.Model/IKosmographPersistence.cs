using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmograph.Model
{
    public interface IKosmographPersistence
    {

        ITagRepository Tags { get; }

        ICategoryRepository Categories { get; }

        IEntityRepository Entities { get; }
    }
}
