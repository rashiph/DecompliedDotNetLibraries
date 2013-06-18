namespace System.Xaml.Schema
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Xaml;

    internal class MemberReflector : Reflector
    {
        private NullableReference<string> _constructorArgument;
        private NullableReference<object> _defaultValue;
        private NullableReference<XamlValueConverter<XamlDeferringLoader>> _deferringLoader;
        private DesignerSerializationVisibility _designerSerializationVisibility;
        [SecurityCritical]
        private NullableReference<MethodInfo> _getter;
        private int _memberBits;
        [SecurityCritical]
        private NullableReference<MethodInfo> _setter;
        private NullableReference<XamlValueConverter<System.ComponentModel.TypeConverter>> _typeConverter;
        private NullableReference<XamlValueConverter<System.Windows.Markup.ValueSerializer>> _valueSerializer;
        private static MemberReflector s_UnknownReflector;
        private const DesignerSerializationVisibility VisibilityInvalid = ((DesignerSerializationVisibility) 0x7fffffff);
        private const DesignerSerializationVisibility VisibilityNone = ((DesignerSerializationVisibility) 0x7ffffffe);

        internal MemberReflector()
        {
            this._designerSerializationVisibility = (DesignerSerializationVisibility) 0x7fffffff;
        }

        internal MemberReflector(bool isEvent) : this()
        {
            if (isEvent)
            {
                this._memberBits = 4;
            }
            this._memberBits |= Reflector.GetValidMask(4);
        }

        [SecuritySafeCritical]
        internal MemberReflector(XamlType type, XamlValueConverter<System.ComponentModel.TypeConverter> typeConverter)
        {
            this.Type = type;
            this._typeConverter.Value = typeConverter;
            this._designerSerializationVisibility = DesignerSerializationVisibility.Visible;
            this._memberBits = -65440;
            this._deferringLoader.Value = null;
            this._getter.Value = null;
            this._setter.Value = null;
            this._valueSerializer.Value = null;
        }

        [SecuritySafeCritical]
        internal MemberReflector(MethodInfo getter, MethodInfo setter, bool isEvent) : this(isEvent)
        {
            this._getter.Value = getter;
            this._setter.Value = setter;
        }

        internal static bool GenericArgumentsAreVisibleTo(MethodInfo method, Assembly accessingAssembly, XamlSchemaContext schemaContext)
        {
            if (method.IsGenericMethod)
            {
                foreach (System.Type type in method.GetGenericArguments())
                {
                    if (!TypeReflector.IsVisibleTo(type, accessingAssembly, schemaContext))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal bool? GetFlag(BoolMemberBits flag)
        {
            return Reflector.GetFlag(this._memberBits, (int) flag);
        }

        internal static bool IsInternalVisibleTo(MethodInfo method, Assembly accessingAssembly, XamlSchemaContext schemaContext)
        {
            if (accessingAssembly == null)
            {
                return false;
            }
            if (!method.IsAssembly && !method.IsFamilyOrAssembly)
            {
                return false;
            }
            return (TypeReflector.IsInternal(method.DeclaringType) || schemaContext.AreInternalsVisibleTo(method.DeclaringType.Assembly, accessingAssembly));
        }

        internal static bool IsProtectedVisibleTo(MethodInfo method, System.Type derivedType, XamlSchemaContext schemaContext)
        {
            if (derivedType == null)
            {
                return false;
            }
            if (!derivedType.Equals(method.DeclaringType) && !derivedType.IsSubclassOf(method.DeclaringType))
            {
                return false;
            }
            if (method.IsFamily || method.IsFamilyOrAssembly)
            {
                return true;
            }
            if (!method.IsFamilyAndAssembly)
            {
                return false;
            }
            return (TypeReflector.IsInternal(method.DeclaringType) || schemaContext.AreInternalsVisibleTo(method.DeclaringType.Assembly, derivedType.Assembly));
        }

        internal void SetFlag(BoolMemberBits flag, bool value)
        {
            Reflector.SetFlag(ref this._memberBits, (int) flag, value);
        }

        internal string ConstructorArgument
        {
            get
            {
                return this._constructorArgument.Value;
            }
            set
            {
                this._constructorArgument.Value = value;
            }
        }

        internal bool ConstructorArgumentIsSet
        {
            get
            {
                return this._constructorArgument.IsSet;
            }
        }

        internal object DefaultValue
        {
            get
            {
                if (!this._defaultValue.IsNotPresent)
                {
                    return this._defaultValue.Value;
                }
                return null;
            }
            set
            {
                this._defaultValue.Value = value;
            }
        }

        internal bool DefaultValueIsNotPresent
        {
            get
            {
                return this._defaultValue.IsNotPresent;
            }
            set
            {
                this._defaultValue.IsNotPresent = value;
            }
        }

        internal bool DefaultValueIsSet
        {
            get
            {
                return this._defaultValue.IsSet;
            }
        }

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

        internal IList<XamlMember> DependsOn { get; set; }

        internal bool DesignerSerializationVisibilityIsSet
        {
            get
            {
                return (this._designerSerializationVisibility != ((DesignerSerializationVisibility) 0x7fffffff));
            }
        }

        internal MethodInfo Getter
        {
            [SecuritySafeCritical]
            get
            {
                return this._getter.Value;
            }
            [SecuritySafeCritical]
            set
            {
                this._getter.SetIfNull(value);
            }
        }

        internal bool GetterIsSet
        {
            [SecuritySafeCritical]
            get
            {
                return this._getter.IsSet;
            }
        }

        internal XamlMemberInvoker Invoker { get; set; }

        internal bool IsUnknown
        {
            get
            {
                return ((this._memberBits & 8) != 0);
            }
        }

        protected override MemberInfo Member
        {
            get
            {
                return this.UnderlyingMember;
            }
        }

        internal DesignerSerializationVisibility? SerializationVisibility
        {
            get
            {
                if (this._designerSerializationVisibility == ((DesignerSerializationVisibility) 0x7ffffffe))
                {
                    return null;
                }
                return new DesignerSerializationVisibility?(this._designerSerializationVisibility);
            }
            set
            {
                this._designerSerializationVisibility = value.GetValueOrDefault((DesignerSerializationVisibility) 0x7ffffffe);
            }
        }

        internal MethodInfo Setter
        {
            [SecuritySafeCritical]
            get
            {
                return this._setter.Value;
            }
            [SecuritySafeCritical]
            set
            {
                this._setter.SetIfNull(value);
            }
        }

        internal bool SetterIsSet
        {
            [SecuritySafeCritical]
            get
            {
                return this._setter.IsSet;
            }
        }

        internal XamlType TargetType { get; set; }

        internal XamlType Type { get; set; }

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

        internal MemberInfo UnderlyingMember { get; set; }

        internal static MemberReflector UnknownReflector
        {
            [SecuritySafeCritical]
            get
            {
                if (s_UnknownReflector == null)
                {
                    s_UnknownReflector = new MemberReflector();
                    s_UnknownReflector._designerSerializationVisibility = DesignerSerializationVisibility.Visible;
                    s_UnknownReflector._memberBits = -65432;
                    s_UnknownReflector._deferringLoader.Value = null;
                    s_UnknownReflector._getter.Value = null;
                    s_UnknownReflector._setter.Value = null;
                    s_UnknownReflector._typeConverter.Value = null;
                    s_UnknownReflector._valueSerializer.Value = null;
                    s_UnknownReflector.DependsOn = XamlType.EmptyList<XamlMember>.Value;
                    s_UnknownReflector.Invoker = XamlMemberInvoker.UnknownInvoker;
                    s_UnknownReflector.Type = XamlLanguage.Object;
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
    }
}

