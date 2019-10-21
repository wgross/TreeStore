using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using CodeOwls.PowerShell.Provider.PathNodes;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PSKosmograph.PathNodes
{
    public class AssignedTagNode : IPathNode, IRemoveItem
    {
        private readonly IKosmographPersistence model;
        private readonly Entity entity;
        private readonly Tag assignedTag;

        public AssignedTagNode(IKosmographPersistence model, Entity entity, Tag tag)
        {
            this.model = model;
            this.entity = entity;
            this.assignedTag = tag;
        }

        public class Value : IPathValue
        {
            private readonly IKosmographPersistence model;
            private readonly Entity entity;
            private readonly Tag assignedTag;

            public Value(IKosmographPersistence model, Entity entity, Tag assignedTag)
            {
                this.model = model;
                this.entity = entity;
                this.assignedTag = assignedTag;
            }

            public string Name => this.assignedTag.Name;

            public bool IsCollection => false;

            public object Item => new Item(this.assignedTag);

            public IEnumerable<PSPropertyInfo> GetItemProperties(IEnumerable<string> propertyNames)
            {
                var assignedTagProperties = this.assignedTag.Facet.Properties.AsEnumerable();

                if (propertyNames != null && propertyNames.Any())
                    assignedTagProperties = assignedTagProperties.Where(p => propertyNames.Contains(p.Name));

                return assignedTagProperties.Select(p =>
                {
                    (_, object value) = this.entity.TryGetFacetProperty(p);
                    return new PSNoteProperty(p.Name, value);
                });
            }

            public void SetItemProperties(IEnumerable<PSPropertyInfo> properties)
            {
                foreach (var p in properties)
                {
                    var fp = this.assignedTag.Facet.Properties.SingleOrDefault(fp => fp.Name.Equals(p.Name));
                    if (fp is null)
                        continue;
                    if (!fp.CanAssignValue(p.Value?.ToString() ?? null))
                        throw new InvalidOperationException($"value='{p.Value}' cant be assigned to property(name='{p.Name}', type='{fp.Type}')");

                    this.entity.SetFacetProperty(fp, p.Value);
                }
                this.model.Entities.Upsert(this.entity);
            }
        }

        public class Item
        {
            private readonly Tag assignedTag;

            public Item(Tag tag)
            {
                this.assignedTag = tag;
            }

            public Guid Id => this.assignedTag.Id;

            public string Name => this.assignedTag.Name;
        }

        public object? GetNodeChildrenParameters => null;

        public string Name => this.assignedTag.Name;

        public string ItemMode => "+";

        public IEnumerable<IPathNode> GetNodeChildren(IProviderContext providerContext) => Enumerable.Empty<IPathNode>();

        public IPathValue GetNodeValue() => new Value(this.model, this.entity, this.assignedTag);

        public IEnumerable<IPathNode> Resolve(IProviderContext providerContext, string name)
        {
            throw new System.NotImplementedException();
        }

        #region IRemoveItem Members

        public object? RemoveItemParameters => null;

        public void RemoveItem(IProviderContext providerContext, string path, bool recurse)
        {
            this.entity.Tags.Remove(this.assignedTag);
            providerContext.Persistence().Entities.Upsert(this.entity);
        }

        #endregion IRemoveItem Members
    }
}