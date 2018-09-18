using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.EditModel
{
    public class AssignedTagEditModelTest
    {
        [Fact]
        public void AssigedTagEditModel_mirrors_ViewModel()
        {
            // ARRANGE

            var model = new Tag("t", new Facet("f", new FacetProperty("p")));
            var values = new Dictionary<string, object>
            {
                { model.Facet.Properties.Single().Id.ToString(), 1 }
            };
            var viewModel = new AssignedTagViewModel(model, values);

            // ACT

            var result = new AssignedTagEditModel(viewModel);

            // ASSERT
            // facte propert yhandlung is tested elsewhere.

            Assert.Equal("t", result.Name);
            Assert.Single(result.Properties);
        }
    }
}