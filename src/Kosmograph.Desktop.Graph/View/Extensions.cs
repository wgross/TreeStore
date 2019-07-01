using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kosmograph.Desktop.Graph.View
{
    public static class Extensions
    {
        public static T AsType<T>(this object instance) where T : class
        {
            return instance as T;
        }
    }
}
