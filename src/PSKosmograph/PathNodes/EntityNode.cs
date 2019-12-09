using CodeOwls.PowerShell.Paths;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PSKosmograph.PathNodes
{
    public class EntityNode : IPathNode, INewItem, IRemoveItem, ICopyItem, IRenameItem
    {
        public sealed class ItemProvider : ContainerItemProvider, IItemProvider
        {
            private readonly Entity entity;
            private readonly IKosmographPersistence model;

            public ItemProvider(IKosmographPersistence model, Entity entity)
                : base(new Item(entity), entity.Name)
            {
                this.entity = entity;
                this.model = model;
            }

            public IEnumerable<PSPropertyInfo> GetItemProperties(IEnumerable<string> propertyNames)
            {
                IEnumerable<PSNoteProperty> staticProperties(Item item)
                {
                    yield return new PSNoteProperty(nameof(Name), item.Name);
                }

                IEnumerable<PSNoteProperty> dynamicProperties()
                {
                    return this.entity.Tags
                        // make tuple <tag-name>, <property>
                        .SelectMany(t => t.Facet.Properties.AsEnumerable().Select(p => (t.Name, p)))
                        // map tuple to PSNoteProperty(<tag-name>.<property-name>,p.Value)
                        .Select(tnp =>
                        {
                            var (hasValue, value) = this.entity.TryGetFacetProperty(tnp.p);
                            if (hasValue)
                                return new PSNoteProperty($"{tnp.Name}.{tnp.p.Name}", value);
                            else
                                return new PSNoteProperty($"{tnp.Name}.{tnp.p.Name}", null);
                        });
                }

                var allProperties = staticProperties((Item)this.GetItem()).Union(dynamicProperties());
                if (propertyNames.Any())
                    return allProperties.Where(p => propertyNames.Contains(p.Name, StringComparer.OrdinalIgnoreCase));
                else
                    return allProperties;
            }

            public void SetItemProperties(IEnumerable<PSPropertyInfo> properties)
            {
                foreach (var property in properties)
                {
                    if (nameof(Name).Equals(property.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        this.entity.Name = (string)property.Value;
                    }
                    else this.SetAssignedFacetProperty(property.Name, property.Value);
                }
                this.model.Entities.Upsert(entity);
            }

            private void SetAssignedFacetProperty(string name, object value)
            {
                if (string.IsNullOrEmpty(name))
                    return;
                var splittedName = name.Split(".", 2, StringSplitOptions.RemoveEmptyEntries);
                if (splittedName.Length != 2)
                    return;

                var assignedTag = this.entity.Tags.FirstOrDefault(t => t.Name.Equals(splittedName[0], StringComparison.OrdinalIgnoreCase));
                if (assignedTag is null)
                    return;

                var facetProperty = assignedTag.Facet.Properties.FirstOrDefault(p => p.Name.Equals(splittedName[1], StringComparison.OrdinalIgnoreCase));
                if (facetProperty is null)
                    return;

                this.entity.SetFacetProperty(facetProperty, value);
            }
        }

        public sealed class Item
        {
            private readonly Entity entitiy;

            public Item(Entity entity)
            {
                this.entitiy = entity;
            }

            public string Name
            {
                get => this.entitiy.Name;
                set => this.entitiy.Name = value;
            }

            public Guid Id => this.entitiy.Id;

            public KosmographItemType ItemType => KosmographItemType.Entity;
        }

        private readonly Entity entity;
        private readonly IKosmographPersistence model;

        public EntityNode(IKosmographPersistence model, Entity entity)
        {
            this.entity = entity;
            this.model = model;
        }

        #region IPathNode Members

        public string Name => this.entity.Name;

        public string ItemMode => "+";

        public IEnumerable<IPathNode> GetNodeChildren(IProviderContext providerContext)
            => this.entity.Tags.Select(t => new AssignedTagNode(providerContext.Persistence(), this.entity, t));

        public IItemProvider GetItemProvider() => new ItemProvider(this.model, this.entity);

        public IEnumerable<IPathNode> Resolve(IProviderContext providerContext, string? name)
        {
            if (name is null)
                return this.GetNodeChildren(providerContext);

            var tag = this.entity.Tags.SingleOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (tag is null)
                return Enumerable.Empty<IPathNode>();
            return new AssignedTagNode(providerContext.Persistence(), this.entity, tag).Yield();
        }

        #endregion IPathNode Members

        #region INewItem Members

        public IEnumerable<string> NewItemTypeNames => "AssignedTag".Yield();

        public IItemProvider NewItem(IProviderContext providerContext, string newItemChildPath, string? itemTypeName, object? newItemValue)
        {
            var tag = providerContext.Persistence().Tags.FindByName(newItemChildPath);
            if (tag is null)
                throw new InvalidOperationException($"tag(name='{newItemChildPath}') doesn't exist.");

            if (this.entity.Tags.Contains(tag))
                return new AssignedTagNode(providerContext.Persistence(), this.entity, tag).GetItemProvider();

            this.entity.AddTag(tag);

            providerContext.Persistence().Entities.Upsert(this.entity);

            return new AssignedTagNode(providerContext.Persistence(), this.entity, tag).GetItemProvider();
        }

        #endregion INewItem Members

        #region IRemoveItem Members

        public void RemoveItem(IProviderContext providerContext, string path, bool recurse)
        {
            if (recurse)
                providerContext.Persistence().Entities.Delete(this.entity);
            else if (!this.entity.Tags.Any())
                providerContext.Persistence().Entities.Delete(this.entity);
        }

        #endregion IRemoveItem Members

        #region ICopyItem Members

        public void CopyItem(IProviderContext providerContext, string sourceItemName, string destinationItemName, IItemProvider destinationContainer, bool recurse)
        {
            var newEntity = new Entity(destinationItemName, this.entity.Tags.ToArray());

            this.entity.Tags.ForEach(t =>
            {
                t.Facet.Properties.ForEach(p =>
                {
                    (var hasValue, var value) = this.entity.TryGetFacetProperty(p);
                    if (hasValue)
                        newEntity.SetFacetProperty(p, value);
                });
            });
            providerContext.Persistence().Entities.Upsert(newEntity);
        }

        #endregion ICopyItem Members

        #region IRenameItem Members

        public void RenameItem(IProviderContext providerContext, string path, string newName)
        {
            if (this.entity.Name.Equals(newName))
                return;

            this.entity.Name = newName;

            providerContext.Persistence().Entities.Upsert(this.entity);
        }

        #endregion IRenameItem Members
    }
}