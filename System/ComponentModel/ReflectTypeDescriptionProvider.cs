namespace System.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    internal sealed class ReflectTypeDescriptionProvider : TypeDescriptionProvider
    {
        private static Hashtable _attributeCache;
        private static object _dictionaryKey = new object();
        private static Hashtable _editorTables;
        private static Hashtable _eventCache;
        private static Hashtable _extendedPropertyCache;
        private static readonly Guid _extenderPropertiesKey = Guid.NewGuid();
        private static readonly Guid _extenderProviderKey = Guid.NewGuid();
        private static readonly Guid _extenderProviderPropertiesKey = Guid.NewGuid();
        private static object _internalSyncObject = new object();
        private static object _intrinsicNullableKey = new object();
        private static object _intrinsicReferenceKey = new object();
        private static Hashtable _intrinsicTypeConverters;
        private static Hashtable _propertyCache;
        private static readonly Type[] _skipInterfaceAttributeList = new Type[] { typeof(GuidAttribute), typeof(ComVisibleAttribute), typeof(InterfaceTypeAttribute) };
        private static Type[] _typeConstructor = new Type[] { typeof(Type) };
        private Hashtable _typeData;

        internal ReflectTypeDescriptionProvider()
        {
        }

        internal static void AddEditorTable(Type editorBaseType, Hashtable table)
        {
            if (editorBaseType == null)
            {
                throw new ArgumentNullException("editorBaseType");
            }
            lock (_internalSyncObject)
            {
                if (_editorTables == null)
                {
                    _editorTables = new Hashtable(4);
                }
                if (!_editorTables.ContainsKey(editorBaseType))
                {
                    _editorTables[editorBaseType] = table;
                }
            }
        }

        private static object CreateInstance(Type objectType, Type callingType)
        {
            object obj2 = SecurityUtils.SecureConstructorInvoke(objectType, _typeConstructor, new object[] { callingType }, false);
            if (obj2 == null)
            {
                obj2 = SecurityUtils.SecureCreateInstance(objectType);
            }
            return obj2;
        }

        public override object CreateInstance(IServiceProvider provider, Type objectType, Type[] argTypes, object[] args)
        {
            object obj2 = null;
            if (argTypes != null)
            {
                obj2 = SecurityUtils.SecureConstructorInvoke(objectType, argTypes, args, true, BindingFlags.ExactBinding);
            }
            else
            {
                if (args != null)
                {
                    argTypes = new Type[args.Length];
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (args[i] != null)
                        {
                            argTypes[i] = args[i].GetType();
                        }
                        else
                        {
                            argTypes[i] = typeof(object);
                        }
                    }
                }
                else
                {
                    argTypes = new Type[0];
                }
                obj2 = SecurityUtils.SecureConstructorInvoke(objectType, argTypes, args, true);
            }
            if (obj2 == null)
            {
                obj2 = SecurityUtils.SecureCreateInstance(objectType, args);
            }
            return obj2;
        }

        internal AttributeCollection GetAttributes(Type type)
        {
            return this.GetTypeData(type, true).GetAttributes();
        }

        public override IDictionary GetCache(object instance)
        {
            IComponent component = instance as IComponent;
            if ((component != null) && (component.Site != null))
            {
                IDictionaryService service = component.Site.GetService(typeof(IDictionaryService)) as IDictionaryService;
                if (service != null)
                {
                    IDictionary dictionary = service.GetValue(_dictionaryKey) as IDictionary;
                    if (dictionary == null)
                    {
                        dictionary = new Hashtable();
                        service.SetValue(_dictionaryKey, dictionary);
                    }
                    return dictionary;
                }
            }
            return null;
        }

        internal string GetClassName(Type type)
        {
            return this.GetTypeData(type, true).GetClassName(null);
        }

        internal string GetComponentName(Type type, object instance)
        {
            return this.GetTypeData(type, true).GetComponentName(instance);
        }

        internal TypeConverter GetConverter(Type type, object instance)
        {
            return this.GetTypeData(type, true).GetConverter(instance);
        }

        internal EventDescriptor GetDefaultEvent(Type type, object instance)
        {
            return this.GetTypeData(type, true).GetDefaultEvent(instance);
        }

        internal PropertyDescriptor GetDefaultProperty(Type type, object instance)
        {
            return this.GetTypeData(type, true).GetDefaultProperty(instance);
        }

        internal object GetEditor(Type type, object instance, Type editorBaseType)
        {
            return this.GetTypeData(type, true).GetEditor(instance, editorBaseType);
        }

        private static Hashtable GetEditorTable(Type editorBaseType)
        {
            if (_editorTables == null)
            {
                lock (_internalSyncObject)
                {
                    if (_editorTables == null)
                    {
                        _editorTables = new Hashtable(4);
                    }
                }
            }
            object obj2 = _editorTables[editorBaseType];
            if (obj2 == null)
            {
                RuntimeHelpers.RunClassConstructor(editorBaseType.TypeHandle);
                obj2 = _editorTables[editorBaseType];
                if (obj2 == null)
                {
                    lock (_internalSyncObject)
                    {
                        obj2 = _editorTables[editorBaseType];
                        if (obj2 == null)
                        {
                            _editorTables[editorBaseType] = _editorTables;
                        }
                    }
                }
            }
            if (obj2 == _editorTables)
            {
                obj2 = null;
            }
            return (Hashtable) obj2;
        }

        internal EventDescriptorCollection GetEvents(Type type)
        {
            return this.GetTypeData(type, true).GetEvents();
        }

        internal AttributeCollection GetExtendedAttributes(object instance)
        {
            return AttributeCollection.Empty;
        }

        internal string GetExtendedClassName(object instance)
        {
            return this.GetClassName(instance.GetType());
        }

        internal string GetExtendedComponentName(object instance)
        {
            return this.GetComponentName(instance.GetType(), instance);
        }

        internal TypeConverter GetExtendedConverter(object instance)
        {
            return this.GetConverter(instance.GetType(), instance);
        }

        internal EventDescriptor GetExtendedDefaultEvent(object instance)
        {
            return null;
        }

        internal PropertyDescriptor GetExtendedDefaultProperty(object instance)
        {
            return null;
        }

        internal object GetExtendedEditor(object instance, Type editorBaseType)
        {
            return this.GetEditor(instance.GetType(), instance, editorBaseType);
        }

        internal EventDescriptorCollection GetExtendedEvents(object instance)
        {
            return EventDescriptorCollection.Empty;
        }

        internal PropertyDescriptorCollection GetExtendedProperties(object instance)
        {
            Type c = instance.GetType();
            IExtenderProvider[] extenderProviders = this.GetExtenderProviders(instance);
            IDictionary cache = TypeDescriptor.GetCache(instance);
            if (extenderProviders.Length == 0)
            {
                return PropertyDescriptorCollection.Empty;
            }
            PropertyDescriptorCollection empty = null;
            if (cache != null)
            {
                empty = cache[_extenderPropertiesKey] as PropertyDescriptorCollection;
            }
            if (empty == null)
            {
                ArrayList list = null;
                for (int i = 0; i < extenderProviders.Length; i++)
                {
                    PropertyDescriptor[] descriptorArray = ReflectGetExtendedProperties(extenderProviders[i]);
                    if (list == null)
                    {
                        list = new ArrayList(descriptorArray.Length * extenderProviders.Length);
                    }
                    for (int j = 0; j < descriptorArray.Length; j++)
                    {
                        PropertyDescriptor descriptor = descriptorArray[j];
                        ExtenderProvidedPropertyAttribute attribute = descriptor.Attributes[typeof(ExtenderProvidedPropertyAttribute)] as ExtenderProvidedPropertyAttribute;
                        if (attribute != null)
                        {
                            Type receiverType = attribute.ReceiverType;
                            if ((receiverType != null) && receiverType.IsAssignableFrom(c))
                            {
                                list.Add(descriptor);
                            }
                        }
                    }
                }
                if (list != null)
                {
                    PropertyDescriptor[] array = new PropertyDescriptor[list.Count];
                    list.CopyTo(array, 0);
                    empty = new PropertyDescriptorCollection(array, true);
                }
                else
                {
                    empty = PropertyDescriptorCollection.Empty;
                }
                if (cache != null)
                {
                    cache[_extenderPropertiesKey] = empty;
                }
            }
            return empty;
        }

        internal object GetExtendedPropertyOwner(object instance, PropertyDescriptor pd)
        {
            return this.GetPropertyOwner(instance.GetType(), instance, pd);
        }

        public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
        {
            return null;
        }

        protected internal override IExtenderProvider[] GetExtenderProviders(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            IComponent component = instance as IComponent;
            if ((component != null) && (component.Site != null))
            {
                IExtenderListService service = component.Site.GetService(typeof(IExtenderListService)) as IExtenderListService;
                IDictionary cache = TypeDescriptor.GetCache(instance);
                if (service != null)
                {
                    return GetExtenders(service.GetExtenderProviders(), instance, cache);
                }
                IContainer container = component.Site.Container;
                if (container != null)
                {
                    return GetExtenders(container.Components, instance, cache);
                }
            }
            return new IExtenderProvider[0];
        }

        private static IExtenderProvider[] GetExtenders(ICollection components, object instance, IDictionary cache)
        {
            bool flag = false;
            int num = 0;
            IExtenderProvider[] providerArray = null;
            ulong num2 = 0L;
            int num3 = 0x40;
            IExtenderProvider[] providerArray2 = components as IExtenderProvider[];
            if (cache != null)
            {
                providerArray = cache[_extenderProviderKey] as IExtenderProvider[];
            }
            if (providerArray == null)
            {
                flag = true;
            }
            int index = 0;
            int num5 = 0;
            if (providerArray2 != null)
            {
                index = 0;
                while (index < providerArray2.Length)
                {
                    if (providerArray2[index].CanExtend(instance))
                    {
                        num++;
                        if (index < num3)
                        {
                            num2 |= ((ulong) 1L) << index;
                        }
                        if (!flag && ((num5 >= providerArray.Length) || (providerArray2[index] != providerArray[num5++])))
                        {
                            flag = true;
                        }
                    }
                    index++;
                }
            }
            else if (components != null)
            {
                foreach (object obj2 in components)
                {
                    IExtenderProvider provider = obj2 as IExtenderProvider;
                    if ((provider != null) && provider.CanExtend(instance))
                    {
                        num++;
                        if (index < num3)
                        {
                            num2 |= ((ulong) 1L) << index;
                        }
                        if (!flag && ((num5 >= providerArray.Length) || (provider != providerArray[num5++])))
                        {
                            flag = true;
                        }
                    }
                    index++;
                }
            }
            if ((providerArray != null) && (num != providerArray.Length))
            {
                flag = true;
            }
            if (!flag)
            {
                return providerArray;
            }
            if ((providerArray2 == null) || (num != providerArray2.Length))
            {
                IExtenderProvider[] providerArray3 = new IExtenderProvider[num];
                index = 0;
                num5 = 0;
                if ((providerArray2 != null) && (num > 0))
                {
                    while (index < providerArray2.Length)
                    {
                        if (((index < num3) && ((num2 & (((ulong) 1L) << index)) != 0L)) || ((index >= num3) && providerArray2[index].CanExtend(instance)))
                        {
                            providerArray3[num5++] = providerArray2[index];
                        }
                        index++;
                    }
                }
                else if (num > 0)
                {
                    IEnumerator enumerator = components.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        IExtenderProvider current = enumerator.Current as IExtenderProvider;
                        if ((current != null) && (((index < num3) && ((num2 & (((ulong) 1L) << index)) != 0L)) || ((index >= num3) && current.CanExtend(instance))))
                        {
                            providerArray3[num5++] = current;
                        }
                        index++;
                    }
                }
                providerArray2 = providerArray3;
            }
            if (cache != null)
            {
                cache[_extenderProviderKey] = providerArray2;
                cache.Remove(_extenderPropertiesKey);
            }
            return providerArray2;
        }

        public override string GetFullComponentName(object component)
        {
            IComponent component2 = component as IComponent;
            if (component2 != null)
            {
                INestedSite site = component2.Site as INestedSite;
                if (site != null)
                {
                    return site.FullName;
                }
            }
            return TypeDescriptor.GetComponentName(component);
        }

        internal Type[] GetPopulatedTypes(Module module)
        {
            ArrayList list = new ArrayList();
            foreach (DictionaryEntry entry in this._typeData)
            {
                Type key = (Type) entry.Key;
                ReflectedTypeData data = (ReflectedTypeData) entry.Value;
                if ((key.Module == module) && data.IsPopulated)
                {
                    list.Add(key);
                }
            }
            return (Type[]) list.ToArray(typeof(Type));
        }

        internal PropertyDescriptorCollection GetProperties(Type type)
        {
            return this.GetTypeData(type, true).GetProperties();
        }

        internal object GetPropertyOwner(Type type, object instance, PropertyDescriptor pd)
        {
            return TypeDescriptor.GetAssociation(type, instance);
        }

        public override Type GetReflectionType(Type objectType, object instance)
        {
            return objectType;
        }

        private ReflectedTypeData GetTypeData(Type type, bool createIfNeeded)
        {
            ReflectedTypeData data = null;
            if (this._typeData != null)
            {
                data = (ReflectedTypeData) this._typeData[type];
                if (data != null)
                {
                    return data;
                }
            }
            lock (_internalSyncObject)
            {
                if (this._typeData != null)
                {
                    data = (ReflectedTypeData) this._typeData[type];
                }
                if ((data != null) || !createIfNeeded)
                {
                    return data;
                }
                data = new ReflectedTypeData(type);
                if (this._typeData == null)
                {
                    this._typeData = new Hashtable();
                }
                this._typeData[type] = data;
            }
            return data;
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            return null;
        }

        private static Type GetTypeFromName(string typeName)
        {
            Type type = Type.GetType(typeName);
            if (type == null)
            {
                int index = typeName.IndexOf(',');
                if (index != -1)
                {
                    type = Type.GetType(typeName.Substring(0, index));
                }
            }
            return type;
        }

        internal bool IsPopulated(Type type)
        {
            ReflectedTypeData typeData = this.GetTypeData(type, false);
            return ((typeData != null) && typeData.IsPopulated);
        }

        internal static Attribute[] ReflectGetAttributes(MemberInfo member)
        {
            if (_attributeCache == null)
            {
                lock (_internalSyncObject)
                {
                    if (_attributeCache == null)
                    {
                        _attributeCache = new Hashtable();
                    }
                }
            }
            Attribute[] array = (Attribute[]) _attributeCache[member];
            if (array == null)
            {
                lock (_internalSyncObject)
                {
                    array = (Attribute[]) _attributeCache[member];
                    if (array == null)
                    {
                        object[] customAttributes = member.GetCustomAttributes(typeof(Attribute), false);
                        array = new Attribute[customAttributes.Length];
                        customAttributes.CopyTo(array, 0);
                        _attributeCache[member] = array;
                    }
                }
            }
            return array;
        }

        private static Attribute[] ReflectGetAttributes(Type type)
        {
            if (_attributeCache == null)
            {
                lock (_internalSyncObject)
                {
                    if (_attributeCache == null)
                    {
                        _attributeCache = new Hashtable();
                    }
                }
            }
            Attribute[] array = (Attribute[]) _attributeCache[type];
            if (array == null)
            {
                lock (_internalSyncObject)
                {
                    array = (Attribute[]) _attributeCache[type];
                    if (array == null)
                    {
                        object[] customAttributes = type.GetCustomAttributes(typeof(Attribute), false);
                        array = new Attribute[customAttributes.Length];
                        customAttributes.CopyTo(array, 0);
                        _attributeCache[type] = array;
                    }
                }
            }
            return array;
        }

        private static EventDescriptor[] ReflectGetEvents(Type type)
        {
            if (_eventCache == null)
            {
                lock (_internalSyncObject)
                {
                    if (_eventCache == null)
                    {
                        _eventCache = new Hashtable();
                    }
                }
            }
            EventDescriptor[] sourceArray = (EventDescriptor[]) _eventCache[type];
            if (sourceArray == null)
            {
                lock (_internalSyncObject)
                {
                    sourceArray = (EventDescriptor[]) _eventCache[type];
                    if (sourceArray != null)
                    {
                        return sourceArray;
                    }
                    BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
                    EventInfo[] events = type.GetEvents(bindingAttr);
                    sourceArray = new EventDescriptor[events.Length];
                    int length = 0;
                    for (int i = 0; i < events.Length; i++)
                    {
                        EventInfo eventInfo = events[i];
                        if ((eventInfo.DeclaringType.IsPublic || eventInfo.DeclaringType.IsNestedPublic) || (eventInfo.DeclaringType.Assembly != typeof(ReflectTypeDescriptionProvider).Assembly))
                        {
                            MethodInfo addMethod = eventInfo.GetAddMethod();
                            MethodInfo removeMethod = eventInfo.GetRemoveMethod();
                            if ((addMethod != null) && (removeMethod != null))
                            {
                                sourceArray[length++] = new ReflectEventDescriptor(type, eventInfo);
                            }
                        }
                    }
                    if (length != sourceArray.Length)
                    {
                        EventDescriptor[] destinationArray = new EventDescriptor[length];
                        Array.Copy(sourceArray, 0, destinationArray, 0, length);
                        sourceArray = destinationArray;
                    }
                    _eventCache[type] = sourceArray;
                }
            }
            return sourceArray;
        }

        private static PropertyDescriptor[] ReflectGetExtendedProperties(IExtenderProvider provider)
        {
            PropertyDescriptor[] descriptorArray;
            IDictionary cache = TypeDescriptor.GetCache(provider);
            if (cache != null)
            {
                descriptorArray = cache[_extenderProviderPropertiesKey] as PropertyDescriptor[];
                if (descriptorArray != null)
                {
                    return descriptorArray;
                }
            }
            if (_extendedPropertyCache == null)
            {
                lock (_internalSyncObject)
                {
                    if (_extendedPropertyCache == null)
                    {
                        _extendedPropertyCache = new Hashtable();
                    }
                }
            }
            Type componentType = provider.GetType();
            ReflectPropertyDescriptor[] array = (ReflectPropertyDescriptor[]) _extendedPropertyCache[componentType];
            if (array == null)
            {
                lock (_internalSyncObject)
                {
                    array = (ReflectPropertyDescriptor[]) _extendedPropertyCache[componentType];
                    if (array == null)
                    {
                        AttributeCollection attributes = TypeDescriptor.GetAttributes(componentType);
                        ArrayList list = new ArrayList(attributes.Count);
                        foreach (Attribute attribute in attributes)
                        {
                            ProvidePropertyAttribute attribute2 = attribute as ProvidePropertyAttribute;
                            if (attribute2 != null)
                            {
                                Type typeFromName = GetTypeFromName(attribute2.ReceiverTypeName);
                                if (typeFromName != null)
                                {
                                    MethodInfo method = componentType.GetMethod("Get" + attribute2.PropertyName, new Type[] { typeFromName });
                                    if (((method != null) && !method.IsStatic) && method.IsPublic)
                                    {
                                        MethodInfo setMethod = componentType.GetMethod("Set" + attribute2.PropertyName, new Type[] { typeFromName, method.ReturnType });
                                        if ((setMethod != null) && (setMethod.IsStatic || !setMethod.IsPublic))
                                        {
                                            setMethod = null;
                                        }
                                        list.Add(new ReflectPropertyDescriptor(componentType, attribute2.PropertyName, method.ReturnType, typeFromName, method, setMethod, null));
                                    }
                                }
                            }
                        }
                        array = new ReflectPropertyDescriptor[list.Count];
                        list.CopyTo(array, 0);
                        _extendedPropertyCache[componentType] = array;
                    }
                }
            }
            descriptorArray = new PropertyDescriptor[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                Attribute[] attributeArray = null;
                IComponent component = provider as IComponent;
                if ((component == null) || (component.Site == null))
                {
                    attributeArray = new Attribute[] { DesignOnlyAttribute.Yes };
                }
                ReflectPropertyDescriptor extenderInfo = array[i];
                descriptorArray[i] = new ExtendedPropertyDescriptor(extenderInfo, extenderInfo.ExtenderGetReceiverType(), provider, attributeArray);
            }
            if (cache != null)
            {
                cache[_extenderProviderPropertiesKey] = descriptorArray;
            }
            return descriptorArray;
        }

        private static PropertyDescriptor[] ReflectGetProperties(Type type)
        {
            if (_propertyCache == null)
            {
                lock (_internalSyncObject)
                {
                    if (_propertyCache == null)
                    {
                        _propertyCache = new Hashtable();
                    }
                }
            }
            PropertyDescriptor[] sourceArray = (PropertyDescriptor[]) _propertyCache[type];
            if (sourceArray == null)
            {
                lock (_internalSyncObject)
                {
                    sourceArray = (PropertyDescriptor[]) _propertyCache[type];
                    if (sourceArray != null)
                    {
                        return sourceArray;
                    }
                    BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
                    PropertyInfo[] properties = type.GetProperties(bindingAttr);
                    sourceArray = new PropertyDescriptor[properties.Length];
                    int length = 0;
                    for (int i = 0; i < properties.Length; i++)
                    {
                        PropertyInfo propInfo = properties[i];
                        if (propInfo.GetIndexParameters().Length <= 0)
                        {
                            MethodInfo getMethod = propInfo.GetGetMethod();
                            MethodInfo setMethod = propInfo.GetSetMethod();
                            string name = propInfo.Name;
                            if (getMethod != null)
                            {
                                sourceArray[length++] = new ReflectPropertyDescriptor(type, name, propInfo.PropertyType, propInfo, getMethod, setMethod, null);
                            }
                        }
                    }
                    if (length != sourceArray.Length)
                    {
                        PropertyDescriptor[] destinationArray = new PropertyDescriptor[length];
                        Array.Copy(sourceArray, 0, destinationArray, 0, length);
                        sourceArray = destinationArray;
                    }
                    _propertyCache[type] = sourceArray;
                }
            }
            return sourceArray;
        }

        internal void Refresh(Type type)
        {
            ReflectedTypeData typeData = this.GetTypeData(type, false);
            if (typeData != null)
            {
                typeData.Refresh();
            }
        }

        private static object SearchIntrinsicTable(Hashtable table, Type callingType)
        {
            object obj2 = null;
            lock (table)
            {
                for (Type type = callingType; (type != null) && (type != typeof(object)); type = type.BaseType)
                {
                    obj2 = table[type];
                    string typeName = obj2 as string;
                    if (typeName != null)
                    {
                        obj2 = Type.GetType(typeName);
                        if (obj2 != null)
                        {
                            table[type] = obj2;
                        }
                    }
                    if (obj2 != null)
                    {
                        break;
                    }
                }
                if (obj2 == null)
                {
                    foreach (DictionaryEntry entry in table)
                    {
                        Type key = entry.Key as Type;
                        if (((key != null) && key.IsInterface) && key.IsAssignableFrom(callingType))
                        {
                            obj2 = entry.Value;
                            string str2 = obj2 as string;
                            if (str2 != null)
                            {
                                obj2 = Type.GetType(str2);
                                if (obj2 != null)
                                {
                                    table[callingType] = obj2;
                                }
                            }
                            if (obj2 != null)
                            {
                                break;
                            }
                        }
                    }
                }
                if (obj2 == null)
                {
                    if (callingType.IsGenericType && (callingType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                    {
                        obj2 = table[_intrinsicNullableKey];
                    }
                    else if (callingType.IsInterface)
                    {
                        obj2 = table[_intrinsicReferenceKey];
                    }
                }
                if (obj2 == null)
                {
                    obj2 = table[typeof(object)];
                }
                Type objectType = obj2 as Type;
                if (objectType != null)
                {
                    obj2 = CreateInstance(objectType, callingType);
                    if (objectType.GetConstructor(_typeConstructor) == null)
                    {
                        table[callingType] = obj2;
                    }
                }
            }
            return obj2;
        }

        internal static Guid ExtenderProviderKey
        {
            get
            {
                return _extenderProviderKey;
            }
        }

        private static Hashtable IntrinsicTypeConverters
        {
            get
            {
                if (_intrinsicTypeConverters == null)
                {
                    Hashtable hashtable = new Hashtable();
                    hashtable[typeof(bool)] = typeof(BooleanConverter);
                    hashtable[typeof(byte)] = typeof(ByteConverter);
                    hashtable[typeof(sbyte)] = typeof(SByteConverter);
                    hashtable[typeof(char)] = typeof(CharConverter);
                    hashtable[typeof(double)] = typeof(DoubleConverter);
                    hashtable[typeof(string)] = typeof(StringConverter);
                    hashtable[typeof(int)] = typeof(Int32Converter);
                    hashtable[typeof(short)] = typeof(Int16Converter);
                    hashtable[typeof(long)] = typeof(Int64Converter);
                    hashtable[typeof(float)] = typeof(SingleConverter);
                    hashtable[typeof(ushort)] = typeof(UInt16Converter);
                    hashtable[typeof(uint)] = typeof(UInt32Converter);
                    hashtable[typeof(ulong)] = typeof(UInt64Converter);
                    hashtable[typeof(object)] = typeof(TypeConverter);
                    hashtable[typeof(void)] = typeof(TypeConverter);
                    hashtable[typeof(CultureInfo)] = typeof(CultureInfoConverter);
                    hashtable[typeof(DateTime)] = typeof(DateTimeConverter);
                    hashtable[typeof(DateTimeOffset)] = typeof(DateTimeOffsetConverter);
                    hashtable[typeof(decimal)] = typeof(DecimalConverter);
                    hashtable[typeof(TimeSpan)] = typeof(TimeSpanConverter);
                    hashtable[typeof(Guid)] = typeof(GuidConverter);
                    hashtable[typeof(Array)] = typeof(ArrayConverter);
                    hashtable[typeof(ICollection)] = typeof(CollectionConverter);
                    hashtable[typeof(Enum)] = typeof(EnumConverter);
                    hashtable[_intrinsicReferenceKey] = typeof(ReferenceConverter);
                    hashtable[_intrinsicNullableKey] = typeof(NullableConverter);
                    _intrinsicTypeConverters = hashtable;
                }
                return _intrinsicTypeConverters;
            }
        }

        private class ReflectedTypeData
        {
            private AttributeCollection _attributes;
            private TypeConverter _converter;
            private int _editorCount;
            private object[] _editors;
            private Type[] _editorTypes;
            private EventDescriptorCollection _events;
            private PropertyDescriptorCollection _properties;
            private Type _type;

            internal ReflectedTypeData(Type type)
            {
                this._type = type;
            }

            internal AttributeCollection GetAttributes()
            {
                if (this._attributes == null)
                {
                    Attribute[] sourceArray = ReflectTypeDescriptionProvider.ReflectGetAttributes(this._type);
                    for (Type type = this._type.BaseType; (type != null) && (type != typeof(object)); type = type.BaseType)
                    {
                        Attribute[] attributeArray2 = ReflectTypeDescriptionProvider.ReflectGetAttributes(type);
                        Attribute[] destinationArray = new Attribute[sourceArray.Length + attributeArray2.Length];
                        Array.Copy(sourceArray, 0, destinationArray, 0, sourceArray.Length);
                        Array.Copy(attributeArray2, 0, destinationArray, sourceArray.Length, attributeArray2.Length);
                        sourceArray = destinationArray;
                    }
                    int length = sourceArray.Length;
                    foreach (Type type2 in this._type.GetInterfaces())
                    {
                        if ((type2.Attributes & TypeAttributes.NestedPrivate) != TypeAttributes.AnsiClass)
                        {
                            AttributeCollection attributes = TypeDescriptor.GetAttributes(type2);
                            if (attributes.Count > 0)
                            {
                                Attribute[] attributeArray4 = new Attribute[sourceArray.Length + attributes.Count];
                                Array.Copy(sourceArray, 0, attributeArray4, 0, sourceArray.Length);
                                attributes.CopyTo(attributeArray4, sourceArray.Length);
                                sourceArray = attributeArray4;
                            }
                        }
                    }
                    OrderedDictionary dictionary = new OrderedDictionary(sourceArray.Length);
                    for (int i = 0; i < sourceArray.Length; i++)
                    {
                        bool flag = true;
                        if (i >= length)
                        {
                            for (int j = 0; j < ReflectTypeDescriptionProvider._skipInterfaceAttributeList.Length; j++)
                            {
                                if (ReflectTypeDescriptionProvider._skipInterfaceAttributeList[j].IsInstanceOfType(sourceArray[i]))
                                {
                                    flag = false;
                                    break;
                                }
                            }
                        }
                        if (flag && !dictionary.Contains(sourceArray[i].TypeId))
                        {
                            dictionary[sourceArray[i].TypeId] = sourceArray[i];
                        }
                    }
                    sourceArray = new Attribute[dictionary.Count];
                    dictionary.Values.CopyTo(sourceArray, 0);
                    this._attributes = new AttributeCollection(sourceArray);
                }
                return this._attributes;
            }

            internal string GetClassName(object instance)
            {
                return this._type.FullName;
            }

            internal string GetComponentName(object instance)
            {
                IComponent component = instance as IComponent;
                if (component != null)
                {
                    ISite site = component.Site;
                    if (site != null)
                    {
                        INestedSite site2 = site as INestedSite;
                        if (site2 != null)
                        {
                            return site2.FullName;
                        }
                        return site.Name;
                    }
                }
                return null;
            }

            internal TypeConverter GetConverter(object instance)
            {
                TypeConverterAttribute attribute = null;
                if (instance != null)
                {
                    attribute = (TypeConverterAttribute) TypeDescriptor.GetAttributes(this._type)[typeof(TypeConverterAttribute)];
                    TypeConverterAttribute attribute2 = (TypeConverterAttribute) TypeDescriptor.GetAttributes(instance)[typeof(TypeConverterAttribute)];
                    if (attribute != attribute2)
                    {
                        Type typeFromName = this.GetTypeFromName(attribute2.ConverterTypeName);
                        if ((typeFromName != null) && typeof(TypeConverter).IsAssignableFrom(typeFromName))
                        {
                            try
                            {
                                IntSecurity.FullReflection.Assert();
                                return (TypeConverter) ReflectTypeDescriptionProvider.CreateInstance(typeFromName, this._type);
                            }
                            finally
                            {
                                CodeAccessPermission.RevertAssert();
                            }
                        }
                    }
                }
                if (this._converter == null)
                {
                    if (attribute == null)
                    {
                        attribute = (TypeConverterAttribute) TypeDescriptor.GetAttributes(this._type)[typeof(TypeConverterAttribute)];
                    }
                    if (attribute != null)
                    {
                        Type c = this.GetTypeFromName(attribute.ConverterTypeName);
                        if ((c != null) && typeof(TypeConverter).IsAssignableFrom(c))
                        {
                            try
                            {
                                IntSecurity.FullReflection.Assert();
                                this._converter = (TypeConverter) ReflectTypeDescriptionProvider.CreateInstance(c, this._type);
                            }
                            finally
                            {
                                CodeAccessPermission.RevertAssert();
                            }
                        }
                    }
                    if (this._converter == null)
                    {
                        this._converter = (TypeConverter) ReflectTypeDescriptionProvider.SearchIntrinsicTable(ReflectTypeDescriptionProvider.IntrinsicTypeConverters, this._type);
                    }
                }
                return this._converter;
            }

            internal EventDescriptor GetDefaultEvent(object instance)
            {
                AttributeCollection attributes;
                if (instance != null)
                {
                    attributes = TypeDescriptor.GetAttributes(instance);
                }
                else
                {
                    attributes = TypeDescriptor.GetAttributes(this._type);
                }
                DefaultEventAttribute attribute = (DefaultEventAttribute) attributes[typeof(DefaultEventAttribute)];
                if ((attribute == null) || (attribute.Name == null))
                {
                    return null;
                }
                if (instance != null)
                {
                    return TypeDescriptor.GetEvents(instance)[attribute.Name];
                }
                return TypeDescriptor.GetEvents(this._type)[attribute.Name];
            }

            internal PropertyDescriptor GetDefaultProperty(object instance)
            {
                AttributeCollection attributes;
                if (instance != null)
                {
                    attributes = TypeDescriptor.GetAttributes(instance);
                }
                else
                {
                    attributes = TypeDescriptor.GetAttributes(this._type);
                }
                DefaultPropertyAttribute attribute = (DefaultPropertyAttribute) attributes[typeof(DefaultPropertyAttribute)];
                if ((attribute == null) || (attribute.Name == null))
                {
                    return null;
                }
                if (instance != null)
                {
                    return TypeDescriptor.GetProperties(instance)[attribute.Name];
                }
                return TypeDescriptor.GetProperties(this._type)[attribute.Name];
            }

            internal object GetEditor(object instance, Type editorBaseType)
            {
                EditorAttribute editorAttribute;
                if (instance != null)
                {
                    editorAttribute = GetEditorAttribute(TypeDescriptor.GetAttributes(this._type), editorBaseType);
                    EditorAttribute attribute2 = GetEditorAttribute(TypeDescriptor.GetAttributes(instance), editorBaseType);
                    if (editorAttribute != attribute2)
                    {
                        Type typeFromName = this.GetTypeFromName(attribute2.EditorTypeName);
                        if ((typeFromName != null) && editorBaseType.IsAssignableFrom(typeFromName))
                        {
                            return ReflectTypeDescriptionProvider.CreateInstance(typeFromName, this._type);
                        }
                    }
                }
                lock (this)
                {
                    for (int i = 0; i < this._editorCount; i++)
                    {
                        if (this._editorTypes[i] == editorBaseType)
                        {
                            return this._editors[i];
                        }
                    }
                }
                object o = null;
                editorAttribute = GetEditorAttribute(TypeDescriptor.GetAttributes(this._type), editorBaseType);
                if (editorAttribute != null)
                {
                    Type c = this.GetTypeFromName(editorAttribute.EditorTypeName);
                    if ((c != null) && editorBaseType.IsAssignableFrom(c))
                    {
                        o = ReflectTypeDescriptionProvider.CreateInstance(c, this._type);
                    }
                }
                if (o == null)
                {
                    Hashtable editorTable = ReflectTypeDescriptionProvider.GetEditorTable(editorBaseType);
                    if (editorTable != null)
                    {
                        o = ReflectTypeDescriptionProvider.SearchIntrinsicTable(editorTable, this._type);
                    }
                    if ((o != null) && !editorBaseType.IsInstanceOfType(o))
                    {
                        o = null;
                    }
                }
                if (o != null)
                {
                    lock (this)
                    {
                        if ((this._editorTypes != null) && (this._editorTypes.Length != this._editorCount))
                        {
                            return o;
                        }
                        int num2 = (this._editorTypes == null) ? 4 : (this._editorTypes.Length * 2);
                        Type[] array = new Type[num2];
                        object[] objArray = new object[num2];
                        if (this._editorTypes != null)
                        {
                            this._editorTypes.CopyTo(array, 0);
                            this._editors.CopyTo(objArray, 0);
                        }
                        this._editorTypes = array;
                        this._editors = objArray;
                        this._editorTypes[this._editorCount] = editorBaseType;
                        this._editors[this._editorCount++] = o;
                    }
                }
                return o;
            }

            private static EditorAttribute GetEditorAttribute(AttributeCollection attributes, Type editorBaseType)
            {
                foreach (Attribute attribute in attributes)
                {
                    EditorAttribute attribute2 = attribute as EditorAttribute;
                    if (attribute2 != null)
                    {
                        Type type = Type.GetType(attribute2.EditorBaseTypeName);
                        if ((type != null) && (type == editorBaseType))
                        {
                            return attribute2;
                        }
                    }
                }
                return null;
            }

            internal EventDescriptorCollection GetEvents()
            {
                if (this._events == null)
                {
                    Dictionary<string, EventDescriptor> dictionary = new Dictionary<string, EventDescriptor>(0x10);
                    Type baseType = this._type;
                    Type type2 = typeof(object);
                    do
                    {
                        foreach (EventDescriptor descriptor in ReflectTypeDescriptionProvider.ReflectGetEvents(baseType))
                        {
                            if (!dictionary.ContainsKey(descriptor.Name))
                            {
                                dictionary.Add(descriptor.Name, descriptor);
                            }
                        }
                        baseType = baseType.BaseType;
                    }
                    while ((baseType != null) && (baseType != type2));
                    EventDescriptor[] array = new EventDescriptor[dictionary.Count];
                    dictionary.Values.CopyTo(array, 0);
                    this._events = new EventDescriptorCollection(array, true);
                }
                return this._events;
            }

            internal PropertyDescriptorCollection GetProperties()
            {
                if (this._properties == null)
                {
                    Dictionary<string, PropertyDescriptor> dictionary = new Dictionary<string, PropertyDescriptor>(10);
                    Type baseType = this._type;
                    Type type2 = typeof(object);
                    do
                    {
                        foreach (PropertyDescriptor descriptor in ReflectTypeDescriptionProvider.ReflectGetProperties(baseType))
                        {
                            if (!dictionary.ContainsKey(descriptor.Name))
                            {
                                dictionary.Add(descriptor.Name, descriptor);
                            }
                        }
                        baseType = baseType.BaseType;
                    }
                    while ((baseType != null) && (baseType != type2));
                    PropertyDescriptor[] array = new PropertyDescriptor[dictionary.Count];
                    dictionary.Values.CopyTo(array, 0);
                    this._properties = new PropertyDescriptorCollection(array, true);
                }
                return this._properties;
            }

            private Type GetTypeFromName(string typeName)
            {
                if ((typeName == null) || (typeName.Length == 0))
                {
                    return null;
                }
                int index = typeName.IndexOf(',');
                Type type = null;
                if (index == -1)
                {
                    type = this._type.Assembly.GetType(typeName);
                }
                if (type == null)
                {
                    type = Type.GetType(typeName);
                }
                if ((type == null) && (index != -1))
                {
                    type = Type.GetType(typeName.Substring(0, index));
                }
                return type;
            }

            internal void Refresh()
            {
                this._attributes = null;
                this._events = null;
                this._properties = null;
                this._converter = null;
                this._editors = null;
                this._editorTypes = null;
                this._editorCount = 0;
            }

            internal bool IsPopulated
            {
                get
                {
                    return (((this._attributes != null) | (this._events != null)) | (this._properties != null));
                }
            }
        }
    }
}

