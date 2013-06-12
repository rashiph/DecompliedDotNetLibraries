namespace System.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public sealed class TypeDescriptor
    {
        private static WeakHashtable _associationTable;
        private static int _collisionIndex;
        private static Hashtable _defaultProviders = new Hashtable();
        private static object _internalSyncObject = new object();
        private static int _metadataVersion;
        private static readonly Guid[] _pipelineAttributeFilterKeys = new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        private static readonly Guid[] _pipelineFilterKeys = new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        private static readonly Guid[] _pipelineInitializeKeys = new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        private static readonly Guid[] _pipelineMergeKeys = new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        private static WeakHashtable _providerTable = new WeakHashtable();
        private static Hashtable _providerTypeTable = new Hashtable();
        private const int PIPELINE_ATTRIBUTES = 0;
        private const int PIPELINE_EVENTS = 2;
        private const int PIPELINE_PROPERTIES = 1;
        private static BooleanSwitch TraceDescriptor = new BooleanSwitch("TypeDescriptor", "Debug TypeDescriptor.");

        public static  event RefreshEventHandler Refreshed;

        private TypeDescriptor()
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public static TypeDescriptionProvider AddAttributes(object instance, params Attribute[] attributes)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            if (attributes == null)
            {
                throw new ArgumentNullException("attributes");
            }
            TypeDescriptionProvider provider = new AttributeProvider(GetProvider(instance), attributes);
            AddProvider(provider, instance);
            return provider;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public static TypeDescriptionProvider AddAttributes(Type type, params Attribute[] attributes)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (attributes == null)
            {
                throw new ArgumentNullException("attributes");
            }
            TypeDescriptionProvider provider = new AttributeProvider(GetProvider(type), attributes);
            AddProvider(provider, type);
            return provider;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void AddEditorTable(Type editorBaseType, Hashtable table)
        {
            ReflectTypeDescriptionProvider.AddEditorTable(editorBaseType, table);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public static void AddProvider(TypeDescriptionProvider provider, object instance)
        {
            bool flag;
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            lock (_providerTable)
            {
                flag = _providerTable.ContainsKey(instance);
                TypeDescriptionNode node = NodeFor(instance, true);
                TypeDescriptionNode node2 = new TypeDescriptionNode(provider) {
                    Next = node
                };
                _providerTable.SetWeak(instance, node2);
                _providerTypeTable.Clear();
            }
            if (flag)
            {
                Refresh(instance, false);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public static void AddProvider(TypeDescriptionProvider provider, Type type)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            lock (_providerTable)
            {
                TypeDescriptionNode node = NodeFor(type, true);
                TypeDescriptionNode node2 = new TypeDescriptionNode(provider) {
                    Next = node
                };
                _providerTable[type] = node2;
                _providerTypeTable.Clear();
            }
            Refresh(type);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void AddProviderTransparent(TypeDescriptionProvider provider, object instance)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            Type type = instance.GetType();
            PermissionSet other = new PermissionSet(PermissionState.None);
            other.AddPermission(new TypeDescriptorPermission(TypeDescriptorPermissionFlags.RestrictedRegistrationAccess));
            type.Assembly.PermissionSet.Union(other).Demand();
            AddProvider(provider, instance);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void AddProviderTransparent(TypeDescriptionProvider provider, Type type)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            PermissionSet other = new PermissionSet(PermissionState.None);
            other.AddPermission(new TypeDescriptorPermission(TypeDescriptorPermissionFlags.RestrictedRegistrationAccess));
            type.Assembly.PermissionSet.Union(other).Demand();
            AddProvider(provider, type);
        }

        private static void CheckDefaultProvider(Type type)
        {
            if (_defaultProviders == null)
            {
                lock (_internalSyncObject)
                {
                    if (_defaultProviders == null)
                    {
                        _defaultProviders = new Hashtable();
                    }
                }
            }
            if (!_defaultProviders.ContainsKey(type))
            {
                lock (_internalSyncObject)
                {
                    if (_defaultProviders.ContainsKey(type))
                    {
                        return;
                    }
                    _defaultProviders[type] = null;
                }
                object[] customAttributes = type.GetCustomAttributes(typeof(TypeDescriptionProviderAttribute), false);
                bool flag2 = false;
                for (int i = customAttributes.Length - 1; i >= 0; i--)
                {
                    TypeDescriptionProviderAttribute attribute = (TypeDescriptionProviderAttribute) customAttributes[i];
                    Type c = Type.GetType(attribute.TypeName);
                    if ((c != null) && typeof(TypeDescriptionProvider).IsAssignableFrom(c))
                    {
                        TypeDescriptionProvider provider;
                        IntSecurity.FullReflection.Assert();
                        try
                        {
                            provider = (TypeDescriptionProvider) Activator.CreateInstance(c);
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }
                        AddProvider(provider, type);
                        flag2 = true;
                    }
                }
                if (!flag2)
                {
                    Type baseType = type.BaseType;
                    if ((baseType != null) && (baseType != type))
                    {
                        CheckDefaultProvider(baseType);
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public static void CreateAssociation(object primary, object secondary)
        {
            bool flag3;
            IList list2;
            if (primary == null)
            {
                throw new ArgumentNullException("primary");
            }
            if (secondary == null)
            {
                throw new ArgumentNullException("secondary");
            }
            if (primary == secondary)
            {
                throw new ArgumentException(SR.GetString("TypeDescriptorSameAssociation"));
            }
            if (_associationTable == null)
            {
                lock (_internalSyncObject)
                {
                    if (_associationTable == null)
                    {
                        _associationTable = new WeakHashtable();
                    }
                }
            }
            IList list = (IList) _associationTable[primary];
            if (list == null)
            {
                lock (_associationTable)
                {
                    list = (IList) _associationTable[primary];
                    if (list == null)
                    {
                        list = new ArrayList(4);
                        _associationTable.SetWeak(primary, list);
                    }
                    goto Label_0103;
                }
            }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                WeakReference reference = (WeakReference) list[i];
                if (reference.IsAlive && (reference.Target == secondary))
                {
                    throw new ArgumentException(SR.GetString("TypeDescriptorAlreadyAssociated"));
                }
            }
        Label_0103:
            flag3 = false;
            try
            {
                Monitor.Enter(list2 = list, ref flag3);
                list.Add(new WeakReference(secondary));
            }
            finally
            {
                if (flag3)
                {
                    Monitor.Exit(list2);
                }
            }
        }

        public static IDesigner CreateDesigner(IComponent component, Type designerBaseType)
        {
            Type type = null;
            IDesigner designer = null;
            AttributeCollection attributes = GetAttributes(component);
            for (int i = 0; i < attributes.Count; i++)
            {
                DesignerAttribute attribute = attributes[i] as DesignerAttribute;
                if (attribute != null)
                {
                    Type type2 = Type.GetType(attribute.DesignerBaseTypeName);
                    if ((type2 != null) && (type2 == designerBaseType))
                    {
                        ISite site = component.Site;
                        bool flag = false;
                        if (site != null)
                        {
                            ITypeResolutionService service = (ITypeResolutionService) site.GetService(typeof(ITypeResolutionService));
                            if (service != null)
                            {
                                flag = true;
                                type = service.GetType(attribute.DesignerTypeName);
                            }
                        }
                        if (!flag)
                        {
                            type = Type.GetType(attribute.DesignerTypeName);
                        }
                        if (type != null)
                        {
                            break;
                        }
                    }
                }
            }
            if (type != null)
            {
                designer = (IDesigner) SecurityUtils.SecureCreateInstance(type, null, true);
            }
            return designer;
        }

        [ReflectionPermission(SecurityAction.LinkDemand, Flags=ReflectionPermissionFlag.MemberAccess)]
        public static EventDescriptor CreateEvent(Type componentType, EventDescriptor oldEventDescriptor, params Attribute[] attributes)
        {
            return new ReflectEventDescriptor(componentType, oldEventDescriptor, attributes);
        }

        [ReflectionPermission(SecurityAction.LinkDemand, Flags=ReflectionPermissionFlag.MemberAccess)]
        public static EventDescriptor CreateEvent(Type componentType, string name, Type type, params Attribute[] attributes)
        {
            return new ReflectEventDescriptor(componentType, name, type, attributes);
        }

        public static object CreateInstance(IServiceProvider provider, Type objectType, Type[] argTypes, object[] args)
        {
            if (objectType == null)
            {
                throw new ArgumentNullException("objectType");
            }
            if (argTypes != null)
            {
                if (args == null)
                {
                    throw new ArgumentNullException("args");
                }
                if (argTypes.Length != args.Length)
                {
                    throw new ArgumentException(SR.GetString("TypeDescriptorArgsCountMismatch"));
                }
            }
            object obj2 = null;
            if (provider != null)
            {
                TypeDescriptionProvider service = provider.GetService(typeof(TypeDescriptionProvider)) as TypeDescriptionProvider;
                if (service != null)
                {
                    obj2 = service.CreateInstance(provider, objectType, argTypes, args);
                }
            }
            if (obj2 == null)
            {
                obj2 = NodeFor(objectType).CreateInstance(provider, objectType, argTypes, args);
            }
            return obj2;
        }

        [ReflectionPermission(SecurityAction.LinkDemand, Flags=ReflectionPermissionFlag.MemberAccess)]
        public static PropertyDescriptor CreateProperty(Type componentType, PropertyDescriptor oldPropertyDescriptor, params Attribute[] attributes)
        {
            if (componentType == oldPropertyDescriptor.ComponentType)
            {
                ExtenderProvidedPropertyAttribute attribute = (ExtenderProvidedPropertyAttribute) oldPropertyDescriptor.Attributes[typeof(ExtenderProvidedPropertyAttribute)];
                if (attribute.ExtenderProperty is ReflectPropertyDescriptor)
                {
                    return new ExtendedPropertyDescriptor(oldPropertyDescriptor, attributes);
                }
            }
            return new ReflectPropertyDescriptor(componentType, oldPropertyDescriptor, attributes);
        }

        [ReflectionPermission(SecurityAction.LinkDemand, Flags=ReflectionPermissionFlag.MemberAccess)]
        public static PropertyDescriptor CreateProperty(Type componentType, string name, Type type, params Attribute[] attributes)
        {
            return new ReflectPropertyDescriptor(componentType, name, type, attributes);
        }

        [Conditional("DEBUG")]
        private static void DebugValidate(AttributeCollection attributes, AttributeCollection debugAttributes)
        {
        }

        [Conditional("DEBUG")]
        private static void DebugValidate(AttributeCollection attributes, Type type)
        {
        }

        [Conditional("DEBUG")]
        private static void DebugValidate(TypeConverter converter, Type type)
        {
        }

        [Conditional("DEBUG")]
        private static void DebugValidate(AttributeCollection attributes, object instance, bool noCustomTypeDesc)
        {
        }

        [Conditional("DEBUG")]
        private static void DebugValidate(EventDescriptorCollection events, Type type, Attribute[] attributes)
        {
        }

        [Conditional("DEBUG")]
        private static void DebugValidate(PropertyDescriptorCollection properties, Type type, Attribute[] attributes)
        {
        }

        [Conditional("DEBUG")]
        private static void DebugValidate(TypeConverter converter, object instance, bool noCustomTypeDesc)
        {
        }

        [Conditional("DEBUG")]
        private static void DebugValidate(Type type, AttributeCollection attributes, AttributeCollection debugAttributes)
        {
        }

        [Conditional("DEBUG")]
        private static void DebugValidate(EventDescriptorCollection events, object instance, Attribute[] attributes, bool noCustomTypeDesc)
        {
        }

        [Conditional("DEBUG")]
        private static void DebugValidate(PropertyDescriptorCollection properties, object instance, Attribute[] attributes, bool noCustomTypeDesc)
        {
        }

        private static ArrayList FilterMembers(IList members, Attribute[] attributes)
        {
            ArrayList list = null;
            int count = members.Count;
            for (int i = 0; i < count; i++)
            {
                bool flag = false;
                for (int j = 0; j < attributes.Length; j++)
                {
                    if (ShouldHideMember((MemberDescriptor) members[i], attributes[j]))
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    if (list == null)
                    {
                        list = new ArrayList(count);
                        for (int k = 0; k < i; k++)
                        {
                            list.Add(members[k]);
                        }
                    }
                }
                else if (list != null)
                {
                    list.Add(members[i]);
                }
            }
            return list;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static object GetAssociation(Type type, object primary)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (primary == null)
            {
                throw new ArgumentNullException("primary");
            }
            object obj2 = primary;
            if (!type.IsInstanceOfType(primary))
            {
                Hashtable hashtable = _associationTable;
                if (hashtable != null)
                {
                    IList list = (IList) hashtable[primary];
                    if (list != null)
                    {
                        lock (list)
                        {
                            for (int i = list.Count - 1; i >= 0; i--)
                            {
                                WeakReference reference = (WeakReference) list[i];
                                object target = reference.Target;
                                if (target == null)
                                {
                                    list.RemoveAt(i);
                                }
                                else if (type.IsInstanceOfType(target))
                                {
                                    obj2 = target;
                                }
                            }
                        }
                    }
                }
                if (obj2 == primary)
                {
                    IComponent component = primary as IComponent;
                    if (component != null)
                    {
                        ISite site = component.Site;
                        if ((site != null) && site.DesignMode)
                        {
                            IDesignerHost service = site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                            if (service != null)
                            {
                                object designer = service.GetDesigner(component);
                                if ((designer != null) && type.IsInstanceOfType(designer))
                                {
                                    obj2 = designer;
                                }
                            }
                        }
                    }
                }
            }
            return obj2;
        }

        public static AttributeCollection GetAttributes(object component)
        {
            return GetAttributes(component, false);
        }

        public static AttributeCollection GetAttributes(Type componentType)
        {
            if (componentType == null)
            {
                return new AttributeCollection(null);
            }
            return GetDescriptor(componentType, "componentType").GetAttributes();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static AttributeCollection GetAttributes(object component, bool noCustomTypeDesc)
        {
            if (component == null)
            {
                return new AttributeCollection(null);
            }
            ICollection primary = GetDescriptor(component, noCustomTypeDesc).GetAttributes();
            if (component is ICustomTypeDescriptor)
            {
                if (noCustomTypeDesc)
                {
                    ICustomTypeDescriptor extendedDescriptor = GetExtendedDescriptor(component);
                    if (extendedDescriptor != null)
                    {
                        ICollection secondary = extendedDescriptor.GetAttributes();
                        primary = PipelineMerge(0, primary, secondary, component, null);
                    }
                }
                else
                {
                    primary = PipelineFilter(0, primary, component, null);
                }
            }
            else
            {
                IDictionary cache = GetCache(component);
                primary = PipelineInitialize(0, primary, cache);
                ICustomTypeDescriptor descriptor3 = GetExtendedDescriptor(component);
                if (descriptor3 != null)
                {
                    ICollection is4 = descriptor3.GetAttributes();
                    primary = PipelineMerge(0, primary, is4, component, cache);
                }
                primary = PipelineFilter(0, primary, component, cache);
            }
            AttributeCollection attributes = primary as AttributeCollection;
            if (attributes == null)
            {
                Attribute[] array = new Attribute[primary.Count];
                primary.CopyTo(array, 0);
                attributes = new AttributeCollection(array);
            }
            return attributes;
        }

        internal static IDictionary GetCache(object instance)
        {
            return NodeFor(instance).GetCache(instance);
        }

        public static string GetClassName(object component)
        {
            return GetClassName(component, false);
        }

        public static string GetClassName(Type componentType)
        {
            return GetDescriptor(componentType, "componentType").GetClassName();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static string GetClassName(object component, bool noCustomTypeDesc)
        {
            return GetDescriptor(component, noCustomTypeDesc).GetClassName();
        }

        public static string GetComponentName(object component)
        {
            return GetComponentName(component, false);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static string GetComponentName(object component, bool noCustomTypeDesc)
        {
            return GetDescriptor(component, noCustomTypeDesc).GetComponentName();
        }

        public static TypeConverter GetConverter(object component)
        {
            return GetConverter(component, false);
        }

        public static TypeConverter GetConverter(Type type)
        {
            return GetDescriptor(type, "type").GetConverter();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static TypeConverter GetConverter(object component, bool noCustomTypeDesc)
        {
            return GetDescriptor(component, noCustomTypeDesc).GetConverter();
        }

        public static EventDescriptor GetDefaultEvent(object component)
        {
            return GetDefaultEvent(component, false);
        }

        public static EventDescriptor GetDefaultEvent(Type componentType)
        {
            if (componentType == null)
            {
                return null;
            }
            return GetDescriptor(componentType, "componentType").GetDefaultEvent();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static EventDescriptor GetDefaultEvent(object component, bool noCustomTypeDesc)
        {
            if (component == null)
            {
                return null;
            }
            return GetDescriptor(component, noCustomTypeDesc).GetDefaultEvent();
        }

        public static PropertyDescriptor GetDefaultProperty(object component)
        {
            return GetDefaultProperty(component, false);
        }

        public static PropertyDescriptor GetDefaultProperty(Type componentType)
        {
            if (componentType == null)
            {
                return null;
            }
            return GetDescriptor(componentType, "componentType").GetDefaultProperty();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static PropertyDescriptor GetDefaultProperty(object component, bool noCustomTypeDesc)
        {
            if (component == null)
            {
                return null;
            }
            return GetDescriptor(component, noCustomTypeDesc).GetDefaultProperty();
        }

        internal static ICustomTypeDescriptor GetDescriptor(object component, bool noCustomTypeDesc)
        {
            if (component == null)
            {
                throw new ArgumentException("component");
            }
            if (component is IUnimplemented)
            {
                throw new NotSupportedException(SR.GetString("TypeDescriptorUnsupportedRemoteObject", new object[] { component.GetType().FullName }));
            }
            ICustomTypeDescriptor typeDescriptor = NodeFor(component).GetTypeDescriptor(component);
            ICustomTypeDescriptor primary = component as ICustomTypeDescriptor;
            if (!noCustomTypeDesc && (primary != null))
            {
                typeDescriptor = new MergedTypeDescriptor(primary, typeDescriptor);
            }
            return typeDescriptor;
        }

        internal static ICustomTypeDescriptor GetDescriptor(Type type, string typeName)
        {
            if (type == null)
            {
                throw new ArgumentNullException(typeName);
            }
            return NodeFor(type).GetTypeDescriptor(type);
        }

        public static object GetEditor(object component, Type editorBaseType)
        {
            return GetEditor(component, editorBaseType, false);
        }

        public static object GetEditor(Type type, Type editorBaseType)
        {
            if (editorBaseType == null)
            {
                throw new ArgumentNullException("editorBaseType");
            }
            return GetDescriptor(type, "type").GetEditor(editorBaseType);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static object GetEditor(object component, Type editorBaseType, bool noCustomTypeDesc)
        {
            if (editorBaseType == null)
            {
                throw new ArgumentNullException("editorBaseType");
            }
            return GetDescriptor(component, noCustomTypeDesc).GetEditor(editorBaseType);
        }

        public static EventDescriptorCollection GetEvents(object component)
        {
            return GetEvents(component, null, false);
        }

        public static EventDescriptorCollection GetEvents(Type componentType)
        {
            if (componentType == null)
            {
                return new EventDescriptorCollection(null, true);
            }
            return GetDescriptor(componentType, "componentType").GetEvents();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static EventDescriptorCollection GetEvents(object component, bool noCustomTypeDesc)
        {
            return GetEvents(component, null, noCustomTypeDesc);
        }

        public static EventDescriptorCollection GetEvents(object component, Attribute[] attributes)
        {
            return GetEvents(component, attributes, false);
        }

        public static EventDescriptorCollection GetEvents(Type componentType, Attribute[] attributes)
        {
            if (componentType == null)
            {
                return new EventDescriptorCollection(null, true);
            }
            EventDescriptorCollection events = GetDescriptor(componentType, "componentType").GetEvents(attributes);
            if ((attributes != null) && (attributes.Length > 0))
            {
                ArrayList list = FilterMembers(events, attributes);
                if (list != null)
                {
                    events = new EventDescriptorCollection((EventDescriptor[]) list.ToArray(typeof(EventDescriptor)), true);
                }
            }
            return events;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static EventDescriptorCollection GetEvents(object component, Attribute[] attributes, bool noCustomTypeDesc)
        {
            ICollection events;
            if (component == null)
            {
                return new EventDescriptorCollection(null, true);
            }
            ICustomTypeDescriptor descriptor = GetDescriptor(component, noCustomTypeDesc);
            if (component is ICustomTypeDescriptor)
            {
                events = descriptor.GetEvents(attributes);
                if (noCustomTypeDesc)
                {
                    ICustomTypeDescriptor extendedDescriptor = GetExtendedDescriptor(component);
                    if (extendedDescriptor != null)
                    {
                        ICollection secondary = extendedDescriptor.GetEvents(attributes);
                        events = PipelineMerge(2, events, secondary, component, null);
                    }
                }
                else
                {
                    events = PipelineFilter(2, events, component, null);
                    events = PipelineAttributeFilter(2, events, attributes, component, null);
                }
            }
            else
            {
                IDictionary cache = GetCache(component);
                events = descriptor.GetEvents(attributes);
                events = PipelineInitialize(2, events, cache);
                ICustomTypeDescriptor descriptor3 = GetExtendedDescriptor(component);
                if (descriptor3 != null)
                {
                    ICollection is4 = descriptor3.GetEvents(attributes);
                    events = PipelineMerge(2, events, is4, component, cache);
                }
                events = PipelineFilter(2, events, component, cache);
                events = PipelineAttributeFilter(2, events, attributes, component, cache);
            }
            EventDescriptorCollection descriptors = events as EventDescriptorCollection;
            if (descriptors == null)
            {
                EventDescriptor[] array = new EventDescriptor[events.Count];
                events.CopyTo(array, 0);
                descriptors = new EventDescriptorCollection(array, true);
            }
            return descriptors;
        }

        internal static ICustomTypeDescriptor GetExtendedDescriptor(object component)
        {
            if (component == null)
            {
                throw new ArgumentException("component");
            }
            return NodeFor(component).GetExtendedTypeDescriptor(component);
        }

        private static string GetExtenderCollisionSuffix(MemberDescriptor member)
        {
            string str = null;
            ExtenderProvidedPropertyAttribute attribute = member.Attributes[typeof(ExtenderProvidedPropertyAttribute)] as ExtenderProvidedPropertyAttribute;
            if (attribute == null)
            {
                return str;
            }
            IExtenderProvider provider = attribute.Provider;
            if (provider == null)
            {
                return str;
            }
            string name = null;
            IComponent component = provider as IComponent;
            if ((component != null) && (component.Site != null))
            {
                name = component.Site.Name;
            }
            if ((name == null) || (name.Length == 0))
            {
                name = _collisionIndex++.ToString(CultureInfo.InvariantCulture);
            }
            return string.Format(CultureInfo.InvariantCulture, "_{0}", new object[] { name });
        }

        public static string GetFullComponentName(object component)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            return GetProvider(component).GetFullComponentName(component);
        }

        private static Type GetNodeForBaseType(Type searchType)
        {
            if (searchType.IsInterface)
            {
                return InterfaceType;
            }
            if (searchType == InterfaceType)
            {
                return null;
            }
            return searchType.BaseType;
        }

        public static PropertyDescriptorCollection GetProperties(object component)
        {
            return GetProperties(component, false);
        }

        public static PropertyDescriptorCollection GetProperties(Type componentType)
        {
            if (componentType == null)
            {
                return new PropertyDescriptorCollection(null, true);
            }
            return GetDescriptor(componentType, "componentType").GetProperties();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static PropertyDescriptorCollection GetProperties(object component, bool noCustomTypeDesc)
        {
            return GetPropertiesImpl(component, null, noCustomTypeDesc, true);
        }

        public static PropertyDescriptorCollection GetProperties(object component, Attribute[] attributes)
        {
            return GetProperties(component, attributes, false);
        }

        public static PropertyDescriptorCollection GetProperties(Type componentType, Attribute[] attributes)
        {
            if (componentType == null)
            {
                return new PropertyDescriptorCollection(null, true);
            }
            PropertyDescriptorCollection properties = GetDescriptor(componentType, "componentType").GetProperties(attributes);
            if ((attributes != null) && (attributes.Length > 0))
            {
                ArrayList list = FilterMembers(properties, attributes);
                if (list != null)
                {
                    properties = new PropertyDescriptorCollection((PropertyDescriptor[]) list.ToArray(typeof(PropertyDescriptor)), true);
                }
            }
            return properties;
        }

        public static PropertyDescriptorCollection GetProperties(object component, Attribute[] attributes, bool noCustomTypeDesc)
        {
            return GetPropertiesImpl(component, attributes, noCustomTypeDesc, false);
        }

        private static PropertyDescriptorCollection GetPropertiesImpl(object component, Attribute[] attributes, bool noCustomTypeDesc, bool noAttributes)
        {
            ICollection is2;
            if (component == null)
            {
                return new PropertyDescriptorCollection(null, true);
            }
            ICustomTypeDescriptor descriptor = GetDescriptor(component, noCustomTypeDesc);
            if (component is ICustomTypeDescriptor)
            {
                is2 = noAttributes ? descriptor.GetProperties() : descriptor.GetProperties(attributes);
                if (noCustomTypeDesc)
                {
                    ICustomTypeDescriptor extendedDescriptor = GetExtendedDescriptor(component);
                    if (extendedDescriptor != null)
                    {
                        ICollection secondary = noAttributes ? extendedDescriptor.GetProperties() : extendedDescriptor.GetProperties(attributes);
                        is2 = PipelineMerge(1, is2, secondary, component, null);
                    }
                }
                else
                {
                    is2 = PipelineFilter(1, is2, component, null);
                    is2 = PipelineAttributeFilter(1, is2, attributes, component, null);
                }
            }
            else
            {
                IDictionary cache = GetCache(component);
                is2 = noAttributes ? descriptor.GetProperties() : descriptor.GetProperties(attributes);
                is2 = PipelineInitialize(1, is2, cache);
                ICustomTypeDescriptor descriptor3 = GetExtendedDescriptor(component);
                if (descriptor3 != null)
                {
                    ICollection is4 = noAttributes ? descriptor3.GetProperties() : descriptor3.GetProperties(attributes);
                    is2 = PipelineMerge(1, is2, is4, component, cache);
                }
                is2 = PipelineFilter(1, is2, component, cache);
                is2 = PipelineAttributeFilter(1, is2, attributes, component, cache);
            }
            PropertyDescriptorCollection descriptors = is2 as PropertyDescriptorCollection;
            if (descriptors == null)
            {
                PropertyDescriptor[] array = new PropertyDescriptor[is2.Count];
                is2.CopyTo(array, 0);
                descriptors = new PropertyDescriptorCollection(array, true);
            }
            return descriptors;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static TypeDescriptionProvider GetProvider(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            return NodeFor(instance, true);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static TypeDescriptionProvider GetProvider(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return NodeFor(type, true);
        }

        internal static TypeDescriptionProvider GetProviderRecursive(Type type)
        {
            return NodeFor(type, false);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static Type GetReflectionType(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            return NodeFor(instance).GetReflectionType(instance);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static Type GetReflectionType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return NodeFor(type).GetReflectionType(type);
        }

        private static TypeDescriptionNode NodeFor(object instance)
        {
            return NodeFor(instance, false);
        }

        private static TypeDescriptionNode NodeFor(Type type)
        {
            return NodeFor(type, false);
        }

        private static TypeDescriptionNode NodeFor(object instance, bool createDelegator)
        {
            TypeDescriptionNode node = (TypeDescriptionNode) _providerTable[instance];
            if (node != null)
            {
                return node;
            }
            Type comObjectType = instance.GetType();
            if (comObjectType.IsCOMObject)
            {
                comObjectType = ComObjectType;
            }
            if (createDelegator)
            {
                return new TypeDescriptionNode(new DelegatingTypeDescriptionProvider(comObjectType));
            }
            return NodeFor(comObjectType);
        }

        private static TypeDescriptionNode NodeFor(Type type, bool createDelegator)
        {
            CheckDefaultProvider(type);
            TypeDescriptionNode node = null;
            Type searchType = type;
            while (node == null)
            {
                node = (TypeDescriptionNode) _providerTypeTable[searchType];
                if (node == null)
                {
                    node = (TypeDescriptionNode) _providerTable[searchType];
                }
                if (node == null)
                {
                    Type nodeForBaseType = GetNodeForBaseType(searchType);
                    if ((searchType == typeof(object)) || (nodeForBaseType == null))
                    {
                        lock (_providerTable)
                        {
                            node = (TypeDescriptionNode) _providerTable[searchType];
                            if (node == null)
                            {
                                node = new TypeDescriptionNode(new ReflectTypeDescriptionProvider());
                                _providerTable[searchType] = node;
                            }
                            continue;
                        }
                    }
                    if (createDelegator)
                    {
                        node = new TypeDescriptionNode(new DelegatingTypeDescriptionProvider(nodeForBaseType));
                        _providerTypeTable[searchType] = node;
                    }
                    else
                    {
                        searchType = nodeForBaseType;
                    }
                }
            }
            return node;
        }

        private static void NodeRemove(object key, TypeDescriptionProvider provider)
        {
            lock (_providerTable)
            {
                TypeDescriptionNode node = (TypeDescriptionNode) _providerTable[key];
                TypeDescriptionNode next = node;
                while ((next != null) && (next.Provider != provider))
                {
                    next = next.Next;
                }
                if (next != null)
                {
                    if (next.Next != null)
                    {
                        next.Provider = next.Next.Provider;
                        next.Next = next.Next.Next;
                        if ((next == node) && (next.Provider is DelegatingTypeDescriptionProvider))
                        {
                            _providerTable.Remove(key);
                        }
                    }
                    else if (next != node)
                    {
                        Type type = key as Type;
                        if (type == null)
                        {
                            type = key.GetType();
                        }
                        next.Provider = new DelegatingTypeDescriptionProvider(type.BaseType);
                    }
                    else
                    {
                        _providerTable.Remove(key);
                    }
                    _providerTypeTable.Clear();
                }
            }
        }

        private static ICollection PipelineAttributeFilter(int pipelineType, ICollection members, Attribute[] filter, object instance, IDictionary cache)
        {
            IList list = members as ArrayList;
            if ((filter == null) || (filter.Length == 0))
            {
                return members;
            }
            if ((cache != null) && ((list == null) || list.IsReadOnly))
            {
                AttributeFilterCacheItem item = cache[_pipelineAttributeFilterKeys[pipelineType]] as AttributeFilterCacheItem;
                if ((item != null) && item.IsValid(filter))
                {
                    return item.FilteredMembers;
                }
            }
            if ((list == null) || list.IsReadOnly)
            {
                list = new ArrayList(members);
            }
            ArrayList list2 = FilterMembers(list, filter);
            if (list2 != null)
            {
                list = list2;
            }
            if (cache != null)
            {
                ICollection is2;
                switch (pipelineType)
                {
                    case 1:
                    {
                        PropertyDescriptor[] array = new PropertyDescriptor[list.Count];
                        list.CopyTo(array, 0);
                        is2 = new PropertyDescriptorCollection(array, true);
                        break;
                    }
                    case 2:
                    {
                        EventDescriptor[] descriptorArray2 = new EventDescriptor[list.Count];
                        list.CopyTo(descriptorArray2, 0);
                        is2 = new EventDescriptorCollection(descriptorArray2, true);
                        break;
                    }
                    default:
                        is2 = null;
                        break;
                }
                AttributeFilterCacheItem item2 = new AttributeFilterCacheItem(filter, is2);
                cache[_pipelineAttributeFilterKeys[pipelineType]] = item2;
            }
            return list;
        }

        private static ICollection PipelineFilter(int pipelineType, ICollection members, object instance, IDictionary cache)
        {
            bool flag;
            IComponent component = instance as IComponent;
            ITypeDescriptorFilterService filterService = null;
            if (component != null)
            {
                ISite site = component.Site;
                if (site != null)
                {
                    filterService = site.GetService(typeof(ITypeDescriptorFilterService)) as ITypeDescriptorFilterService;
                }
            }
            IList list = members as ArrayList;
            if (filterService == null)
            {
                return members;
            }
            if ((cache != null) && ((list == null) || list.IsReadOnly))
            {
                FilterCacheItem item = cache[_pipelineFilterKeys[pipelineType]] as FilterCacheItem;
                if ((item != null) && item.IsValid(filterService))
                {
                    return item.FilteredMembers;
                }
            }
            OrderedDictionary attributes = new OrderedDictionary(members.Count);
            switch (pipelineType)
            {
                case 0:
                    foreach (Attribute attribute in members)
                    {
                        attributes[attribute.TypeId] = attribute;
                    }
                    flag = filterService.FilterAttributes(component, attributes);
                    break;

                case 1:
                case 2:
                    foreach (MemberDescriptor descriptor in members)
                    {
                        string name = descriptor.Name;
                        if (attributes.Contains(name))
                        {
                            string extenderCollisionSuffix = GetExtenderCollisionSuffix(descriptor);
                            if (extenderCollisionSuffix != null)
                            {
                                attributes[name + extenderCollisionSuffix] = descriptor;
                            }
                            MemberDescriptor member = (MemberDescriptor) attributes[name];
                            extenderCollisionSuffix = GetExtenderCollisionSuffix(member);
                            if (extenderCollisionSuffix != null)
                            {
                                attributes.Remove(name);
                                attributes[member.Name + extenderCollisionSuffix] = member;
                            }
                        }
                        else
                        {
                            attributes[name] = descriptor;
                        }
                    }
                    if (pipelineType == 1)
                    {
                        flag = filterService.FilterProperties(component, attributes);
                    }
                    else
                    {
                        flag = filterService.FilterEvents(component, attributes);
                    }
                    break;

                default:
                    flag = false;
                    break;
            }
            if ((list == null) || list.IsReadOnly)
            {
                list = new ArrayList(attributes.Values);
            }
            else
            {
                list.Clear();
                foreach (object obj2 in attributes.Values)
                {
                    list.Add(obj2);
                }
            }
            if (flag && (cache != null))
            {
                ICollection is2;
                switch (pipelineType)
                {
                    case 0:
                    {
                        Attribute[] array = new Attribute[list.Count];
                        try
                        {
                            list.CopyTo(array, 0);
                        }
                        catch (InvalidCastException)
                        {
                            throw new ArgumentException(SR.GetString("TypeDescriptorExpectedElementType", new object[] { typeof(Attribute).FullName }));
                        }
                        is2 = new AttributeCollection(array);
                        break;
                    }
                    case 1:
                    {
                        PropertyDescriptor[] descriptorArray = new PropertyDescriptor[list.Count];
                        try
                        {
                            list.CopyTo(descriptorArray, 0);
                        }
                        catch (InvalidCastException)
                        {
                            throw new ArgumentException(SR.GetString("TypeDescriptorExpectedElementType", new object[] { typeof(PropertyDescriptor).FullName }));
                        }
                        is2 = new PropertyDescriptorCollection(descriptorArray, true);
                        break;
                    }
                    case 2:
                    {
                        EventDescriptor[] descriptorArray2 = new EventDescriptor[list.Count];
                        try
                        {
                            list.CopyTo(descriptorArray2, 0);
                        }
                        catch (InvalidCastException)
                        {
                            throw new ArgumentException(SR.GetString("TypeDescriptorExpectedElementType", new object[] { typeof(EventDescriptor).FullName }));
                        }
                        is2 = new EventDescriptorCollection(descriptorArray2, true);
                        break;
                    }
                    default:
                        is2 = null;
                        break;
                }
                FilterCacheItem item2 = new FilterCacheItem(filterService, is2);
                cache[_pipelineFilterKeys[pipelineType]] = item2;
                cache.Remove(_pipelineAttributeFilterKeys[pipelineType]);
            }
            return list;
        }

        private static ICollection PipelineInitialize(int pipelineType, ICollection members, IDictionary cache)
        {
            if (cache != null)
            {
                bool flag = true;
                ICollection is2 = cache[_pipelineInitializeKeys[pipelineType]] as ICollection;
                if ((is2 != null) && (is2.Count == members.Count))
                {
                    IEnumerator enumerator = is2.GetEnumerator();
                    IEnumerator enumerator2 = members.GetEnumerator();
                    while (enumerator.MoveNext() && enumerator2.MoveNext())
                    {
                        if (enumerator.Current != enumerator2.Current)
                        {
                            flag = false;
                            break;
                        }
                    }
                }
                if (!flag)
                {
                    cache.Remove(_pipelineMergeKeys[pipelineType]);
                    cache.Remove(_pipelineFilterKeys[pipelineType]);
                    cache.Remove(_pipelineAttributeFilterKeys[pipelineType]);
                    cache[_pipelineInitializeKeys[pipelineType]] = members;
                }
            }
            return members;
        }

        private static ICollection PipelineMerge(int pipelineType, ICollection primary, ICollection secondary, object instance, IDictionary cache)
        {
            if ((secondary == null) || (secondary.Count == 0))
            {
                return primary;
            }
            if (cache != null)
            {
                ICollection is2 = cache[_pipelineMergeKeys[pipelineType]] as ICollection;
                if ((is2 != null) && (is2.Count == (primary.Count + secondary.Count)))
                {
                    IEnumerator enumerator = is2.GetEnumerator();
                    IEnumerator enumerator2 = primary.GetEnumerator();
                    bool flag = true;
                    while (enumerator2.MoveNext() && enumerator.MoveNext())
                    {
                        if (enumerator2.Current != enumerator.Current)
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        IEnumerator enumerator3 = secondary.GetEnumerator();
                        while (enumerator3.MoveNext() && enumerator.MoveNext())
                        {
                            if (enumerator3.Current != enumerator.Current)
                            {
                                flag = false;
                                break;
                            }
                        }
                    }
                    if (flag)
                    {
                        return is2;
                    }
                }
            }
            ArrayList list = new ArrayList(primary.Count + secondary.Count);
            foreach (object obj2 in primary)
            {
                list.Add(obj2);
            }
            foreach (object obj3 in secondary)
            {
                list.Add(obj3);
            }
            if (cache != null)
            {
                ICollection is3;
                switch (pipelineType)
                {
                    case 0:
                    {
                        Attribute[] array = new Attribute[list.Count];
                        list.CopyTo(array, 0);
                        is3 = new AttributeCollection(array);
                        break;
                    }
                    case 1:
                    {
                        PropertyDescriptor[] descriptorArray = new PropertyDescriptor[list.Count];
                        list.CopyTo(descriptorArray, 0);
                        is3 = new PropertyDescriptorCollection(descriptorArray, true);
                        break;
                    }
                    case 2:
                    {
                        EventDescriptor[] descriptorArray2 = new EventDescriptor[list.Count];
                        list.CopyTo(descriptorArray2, 0);
                        is3 = new EventDescriptorCollection(descriptorArray2, true);
                        break;
                    }
                    default:
                        is3 = null;
                        break;
                }
                cache[_pipelineMergeKeys[pipelineType]] = is3;
                cache.Remove(_pipelineFilterKeys[pipelineType]);
                cache.Remove(_pipelineAttributeFilterKeys[pipelineType]);
            }
            return list;
        }

        private static void RaiseRefresh(object component)
        {
            RefreshEventHandler handler = _refreshHandler;
            if (handler != null)
            {
                handler(new RefreshEventArgs(component));
            }
        }

        private static void RaiseRefresh(Type type)
        {
            RefreshEventHandler handler = _refreshHandler;
            if (handler != null)
            {
                handler(new RefreshEventArgs(type));
            }
        }

        public static void Refresh(object component)
        {
            Refresh(component, true);
        }

        public static void Refresh(Assembly assembly)
        {
            if (assembly != null)
            {
                foreach (Module module in assembly.GetModules())
                {
                    Refresh(module);
                }
            }
        }

        public static void Refresh(Module module)
        {
            if (module != null)
            {
                Hashtable hashtable = null;
                lock (_providerTable)
                {
                    foreach (DictionaryEntry entry in _providerTable)
                    {
                        Type key = entry.Key as Type;
                        if (((key != null) && key.Module.Equals(module)) || (key == typeof(object)))
                        {
                            TypeDescriptionNode next = (TypeDescriptionNode) entry.Value;
                            while ((next != null) && !(next.Provider is ReflectTypeDescriptionProvider))
                            {
                                if (hashtable == null)
                                {
                                    hashtable = new Hashtable();
                                }
                                hashtable[key] = key;
                                next = next.Next;
                            }
                            if (next != null)
                            {
                                ReflectTypeDescriptionProvider provider = (ReflectTypeDescriptionProvider) next.Provider;
                                foreach (Type type2 in provider.GetPopulatedTypes(module))
                                {
                                    provider.Refresh(type2);
                                    if (hashtable == null)
                                    {
                                        hashtable = new Hashtable();
                                    }
                                    hashtable[type2] = type2;
                                }
                            }
                        }
                    }
                }
                if ((hashtable != null) && (_refreshHandler != null))
                {
                    foreach (Type type3 in hashtable.Keys)
                    {
                        RaiseRefresh(type3);
                    }
                }
            }
        }

        public static void Refresh(Type type)
        {
            if (type != null)
            {
                bool flag = false;
                lock (_providerTable)
                {
                    foreach (DictionaryEntry entry in _providerTable)
                    {
                        Type key = entry.Key as Type;
                        if (((key != null) && type.IsAssignableFrom(key)) || (key == typeof(object)))
                        {
                            TypeDescriptionNode next = (TypeDescriptionNode) entry.Value;
                            while ((next != null) && !(next.Provider is ReflectTypeDescriptionProvider))
                            {
                                flag = true;
                                next = next.Next;
                            }
                            if (next != null)
                            {
                                ReflectTypeDescriptionProvider provider = (ReflectTypeDescriptionProvider) next.Provider;
                                if (provider.IsPopulated(type))
                                {
                                    flag = true;
                                    provider.Refresh(type);
                                }
                            }
                        }
                    }
                }
                if (flag)
                {
                    Interlocked.Increment(ref _metadataVersion);
                    RaiseRefresh(type);
                }
            }
        }

        private static void Refresh(object component, bool refreshReflectionProvider)
        {
            if (component != null)
            {
                bool flag = false;
                if (refreshReflectionProvider)
                {
                    Type type = component.GetType();
                    lock (_providerTable)
                    {
                        foreach (DictionaryEntry entry in _providerTable)
                        {
                            Type key = entry.Key as Type;
                            if (((key != null) && type.IsAssignableFrom(key)) || (key == typeof(object)))
                            {
                                TypeDescriptionNode next = (TypeDescriptionNode) entry.Value;
                                while ((next != null) && !(next.Provider is ReflectTypeDescriptionProvider))
                                {
                                    flag = true;
                                    next = next.Next;
                                }
                                if (next != null)
                                {
                                    ReflectTypeDescriptionProvider provider = (ReflectTypeDescriptionProvider) next.Provider;
                                    if (provider.IsPopulated(type))
                                    {
                                        flag = true;
                                        provider.Refresh(type);
                                    }
                                }
                            }
                        }
                    }
                }
                IDictionary cache = GetCache(component);
                if (flag || (cache != null))
                {
                    if (cache != null)
                    {
                        for (int i = 0; i < _pipelineFilterKeys.Length; i++)
                        {
                            cache.Remove(_pipelineFilterKeys[i]);
                            cache.Remove(_pipelineMergeKeys[i]);
                            cache.Remove(_pipelineAttributeFilterKeys[i]);
                        }
                    }
                    Interlocked.Increment(ref _metadataVersion);
                    RaiseRefresh(component);
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public static void RemoveAssociation(object primary, object secondary)
        {
            if (primary == null)
            {
                throw new ArgumentNullException("primary");
            }
            if (secondary == null)
            {
                throw new ArgumentNullException("secondary");
            }
            Hashtable hashtable = _associationTable;
            if (hashtable != null)
            {
                IList list = (IList) hashtable[primary];
                if (list != null)
                {
                    lock (list)
                    {
                        for (int i = list.Count - 1; i >= 0; i--)
                        {
                            WeakReference reference = (WeakReference) list[i];
                            object target = reference.Target;
                            if ((target == null) || (target == secondary))
                            {
                                list.RemoveAt(i);
                            }
                        }
                    }
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public static void RemoveAssociations(object primary)
        {
            if (primary == null)
            {
                throw new ArgumentNullException("primary");
            }
            Hashtable hashtable = _associationTable;
            if (hashtable != null)
            {
                hashtable.Remove(primary);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public static void RemoveProvider(TypeDescriptionProvider provider, object instance)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            NodeRemove(instance, provider);
            RaiseRefresh(instance);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public static void RemoveProvider(TypeDescriptionProvider provider, Type type)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            NodeRemove(type, provider);
            RaiseRefresh(type);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void RemoveProviderTransparent(TypeDescriptionProvider provider, object instance)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            Type type = instance.GetType();
            PermissionSet other = new PermissionSet(PermissionState.None);
            other.AddPermission(new TypeDescriptorPermission(TypeDescriptorPermissionFlags.RestrictedRegistrationAccess));
            type.Assembly.PermissionSet.Union(other).Demand();
            RemoveProvider(provider, instance);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void RemoveProviderTransparent(TypeDescriptionProvider provider, Type type)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            PermissionSet other = new PermissionSet(PermissionState.None);
            other.AddPermission(new TypeDescriptorPermission(TypeDescriptorPermissionFlags.RestrictedRegistrationAccess));
            type.Assembly.PermissionSet.Union(other).Demand();
            RemoveProvider(provider, type);
        }

        private static bool ShouldHideMember(MemberDescriptor member, Attribute attribute)
        {
            if ((member == null) || (attribute == null))
            {
                return true;
            }
            Attribute attribute2 = member.Attributes[attribute.GetType()];
            if (attribute2 == null)
            {
                return !attribute.IsDefaultAttribute();
            }
            return !attribute.Match(attribute2);
        }

        public static void SortDescriptorArray(IList infos)
        {
            if (infos == null)
            {
                throw new ArgumentNullException("infos");
            }
            ArrayList.Adapter(infos).Sort(MemberDescriptorComparer.Instance);
        }

        [Conditional("DEBUG")]
        internal static void Trace(string message, params object[] args)
        {
        }

        [Obsolete("This property has been deprecated.  Use a type description provider to supply type information for COM types instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        public static IComNativeDescriptorHandler ComNativeDescriptorHandler
        {
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            get
            {
                TypeDescriptionNode next = NodeFor(ComObjectType);
                ComNativeDescriptionProvider provider = null;
                do
                {
                    provider = next.Provider as ComNativeDescriptionProvider;
                    next = next.Next;
                }
                while ((next != null) && (provider == null));
                if (provider != null)
                {
                    return provider.Handler;
                }
                return null;
            }
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            set
            {
                TypeDescriptionNode next = NodeFor(ComObjectType);
                while ((next != null) && !(next.Provider is ComNativeDescriptionProvider))
                {
                    next = next.Next;
                }
                if (next == null)
                {
                    AddProvider(new ComNativeDescriptionProvider(value), ComObjectType);
                }
                else
                {
                    ComNativeDescriptionProvider provider = (ComNativeDescriptionProvider) next.Provider;
                    provider.Handler = value;
                }
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static Type ComObjectType
        {
            get
            {
                return typeof(TypeDescriptorComObject);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static Type InterfaceType
        {
            get
            {
                return typeof(TypeDescriptorInterface);
            }
        }

        internal static int MetadataVersion
        {
            get
            {
                return _metadataVersion;
            }
        }

        private sealed class AttributeFilterCacheItem
        {
            private Attribute[] _filter;
            internal ICollection FilteredMembers;

            internal AttributeFilterCacheItem(Attribute[] filter, ICollection filteredMembers)
            {
                this._filter = filter;
                this.FilteredMembers = filteredMembers;
            }

            internal bool IsValid(Attribute[] filter)
            {
                if (this._filter.Length != filter.Length)
                {
                    return false;
                }
                for (int i = 0; i < filter.Length; i++)
                {
                    if (this._filter[i] != filter[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private sealed class AttributeProvider : TypeDescriptionProvider
        {
            private Attribute[] _attrs;

            internal AttributeProvider(TypeDescriptionProvider existingProvider, params Attribute[] attrs) : base(existingProvider)
            {
                this._attrs = attrs;
            }

            public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
            {
                return new AttributeTypeDescriptor(this._attrs, base.GetTypeDescriptor(objectType, instance));
            }

            private class AttributeTypeDescriptor : CustomTypeDescriptor
            {
                private Attribute[] _attributeArray;

                internal AttributeTypeDescriptor(Attribute[] attrs, ICustomTypeDescriptor parent) : base(parent)
                {
                    this._attributeArray = attrs;
                }

                public override AttributeCollection GetAttributes()
                {
                    Attribute[] destinationArray = null;
                    AttributeCollection attributes = base.GetAttributes();
                    Attribute[] attributeArray2 = this._attributeArray;
                    Attribute[] array = new Attribute[attributes.Count + attributeArray2.Length];
                    int count = attributes.Count;
                    attributes.CopyTo(array, 0);
                    for (int i = 0; i < attributeArray2.Length; i++)
                    {
                        bool flag = false;
                        for (int j = 0; j < attributes.Count; j++)
                        {
                            if (array[j].TypeId.Equals(attributeArray2[i].TypeId))
                            {
                                flag = true;
                                array[j] = attributeArray2[i];
                                break;
                            }
                        }
                        if (!flag)
                        {
                            array[count++] = attributeArray2[i];
                        }
                    }
                    if (count < array.Length)
                    {
                        destinationArray = new Attribute[count];
                        Array.Copy(array, 0, destinationArray, 0, count);
                    }
                    else
                    {
                        destinationArray = array;
                    }
                    return new AttributeCollection(destinationArray);
                }
            }
        }

        private sealed class ComNativeDescriptionProvider : TypeDescriptionProvider
        {
            private IComNativeDescriptorHandler _handler;

            internal ComNativeDescriptionProvider(IComNativeDescriptorHandler handler)
            {
                this._handler = handler;
            }

            public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
            {
                if (objectType == null)
                {
                    throw new ArgumentNullException("objectType");
                }
                if (instance == null)
                {
                    return null;
                }
                if (!objectType.IsInstanceOfType(instance))
                {
                    throw new ArgumentException("instance");
                }
                return new ComNativeTypeDescriptor(this._handler, instance);
            }

            internal IComNativeDescriptorHandler Handler
            {
                get
                {
                    return this._handler;
                }
                set
                {
                    this._handler = value;
                }
            }

            private sealed class ComNativeTypeDescriptor : ICustomTypeDescriptor
            {
                private IComNativeDescriptorHandler _handler;
                private object _instance;

                internal ComNativeTypeDescriptor(IComNativeDescriptorHandler handler, object instance)
                {
                    this._handler = handler;
                    this._instance = instance;
                }

                AttributeCollection ICustomTypeDescriptor.GetAttributes()
                {
                    return this._handler.GetAttributes(this._instance);
                }

                string ICustomTypeDescriptor.GetClassName()
                {
                    return this._handler.GetClassName(this._instance);
                }

                string ICustomTypeDescriptor.GetComponentName()
                {
                    return null;
                }

                TypeConverter ICustomTypeDescriptor.GetConverter()
                {
                    return this._handler.GetConverter(this._instance);
                }

                EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
                {
                    return this._handler.GetDefaultEvent(this._instance);
                }

                PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
                {
                    return this._handler.GetDefaultProperty(this._instance);
                }

                object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
                {
                    return this._handler.GetEditor(this._instance, editorBaseType);
                }

                EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
                {
                    return this._handler.GetEvents(this._instance);
                }

                EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
                {
                    return this._handler.GetEvents(this._instance, attributes);
                }

                PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
                {
                    return this._handler.GetProperties(this._instance, null);
                }

                PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
                {
                    return this._handler.GetProperties(this._instance, attributes);
                }

                object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
                {
                    return this._instance;
                }
            }
        }

        private sealed class FilterCacheItem
        {
            private ITypeDescriptorFilterService _filterService;
            internal ICollection FilteredMembers;

            internal FilterCacheItem(ITypeDescriptorFilterService filterService, ICollection filteredMembers)
            {
                this._filterService = filterService;
                this.FilteredMembers = filteredMembers;
            }

            internal bool IsValid(ITypeDescriptorFilterService filterService)
            {
                if (!object.ReferenceEquals(this._filterService, filterService))
                {
                    return false;
                }
                return true;
            }
        }

        private interface IUnimplemented
        {
        }

        private sealed class MemberDescriptorComparer : IComparer
        {
            public static readonly TypeDescriptor.MemberDescriptorComparer Instance = new TypeDescriptor.MemberDescriptorComparer();

            public int Compare(object left, object right)
            {
                return string.Compare(((MemberDescriptor) left).Name, ((MemberDescriptor) right).Name, false, CultureInfo.InvariantCulture);
            }
        }

        private sealed class MergedTypeDescriptor : ICustomTypeDescriptor
        {
            private ICustomTypeDescriptor _primary;
            private ICustomTypeDescriptor _secondary;

            internal MergedTypeDescriptor(ICustomTypeDescriptor primary, ICustomTypeDescriptor secondary)
            {
                this._primary = primary;
                this._secondary = secondary;
            }

            AttributeCollection ICustomTypeDescriptor.GetAttributes()
            {
                AttributeCollection attributes = this._primary.GetAttributes();
                if (attributes == null)
                {
                    attributes = this._secondary.GetAttributes();
                }
                return attributes;
            }

            string ICustomTypeDescriptor.GetClassName()
            {
                string className = this._primary.GetClassName();
                if (className == null)
                {
                    className = this._secondary.GetClassName();
                }
                return className;
            }

            string ICustomTypeDescriptor.GetComponentName()
            {
                string componentName = this._primary.GetComponentName();
                if (componentName == null)
                {
                    componentName = this._secondary.GetComponentName();
                }
                return componentName;
            }

            TypeConverter ICustomTypeDescriptor.GetConverter()
            {
                TypeConverter converter = this._primary.GetConverter();
                if (converter == null)
                {
                    converter = this._secondary.GetConverter();
                }
                return converter;
            }

            EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
            {
                EventDescriptor defaultEvent = this._primary.GetDefaultEvent();
                if (defaultEvent == null)
                {
                    defaultEvent = this._secondary.GetDefaultEvent();
                }
                return defaultEvent;
            }

            PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
            {
                PropertyDescriptor defaultProperty = this._primary.GetDefaultProperty();
                if (defaultProperty == null)
                {
                    defaultProperty = this._secondary.GetDefaultProperty();
                }
                return defaultProperty;
            }

            object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
            {
                if (editorBaseType == null)
                {
                    throw new ArgumentNullException("editorBaseType");
                }
                object editor = this._primary.GetEditor(editorBaseType);
                if (editor == null)
                {
                    editor = this._secondary.GetEditor(editorBaseType);
                }
                return editor;
            }

            EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
            {
                EventDescriptorCollection events = this._primary.GetEvents();
                if (events == null)
                {
                    events = this._secondary.GetEvents();
                }
                return events;
            }

            EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
            {
                EventDescriptorCollection events = this._primary.GetEvents(attributes);
                if (events == null)
                {
                    events = this._secondary.GetEvents(attributes);
                }
                return events;
            }

            PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
            {
                PropertyDescriptorCollection properties = this._primary.GetProperties();
                if (properties == null)
                {
                    properties = this._secondary.GetProperties();
                }
                return properties;
            }

            PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
            {
                PropertyDescriptorCollection properties = this._primary.GetProperties(attributes);
                if (properties == null)
                {
                    properties = this._secondary.GetProperties(attributes);
                }
                return properties;
            }

            object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
            {
                object propertyOwner = this._primary.GetPropertyOwner(pd);
                if (propertyOwner == null)
                {
                    propertyOwner = this._secondary.GetPropertyOwner(pd);
                }
                return propertyOwner;
            }
        }

        private sealed class TypeDescriptionNode : TypeDescriptionProvider
        {
            internal TypeDescriptor.TypeDescriptionNode Next;
            internal TypeDescriptionProvider Provider;

            internal TypeDescriptionNode(TypeDescriptionProvider provider)
            {
                this.Provider = provider;
            }

            public override object CreateInstance(IServiceProvider provider, Type objectType, Type[] argTypes, object[] args)
            {
                if (objectType == null)
                {
                    throw new ArgumentNullException("objectType");
                }
                if (argTypes != null)
                {
                    if (args == null)
                    {
                        throw new ArgumentNullException("args");
                    }
                    if (argTypes.Length != args.Length)
                    {
                        throw new ArgumentException(SR.GetString("TypeDescriptorArgsCountMismatch"));
                    }
                }
                return this.Provider.CreateInstance(provider, objectType, argTypes, args);
            }

            public override IDictionary GetCache(object instance)
            {
                if (instance == null)
                {
                    throw new ArgumentNullException("instance");
                }
                return this.Provider.GetCache(instance);
            }

            public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
            {
                if (instance == null)
                {
                    throw new ArgumentNullException("instance");
                }
                return new DefaultExtendedTypeDescriptor(this, instance);
            }

            protected internal override IExtenderProvider[] GetExtenderProviders(object instance)
            {
                if (instance == null)
                {
                    throw new ArgumentNullException("instance");
                }
                return this.Provider.GetExtenderProviders(instance);
            }

            public override string GetFullComponentName(object component)
            {
                if (component == null)
                {
                    throw new ArgumentNullException("component");
                }
                return this.Provider.GetFullComponentName(component);
            }

            public override Type GetReflectionType(Type objectType, object instance)
            {
                if (objectType == null)
                {
                    throw new ArgumentNullException("objectType");
                }
                return this.Provider.GetReflectionType(objectType, instance);
            }

            public override Type GetRuntimeType(Type objectType)
            {
                if (objectType == null)
                {
                    throw new ArgumentNullException("objectType");
                }
                return this.Provider.GetRuntimeType(objectType);
            }

            public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
            {
                if (objectType == null)
                {
                    throw new ArgumentNullException("objectType");
                }
                if ((instance != null) && !objectType.IsInstanceOfType(instance))
                {
                    throw new ArgumentException("instance");
                }
                return new DefaultTypeDescriptor(this, objectType, instance);
            }

            public override bool IsSupportedType(Type type)
            {
                if (type == null)
                {
                    throw new ArgumentNullException("type");
                }
                return this.Provider.IsSupportedType(type);
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct DefaultExtendedTypeDescriptor : ICustomTypeDescriptor
            {
                private TypeDescriptor.TypeDescriptionNode _node;
                private object _instance;
                internal DefaultExtendedTypeDescriptor(TypeDescriptor.TypeDescriptionNode node, object instance)
                {
                    this._node = node;
                    this._instance = instance;
                }

                AttributeCollection ICustomTypeDescriptor.GetAttributes()
                {
                    TypeDescriptionProvider provider = this._node.Provider;
                    ReflectTypeDescriptionProvider provider2 = provider as ReflectTypeDescriptionProvider;
                    if (provider2 != null)
                    {
                        return provider2.GetExtendedAttributes(this._instance);
                    }
                    ICustomTypeDescriptor extendedTypeDescriptor = provider.GetExtendedTypeDescriptor(this._instance);
                    if (extendedTypeDescriptor == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetExtendedTypeDescriptor" }));
                    }
                    AttributeCollection attributes = extendedTypeDescriptor.GetAttributes();
                    if (attributes == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetAttributes" }));
                    }
                    return attributes;
                }

                string ICustomTypeDescriptor.GetClassName()
                {
                    TypeDescriptionProvider provider = this._node.Provider;
                    ReflectTypeDescriptionProvider provider2 = provider as ReflectTypeDescriptionProvider;
                    if (provider2 != null)
                    {
                        return provider2.GetExtendedClassName(this._instance);
                    }
                    ICustomTypeDescriptor extendedTypeDescriptor = provider.GetExtendedTypeDescriptor(this._instance);
                    if (extendedTypeDescriptor == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetExtendedTypeDescriptor" }));
                    }
                    string className = extendedTypeDescriptor.GetClassName();
                    if (className == null)
                    {
                        className = this._instance.GetType().FullName;
                    }
                    return className;
                }

                string ICustomTypeDescriptor.GetComponentName()
                {
                    TypeDescriptionProvider provider = this._node.Provider;
                    ReflectTypeDescriptionProvider provider2 = provider as ReflectTypeDescriptionProvider;
                    if (provider2 != null)
                    {
                        return provider2.GetExtendedComponentName(this._instance);
                    }
                    ICustomTypeDescriptor extendedTypeDescriptor = provider.GetExtendedTypeDescriptor(this._instance);
                    if (extendedTypeDescriptor == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetExtendedTypeDescriptor" }));
                    }
                    return extendedTypeDescriptor.GetComponentName();
                }

                TypeConverter ICustomTypeDescriptor.GetConverter()
                {
                    TypeDescriptionProvider provider = this._node.Provider;
                    ReflectTypeDescriptionProvider provider2 = provider as ReflectTypeDescriptionProvider;
                    if (provider2 != null)
                    {
                        return provider2.GetExtendedConverter(this._instance);
                    }
                    ICustomTypeDescriptor extendedTypeDescriptor = provider.GetExtendedTypeDescriptor(this._instance);
                    if (extendedTypeDescriptor == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetExtendedTypeDescriptor" }));
                    }
                    TypeConverter converter = extendedTypeDescriptor.GetConverter();
                    if (converter == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetConverter" }));
                    }
                    return converter;
                }

                EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
                {
                    TypeDescriptionProvider provider = this._node.Provider;
                    ReflectTypeDescriptionProvider provider2 = provider as ReflectTypeDescriptionProvider;
                    if (provider2 != null)
                    {
                        return provider2.GetExtendedDefaultEvent(this._instance);
                    }
                    ICustomTypeDescriptor extendedTypeDescriptor = provider.GetExtendedTypeDescriptor(this._instance);
                    if (extendedTypeDescriptor == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetExtendedTypeDescriptor" }));
                    }
                    return extendedTypeDescriptor.GetDefaultEvent();
                }

                PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
                {
                    TypeDescriptionProvider provider = this._node.Provider;
                    ReflectTypeDescriptionProvider provider2 = provider as ReflectTypeDescriptionProvider;
                    if (provider2 != null)
                    {
                        return provider2.GetExtendedDefaultProperty(this._instance);
                    }
                    ICustomTypeDescriptor extendedTypeDescriptor = provider.GetExtendedTypeDescriptor(this._instance);
                    if (extendedTypeDescriptor == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetExtendedTypeDescriptor" }));
                    }
                    return extendedTypeDescriptor.GetDefaultProperty();
                }

                object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
                {
                    if (editorBaseType == null)
                    {
                        throw new ArgumentNullException("editorBaseType");
                    }
                    TypeDescriptionProvider provider = this._node.Provider;
                    ReflectTypeDescriptionProvider provider2 = provider as ReflectTypeDescriptionProvider;
                    if (provider2 != null)
                    {
                        return provider2.GetExtendedEditor(this._instance, editorBaseType);
                    }
                    ICustomTypeDescriptor extendedTypeDescriptor = provider.GetExtendedTypeDescriptor(this._instance);
                    if (extendedTypeDescriptor == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetExtendedTypeDescriptor" }));
                    }
                    return extendedTypeDescriptor.GetEditor(editorBaseType);
                }

                EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
                {
                    TypeDescriptionProvider provider = this._node.Provider;
                    ReflectTypeDescriptionProvider provider2 = provider as ReflectTypeDescriptionProvider;
                    if (provider2 != null)
                    {
                        return provider2.GetExtendedEvents(this._instance);
                    }
                    ICustomTypeDescriptor extendedTypeDescriptor = provider.GetExtendedTypeDescriptor(this._instance);
                    if (extendedTypeDescriptor == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetExtendedTypeDescriptor" }));
                    }
                    EventDescriptorCollection events = extendedTypeDescriptor.GetEvents();
                    if (events == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetEvents" }));
                    }
                    return events;
                }

                EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
                {
                    TypeDescriptionProvider provider = this._node.Provider;
                    ReflectTypeDescriptionProvider provider2 = provider as ReflectTypeDescriptionProvider;
                    if (provider2 != null)
                    {
                        return provider2.GetExtendedEvents(this._instance);
                    }
                    ICustomTypeDescriptor extendedTypeDescriptor = provider.GetExtendedTypeDescriptor(this._instance);
                    if (extendedTypeDescriptor == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetExtendedTypeDescriptor" }));
                    }
                    EventDescriptorCollection events = extendedTypeDescriptor.GetEvents(attributes);
                    if (events == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetEvents" }));
                    }
                    return events;
                }

                PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
                {
                    TypeDescriptionProvider provider = this._node.Provider;
                    ReflectTypeDescriptionProvider provider2 = provider as ReflectTypeDescriptionProvider;
                    if (provider2 != null)
                    {
                        return provider2.GetExtendedProperties(this._instance);
                    }
                    ICustomTypeDescriptor extendedTypeDescriptor = provider.GetExtendedTypeDescriptor(this._instance);
                    if (extendedTypeDescriptor == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetExtendedTypeDescriptor" }));
                    }
                    PropertyDescriptorCollection properties = extendedTypeDescriptor.GetProperties();
                    if (properties == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetProperties" }));
                    }
                    return properties;
                }

                PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
                {
                    TypeDescriptionProvider provider = this._node.Provider;
                    ReflectTypeDescriptionProvider provider2 = provider as ReflectTypeDescriptionProvider;
                    if (provider2 != null)
                    {
                        return provider2.GetExtendedProperties(this._instance);
                    }
                    ICustomTypeDescriptor extendedTypeDescriptor = provider.GetExtendedTypeDescriptor(this._instance);
                    if (extendedTypeDescriptor == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetExtendedTypeDescriptor" }));
                    }
                    PropertyDescriptorCollection properties = extendedTypeDescriptor.GetProperties(attributes);
                    if (properties == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetProperties" }));
                    }
                    return properties;
                }

                object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
                {
                    TypeDescriptionProvider provider = this._node.Provider;
                    ReflectTypeDescriptionProvider provider2 = provider as ReflectTypeDescriptionProvider;
                    if (provider2 != null)
                    {
                        return provider2.GetExtendedPropertyOwner(this._instance, pd);
                    }
                    ICustomTypeDescriptor extendedTypeDescriptor = provider.GetExtendedTypeDescriptor(this._instance);
                    if (extendedTypeDescriptor == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetExtendedTypeDescriptor" }));
                    }
                    object propertyOwner = extendedTypeDescriptor.GetPropertyOwner(pd);
                    if (propertyOwner == null)
                    {
                        propertyOwner = this._instance;
                    }
                    return propertyOwner;
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct DefaultTypeDescriptor : ICustomTypeDescriptor
            {
                private TypeDescriptor.TypeDescriptionNode _node;
                private Type _objectType;
                private object _instance;
                internal DefaultTypeDescriptor(TypeDescriptor.TypeDescriptionNode node, Type objectType, object instance)
                {
                    this._node = node;
                    this._objectType = objectType;
                    this._instance = instance;
                }

                AttributeCollection ICustomTypeDescriptor.GetAttributes()
                {
                    TypeDescriptionProvider provider = this._node.Provider;
                    ReflectTypeDescriptionProvider provider2 = provider as ReflectTypeDescriptionProvider;
                    if (provider2 != null)
                    {
                        return provider2.GetAttributes(this._objectType);
                    }
                    ICustomTypeDescriptor typeDescriptor = provider.GetTypeDescriptor(this._objectType, this._instance);
                    if (typeDescriptor == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetTypeDescriptor" }));
                    }
                    AttributeCollection attributes = typeDescriptor.GetAttributes();
                    if (attributes == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetAttributes" }));
                    }
                    return attributes;
                }

                string ICustomTypeDescriptor.GetClassName()
                {
                    TypeDescriptionProvider provider = this._node.Provider;
                    ReflectTypeDescriptionProvider provider2 = provider as ReflectTypeDescriptionProvider;
                    if (provider2 != null)
                    {
                        return provider2.GetClassName(this._objectType);
                    }
                    ICustomTypeDescriptor typeDescriptor = provider.GetTypeDescriptor(this._objectType, this._instance);
                    if (typeDescriptor == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetTypeDescriptor" }));
                    }
                    string className = typeDescriptor.GetClassName();
                    if (className == null)
                    {
                        className = this._objectType.FullName;
                    }
                    return className;
                }

                string ICustomTypeDescriptor.GetComponentName()
                {
                    TypeDescriptionProvider provider = this._node.Provider;
                    ReflectTypeDescriptionProvider provider2 = provider as ReflectTypeDescriptionProvider;
                    if (provider2 != null)
                    {
                        return provider2.GetComponentName(this._objectType, this._instance);
                    }
                    ICustomTypeDescriptor typeDescriptor = provider.GetTypeDescriptor(this._objectType, this._instance);
                    if (typeDescriptor == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetTypeDescriptor" }));
                    }
                    return typeDescriptor.GetComponentName();
                }

                TypeConverter ICustomTypeDescriptor.GetConverter()
                {
                    TypeDescriptionProvider provider = this._node.Provider;
                    ReflectTypeDescriptionProvider provider2 = provider as ReflectTypeDescriptionProvider;
                    if (provider2 != null)
                    {
                        return provider2.GetConverter(this._objectType, this._instance);
                    }
                    ICustomTypeDescriptor typeDescriptor = provider.GetTypeDescriptor(this._objectType, this._instance);
                    if (typeDescriptor == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetTypeDescriptor" }));
                    }
                    TypeConverter converter = typeDescriptor.GetConverter();
                    if (converter == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetConverter" }));
                    }
                    return converter;
                }

                EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
                {
                    TypeDescriptionProvider provider = this._node.Provider;
                    ReflectTypeDescriptionProvider provider2 = provider as ReflectTypeDescriptionProvider;
                    if (provider2 != null)
                    {
                        return provider2.GetDefaultEvent(this._objectType, this._instance);
                    }
                    ICustomTypeDescriptor typeDescriptor = provider.GetTypeDescriptor(this._objectType, this._instance);
                    if (typeDescriptor == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetTypeDescriptor" }));
                    }
                    return typeDescriptor.GetDefaultEvent();
                }

                PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
                {
                    TypeDescriptionProvider provider = this._node.Provider;
                    ReflectTypeDescriptionProvider provider2 = provider as ReflectTypeDescriptionProvider;
                    if (provider2 != null)
                    {
                        return provider2.GetDefaultProperty(this._objectType, this._instance);
                    }
                    ICustomTypeDescriptor typeDescriptor = provider.GetTypeDescriptor(this._objectType, this._instance);
                    if (typeDescriptor == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetTypeDescriptor" }));
                    }
                    return typeDescriptor.GetDefaultProperty();
                }

                object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
                {
                    if (editorBaseType == null)
                    {
                        throw new ArgumentNullException("editorBaseType");
                    }
                    TypeDescriptionProvider provider = this._node.Provider;
                    ReflectTypeDescriptionProvider provider2 = provider as ReflectTypeDescriptionProvider;
                    if (provider2 != null)
                    {
                        return provider2.GetEditor(this._objectType, this._instance, editorBaseType);
                    }
                    ICustomTypeDescriptor typeDescriptor = provider.GetTypeDescriptor(this._objectType, this._instance);
                    if (typeDescriptor == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetTypeDescriptor" }));
                    }
                    return typeDescriptor.GetEditor(editorBaseType);
                }

                EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
                {
                    TypeDescriptionProvider provider = this._node.Provider;
                    ReflectTypeDescriptionProvider provider2 = provider as ReflectTypeDescriptionProvider;
                    if (provider2 != null)
                    {
                        return provider2.GetEvents(this._objectType);
                    }
                    ICustomTypeDescriptor typeDescriptor = provider.GetTypeDescriptor(this._objectType, this._instance);
                    if (typeDescriptor == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetTypeDescriptor" }));
                    }
                    EventDescriptorCollection events = typeDescriptor.GetEvents();
                    if (events == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetEvents" }));
                    }
                    return events;
                }

                EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
                {
                    TypeDescriptionProvider provider = this._node.Provider;
                    ReflectTypeDescriptionProvider provider2 = provider as ReflectTypeDescriptionProvider;
                    if (provider2 != null)
                    {
                        return provider2.GetEvents(this._objectType);
                    }
                    ICustomTypeDescriptor typeDescriptor = provider.GetTypeDescriptor(this._objectType, this._instance);
                    if (typeDescriptor == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetTypeDescriptor" }));
                    }
                    EventDescriptorCollection events = typeDescriptor.GetEvents(attributes);
                    if (events == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetEvents" }));
                    }
                    return events;
                }

                PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
                {
                    TypeDescriptionProvider provider = this._node.Provider;
                    ReflectTypeDescriptionProvider provider2 = provider as ReflectTypeDescriptionProvider;
                    if (provider2 != null)
                    {
                        return provider2.GetProperties(this._objectType);
                    }
                    ICustomTypeDescriptor typeDescriptor = provider.GetTypeDescriptor(this._objectType, this._instance);
                    if (typeDescriptor == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetTypeDescriptor" }));
                    }
                    PropertyDescriptorCollection properties = typeDescriptor.GetProperties();
                    if (properties == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetProperties" }));
                    }
                    return properties;
                }

                PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
                {
                    TypeDescriptionProvider provider = this._node.Provider;
                    ReflectTypeDescriptionProvider provider2 = provider as ReflectTypeDescriptionProvider;
                    if (provider2 != null)
                    {
                        return provider2.GetProperties(this._objectType);
                    }
                    ICustomTypeDescriptor typeDescriptor = provider.GetTypeDescriptor(this._objectType, this._instance);
                    if (typeDescriptor == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetTypeDescriptor" }));
                    }
                    PropertyDescriptorCollection properties = typeDescriptor.GetProperties(attributes);
                    if (properties == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetProperties" }));
                    }
                    return properties;
                }

                object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
                {
                    TypeDescriptionProvider provider = this._node.Provider;
                    ReflectTypeDescriptionProvider provider2 = provider as ReflectTypeDescriptionProvider;
                    if (provider2 != null)
                    {
                        return provider2.GetPropertyOwner(this._objectType, this._instance, pd);
                    }
                    ICustomTypeDescriptor typeDescriptor = provider.GetTypeDescriptor(this._objectType, this._instance);
                    if (typeDescriptor == null)
                    {
                        throw new InvalidOperationException(SR.GetString("TypeDescriptorProviderError", new object[] { this._node.Provider.GetType().FullName, "GetTypeDescriptor" }));
                    }
                    object propertyOwner = typeDescriptor.GetPropertyOwner(pd);
                    if (propertyOwner == null)
                    {
                        propertyOwner = this._instance;
                    }
                    return propertyOwner;
                }
            }
        }

        [TypeDescriptionProvider("System.Windows.Forms.ComponentModel.Com2Interop.ComNativeDescriptor, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
        private sealed class TypeDescriptorComObject
        {
        }

        private sealed class TypeDescriptorInterface
        {
        }
    }
}

