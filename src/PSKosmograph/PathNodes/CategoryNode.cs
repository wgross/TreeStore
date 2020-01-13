using CodeOwls.PowerShell.Paths;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PSKosmograph.PathNodes
{
    public sealed class CategoryNode : PathNode,
        // item capabilities
        IGetChildItem, INewItem, IRemoveItem, ICopyItem, IRenameItem, IMoveItem
    {
        public sealed class ItemProvider : ContainerItemProvider, IItemProvider
        {
            public ItemProvider(Category category)
                : base(new Item(category), category.Name)
            {
            }

            public Guid Id => ((Item)this.GetItem()).Id;
        }

        public sealed class Item
        {
            private Category category;

            public Item(Category category)
            {
                this.category = category;
            }

            public Guid Id => this.category.Id;

            public string Name => this.category.Name;

            public KosmographItemType ItemType => KosmographItemType.Category;
        }

        private readonly IKosmographPersistence persistence;
        private readonly Category category;

        public CategoryNode(IKosmographPersistence persistence, Category category)
        {
            this.persistence = persistence;
            this.category = category;
        }

        public override string Name => this.category.Name;

        #region PathNode Members

        public override IEnumerable<PathNode> Resolve(IProviderContext providerContext, string nodeName)
        {
            var persistence = providerContext.Persistence();

            IEnumerable<PathNode> categories() => this.category
                .SubCategories
                .Where(c => c.Name.Equals(nodeName, StringComparison.InvariantCultureIgnoreCase))
                .Select(c => new CategoryNode(persistence, c));

            IEnumerable<PathNode> entities() => persistence.Entities.FindByCategory(this.category)
                .Where(c => c.Name.Equals(nodeName, StringComparison.InvariantCultureIgnoreCase))
                .Select(e => new EntityNode(persistence, e));

            return categories().Union(entities());
        }

        #endregion PathNode Members

        #region IGetChildItem Members

        override public IEnumerable<PathNode> GetChildNodes(IProviderContext context)
        {
            var persistence = context.Persistence();

            IEnumerable<PathNode> entities() => SubEntities(persistence).Select(e => new EntityNode(persistence, e));
            IEnumerable<PathNode> categories() => SubCategories(persistence).Select(c => new CategoryNode(persistence, c));

            return categories().Union(entities());
        }

        #endregion IGetChildItem Members

        #region INewItem Members

        public IEnumerable<string> NewItemTypeNames { get; } = new[] { nameof(KosmographItemType.Category), nameof(KosmographItemType.Entity) };

        public IItemProvider NewItem(IProviderContext providerContext, string newItemName, string itemTypeName, object newItemValue)
        {
            switch (itemTypeName ?? nameof(KosmographItemType.Entity))
            {
                case nameof(KosmographItemType.Category):
                    return NewCategory(providerContext, newItemName);

                case nameof(KosmographItemType.Entity):
                    return NewEntity(providerContext, newItemName);

                default:
                    throw new InvalidOperationException($"ItemType '{itemTypeName}' not allowed in the context");
            }
        }

        private IItemProvider NewCategory(IProviderContext providerContext, string newItemName)
        {
            var persistence = providerContext.Persistence();

            if (SubCategory(persistence, newItemName) is { })
            {
                throw new InvalidOperationException($"Name is already used by and item of type '{nameof(KosmographItemType.Category)}'");
            }

            if (SubEntity(persistence, newItemName) is { })
            {
                throw new InvalidOperationException($"Name is already used by and item of type '{nameof(KosmographItemType.Entity)}'");
            }

            var subCategory = new Category(newItemName);

            this.category.AddSubCategory(subCategory);

            return new CategoryNode(persistence, persistence.Categories.Upsert(subCategory)).GetItemProvider();
        }

        private IItemProvider NewEntity(IProviderContext providerContext, string newItemName)
        {
            var persistence = providerContext.Persistence();
            if (SubCategory(persistence, newItemName) is { })
            {
                throw new InvalidOperationException($"Name is already used by and item of type '{nameof(KosmographItemType.Category)}'");
            }

            if (SubEntity(persistence, newItemName) is { })
            {
                throw new InvalidOperationException($"Name is already used by and item of type '{nameof(KosmographItemType.Entity)}'");
            }

            var entity = new Entity(newItemName);

            //todo: create entity with tag
            //switch (providerContext.DynamicParameters)
            //{
            //    case NewItemParametersDefinition d when d.Tags.Any():
            //        foreach (var tag in d.Tags.Select(t => Tag(persistence, t)).Where(t => t != null))
            //        {
            //            entity.AddTag(tag!);
            //        }
            //        break;
            //}

            return new EntityNode(providerContext.Persistence(), providerContext.Persistence().Entities.Upsert(entity)).GetItemProvider();
        }

        #endregion INewItem Members

        public void CopyItem(IProviderContext providerContext, string sourceItemName, string destinationItemName, IItemProvider destinationProvider, bool recurse)
        {
            var persistennce = providerContext.Persistence();

            var newCategory = new Category(destinationItemName ?? sourceItemName);
            if (destinationProvider is EntitiesNode.ItemProvider)
            {
                // dont add the category yet to its parent
                newCategory.Parent = persistennce.Categories.Root();
            }
            else if (destinationProvider is CategoryNode.ItemProvider containerProvider)
            {
                newCategory.Parent = persistennce.Categories.FindById(containerProvider.Id);
            }

            this.EnsureUniqueDestinationName(persistennce, newCategory.Parent!, newCategory.Name);

            // finalize the link
            newCategory.Parent!.AddSubCategory(newCategory);
            persistence.Categories.Upsert(newCategory);
        }

        #region IRemoveItem Members

        public void RemoveItem(IProviderContext providerContext, string path, bool recurse)
        {
            providerContext.Persistence().DeleteCategory(this.category, recurse);
        }

        #endregion IRemoveItem Members

        #region IRenameItem Members

        public void RenameItem(IProviderContext providerContext, string path, string newName)
        {
            // this is explicitely not case insensitive. Renaminh to different cases is allowed,
            // even if it has no effect to the the identification by name.
            if (this.category.Name.Equals(newName))
                return;

            if (this.SiblingCategory(newName) is { })
                return;

            var persistence = providerContext.Persistence();

            if (this.SiblingEntity(persistence, newName) is { })
                return;

            this.category.Name = newName;

            persistence.Categories.Upsert(this.category);
        }

        #endregion IRenameItem Members

        #region IMoveItem Members

        public IItemProvider MoveItem(IProviderContext providerContext, string path, string? movePath, IItemProvider destinationProvider)
        {
            if (destinationProvider is CategoryNode.ItemProvider categoryProvider)
            {
                var persistence = providerContext.Persistence();
                var destinationCategory = persistence.Categories.FindById(categoryProvider.Id);
                var movePathResolved = movePath ?? this.category.Name;

                if (SubCategory(persistence, destinationCategory, movePathResolved) is { })
                    throw new InvalidOperationException($"Destination container contains already a category with name '{movePathResolved}'");

                if (SubEntity(persistence, destinationCategory, movePathResolved) is { })
                    throw new InvalidOperationException($"Destination container contains already an entity with name '{movePathResolved}'");

                this.category.Name = movePathResolved;
                destinationCategory.AddSubCategory(this.category);
                persistence.Categories.Upsert(destinationCategory);
            }
            return this.GetItemProvider();
        }

        #endregion IMoveItem Members

        public override IItemProvider GetItemProvider() => new ItemProvider(this.category);

        #region Model Accessors

        private void EnsureUniqueDestinationName(IKosmographPersistence persistence, Category category, string destinationName)
        {
            if (SubCategory(persistence, category, destinationName) is { })
                throw new InvalidOperationException($"Destination container contains already a category with name '{destinationName}'");

            if (SubEntity(persistence, category, destinationName) is { })
                throw new InvalidOperationException($"Destination container contains already an entity with name '{destinationName}'");
        }

        private IEnumerable<Category> SubCategories() => this.category.SubCategories;

        private IEnumerable<Entity> SubEntities(IKosmographPersistence persistence) => persistence.Entities.FindByCategory(this.category);

        private Category? SubCategory(IKosmographPersistence persistence, string name) => this.SubCategory(persistence, parentCategeory: this.category, name);

        private Category? SubCategory(IKosmographPersistence persistence, Category parentCategeory, string name) => persistence.Categories.FindByCategoryAndName(parentCategeory, name);

        private IEnumerable<Category> SubCategories(IKosmographPersistence persistence) => persistence.Categories.FindByCategory(this.category);

        private Category? SiblingCategory(string name) => this.category.Parent?.FindSubCategory(name, StringComparer.OrdinalIgnoreCase);

        private Entity? SiblingEntity(IKosmographPersistence persistence, string name) => persistence.Entities.FindByCategoryAndName(this.category.Parent!, name);

        private Entity? SubEntity(IKosmographPersistence persistence, string name) => this.SubEntity(persistence, parentCategory: this.category, name);

        private Entity? SubEntity(IKosmographPersistence persistence, Category parentCategory, string name) => persistence.Entities.FindByCategoryAndName(parentCategory, name);

        private Tag? Tag(IKosmographPersistence persistence, string name) => persistence.Tags.FindByName(name);

        #endregion Model Accessors
    }
}