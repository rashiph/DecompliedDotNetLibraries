namespace System.Xaml
{
    using MS.Internal.Xaml.Parser;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Security;
    using System.Threading;
    using System.Windows.Markup;
    using System.Xaml.Schema;

    public class XamlMember : IEquatable<XamlMember>
    {
        private XamlType _declaringType;
        private ThreeValuedBool _isNameValid;
        private MemberType _memberType;
        private string _name;
        private MemberReflector _reflector;
        [SecurityCritical]
        private NullableReference<MemberInfo> _underlyingMember;

        public XamlMember(EventInfo eventInfo, XamlSchemaContext schemaContext) : this(eventInfo, schemaContext, null)
        {
        }

        public XamlMember(PropertyInfo propertyInfo, XamlSchemaContext schemaContext) : this(propertyInfo, schemaContext, null)
        {
        }

        internal XamlMember(string name, MemberReflector reflector)
        {
            this._name = name;
            this._declaringType = null;
            this._reflector = reflector ?? MemberReflector.UnknownReflector;
            this._memberType = MemberType.Directive;
        }

        public XamlMember(EventInfo eventInfo, XamlSchemaContext schemaContext, XamlMemberInvoker invoker) : this(eventInfo, schemaContext, invoker, new MemberReflector(true))
        {
        }

        public XamlMember(PropertyInfo propertyInfo, XamlSchemaContext schemaContext, XamlMemberInvoker invoker) : this(propertyInfo, schemaContext, invoker, new MemberReflector(false))
        {
        }

        public XamlMember(string attachableEventName, MethodInfo adder, XamlSchemaContext schemaContext) : this(attachableEventName, adder, schemaContext, null)
        {
        }

        public XamlMember(string name, XamlType declaringType, bool isAttachable)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (declaringType == null)
            {
                throw new ArgumentNullException("declaringType");
            }
            this._name = name;
            this._declaringType = declaringType;
            this._memberType = isAttachable ? MemberType.Attachable : MemberType.Instance;
        }

        [SecuritySafeCritical]
        internal XamlMember(EventInfo eventInfo, XamlSchemaContext schemaContext, XamlMemberInvoker invoker, MemberReflector reflector)
        {
            if (eventInfo == null)
            {
                throw new ArgumentNullException("eventInfo");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            this._name = eventInfo.Name;
            this._declaringType = schemaContext.GetXamlType(eventInfo.DeclaringType);
            this._memberType = MemberType.Instance;
            this._reflector = reflector;
            this._reflector.Invoker = invoker;
            this._underlyingMember.Value = eventInfo;
        }

        [SecuritySafeCritical]
        internal XamlMember(PropertyInfo propertyInfo, XamlSchemaContext schemaContext, XamlMemberInvoker invoker, MemberReflector reflector)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentNullException("propertyInfo");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            this._name = propertyInfo.Name;
            this._declaringType = schemaContext.GetXamlType(propertyInfo.DeclaringType);
            this._memberType = MemberType.Instance;
            this._reflector = reflector;
            this._reflector.Invoker = invoker;
            this._underlyingMember.Value = propertyInfo;
        }

        public XamlMember(string attachablePropertyName, MethodInfo getter, MethodInfo setter, XamlSchemaContext schemaContext) : this(attachablePropertyName, getter, setter, schemaContext, null)
        {
        }

        public XamlMember(string attachableEventName, MethodInfo adder, XamlSchemaContext schemaContext, XamlMemberInvoker invoker) : this(attachableEventName, adder, schemaContext, invoker, new MemberReflector(null, adder, true))
        {
        }

        public XamlMember(string attachablePropertyName, MethodInfo getter, MethodInfo setter, XamlSchemaContext schemaContext, XamlMemberInvoker invoker) : this(attachablePropertyName, getter, setter, schemaContext, invoker, new MemberReflector(getter, setter, false))
        {
        }

        [SecuritySafeCritical]
        internal XamlMember(string attachableEventName, MethodInfo adder, XamlSchemaContext schemaContext, XamlMemberInvoker invoker, MemberReflector reflector)
        {
            if (attachableEventName == null)
            {
                throw new ArgumentNullException("attachableEventName");
            }
            if (adder == null)
            {
                throw new ArgumentNullException("adder");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            ValidateSetter(adder, "adder");
            this._name = attachableEventName;
            this._declaringType = schemaContext.GetXamlType(adder.DeclaringType);
            this._reflector = reflector;
            this._memberType = MemberType.Attachable;
            this._reflector.Invoker = invoker;
            this._underlyingMember.Value = adder;
        }

        [SecuritySafeCritical]
        internal XamlMember(string attachablePropertyName, MethodInfo getter, MethodInfo setter, XamlSchemaContext schemaContext, XamlMemberInvoker invoker, MemberReflector reflector)
        {
            if (attachablePropertyName == null)
            {
                throw new ArgumentNullException("attachablePropertyName");
            }
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            MethodInfo info = getter ?? setter;
            if (info == null)
            {
                throw new ArgumentNullException(System.Xaml.SR.Get("GetterOrSetterRequired"), null);
            }
            ValidateGetter(getter, "getter");
            ValidateSetter(setter, "setter");
            this._name = attachablePropertyName;
            this._declaringType = schemaContext.GetXamlType(info.DeclaringType);
            this._reflector = reflector;
            this._memberType = MemberType.Attachable;
            this._reflector.Invoker = invoker;
            this._underlyingMember.Value = getter ?? setter;
        }

        private void CreateReflector()
        {
            MemberReflector reflector = this.LookupIsUnknown() ? MemberReflector.UnknownReflector : new MemberReflector();
            Interlocked.CompareExchange<MemberReflector>(ref this._reflector, reflector, null);
        }

        private void EnsureDefaultValue()
        {
            this.EnsureReflector();
            if (!this._reflector.DefaultValueIsSet)
            {
                DefaultValueAttribute attribute = null;
                if (this.AreAttributesAvailable)
                {
                    ICustomAttributeProvider provider = this._reflector.CustomAttributeProvider ?? this.UnderlyingMember;
                    object[] customAttributes = provider.GetCustomAttributes(typeof(DefaultValueAttribute), true);
                    if (customAttributes.Length > 0)
                    {
                        attribute = (DefaultValueAttribute) customAttributes[0];
                    }
                }
                if (attribute != null)
                {
                    this._reflector.DefaultValue = attribute.Value;
                }
                else
                {
                    this._reflector.DefaultValueIsNotPresent = true;
                }
            }
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
            XamlMember member = obj as XamlMember;
            return (this == member);
        }

        public bool Equals(XamlMember other)
        {
            return (this == other);
        }

        private static bool GetDefaultFlag(BoolMemberBits flagBit)
        {
            return ((BoolMemberBits.Default & flagBit) == flagBit);
        }

        private bool GetFlag(BoolMemberBits flagBit)
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
            return (((int) (((this.Name == null) ? MemberType.Instance : this.Name.GetHashCode()) ^ this._memberType)) ^ this.DeclaringType.GetHashCode());
        }

        public virtual IList<string> GetXamlNamespaces()
        {
            return this.DeclaringType.GetXamlNamespaces();
        }

        internal bool IsReadVisibleTo(Assembly accessingAssembly, System.Type accessingType)
        {
            if (!this.IsReadPublicIgnoringType)
            {
                MethodInfo getter = this.Getter;
                if (getter == null)
                {
                    return false;
                }
                if (!MemberReflector.GenericArgumentsAreVisibleTo(getter, accessingAssembly, this.SchemaContext))
                {
                    return false;
                }
                if (!MemberReflector.IsInternalVisibleTo(getter, accessingAssembly, this.SchemaContext))
                {
                    return MemberReflector.IsProtectedVisibleTo(getter, accessingType, this.SchemaContext);
                }
            }
            return true;
        }

        internal bool IsWriteVisibleTo(Assembly accessingAssembly, System.Type accessingType)
        {
            if (!this.IsWritePublicIgnoringType)
            {
                MethodInfo setter = this.Setter;
                if (setter == null)
                {
                    return false;
                }
                if (!MemberReflector.GenericArgumentsAreVisibleTo(setter, accessingAssembly, this.SchemaContext))
                {
                    return false;
                }
                if (!MemberReflector.IsInternalVisibleTo(setter, accessingAssembly, this.SchemaContext))
                {
                    return MemberReflector.IsProtectedVisibleTo(setter, accessingType, this.SchemaContext);
                }
            }
            return true;
        }

        private bool LookupBooleanValue(BoolMemberBits flag)
        {
            switch (flag)
            {
                case BoolMemberBits.ReadPublic:
                    return this.LookupIsReadPublic();

                case BoolMemberBits.WritePublic:
                    return this.LookupIsWritePublic();

                case BoolMemberBits.ReadOnly:
                    return this.LookupIsReadOnly();

                case BoolMemberBits.WriteOnly:
                    return this.LookupIsWriteOnly();

                case BoolMemberBits.Event:
                    return this.LookupIsEvent();

                case BoolMemberBits.Ambient:
                    return this.LookupIsAmbient();
            }
            return GetDefaultFlag(flag);
        }

        private string LookupConstructorArgument()
        {
            string attributeString = null;
            if (this.AreAttributesAvailable)
            {
                bool flag;
                attributeString = this._reflector.GetAttributeString(typeof(ConstructorArgumentAttribute), out flag);
            }
            return attributeString;
        }

        protected virtual ICustomAttributeProvider LookupCustomAttributeProvider()
        {
            return null;
        }

        protected virtual XamlValueConverter<XamlDeferringLoader> LookupDeferringLoader()
        {
            if (this.AreAttributesAvailable)
            {
                System.Type[] attributeTypes = this._reflector.GetAttributeTypes(typeof(XamlDeferLoadAttribute), 2);
                if (attributeTypes != null)
                {
                    return this.SchemaContext.GetValueConverter<XamlDeferringLoader>(attributeTypes[0], null);
                }
            }
            if (this.Type != null)
            {
                return this.Type.DeferringLoader;
            }
            return null;
        }

        protected virtual IList<XamlMember> LookupDependsOn()
        {
            if (!this.AreAttributesAvailable)
            {
                return null;
            }
            List<string> allAttributeContents = this._reflector.GetAllAttributeContents<string>(typeof(DependsOnAttribute));
            if ((allAttributeContents == null) || (allAttributeContents.Count == 0))
            {
                return null;
            }
            List<XamlMember> list = new List<XamlMember>();
            foreach (string str in allAttributeContents)
            {
                XamlMember item = this._declaringType.GetMember(str);
                if (item != null)
                {
                    list.Add(item);
                }
            }
            return XamlType.GetReadOnly<XamlMember>(list);
        }

        protected virtual XamlMemberInvoker LookupInvoker()
        {
            if (this.UnderlyingMember != null)
            {
                return new XamlMemberInvoker(this);
            }
            return null;
        }

        protected virtual bool LookupIsAmbient()
        {
            if (this.AreAttributesAvailable)
            {
                return this._reflector.IsAttributePresent(typeof(AmbientAttribute));
            }
            return GetDefaultFlag(BoolMemberBits.Ambient);
        }

        protected virtual bool LookupIsEvent()
        {
            return (this.UnderlyingMember is EventInfo);
        }

        protected virtual bool LookupIsReadOnly()
        {
            if (this.UnderlyingMember != null)
            {
                return (this.Setter == null);
            }
            return GetDefaultFlag(BoolMemberBits.ReadOnly);
        }

        protected virtual bool LookupIsReadPublic()
        {
            MethodInfo getter = this.Getter;
            if ((getter != null) && !getter.IsPublic)
            {
                return false;
            }
            return !this.IsWriteOnly;
        }

        protected virtual bool LookupIsUnknown()
        {
            if (this._reflector != null)
            {
                return this._reflector.IsUnknown;
            }
            return (this.UnderlyingMember == null);
        }

        protected virtual bool LookupIsWriteOnly()
        {
            if (this.UnderlyingMember != null)
            {
                return (this.Getter == null);
            }
            return GetDefaultFlag(BoolMemberBits.WriteOnly);
        }

        protected virtual bool LookupIsWritePublic()
        {
            MethodInfo setter = this.Setter;
            if ((setter != null) && !setter.IsPublic)
            {
                return false;
            }
            return !this.IsReadOnly;
        }

        private DesignerSerializationVisibility? LookupSerializationVisibility()
        {
            DesignerSerializationVisibility? attributeValue = null;
            if (this.AreAttributesAvailable)
            {
                attributeValue = this._reflector.GetAttributeValue<DesignerSerializationVisibility>(typeof(DesignerSerializationVisibilityAttribute));
            }
            return attributeValue;
        }

        private System.Type LookupSystemType()
        {
            MemberInfo underlyingMember = this.UnderlyingMember;
            PropertyInfo info2 = underlyingMember as PropertyInfo;
            if (info2 != null)
            {
                return info2.PropertyType;
            }
            EventInfo info3 = underlyingMember as EventInfo;
            if (info3 != null)
            {
                return info3.EventHandlerType;
            }
            MethodInfo info4 = underlyingMember as MethodInfo;
            if (info4 != null)
            {
                if ((info4.ReturnType != null) && (info4.ReturnType != typeof(void)))
                {
                    return info4.ReturnType;
                }
                ParameterInfo[] parameters = info4.GetParameters();
                if (parameters.Length == 2)
                {
                    return parameters[1].ParameterType;
                }
            }
            return null;
        }

        protected virtual XamlType LookupTargetType()
        {
            if (!this.IsAttachable)
            {
                return this._declaringType;
            }
            MethodInfo underlyingMember = this.UnderlyingMember as MethodInfo;
            if (underlyingMember != null)
            {
                ParameterInfo[] parameters = underlyingMember.GetParameters();
                if (parameters.Length > 0)
                {
                    System.Type parameterType = parameters[0].ParameterType;
                    return this.SchemaContext.GetXamlType(parameterType);
                }
            }
            return XamlLanguage.Object;
        }

        protected virtual XamlType LookupType()
        {
            System.Type systemType = this.LookupSystemType();
            if (systemType == null)
            {
                return null;
            }
            return this.SchemaContext.GetXamlType(systemType);
        }

        protected virtual XamlValueConverter<System.ComponentModel.TypeConverter> LookupTypeConverter()
        {
            XamlValueConverter<System.ComponentModel.TypeConverter> valueConverter = null;
            if (this.AreAttributesAvailable)
            {
                System.Type attributeType = this._reflector.GetAttributeType(typeof(TypeConverterAttribute));
                if (attributeType != null)
                {
                    valueConverter = this.SchemaContext.GetValueConverter<System.ComponentModel.TypeConverter>(attributeType, null);
                }
            }
            if ((valueConverter == null) && (this.Type != null))
            {
                valueConverter = this.Type.TypeConverter;
            }
            return valueConverter;
        }

        protected virtual MethodInfo LookupUnderlyingGetter()
        {
            this.EnsureReflector();
            if (this._reflector.Getter != null)
            {
                return this._reflector.Getter;
            }
            PropertyInfo underlyingMember = this.UnderlyingMember as PropertyInfo;
            if (underlyingMember == null)
            {
                return null;
            }
            return underlyingMember.GetGetMethod(true);
        }

        protected virtual MemberInfo LookupUnderlyingMember()
        {
            return this.UnderlyingMemberInternal.Value;
        }

        protected virtual MethodInfo LookupUnderlyingSetter()
        {
            this.EnsureReflector();
            if (this._reflector.Setter != null)
            {
                return this._reflector.Setter;
            }
            PropertyInfo underlyingMember = this.UnderlyingMember as PropertyInfo;
            if (underlyingMember != null)
            {
                return underlyingMember.GetSetMethod(true);
            }
            EventInfo info2 = this.UnderlyingMember as EventInfo;
            if (info2 == null)
            {
                return null;
            }
            return info2.GetAddMethod(true);
        }

        protected virtual XamlValueConverter<System.Windows.Markup.ValueSerializer> LookupValueSerializer()
        {
            XamlValueConverter<System.Windows.Markup.ValueSerializer> valueConverter = null;
            if (this.AreAttributesAvailable)
            {
                System.Type attributeType = this._reflector.GetAttributeType(typeof(ValueSerializerAttribute));
                if (attributeType != null)
                {
                    valueConverter = this.SchemaContext.GetValueConverter<System.Windows.Markup.ValueSerializer>(attributeType, null);
                }
            }
            if ((valueConverter == null) && (this.Type != null))
            {
                valueConverter = this.Type.ValueSerializer;
            }
            return valueConverter;
        }

        public static bool operator ==(XamlMember xamlMember1, XamlMember xamlMember2)
        {
            if (object.ReferenceEquals(xamlMember1, xamlMember2))
            {
                return true;
            }
            if (object.ReferenceEquals(xamlMember1, null) || object.ReferenceEquals(xamlMember2, null))
            {
                return false;
            }
            if ((xamlMember1._memberType != xamlMember2._memberType) || (xamlMember1.Name != xamlMember2.Name))
            {
                return false;
            }
            if (xamlMember1.IsDirective)
            {
                return XamlDirective.NamespacesAreEqual((XamlDirective) xamlMember1, (XamlDirective) xamlMember2);
            }
            return ((xamlMember1.DeclaringType == xamlMember2.DeclaringType) && (xamlMember1.IsUnknown == xamlMember2.IsUnknown));
        }

        public static bool operator !=(XamlMember xamlMember1, XamlMember xamlMember2)
        {
            return !(xamlMember1 == xamlMember2);
        }

        public override string ToString()
        {
            return (this._declaringType.ToString() + "." + this.Name);
        }

        private static void ValidateGetter(MethodInfo method, string argumentName)
        {
            if ((method != null) && ((method.GetParameters().Length != 1) || (method.ReturnType == typeof(void))))
            {
                throw new ArgumentException(System.Xaml.SR.Get("IncorrectGetterParamNum"), argumentName);
            }
        }

        private static void ValidateSetter(MethodInfo method, string argumentName)
        {
            if ((method != null) && (method.GetParameters().Length != 2))
            {
                throw new ArgumentException(System.Xaml.SR.Get("IncorrectSetterParamNum"), argumentName);
            }
        }

        private bool AreAttributesAvailable
        {
            get
            {
                this.EnsureReflector();
                if (!this._reflector.CustomAttributeProviderIsSetVolatile)
                {
                    ICustomAttributeProvider customAttributeProvider = this.LookupCustomAttributeProvider();
                    if (customAttributeProvider == null)
                    {
                        this._reflector.UnderlyingMember = this.UnderlyingMember;
                    }
                    this._reflector.SetCustomAttributeProviderVolatile(customAttributeProvider);
                }
                if (this._reflector.CustomAttributeProvider == null)
                {
                    return (this.UnderlyingMemberInternal.Value != null);
                }
                return true;
            }
        }

        internal string ConstructorArgument
        {
            get
            {
                this.EnsureReflector();
                if (!this._reflector.ConstructorArgumentIsSet)
                {
                    this._reflector.ConstructorArgument = this.LookupConstructorArgument();
                }
                return this._reflector.ConstructorArgument;
            }
        }

        public XamlType DeclaringType
        {
            get
            {
                return this._declaringType;
            }
        }

        internal object DefaultValue
        {
            get
            {
                this.EnsureDefaultValue();
                return this._reflector.DefaultValue;
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

        public IList<XamlMember> DependsOn
        {
            get
            {
                this.EnsureReflector();
                if (this._reflector.DependsOn == null)
                {
                    this._reflector.DependsOn = this.LookupDependsOn() ?? XamlType.EmptyList<XamlMember>.Value;
                }
                return this._reflector.DependsOn;
            }
        }

        internal MethodInfo Getter
        {
            get
            {
                this.EnsureReflector();
                if (!this._reflector.GetterIsSet)
                {
                    this._reflector.Getter = this.LookupUnderlyingGetter();
                }
                return this._reflector.Getter;
            }
        }

        internal bool HasDefaultValue
        {
            get
            {
                this.EnsureDefaultValue();
                return !this._reflector.DefaultValueIsNotPresent;
            }
        }

        internal bool HasSerializationVisibility
        {
            get
            {
                this.EnsureReflector();
                if (!this._reflector.DesignerSerializationVisibilityIsSet)
                {
                    this._reflector.SerializationVisibility = this.LookupSerializationVisibility();
                }
                return this._reflector.SerializationVisibility.HasValue;
            }
        }

        public XamlMemberInvoker Invoker
        {
            get
            {
                this.EnsureReflector();
                if (this._reflector.Invoker == null)
                {
                    this._reflector.Invoker = this.LookupInvoker() ?? XamlMemberInvoker.UnknownInvoker;
                }
                return this._reflector.Invoker;
            }
        }

        public bool IsAmbient
        {
            get
            {
                return this.GetFlag(BoolMemberBits.Ambient);
            }
        }

        public bool IsAttachable
        {
            get
            {
                return (this._memberType == MemberType.Attachable);
            }
        }

        public bool IsDirective
        {
            get
            {
                return (this._memberType == MemberType.Directive);
            }
        }

        public bool IsEvent
        {
            get
            {
                return this.GetFlag(BoolMemberBits.Event);
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

        public bool IsReadOnly
        {
            get
            {
                return this.GetFlag(BoolMemberBits.ReadOnly);
            }
        }

        public bool IsReadPublic
        {
            get
            {
                if (!this.IsReadPublicIgnoringType)
                {
                    return false;
                }
                if (this._declaringType != null)
                {
                    return this._declaringType.IsPublic;
                }
                return true;
            }
        }

        private bool IsReadPublicIgnoringType
        {
            get
            {
                this.EnsureReflector();
                bool? flag = this._reflector.GetFlag(BoolMemberBits.ReadPublic);
                if (!flag.HasValue)
                {
                    flag = new bool?(this.LookupIsReadPublic());
                    this._reflector.SetFlag(BoolMemberBits.ReadPublic, flag.Value);
                }
                return flag.Value;
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

        public bool IsWriteOnly
        {
            get
            {
                return this.GetFlag(BoolMemberBits.WriteOnly);
            }
        }

        public bool IsWritePublic
        {
            get
            {
                if (!this.IsWritePublicIgnoringType)
                {
                    return false;
                }
                if (this._declaringType != null)
                {
                    return this._declaringType.IsPublic;
                }
                return true;
            }
        }

        private bool IsWritePublicIgnoringType
        {
            get
            {
                this.EnsureReflector();
                bool? flag = this._reflector.GetFlag(BoolMemberBits.WritePublic);
                if (!flag.HasValue)
                {
                    flag = new bool?(this.LookupIsWritePublic());
                    this._reflector.SetFlag(BoolMemberBits.WritePublic, flag.Value);
                }
                return flag.Value;
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

        private XamlSchemaContext SchemaContext
        {
            get
            {
                return this._declaringType.SchemaContext;
            }
        }

        public DesignerSerializationVisibility SerializationVisibility
        {
            get
            {
                this.EnsureReflector();
                if (!this._reflector.DesignerSerializationVisibilityIsSet)
                {
                    this._reflector.SerializationVisibility = this.LookupSerializationVisibility();
                }
                DesignerSerializationVisibility? serializationVisibility = this._reflector.SerializationVisibility;
                if (!serializationVisibility.HasValue)
                {
                    return DesignerSerializationVisibility.Visible;
                }
                return serializationVisibility.GetValueOrDefault();
            }
        }

        internal MethodInfo Setter
        {
            get
            {
                this.EnsureReflector();
                if (!this._reflector.SetterIsSet)
                {
                    this._reflector.Setter = this.LookupUnderlyingSetter();
                }
                return this._reflector.Setter;
            }
        }

        public XamlType TargetType
        {
            get
            {
                if (!this.IsAttachable)
                {
                    return this._declaringType;
                }
                this.EnsureReflector();
                if (this._reflector.TargetType == null)
                {
                    if (this._reflector.IsUnknown)
                    {
                        return XamlLanguage.Object;
                    }
                    this._reflector.TargetType = this.LookupTargetType() ?? XamlLanguage.Object;
                }
                return this._reflector.TargetType;
            }
        }

        public XamlType Type
        {
            get
            {
                this.EnsureReflector();
                if (this._reflector.Type == null)
                {
                    this._reflector.Type = this.LookupType() ?? XamlLanguage.Object;
                }
                return this._reflector.Type;
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

        public MemberInfo UnderlyingMember
        {
            [SecuritySafeCritical]
            get
            {
                if (!this._underlyingMember.IsSet)
                {
                    this._underlyingMember.SetIfNull(this.LookupUnderlyingMember());
                }
                return this._underlyingMember.Value;
            }
        }

        internal NullableReference<MemberInfo> UnderlyingMemberInternal
        {
            [SecuritySafeCritical]
            get
            {
                return this._underlyingMember;
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

        private enum MemberType : byte
        {
            Attachable = 1,
            Directive = 2,
            Instance = 0
        }
    }
}

