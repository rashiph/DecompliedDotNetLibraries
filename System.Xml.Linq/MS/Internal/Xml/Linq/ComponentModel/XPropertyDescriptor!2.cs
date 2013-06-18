namespace MS.Internal.Xml.Linq.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.Xml.Linq;

    internal abstract class XPropertyDescriptor<T, TProperty> : PropertyDescriptor where T: XObject
    {
        public XPropertyDescriptor(string name) : base(name, null)
        {
        }

        public override void AddValueChanged(object component, EventHandler handler)
        {
            bool flag = base.GetValueChangedHandler(component) != null;
            base.AddValueChanged(component, handler);
            if (!flag)
            {
                T local = component as T;
                if ((local != null) && (base.GetValueChangedHandler(component) != null))
                {
                    local.Changing += new EventHandler<XObjectChangeEventArgs>(this.OnChanging);
                    local.Changed += new EventHandler<XObjectChangeEventArgs>(this.OnChanged);
                }
            }
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        protected virtual void OnChanged(object sender, XObjectChangeEventArgs args)
        {
        }

        protected virtual void OnChanging(object sender, XObjectChangeEventArgs args)
        {
        }

        public override void RemoveValueChanged(object component, EventHandler handler)
        {
            base.RemoveValueChanged(component, handler);
            T local = component as T;
            if ((local != null) && (base.GetValueChangedHandler(component) == null))
            {
                local.Changing -= new EventHandler<XObjectChangeEventArgs>(this.OnChanging);
                local.Changed -= new EventHandler<XObjectChangeEventArgs>(this.OnChanged);
            }
        }

        public override void ResetValue(object component)
        {
        }

        public override void SetValue(object component, object value)
        {
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override Type ComponentType
        {
            get
            {
                return typeof(T);
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return typeof(TProperty);
            }
        }

        public override bool SupportsChangeEvents
        {
            get
            {
                return true;
            }
        }
    }
}

