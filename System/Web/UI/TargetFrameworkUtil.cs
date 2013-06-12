namespace System.Web.UI
{
    using System;
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Web.Compilation;

    internal static class TargetFrameworkUtil
    {
        private static ClientBuildManagerTypeDescriptionProviderBridge s_cbmTdpBridge;
        private static ConcurrentDictionary<Type, EventDescriptorCollection> s_eventDescriptorCollectionDict = new ConcurrentDictionary<Type, EventDescriptorCollection>();
        private static ConcurrentDictionary<Type, bool> s_isFrameworkType = new ConcurrentDictionary<Type, bool>();
        private static ConcurrentDictionary<Type, MemberCache> s_memberCache = new ConcurrentDictionary<Type, MemberCache>();
        private static ConcurrentDictionary<object, PropertyDescriptorCollection> s_objectPropertyDescriptorCollectionDict = new ConcurrentDictionary<object, PropertyDescriptorCollection>();
        private static ConcurrentDictionary<Type, PropertyDescriptorCollection> s_typePropertyDescriptorCollectionDict = new ConcurrentDictionary<Type, PropertyDescriptorCollection>();

        internal static System.ComponentModel.AttributeCollection GetAttributes(Type type)
        {
            ICustomTypeDescriptor typeDescriptor = GetTypeDescriptor(type);
            if (typeDescriptor != null)
            {
                return typeDescriptor.GetAttributes();
            }
            return TypeDescriptor.GetAttributes(type);
        }

        internal static object[] GetCustomAttributes(Type type, Type attributeType, bool inherit)
        {
            return GetReflectionType(type).GetCustomAttributes(attributeType, inherit);
        }

        internal static EventInfo GetEvent(Type type, string name)
        {
            if (SkipCache)
            {
                return GetEventInfo(type, name);
            }
            EventInfo eventInfo = null;
            MemberCache memberCache = GetMemberCache(type);
            if (!memberCache.Events.TryGetValue(name, out eventInfo))
            {
                eventInfo = GetEventInfo(type, name);
                memberCache.Events.TryAdd(name, eventInfo);
            }
            return eventInfo;
        }

        private static EventDescriptorCollection GetEventDescriptorCollection(Type type)
        {
            if ((s_cbmTdpBridge != null) && IsFrameworkType(type))
            {
                return GetFilteredEventDescriptorCollection(type, null);
            }
            ICustomTypeDescriptor typeDescriptor = GetTypeDescriptor(type);
            if (typeDescriptor != null)
            {
                return typeDescriptor.GetEvents();
            }
            return TypeDescriptor.GetEvents(type);
        }

        private static EventInfo GetEventInfo(Type type, string name)
        {
            if ((s_cbmTdpBridge != null) && IsFrameworkType(type))
            {
                Type typeToUseForCBMBridge = GetTypeToUseForCBMBridge(type);
                if (s_cbmTdpBridge.HasEvent(typeToUseForCBMBridge, name))
                {
                    return type.GetEvent(name);
                }
                return null;
            }
            if (GetReflectionType(type).GetEvent(name) != null)
            {
                return type.GetEvent(name);
            }
            return null;
        }

        internal static EventDescriptorCollection GetEvents(Type type)
        {
            if (SkipCache)
            {
                return GetEventDescriptorCollection(type);
            }
            EventDescriptorCollection eventDescriptorCollection = null;
            if (!s_eventDescriptorCollectionDict.TryGetValue(type, out eventDescriptorCollection))
            {
                eventDescriptorCollection = GetEventDescriptorCollection(type);
                s_eventDescriptorCollectionDict.TryAdd(type, eventDescriptorCollection);
            }
            return eventDescriptorCollection;
        }

        internal static FieldInfo GetField(Type type, string name, BindingFlags bindingAttr)
        {
            if (SkipCache)
            {
                return GetFieldInfo(type, name, bindingAttr);
            }
            FieldInfo info = null;
            MemberCache memberCache = GetMemberCache(type);
            Tuple<string, int> key = MakeTuple(name, bindingAttr);
            if (!memberCache.Fields.TryGetValue(key, out info))
            {
                info = GetFieldInfo(type, name, bindingAttr);
                memberCache.Fields.TryAdd(key, info);
            }
            return info;
        }

        private static FieldInfo GetFieldInfo(Type type, string name, BindingFlags bindingAttr)
        {
            if ((s_cbmTdpBridge != null) && IsFrameworkType(type))
            {
                Type typeToUseForCBMBridge = GetTypeToUseForCBMBridge(type);
                if (s_cbmTdpBridge.HasField(typeToUseForCBMBridge, name, bindingAttr))
                {
                    return type.GetField(name, bindingAttr);
                }
                return null;
            }
            if (GetReflectionType(type).GetField(name, bindingAttr) != null)
            {
                return type.GetField(name, bindingAttr);
            }
            return null;
        }

        private static EventDescriptorCollection GetFilteredEventDescriptorCollection(Type objectType, object instance)
        {
            EventDescriptorCollection eventDescriptors = null;
            if (instance != null)
            {
                eventDescriptors = TypeDescriptor.GetEvents(instance);
            }
            else
            {
                if (objectType == null)
                {
                    throw new ArgumentException("At least one argument should be non-null");
                }
                eventDescriptors = TypeDescriptor.GetEvents(objectType);
            }
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            Type typeToUseForCBMBridge = GetTypeToUseForCBMBridge(objectType);
            return new EventDescriptorCollection((from e in s_cbmTdpBridge.GetFilteredEvents(typeToUseForCBMBridge, bindingFlags)
                let d = eventDescriptors[e]
                where d != null
                select d).ToArray<EventDescriptor>());
        }

        private static PropertyDescriptorCollection GetFilteredPropertyDescriptorCollection(Type objectType, object instance)
        {
            PropertyDescriptorCollection propertyDescriptors = null;
            if (instance != null)
            {
                propertyDescriptors = TypeDescriptor.GetProperties(instance);
            }
            else
            {
                if (objectType == null)
                {
                    throw new ArgumentException("At least one argument should be non-null");
                }
                propertyDescriptors = TypeDescriptor.GetProperties(objectType);
            }
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            Type typeToUseForCBMBridge = GetTypeToUseForCBMBridge(objectType);
            return new PropertyDescriptorCollection((from p in s_cbmTdpBridge.GetFilteredProperties(typeToUseForCBMBridge, bindingFlags)
                let d = propertyDescriptors[p]
                where d != null
                select d).ToArray<PropertyDescriptor>());
        }

        private static MemberCache GetMemberCache(Type type)
        {
            MemberCache cache = null;
            if (!s_memberCache.TryGetValue(type, out cache))
            {
                cache = new MemberCache();
                s_memberCache.TryAdd(type, cache);
            }
            return cache;
        }

        private static PropertyInfo GetMostSpecificProperty(Type type, string name, BindingFlags additionalFlags)
        {
            BindingFlags declaredOnly = BindingFlags.DeclaredOnly;
            declaredOnly |= additionalFlags;
            for (Type type2 = type; type2 != null; type2 = type2.BaseType)
            {
                PropertyInfo info = GetProperty(type2, name, declaredOnly);
                if (info != null)
                {
                    return info;
                }
            }
            return null;
        }

        internal static PropertyDescriptorCollection GetProperties(object obj)
        {
            if (SkipCache)
            {
                return GetPropertyDescriptorCollection(obj);
            }
            PropertyDescriptorCollection propertyDescriptorCollection = null;
            if (!s_objectPropertyDescriptorCollectionDict.TryGetValue(obj, out propertyDescriptorCollection))
            {
                propertyDescriptorCollection = GetPropertyDescriptorCollection(obj);
                s_objectPropertyDescriptorCollectionDict.TryAdd(obj, propertyDescriptorCollection);
            }
            return propertyDescriptorCollection;
        }

        internal static PropertyDescriptorCollection GetProperties(Type type)
        {
            if (SkipCache)
            {
                return GetPropertyDescriptorCollection(type);
            }
            PropertyDescriptorCollection propertyDescriptorCollection = null;
            if (!s_typePropertyDescriptorCollectionDict.TryGetValue(type, out propertyDescriptorCollection))
            {
                propertyDescriptorCollection = GetPropertyDescriptorCollection(type);
                s_typePropertyDescriptorCollectionDict.TryAdd(type, propertyDescriptorCollection);
            }
            return propertyDescriptorCollection;
        }

        internal static PropertyInfo GetProperty(Type type, string name, BindingFlags bindingAttr)
        {
            return GetProperty(type, name, bindingAttr, false);
        }

        internal static PropertyInfo GetProperty(Type type, string name, BindingFlags bindingAttr, bool throwAmbiguousMatchException)
        {
            return GetProperty(type, name, bindingAttr, null, null, Type.EmptyTypes, null);
        }

        internal static PropertyInfo GetProperty(Type type, string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            return GetProperty(type, name, bindingAttr, binder, returnType, types, modifiers, false);
        }

        internal static PropertyInfo GetProperty(Type type, string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers, bool throwAmbiguousMatchException)
        {
            if ((SkipCache || (binder != null)) || (((returnType != null) || (types != Type.EmptyTypes)) || (modifiers != null)))
            {
                return GetPropertyHelper(type, name, bindingAttr, binder, returnType, types, modifiers, throwAmbiguousMatchException);
            }
            PropertyInfo info = null;
            MemberCache memberCache = GetMemberCache(type);
            Tuple<string, int> key = MakeTuple(name, bindingAttr);
            if (!memberCache.Properties.TryGetValue(key, out info))
            {
                info = GetPropertyHelper(type, name, bindingAttr, binder, returnType, types, modifiers, throwAmbiguousMatchException);
                memberCache.Properties.TryAdd(key, info);
            }
            return info;
        }

        private static PropertyDescriptorCollection GetPropertyDescriptorCollection(object obj)
        {
            Type type = obj.GetType();
            if ((s_cbmTdpBridge != null) && IsFrameworkType(type))
            {
                return GetFilteredPropertyDescriptorCollection(type, obj);
            }
            ICustomTypeDescriptor typeDescriptor = GetTypeDescriptor(obj);
            if (typeDescriptor != null)
            {
                return typeDescriptor.GetProperties();
            }
            return TypeDescriptor.GetProperties(obj);
        }

        private static PropertyDescriptorCollection GetPropertyDescriptorCollection(Type type)
        {
            if ((s_cbmTdpBridge != null) && IsFrameworkType(type))
            {
                return GetFilteredPropertyDescriptorCollection(type, null);
            }
            ICustomTypeDescriptor typeDescriptor = GetTypeDescriptor(type);
            if (typeDescriptor != null)
            {
                return typeDescriptor.GetProperties();
            }
            return TypeDescriptor.GetProperties(type);
        }

        private static PropertyInfo GetPropertyHelper(Type type, string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers, bool throwAmbiguousMatchException)
        {
            try
            {
                bool flag = false;
                if ((s_cbmTdpBridge != null) && IsFrameworkType(type))
                {
                    Type typeToUseForCBMBridge = GetTypeToUseForCBMBridge(type);
                    flag = s_cbmTdpBridge.HasProperty(typeToUseForCBMBridge, name, bindingAttr);
                }
                else
                {
                    Type reflectionType = GetReflectionType(type);
                    Type type4 = GetReflectionType(returnType);
                    Type[] reflectionTypes = GetReflectionTypes(types);
                    flag = reflectionType.GetProperty(name, bindingAttr, binder, type4, reflectionTypes, modifiers) != null;
                }
                if (flag)
                {
                    return type.GetProperty(name, bindingAttr, binder, returnType, types, modifiers);
                }
            }
            catch (AmbiguousMatchException)
            {
                if (throwAmbiguousMatchException)
                {
                    throw;
                }
                return GetMostSpecificProperty(type, name, bindingAttr);
            }
            return null;
        }

        private static Type GetReflectionType(Type type)
        {
            if (type == null)
            {
                return null;
            }
            TypeDescriptionProvider targetFrameworkProvider = GetTargetFrameworkProvider(type);
            if (targetFrameworkProvider != null)
            {
                return targetFrameworkProvider.GetReflectionType(type);
            }
            return type;
        }

        private static Type[] GetReflectionTypes(Type[] types)
        {
            if (types == null)
            {
                return null;
            }
            return (from t in types select GetReflectionType(t)).ToArray<Type>();
        }

        private static TypeDescriptionProvider GetTargetFrameworkProvider(object obj)
        {
            System.ComponentModel.Design.TypeDescriptionProviderService typeDescriptionProviderService = TypeDescriptionProviderService;
            if (typeDescriptionProviderService != null)
            {
                return typeDescriptionProviderService.GetProvider(obj);
            }
            return null;
        }

        private static TypeDescriptionProvider GetTargetFrameworkProvider(Type type)
        {
            System.ComponentModel.Design.TypeDescriptionProviderService typeDescriptionProviderService = TypeDescriptionProviderService;
            if (typeDescriptionProviderService != null)
            {
                return typeDescriptionProviderService.GetProvider(type);
            }
            return null;
        }

        private static ICustomTypeDescriptor GetTypeDescriptor(object obj)
        {
            TypeDescriptionProvider targetFrameworkProvider = GetTargetFrameworkProvider(obj);
            if (targetFrameworkProvider != null)
            {
                ICustomTypeDescriptor typeDescriptor = targetFrameworkProvider.GetTypeDescriptor(obj);
                if (typeDescriptor != null)
                {
                    return typeDescriptor;
                }
            }
            return null;
        }

        private static ICustomTypeDescriptor GetTypeDescriptor(Type type)
        {
            TypeDescriptionProvider targetFrameworkProvider = GetTargetFrameworkProvider(type);
            if (targetFrameworkProvider != null)
            {
                ICustomTypeDescriptor typeDescriptor = targetFrameworkProvider.GetTypeDescriptor(type);
                if (typeDescriptor != null)
                {
                    return typeDescriptor;
                }
            }
            return null;
        }

        private static Type GetTypeToUseForCBMBridge(Type type)
        {
            if (!type.IsGenericType)
            {
                return type;
            }
            return type.GetGenericTypeDefinition();
        }

        internal static bool HasMethod(Type type, string name, BindingFlags bindingAttr)
        {
            if ((s_cbmTdpBridge != null) && IsFrameworkType(type))
            {
                Type typeToUseForCBMBridge = GetTypeToUseForCBMBridge(type);
                return s_cbmTdpBridge.HasMethod(typeToUseForCBMBridge, name, bindingAttr);
            }
            return (GetReflectionType(type).GetMethod(name, bindingAttr) != null);
        }

        private static bool IsFrameworkType(Type type)
        {
            bool flag;
            if (!s_isFrameworkType.TryGetValue(type, out flag))
            {
                string str;
                flag = AssemblyResolver.GetPathToReferenceAssembly(type.Assembly, out str) != ReferenceAssemblyType.NonFrameworkAssembly;
                s_isFrameworkType.TryAdd(type, flag);
            }
            return flag;
        }

        internal static bool IsSupportedType(Type type)
        {
            TypeDescriptionProvider targetFrameworkProvider = GetTargetFrameworkProvider(type);
            if (targetFrameworkProvider == null)
            {
                targetFrameworkProvider = TypeDescriptor.GetProvider(type);
            }
            return targetFrameworkProvider.IsSupportedType(type);
        }

        private static Tuple<string, int> MakeTuple(string name, BindingFlags bindingAttr)
        {
            return new Tuple<string, int>(name, (int) bindingAttr);
        }

        internal static string TypeNameConverter(Type type)
        {
            string assemblyQualifiedName = null;
            if (type != null)
            {
                Type reflectionType = GetReflectionType(type);
                if (reflectionType != null)
                {
                    assemblyQualifiedName = reflectionType.AssemblyQualifiedName;
                }
            }
            return assemblyQualifiedName;
        }

        internal static ClientBuildManagerTypeDescriptionProviderBridge CBMTypeDescriptionProviderBridge
        {
            set
            {
                s_cbmTdpBridge = value;
            }
        }

        internal static IDesignerHost DesignerHost
        {
            [CompilerGenerated]
            get
            {
                return <DesignerHost>k__BackingField;
            }
            [CompilerGenerated]
            set
            {
                <DesignerHost>k__BackingField = value;
            }
        }

        private static bool SkipCache
        {
            get
            {
                return (s_cbmTdpBridge == null);
            }
        }

        private static System.ComponentModel.Design.TypeDescriptionProviderService TypeDescriptionProviderService
        {
            get
            {
                if (DesignerHost == null)
                {
                    return null;
                }
                return (DesignerHost.GetService(typeof(System.ComponentModel.Design.TypeDescriptionProviderService)) as System.ComponentModel.Design.TypeDescriptionProviderService);
            }
        }

        private class MemberCache
        {
            private ConcurrentDictionary<string, EventInfo> _events;
            private ConcurrentDictionary<Tuple<string, int>, FieldInfo> _fields;
            private ConcurrentDictionary<Tuple<string, int>, PropertyInfo> _properties;

            internal MemberCache()
            {
            }

            internal ConcurrentDictionary<string, EventInfo> Events
            {
                get
                {
                    if (this._events == null)
                    {
                        this._events = new ConcurrentDictionary<string, EventInfo>();
                    }
                    return this._events;
                }
            }

            internal ConcurrentDictionary<Tuple<string, int>, FieldInfo> Fields
            {
                get
                {
                    if (this._fields == null)
                    {
                        this._fields = new ConcurrentDictionary<Tuple<string, int>, FieldInfo>();
                    }
                    return this._fields;
                }
            }

            internal ConcurrentDictionary<Tuple<string, int>, PropertyInfo> Properties
            {
                get
                {
                    if (this._properties == null)
                    {
                        this._properties = new ConcurrentDictionary<Tuple<string, int>, PropertyInfo>();
                    }
                    return this._properties;
                }
            }
        }
    }
}

