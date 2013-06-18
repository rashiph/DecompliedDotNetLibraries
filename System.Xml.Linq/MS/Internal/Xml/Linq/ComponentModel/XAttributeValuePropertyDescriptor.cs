namespace MS.Internal.Xml.Linq.ComponentModel
{
    using System;
    using System.Xml.Linq;

    internal class XAttributeValuePropertyDescriptor : XPropertyDescriptor<XAttribute, string>
    {
        private XAttribute attribute;

        public XAttributeValuePropertyDescriptor() : base("Value")
        {
        }

        public override object GetValue(object component)
        {
            this.attribute = component as XAttribute;
            if (this.attribute == null)
            {
                return string.Empty;
            }
            return this.attribute.Value;
        }

        protected override void OnChanged(object sender, XObjectChangeEventArgs args)
        {
            if ((this.attribute != null) && (args.ObjectChange == XObjectChange.Value))
            {
                this.OnValueChanged(this.attribute, EventArgs.Empty);
            }
        }

        public override void SetValue(object component, object value)
        {
            this.attribute = component as XAttribute;
            if (this.attribute != null)
            {
                this.attribute.Value = value as string;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }
    }
}

