using Kosmograph.Model;
using System;

namespace PSKosmograph.Test.Service
{
    public class KosmographItemTestBase
    {
        protected T Setup<T>(T t, Action<T>? setup = null)
        {
            setup?.Invoke(t);
            return t;
        }

        protected Tag DefaultTag(Action<Tag>? setup = null) => Setup(new Tag("t", new Facet("f", new FacetProperty("p"))), setup);
    }
}