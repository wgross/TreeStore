using Kosmograph.Model;
using PSKosmograph.Service;
using System;
using System.Collections.Generic;

namespace PSKosmograph
{
    public interface IKosmographProviderService
    {
        IEnumerable<KosmographContainer> GetContainers();

        KosmographContainer GetContainerByPath(KosmographPath path);

        IEnumerable<object> GetItemByPath(KosmographPath path);

        IEnumerable<KosmographItem> GetItemsByPath(string path);

        IEnumerable<Tag> GetTags();
    }

    public static class KosmographProviderServiceExtensions
    {
        public static void ForEach(this IEnumerable<KosmographContainer> containers, Action<KosmographContainer> call)
        {
            foreach (var c in containers)
                call(c);
        }

        public static void ForEach(this IEnumerable<KosmographItem> items, Action<KosmographItem> call)
        {
            foreach (var i in items)
                call(i);
        }
    }
}