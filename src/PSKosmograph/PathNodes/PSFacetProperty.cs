using Kosmograph.Model;
using System.Management.Automation;

namespace PSKosmograph.PathNodes
{
    public class PSFacetProperty : PSNoteProperty
    {
        public PSFacetProperty(string name, FacetPropertyTypeValues type, object? value)
            : base(name, value)
        {
            this.FacetPropertyType = type;
        }

        private FacetPropertyTypeValues FacetPropertyType { get; }

        public override string TypeNameOfValue => this.FacetPropertyType.ToString();
    }
}