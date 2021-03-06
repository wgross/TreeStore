﻿using CodeOwls.PowerShell.Paths;
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
    public class AssignedTagNode : LeafNode, IRemoveItem,
        IClearItemProperty, ISetItemProperty, IGetItemProperty
    {
        private readonly Entity entity;
        private readonly Tag assignedTag;

        public AssignedTagNode(Entity entity, Tag tag)
        {
            this.entity = entity;
            this.assignedTag = tag;
        }

        public sealed class Item
        {
            private readonly Tag assignedTag;
            private readonly Entity entity;

            public Item(Entity entity, Tag tag)
            {
                this.assignedTag = tag;
                this.entity = entity;
            }

            public Guid Id => this.assignedTag.Id;

            public string Name => this.assignedTag.Name;

            public TreeStoreItemType ItemType => TreeStoreItemType.AssignedTag;

            //public string[] Properties => this.assignedTag
            //    .Facet
            //    .Properties
            //    .Select(p => p.Name)
            //    .ToArray();

            public string ToFormattedString()
            {
                var maxPropertyLength = 0;
                foreach (var p in this.assignedTag.Facet.Properties)
                {
                    maxPropertyLength = Math.Max(maxPropertyLength, p.Name.Length);
                }

                var propertyIndent = "  ";
                var builder = new StringBuilder().AppendLine(this.assignedTag.Name);
                foreach (var p in this.assignedTag.Facet.Properties)
                {
                    builder.Append(propertyIndent).Append(p.Name.PadRight(maxPropertyLength)).Append(" : ");
                    switch (this.entity.TryGetFacetProperty(p))
                    {
                        case var result when result.hasValue:
                            builder.AppendLine(result.value?.ToString());
                            break;

                        default:
                            builder.AppendLine("<no value>");
                            break;
                    };
                }

                return builder.ToString();
            }
        }

        public override string Name => this.assignedTag.Name;

        #region IGetItem

        public override PSObject GetItem(IProviderContext providerContext)
        {
            var psObject = PSObject.AsPSObject(new Item(this.entity, this.assignedTag));

            this.entity
               .AllAssignedPropertyValues(this.assignedTag)
               .Aggregate(psObject, (pso, p) =>
               {
                   pso.Properties.Add(new PSNoteProperty(p.propertyName, p.value));
                   return pso;
               });

            return psObject;
        }

        #endregion IGetItem

        #region IRemoveItem

        public object RemoveItemParameters => new RuntimeDefinedParameterDictionary();

        public void RemoveItem(IProviderContext providerContext, string path)
        {
            if (!providerContext.Force)
            {
                if (this.assignedTag.Facet.Properties.Any(p => this.entity.Values.ContainsKey(p.Id.ToString())))
                {
                    providerContext.WriteError(
                       new ErrorRecord(new InvalidOperationException($"Can't delete assigned tag(name='{this.assignedTag.Name}'): Its properties have values. Use -Force to delete anyway."),
                           errorId: "PropertyHasData",
                           errorCategory: ErrorCategory.InvalidOperation,
                           targetObject: this.GetItem(providerContext)));

                    return;
                }
            }

            this.entity.RemoveTag(this.assignedTag);
            providerContext.Persistence().Entities.Upsert(this.entity);
        }

        #endregion IRemoveItem

        #region IClearItemProperty

        public object ClearItemPropertyParameters => this.BuildItemPropertyParameters(this.ValidateSetAttributeForSettable());

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

            var facetProperty = this.assignedTag.Facet.Properties.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (facetProperty is null)
                return (assignedTag, null);

            return (assignedTag, facetProperty);
        }

        #endregion IClearItemProperty

        #region ISetItemProperty

        public object SetItemPropertyParameters => this.BuildItemPropertyParameters(this.ValidateSetAttributeForSettable());

        private RuntimeDefinedParameterDictionary BuildItemPropertyParameters(ValidateSetAttribute validateSet)
        {
            var propertyNameParameter = new RuntimeDefinedParameter("TreeStorePropertyName", typeof(string), new Collection<Attribute>());
            propertyNameParameter.Attributes.Add(this.ParameterAttribute());
            propertyNameParameter.Attributes.Add(validateSet);
            var parameter = new RuntimeDefinedParameterDictionary();
            parameter.Add(propertyNameParameter.Name, propertyNameParameter);
            return parameter;
        }

        private ParameterAttribute ParameterAttribute() => new ParameterAttribute();

        private ValidateSetAttribute ValidateSetAttributeForSettable() => new ValidateSetAttribute(this.entity
            .AllAssignedPropertyValues(this.assignedTag)
            .Select(pv => pv.propertyName)
            .ToArray());

        public void SetItemProperties(IProviderContext providerContext, IEnumerable<PSPropertyInfo> properties)
        {
            foreach (var p in properties)
            {
                var fp = this.assignedTag.Facet.Properties.SingleOrDefault(fp => fp.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase));
                if (fp is null)
                    continue;

                this.entity.SetFacetProperty(fp, p.Value);
            }
            providerContext.Persistence().Entities.Upsert(this.entity);
        }

        #endregion ISetItemProperty

        #region IGetItemProperty - argument completion only

        override public object GetItemPropertyParameters => this.BuildItemPropertyParameters(this.ValidateSetAttributeForGettable());

        private ValidateSetAttribute ValidateSetAttributeForGettable() => new ValidateSetAttribute(this.entity
            .AllAssignedPropertyValues(this.assignedTag)
            .Select(pv => pv.propertyName)
            .Union(new[] { "Name" })
            .ToArray());

        #endregion IGetItemProperty - argument completion only
    }
}