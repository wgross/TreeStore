using Kosmograph.Desktop.Lists.ViewModel;
using Kosmograph.Model;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Lists.Test.ViewModel
{
    public class RelationshipViewModelTest
    {
        public Tag DefaultTag() => new Tag("tag", new Facet("facet", new FacetProperty("p")));

        public Entity DefaultEntity() => new Entity("entity", DefaultTag());

        public Relationship DefaultRelationship() => new Relationship("relationship", from: DefaultEntity(), to: DefaultEntity(), tags: DefaultTag());

        [Fact]
        public void RelationshipViewModel_mirrors_Model()
        {
            // ARRANGE

            var model = DefaultRelationship();

            // ACT

            var result = new RelationshipViewModel(model);

            // ASSERT

            Assert.Equal("relationship", result.Name);
            Assert.Equal(model.From, result.From.Model);
            Assert.Equal(model.To, result.To.Model);
            Assert.Equal(model.Tags.Single(), result.Tags.Single().Tag);
        }

        [Fact]
        public void RelationshipViewModel_are_equal_if_Relationship_are_equal()
        {
            // ARRANGE

            var relationship = DefaultRelationship();

            // ASSART

            Assert.Equal(new RelationshipViewModel(relationship), new RelationshipViewModel(relationship));
            Assert.Equal(new RelationshipViewModel(relationship).GetHashCode(), new RelationshipViewModel(relationship).GetHashCode());
        }
    }
}