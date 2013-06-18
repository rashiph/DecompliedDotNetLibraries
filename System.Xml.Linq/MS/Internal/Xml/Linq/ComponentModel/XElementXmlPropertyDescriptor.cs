namespace MS.Internal.Xml.Linq.ComponentModel
{
    using System;
    using System.Xml.Linq;

    internal class XElementXmlPropertyDescriptor : XPropertyDescriptor<XElement, string>
    {
        private XElement element;

        public XElementXmlPropertyDescriptor() : base("Xml")
        {
        }

        public override object GetValue(object component)
        {
            this.element = component as XElement;
            if (this.element == null)
            {
                return string.Empty;
            }
            return this.element.ToString(SaveOptions.DisableFormatting);
        }

        protected override void OnChanged(object sender, XObjectChangeEventArgs args)
        {
            if (this.element != null)
            {
                this.OnValueChanged(this.element, EventArgs.Empty);
            }
        }
    }
}

