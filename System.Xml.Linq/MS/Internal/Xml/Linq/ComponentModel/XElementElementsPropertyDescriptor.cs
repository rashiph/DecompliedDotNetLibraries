namespace MS.Internal.Xml.Linq.ComponentModel
{
    using System;
    using System.Xml.Linq;

    internal class XElementElementsPropertyDescriptor : XPropertyDescriptor<XElement, IEnumerable<XElement>>
    {
        private object changeState;
        private XDeferredAxis<XElement> value;

        public XElementElementsPropertyDescriptor() : base("Elements")
        {
        }

        public override object GetValue(object component)
        {
            return (this.value = new XDeferredAxis<XElement>(delegate (XElement e, XName n) {
                if (n == null)
                {
                    return e.Elements();
                }
                return e.Elements(n);
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
                        element = sender as XElement;
                        if (((element != null) && (this.value.element == element.parent)) && ((this.value.name == element.Name) || (this.value.name == null)))
                        {
                            this.OnValueChanged(this.value.element, EventArgs.Empty);
                        }
                        return;

                    case XObjectChange.Remove:
                        element = sender as XElement;
                        if (((element != null) && (this.value.element == (this.changeState as XContainer))) && ((this.value.name == element.Name) || (this.value.name == null)))
                        {
                            this.changeState = null;
                            this.OnValueChanged(this.value.element, EventArgs.Empty);
                        }
                        return;

                    case XObjectChange.Name:
                        element = sender as XElement;
                        if ((((element != null) && (this.value.element == element.parent)) && (this.value.name != null)) && ((this.value.name == element.Name) || (this.value.name == (this.changeState as XName))))
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
            if (this.value != null)
            {
                XElement element;
                switch (args.ObjectChange)
                {
                    case XObjectChange.Remove:
                        element = sender as XElement;
                        this.changeState = (element != null) ? element.parent : null;
                        return;

                    case XObjectChange.Name:
                        element = sender as XElement;
                        this.changeState = (element != null) ? element.Name : null;
                        return;
                }
            }
        }
    }
}

