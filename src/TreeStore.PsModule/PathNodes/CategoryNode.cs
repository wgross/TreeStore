﻿using CodeOwls.PowerShell.Paths;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using TreeStore.Model;

namespace TreeStore.PsModule.PathNodes
{
    /// <summary>
    /// Categorizes a set of <see cref="EntityNode"/>. Has no accessible properties ecept the C# ones.
    /// </summary>
    public sealed class CategoryNode : ContainerNode,
        IGetChildItem, INewItem, IRemoveItem, ICopyItem, IRenameItem, IMoveItem
    {
        public sealed class ItemProvider : ContainerItemProvider, IItemProvider
        {
            public ItemProvider(Category category)
                : base(new Item(category), category.Name)
            {
            }

            public Guid Id => ((Item)this.GetItem().ImmediateBaseObject).Id;
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

            public TreeStoreItemType ItemType => TreeStoreItemType.Category;
        }

        private readonly ITreeStorePersistence persistence;
        private readonly Category category;

        public CategoryNode(ITreeStorePersistence persistence, Category category)
        {
            this.persistence = persistence;
            this.category = category;
        }

        public Guid Id => this.category.Id;

        public override string Name => this.category.Name;

        #region PathNode

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

        #endregion PathNode

        #region IGetItem

        public override PSObject GetItem() => PSObject.AsPSObject(new Item(this.category));

        #endregion IGetItem

        #region IGetChildItem

        override public IEnumerable<PathNode> GetChildNodes(IProviderContext context)
        {
            var persistence = context.Persistence();

            IEnumerable<PathNode> entities() => SubEntities(persistence).Select(e => new EntityNode(persistence, e));
            IEnumerable<PathNode> categories() => SubCategories(persistence).Select(c => new CategoryNode(persistence, c));

            return categories().Union(entities());
        }

        #endregion IGetChildItem

        #region INewItem

        public IEnumerable<string> NewItemTypeNames { get; } = new[] { nameof(TreeStoreItemType.Category), nameof(TreeStoreItemType.Entity) };

        public PathNode NewItem(IProviderContext providerContext, string newItemName, string itemTypeName, object newItemValue)
        {
            switch (itemTypeName ?? nameof(TreeStoreItemType.Entity))
            {
                case nameof(TreeStoreItemType.Category):
                    return NewCategory(providerContext, newItemName);

                case nameof(TreeStoreItemType.Entity):
                    return NewEntity(providerContext, newItemName);

                default:
                    throw new InvalidOperationException($"ItemType '{itemTypeName}' not allowed in the context");
            }
        }

        private PathNode NewCategory(IProviderContext providerContext, string newItemName)
        {
            var persistence = providerContext.Persistence();

            if (SubCategory(persistence, newItemName) is { })
            {
                throw new InvalidOperationException($"Name is already used by and item of type '{nameof(TreeStoreItemType.Category)}'");
            }

            if (SubEntity(persistence, newItemName) is { })
            {
                throw new InvalidOperationException($"Name is already used by and item of type '{nameof(TreeStoreItemType.Entity)}'");
            }

            var subCategory = new Category(newItemName);

            this.category.AddSubCategory(subCategory);

            return new CategoryNode(persistence, persistence.Categories.Upsert(subCategory));
        }

        private PathNode NewEntity(IProviderContext providerContext, string newItemName)
        {
            var persistence = providerContext.Persistence();
            if (SubCategory(persistence, newItemName) is { })
            {
                throw new InvalidOperationException($"Name is already used by and item of type '{nameof(TreeStoreItemType.Category)}'");
            }

            if (SubEntity(persistence, newItemName) is { })
            {
                throw new InvalidOperationException($"Name is already used by and item of type '{nameof(TreeStoreItemType.Entity)}'");
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

            return new EntityNode(providerContext.Persistence(), providerContext.Persistence().Entities.Upsert(entity));
        }

        #endregion INewItem

        public void CopyItem(IProviderContext providerContext, string sourceItemName, string destinationItemName, PathNode destinationNode)
        {
            var persistennce = providerContext.Persistence();

            var newCategory = new Category(destinationItemName ?? sourceItemName);
            if (destinationNode is EntitiesNode)
            {
                // dont add the category yet to its parent
                newCategory.Parent = persistennce.Categories.Root();
            }
            else if (destinationNode is CategoryNode containerProvider)
            {
                newCategory.Parent = persistennce.Categories.FindById(containerProvider.Id);
            }

            this.EnsureUniqueDestinationName(persistennce, newCategory.Parent!, newCategory.Name);

            // finalize the link
            newCategory.Parent!.AddSubCategory(newCategory);
            persistence.Categories.Upsert(newCategory);
        }

        #region IRemoveItem

        public void RemoveItem(IProviderContext providerContext, string path)
        {
            providerContext.Persistence().DeleteCategory(this.category, providerContext.Recurse);
        }

        #endregion IRemoveItem

        #region IRenameItem

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

        #endregion IRenameItem

        #region IMoveItem

        public void MoveItem(IProviderContext providerContext, string path, string? movePath, PathNode destinationNode)
        {
            if (destinationNode is CategoryNode categoryNode)
            {
                var persistence = providerContext.Persistence();
                var destinationCategory = persistence.Categories.FindById(categoryNode.Id);
                var movePathResolved = movePath ?? this.category.Name;

                if (SubCategory(persistence, destinationCategory, movePathResolved) is { })
                    throw new InvalidOperationException($"Destination container contains already a category with name '{movePathResolved}'");

                if (SubEntity(persistence, destinationCategory, movePathResolved) is { })
                    throw new InvalidOperationException($"Destination container contains already an entity with name '{movePathResolved}'");

                this.category.Name = movePathResolved;
                destinationCategory.AddSubCategory(this.category);
                persistence.Categories.Upsert(destinationCategory);
            }
        }

        #endregion IMoveItem

        #region Model Accessors

        private void EnsureUniqueDestinationName(ITreeStorePersistence persistence, Category category, string destinationName)
        {
            if (SubCategory(persistence, category, destinationName) is { })
                throw new InvalidOperationException($"Destination container contains already a category with name '{destinationName}'");

            if (SubEntity(persistence, category, destinationName) is { })
                throw new InvalidOperationException($"Destination container contains already an entity with name '{destinationName}'");
        }

        private IEnumerable<Category> SubCategories() => this.category.SubCategories;

        private IEnumerable<Entity> SubEntities(ITreeStorePersistence persistence) => persistence.Entities.FindByCategory(this.category);

        private Category? SubCategory(ITreeStorePersistence persistence, string name) => this.SubCategory(persistence, parentCategeory: this.category, name);

        private Category? SubCategory(ITreeStorePersistence persistence, Category parentCategeory, string name) => persistence.Categories.FindByCategoryAndName(parentCategeory, name);

        private IEnumerable<Category> SubCategories(ITreeStorePersistence persistence) => persistence.Categories.FindByCategory(this.category);

        private Category? SiblingCategory(string name) => this.category.Parent?.FindSubCategory(name, StringComparer.OrdinalIgnoreCase);

        private Entity? SiblingEntity(ITreeStorePersistence persistence, string name) => persistence.Entities.FindByCategoryAndName(this.category.Parent!, name);

        private Entity? SubEntity(ITreeStorePersistence persistence, string name) => this.SubEntity(persistence, parentCategory: this.category, name);

        private Entity? SubEntity(ITreeStorePersistence persistence, Category parentCategory, string name) => persistence.Entities.FindByCategoryAndName(parentCategory, name);

        private Tag? Tag(ITreeStorePersistence persistence, string name) => persistence.Tags.FindByName(name);

        #endregion Model Accessors
    }
}