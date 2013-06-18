namespace System.Windows.Markup
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Xaml;

    [TypeForwardedFrom("PresentationFramework, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"), MarkupExtensionReturnType(typeof(System.Type)), TypeConverter(typeof(TypeExtensionConverter))]
    public class TypeExtension : MarkupExtension
    {
        private System.Type _type;
        private string _typeName;

        public TypeExtension()
        {
        }

        public TypeExtension(string typeName)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            this._typeName = typeName;
        }

        public TypeExtension(System.Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            this._type = type;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (this._type == null)
            {
                if (this._typeName == null)
                {
                    throw new InvalidOperationException(System.Xaml.SR.Get("MarkupExtensionTypeName"));
                }
                if (serviceProvider == null)
                {
                    throw new ArgumentNullException("serviceProvider");
                }
                IXamlTypeResolver service = serviceProvider.GetService(typeof(IXamlTypeResolver)) as IXamlTypeResolver;
                if (service == null)
                {
                    throw new InvalidOperationException(System.Xaml.SR.Get("MarkupExtensionNoContext", new object[] { base.GetType().Name, "IXamlTypeResolver" }));
                }
                this._type = service.Resolve(this._typeName);
                if (this._type == null)
                {
                    throw new InvalidOperationException(System.Xaml.SR.Get("MarkupExtensionTypeNameBad", new object[] { this._typeName }));
                }
            }
            return this._type;
        }

        [ConstructorArgument("type"), DefaultValue((string) null)]
        public System.Type Type
        {
            get
            {
                return this._type;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._type = value;
                this._typeName = null;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string TypeName
        {
            get
            {
                return this._typeName;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this._typeName = value;
                this._type = null;
            }
        }
    }
}

