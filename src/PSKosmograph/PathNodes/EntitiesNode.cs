using CodeOwls.PowerShell.Paths;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using Kosmograph.Model;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PSKosmograph.PathNodes
{
    public sealed class EntitiesNode : PathNode, INewItem
    {
        public sealed class ItemProvider : ContainerItemProvider
        {
            public ItemProvider()
                : base(new Item(), name: "Entities")
            { }
        }

        public sealed class Item
        {
            public string Name => "Entities";
        }

        #region IPathNode Members

        public override string Name => "Entities";

        public override string ItemMode => "+";

        public override IEnumerable<PathNode> GetNodeChildren(IProviderContext providerContext) => providerContext
            .Persistence()
            .Entities.FindAll()
            .Select(e => new EntityNode(providerContext.Persistence(), e));

        public override IItemProvider GetItemProvider() => new ItemProvider();

        public override IEnumerable<PathNode> Resolve(IProviderContext providerContext, string? name)
        {
            if (string.IsNullOrEmpty(name))
                return this.GetNodeChildren(providerContext);

            var entity = providerContext.Persistence().Entities.FindByName(name);
            if (entity is null)
                return Enumerable.Empty<PathNode>();
            return new EntityNode(providerContext.Persistence(), entity).Yield();
        }

        #endregion IPathNode Members

        #region NewItem Members

        public IEnumerable<string> NewItemTypeNames => "Entity".Yield();

        public class NewItemParametersDefinition
        {
            [Parameter]
            [ArgumentCompleter(typeof(AvailableTagNameCompleter))]
            public string[] Tags { get; set; } = new string[0];
        }

        public object NewItemParameters => new NewItemParametersDefinition();

        public IItemProvider NewItem(IProviderContext providerContext, string newItemName, string itemTypeName, object newItemValue)
        {
            var entity = new Entity(newItemName);

            switch (providerContext.DynamicParameters)
            {
                case NewItemParametersDefinition d when d.Tags.Any():
                    foreach (var tag in d.Tags.Select(t => providerContext.Persistence().Tags.FindByName(t)).Where(t => t != null))
                    {
                        entity.AddTag(tag!);
                    }
                    break;
            }

            return new EntityNode(providerContext.Persistence(), providerContext.Persistence().Entities.Upsert(entity)).GetItemProvider();
        }

        #endregion NewItem Members
    }
}