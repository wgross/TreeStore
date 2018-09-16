using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class RelationshipViewModelTest
    {
        [Fact]
        public void RelationshipViewModel_mirrors_Relationship()
        {
            // ARRANGE

            var relationship = new Relationship("r", new Entity(), new Entity(), new Tag());

            // ACT

            var result = new RelationshipViewModel(relationship);

            // ASSERT

            Assert.Equal("r", result.Name);
            Assert.Equal(relationship.From, result.From.Model);
            Assert.Equal(relationship.To, result.To.Model);
            Assert.Equal(relationship.Tags.Single(), result.Tags.Single().Model);
        }
    }
}