using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PSKosmograph.Service
{
    public class KosmographProviderService : IKosmographProviderService
    {
        private readonly KosmographModel model;

        public KosmographProviderService(KosmographModel model)
        {
            this.model = model;
        }

        public IEnumerable<KosmographContainer> GetContainers()
        {
            throw new NotImplementedException();
        }

        public KosmographContainer GetContainerByPath(KosmographPath path)
        {
            switch (path.Items.First())
            {
                case "Tags":
                case "Entities":
                case "Relationships":
                    return new KosmographContainer(path.ToString());

                default:
                    throw new NotImplementedException();
            }
        }

        public IEnumerable<object> GetItemByPath(KosmographPath v)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<KosmographItem> GetItemsByPath(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Tag> GetTags()
        {
            throw new NotImplementedException();
        }
    }
}