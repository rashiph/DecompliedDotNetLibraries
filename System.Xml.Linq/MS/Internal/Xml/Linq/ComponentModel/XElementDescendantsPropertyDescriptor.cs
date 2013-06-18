namespace MS.Internal.Xml.Linq.ComponentModel
{
    using System;
    using System.Xml.Linq;

    internal class XElementDescendantsPropertyDescriptor : XPropertyDescriptor<XElement, IEnumerable<XElement>>
    {
        private XName changeState;
        private XDeferredAxis<XElement> value;

        public XElementDescendantsPropertyDescriptor() : base("Descendants")
        {
        }

        public override object GetValue(object component)
        {
            return (this.value = new XDeferredAxis<XElement>(delegate (XElement e, XName n) {
                if (n == null)
                {
                    return e.Descendants();
                }
                return e.Descendants(n);
            }, component as XElement, null));
        }

        protected override void OnChanged(object sender, XObjectChangeEventArgs args)
        {
            if (this.value != null)
            {
                XElement element;
                switch (args.ObjectChange)
                {
                    case XObjectChange.Add:
                    case XObjectChange.Remove:
                        element = sender as XElement;
                        if ((element != null) && ((this.value.name == element.Name) || (this.value.name == null)))
                        {
                            this.OnValueChanged(this.value.element, EventArgs.Empty);
                        }
                        return;

                    case XObjectChange.Name:
                        element = sender as XElement;
                        if ((((element != null) && (this.value.element != element)) && (this.value.name != null)) && ((this.value.name == element.Name) || (this.value.name == this.changeState)))
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
            if ((this.value != null) && (args.ObjectChange == XObjectChange.Name))
            {
                XElement element = sender as XElement;
                this.changeState = (element != null) ? element.Name : null;
            }
        }
    }
}

