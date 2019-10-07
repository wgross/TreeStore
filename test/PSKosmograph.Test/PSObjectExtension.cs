using System.Linq;
using System.Management.Automation;

namespace PSKosmograph.Test
{
    public static class PSObjectExtensions
    {
        public static T As<T>(this PSObject obj)
        {
            return (T)obj.ImmediateBaseObject;
        }

        public static bool PropertyContains(this PSObject obj, string name)
            => obj.Properties.Any(p => p.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));

        public static object Property(this PSObject obj, string name)
            => obj.Properties[name].Value;

        public static V Property<V>(this PSObject obj, string name)
            => (V)obj.Properties[name].Value;
    }
}