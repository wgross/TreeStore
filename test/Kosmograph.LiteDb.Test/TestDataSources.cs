using Kosmograph.Model;
using System;
using System.Linq;

namespace Kosmograph.LiteDb.Test
{
    public static class TestDataSources
    {
        #region Default Tag

        public static Tag DefaultTag(params Action<Tag>[] setup)
        {
            var tmp = new Tag("t", new Facet("f", new FacetProperty("p", FacetPropertyTypeValues.String)));
            setup.ForEach(s => s(tmp));
            return tmp;
        }

        public static void WithDefaultProperty(Tag tag)
        {
            tag.Facet.Properties.Clear();
            tag.Facet.AddProperty(new FacetProperty("p", FacetPropertyTypeValues.String));
        }

        public static void WithoutProperty(Tag tag) => tag.Facet.Properties.Clear();

        public static Action<Tag> WithProperty(string name, FacetPropertyTypeValues type)
        {
            return tag => tag.Facet.AddProperty(new FacetProperty(name, type));
        }

        #endregion Default Tag

        #region Default Entity

        public static Entity DefaultEntity(params Action<Entity>[] setup)
        {
            var tmp = new Entity("e");
            setup.ForEach(s => s(tmp));
            return tmp;
        }

        public static void WithDefaultTag(Entity entity) => entity.Tags.Add(DefaultTag(WithDefaultProperty));

        public static Action<Entity> WithDefaultPropertySet<V>(V value)
            => e => e.SetFacetProperty(e.Tags.First().Facet.Properties.First(), value);

        public static void WithoutTags(Entity entity) => entity.Tags.Clear();

        public static Action<Entity> WithCategory(Category c) => e => e.SetCategory(c);

        #endregion Default Entity

        #region Default Category

        public static Category DefaultCategory(Category parent, params Action<Category>[] setup)
        {
            var tmp = new Category("c");
            parent.AddSubCategory(tmp);
            setup.ForEach(s => s(tmp));
            return tmp;
        }

        #endregion Default Category
    }
}