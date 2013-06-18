namespace MS.Internal.Xml.Linq.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.Xml.Linq;

    internal class XTypeDescriptor<T> : CustomTypeDescriptor
    {
        public XTypeDescriptor(ICustomTypeDescriptor parent) : base(parent)
        {
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            return this.GetProperties(null);
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection descriptors = new PropertyDescriptorCollection(null);
            if (attributes == null)
            {
                if (typeof(T) == typeof(XElement))
                {
                    descriptors.Add(new XElementAttributePropertyDescriptor());
                    descriptors.Add(new XElementDescendantsPropertyDescriptor());
                    descriptors.Add(new XElementElementPropertyDescriptor());
                    descriptors.Add(new XElementElementsPropertyDescriptor());
                    descriptors.Add(new XElementValuePropertyDescriptor());
                    descriptors.Add(new XElementXmlPropertyDescriptor());
                }
                else if (typeof(T) == typeof(XAttribute))
                {
                    descriptors.Add(new XAttributeValuePropertyDescriptor());
                }
            }
            foreach (PropertyDescriptor descriptor in base.GetProperties(attributes))
            {
                descriptors.Add(descriptor);
            }
            return descriptors;
        }
    }
}

