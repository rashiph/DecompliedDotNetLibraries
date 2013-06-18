namespace System.Windows.Markup
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Xaml;

    [MarkupExtensionReturnType(typeof(object)), TypeForwardedFrom("PresentationFramework, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"), TypeConverter(typeof(StaticExtensionConverter))]
    public class StaticExtension : MarkupExtension
    {
        private string _member;
        private Type _memberType;

        public StaticExtension()
        {
        }

        public StaticExtension(string member)
        {
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }
            this._member = member;
        }

        private bool GetFieldOrPropertyValue(Type type, string name, out object value)
        {
            FieldInfo field = null;
            Type baseType = type;
            do
            {
                field = baseType.GetField(name, BindingFlags.Public | BindingFlags.Static);
                if (field != null)
                {
                    value = field.GetValue(null);
                    return true;
                }
                baseType = baseType.BaseType;
            }
            while (baseType != null);
            PropertyInfo property = null;
            baseType = type;
            do
            {
                property = baseType.GetProperty(name, BindingFlags.Public | BindingFlags.Static);
                if (property != null)
                {
                    value = property.GetValue(null, null);
                    return true;
                }
                baseType = baseType.BaseType;
            }
            while (baseType != null);
            value = null;
            return false;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            object obj2;
            if (this._member == null)
            {
                throw new InvalidOperationException(System.Xaml.SR.Get("MarkupExtensionStaticMember"));
            }
            Type memberType = this.MemberType;
            string str = null;
            string str2 = null;
            if (memberType != null)
            {
                str = this._member;
                str2 = memberType.FullName + "." + this._member;
            }
            else
            {
                str2 = this._member;
                int index = this._member.IndexOf('.');
                if (index < 0)
                {
                    throw new ArgumentException(System.Xaml.SR.Get("MarkupExtensionBadStatic", new object[] { this._member }));
                }
                string qualifiedTypeName = this._member.Substring(0, index);
                if (qualifiedTypeName == string.Empty)
                {
                    throw new ArgumentException(System.Xaml.SR.Get("MarkupExtensionBadStatic", new object[] { this._member }));
                }
                if (serviceProvider == null)
                {
                    throw new ArgumentNullException("serviceProvider");
                }
                IXamlTypeResolver service = serviceProvider.GetService(typeof(IXamlTypeResolver)) as IXamlTypeResolver;
                if (service == null)
                {
                    throw new ArgumentException(System.Xaml.SR.Get("MarkupExtensionNoContext", new object[] { base.GetType().Name, "IXamlTypeResolver" }));
                }
                memberType = service.Resolve(qualifiedTypeName);
                str = this._member.Substring(index + 1, (this._member.Length - index) - 1);
                if (str == string.Empty)
                {
                    throw new ArgumentException(System.Xaml.SR.Get("MarkupExtensionBadStatic", new object[] { this._member }));
                }
            }
            if (memberType.IsEnum)
            {
                return Enum.Parse(memberType, str);
            }
            if (!this.GetFieldOrPropertyValue(memberType, str, out obj2))
            {
                throw new ArgumentException(System.Xaml.SR.Get("MarkupExtensionBadStatic", new object[] { str2 }));
            }
            return obj2;
        }

        [ConstructorArgument("member")]
        public string Member
        {
            get
            {
                return this._member;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._member = value;
            }
        }

        [DefaultValue((string) null)]
        public Type MemberType
        {
            get
            {
                return this._memberType;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._memberType = value;
            }
        }
    }
}

