namespace System.Xaml.Schema
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Markup;
    using System.Xaml;
    using System.Xaml.MS.Impl;

    internal class TypeReflector : Reflector
    {
        private NullableReference<MethodInfo> _addMethod;
        private ConcurrentDictionary<XamlDirective, XamlMember> _aliasedProperties;
        private ThreadSafeDictionary<string, XamlMember> _attachableMemberCache;
        private NullableReference<XamlType> _baseType;
        private int _boolTypeBits;
        private XamlCollectionKind _collectionKind;
        private NullableReference<XamlMember> _contentProperty;
        private NullableReference<XamlValueConverter<XamlDeferringLoader>> _deferringLoader;
        private NullableReference<XamlMember> _dictionaryKeyProperty;
        private NullableReference<MethodInfo> _getEnumeratorMethod;
        private NullableReference<MethodInfo> _isReadOnlyMethod;
        private ThreadSafeDictionary<string, XamlMember> _nonAttachableMemberCache;
        private ThreadSafeDictionary<int, IList<XamlType>> _positionalParameterTypes;
        private NullableReference<XamlMember> _runtimeNameProperty;
        private NullableReference<XamlValueConverter<System.ComponentModel.TypeConverter>> _typeConverter;
        private NullableReference<XamlMember> _uidProperty;
        private NullableReference<XamlValueConverter<System.Windows.Markup.ValueSerializer>> _valueSerializer;
        private NullableReference<EventHandler<XamlSetMarkupExtensionEventArgs>> _xamlSetMarkupExtensionHandler;
        private NullableReference<EventHandler<XamlSetTypeConverterEventArgs>> _xamlSetTypeConverterHandler;
        private NullableReference<XamlMember> _xmlLangProperty;
        private const BindingFlags AllProperties_BF = (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        private const BindingFlags AttachableProperties_BF = (BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        private static TypeReflector s_UnknownReflector;
        private const XamlCollectionKind XamlCollectionKindInvalid = ((XamlCollectionKind) 0xff);

        private TypeReflector()
        {
            this._nonAttachableMemberCache = new ThreadSafeDictionary<string, XamlMember>();
            this._nonAttachableMemberCache.IsComplete = true;
            this._attachableMemberCache = new ThreadSafeDictionary<string, XamlMember>();
            this._attachableMemberCache.IsComplete = true;
            this._baseType.Value = XamlLanguage.Object;
            this._boolTypeBits = -57015;
            this._collectionKind = XamlCollectionKind.None;
            this._addMethod.Value = null;
            this._contentProperty.Value = null;
            this._deferringLoader.Value = null;
            this._dictionaryKeyProperty.Value = null;
            this._getEnumeratorMethod.Value = null;
            this._isReadOnlyMethod.Value = null;
            this._runtimeNameProperty.Value = null;
            this._typeConverter.Value = null;
            this._uidProperty.Value = null;
            this._valueSerializer.Value = null;
            this._xamlSetMarkupExtensionHandler.Value = null;
            this._xamlSetTypeConverterHandler.Value = null;
            this._xmlLangProperty.Value = null;
            base.CustomAttributeProvider = null;
            this.Invoker = XamlTypeInvoker.UnknownInvoker;
        }

        public TypeReflector(Type underlyingType)
        {
            this.UnderlyingType = underlyingType;
            this._collectionKind = (XamlCollectionKind) 0xff;
        }

        private static void AddToMultiDict(Dictionary<string, List<MethodInfo>> dict, string name, MethodInfo value, bool isUnderlyingTypePublic)
        {
            List<MethodInfo> list;
            if (dict.TryGetValue(name, out list))
            {
                if (!isUnderlyingTypePublic)
                {
                    list.Add(value);
                }
                else if (value.IsPublic)
                {
                    if (!list[0].IsPublic)
                    {
                        list.Clear();
                    }
                    list.Add(value);
                }
                else if (!list[0].IsPublic)
                {
                    list.Add(value);
                }
            }
            else
            {
                list = new List<MethodInfo>();
                dict.Add(name, list);
                list.Add(value);
            }
        }

        private ICollection<EventInfo> FilterEvents(EventInfo[] eventList, List<XamlMember> knownMembers)
        {
            Dictionary<string, EventInfo> dictionary = new Dictionary<string, EventInfo>(eventList.Length);
            for (int i = 0; i < eventList.Length; i++)
            {
                XamlMember member;
                EventInfo info = eventList[i];
                if (this._nonAttachableMemberCache.TryGetValue(info.Name, out member))
                {
                    if (member != null)
                    {
                        knownMembers.Add(member);
                    }
                }
                else
                {
                    EventInfo info2;
                    if (dictionary.TryGetValue(info.Name, out info2))
                    {
                        if (info2.DeclaringType.IsAssignableFrom(info.DeclaringType))
                        {
                            dictionary[info.Name] = info;
                        }
                    }
                    else
                    {
                        dictionary.Add(info.Name, info);
                    }
                }
            }
            if (dictionary.Count == 0)
            {
                return null;
            }
            List<EventInfo> list = new List<EventInfo>(dictionary.Count);
            foreach (EventInfo info3 in dictionary.Values)
            {
                if (!IsPrivate(info3))
                {
                    list.Add(info3);
                }
            }
            return list;
        }

        private IList<PropertyInfo> FilterProperties(PropertyInfo[] propList, List<XamlMember> knownMembers, bool skipKnownNegatives)
        {
            Dictionary<string, PropertyInfo> dictionary = new Dictionary<string, PropertyInfo>(propList.Length);
            for (int i = 0; i < propList.Length; i++)
            {
                PropertyInfo info = propList[i];
                if (info.GetIndexParameters().Length <= 0)
                {
                    XamlMember member;
                    PropertyInfo info2;
                    if (this._nonAttachableMemberCache.TryGetValue(info.Name, out member))
                    {
                        if (member != null)
                        {
                            if (knownMembers != null)
                            {
                                knownMembers.Add(member);
                            }
                            continue;
                        }
                        if (skipKnownNegatives)
                        {
                            continue;
                        }
                    }
                    if (dictionary.TryGetValue(info.Name, out info2))
                    {
                        if (info2.DeclaringType.IsAssignableFrom(info.DeclaringType))
                        {
                            dictionary[info.Name] = info;
                        }
                    }
                    else
                    {
                        dictionary.Add(info.Name, info);
                    }
                }
            }
            if (dictionary.Count == 0)
            {
                return null;
            }
            List<PropertyInfo> list = new List<PropertyInfo>(dictionary.Count);
            foreach (PropertyInfo info3 in dictionary.Values)
            {
                if (!IsPrivate(info3))
                {
                    list.Add(info3);
                }
            }
            return list;
        }

        private static object GetCustomAttribute(Type attrType, Type reflectedType)
        {
            object[] customAttributes = reflectedType.GetCustomAttributes(attrType, true);
            if (customAttributes.Length == 0)
            {
                return null;
            }
            if (customAttributes.Length > 1)
            {
                throw new XamlSchemaException(System.Xaml.SR.Get("TooManyAttributesOnType", new object[] { reflectedType.Name, attrType.Name }));
            }
            return customAttributes[0];
        }

        internal bool? GetFlag(BoolTypeBits typeBit)
        {
            return Reflector.GetFlag(this._boolTypeBits, (int) typeBit);
        }

        private PropertyInfo GetNonIndexerProperty(string name)
        {
            PropertyInfo info = null;
            foreach (PropertyInfo info2 in this.UnderlyingType.GetMember(name, MemberTypes.Property, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                if ((info2.GetIndexParameters().Length == 0) && ((info == null) || info.DeclaringType.IsAssignableFrom(info2.DeclaringType)))
                {
                    info = info2;
                }
            }
            return info;
        }

        private void GetOrCreateAttachableEvents(XamlSchemaContext schemaContext, List<XamlMember> result, Dictionary<string, List<MethodInfo>> adders)
        {
            foreach (KeyValuePair<string, List<MethodInfo>> pair in adders)
            {
                string key = pair.Key;
                XamlMember attachableEvent = null;
                if (!this._attachableMemberCache.TryGetValue(key, out attachableEvent))
                {
                    MethodInfo adder = this.PickAttachableEventAdder(pair.Value);
                    attachableEvent = schemaContext.GetAttachableEvent(key, adder);
                }
                if (attachableEvent != null)
                {
                    result.Add(attachableEvent);
                }
            }
        }

        private void GetOrCreateAttachableProperties(XamlSchemaContext schemaContext, List<XamlMember> result, Dictionary<string, List<MethodInfo>> getters, Dictionary<string, List<MethodInfo>> setters)
        {
            foreach (KeyValuePair<string, List<MethodInfo>> pair in setters)
            {
                string key = pair.Key;
                XamlMember member = null;
                if (!this._attachableMemberCache.TryGetValue(key, out member))
                {
                    List<MethodInfo> list;
                    MethodInfo info;
                    MethodInfo info2;
                    getters.TryGetValue(key, out list);
                    getters.Remove(key);
                    this.PickAttachablePropertyAccessors(list, pair.Value, out info, out info2);
                    member = schemaContext.GetAttachableProperty(key, info, info2);
                    if (member.IsReadOnly && !member.Type.IsUsableAsReadOnly)
                    {
                        member = null;
                    }
                }
                if (member != null)
                {
                    result.Add(member);
                }
            }
            foreach (KeyValuePair<string, List<MethodInfo>> pair2 in getters)
            {
                string name = pair2.Key;
                XamlMember member2 = null;
                if (!this._attachableMemberCache.TryGetValue(name, out member2))
                {
                    member2 = schemaContext.GetAttachableProperty(name, pair2.Value[0], null);
                }
                result.Add(member2);
            }
        }

        private static TypeVisibility GetVisibility(Type type)
        {
            bool flag = false;
            while (type.IsNested)
            {
                if (type.IsNestedAssembly || type.IsNestedFamORAssem)
                {
                    flag = true;
                }
                else if (!type.IsNestedPublic)
                {
                    return TypeVisibility.NotVisible;
                }
                type = type.DeclaringType;
            }
            if (!type.IsNotPublic && !flag)
            {
                return TypeVisibility.Public;
            }
            return TypeVisibility.Internal;
        }

        private bool IsAttachableEventAdder(MethodInfo mi)
        {
            ParameterInfo[] parameters = mi.GetParameters();
            return ((parameters.Length == 2) && typeof(Delegate).IsAssignableFrom(parameters[1].ParameterType));
        }

        private bool IsAttachableEventAdder(MethodInfo mi, out string name)
        {
            name = null;
            if (!KS.StartsWith(mi.Name, "Add") || !KS.EndsWith(mi.Name, "Handler"))
            {
                return false;
            }
            if (!this.IsAttachableEventAdder(mi))
            {
                return false;
            }
            name = mi.Name.Substring("Add".Length, (mi.Name.Length - "Add".Length) - "Handler".Length);
            return true;
        }

        private bool IsAttachablePropertyAccessor(bool isEvent, bool isGetter, MethodInfo accessor)
        {
            if (isEvent)
            {
                return this.IsAttachableEventAdder(accessor);
            }
            if (isGetter)
            {
                return this.IsAttachablePropertyGetter(accessor);
            }
            return this.IsAttachablePropertySetter(accessor);
        }

        private bool IsAttachablePropertyGetter(MethodInfo mi)
        {
            return ((mi.GetParameters().Length == 1) && (mi.ReturnType != typeof(void)));
        }

        private bool IsAttachablePropertyGetter(MethodInfo mi, out string name)
        {
            name = null;
            if (!KS.StartsWith(mi.Name, "Get"))
            {
                return false;
            }
            if (!this.IsAttachablePropertyGetter(mi))
            {
                return false;
            }
            name = mi.Name.Substring("Get".Length);
            return true;
        }

        private bool IsAttachablePropertySetter(MethodInfo mi)
        {
            return (mi.GetParameters().Length == 2);
        }

        private bool IsAttachablePropertySetter(MethodInfo mi, out string name)
        {
            name = null;
            if (!KS.StartsWith(mi.Name, "Set"))
            {
                return false;
            }
            if (!this.IsAttachablePropertySetter(mi))
            {
                return false;
            }
            name = mi.Name.Substring("Set".Length);
            return true;
        }

        internal static bool IsInternal(Type type)
        {
            return (GetVisibility(type) == TypeVisibility.Internal);
        }

        private static bool IsPrivate(EventInfo ei)
        {
            return IsPrivateOrNull(ei.GetAddMethod(true));
        }

        private static bool IsPrivate(PropertyInfo pi)
        {
            return (IsPrivateOrNull(pi.GetGetMethod(true)) && IsPrivateOrNull(pi.GetSetMethod(true)));
        }

        private static bool IsPrivateOrNull(MethodInfo mi)
        {
            if (mi != null)
            {
                return mi.IsPrivate;
            }
            return true;
        }

        internal static bool IsPublicOrInternal(MethodBase method)
        {
            if (!method.IsPublic && !method.IsAssembly)
            {
                return method.IsFamilyOrAssembly;
            }
            return true;
        }

        internal static bool IsVisibleTo(Type type, Assembly accessingAssembly, XamlSchemaContext schemaContext)
        {
            TypeVisibility visibility = GetVisibility(type);
            if (visibility == TypeVisibility.NotVisible)
            {
                return false;
            }
            if ((visibility == TypeVisibility.Internal) && !schemaContext.AreInternalsVisibleTo(type.Assembly, accessingAssembly))
            {
                return false;
            }
            if (type.IsGenericType)
            {
                foreach (Type type2 in type.GetGenericArguments())
                {
                    if (!IsVisibleTo(type2, accessingAssembly, schemaContext))
                    {
                        return false;
                    }
                }
            }
            else if (type.HasElementType)
            {
                return IsVisibleTo(type.GetElementType(), accessingAssembly, schemaContext);
            }
            return true;
        }

        internal IList<XamlMember> LookupAllAttachableMembers(XamlSchemaContext schemaContext)
        {
            Dictionary<string, List<MethodInfo>> dictionary;
            Dictionary<string, List<MethodInfo>> dictionary2;
            Dictionary<string, List<MethodInfo>> dictionary3;
            List<XamlMember> result = new List<XamlMember>();
            this.LookupAllStaticAccessors(out dictionary, out dictionary2, out dictionary3);
            this.GetOrCreateAttachableProperties(schemaContext, result, dictionary, dictionary2);
            this.GetOrCreateAttachableEvents(schemaContext, result, dictionary3);
            return result;
        }

        internal void LookupAllMembers(out ICollection<PropertyInfo> newProperties, out ICollection<EventInfo> newEvents, out List<XamlMember> knownMembers)
        {
            PropertyInfo[] properties = this.UnderlyingType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            EventInfo[] events = this.UnderlyingType.GetEvents(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            knownMembers = new List<XamlMember>(properties.Length + events.Length);
            newProperties = this.FilterProperties(properties, knownMembers, true);
            newEvents = this.FilterEvents(events, knownMembers);
        }

        private void LookupAllStaticAccessors(out Dictionary<string, List<MethodInfo>> getters, out Dictionary<string, List<MethodInfo>> setters, out Dictionary<string, List<MethodInfo>> adders)
        {
            getters = new Dictionary<string, List<MethodInfo>>();
            setters = new Dictionary<string, List<MethodInfo>>();
            adders = new Dictionary<string, List<MethodInfo>>();
            MethodInfo[] methods = this.UnderlyingType.GetMethods(BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            if (this.UnderlyingType.IsVisible)
            {
                this.LookupAllStaticAccessorsHelper(methods, getters, setters, adders, true);
            }
            else
            {
                this.LookupAllStaticAccessorsHelper(methods, getters, setters, adders, false);
            }
        }

        private void LookupAllStaticAccessorsHelper(MethodInfo[] allMethods, Dictionary<string, List<MethodInfo>> getters, Dictionary<string, List<MethodInfo>> setters, Dictionary<string, List<MethodInfo>> adders, bool isUnderlyingTypePublic)
        {
            foreach (MethodInfo info in allMethods)
            {
                if (!info.IsPrivate)
                {
                    string str;
                    if (this.IsAttachablePropertyGetter(info, out str))
                    {
                        AddToMultiDict(getters, str, info, isUnderlyingTypePublic);
                    }
                    else if (this.IsAttachablePropertySetter(info, out str))
                    {
                        AddToMultiDict(setters, str, info, isUnderlyingTypePublic);
                    }
                    else if (this.IsAttachableEventAdder(info, out str))
                    {
                        AddToMultiDict(adders, str, info, isUnderlyingTypePublic);
                    }
                }
            }
        }

        internal MethodInfo LookupAttachableEvent(string name)
        {
            List<MethodInfo> staticAdders = this.LookupStaticAdders(name);
            if ((staticAdders != null) && (staticAdders.Count != 0))
            {
                return this.PickAttachableEventAdder(staticAdders);
            }
            return null;
        }

        internal bool LookupAttachableProperty(string name, out MethodInfo getter, out MethodInfo setter)
        {
            List<MethodInfo> staticSetters = this.LookupStaticSetters(name);
            List<MethodInfo> staticGetters = this.LookupStaticGetters(name);
            if (((staticSetters == null) || (staticSetters.Count == 0)) && ((staticGetters == null) || (staticGetters.Count == 0)))
            {
                getter = null;
                setter = null;
                return false;
            }
            this.PickAttachablePropertyAccessors(staticGetters, staticSetters, out getter, out setter);
            return true;
        }

        internal EventInfo LookupEvent(string name)
        {
            EventInfo ei = this.UnderlyingType.GetEvent(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if ((ei != null) && IsPrivate(ei))
            {
                ei = null;
            }
            return ei;
        }

        internal static XamlMember LookupNameScopeProperty(XamlType xamlType)
        {
            if (xamlType.UnderlyingType == null)
            {
                return null;
            }
            NameScopePropertyAttribute customAttribute = GetCustomAttribute(typeof(NameScopePropertyAttribute), xamlType.UnderlyingType) as NameScopePropertyAttribute;
            if (customAttribute == null)
            {
                return null;
            }
            Type type = customAttribute.Type;
            string name = customAttribute.Name;
            if (type != null)
            {
                return xamlType.SchemaContext.GetXamlType(type).GetAttachableMember(name);
            }
            return xamlType.GetMember(name);
        }

        internal PropertyInfo LookupProperty(string name)
        {
            PropertyInfo nonIndexerProperty = this.GetNonIndexerProperty(name);
            if ((nonIndexerProperty != null) && IsPrivate(nonIndexerProperty))
            {
                nonIndexerProperty = null;
            }
            return nonIndexerProperty;
        }

        internal IList<PropertyInfo> LookupRemainingProperties()
        {
            PropertyInfo[] properties = this.UnderlyingType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            return this.FilterProperties(properties, null, false);
        }

        private List<MethodInfo> LookupStaticAdders(string name)
        {
            List<MethodInfo> list;
            List<MethodInfo> list2;
            string str = "Add" + name + "Handler";
            MemberInfo[] accessors = this.UnderlyingType.GetMember(str, MemberTypes.Method, BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            this.PrioritizeAccessors(accessors, true, false, out list, out list2);
            return (list ?? list2);
        }

        private List<MethodInfo> LookupStaticGetters(string name)
        {
            List<MethodInfo> list;
            List<MethodInfo> list2;
            MemberInfo[] accessors = this.UnderlyingType.GetMember("Get" + name, MemberTypes.Method, BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            this.PrioritizeAccessors(accessors, false, true, out list, out list2);
            return (list ?? list2);
        }

        private List<MethodInfo> LookupStaticSetters(string name)
        {
            List<MethodInfo> list;
            List<MethodInfo> list2;
            MemberInfo[] accessors = this.UnderlyingType.GetMember("Set" + name, MemberTypes.Method, BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            this.PrioritizeAccessors(accessors, false, false, out list, out list2);
            return (list ?? list2);
        }

        private MethodInfo PickAttachableEventAdder(IEnumerable<MethodInfo> adders)
        {
            if (adders != null)
            {
                foreach (MethodInfo info in adders)
                {
                    if (!info.IsPrivate)
                    {
                        return info;
                    }
                }
            }
            return null;
        }

        private void PickAttachablePropertyAccessors(List<MethodInfo> getters, List<MethodInfo> setters, out MethodInfo getter, out MethodInfo setter)
        {
            List<KeyValuePair<MethodInfo, MethodInfo>> list = new List<KeyValuePair<MethodInfo, MethodInfo>>();
            if ((setters != null) && (getters != null))
            {
                foreach (MethodInfo info in setters)
                {
                    foreach (MethodInfo info2 in getters)
                    {
                        ParameterInfo[] parameters = info2.GetParameters();
                        ParameterInfo[] infoArray2 = info.GetParameters();
                        if ((parameters[0].ParameterType == infoArray2[0].ParameterType) && (info2.ReturnType == infoArray2[1].ParameterType))
                        {
                            list.Add(new KeyValuePair<MethodInfo, MethodInfo>(info2, info));
                        }
                    }
                }
            }
            if (list.Count > 0)
            {
                KeyValuePair<MethodInfo, MethodInfo> pair = list[0];
                getter = pair.Key;
                KeyValuePair<MethodInfo, MethodInfo> pair2 = list[0];
                setter = pair2.Value;
            }
            else if (((setters == null) || (setters.Count == 0)) || ((((getters != null) && (getters.Count > 0)) && (this.UnderlyingType.IsVisible && getters[0].IsPublic)) && !setters[0].IsPublic))
            {
                getter = getters[0];
                setter = null;
            }
            else
            {
                getter = null;
                setter = setters[0];
            }
        }

        private void PrioritizeAccessors(MemberInfo[] accessors, bool isEvent, bool isGetter, out List<MethodInfo> preferredAccessors, out List<MethodInfo> otherAccessors)
        {
            preferredAccessors = null;
            otherAccessors = null;
            if (this.UnderlyingType.IsVisible)
            {
                foreach (MethodInfo info in accessors)
                {
                    if (info.IsPublic && this.IsAttachablePropertyAccessor(isEvent, isGetter, info))
                    {
                        if (preferredAccessors == null)
                        {
                            preferredAccessors = new List<MethodInfo>();
                        }
                        preferredAccessors.Add(info);
                    }
                    else if (!info.IsPrivate && this.IsAttachablePropertyAccessor(isEvent, isGetter, info))
                    {
                        if (otherAccessors == null)
                        {
                            otherAccessors = new List<MethodInfo>();
                        }
                        otherAccessors.Add(info);
                    }
                }
            }
            else
            {
                foreach (MethodInfo info2 in accessors)
                {
                    if (!info2.IsPrivate && this.IsAttachablePropertyAccessor(isEvent, isGetter, info2))
                    {
                        if (preferredAccessors == null)
                        {
                            preferredAccessors = new List<MethodInfo>();
                        }
                        preferredAccessors.Add(info2);
                    }
                }
            }
        }

        internal void SetFlag(BoolTypeBits typeBit, bool value)
        {
            Reflector.SetFlag(ref this._boolTypeBits, (int) typeBit, value);
        }

        internal void TryAddAliasedProperty(XamlDirective directive, XamlMember member)
        {
            if (directive == XamlLanguage.Key)
            {
                this._dictionaryKeyProperty.Value = member;
            }
            else if (directive == XamlLanguage.Name)
            {
                this._runtimeNameProperty.Value = member;
            }
            else if (directive == XamlLanguage.Uid)
            {
                this._uidProperty.Value = member;
            }
            else if (directive == XamlLanguage.Lang)
            {
                this._xmlLangProperty.Value = member;
            }
            else
            {
                if (this._aliasedProperties == null)
                {
                    ConcurrentDictionary<XamlDirective, XamlMember> dictionary = XamlSchemaContext.CreateDictionary<XamlDirective, XamlMember>();
                    Interlocked.CompareExchange<ConcurrentDictionary<XamlDirective, XamlMember>>(ref this._aliasedProperties, dictionary, null);
                }
                this._aliasedProperties.TryAdd(directive, member);
            }
        }

        internal IList<XamlType> TryAddPositionalParameters(int paramCount, IList<XamlType> paramList)
        {
            return this._positionalParameterTypes.TryAdd(paramCount, paramList);
        }

        internal bool TryGetAliasedProperty(XamlDirective directive, out XamlMember member)
        {
            member = null;
            if (this.IsUnknown)
            {
                return true;
            }
            bool isSet = false;
            if (directive == XamlLanguage.Key)
            {
                isSet = this._dictionaryKeyProperty.IsSet;
                member = this._dictionaryKeyProperty.Value;
                return isSet;
            }
            if (directive == XamlLanguage.Name)
            {
                isSet = this._runtimeNameProperty.IsSet;
                member = this._runtimeNameProperty.Value;
                return isSet;
            }
            if (directive == XamlLanguage.Uid)
            {
                isSet = this._uidProperty.IsSet;
                member = this._uidProperty.Value;
                return isSet;
            }
            if (directive == XamlLanguage.Lang)
            {
                isSet = this._xmlLangProperty.IsSet;
                member = this._xmlLangProperty.Value;
                return isSet;
            }
            if (this._aliasedProperties != null)
            {
                isSet = this._aliasedProperties.TryGetValue(directive, out member);
            }
            return isSet;
        }

        internal bool TryGetPositionalParameters(int paramCount, out IList<XamlType> result)
        {
            result = null;
            if (this._positionalParameterTypes == null)
            {
                if (this.IsUnknown)
                {
                    return true;
                }
                Interlocked.CompareExchange<ThreadSafeDictionary<int, IList<XamlType>>>(ref this._positionalParameterTypes, new ThreadSafeDictionary<int, IList<XamlType>>(), null);
            }
            return this._positionalParameterTypes.TryGetValue(paramCount, out result);
        }

        internal MethodInfo AddMethod
        {
            get
            {
                return this._addMethod.Value;
            }
            set
            {
                this._addMethod.Value = value;
            }
        }

        internal bool AddMethodIsSet
        {
            get
            {
                return this._addMethod.IsSet;
            }
        }

        internal IList<XamlType> AllowedContentTypes { get; set; }

        internal ThreadSafeDictionary<string, XamlMember> AttachableMembers
        {
            get
            {
                if (this._attachableMemberCache == null)
                {
                    Interlocked.CompareExchange<ThreadSafeDictionary<string, XamlMember>>(ref this._attachableMemberCache, new ThreadSafeDictionary<string, XamlMember>(), null);
                }
                return this._attachableMemberCache;
            }
        }

        internal XamlType BaseType
        {
            get
            {
                return this._baseType.Value;
            }
            set
            {
                this._baseType.Value = value;
            }
        }

        internal bool BaseTypeIsSet
        {
            get
            {
                return this._baseType.IsSet;
            }
        }

        internal XamlCollectionKind CollectionKind
        {
            get
            {
                return this._collectionKind;
            }
            set
            {
                this._collectionKind = value;
            }
        }

        internal bool CollectionKindIsSet
        {
            get
            {
                return (this._collectionKind != ((XamlCollectionKind) 0xff));
            }
        }

        internal XamlMember ContentProperty
        {
            get
            {
                return this._contentProperty.Value;
            }
            set
            {
                this._contentProperty.Value = value;
            }
        }

        internal bool ContentPropertyIsSet
        {
            get
            {
                return this._contentProperty.IsSet;
            }
        }

        internal IList<XamlType> ContentWrappers { get; set; }

        internal XamlValueConverter<XamlDeferringLoader> DeferringLoader
        {
            get
            {
                return this._deferringLoader.Value;
            }
            set
            {
                this._deferringLoader.Value = value;
            }
        }

        internal bool DeferringLoaderIsSet
        {
            get
            {
                return this._deferringLoader.IsSet;
            }
        }

        internal ICollection<XamlMember> ExcludedReadOnlyMembers { get; set; }

        internal MethodInfo GetEnumeratorMethod
        {
            get
            {
                return this._getEnumeratorMethod.Value;
            }
            set
            {
                this._getEnumeratorMethod.Value = value;
            }
        }

        internal bool GetEnumeratorMethodIsSet
        {
            get
            {
                return this._getEnumeratorMethod.IsSet;
            }
        }

        internal XamlTypeInvoker Invoker { get; set; }

        internal MethodInfo IsReadOnlyMethod
        {
            get
            {
                return this._isReadOnlyMethod.Value;
            }
            set
            {
                this._isReadOnlyMethod.Value = value;
            }
        }

        internal bool IsReadOnlyMethodIsSet
        {
            get
            {
                return this._isReadOnlyMethod.IsSet;
            }
        }

        internal bool IsUnknown
        {
            get
            {
                return ((this._boolTypeBits & 0x100) != 0);
            }
        }

        internal XamlType ItemType { get; set; }

        internal XamlType KeyType { get; set; }

        internal XamlType MarkupExtensionReturnType { get; set; }

        protected override MemberInfo Member
        {
            get
            {
                return this.UnderlyingType;
            }
        }

        internal ThreadSafeDictionary<string, XamlMember> Members
        {
            get
            {
                if (this._nonAttachableMemberCache == null)
                {
                    Interlocked.CompareExchange<ThreadSafeDictionary<string, XamlMember>>(ref this._nonAttachableMemberCache, new ThreadSafeDictionary<string, XamlMember>(), null);
                }
                return this._nonAttachableMemberCache;
            }
        }

        internal Dictionary<int, IList<XamlType>> ReflectedPositionalParameters { get; set; }

        internal XamlValueConverter<System.ComponentModel.TypeConverter> TypeConverter
        {
            get
            {
                return this._typeConverter.Value;
            }
            set
            {
                this._typeConverter.Value = value;
            }
        }

        internal bool TypeConverterIsSet
        {
            get
            {
                return this._typeConverter.IsSet;
            }
        }

        internal Type UnderlyingType { get; set; }

        internal static TypeReflector UnknownReflector
        {
            get
            {
                if (s_UnknownReflector == null)
                {
                    s_UnknownReflector = new TypeReflector();
                }
                return s_UnknownReflector;
            }
        }

        internal XamlValueConverter<System.Windows.Markup.ValueSerializer> ValueSerializer
        {
            get
            {
                return this._valueSerializer.Value;
            }
            set
            {
                this._valueSerializer.Value = value;
            }
        }

        internal bool ValueSerializerIsSet
        {
            get
            {
                return this._valueSerializer.IsSet;
            }
        }

        internal EventHandler<XamlSetMarkupExtensionEventArgs> XamlSetMarkupExtensionHandler
        {
            get
            {
                return this._xamlSetMarkupExtensionHandler.Value;
            }
            set
            {
                this._xamlSetMarkupExtensionHandler.Value = value;
            }
        }

        internal bool XamlSetMarkupExtensionHandlerIsSet
        {
            get
            {
                return this._xamlSetMarkupExtensionHandler.IsSet;
            }
        }

        internal EventHandler<XamlSetTypeConverterEventArgs> XamlSetTypeConverterHandler
        {
            get
            {
                return this._xamlSetTypeConverterHandler.Value;
            }
            set
            {
                this._xamlSetTypeConverterHandler.Value = value;
            }
        }

        internal bool XamlSetTypeConverterHandlerIsSet
        {
            get
            {
                return this._xamlSetTypeConverterHandler.IsSet;
            }
        }

        internal class ThreadSafeDictionary<K, V> : Dictionary<K, V> where V: class
        {
            private bool _isComplete;

            internal ThreadSafeDictionary()
            {
            }

            private void SetComplete()
            {
                List<K> list = null;
                foreach (KeyValuePair<K, V> pair in this)
                {
                    if (pair.Value == null)
                    {
                        if (list == null)
                        {
                            list = new List<K>();
                        }
                        list.Add(pair.Key);
                    }
                }
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        base.Remove(list[i]);
                    }
                }
                this._isComplete = true;
            }

            public V TryAdd(K name, V member)
            {
                lock (((TypeReflector.ThreadSafeDictionary<K, V>) this))
                {
                    V local;
                    if (!base.TryGetValue(name, out local))
                    {
                        if (!this.IsComplete)
                        {
                            base.Add(name, member);
                        }
                        local = member;
                    }
                    return local;
                }
            }

            public bool TryGetValue(K name, out V member)
            {
                lock (((TypeReflector.ThreadSafeDictionary<K, V>) this))
                {
                    return base.TryGetValue(name, out member);
                }
            }

            public bool IsComplete
            {
                get
                {
                    return this._isComplete;
                }
                set
                {
                    lock (((TypeReflector.ThreadSafeDictionary<K, V>) this))
                    {
                        this.SetComplete();
                    }
                }
            }
        }

        private enum TypeVisibility
        {
            NotVisible,
            Internal,
            Public
        }
    }
}

