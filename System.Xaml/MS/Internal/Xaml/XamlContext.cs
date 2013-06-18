namespace MS.Internal.Xaml
{
    using MS.Internal.Xaml.Parser;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Xaml;
    using System.Xaml.Schema;

    internal abstract class XamlContext
    {
        protected Assembly _localAssembly;
        private Func<string, string> _resolvePrefixCachedDelegate;
        private XamlSchemaContext _schemaContext;

        protected XamlContext(XamlSchemaContext schemaContext)
        {
            this._schemaContext = schemaContext;
        }

        public abstract void AddNamespacePrefix(string prefix, string xamlNamespace);
        private XamlMember CreateUnknownAttachableMember(XamlType declaringType, string name)
        {
            return new XamlMember(name, declaringType, true);
        }

        private XamlMember CreateUnknownMember(XamlType declaringType, string name)
        {
            return new XamlMember(name, declaringType, false);
        }

        public abstract string FindNamespaceByPrefix(string prefix);
        public string GetAttributeNamespace(XamlPropertyName propName, string tagNamespace)
        {
            if (string.IsNullOrEmpty(propName.Prefix) && !propName.IsDotted)
            {
                return tagNamespace;
            }
            return this.ResolveXamlNameNS(propName);
        }

        public XamlMember GetDottedProperty(XamlType tagType, string tagNamespace, XamlPropertyName propName, bool tagIsRoot)
        {
            if (tagType == null)
            {
                throw new XamlInternalException(System.Xaml.SR.Get("ParentlessPropertyElement", new object[] { propName.ScopedName }));
            }
            XamlMember xamlAttachableProperty = null;
            XamlType xamlType = null;
            string propNs = this.ResolveXamlNameNS(propName);
            if (propNs == null)
            {
                throw new XamlParseException(System.Xaml.SR.Get("PrefixNotFound", new object[] { propName.Prefix }));
            }
            XamlType rootTagType = tagIsRoot ? tagType : null;
            bool flag = false;
            if (tagType.IsGeneric)
            {
                flag = this.PropertyTypeMatchesGenericTagType(tagType, tagNamespace, propNs, propName.OwnerName);
                if (flag)
                {
                    xamlAttachableProperty = this.GetInstanceOrAttachableProperty(tagType, propName.Name, rootTagType);
                    if (xamlAttachableProperty != null)
                    {
                        return xamlAttachableProperty;
                    }
                }
            }
            XamlTypeName typeName = new XamlTypeName(propNs, propName.Owner.Name);
            xamlType = this.GetXamlType(typeName, true);
            bool flag2 = tagType.CanAssignTo(xamlType);
            if (flag2)
            {
                xamlAttachableProperty = this.GetInstanceOrAttachableProperty(xamlType, propName.Name, rootTagType);
            }
            else
            {
                xamlAttachableProperty = this.GetXamlAttachableProperty(xamlType, propName.Name);
            }
            if (xamlAttachableProperty != null)
            {
                return xamlAttachableProperty;
            }
            XamlType declaringType = flag ? tagType : xamlType;
            if (flag || flag2)
            {
                return this.CreateUnknownMember(declaringType, propName.Name);
            }
            return this.CreateUnknownAttachableMember(declaringType, propName.Name);
        }

        private XamlMember GetInstanceOrAttachableProperty(XamlType tagType, string propName, XamlType rootTagType)
        {
            XamlMember xamlAttachableProperty = this.GetXamlProperty(tagType, propName, rootTagType);
            if (xamlAttachableProperty == null)
            {
                xamlAttachableProperty = this.GetXamlAttachableProperty(tagType, propName);
            }
            return xamlAttachableProperty;
        }

        public abstract IEnumerable<NamespaceDeclaration> GetNamespacePrefixes();
        public XamlMember GetNoDotAttributeProperty(XamlType tagType, XamlPropertyName propName, string tagNamespace, string propUsageNamespace, bool tagIsRoot)
        {
            XamlMember xamlAttachableProperty = null;
            if ((propUsageNamespace == tagNamespace) || (((tagNamespace == null) && (propUsageNamespace != null)) && tagType.GetXamlNamespaces().Contains(propUsageNamespace)))
            {
                XamlType rootObjectType = tagIsRoot ? tagType : null;
                xamlAttachableProperty = this.GetXamlProperty(tagType, propName.Name, rootObjectType);
                if (xamlAttachableProperty == null)
                {
                    xamlAttachableProperty = this.GetXamlAttachableProperty(tagType, propName.Name);
                }
            }
            if ((xamlAttachableProperty == null) && (propUsageNamespace != null))
            {
                XamlDirective xamlDirective = this.SchemaContext.GetXamlDirective(propUsageNamespace, propName.Name);
                if (xamlDirective != null)
                {
                    if ((xamlDirective.AllowedLocation & AllowedMemberLocations.Attribute) == AllowedMemberLocations.None)
                    {
                        xamlDirective = new XamlDirective(propUsageNamespace, propName.Name);
                    }
                    xamlAttachableProperty = xamlDirective;
                }
            }
            if (xamlAttachableProperty != null)
            {
                return xamlAttachableProperty;
            }
            if (tagNamespace == propUsageNamespace)
            {
                return new XamlMember(propName.Name, tagType, false);
            }
            return new XamlDirective(propUsageNamespace, propName.Name);
        }

        public XamlMember GetXamlAttachableProperty(XamlType xamlType, string propertyName)
        {
            if (xamlType.IsUnknown)
            {
                return null;
            }
            XamlMember attachableMember = xamlType.GetAttachableMember(propertyName);
            if (!this.IsVisible(attachableMember, null))
            {
                return null;
            }
            return attachableMember;
        }

        public XamlMember GetXamlProperty(XamlType xamlType, string propertyName, XamlType rootObjectType)
        {
            if (xamlType.IsUnknown)
            {
                return null;
            }
            XamlMember member = xamlType.GetMember(propertyName);
            if (!this.IsVisible(member, rootObjectType))
            {
                return null;
            }
            return member;
        }

        internal XamlType GetXamlType(XamlName typeName)
        {
            return this.GetXamlType(typeName, false);
        }

        internal XamlType GetXamlType(XamlTypeName typeName)
        {
            return this.GetXamlType(typeName, false, false);
        }

        internal XamlType GetXamlType(XamlName typeName, bool returnUnknownTypesOnFailure)
        {
            XamlTypeName xamlTypeName = this.GetXamlTypeName(typeName);
            return this.GetXamlType(xamlTypeName, returnUnknownTypesOnFailure);
        }

        internal XamlType GetXamlType(XamlTypeName typeName, bool returnUnknownTypesOnFailure)
        {
            return this.GetXamlType(typeName, returnUnknownTypesOnFailure, false);
        }

        private XamlType GetXamlType(string ns, string name, IList<XamlType> typeArguments)
        {
            XamlType[] array = new XamlType[typeArguments.Count];
            typeArguments.CopyTo(array, 0);
            XamlType type = this._schemaContext.GetXamlType(ns, name, array);
            if ((type != null) && !type.IsVisibleTo(this.LocalAssembly))
            {
                type = null;
            }
            return type;
        }

        internal XamlType GetXamlType(XamlTypeName typeName, bool returnUnknownTypesOnFailure, bool skipVisibilityCheck)
        {
            XamlType xamlType = this._schemaContext.GetXamlType(typeName);
            if (((xamlType != null) && !skipVisibilityCheck) && !xamlType.IsVisibleTo(this.LocalAssembly))
            {
                xamlType = null;
            }
            if ((xamlType != null) || !returnUnknownTypesOnFailure)
            {
                return xamlType;
            }
            XamlType[] typeArguments = null;
            if (typeName.HasTypeArgs)
            {
                typeArguments = ArrayHelper.ConvertArrayType<XamlTypeName, XamlType>(typeName.TypeArguments, new Func<XamlTypeName, XamlType>(this.GetXamlTypeOrUnknown));
            }
            return new XamlType(typeName.Namespace, typeName.Name, typeArguments, this.SchemaContext);
        }

        internal XamlTypeName GetXamlTypeName(XamlName typeName)
        {
            string xamlNamespace = this.ResolveXamlNameNS(typeName);
            if (xamlNamespace == null)
            {
                throw new XamlParseException(System.Xaml.SR.Get("PrefixNotFound", new object[] { typeName.Prefix }));
            }
            return new XamlTypeName(xamlNamespace, typeName.Name);
        }

        private XamlType GetXamlTypeOrUnknown(XamlTypeName typeName)
        {
            return this.GetXamlType(typeName, true);
        }

        internal virtual bool IsVisible(XamlMember member, XamlType rootObjectType)
        {
            return true;
        }

        private bool PropertyTypeMatchesGenericTagType(XamlType tagType, string tagNs, string propNs, string propTypeName)
        {
            if (((tagNs != propNs) && (tagType.Name != propTypeName)) && !tagType.GetXamlNamespaces().Contains(propNs))
            {
                return false;
            }
            XamlType type = this.GetXamlType(propNs, propTypeName, tagType.TypeArguments);
            return (tagType == type);
        }

        internal XamlMember ResolveDirectiveProperty(string xamlNS, string name)
        {
            if (xamlNS != null)
            {
                return this.SchemaContext.GetXamlDirective(xamlNS, name);
            }
            return null;
        }

        private string ResolveXamlNameNS(XamlName name)
        {
            return (name.Namespace ?? this.FindNamespaceByPrefix(name.Prefix));
        }

        internal XamlType ResolveXamlType(string qName, bool skipVisibilityCheck)
        {
            string str;
            XamlTypeName typeName = XamlTypeName.ParseInternal(qName, this.ResolvePrefixCachedDelegate, out str);
            if (typeName == null)
            {
                throw new XamlParseException(str);
            }
            return this.GetXamlType(typeName, false, skipVisibilityCheck);
        }

        public virtual Assembly LocalAssembly
        {
            get
            {
                return this._localAssembly;
            }
            protected set
            {
                this._localAssembly = value;
            }
        }

        internal Func<string, string> ResolvePrefixCachedDelegate
        {
            get
            {
                if (this._resolvePrefixCachedDelegate == null)
                {
                    this._resolvePrefixCachedDelegate = new Func<string, string>(this.FindNamespaceByPrefix);
                }
                return this._resolvePrefixCachedDelegate;
            }
        }

        public XamlSchemaContext SchemaContext
        {
            get
            {
                return this._schemaContext;
            }
        }
    }
}

