using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using CodeOwls.PowerShell.Provider.PathNodes;
using Kosmograph.Model;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PSKosmograph.PathNodes
{
    public sealed class EntitiesNode : IPathNode, INewItem
    {
        public sealed class Value : ContainerPathValue
        {
            public Value()
                : base(new Item(), name: "Entities")
            { }
        }

        public sealed class Item
        {
        }

        #region IPathNode Members

        public string Name => "Entities";

        public string ItemMode => "+";

        public IEnumerable<IPathNode> GetNodeChildren(IProviderContext providerContext) => providerContext
            .Persistence()
            .Entities.FindAll()
            .Select(e => new EntityNode(providerContext.Persistence(), e));

        public IPathValue GetNodeValue() => new Value();

        public IEnumerable<IPathNode> Resolve(IProviderContext providerContext, string? name)
        {
            if (string.IsNullOrEmpty(name))
                return this.GetNodeChildren(providerContext);

            var entity = providerContext.Persistence().Entities.FindByName(name);
            if (entity is null)
                return Enumerable.Empty<IPathNode>();
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

        public IPathValue NewItem(IProviderContext providerContext, string newItemName, string itemTypeName, object newItemValue)
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

            return new EntityNode(providerContext.Persistence(), providerContext.Persistence().Entities.Upsert(entity)).GetNodeValue();
        }

        #endregion NewItem Members
    }
}