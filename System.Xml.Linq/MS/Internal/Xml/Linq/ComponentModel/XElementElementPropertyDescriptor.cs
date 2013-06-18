namespace MS.Internal.Xml.Linq.ComponentModel
{
    using System;
    using System.Xml.Linq;

    internal class XElementElementPropertyDescriptor : XPropertyDescriptor<XElement, object>
    {
        private XElement changeState;
        private XDeferredSingleton<XElement> value;

        public XElementElementPropertyDescriptor() : base("Element")
        {
        }

        public override object GetValue(object component)
        {
            return (this.value = new XDeferredSingleton<XElement>((e, n) => e.Element(n), component as XElement, null));
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
                        if (((element != null) && (this.value.element == element.parent)) && ((this.value.name == element.Name) && (this.value.element.Element(this.value.name) == element)))
                        {
                            this.OnValueChanged(this.value.element, EventArgs.Empty);
                        }
                        return;

                    case XObjectChange.Remove:
                        element = sender as XElement;
                        if ((element != null) && (this.changeState == element))
                        {
                            this.changeState = null;
                            this.OnValueChanged(this.value.element, EventArgs.Empty);
                        }
                        return;

                    case XObjectChange.Name:
                        element = sender as XElement;
                        if (element != null)
                        {
                            if (((this.value.element != element.parent) || !(this.value.name == element.Name)) || (this.value.element.Element(this.value.name) != element))
                            {
                                if (this.changeState == element)
                                {
                                    this.changeState = null;
                                    this.OnValueChanged(this.value.element, EventArgs.Empty);
                                }
                                return;
                            }
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
                switch (args.ObjectChange)
                {
                    case XObjectChange.Remove:
                    case XObjectChange.Name:
                    {
                        XElement element = sender as XElement;
                        this.changeState = (((element != null) && (this.value.element == element.parent)) && ((this.value.name == element.Name) && (this.value.element.Element(this.value.name) == element))) ? element : null;
                        return;
                    }
                }
            }
        }
    }
}

