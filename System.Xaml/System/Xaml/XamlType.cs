namespace System.Xaml
{
    using MS.Internal.Xaml.Parser;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Threading;
    using System.Windows.Markup;
    using System.Xaml.MS.Impl;
    using System.Xaml.Schema;

    public class XamlType : IEquatable<XamlType>
    {
        private ThreeValuedBool _isNameValid;
        private string _name;
        private ReadOnlyCollection<string> _namespaces;
        private TypeReflector _reflector;
        private XamlSchemaContext _schemaContext;
        private IList<XamlType> _typeArguments;
        [SecurityCritical]
        private NullableReference<Type> _underlyingType;

        public XamlType(Type underlyingType, XamlSchemaContext schemaContext) : this(underlyingType, schemaContext, null)
        {
        }

        protected XamlType(string typeName, IList<XamlType> typeArguments, XamlSchemaContext schemaContext)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            this._name = typeName;
            this._schemaContext = schemaContext;
            this._typeArguments = GetTypeArguments(typeArguments);
        }

        public XamlType(Type underlyingType, XamlSchemaContext schemaContext, XamlTypeInvoker invoker) : this(null, underlyingType, schemaContext, invoker, null)
        {
        }

        public XamlType(string unknownTypeNamespace, string unknownTypeName, IList<XamlType> typeArguments, XamlSchemaContext schemaContext)
        {
            if (unknownTypeNamespace == null)
            {
                throw new ArgumentNullException("unknownTypeNamespace");
            }
            if (unknownTypeName == null)
            {
                throw new ArgumentNullException("unknownTypeName");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            this._name = unknownTypeName;
            this._namespaces = new ReadOnlyCollection<string>(new string[] { unknownTypeNamespace });
            this._schemaContext = schemaContext;
            this._typeArguments = GetTypeArguments(typeArguments);
            this._reflector = TypeReflector.UnknownReflector;
        }

        [SecuritySafeCritical]
        internal XamlType(string alias, Type underlyingType, XamlSchemaContext schemaContext, XamlTypeInvoker invoker, TypeReflector reflector)
        {
            if (underlyingType == null)
            {
                throw new ArgumentNullException("underlyingType");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            this._reflector = reflector ?? new TypeReflector(underlyingType);
            this._name = alias ?? GetTypeName(underlyingType);
            this._schemaContext = schemaContext;
            this._typeArguments = GetTypeArguments(underlyingType, schemaContext);
            this._underlyingType.Value = underlyingType;
            this._reflector.Invoker = invoker;
        }

        private void AppendTypeName(StringBuilder sb, bool forceNsInitialization)
        {
            string preferredXamlNamespace = null;
            if (forceNsInitialization)
            {
                preferredXamlNamespace = this.PreferredXamlNamespace;
            }
            else if ((this._namespaces != null) && (this._namespaces.Count > 0))
            {
                preferredXamlNamespace = this._namespaces[0];
            }
            if (!string.IsNullOrEmpty(preferredXamlNamespace))
            {
                sb.Append("{");
                sb.Append(this.PreferredXamlNamespace);
                sb.Append("}");
            }
            else if (this.UnderlyingTypeInternal.Value != null)
            {
                sb.Append(this.UnderlyingTypeInternal.Value.Namespace);
                sb.Append(".");
            }
            sb.Append(this.Name);
            if (this.IsGeneric)
            {
                sb.Append("(");
                for (int i = 0; i < this.TypeArguments.Count; i++)
                {
                    this.TypeArguments[i].AppendTypeName(sb, forceNsInitialization);
                    if (i < (this.TypeArguments.Count - 1))
                    {
                        sb.Append(", ");
                    }
                }
                sb.Append(")");
            }
        }

        public virtual bool CanAssignTo(XamlType xamlType)
        {
            if (!object.ReferenceEquals(xamlType, null))
            {
                Type underlyingType = xamlType.UnderlyingType;
                XamlType baseType = this;
                do
                {
                    Type type3 = baseType.UnderlyingType;
                    if ((underlyingType != null) && (type3 != null))
                    {
                        if (type3.Assembly.ReflectionOnly && (underlyingType.Assembly == typeof(XamlType).Assembly))
                        {
                            return LooseTypeExtensions.IsAssemblyQualifiedNameAssignableFrom(underlyingType, type3);
                        }
                        return underlyingType.IsAssignableFrom(type3);
                    }
                    if (baseType == xamlType)
                    {
                        return true;
                    }
                    baseType = baseType.BaseType;
                }
                while (baseType != null);
            }
            return false;
        }

        private void CreateReflector()
        {
            TypeReflector unknownReflector;
            if (this.LookupIsUnknown())
            {
                unknownReflector = TypeReflector.UnknownReflector;
            }
            else
            {
                unknownReflector = new TypeReflector(this.UnderlyingType);
            }
            Interlocked.CompareExchange<TypeReflector>(ref this._reflector, unknownReflector, null);
        }

        private void EnsureReflector()
        {
            if (this._reflector == null)
            {
                this.CreateReflector();
            }
        }

        public override bool Equals(object obj)
        {
            XamlType type = obj as XamlType;
            return (this == type);
        }

        public bool Equals(XamlType other)
        {
            return (this == other);
        }

        public XamlMember GetAliasedProperty(XamlDirective directive)
        {
            XamlMember aliasedProperty;
            this.EnsureReflector();
            if (!this._reflector.TryGetAliasedProperty(directive, out aliasedProperty))
            {
                aliasedProperty = this.LookupAliasedProperty(directive);
                this._reflector.TryAddAliasedProperty(directive, aliasedProperty);
            }
            return aliasedProperty;
        }

        public ICollection<XamlMember> GetAllAttachableMembers()
        {
            this.EnsureReflector();
            if (!this._reflector.AttachableMembers.IsComplete)
            {
                IEnumerable<XamlMember> allAttachableMembers = this.LookupAllAttachableMembers();
                if (allAttachableMembers != null)
                {
                    foreach (XamlMember member in allAttachableMembers)
                    {
                        this._reflector.AttachableMembers.TryAdd(member.Name, member);
                    }
                }
                this._reflector.AttachableMembers.IsComplete = true;
            }
            return this._reflector.AttachableMembers.Values;
        }

        internal ICollection<XamlMember> GetAllExcludedReadOnlyMembers()
        {
            this.EnsureReflector();
            if (this._reflector.ExcludedReadOnlyMembers == null)
            {
                this._reflector.ExcludedReadOnlyMembers = this.LookupAllExcludedReadOnlyMembers() ?? EmptyList<XamlMember>.Value;
            }
            return this._reflector.ExcludedReadOnlyMembers;
        }

        public ICollection<XamlMember> GetAllMembers()
        {
            this.EnsureReflector();
            if (!this._reflector.Members.IsComplete)
            {
                IEnumerable<XamlMember> allMembers = this.LookupAllMembers();
                if (allMembers != null)
                {
                    foreach (XamlMember member in allMembers)
                    {
                        this._reflector.Members.TryAdd(member.Name, member);
                    }
                }
                this._reflector.Members.IsComplete = true;
            }
            return this._reflector.Members.Values;
        }

        public XamlMember GetAttachableMember(string name)
        {
            XamlMember attachableMember;
            this.EnsureReflector();
            if (!this._reflector.AttachableMembers.TryGetValue(name, out attachableMember) && !this._reflector.AttachableMembers.IsComplete)
            {
                attachableMember = this.LookupAttachableMember(name);
                attachableMember = this._reflector.AttachableMembers.TryAdd(name, attachableMember);
            }
            return attachableMember;
        }

        private XamlCollectionKind GetCollectionKind()
        {
            this.EnsureReflector();
            if (!this._reflector.CollectionKindIsSet)
            {
                this._reflector.CollectionKind = this.LookupCollectionKind();
            }
            return this._reflector.CollectionKind;
        }

        internal ConstructorInfo GetConstructor(Type[] paramTypes)
        {
            if (this.UnderlyingType == null)
            {
                return null;
            }
            IEnumerable<ConstructorInfo> constructors = this.GetConstructors();
            ConstructorInfo[] match = constructors as ConstructorInfo[];
            if (match == null)
            {
                match = new List<ConstructorInfo>(constructors).ToArray();
            }
            return (ConstructorInfo) Type.DefaultBinder.SelectMethod(this.ConstructorBindingFlags, match, paramTypes, null);
        }

        internal IEnumerable<ConstructorInfo> GetConstructors()
        {
            if (this.UnderlyingType == null)
            {
                return EmptyList<ConstructorInfo>.Value;
            }
            if (this.IsPublic)
            {
                return this.UnderlyingType.GetConstructors();
            }
            return this.GetPublicAndInternalConstructors();
        }

        private static bool GetDefaultFlag(BoolTypeBits flagBit)
        {
            return ((BoolTypeBits.Default & flagBit) == flagBit);
        }

        private bool GetFlag(BoolTypeBits flagBit)
        {
            this.EnsureReflector();
            bool? flag = this._reflector.GetFlag(flagBit);
            if (!flag.HasValue)
            {
                flag = new bool?(this.LookupBooleanValue(flagBit));
                this._reflector.SetFlag(flagBit, flag.Value);
            }
            return flag.Value;
        }

        public override int GetHashCode()
        {
            if (this.IsUnknown)
            {
                int num = this._name.GetHashCode() ^ this._namespaces[0].GetHashCode();
                if ((this._typeArguments != null) && (this._typeArguments.Count > 0))
                {
                    foreach (XamlType type in this._typeArguments)
                    {
                        num ^= type.GetHashCode();
                    }
                }
                return num;
            }
            if (this.UnderlyingType != null)
            {
                return (this.UnderlyingType.GetHashCode() ^ 8);
            }
            return base.GetHashCode();
        }

        public XamlMember GetMember(string name)
        {
            XamlMember member;
            this.EnsureReflector();
            if (!this._reflector.Members.TryGetValue(name, out member) && !this._reflector.Members.IsComplete)
            {
                member = this.LookupMember(name, false);
                member = this._reflector.Members.TryAdd(name, member);
            }
            return member;
        }

        public IList<XamlType> GetPositionalParameters(int parameterCount)
        {
            IList<XamlType> positionalParameters;
            this.EnsureReflector();
            if (!this._reflector.TryGetPositionalParameters(parameterCount, out positionalParameters))
            {
                positionalParameters = this.LookupPositionalParameters(parameterCount);
                positionalParameters = this._reflector.TryAddPositionalParameters(parameterCount, positionalParameters);
            }
            return positionalParameters;
        }

        private XamlMember GetPropertyOrUnknown(string propertyName, bool skipReadOnlyCheck)
        {
            XamlMember member = skipReadOnlyCheck ? this.LookupMember(propertyName, true) : this.GetMember(propertyName);
            if (member == null)
            {
                member = new XamlMember(propertyName, this, false);
            }
            return member;
        }

        private IEnumerable<ConstructorInfo> GetPublicAndInternalConstructors()
        {
            foreach (ConstructorInfo iteratorVariable0 in this.UnderlyingType.GetConstructors(this.ConstructorBindingFlags))
            {
                if (TypeReflector.IsPublicOrInternal(iteratorVariable0))
                {
                    yield return iteratorVariable0;
                }
            }
        }

        internal string GetQualifiedName()
        {
            StringBuilder sb = new StringBuilder();
            this.AppendTypeName(sb, true);
            return sb.ToString();
        }

        internal static ReadOnlyCollection<T> GetReadOnly<T>(IList<T> list)
        {
            if (list == null)
            {
                return null;
            }
            if (list.Count > 0)
            {
                return new ReadOnlyCollection<T>(list);
            }
            return EmptyList<T>.Value;
        }

        private static ReadOnlyCollection<XamlType> GetTypeArguments(IList<XamlType> typeArguments)
        {
            if ((typeArguments == null) || (typeArguments.Count == 0))
            {
                return null;
            }
            foreach (XamlType type in typeArguments)
            {
                if (type == null)
                {
                    throw new ArgumentException(System.Xaml.SR.Get("CollectionCannotContainNulls", new object[] { "typeArguments" }));
                }
            }
            return new List<XamlType>(typeArguments).AsReadOnly();
        }

        private static ReadOnlyCollection<XamlType> GetTypeArguments(Type type, XamlSchemaContext schemaContext)
        {
            Type elementType = type;
            while (elementType.IsArray)
            {
                elementType = elementType.GetElementType();
            }
            if (!elementType.IsGenericType)
            {
                return null;
            }
            Type[] genericArguments = elementType.GetGenericArguments();
            XamlType[] list = new XamlType[genericArguments.Length];
            for (int i = 0; i < genericArguments.Length; i++)
            {
                list[i] = schemaContext.GetXamlType(genericArguments[i]);
            }
            return GetReadOnly<XamlType>(list);
        }

        private static string GetTypeName(Type type)
        {
            string name = type.Name;
            int index = name.IndexOf('`');
            if (index >= 0)
            {
                string str2;
                name = GenericTypeNameScanner.StripSubscript(name, out str2).Substring(0, index) + str2;
            }
            if (type.IsNested)
            {
                name = GetTypeName(type.DeclaringType) + '+' + name;
            }
            return name;
        }

        public virtual IList<string> GetXamlNamespaces()
        {
            if (this._namespaces == null)
            {
                this._namespaces = this._schemaContext.GetXamlNamespaces(this);
                if (this._namespaces == null)
                {
                    this._namespaces = new ReadOnlyCollection<string>(new string[] { string.Empty });
                }
            }
            return this._namespaces;
        }

        private bool IsNullableGeneric()
        {
            if (this.UnderlyingType == null)
            {
                return false;
            }
            return ((KS.Eq(this.UnderlyingType.Name, "Nullable`1") && (this.UnderlyingType.Assembly == typeof(Nullable<>).Assembly)) && (this.UnderlyingType.Namespace == typeof(Nullable<>).Namespace));
        }

        internal bool IsVisibleTo(Assembly accessingAssembly)
        {
            if (this.IsPublic)
            {
                return true;
            }
            Type underlyingType = this.UnderlyingType;
            return (((accessingAssembly != null) && (underlyingType != null)) && TypeReflector.IsVisibleTo(underlyingType, accessingAssembly, this.SchemaContext));
        }

        protected virtual XamlMember LookupAliasedProperty(XamlDirective directive)
        {
            if (this.AreAttributesAvailable)
            {
                string str;
                Type attributeType = null;
                bool skipReadOnlyCheck = false;
                if (directive == XamlLanguage.Key)
                {
                    attributeType = typeof(DictionaryKeyPropertyAttribute);
                    skipReadOnlyCheck = true;
                }
                else if (directive == XamlLanguage.Name)
                {
                    attributeType = typeof(RuntimeNamePropertyAttribute);
                }
                else if (directive == XamlLanguage.Uid)
                {
                    attributeType = typeof(UidPropertyAttribute);
                }
                else if (directive == XamlLanguage.Lang)
                {
                    attributeType = typeof(XmlLangPropertyAttribute);
                }
                if ((attributeType != null) && this.TryGetAttributeString(attributeType, out str))
                {
                    if (string.IsNullOrEmpty(str))
                    {
                        return null;
                    }
                    return this.GetPropertyOrUnknown(str, skipReadOnlyCheck);
                }
            }
            if (this.BaseType != null)
            {
                return this.BaseType.GetAliasedProperty(directive);
            }
            return null;
        }

        protected virtual IEnumerable<XamlMember> LookupAllAttachableMembers()
        {
            if (this.UnderlyingType == null)
            {
                if (this.BaseType == null)
                {
                    return null;
                }
                return this.BaseType.GetAllAttachableMembers();
            }
            this.EnsureReflector();
            return this._reflector.LookupAllAttachableMembers(this.SchemaContext);
        }

        private ICollection<XamlMember> LookupAllExcludedReadOnlyMembers()
        {
            if (this.UnderlyingType == null)
            {
                return null;
            }
            this.GetAllMembers();
            IList<PropertyInfo> remainingProperties = this._reflector.LookupRemainingProperties();
            if (remainingProperties == null)
            {
                return null;
            }
            List<XamlMember> list = new List<XamlMember>(remainingProperties.Count);
            for (int i = 0; i < remainingProperties.Count; i++)
            {
                XamlMember item = new XamlMember(remainingProperties[i], this.SchemaContext);
                if (item.IsReadOnly && !item.Type.IsUsableAsReadOnly)
                {
                    list.Add(item);
                }
            }
            return new ReadOnlyCollection<XamlMember>(list);
        }

        protected virtual IEnumerable<XamlMember> LookupAllMembers()
        {
            ICollection<PropertyInfo> is2;
            ICollection<EventInfo> is3;
            List<XamlMember> list;
            if (this.UnderlyingType == null)
            {
                if (this.BaseType == null)
                {
                    return null;
                }
                return this.BaseType.GetAllMembers();
            }
            this.EnsureReflector();
            this._reflector.LookupAllMembers(out is2, out is3, out list);
            if (is2 != null)
            {
                foreach (PropertyInfo info in is2)
                {
                    XamlMember property = this.SchemaContext.GetProperty(info);
                    if (!property.IsReadOnly || property.Type.IsUsableAsReadOnly)
                    {
                        list.Add(property);
                    }
                }
            }
            if (is3 != null)
            {
                foreach (EventInfo info2 in is3)
                {
                    XamlMember item = this.SchemaContext.GetEvent(info2);
                    list.Add(item);
                }
            }
            return list;
        }

        protected virtual IList<XamlType> LookupAllowedContentTypes()
        {
            IList<XamlType> list = this.ContentWrappers ?? EmptyList<XamlType>.Value;
            List<XamlType> list2 = new List<XamlType>(list.Count + 1) {
                this.ItemType
            };
            foreach (XamlType type in list)
            {
                if ((type.ContentProperty != null) && !type.ContentProperty.IsUnknown)
                {
                    XamlType item = type.ContentProperty.Type;
                    if (!list2.Contains(item))
                    {
                        list2.Add(item);
                    }
                }
            }
            return list2.AsReadOnly();
        }

        private Dictionary<int, IList<XamlType>> LookupAllPositionalParameters()
        {
            if (this.UnderlyingType == XamlLanguage.Type.UnderlyingType)
            {
                Dictionary<int, IList<XamlType>> dictionary = new Dictionary<int, IList<XamlType>>();
                XamlType xamlType = this.SchemaContext.GetXamlType(typeof(Type));
                XamlType[] list = new XamlType[] { xamlType };
                dictionary.Add(1, GetReadOnly<XamlType>(list));
                return dictionary;
            }
            Dictionary<int, IList<XamlType>> dictionary2 = new Dictionary<int, IList<XamlType>>();
            foreach (ConstructorInfo info in this.GetConstructors())
            {
                ParameterInfo[] parameters = info.GetParameters();
                XamlType[] typeArray2 = new XamlType[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    ParameterInfo info2 = parameters[i];
                    Type parameterType = info2.ParameterType;
                    typeArray2[i] = this.SchemaContext.GetXamlType(parameterType);
                }
                if (dictionary2.ContainsKey(typeArray2.Length))
                {
                    if (!this.SchemaContext.SupportMarkupExtensionsWithDuplicateArity)
                    {
                        throw new XamlSchemaException(System.Xaml.SR.Get("MarkupExtensionWithDuplicateArity", new object[] { this.UnderlyingType, typeArray2.Length }));
                    }
                }
                else
                {
                    dictionary2.Add(typeArray2.Length, GetReadOnly<XamlType>(typeArray2));
                }
            }
            return dictionary2;
        }

        protected virtual XamlMember LookupAttachableMember(string name)
        {
            MethodInfo info;
            MethodInfo attachableEvent;
            if (this.UnderlyingType == null)
            {
                if (this.BaseType == null)
                {
                    return null;
                }
                return this.BaseType.GetAttachableMember(name);
            }
            this.EnsureReflector();
            if (this._reflector.LookupAttachableProperty(name, out info, out attachableEvent))
            {
                XamlMember member = this.SchemaContext.GetAttachableProperty(name, info, attachableEvent);
                if (member.IsReadOnly && !member.Type.IsUsableAsReadOnly)
                {
                    return null;
                }
                return member;
            }
            attachableEvent = this._reflector.LookupAttachableEvent(name);
            if (attachableEvent != null)
            {
                return this.SchemaContext.GetAttachableEvent(name, attachableEvent);
            }
            return null;
        }

        protected virtual XamlType LookupBaseType()
        {
            Type underlyingType = this.UnderlyingType;
            if (underlyingType == null)
            {
                return XamlLanguage.Object;
            }
            if (underlyingType.BaseType != null)
            {
                return this.SchemaContext.GetXamlType(underlyingType.BaseType);
            }
            return null;
        }

        private bool LookupBooleanValue(BoolTypeBits typeBit)
        {
            switch (typeBit)
            {
                case BoolTypeBits.NameScope:
                    return this.LookupIsNameScope();

                case BoolTypeBits.ConstructionRequiresArguments:
                    return this.LookupConstructionRequiresArguments();

                case BoolTypeBits.Constructible:
                    return this.LookupIsConstructible();

                case BoolTypeBits.XmlData:
                    return this.LookupIsXData();

                case BoolTypeBits.MarkupExtension:
                    return this.LookupIsMarkupExtension();

                case BoolTypeBits.Nullable:
                    return this.LookupIsNullable();

                case BoolTypeBits.Public:
                    return this.LookupIsPublic();

                case BoolTypeBits.TrimSurroundingWhitespace:
                    return this.LookupTrimSurroundingWhitespace();

                case BoolTypeBits.WhitespaceSignificantCollection:
                    return this.LookupIsWhitespaceSignificantCollection();

                case BoolTypeBits.UsableDuringInitialization:
                {
                    bool usableDuringInitialization = this.LookupUsableDuringInitialization();
                    if (usableDuringInitialization && this.IsMarkupExtension)
                    {
                        throw new XamlSchemaException(System.Xaml.SR.Get("UsableDuringInitializationOnME", new object[] { this }));
                    }
                    return usableDuringInitialization;
                }
                case BoolTypeBits.Ambient:
                    return this.LookupIsAmbient();
            }
            return GetDefaultFlag(typeBit);
        }

        protected virtual XamlCollectionKind LookupCollectionKind()
        {
            if (this.UnderlyingType == null)
            {
                if (this.BaseType == null)
                {
                    return XamlCollectionKind.None;
                }
                return this.BaseType.GetCollectionKind();
            }
            MethodInfo addMethod = null;
            XamlCollectionKind collectionKind = CollectionReflector.LookupCollectionKind(this.UnderlyingType, out addMethod);
            if (addMethod != null)
            {
                this._reflector.AddMethod = addMethod;
            }
            return collectionKind;
        }

        protected virtual bool LookupConstructionRequiresArguments()
        {
            Type underlyingType = this.UnderlyingType;
            if (underlyingType == null)
            {
                return GetDefaultFlag(BoolTypeBits.ConstructionRequiresArguments);
            }
            if (underlyingType.IsValueType)
            {
                return false;
            }
            ConstructorInfo method = underlyingType.GetConstructor(this.ConstructorBindingFlags, null, Type.EmptyTypes, null);
            if (method != null)
            {
                return !TypeReflector.IsPublicOrInternal(method);
            }
            return true;
        }

        protected virtual XamlMember LookupContentProperty()
        {
            string str;
            if (this.TryGetAttributeString(typeof(ContentPropertyAttribute), out str))
            {
                if (string.IsNullOrEmpty(str))
                {
                    return null;
                }
                return this.GetPropertyOrUnknown(str, false);
            }
            if (this.BaseType != null)
            {
                return this.BaseType.ContentProperty;
            }
            return null;
        }

        protected virtual IList<XamlType> LookupContentWrappers()
        {
            List<XamlType> list = null;
            if (this.AreAttributesAvailable)
            {
                List<Type> allAttributeContents = this._reflector.GetAllAttributeContents<Type>(typeof(ContentWrapperAttribute));
                if (allAttributeContents != null)
                {
                    list = new List<XamlType>(allAttributeContents.Count);
                    foreach (Type type in allAttributeContents)
                    {
                        list.Add(this.SchemaContext.GetXamlType(type));
                    }
                }
            }
            if (this.BaseType != null)
            {
                IList<XamlType> contentWrappers = this.BaseType.ContentWrappers;
                if (list == null)
                {
                    return contentWrappers;
                }
                if (contentWrappers != null)
                {
                    list.AddRange(contentWrappers);
                }
            }
            return GetReadOnly<XamlType>(list);
        }

        protected virtual ICustomAttributeProvider LookupCustomAttributeProvider()
        {
            return null;
        }

        protected virtual XamlValueConverter<XamlDeferringLoader> LookupDeferringLoader()
        {
            if (this.AreAttributesAvailable)
            {
                Type[] attributeTypes = this._reflector.GetAttributeTypes(typeof(XamlDeferLoadAttribute), 2);
                if (attributeTypes != null)
                {
                    return this.SchemaContext.GetValueConverter<XamlDeferringLoader>(attributeTypes[0], null);
                }
            }
            if (this.BaseType != null)
            {
                return this.BaseType.DeferringLoader;
            }
            return null;
        }

        protected virtual XamlTypeInvoker LookupInvoker()
        {
            if (this.UnderlyingType == null)
            {
                return null;
            }
            return new XamlTypeInvoker(this);
        }

        protected virtual bool LookupIsAmbient()
        {
            if (this.AreAttributesAvailable && this._reflector.IsAttributePresent(typeof(AmbientAttribute)))
            {
                return true;
            }
            if (this.BaseType != null)
            {
                return this.BaseType.IsAmbient;
            }
            if (this.IsUnknown)
            {
                return this._reflector.GetFlag(BoolTypeBits.Ambient).Value;
            }
            return GetDefaultFlag(BoolTypeBits.Ambient);
        }

        protected virtual bool LookupIsConstructible()
        {
            Type underlyingType = this.UnderlyingType;
            if (underlyingType == null)
            {
                return GetDefaultFlag(BoolTypeBits.Constructible);
            }
            if ((!underlyingType.IsAbstract && !underlyingType.IsInterface) && ((!underlyingType.IsNested && !underlyingType.IsGenericParameter) && !underlyingType.IsGenericTypeDefinition))
            {
                if (underlyingType.IsValueType)
                {
                    return true;
                }
                if (!this.ConstructionRequiresArguments)
                {
                    return true;
                }
                using (IEnumerator<ConstructorInfo> enumerator = this.GetConstructors().GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        ConstructorInfo current = enumerator.Current;
                        return true;
                    }
                }
            }
            return false;
        }

        protected virtual bool LookupIsMarkupExtension()
        {
            return this.CanAssignTo(XamlLanguage.MarkupExtension);
        }

        protected virtual bool LookupIsNameScope()
        {
            return this.CanAssignTo(XamlLanguage.INameScope);
        }

        protected virtual bool LookupIsNullable()
        {
            if (this.UnderlyingType == null)
            {
                return GetDefaultFlag(BoolTypeBits.Nullable);
            }
            if (this.UnderlyingType.IsValueType)
            {
                return this.IsNullableGeneric();
            }
            return true;
        }

        protected virtual bool LookupIsPublic()
        {
            Type underlyingType = this.UnderlyingType;
            if (underlyingType == null)
            {
                return GetDefaultFlag(BoolTypeBits.Public);
            }
            return underlyingType.IsVisible;
        }

        protected virtual bool LookupIsUnknown()
        {
            if (this._reflector != null)
            {
                return this._reflector.IsUnknown;
            }
            return (this.UnderlyingType == null);
        }

        protected virtual bool LookupIsWhitespaceSignificantCollection()
        {
            if (this.AreAttributesAvailable && this._reflector.IsAttributePresent(typeof(WhitespaceSignificantCollectionAttribute)))
            {
                return true;
            }
            if (this.BaseType != null)
            {
                return this.BaseType.IsWhitespaceSignificantCollection;
            }
            if (this.IsUnknown)
            {
                return this._reflector.GetFlag(BoolTypeBits.WhitespaceSignificantCollection).Value;
            }
            return GetDefaultFlag(BoolTypeBits.WhitespaceSignificantCollection);
        }

        protected virtual bool LookupIsXData()
        {
            return this.CanAssignTo(XamlLanguage.IXmlSerializable);
        }

        protected virtual XamlType LookupItemType()
        {
            Type parameterType = null;
            MethodInfo addMethod = this.AddMethod;
            if (addMethod != null)
            {
                ParameterInfo[] parameters = addMethod.GetParameters();
                if (parameters.Length == 2)
                {
                    parameterType = parameters[1].ParameterType;
                }
                else if (parameters.Length == 1)
                {
                    parameterType = parameters[0].ParameterType;
                }
            }
            else if (this.UnderlyingType != null)
            {
                if (this.UnderlyingType.IsArray)
                {
                    parameterType = this.UnderlyingType.GetElementType();
                }
            }
            else if (this.BaseType != null)
            {
                return this.BaseType.ItemType;
            }
            if (parameterType == null)
            {
                return null;
            }
            return this.SchemaContext.GetXamlType(parameterType);
        }

        protected virtual XamlType LookupKeyType()
        {
            MethodInfo addMethod = this.AddMethod;
            if (addMethod != null)
            {
                ParameterInfo[] parameters = addMethod.GetParameters();
                if (parameters.Length == 2)
                {
                    return this.SchemaContext.GetXamlType(parameters[0].ParameterType);
                }
            }
            else if ((this.UnderlyingType == null) && (this.BaseType != null))
            {
                return this.BaseType.KeyType;
            }
            return null;
        }

        protected virtual XamlType LookupMarkupExtensionReturnType()
        {
            if (this.AreAttributesAvailable)
            {
                Type attributeType = this._reflector.GetAttributeType(typeof(MarkupExtensionReturnTypeAttribute));
                if (attributeType != null)
                {
                    return this.SchemaContext.GetXamlType(attributeType);
                }
            }
            if (this.BaseType != null)
            {
                return this.BaseType.MarkupExtensionReturnType;
            }
            return null;
        }

        protected virtual XamlMember LookupMember(string name, bool skipReadOnlyCheck)
        {
            if (this.UnderlyingType == null)
            {
                if (this.BaseType == null)
                {
                    return null;
                }
                if (!skipReadOnlyCheck)
                {
                    return this.BaseType.GetMember(name);
                }
                return this.BaseType.LookupMember(name, true);
            }
            this.EnsureReflector();
            PropertyInfo property = this._reflector.LookupProperty(name);
            if (property != null)
            {
                XamlMember member = this.SchemaContext.GetProperty(property);
                if ((!skipReadOnlyCheck && member.IsReadOnly) && !member.Type.IsUsableAsReadOnly)
                {
                    return null;
                }
                return member;
            }
            EventInfo ei = this._reflector.LookupEvent(name);
            if (ei != null)
            {
                return this.SchemaContext.GetEvent(ei);
            }
            return null;
        }

        protected virtual IList<XamlType> LookupPositionalParameters(int parameterCount)
        {
            IList<XamlType> list;
            if (this.UnderlyingType == null)
            {
                return null;
            }
            this.EnsureReflector();
            if (this._reflector.ReflectedPositionalParameters == null)
            {
                this._reflector.ReflectedPositionalParameters = this.LookupAllPositionalParameters();
            }
            this._reflector.ReflectedPositionalParameters.TryGetValue(parameterCount, out list);
            return list;
        }

        protected virtual EventHandler<XamlSetMarkupExtensionEventArgs> LookupSetMarkupExtensionHandler()
        {
            string str;
            if ((this.UnderlyingType != null) && this.TryGetAttributeString(typeof(XamlSetMarkupExtensionAttribute), out str))
            {
                if (string.IsNullOrEmpty(str))
                {
                    return null;
                }
                return (EventHandler<XamlSetMarkupExtensionEventArgs>) SafeReflectionInvoker.CreateDelegate(typeof(EventHandler<XamlSetMarkupExtensionEventArgs>), this.UnderlyingType, str);
            }
            if (this.BaseType != null)
            {
                return this.BaseType.SetMarkupExtensionHandler;
            }
            return null;
        }

        protected virtual EventHandler<XamlSetTypeConverterEventArgs> LookupSetTypeConverterHandler()
        {
            string str;
            if ((this.UnderlyingType != null) && this.TryGetAttributeString(typeof(XamlSetTypeConverterAttribute), out str))
            {
                if (string.IsNullOrEmpty(str))
                {
                    return null;
                }
                return (EventHandler<XamlSetTypeConverterEventArgs>) SafeReflectionInvoker.CreateDelegate(typeof(EventHandler<XamlSetTypeConverterEventArgs>), this.UnderlyingType, str);
            }
            if (this.BaseType != null)
            {
                return this.BaseType.SetTypeConverterHandler;
            }
            return null;
        }

        protected virtual bool LookupTrimSurroundingWhitespace()
        {
            if (this.AreAttributesAvailable && this._reflector.IsAttributePresent(typeof(TrimSurroundingWhitespaceAttribute)))
            {
                return true;
            }
            if (this.BaseType != null)
            {
                return this.BaseType.TrimSurroundingWhitespace;
            }
            return GetDefaultFlag(BoolTypeBits.TrimSurroundingWhitespace);
        }

        protected virtual XamlValueConverter<System.ComponentModel.TypeConverter> LookupTypeConverter()
        {
            if (this.AreAttributesAvailable)
            {
                Type attributeType = this._reflector.GetAttributeType(typeof(TypeConverterAttribute));
                if (attributeType != null)
                {
                    return this.SchemaContext.GetValueConverter<System.ComponentModel.TypeConverter>(attributeType, null);
                }
            }
            if (this.BaseType != null)
            {
                XamlValueConverter<System.ComponentModel.TypeConverter> typeConverter = this.BaseType.TypeConverter;
                if ((typeConverter != null) && (typeConverter.TargetType != XamlLanguage.Object))
                {
                    return typeConverter;
                }
            }
            Type underlyingType = this.UnderlyingType;
            if (underlyingType != null)
            {
                if (underlyingType.IsEnum)
                {
                    return this.SchemaContext.GetValueConverter<System.ComponentModel.TypeConverter>(typeof(EnumConverter), this);
                }
                XamlValueConverter<System.ComponentModel.TypeConverter> converter2 = BuiltInValueConverter.GetTypeConverter(underlyingType);
                if (converter2 != null)
                {
                    return converter2;
                }
                if (this.IsNullableGeneric())
                {
                    Type[] genericArguments = underlyingType.GetGenericArguments();
                    return this.SchemaContext.GetXamlType(genericArguments[0]).TypeConverter;
                }
            }
            return null;
        }

        protected virtual Type LookupUnderlyingType()
        {
            return this.UnderlyingTypeInternal.Value;
        }

        protected virtual bool LookupUsableDuringInitialization()
        {
            if (this.AreAttributesAvailable)
            {
                bool? attributeValue = this._reflector.GetAttributeValue<bool>(typeof(UsableDuringInitializationAttribute));
                if (attributeValue.HasValue)
                {
                    return attributeValue.Value;
                }
            }
            if (this.BaseType != null)
            {
                return this.BaseType.IsUsableDuringInitialization;
            }
            return GetDefaultFlag(BoolTypeBits.UsableDuringInitialization);
        }

        protected virtual XamlValueConverter<System.Windows.Markup.ValueSerializer> LookupValueSerializer()
        {
            if (this.AreAttributesAvailable)
            {
                Type attributeType = this._reflector.GetAttributeType(typeof(ValueSerializerAttribute));
                if (attributeType != null)
                {
                    return this.SchemaContext.GetValueConverter<System.Windows.Markup.ValueSerializer>(attributeType, null);
                }
            }
            if (this.BaseType != null)
            {
                XamlValueConverter<System.Windows.Markup.ValueSerializer> valueSerializer = this.BaseType.ValueSerializer;
                if (valueSerializer != null)
                {
                    return valueSerializer;
                }
            }
            Type underlyingType = this.UnderlyingType;
            if (underlyingType != null)
            {
                XamlValueConverter<System.Windows.Markup.ValueSerializer> converter2 = BuiltInValueConverter.GetValueSerializer(underlyingType);
                if (converter2 != null)
                {
                    return converter2;
                }
                if (this.IsNullableGeneric())
                {
                    Type[] genericArguments = underlyingType.GetGenericArguments();
                    return this.SchemaContext.GetXamlType(genericArguments[0]).ValueSerializer;
                }
            }
            return null;
        }

        public static bool operator ==(XamlType xamlType1, XamlType xamlType2)
        {
            if (object.ReferenceEquals(xamlType1, xamlType2))
            {
                return true;
            }
            if (object.ReferenceEquals(xamlType1, null) || object.ReferenceEquals(xamlType2, null))
            {
                return false;
            }
            if (xamlType1.IsUnknown)
            {
                return ((xamlType2.IsUnknown && ((xamlType1._name == xamlType2._name) && (xamlType1._namespaces[0] == xamlType2._namespaces[0]))) && typeArgumentsAreEqual(xamlType1, xamlType2));
            }
            if (xamlType2.IsUnknown)
            {
                return false;
            }
            return (xamlType1.UnderlyingType == xamlType2.UnderlyingType);
        }

        public static bool operator !=(XamlType xamlType1, XamlType xamlType2)
        {
            return !(xamlType1 == xamlType2);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            this.AppendTypeName(sb, false);
            return sb.ToString();
        }

        private bool TryGetAttributeString(Type attributeType, out string result)
        {
            bool flag;
            if (!this.AreAttributesAvailable)
            {
                result = null;
                return false;
            }
            result = this._reflector.GetAttributeString(attributeType, out flag);
            if (!flag && (result == null))
            {
                XamlType baseType = this.BaseType;
                if (baseType != null)
                {
                    return baseType.TryGetAttributeString(attributeType, out result);
                }
            }
            return true;
        }

        private static bool typeArgumentsAreEqual(XamlType xamlType1, XamlType xamlType2)
        {
            if (!xamlType1.IsGeneric)
            {
                return !xamlType2.IsGeneric;
            }
            if (!xamlType2.IsGeneric)
            {
                return false;
            }
            if (xamlType1._typeArguments.Count != xamlType2._typeArguments.Count)
            {
                return false;
            }
            for (int i = 0; i < xamlType1._typeArguments.Count; i++)
            {
                if (xamlType1._typeArguments[i] != xamlType2._typeArguments[i])
                {
                    return false;
                }
            }
            return true;
        }

        internal MethodInfo AddMethod
        {
            get
            {
                if (this.UnderlyingType == null)
                {
                    return null;
                }
                this.EnsureReflector();
                if (!this._reflector.AddMethodIsSet)
                {
                    XamlCollectionKind collectionKind = this.GetCollectionKind();
                    this._reflector.AddMethod = CollectionReflector.LookupAddMethod(this.UnderlyingType, collectionKind);
                }
                return this._reflector.AddMethod;
            }
        }

        public IList<XamlType> AllowedContentTypes
        {
            get
            {
                XamlCollectionKind collectionKind = this.GetCollectionKind();
                if ((collectionKind != XamlCollectionKind.Collection) && (collectionKind != XamlCollectionKind.Dictionary))
                {
                    return null;
                }
                if (this._reflector.AllowedContentTypes == null)
                {
                    this._reflector.AllowedContentTypes = this.LookupAllowedContentTypes() ?? EmptyList<XamlType>.Value;
                }
                return this._reflector.AllowedContentTypes;
            }
        }

        private bool AreAttributesAvailable
        {
            get
            {
                this.EnsureReflector();
                if (!this._reflector.CustomAttributeProviderIsSet)
                {
                    this._reflector.CustomAttributeProvider = this.LookupCustomAttributeProvider();
                }
                if (this._reflector.CustomAttributeProvider == null)
                {
                    return (this.UnderlyingTypeInternal.Value != null);
                }
                return true;
            }
        }

        public XamlType BaseType
        {
            get
            {
                this.EnsureReflector();
                if (!this._reflector.BaseTypeIsSet)
                {
                    this._reflector.BaseType = this.LookupBaseType();
                }
                return this._reflector.BaseType;
            }
        }

        public bool ConstructionRequiresArguments
        {
            get
            {
                return this.GetFlag(BoolTypeBits.ConstructionRequiresArguments);
            }
        }

        private BindingFlags ConstructorBindingFlags
        {
            get
            {
                BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
                if (!this.IsPublic)
                {
                    flags |= BindingFlags.NonPublic;
                }
                return flags;
            }
        }

        public XamlMember ContentProperty
        {
            get
            {
                this.EnsureReflector();
                if (!this._reflector.ContentPropertyIsSet)
                {
                    this._reflector.ContentProperty = this.LookupContentProperty();
                }
                return this._reflector.ContentProperty;
            }
        }

        public IList<XamlType> ContentWrappers
        {
            get
            {
                if (!this.IsCollection)
                {
                    return null;
                }
                if (this._reflector.ContentWrappers == null)
                {
                    this._reflector.ContentWrappers = this.LookupContentWrappers() ?? EmptyList<XamlType>.Value;
                }
                return this._reflector.ContentWrappers;
            }
        }

        public XamlValueConverter<XamlDeferringLoader> DeferringLoader
        {
            get
            {
                this.EnsureReflector();
                if (!this._reflector.DeferringLoaderIsSet)
                {
                    this._reflector.DeferringLoader = this.LookupDeferringLoader();
                }
                return this._reflector.DeferringLoader;
            }
        }

        internal MethodInfo GetEnumeratorMethod
        {
            get
            {
                if ((this.GetCollectionKind() == XamlCollectionKind.None) || (this.UnderlyingType == null))
                {
                    return null;
                }
                if (!this._reflector.GetEnumeratorMethodIsSet)
                {
                    this._reflector.GetEnumeratorMethod = CollectionReflector.GetEnumeratorMethod(this.UnderlyingType);
                }
                return this._reflector.GetEnumeratorMethod;
            }
        }

        public XamlTypeInvoker Invoker
        {
            get
            {
                this.EnsureReflector();
                if (this._reflector.Invoker == null)
                {
                    this._reflector.Invoker = this.LookupInvoker() ?? XamlTypeInvoker.UnknownInvoker;
                }
                return this._reflector.Invoker;
            }
        }

        public bool IsAmbient
        {
            get
            {
                return this.GetFlag(BoolTypeBits.Ambient);
            }
        }

        public bool IsArray
        {
            get
            {
                return (this.GetCollectionKind() == XamlCollectionKind.Array);
            }
        }

        public bool IsCollection
        {
            get
            {
                return (this.GetCollectionKind() == XamlCollectionKind.Collection);
            }
        }

        public bool IsConstructible
        {
            get
            {
                return this.GetFlag(BoolTypeBits.Constructible);
            }
        }

        public bool IsDictionary
        {
            get
            {
                return (this.GetCollectionKind() == XamlCollectionKind.Dictionary);
            }
        }

        public bool IsGeneric
        {
            get
            {
                return (this.TypeArguments != null);
            }
        }

        public bool IsMarkupExtension
        {
            get
            {
                return this.GetFlag(BoolTypeBits.MarkupExtension);
            }
        }

        public bool IsNameScope
        {
            get
            {
                return this.GetFlag(BoolTypeBits.NameScope);
            }
        }

        public bool IsNameValid
        {
            get
            {
                if (this._isNameValid == ThreeValuedBool.NotSet)
                {
                    this._isNameValid = XamlName.IsValidXamlName(this._name) ? ThreeValuedBool.True : ThreeValuedBool.False;
                }
                return (this._isNameValid == ThreeValuedBool.True);
            }
        }

        public bool IsNullable
        {
            get
            {
                return this.GetFlag(BoolTypeBits.Nullable);
            }
        }

        public bool IsPublic
        {
            get
            {
                return this.GetFlag(BoolTypeBits.Public);
            }
        }

        internal MethodInfo IsReadOnlyMethod
        {
            get
            {
                if ((this.ItemType == null) || (this.UnderlyingType == null))
                {
                    return null;
                }
                if (!this._reflector.IsReadOnlyMethodIsSet)
                {
                    if ((this.UnderlyingType != null) && (this.ItemType.UnderlyingType != null))
                    {
                        this._reflector.IsReadOnlyMethod = CollectionReflector.GetIsReadOnlyMethod(this.UnderlyingType, this.ItemType.UnderlyingType);
                    }
                    else
                    {
                        this._reflector.IsReadOnlyMethod = null;
                    }
                }
                return this._reflector.IsReadOnlyMethod;
            }
        }

        public bool IsUnknown
        {
            get
            {
                this.EnsureReflector();
                return this._reflector.IsUnknown;
            }
        }

        internal bool IsUsableAsReadOnly
        {
            get
            {
                XamlCollectionKind collectionKind = this.GetCollectionKind();
                if ((collectionKind != XamlCollectionKind.Collection) && (collectionKind != XamlCollectionKind.Dictionary))
                {
                    return this.IsXData;
                }
                return true;
            }
        }

        public bool IsUsableDuringInitialization
        {
            get
            {
                return this.GetFlag(BoolTypeBits.UsableDuringInitialization);
            }
        }

        public bool IsWhitespaceSignificantCollection
        {
            get
            {
                return this.GetFlag(BoolTypeBits.WhitespaceSignificantCollection);
            }
        }

        public bool IsXData
        {
            get
            {
                return this.GetFlag(BoolTypeBits.XmlData);
            }
        }

        public XamlType ItemType
        {
            get
            {
                if (this.GetCollectionKind() == XamlCollectionKind.None)
                {
                    return null;
                }
                if (this._reflector.ItemType == null)
                {
                    this._reflector.ItemType = this.LookupItemType() ?? XamlLanguage.Object;
                }
                return this._reflector.ItemType;
            }
        }

        public XamlType KeyType
        {
            get
            {
                if (!this.IsDictionary)
                {
                    return null;
                }
                if (this._reflector.KeyType == null)
                {
                    this._reflector.KeyType = this.LookupKeyType() ?? XamlLanguage.Object;
                }
                return this._reflector.KeyType;
            }
        }

        public XamlType MarkupExtensionReturnType
        {
            get
            {
                if (!this.IsMarkupExtension)
                {
                    return null;
                }
                if (this._reflector.MarkupExtensionReturnType == null)
                {
                    this._reflector.MarkupExtensionReturnType = this.LookupMarkupExtensionReturnType() ?? XamlLanguage.Object;
                }
                return this._reflector.MarkupExtensionReturnType;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public string PreferredXamlNamespace
        {
            get
            {
                IList<string> xamlNamespaces = this.GetXamlNamespaces();
                if (xamlNamespaces.Count > 0)
                {
                    return xamlNamespaces[0];
                }
                return null;
            }
        }

        public XamlSchemaContext SchemaContext
        {
            get
            {
                return this._schemaContext;
            }
        }

        internal EventHandler<XamlSetMarkupExtensionEventArgs> SetMarkupExtensionHandler
        {
            get
            {
                if (!this._reflector.XamlSetMarkupExtensionHandlerIsSet)
                {
                    this._reflector.XamlSetMarkupExtensionHandler = this.LookupSetMarkupExtensionHandler();
                }
                return this._reflector.XamlSetMarkupExtensionHandler;
            }
        }

        internal EventHandler<XamlSetTypeConverterEventArgs> SetTypeConverterHandler
        {
            get
            {
                this.EnsureReflector();
                if (!this._reflector.XamlSetTypeConverterHandlerIsSet)
                {
                    this._reflector.XamlSetTypeConverterHandler = this.LookupSetTypeConverterHandler();
                }
                return this._reflector.XamlSetTypeConverterHandler;
            }
        }

        public bool TrimSurroundingWhitespace
        {
            get
            {
                return this.GetFlag(BoolTypeBits.TrimSurroundingWhitespace);
            }
        }

        public IList<XamlType> TypeArguments
        {
            get
            {
                return this._typeArguments;
            }
        }

        public XamlValueConverter<System.ComponentModel.TypeConverter> TypeConverter
        {
            get
            {
                this.EnsureReflector();
                if (!this._reflector.TypeConverterIsSet)
                {
                    this._reflector.TypeConverter = this.LookupTypeConverter();
                }
                return this._reflector.TypeConverter;
            }
        }

        public Type UnderlyingType
        {
            [SecuritySafeCritical]
            get
            {
                if (!this._underlyingType.IsSet)
                {
                    this._underlyingType.SetIfNull(this.LookupUnderlyingType());
                }
                return this._underlyingType.Value;
            }
        }

        internal NullableReference<Type> UnderlyingTypeInternal
        {
            [SecuritySafeCritical]
            get
            {
                return this._underlyingType;
            }
        }

        public XamlValueConverter<System.Windows.Markup.ValueSerializer> ValueSerializer
        {
            get
            {
                this.EnsureReflector();
                if (!this._reflector.ValueSerializerIsSet)
                {
                    this._reflector.ValueSerializer = this.LookupValueSerializer();
                }
                return this._reflector.ValueSerializer;
            }
        }


        internal static class EmptyList<T>
        {
            public static readonly ReadOnlyCollection<T> Value;

            static EmptyList()
            {
                XamlType.EmptyList<T>.Value = new ReadOnlyCollection<T>(new T[0]);
            }
        }
    }
}

