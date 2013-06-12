namespace System.ComponentModel
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class ReferenceConverter : TypeConverter
    {
        private static readonly string none = SR.GetString("toStringNone");
        private Type type;

        public ReferenceConverter(Type type)
        {
            this.type = type;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (((sourceType == typeof(string)) && (context != null)) || base.CanConvertFrom(context, sourceType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (!(value is string))
            {
                return base.ConvertFrom(context, culture, value);
            }
            string a = ((string) value).Trim();
            if (!string.Equals(a, none) && (context != null))
            {
                IReferenceService service = (IReferenceService) context.GetService(typeof(IReferenceService));
                if (service != null)
                {
                    object reference = service.GetReference(a);
                    if (reference != null)
                    {
                        return reference;
                    }
                }
                IContainer container = context.Container;
                if (container != null)
                {
                    object obj3 = container.Components[a];
                    if (obj3 != null)
                    {
                        return obj3;
                    }
                }
            }
            return null;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (!(destinationType == typeof(string)))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            if (value == null)
            {
                return none;
            }
            if (context != null)
            {
                IReferenceService service = (IReferenceService) context.GetService(typeof(IReferenceService));
                if (service != null)
                {
                    string name = service.GetName(value);
                    if (name != null)
                    {
                        return name;
                    }
                }
            }
            if (!Marshal.IsComObject(value) && (value is IComponent))
            {
                IComponent component = (IComponent) value;
                ISite site = component.Site;
                if (site != null)
                {
                    string str2 = site.Name;
                    if (str2 != null)
                    {
                        return str2;
                    }
                }
            }
            return string.Empty;
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            object[] array = null;
            if (context != null)
            {
                ArrayList list = new ArrayList();
                list.Add(null);
                IReferenceService service = (IReferenceService) context.GetService(typeof(IReferenceService));
                if (service != null)
                {
                    object[] references = service.GetReferences(this.type);
                    int length = references.Length;
                    for (int i = 0; i < length; i++)
                    {
                        if (this.IsValueAllowed(context, references[i]))
                        {
                            list.Add(references[i]);
                        }
                    }
                }
                else
                {
                    IContainer container = context.Container;
                    if (container != null)
                    {
                        foreach (IComponent component in container.Components)
                        {
                            if (((component != null) && this.type.IsInstanceOfType(component)) && this.IsValueAllowed(context, component))
                            {
                                list.Add(component);
                            }
                        }
                    }
                }
                array = list.ToArray();
                Array.Sort(array, 0, array.Length, new ReferenceComparer(this));
            }
            return new TypeConverter.StandardValuesCollection(array);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        protected virtual bool IsValueAllowed(ITypeDescriptorContext context, object value)
        {
            return true;
        }

        private class ReferenceComparer : IComparer
        {
            private ReferenceConverter converter;

            public ReferenceComparer(ReferenceConverter converter)
            {
                this.converter = converter;
            }

            public int Compare(object item1, object item2)
            {
                string strA = this.converter.ConvertToString(item1);
                string strB = this.converter.ConvertToString(item2);
                return string.Compare(strA, strB, false, CultureInfo.InvariantCulture);
            }
        }
    }
}

