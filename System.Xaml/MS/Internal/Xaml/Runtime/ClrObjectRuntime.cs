namespace MS.Internal.Xaml.Runtime
{
    using MS.Internal.Xaml;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Markup;
    using System.Xaml;
    using System.Xaml.Schema;
    using System.Xml;
    using System.Xml.Serialization;

    internal class ClrObjectRuntime : XamlRuntime
    {
        private bool _ignoreCanConvert;
        private bool _isWriter;

        public ClrObjectRuntime(XamlRuntimeSettings settings, bool isWriter)
        {
            if (settings != null)
            {
                this._ignoreCanConvert = settings.IgnoreCanConvert;
            }
            this._isWriter = isWriter;
        }

        public override void Add(object collection, XamlType collectionType, object value, XamlType valueXamlType)
        {
            try
            {
                collectionType.Invoker.AddToCollection(collection, value);
            }
            catch (Exception exception)
            {
                if (CriticalExceptions.IsCriticalException(exception))
                {
                    throw;
                }
                throw this.CreateException(System.Xaml.SR.Get("AddCollection", new object[] { collectionType }), UnwrapTargetInvocationException(exception));
            }
        }

        public override void AddToDictionary(object collection, XamlType dictionaryType, object value, XamlType valueXamlType, object key)
        {
            try
            {
                dictionaryType.Invoker.AddToDictionary(collection, key, value);
            }
            catch (Exception exception)
            {
                if (CriticalExceptions.IsCriticalException(exception))
                {
                    throw;
                }
                throw this.CreateException(System.Xaml.SR.Get("AddDictionary", new object[] { dictionaryType }), UnwrapTargetInvocationException(exception));
            }
        }

        public override int AttachedPropertyCount(object instance)
        {
            int attachedPropertyCount;
            try
            {
                attachedPropertyCount = AttachablePropertyServices.GetAttachedPropertyCount(instance);
            }
            catch (Exception exception)
            {
                if (CriticalExceptions.IsCriticalException(exception))
                {
                    throw;
                }
                throw this.CreateException(System.Xaml.SR.Get("APSException", new object[] { instance }));
            }
            return attachedPropertyCount;
        }

        protected MethodBase BindToMethod(BindingFlags bindingFlags, MethodBase[] candidates, object[] args)
        {
            object obj2;
            return Type.DefaultBinder.BindToMethod(bindingFlags, candidates, ref args, null, null, null, out obj2);
        }

        public override object CallProvideValue(MarkupExtension me, IServiceProvider serviceProvider)
        {
            object obj3;
            try
            {
                obj3 = me.ProvideValue(serviceProvider);
            }
            catch (Exception exception)
            {
                if (CriticalExceptions.IsCriticalException(exception))
                {
                    throw;
                }
                throw this.CreateException(System.Xaml.SR.Get("ProvideValue", new object[] { me.GetType() }), exception);
            }
            return obj3;
        }

        public override bool CanConvertFrom<T>(ITypeDescriptorContext context, TypeConverter converter)
        {
            bool flag;
            try
            {
                flag = converter.CanConvertFrom(context, typeof(T));
            }
            catch (Exception exception)
            {
                if (CriticalExceptions.IsCriticalException(exception))
                {
                    throw;
                }
                throw this.CreateException(System.Xaml.SR.Get("CanConvertFromFailed", new object[] { typeof(T), converter.GetType() }), exception);
            }
            return flag;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, TypeConverter converter, Type type)
        {
            bool flag;
            try
            {
                flag = converter.CanConvertTo(context, type);
            }
            catch (Exception exception)
            {
                if (CriticalExceptions.IsCriticalException(exception))
                {
                    throw;
                }
                throw this.CreateException(System.Xaml.SR.Get("CanConvertToFailed", new object[] { type, converter.GetType() }), exception);
            }
            return flag;
        }

        public override bool CanConvertToString(IValueSerializerContext context, ValueSerializer serializer, object instance)
        {
            bool flag;
            try
            {
                flag = serializer.CanConvertToString(instance, context);
            }
            catch (Exception exception)
            {
                if (CriticalExceptions.IsCriticalException(exception))
                {
                    throw;
                }
                throw this.CreateException(System.Xaml.SR.Get("TypeConverterFailed2", new object[] { instance, typeof(string) }), exception);
            }
            return flag;
        }

        public override string ConvertToString(IValueSerializerContext context, ValueSerializer serializer, object instance)
        {
            string str;
            try
            {
                str = serializer.ConvertToString(instance, context);
            }
            catch (Exception exception)
            {
                if (CriticalExceptions.IsCriticalException(exception))
                {
                    throw;
                }
                throw this.CreateException(System.Xaml.SR.Get("TypeConverterFailed2", new object[] { instance, typeof(string) }), exception);
            }
            return str;
        }

        public override T ConvertToValue<T>(ITypeDescriptorContext context, TypeConverter converter, object instance)
        {
            T local;
            try
            {
                local = (T) converter.ConvertTo(context, TypeConverterHelper.InvariantEnglishUS, instance, typeof(T));
            }
            catch (Exception exception)
            {
                if (CriticalExceptions.IsCriticalException(exception))
                {
                    throw;
                }
                throw this.CreateException(System.Xaml.SR.Get("TypeConverterFailed2", new object[] { instance, typeof(T) }), exception);
            }
            return local;
        }

        protected virtual Delegate CreateDelegate(Type delegateType, object target, string methodName)
        {
            return SafeReflectionInvoker.CreateDelegate(delegateType, target, methodName);
        }

        private XamlException CreateException(string message)
        {
            return this.CreateException(message, null);
        }

        private XamlException CreateException(string message, Exception innerException)
        {
            XamlException exception;
            if (this._isWriter)
            {
                exception = new XamlObjectWriterException(message, innerException);
            }
            else
            {
                exception = new XamlObjectReaderException(message, innerException);
            }
            if (this.LineInfo == null)
            {
                return exception;
            }
            return this.LineInfo.WithLineInfo(exception);
        }

        public override object CreateFromValue(ServiceProviderContext serviceContext, XamlValueConverter<TypeConverter> ts, object value, XamlMember property)
        {
            if (!(ts == BuiltInValueConverter.String) && !(ts == BuiltInValueConverter.Object))
            {
                return this.CreateObjectWithTypeConverter(serviceContext, ts, value);
            }
            return value;
        }

        public override object CreateInstance(XamlType xamlType, object[] args)
        {
            object obj2;
            if (xamlType.IsUnknown)
            {
                throw this.CreateException(System.Xaml.SR.Get("CannotCreateBadType", new object[] { xamlType.Name }));
            }
            try
            {
                obj2 = this.CreateInstanceWithCtor(xamlType, args);
            }
            catch (MissingMethodException exception)
            {
                throw this.CreateException(System.Xaml.SR.Get("NoConstructor", new object[] { xamlType.UnderlyingType }), exception);
            }
            catch (Exception exception2)
            {
                if (CriticalExceptions.IsCriticalException(exception2))
                {
                    throw;
                }
                throw this.CreateException(System.Xaml.SR.Get("ConstructorInvocation", new object[] { xamlType.UnderlyingType }), UnwrapTargetInvocationException(exception2));
            }
            return obj2;
        }

        protected virtual object CreateInstanceWithCtor(XamlType xamlType, object[] args)
        {
            return xamlType.Invoker.CreateInstance(args);
        }

        private object CreateObjectWithTypeConverter(ServiceProviderContext serviceContext, XamlValueConverter<TypeConverter> ts, object value)
        {
            TypeConverter converterInstance = this.GetConverterInstance<TypeConverter>(ts);
            if (converterInstance != null)
            {
                if (this._ignoreCanConvert && (value.GetType() == typeof(string)))
                {
                    return converterInstance.ConvertFrom(serviceContext, TypeConverterHelper.InvariantEnglishUS, value);
                }
                if (converterInstance.CanConvertFrom(value.GetType()))
                {
                    return converterInstance.ConvertFrom(serviceContext, TypeConverterHelper.InvariantEnglishUS, value);
                }
                return value;
            }
            return value;
        }

        public override object CreateWithFactoryMethod(XamlType xamlType, string methodName, object[] args)
        {
            Type underlyingType = xamlType.UnderlyingType;
            if (underlyingType == null)
            {
                throw this.CreateException(System.Xaml.SR.Get("CannotResolveTypeForFactoryMethod", new object[] { xamlType, methodName }));
            }
            string str = underlyingType.ToString() + "." + methodName;
            object obj2 = null;
            try
            {
                obj2 = this.InvokeFactoryMethod(underlyingType, methodName, args);
            }
            catch (Exception exception)
            {
                if (CriticalExceptions.IsCriticalException(exception))
                {
                    throw;
                }
                throw this.CreateException(System.Xaml.SR.Get("MethodInvocation", new object[] { str }), UnwrapTargetInvocationException(exception));
            }
            if (obj2 == null)
            {
                throw this.CreateException(System.Xaml.SR.Get("FactoryReturnedNull", new object[] { str }));
            }
            return obj2;
        }

        public override object DeferredLoad(ServiceProviderContext serviceContext, XamlValueConverter<XamlDeferringLoader> deferringLoader, XamlReader deferredContent)
        {
            object obj2;
            try
            {
                XamlDeferringLoader converterInstance = this.GetConverterInstance<XamlDeferringLoader>(deferringLoader);
                if (converterInstance == null)
                {
                    throw new XamlObjectWriterException(System.Xaml.SR.Get("DeferringLoaderInstanceNull", new object[] { deferringLoader }));
                }
                obj2 = converterInstance.Load(deferredContent, serviceContext);
            }
            catch (Exception exception)
            {
                IXamlIndexingReader reader = deferredContent as IXamlIndexingReader;
                if ((reader != null) && (reader.CurrentIndex >= 0))
                {
                    reader.CurrentIndex = -1;
                }
                if (!CriticalExceptions.IsCriticalException(exception) && !(exception is XamlException))
                {
                    throw this.CreateException(System.Xaml.SR.Get("DeferredLoad"), exception);
                }
                throw;
            }
            return obj2;
        }

        public override XamlReader DeferredSave(IServiceProvider serviceContext, XamlValueConverter<XamlDeferringLoader> deferringLoader, object value)
        {
            XamlReader reader;
            try
            {
                XamlDeferringLoader converterInstance = this.GetConverterInstance<XamlDeferringLoader>(deferringLoader);
                if (converterInstance == null)
                {
                    throw new XamlObjectWriterException(System.Xaml.SR.Get("DeferringLoaderInstanceNull", new object[] { deferringLoader }));
                }
                reader = converterInstance.Save(value, serviceContext);
            }
            catch (Exception exception)
            {
                if (!CriticalExceptions.IsCriticalException(exception) && !(exception is XamlException))
                {
                    throw this.CreateException(System.Xaml.SR.Get("DeferredSave"), exception);
                }
                throw;
            }
            return reader;
        }

        private static IEnumerable<DictionaryEntry> DictionaryEntriesFromIDictionaryEnumerator(IDictionaryEnumerator enumerator)
        {
            while (true)
            {
                if (!enumerator.MoveNext())
                {
                    yield break;
                }
                yield return enumerator.Entry;
            }
        }

        private static IEnumerable<DictionaryEntry> DictionaryEntriesFromIEnumerator(IEnumerator enumerator)
        {
            while (true)
            {
                if (!enumerator.MoveNext())
                {
                    yield break;
                }
                yield return (DictionaryEntry) enumerator.Current;
            }
        }

        private static IEnumerable<DictionaryEntry> DictionaryEntriesFromIEnumeratorKvp<TKey, TValue>(IEnumerator<KeyValuePair<TKey, TValue>> enumerator)
        {
            while (true)
            {
                if (!enumerator.MoveNext())
                {
                    yield break;
                }
                yield return new DictionaryEntry(enumerator.Current.Key, enumerator.Current.Value);
            }
        }

        public override KeyValuePair<AttachableMemberIdentifier, object>[] GetAttachedProperties(object instance)
        {
            KeyValuePair<AttachableMemberIdentifier, object>[] pairArray2;
            try
            {
                KeyValuePair<AttachableMemberIdentifier, object>[] array = null;
                int attachedPropertyCount = AttachablePropertyServices.GetAttachedPropertyCount(instance);
                if (attachedPropertyCount > 0)
                {
                    array = new KeyValuePair<AttachableMemberIdentifier, object>[attachedPropertyCount];
                    AttachablePropertyServices.CopyPropertiesTo(instance, array, 0);
                }
                pairArray2 = array;
            }
            catch (Exception exception)
            {
                if (CriticalExceptions.IsCriticalException(exception))
                {
                    throw;
                }
                throw this.CreateException(System.Xaml.SR.Get("APSException", new object[] { instance }));
            }
            return pairArray2;
        }

        public override IList<object> GetCollectionItems(object collection, XamlType collectionType)
        {
            List<object> list;
            IEnumerator items = this.GetItems(collection, collectionType);
            try
            {
                list = new List<object>();
                while (items.MoveNext())
                {
                    list.Add(items.Current);
                }
            }
            catch (Exception exception)
            {
                if (CriticalExceptions.IsCriticalException(exception))
                {
                    throw;
                }
                throw this.CreateException(System.Xaml.SR.Get("GetItemsException", new object[] { collectionType }), exception);
            }
            return list;
        }

        public override TConverterBase GetConverterInstance<TConverterBase>(XamlValueConverter<TConverterBase> converter) where TConverterBase: class
        {
            return converter.ConverterInstance;
        }

        public override IEnumerable<DictionaryEntry> GetDictionaryItems(object dictionary, XamlType dictionaryType)
        {
            IEnumerable<DictionaryEntry> enumerable;
            IEnumerator items = this.GetItems(dictionary, dictionaryType);
            try
            {
                IDictionaryEnumerator enumerator = items as IDictionaryEnumerator;
                if (enumerator != null)
                {
                    return DictionaryEntriesFromIDictionaryEnumerator(enumerator);
                }
                Type underlyingType = dictionaryType.KeyType.UnderlyingType;
                Type type2 = dictionaryType.ItemType.UnderlyingType;
                Type type3 = typeof(KeyValuePair<,>).MakeGenericType(new Type[] { underlyingType, type2 });
                if (typeof(IEnumerator<>).MakeGenericType(new Type[] { type3 }).IsAssignableFrom(items.GetType()))
                {
                    return (IEnumerable<DictionaryEntry>) typeof(ClrObjectRuntime).GetMethod("DictionaryEntriesFromIEnumeratorKvp", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(new Type[] { underlyingType, type2 }).Invoke(null, new object[] { items });
                }
                enumerable = DictionaryEntriesFromIEnumerator(items);
            }
            catch (Exception exception)
            {
                if (CriticalExceptions.IsCriticalException(exception))
                {
                    throw;
                }
                throw this.CreateException(System.Xaml.SR.Get("GetItemsException", new object[] { dictionaryType }), exception);
            }
            return enumerable;
        }

        protected MethodInfo GetFactoryMethod(Type type, string methodName, object[] args, BindingFlags flags)
        {
            MethodInfo info = null;
            if ((args == null) || (args.Length == 0))
            {
                info = type.GetMethod(methodName, flags, null, Type.EmptyTypes, null);
            }
            if (info != null)
            {
                return info;
            }
            MemberInfo[] sourceArray = type.GetMember(methodName, MemberTypes.Method, flags);
            MethodBase[] destinationArray = sourceArray as MethodBase[];
            if (destinationArray == null)
            {
                destinationArray = new MethodBase[sourceArray.Length];
                Array.Copy(sourceArray, destinationArray, sourceArray.Length);
            }
            return (MethodInfo) this.BindToMethod(flags, destinationArray, args);
        }

        private IEnumerator GetItems(object collection, XamlType collectionType)
        {
            IEnumerator items;
            try
            {
                items = collectionType.Invoker.GetItems(collection);
            }
            catch (Exception exception)
            {
                if (CriticalExceptions.IsCriticalException(exception))
                {
                    throw;
                }
                throw this.CreateException(System.Xaml.SR.Get("GetItemsException", new object[] { collectionType }), UnwrapTargetInvocationException(exception));
            }
            if (items == null)
            {
                throw this.CreateException(System.Xaml.SR.Get("GetItemsReturnedNull", new object[] { collectionType }));
            }
            return items;
        }

        internal XamlRuntimeSettings GetSettings()
        {
            return new XamlRuntimeSettings { IgnoreCanConvert = this._ignoreCanConvert };
        }

        protected virtual object GetValue(XamlMember member, object obj)
        {
            return member.Invoker.GetValue(obj);
        }

        public override object GetValue(object obj, XamlMember property, bool failIfWriteOnly)
        {
            object obj2;
            try
            {
                if (property.IsDirective)
                {
                    return this.CreateInstance(property.Type, null);
                }
                if (!failIfWriteOnly)
                {
                    try
                    {
                        return this.GetValue(property, obj);
                    }
                    catch (NotSupportedException)
                    {
                        return null;
                    }
                }
                obj2 = this.GetValue(property, obj);
            }
            catch (Exception exception)
            {
                if (CriticalExceptions.IsCriticalException(exception))
                {
                    throw;
                }
                throw this.CreateException(System.Xaml.SR.Get("GetValue", new object[] { property }), UnwrapTargetInvocationException(exception));
            }
            return obj2;
        }

        public override void InitializationGuard(XamlType xamlType, object obj, bool begin)
        {
            try
            {
                ISupportInitialize initialize = obj as ISupportInitialize;
                if (initialize != null)
                {
                    if (begin)
                    {
                        initialize.BeginInit();
                    }
                    else
                    {
                        initialize.EndInit();
                    }
                }
            }
            catch (Exception exception)
            {
                if (CriticalExceptions.IsCriticalException(exception))
                {
                    throw;
                }
                throw this.CreateException(System.Xaml.SR.Get("InitializationGuard", new object[] { xamlType }), exception);
            }
        }

        protected virtual object InvokeFactoryMethod(Type type, string methodName, object[] args)
        {
            return SafeReflectionInvoker.InvokeMethod(this.GetFactoryMethod(type, methodName, args, BindingFlags.Public | BindingFlags.Static), null, args);
        }

        public override void SetConnectionId(object root, int connectionId, object instance)
        {
            try
            {
                IComponentConnector connector = root as IComponentConnector;
                if (connector != null)
                {
                    connector.Connect(connectionId, instance);
                }
            }
            catch (Exception exception)
            {
                if (CriticalExceptions.IsCriticalException(exception))
                {
                    throw;
                }
                throw this.CreateException(System.Xaml.SR.Get("SetConnectionId"), exception);
            }
        }

        public override void SetUriBase(XamlType xamlType, object obj, Uri baseUri)
        {
            try
            {
                IUriContext context = obj as IUriContext;
                if (context != null)
                {
                    context.BaseUri = baseUri;
                }
            }
            catch (Exception exception)
            {
                if (CriticalExceptions.IsCriticalException(exception))
                {
                    throw;
                }
                throw this.CreateException(System.Xaml.SR.Get("AddDictionary", new object[] { xamlType }), exception);
            }
        }

        public override void SetValue(object inst, XamlMember property, object value)
        {
            try
            {
                if (!property.IsDirective)
                {
                    this.SetValue(property, inst, value);
                }
            }
            catch (Exception exception)
            {
                if (CriticalExceptions.IsCriticalException(exception))
                {
                    throw;
                }
                throw this.CreateException(System.Xaml.SR.Get("SetValue", new object[] { property }), UnwrapTargetInvocationException(exception));
            }
        }

        protected virtual void SetValue(XamlMember member, object obj, object value)
        {
            member.Invoker.SetValue(obj, value);
        }

        public override void SetXmlInstance(object inst, XamlMember property, XData xData)
        {
            IXmlSerializable serializable = this.GetValue(inst, property, true) as IXmlSerializable;
            if (serializable == null)
            {
                throw this.CreateException(System.Xaml.SR.Get("XmlDataNull", new object[] { property.Name }));
            }
            XmlReader xmlReader = xData.XmlReader as XmlReader;
            if (xmlReader == null)
            {
                throw new XamlInternalException(System.Xaml.SR.Get("XmlValueNotReader", new object[] { property.Name }));
            }
            try
            {
                serializable.ReadXml(xmlReader);
            }
            catch (Exception exception)
            {
                if (CriticalExceptions.IsCriticalException(exception))
                {
                    throw;
                }
                throw this.CreateException(System.Xaml.SR.Get("SetXmlInstance", new object[] { property }), exception);
            }
        }

        public override ShouldSerializeResult ShouldSerialize(XamlMember member, object instance)
        {
            ShouldSerializeResult result;
            try
            {
                result = member.Invoker.ShouldSerializeValue(instance);
            }
            catch (Exception exception)
            {
                if (CriticalExceptions.IsCriticalException(exception))
                {
                    throw;
                }
                throw this.CreateException(System.Xaml.SR.Get("ShouldSerializeFailed", new object[] { member }));
            }
            return result;
        }

        private static Exception UnwrapTargetInvocationException(Exception e)
        {
            if ((e is TargetInvocationException) && (e.InnerException != null))
            {
                return e.InnerException;
            }
            return e;
        }

        public override IAddLineInfo LineInfo { get; set; }



        [CompilerGenerated]
        private sealed class <DictionaryEntriesFromIEnumeratorKvp>d__7<TKey, TValue> : IEnumerable<DictionaryEntry>, IEnumerable, IEnumerator<DictionaryEntry>, IEnumerator, IDisposable
        {
            private int <>1__state;
            private DictionaryEntry <>2__current;
            public IEnumerator<KeyValuePair<TKey, TValue>> <>3__enumerator;
            private int <>l__initialThreadId;
            public IEnumerator<KeyValuePair<TKey, TValue>> enumerator;

            [DebuggerHidden]
            public <DictionaryEntriesFromIEnumeratorKvp>d__7(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private bool MoveNext()
            {
                switch (this.<>1__state)
                {
                    case 0:
                        this.<>1__state = -1;
                        break;

                    case 1:
                        this.<>1__state = -1;
                        break;

                    default:
                        goto Label_0078;
                }
                if (this.enumerator.MoveNext())
                {
                    this.<>2__current = new DictionaryEntry(this.enumerator.Current.Key, this.enumerator.Current.Value);
                    this.<>1__state = 1;
                    return true;
                }
            Label_0078:
                return false;
            }

            [DebuggerHidden]
            IEnumerator<DictionaryEntry> IEnumerable<DictionaryEntry>.GetEnumerator()
            {
                ClrObjectRuntime.<DictionaryEntriesFromIEnumeratorKvp>d__7<TKey, TValue> d__;
                if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                {
                    this.<>1__state = 0;
                    d__ = (ClrObjectRuntime.<DictionaryEntriesFromIEnumeratorKvp>d__7<TKey, TValue>) this;
                }
                else
                {
                    d__ = new ClrObjectRuntime.<DictionaryEntriesFromIEnumeratorKvp>d__7<TKey, TValue>(0);
                }
                d__.enumerator = this.<>3__enumerator;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<System.Collections.DictionaryEntry>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            void IDisposable.Dispose()
            {
            }

            DictionaryEntry IEnumerator<DictionaryEntry>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }
    }
}

