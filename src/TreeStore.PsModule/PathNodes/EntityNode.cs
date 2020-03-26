using CodeOwls.PowerShell.Paths;
using CodeOwls.PowerShell.Provider.PathNodeProcessors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Text;
using TreeStore.Model;

namespace TreeStore.PsModule.PathNodes
{
    /// <summary>
    /// Represents and object of the data model. It has Assigned tags as child nodes and
    /// Allows to read/write all properties of all tags assiged to it.
    /// </summary>
    public sealed class EntityNode : ContainerNode,
        INewItem, IRemoveItem, ICopyItem, IRenameItem, IMoveItem,
        IClearItemProperty, ISetItemProperty, IGetItemProperty
    {
        #region Item - to be used in powershell pipe

        public sealed class Property
        {
            internal Property(string name, object? value, FacetPropertyTypeValues type)
            {
                this.Name = name;
                this.Value = value;
                this.ValueType = type;
            }

            public string Name { get; }

            public object? Value { get; }

            public FacetPropertyTypeValues ValueType { get; }
        }

        public sealed class Item
        {
            private readonly Entity entity;

            public Item(Entity entity)
            {
                this.entity = entity;
            }

            public string Name
            {
                get => this.entity.Name;
                set => this.entity.Name = value;
            }

            public Guid Id => this.entity.Id;

            public TreeStoreItemType ItemType => TreeStoreItemType.Entity;

            // todo: properties

            #region // properties collection

            //private string[]? properties = null;
            //public string[] Properties => this.properties ??= this.SelectAssignedProperties();
            //private string[] SelectAssignedProperties() => this.entity
            //    .Tags
            //    .SelectMany(t => t.Facet.Properties.AsEnumerable().Select(p => (t.Name, p)))
            //    .Select(tnp => $"{tnp.Name}.{tnp.p.Name}")
            //    .ToArray();

            #endregion // properties collection

            public string ToFormattedString()
            {
                var maxPropertyLength = 0;
                foreach (var tag in this.entity.Tags)
                {
                    foreach (var p in tag.Facet.Properties)
                    {
                        maxPropertyLength = Math.Max(maxPropertyLength, p.Name.Length);
                    }
                }

                var tagIndent = "  ";
                var propertyIndent = "    ";
                var builder = new StringBuilder().AppendLine(this.entity.Name);
                foreach (var tag in this.entity.Tags)
                {
                    builder.Append(tagIndent).AppendLine(tag.Name);
                    foreach (var p in tag.Facet.Properties)
                    {
                        builder.Append(propertyIndent).Append(p.Name.PadRight(maxPropertyLength)).Append(" : ");
                        switch (this.entity.TryGetFacetProperty(p))
                        {
                            case var result when result.exists:
                                builder.AppendLine(result.value?.ToString());
                                break;

                            default:
                                builder.AppendLine();
                                break;
                        };
                    }
                }

                return builder.ToString();
            }
        }

        #endregion Item - to be used in powershell pipe

        private readonly Entity entity;

        public EntityNode(Entity entity)
        {
            this.entity = entity;
        }

        #region IGetItem

        public override PSObject GetItem(IProviderContext providerContext)
        {
            // creates an PSObject wth all properties from Item
            return this.entity
                .Tags
                .Select(tag => (name: tag.Name, psobj: new AssignedTagNode(this.entity, tag).GetItem(providerContext)))
                .Aggregate(PSObject.AsPSObject(new Item(this.entity)), (epso, tpso) =>
                 {
                     epso.Properties.Add(new PSNoteProperty(tpso.name, tpso.psobj));
                     return epso;
                 });
        }

        #endregion IGetItem

        public override string Name => this.entity.Name;

        #region IGetChildNodes

        public override IEnumerable<PathNode> GetChildNodes(IProviderContext providerContext)
            => this.entity.Tags.Select(t => new AssignedTagNode(this.entity, t));

        #endregion IGetChildNodes

        public override IEnumerable<PathNode> Resolve(IProviderContext providerContext, string? name)
        {
            if (name is null)
                return this.GetChildNodes(providerContext);

            var tag = this.entity.Tags.SingleOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (tag is null)
                return Enumerable.Empty<PathNode>();
            return new AssignedTagNode(this.entity, tag).Yield();
        }

        #region INewItem

        public IEnumerable<string> NewItemTypeNames => "AssignedTag".Yield();

        public PathNode NewItem(IProviderContext providerContext, string newItemChildPath, string? itemTypeName, object? newItemValue)
        {
            var tag = providerContext.Persistence().Tags.FindByName(newItemChildPath);
            if (tag is null)
                throw new InvalidOperationException($"tag(name='{newItemChildPath}') doesn't exist.");

            if (this.entity.Tags.Contains(tag))
                return new AssignedTagNode(this.entity, tag);

            this.entity.AddTag(tag);

            providerContext.Persistence().Entities.Upsert(this.entity);

            return new AssignedTagNode(this.entity, tag);
        }

        #endregion INewItem

        #region IRemoveItem

        public void RemoveItem(IProviderContext providerContext, string path)
        {
            if (providerContext.Recurse)
                providerContext.Persistence().Entities.Delete(this.entity);
            else if (!this.entity.Tags.Any())
                providerContext.Persistence().Entities.Delete(this.entity);
        }

        #endregion IRemoveItem

        #region ICopyItem

        public void CopyItem(IProviderContext providerContext, string sourceItemName, string? destinationItemName, PathNode destinationNode)
        {
            if (destinationItemName != null)
                Guard.Against.InvalidNameCharacters(destinationItemName, $"entity(name='{destinationItemName}' wasn't copied");

            var newEntity = new Entity(destinationItemName ?? sourceItemName, this.entity.Tags.ToArray());
            var persistence = providerContext.Persistence();

            if (destinationNode is CategoryNode categoryProvider)
            {
                newEntity.SetCategory(persistence.Categories.FindById(categoryProvider.Id));
            }
            else if (destinationNode is EntitiesNode enttiesProvider)
            {
                newEntity.SetCategory(persistence.Categories.Root());
            }
            else throw new InvalidOperationException($"Entity(name={sourceItemName} can't be copied to destnatination(typof='{destinationNode.GetType()}'");

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

        #endregion ICopyItem

        #region IRenameItem

        public void RenameItem(IProviderContext providerContext, string path, string newName)
        {
            Guard.Against.InvalidNameCharacters(newName, $"entity(name='{newName}' wasn't renamed");

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

        #endregion IRenameItem

        #region IMoveItem

        public void MoveItem(IProviderContext providerContext, string path, string? movePath, PathNode destinationNode)
        {
            var persistence = providerContext.Persistence();
            var resolvedDestinationName = movePath ?? this.entity.Name;

            Category category;
            if (destinationNode is CategoryNode categoryProvider)
            {
                category = persistence.Categories.FindById(categoryProvider.Id);
            }
            else if (destinationNode is EntitiesNode entitiesProvider)
            {
                category = persistence.Categories.Root();
            }
            else throw new InvalidOperationException($"Entity(name={path} can't be moved to destnatination(typof='{destinationNode.GetType()}'");

            this.EnsureUniqueDestinationName(persistence, category, resolvedDestinationName);
            // edit entity after checks are done
            this.entity.Name = resolvedDestinationName;
            this.entity.SetCategory(category);
            persistence.Entities.Upsert(this.entity);
        }

        #endregion IMoveItem

        #region IClearItemProperty

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

        public object ClearItemPropertyParameters => this.BuildItemPropertyParameters(this.ValidateSetAttributeForSettable());

        #endregion IClearItemProperty

        #region ISetItemProperty

        public object SetItemPropertyParameters => this.BuildItemPropertyParameters(this.ValidateSetAttributeForSettable());

        public void SetItemProperties(IProviderContext providerContext, IEnumerable<PSPropertyInfo> properties)
        {
            foreach (var property in properties)
            {
                this.SetAssignedFacetProperty(property.Name, property.Value);
            }
            providerContext.Persistence().Entities.Upsert(entity);
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

            if (facetProperty.CanAssignValue(value))
                this.entity.SetFacetProperty(facetProperty, value);
        }

        private ParameterAttribute ParameterAttribute()
        {
            return new ParameterAttribute();
        }

        private ValidateSetAttribute ValidateSetAttributeForSettable() => new ValidateSetAttribute(this.entity
            .AllAssignedPropertyValues()
            .Select(pv => $"{pv.tagName}.{pv.propertyName}")
            .ToArray());

        #endregion ISetItemProperty

        #region IGetItemProperty - dynamic parameters only

        public object GetItemPropertyParameters => this.BuildItemPropertyParameters(this.ValidateSetAttributeForGettable());

        private RuntimeDefinedParameterDictionary BuildItemPropertyParameters(ValidateSetAttribute validateSet)
        {
            var propertyNameParameter = new RuntimeDefinedParameter("TreeStorePropertyName", typeof(string), new Collection<Attribute>());
            propertyNameParameter.Attributes.Add(this.ParameterAttribute());
            propertyNameParameter.Attributes.Add(validateSet);
            var parameter = new RuntimeDefinedParameterDictionary();
            parameter.Add(propertyNameParameter.Name, propertyNameParameter);
            return parameter;
        }

        private ValidateSetAttribute ValidateSetAttributeForGettable() => new ValidateSetAttribute(this.entity
            .AllAssignedPropertyValues()
            .Select(pv => $"{pv.tagName}.{pv.propertyName}")
            .Union(new[] { "Id", "Name" })
            .ToArray());

        #endregion IGetItemProperty - dynamic parameters only

        #region Model Accessors

        private void EnsureUniqueDestinationName(ITreeStorePersistence persistence, Category category, string destinationName)
        {
            if (SubCategory(persistence, category, destinationName) is { })
                throw new InvalidOperationException($"Destination container contains already a category with name '{destinationName}'");

            if (SubEntity(persistence, category, destinationName) is { })
                throw new InvalidOperationException($"Destination container contains already an entity with name '{destinationName}'");
        }

        private Category? SiblingCategory(ITreeStorePersistence persistence, string name) => persistence.Categories.FindByParentAndName(this.entity.Category!, name);

        private Entity? SiblingEntity(ITreeStorePersistence persistence, string name) => persistence.Entities.FindByCategoryAndName(this.entity.Category!, name);

        private Entity? SubEntity(ITreeStorePersistence persistence, Category category) => this.SubEntity(persistence, category, this.entity.Name);

        private Entity? SubEntity(ITreeStorePersistence persistence, Category category, string name) => persistence.Entities.FindByCategoryAndName(category, name);

        private Category? SubCategory(ITreeStorePersistence persistence, Category category) => this.SubCategory(persistence, category, this.entity.Name);

        private Category? SubCategory(ITreeStorePersistence persistence, Category parentCategory, string name) => persistence.Categories.FindByParentAndName(parentCategory, name);

        public RuntimeDefinedParameterDictionary ToFormattedString()
        {
            throw new NotImplementedException();
        }

        #endregion Model Accessors
    }
}