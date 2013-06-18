namespace MS.Internal.Xml.Linq.ComponentModel
{
    using System;
    using System.Xml.Linq;

    internal class XElementValuePropertyDescriptor : XPropertyDescriptor<XElement, string>
    {
        private XElement element;

        public XElementValuePropertyDescriptor() : base("Value")
        {
        }

        public override object GetValue(object component)
        {
            this.element = component as XElement;
            if (this.element == null)
            {
                return string.Empty;
            }
            return this.element.Value;
        }

        protected override void OnChanged(object sender, XObjectChangeEventArgs args)
        {
            if (this.element != null)
            {
                switch (args.ObjectChange)
                {
                    case XObjectChange.Add:
                    case XObjectChange.Remove:
                        if ((sender is XElement) || (sender is XText))
                        {
                            this.OnValueChanged(this.element, EventArgs.Empty);
                        }
                        return;

                    case XObjectChange.Name:
                        return;

                    case XObjectChange.Value:
                        if (sender is XText)
                        {
                            this.OnValueChanged(this.element, EventArgs.Empty);
                        }
                        return;
                }
            }
        }

        public override void SetValue(object component, object value)
        {
            this.element = component as XElement;
            if (this.element != null)
            {
                this.element.Value = value as string;
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

