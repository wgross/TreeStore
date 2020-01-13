using CodeOwls.PowerShell.Paths;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PSKosmograph.PathNodes
{
    public sealed class EntityNode : PathNode,
        // item capabilities
        INewItem, IRemoveItem, ICopyItem, IRenameItem, IMoveItem,
        // item property capabilities
        IClearItemProperty
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

        public override string Name => this.entity.Name;

        public override string ItemMode => "+";

        public override IEnumerable<PathNode> GetChildNodes(IProviderContext providerContext)
            => this.entity.Tags.Select(t => new AssignedTagNode(providerContext.Persistence(), this.entity, t));

        public override IItemProvider GetItemProvider() => new ItemProvider(this.model, this.entity);

        public override IEnumerable<PathNode> Resolve(IProviderContext providerContext, string? name)
        {
            if (name is null)
                return this.GetChildNodes(providerContext);

            var tag = this.entity.Tags.SingleOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (tag is null)
                return Enumerable.Empty<PathNode>();
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

        public void CopyItem(IProviderContext providerContext, string sourceItemName, string? destinationItemName, IItemProvider destinationProvider, bool recurse)
        {
            var newEntity = new Entity(destinationItemName ?? sourceItemName, this.entity.Tags.ToArray());
            var persistence = providerContext.Persistence();

            if (destinationProvider is CategoryNode.ItemProvider categoryProvider)
            {
                newEntity.SetCategory(persistence.Categories.FindById(categoryProvider.Id));
            }
            else if (destinationProvider is EntitiesNode.ItemProvider enttiesProvider)
            {
                newEntity.SetCategory(persistence.Categories.Root());
            }
            else throw new InvalidOperationException($"Entity(name={sourceItemName} can't be copied to destnatination(typof='{destinationProvider.GetType()}'");

            this.entity.Tags.ForEach(t =>
            {
                t.Facet.Properties.ForEach(p =>
                {
                    (var hasValue, var value) = this.entity.TryGetFacetProperty(p);

                    if (hasValue)
                        newEntity.SetFacetProperty(p, value);
                });
            });

            //todo: make Category non-nullable
            this.EnsureUniqueDestinationName(persistence, newEntity.Category!, newEntity.Name);

            persistence.Entities.Upsert(newEntity);
        }

        #endregion ICopyItem Members

        #region IRenameItem Members

        public void RenameItem(IProviderContext providerContext, string path, string newName)
        {
            if (this.entity.Name.Equals(newName))
                return;

            var persistence = providerContext.Persistence();
            if (SiblingEntity(persistence, newName) is { })
                return;
            if (SiblingCategory(persistence, newName) is { })
                return;

            this.entity.Name = newName;

            providerContext.Persistence().Entities.Upsert(this.entity);
        }

        #endregion IRenameItem Members

        #region IMoveItem Members

        public IItemProvider MoveItem(IProviderContext providerContext, string path, string? movePath, IItemProvider destinationProvider)
        {
            var persistence = providerContext.Persistence();
            var resolvedDestinationName = movePath ?? this.entity.Name;

            Category category;
            if (destinationProvider is CategoryNode.ItemProvider categoryProvider)
            {
                category = persistence.Categories.FindById(categoryProvider.Id);
            }
            else if (destinationProvider is EntitiesNode.ItemProvider entitiesProvider)
            {
                category = persistence.Categories.Root();
            }
            else throw new InvalidOperationException($"Entity(name={path} can't be moved to destnatination(typof='{destinationProvider.GetType()}'");

            this.EnsureUniqueDestinationName(persistence, category, resolvedDestinationName);
            // edit entity after checks are done
            this.entity.Name = resolvedDestinationName;
            this.entity.SetCategory(category);
            persistence.Entities.Upsert(this.entity);

            return this.GetItemProvider();
        }

        #endregion IMoveItem Members

        #region IClearItemProperty Members

        public void ClearItemProperty(IProviderContext providerContext, IEnumerable<string> propertyToClear)
        {
            foreach (var propertyName in propertyToClear)
            {
                var (_, property) = this.GetAssignedFacetProperty(propertyName);
                if (property is null)
                    return;
                this.entity.Values.Remove(property.Id.ToString());
            }
            providerContext.Persistence().Entities.Upsert(this.entity);
        }

        private (Tag? tag, FacetProperty? propperty) GetAssignedFacetProperty(string name)
        {
            if (string.IsNullOrEmpty(name))
                return (null, null);

            var splittedName = name.Split(".", 2, StringSplitOptions.RemoveEmptyEntries);
            if (splittedName.Length != 2)
                return (null, null);

            var assignedTag = this.entity.Tags.FirstOrDefault(t => t.Name.Equals(splittedName[0], StringComparison.OrdinalIgnoreCase));
            if (assignedTag is null)
                return (null, null);

            var facetProperty = assignedTag.Facet.Properties.FirstOrDefault(p => p.Name.Equals(splittedName[1], StringComparison.OrdinalIgnoreCase));
            if (facetProperty is null)
                return (assignedTag, null);

            return (assignedTag, facetProperty);
        }

        #endregion IClearItemProperty Members

        #region Model Accessors

        private void EnsureUniqueDestinationName(IKosmographPersistence persistence, Category category, string destinationName)
        {
            if (SubCategory(persistence, category, destinationName) is { })
                throw new InvalidOperationException($"Destination container contains already a category with name '{destinationName}'");

            if (SubEntity(persistence, category, destinationName) is { })
                throw new InvalidOperationException($"Destination container contains already an entity with name '{destinationName}'");
        }

        private Category? SiblingCategory(IKosmographPersistence persistence, string name) => persistence.Categories.FindByCategoryAndName(this.entity.Category!, name);

        private Entity? SiblingEntity(IKosmographPersistence persistence, string name) => persistence.Entities.FindByCategoryAndName(this.entity.Category!, name);

        private Entity? SubEntity(IKosmographPersistence persistence, Category category) => this.SubEntity(persistence, category, this.entity.Name);

        private Entity? SubEntity(IKosmographPersistence persistence, Category category, string name) => persistence.Entities.FindByCategoryAndName(category, name);

        private Category? SubCategory(IKosmographPersistence persistence, Category category) => this.SubCategory(persistence, category, this.entity.Name);

        private Category? SubCategory(IKosmographPersistence persistence, Category parentCategory, string name) => persistence.Categories.FindByCategoryAndName(parentCategory, name);

        #endregion Model Accessors
    }
}