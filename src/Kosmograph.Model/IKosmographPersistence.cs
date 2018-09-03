using System;
using System.Collections.Generic;
using System.Text;

namespace Kosmograph.Model
{
    public interface IKosmographPersistence
    {

        ICategoryRepository Categories { get; set; }
    }
}
