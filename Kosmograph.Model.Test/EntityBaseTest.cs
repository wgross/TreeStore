using System;
using System.Collections.Generic;
using Xunit;

namespace Kosmograph.Model.Test
{
    public class EntityBaseTest
    {
        public static IEnumerable<object[]> GetEntityBaseInstances()
        {
            var refId = Guid.NewGuid();
            var differentId = Guid.NewGuid();

            yield return new object[] { new Entity { Id = refId }, new Entity { Id = refId }, new Entity { Id = differentId }, new FacetProperty { Id = refId } };
            yield return new object[] { new Category { Id = refId }, new Category { Id = refId }, new Category { Id = differentId }, new Tag { Id = refId } };
            yield return new object[] { new Tag { Id = refId }, new Tag { Id = refId }, new Tag { Id = differentId }, new Facet { Id = refId } };
            yield return new object[] { new Facet { Id = refId }, new Facet { Id = refId }, new Facet { Id = differentId }, new FacetProperty { Id = refId } };
            yield return new object[] { new FacetProperty { Id = refId }, new FacetProperty { Id = refId }, new FacetProperty { Id = differentId }, new Entity { Id = refId } };
        }

        [Theory]
        [MemberData(nameof(GetEntityBaseInstances))]
        public void Entites_are_equal_if_Id_are_equal_and_Type(EntityBase refEntity, EntityBase sameId, EntityBase differentId, EntityBase differentType)
        {
            // ACT & ASSERT

            Assert.Equal(refEntity, refEntity);
            Assert.Equal(refEntity.GetHashCode(), refEntity.GetHashCode());
            Assert.Equal(refEntity, sameId);
            Assert.Equal(refEntity.GetHashCode(), sameId.GetHashCode());
            Assert.NotEqual(refEntity, differentId);
            Assert.NotEqual(refEntity.GetHashCode(), differentId.GetHashCode());
            Assert.NotEqual(refEntity, differentType);
            Assert.NotEqual(refEntity.GetHashCode(), differentType.GetHashCode());
        }
    }
}