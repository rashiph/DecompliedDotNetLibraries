namespace MS.Internal.Xaml.Runtime
{
    using MS.Internal.Xaml;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Security;
    using System.Windows.Markup;
    using System.Xaml;
    using System.Xaml.Permissions;
    using System.Xaml.Schema;

    internal class PartialTrustTolerantRuntime : XamlRuntime
    {
        private XamlAccessLevel _accessLevel;
        private ClrObjectRuntime _elevatedRuntime;
        private bool _memberAccessPermissionDenied;
        private XamlSchemaContext _schemaContext;
        private ClrObjectRuntime _transparentRuntime;

        public PartialTrustTolerantRuntime(XamlRuntimeSettings runtimeSettings, XamlAccessLevel accessLevel, XamlSchemaContext schemaContext)
        {
            this._transparentRuntime = new ClrObjectRuntime(runtimeSettings, true);
            this._accessLevel = accessLevel;
            this._schemaContext = schemaContext;
        }

        public override void Add(object collection, XamlType collectionType, object value, XamlType valueXamlType)
        {
            this._transparentRuntime.Add(collection, collectionType, value, valueXamlType);
        }

        public override void AddToDictionary(object collection, XamlType dictionaryType, object value, XamlType valueXamlType, object key)
        {
            this._transparentRuntime.AddToDictionary(collection, dictionaryType, value, valueXamlType, key);
        }

        public override int AttachedPropertyCount(object instance)
        {
            return this._transparentRuntime.AttachedPropertyCount(instance);
        }

        public override object CallProvideValue(MarkupExtension me, IServiceProvider serviceProvider)
        {
            return this._transparentRuntime.CallProvideValue(me, serviceProvider);
        }

        public override bool CanConvertFrom<T>(ITypeDescriptorContext context, TypeConverter converter)
        {
            return this._transparentRuntime.CanConvertFrom<T>(context, converter);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, TypeConverter converter, Type type)
        {
            return this._transparentRuntime.CanConvertTo(context, converter, type);
        }

        public override bool CanConvertToString(IValueSerializerContext context, ValueSerializer serializer, object instance)
        {
            return this._transparentRuntime.CanConvertToString(context, serializer, instance);
        }

        public override string ConvertToString(IValueSerializerContext context, ValueSerializer serializer, object instance)
        {
            return this._transparentRuntime.ConvertToString(context, serializer, instance);
        }

        public override T ConvertToValue<T>(ITypeDescriptorContext context, TypeConverter converter, object instance)
        {
            return this._transparentRuntime.ConvertToValue<T>(context, converter, instance);
        }

        public override object CreateFromValue(ServiceProviderContext serviceContext, XamlValueConverter<TypeConverter> ts, object value, XamlMember property)
        {
            if ((!this.MemberAccessPermissionDenied || ts.IsPublic) || !IsDefaultConverter<TypeConverter>(ts))
            {
                try
                {
                    return this._transparentRuntime.CreateFromValue(serviceContext, ts, value, property);
                }
                catch (MissingMethodException)
                {
                    this.EnsureElevatedRuntime();
                }
                catch (MethodAccessException)
                {
                    this.MemberAccessPermissionDenied = true;
                }
                catch (SecurityException)
                {
                    this.MemberAccessPermissionDenied = true;
                }
            }
            return this._elevatedRuntime.CreateFromValue(serviceContext, ts, value, property);
        }

        public override object CreateInstance(XamlType xamlType, object[] args)
        {
            if ((!this.MemberAccessPermissionDenied || xamlType.IsPublic) || !HasDefaultInvoker(xamlType))
            {
                try
                {
                    return this._transparentRuntime.CreateInstance(xamlType, args);
                }
                catch (XamlException exception)
                {
                    if (!(exception.InnerException is MethodAccessException))
                    {
                        if (!(exception.InnerException is MissingMethodException))
                        {
                            throw;
                        }
                        this.EnsureElevatedRuntime();
                    }
                    else
                    {
                        this.MemberAccessPermissionDenied = true;
                    }
                }
                catch (SecurityException)
                {
                    this.MemberAccessPermissionDenied = true;
                }
            }
            return this._elevatedRuntime.CreateInstance(xamlType, args);
        }

        public override object CreateWithFactoryMethod(XamlType xamlType, string methodName, object[] args)
        {
            if (!this.MemberAccessPermissionDenied || xamlType.IsPublic)
            {
                try
                {
                    return this._transparentRuntime.CreateWithFactoryMethod(xamlType, methodName, args);
                }
                catch (XamlException exception)
                {
                    if (!(exception.InnerException is MethodAccessException))
                    {
                        if (!(exception.InnerException is MissingMethodException))
                        {
                            throw;
                        }
                        this.EnsureElevatedRuntime();
                    }
                    else
                    {
                        this.MemberAccessPermissionDenied = true;
                    }
                }
                catch (SecurityException)
                {
                    this.MemberAccessPermissionDenied = true;
                }
            }
            return this._elevatedRuntime.CreateWithFactoryMethod(xamlType, methodName, args);
        }

        public override object DeferredLoad(ServiceProviderContext serviceContext, XamlValueConverter<XamlDeferringLoader> deferringLoader, XamlReader deferredContent)
        {
            if ((!this.MemberAccessPermissionDenied || deferringLoader.IsPublic) || !IsDefaultConverter<XamlDeferringLoader>(deferringLoader))
            {
                try
                {
                    return this._transparentRuntime.DeferredLoad(serviceContext, deferringLoader, deferredContent);
                }
                catch (XamlException exception)
                {
                    if (!(exception.InnerException is MissingMethodException))
                    {
                        if (!(exception.InnerException is MethodAccessException))
                        {
                            throw;
                        }
                        this.MemberAccessPermissionDenied = true;
                    }
                    else
                    {
                        this.EnsureElevatedRuntime();
                    }
                }
                catch (SecurityException)
                {
                    this.MemberAccessPermissionDenied = true;
                }
            }
            return this._elevatedRuntime.DeferredLoad(serviceContext, deferringLoader, deferredContent);
        }

        public override XamlReader DeferredSave(IServiceProvider context, XamlValueConverter<XamlDeferringLoader> deferringLoader, object value)
        {
            if ((!this.MemberAccessPermissionDenied || deferringLoader.IsPublic) || !IsDefaultConverter<XamlDeferringLoader>(deferringLoader))
            {
                try
                {
                    return this._transparentRuntime.DeferredSave(context, deferringLoader, value);
                }
                catch (XamlException exception)
                {
                    if (!(exception.InnerException is MissingMethodException))
                    {
                        if (!(exception.InnerException is MethodAccessException))
                        {
                            throw;
                        }
                        this.MemberAccessPermissionDenied = true;
                    }
                    else
                    {
                        this.EnsureElevatedRuntime();
                    }
                }
                catch (SecurityException)
                {
                    this.MemberAccessPermissionDenied = true;
                }
            }
            return this._elevatedRuntime.DeferredSave(context, deferringLoader, value);
        }

        [SecuritySafeCritical]
        private void EnsureElevatedRuntime()
        {
            if (this._elevatedRuntime == null)
            {
                this._elevatedRuntime = new DynamicMethodRuntime(this._transparentRuntime.GetSettings(), this._schemaContext, this._accessLevel);
                this._elevatedRuntime.LineInfo = this.LineInfo;
            }
        }

        public override KeyValuePair<AttachableMemberIdentifier, object>[] GetAttachedProperties(object instance)
        {
            return this._transparentRuntime.GetAttachedProperties(instance);
        }

        public override IList<object> GetCollectionItems(object collection, XamlType collectionType)
        {
            return this._transparentRuntime.GetCollectionItems(collection, collectionType);
        }

        public override TConverterBase GetConverterInstance<TConverterBase>(XamlValueConverter<TConverterBase> converter) where TConverterBase: class
        {
            if ((!this.MemberAccessPermissionDenied || converter.IsPublic) || !IsDefaultConverter<TConverterBase>(converter))
            {
                try
                {
                    return this._transparentRuntime.GetConverterInstance<TConverterBase>(converter);
                }
                catch (MissingMethodException)
                {
                    this.EnsureElevatedRuntime();
                }
                catch (MethodAccessException)
                {
                    this.MemberAccessPermissionDenied = true;
                }
                catch (SecurityException)
                {
                    this.MemberAccessPermissionDenied = true;
                }
            }
            return this._elevatedRuntime.GetConverterInstance<TConverterBase>(converter);
        }

        public override IEnumerable<DictionaryEntry> GetDictionaryItems(object dictionary, XamlType dictionaryType)
        {
            return this._transparentRuntime.GetDictionaryItems(dictionary, dictionaryType);
        }

        public override object GetValue(object obj, XamlMember property, bool failIfWriteOnly)
        {
            if ((!this.MemberAccessPermissionDenied || property.IsReadPublic) || !HasDefaultInvoker(property))
            {
                try
                {
                    return this._transparentRuntime.GetValue(obj, property, failIfWriteOnly);
                }
                catch (XamlException exception)
                {
                    if (!(exception.InnerException is MethodAccessException))
                    {
                        throw;
                    }
                    this.MemberAccessPermissionDenied = true;
                }
                catch (SecurityException)
                {
                    this.MemberAccessPermissionDenied = true;
                }
            }
            return this._elevatedRuntime.GetValue(obj, property, failIfWriteOnly);
        }

        private static bool HasDefaultInvoker(XamlMember xamlMember)
        {
            return (xamlMember.Invoker.GetType() == typeof(XamlMemberInvoker));
        }

        private static bool HasDefaultInvoker(XamlType xamlType)
        {
            return (xamlType.Invoker.GetType() == typeof(XamlTypeInvoker));
        }

        public override void InitializationGuard(XamlType xamlType, object obj, bool begin)
        {
            this._transparentRuntime.InitializationGuard(xamlType, obj, begin);
        }

        private static bool IsDefaultConverter<TConverterBase>(XamlValueConverter<TConverterBase> converter) where TConverterBase: class
        {
            return (converter.GetType() == typeof(XamlValueConverter<TConverterBase>));
        }

        public override void SetConnectionId(object root, int connectionId, object instance)
        {
            this._transparentRuntime.SetConnectionId(root, connectionId, instance);
        }

        public override void SetUriBase(XamlType xamlType, object obj, Uri baseUri)
        {
            this._transparentRuntime.SetUriBase(xamlType, obj, baseUri);
        }

        public override void SetValue(object obj, XamlMember property, object value)
        {
            if ((!this.MemberAccessPermissionDenied || property.IsWritePublic) || !HasDefaultInvoker(property))
            {
                try
                {
                    this._transparentRuntime.SetValue(obj, property, value);
                    return;
                }
                catch (XamlException exception)
                {
                    if (!(exception.InnerException is MethodAccessException))
                    {
                        throw;
                    }
                    this.MemberAccessPermissionDenied = true;
                }
                catch (SecurityException)
                {
                    this.MemberAccessPermissionDenied = true;
                }
            }
            this._elevatedRuntime.SetValue(obj, property, value);
        }

        public override void SetXmlInstance(object inst, XamlMember property, XData xData)
        {
            if (!this.MemberAccessPermissionDenied || property.IsReadPublic)
            {
                try
                {
                    this._transparentRuntime.SetXmlInstance(inst, property, xData);
                    return;
                }
                catch (XamlException exception)
                {
                    if (!(exception.InnerException is MethodAccessException))
                    {
                        throw;
                    }
                    this.MemberAccessPermissionDenied = true;
                }
                catch (SecurityException)
                {
                    this.MemberAccessPermissionDenied = true;
                }
            }
            this._elevatedRuntime.SetXmlInstance(inst, property, xData);
        }

        public override ShouldSerializeResult ShouldSerialize(XamlMember member, object instance)
        {
            return this._transparentRuntime.ShouldSerialize(member, instance);
        }

        public override IAddLineInfo LineInfo
        {
            get
            {
                return this._transparentRuntime.LineInfo;
            }
            set
            {
                this._transparentRuntime.LineInfo = value;
                if (this._elevatedRuntime != null)
                {
                    this._elevatedRuntime.LineInfo = value;
                }
            }
        }

        private bool MemberAccessPermissionDenied
        {
            get
            {
                return this._memberAccessPermissionDenied;
            }
            set
            {
                this._memberAccessPermissionDenied = value;
                if (value)
                {
                    this.EnsureElevatedRuntime();
                }
            }
        }
    }
}

