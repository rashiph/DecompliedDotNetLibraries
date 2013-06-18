namespace MS.Internal.Xml.Linq.ComponentModel
{
    using System;
    using System.Xml.Linq;

    internal class XElementAttributePropertyDescriptor : XPropertyDescriptor<XElement, object>
    {
        private XAttribute changeState;
        private XDeferredSingleton<XAttribute> value;

        public XElementAttributePropertyDescriptor() : base("Attribute")
        {
        }

        public override object GetValue(object component)
        {
            return (this.value = new XDeferredSingleton<XAttribute>((e, n) => e.Attribute(n), component as XElement, null));
        }

        protected override void OnChanged(object sender, XObjectChangeEventArgs args)
        {
            if (this.value != null)
            {
                XAttribute attribute;
                switch (args.ObjectChange)
                {
                    case XObjectChange.Add:
                        attribute = sender as XAttribute;
                        if (((attribute != null) && (this.value.element == attribute.parent)) && (this.value.name == attribute.Name))
                        {
                            this.OnValueChanged(this.value.element, EventArgs.Empty);
                        }
                        return;

                    case XObjectChange.Remove:
                        attribute = sender as XAttribute;
                        if ((attribute != null) && (this.changeState == attribute))
                        {
                            this.changeState = null;
                            this.OnValueChanged(this.value.element, EventArgs.Empty);
                        }
                        return;
                }
            }
        }

        protected override void OnChanging(object sender, XObjectChangeEventArgs args)
        {
            if ((this.value != null) && (args.ObjectChange == XObjectChange.Remove))
            {
                XAttribute attribute = sender as XAttribute;
                this.changeState = (((attribute != null) && (this.value.element == attribute.parent)) && (this.value.name == attribute.Name)) ? attribute : null;
            }
        }
    }
}

