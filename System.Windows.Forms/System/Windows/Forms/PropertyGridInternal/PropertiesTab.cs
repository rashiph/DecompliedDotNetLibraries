namespace System.Windows.Forms.PropertyGridInternal
{
    using System;
    using System.ComponentModel;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public class PropertiesTab : PropertyTab
    {
        public override PropertyDescriptor GetDefaultProperty(object obj)
        {
            PropertyDescriptor defaultProperty = base.GetDefaultProperty(obj);
            if (defaultProperty == null)
            {
                PropertyDescriptorCollection properties = this.GetProperties(obj);
                if (properties == null)
                {
                    return defaultProperty;
                }
                for (int i = 0; i < properties.Count; i++)
                {
                    if ("Name".Equals(properties[i].Name))
                    {
                        return properties[i];
                    }
                }
            }
            return defaultProperty;
        }

        public override PropertyDescriptorCollection GetProperties(object component, Attribute[] attributes)
        {
            return this.GetProperties(null, component, attributes);
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attributes)
        {
            if (attributes == null)
            {
                attributes = new Attribute[] { BrowsableAttribute.Yes };
            }
            if (context != null)
            {
                TypeConverter converter = (context.PropertyDescriptor == null) ? TypeDescriptor.GetConverter(component) : context.PropertyDescriptor.Converter;
                if ((converter != null) && converter.GetPropertiesSupported(context))
                {
                    return converter.GetProperties(context, component, attributes);
                }
            }
            return TypeDescriptor.GetProperties(component, attributes);
        }

        public override string HelpKeyword
        {
            get
            {
                return "vs.properties";
            }
        }

        public override string TabName
        {
            get
            {
                return System.Windows.Forms.SR.GetString("PBRSToolTipProperties");
            }
        }
    }
}

