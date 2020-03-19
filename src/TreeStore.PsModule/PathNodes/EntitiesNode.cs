using CodeOwls.PowerShell.Paths;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using TreeStore.Model;

namespace TreeStore.PsModule.PathNodes
{
    public sealed class EntitiesNode : ContainerNode, INewItem
    {
        public sealed class Item
        {
            public string Name => "Entities";
        }

        public override string Name => "Entities";

        public override IEnumerable<PathNode> Resolve(IProviderContext providerContext, string? name)
        {
            if (string.IsNullOrEmpty(name))
                return this.GetChildNodes(providerContext);

            var persistence = providerContext.Persistence();

            // first check if there is a category: its faster
            var subCategory = SubCategory(persistence, name);
            if (subCategory is { })
                return new CategoryNode(subCategory).Yield();

            // no category, might be an entity
            var entity = providerContext.Persistence().Entities.FindByCategoryAndName(RootCategory(persistence), name);
            if (entity is null)
                return Enumerable.Empty<PathNode>();

            return new EntityNode(entity).Yield();
        }

        #region IGetItem

        public override PSObject GetItem(IProviderContext providerContext) => PSObject.AsPSObject(new Item());

        #endregion IGetItem

        #region IGetChildItem

        public override IEnumerable<PathNode> GetChildNodes(IProviderContext providerContext)
        {
            var persistence = providerContext.Persistence();

            return SubCategories(persistence).Select(c => new CategoryNode(c))
                    .Union<PathNode>(Entities(persistence).Select(e => new EntityNode(e)));
        }

        #endregion IGetChildItem

        #region INewItem

        public IEnumerable<string> NewItemTypeNames { get; } = new[] { nameof(TreeStoreItemType.Category), nameof(TreeStoreItemType.Entity) };

        public class NewItemParametersDefinition
        {
            [Parameter]
            [ArgumentCompleter(typeof(AvailableTagNameCompleter))]
            public string[] Tags { get; set; } = new string[0];
        }

        public object NewItemParameters => new NewItemParametersDefinition();

        public PathNode NewItem(IProviderContext providerContext, string newItemName, string itemTypeName, object newItemValue)
        {
            Guard.Against.InvalidNameCharacters(newItemName, $"entity(name='{newItemName}' wasn't created");

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

            if (SubEntity(persistence, newItemName) is { })
            {
                throw new InvalidOperationException("Name is already used by and item of type 'Entity'");
            }

            var category = new Category(newItemName);

            RootCategory(persistence).AddSubCategory(category);

            return new CategoryNode(persistence.Categories.Upsert(category));
        }

        private PathNode NewEntity(IProviderContext providerContext, string newItemName)
        {
            var persistence = providerContext.Persistence();
            if (SubCategory(persistence, newItemName) is { })
            {
                throw new InvalidOperationException("Name is already used by and item of type 'Category'");
            }

            var entity = new Entity(newItemName)
            {
                Category = RootCategory(persistence)
            };

            switch (providerContext.DynamicParameters)
            {
                case NewItemParametersDefinition d when d.Tags.Any():
                    foreach (var tag in d.Tags.Select(t => Tag(persistence, t)).Where(t => t != null))
                    {
                        entity.AddTag(tag!);
                    }
                    break;
            }

            return new EntityNode(providerContext.Persistence().Entities.Upsert(entity));
        }

        #endregion INewItem

        #region Model Accessors

        private static IEnumerable<Category> SubCategories(ITreeStorePersistence persistence) => persistence.Categories.FindByCategory(RootCategory(persistence));

        private static IEnumerable<Entity> Entities(ITreeStorePersistence persistence) => persistence.Entities.FindByCategory(RootCategory(persistence));

        private static Category RootCategory(ITreeStorePersistence persistence) => persistence.Categories.Root();

        private static Category? SubCategory(ITreeStorePersistence persistence, string name) => persistence.Categories.FindByCategoryAndName(RootCategory(persistence), name);

        private static Entity? SubEntity(ITreeStorePersistence persistence, string name) => persistence.Entities.FindByCategoryAndName(RootCategory(persistence), name);

        private static Tag? Tag(ITreeStorePersistence persistence, string name) => persistence.Tags.FindByName(name);

        #endregion Model Accessors
    }
}