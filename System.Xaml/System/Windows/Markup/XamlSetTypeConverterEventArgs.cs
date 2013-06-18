namespace System.Windows.Markup
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Xaml;

    public class XamlSetTypeConverterEventArgs : XamlSetValueEventArgs
    {
        public XamlSetTypeConverterEventArgs(XamlMember member, System.ComponentModel.TypeConverter typeConverter, object value, ITypeDescriptorContext serviceProvider, System.Globalization.CultureInfo cultureInfo) : base(member, value)
        {
            this.TypeConverter = typeConverter;
            this.ServiceProvider = serviceProvider;
            this.CultureInfo = cultureInfo;
        }

        internal XamlSetTypeConverterEventArgs(XamlMember member, System.ComponentModel.TypeConverter typeConverter, object value, ITypeDescriptorContext serviceProvider, System.Globalization.CultureInfo cultureInfo, object targetObject) : this(member, typeConverter, value, serviceProvider, cultureInfo)
        {
            this.TargetObject = targetObject;
        }

        public override void CallBase()
        {
            if (this.CurrentType != null)
            {
                XamlType baseType = this.CurrentType.BaseType;
                if (baseType != null)
                {
                    this.CurrentType = baseType;
                    if (baseType.SetTypeConverterHandler != null)
                    {
                        baseType.SetTypeConverterHandler(this.TargetObject, this);
                    }
                }
            }
        }

        public System.Globalization.CultureInfo CultureInfo { get; private set; }

        internal XamlType CurrentType { get; set; }

        public ITypeDescriptorContext ServiceProvider { get; private set; }

        internal object TargetObject { get; private set; }

        public System.ComponentModel.TypeConverter TypeConverter { get; private set; }
    }
}

