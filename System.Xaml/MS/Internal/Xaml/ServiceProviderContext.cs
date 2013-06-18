namespace MS.Internal.Xaml
{
    using MS.Internal.Xaml.Context;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Windows.Markup;
    using System.Xaml;

    internal class ServiceProviderContext : ITypeDescriptorContext, IServiceProvider, IXamlTypeResolver, IUriContext, IAmbientProvider, IXamlSchemaContextProvider, IRootObjectProvider, IXamlNamespaceResolver, IProvideValueTarget, IXamlNameResolver, IDestinationTypeProvider
    {
        private ObjectWriterContext _xamlContext;

        event EventHandler IXamlNameResolver.OnNameScopeInitializationComplete
        {
            add
            {
                this._xamlContext.AddNameScopeInitializationCompleteSubscriber(value);
            }
            remove
            {
                this._xamlContext.RemoveNameScopeInitializationCompleteSubscriber(value);
            }
        }

        public ServiceProviderContext(ObjectWriterContext context)
        {
            this._xamlContext = context;
        }

        public Type GetDestinationType()
        {
            return this._xamlContext.GetDestinationType().UnderlyingType;
        }

        void ITypeDescriptorContext.OnComponentChanged()
        {
        }

        bool ITypeDescriptorContext.OnComponentChanging()
        {
            return false;
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType == typeof(IXamlTypeResolver))
            {
                return this;
            }
            if (serviceType == typeof(IUriContext))
            {
                return this;
            }
            if (serviceType == typeof(IAmbientProvider))
            {
                return this;
            }
            if (serviceType == typeof(IXamlSchemaContextProvider))
            {
                return this;
            }
            if (serviceType == typeof(IProvideValueTarget))
            {
                return this;
            }
            if (serviceType == typeof(IRootObjectProvider))
            {
                return this;
            }
            if (serviceType == typeof(IXamlNamespaceResolver))
            {
                return this;
            }
            if (serviceType == typeof(IXamlNameResolver))
            {
                return this;
            }
            if (serviceType == typeof(IXamlObjectWriterFactory))
            {
                return new XamlObjectWriterFactory(this._xamlContext);
            }
            if (serviceType == typeof(IDestinationTypeProvider))
            {
                return this;
            }
            return null;
        }

        Type IXamlTypeResolver.Resolve(string qName)
        {
            return this._xamlContext.ServiceProvider_Resolve(qName);
        }

        IEnumerable<object> IAmbientProvider.GetAllAmbientValues(params XamlType[] types)
        {
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }
            foreach (XamlType type in types)
            {
                if (type == null)
                {
                    throw new ArgumentException(System.Xaml.SR.Get("ValueInArrayIsNull", new object[] { "types" }));
                }
            }
            return this._xamlContext.ServiceProvider_GetAllAmbientValues(types);
        }

        IEnumerable<AmbientPropertyValue> IAmbientProvider.GetAllAmbientValues(IEnumerable<XamlType> ceilingTypes, params XamlMember[] properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }
            foreach (XamlMember member in properties)
            {
                if (member == null)
                {
                    throw new ArgumentException(System.Xaml.SR.Get("ValueInArrayIsNull", new object[] { "properties" }));
                }
            }
            return this._xamlContext.ServiceProvider_GetAllAmbientValues(ceilingTypes, properties);
        }

        IEnumerable<AmbientPropertyValue> IAmbientProvider.GetAllAmbientValues(IEnumerable<XamlType> ceilingTypes, bool searchLiveStackOnly, IEnumerable<XamlType> types, params XamlMember[] properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }
            foreach (XamlMember member in properties)
            {
                if (member == null)
                {
                    throw new ArgumentException(System.Xaml.SR.Get("ValueInArrayIsNull", new object[] { "properties" }));
                }
            }
            return this._xamlContext.ServiceProvider_GetAllAmbientValues(ceilingTypes, searchLiveStackOnly, types, properties);
        }

        object IAmbientProvider.GetFirstAmbientValue(params XamlType[] types)
        {
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }
            foreach (XamlType type in types)
            {
                if (type == null)
                {
                    throw new ArgumentException(System.Xaml.SR.Get("ValueInArrayIsNull", new object[] { "types" }));
                }
            }
            return this._xamlContext.ServiceProvider_GetFirstAmbientValue(types);
        }

        AmbientPropertyValue IAmbientProvider.GetFirstAmbientValue(IEnumerable<XamlType> ceilingTypes, params XamlMember[] properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }
            foreach (XamlMember member in properties)
            {
                if (member == null)
                {
                    throw new ArgumentException(System.Xaml.SR.Get("ValueInArrayIsNull", new object[] { "properties" }));
                }
            }
            return this._xamlContext.ServiceProvider_GetFirstAmbientValue(ceilingTypes, properties);
        }

        IEnumerable<KeyValuePair<string, object>> IXamlNameResolver.GetAllNamesAndValuesInScope()
        {
            return this._xamlContext.GetAllNamesAndValuesInScope();
        }

        object IXamlNameResolver.GetFixupToken(IEnumerable<string> names)
        {
            return ((IXamlNameResolver) this).GetFixupToken(names, false);
        }

        object IXamlNameResolver.GetFixupToken(IEnumerable<string> names, bool canAssignDirectly)
        {
            if (this._xamlContext.NameResolutionComplete)
            {
                return null;
            }
            NameFixupToken token = new NameFixupToken {
                CanAssignDirectly = canAssignDirectly
            };
            token.NeededNames.AddRange(names);
            if (token.CanAssignDirectly && (token.NeededNames.Count != 1))
            {
                throw new ArgumentException(System.Xaml.SR.Get("SimpleFixupsMustHaveOneName"), "names");
            }
            if (this._xamlContext.CurrentType == null)
            {
                if (this._xamlContext.ParentProperty == XamlLanguage.Initialization)
                {
                    token.FixupType = FixupType.ObjectInitializationValue;
                    token.Target.Instance = this._xamlContext.GrandParentInstance;
                    token.Target.InstanceWasGotten = this._xamlContext.GrandParentIsObjectFromMember;
                    token.Target.InstanceType = this._xamlContext.GrandParentType;
                    token.Target.Property = this._xamlContext.GrandParentProperty;
                }
                else
                {
                    token.FixupType = FixupType.PropertyValue;
                    token.Target.Instance = this._xamlContext.ParentInstance;
                    token.Target.InstanceWasGotten = this._xamlContext.ParentIsObjectFromMember;
                    token.Target.InstanceType = this._xamlContext.ParentType;
                    token.Target.Property = this._xamlContext.ParentProperty;
                }
            }
            else
            {
                token.FixupType = FixupType.MarkupExtensionRerun;
                token.Target.Instance = this._xamlContext.ParentInstance;
                token.Target.InstanceWasGotten = this._xamlContext.ParentIsObjectFromMember;
                token.Target.InstanceType = this._xamlContext.ParentType;
                token.Target.Property = this._xamlContext.ParentProperty;
            }
            if (token.CanAssignDirectly)
            {
                token.NameScopeDictionaryList.AddRange(this._xamlContext.StackWalkOfNameScopes);
                return token;
            }
            token.SavedContext = this._xamlContext.GetSavedContext((token.FixupType == FixupType.MarkupExtensionRerun) ? SavedContextType.ReparseMarkupExtension : SavedContextType.ReparseValue);
            return token;
        }

        object IXamlNameResolver.Resolve(string name)
        {
            bool flag;
            return this._xamlContext.ResolveName(name, out flag);
        }

        object IXamlNameResolver.Resolve(string name, out bool isFullyInitialized)
        {
            return this._xamlContext.ResolveName(name, out isFullyInitialized);
        }

        string IXamlNamespaceResolver.GetNamespace(string prefix)
        {
            return this._xamlContext.FindNamespaceByPrefix(prefix);
        }

        IEnumerable<NamespaceDeclaration> IXamlNamespaceResolver.GetNamespacePrefixes()
        {
            return this._xamlContext.GetNamespacePrefixes();
        }

        IContainer ITypeDescriptorContext.Container
        {
            get
            {
                return null;
            }
        }

        object ITypeDescriptorContext.Instance
        {
            get
            {
                return null;
            }
        }

        PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor
        {
            get
            {
                return null;
            }
        }

        object IProvideValueTarget.TargetObject
        {
            get
            {
                return this._xamlContext.ParentInstance;
            }
        }

        object IProvideValueTarget.TargetProperty
        {
            get
            {
                return ContextServices.GetTargetProperty(this._xamlContext);
            }
        }

        Uri IUriContext.BaseUri
        {
            get
            {
                return this._xamlContext.BaseUri;
            }
            set
            {
                throw new InvalidOperationException(System.Xaml.SR.Get("MustNotCallSetter"));
            }
        }

        object IRootObjectProvider.RootObject
        {
            get
            {
                return this._xamlContext.RootInstance;
            }
        }

        bool IXamlNameResolver.IsFixupTokenAvailable
        {
            get
            {
                return !this._xamlContext.NameResolutionComplete;
            }
        }

        XamlSchemaContext IXamlSchemaContextProvider.SchemaContext
        {
            get
            {
                return this._xamlContext.SchemaContext;
            }
        }
    }
}

