namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    internal sealed class ReadonlyPropertyDescriptor : PropertyDescriptor
    {
        private PropertyDescriptor realPropertyDescriptor;

        internal ReadonlyPropertyDescriptor(PropertyDescriptor descriptor) : base(descriptor, null)
        {
            this.realPropertyDescriptor = descriptor;
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override object GetValue(object component)
        {
            return this.realPropertyDescriptor.GetValue(component);
        }

        public override void ResetValue(object component)
        {
            this.realPropertyDescriptor.ResetValue(component);
        }

        public override void SetValue(object component, object value)
        {
        }

        public override bool ShouldSerializeValue(object component)
        {
            return this.realPropertyDescriptor.ShouldSerializeValue(component);
        }

        public override AttributeCollection Attributes
        {
            get
            {
                ArrayList list = new ArrayList();
                foreach (Attribute attribute in this.realPropertyDescriptor.Attributes)
                {
                    if (!(attribute is EditorAttribute) && !(attribute is ReadOnlyAttribute))
                    {
                        list.Add(attribute);
                    }
                }
                list.Add(new ReadOnlyAttribute(true));
                return new AttributeCollection((Attribute[]) list.ToArray(typeof(Attribute)));
            }
        }

        public override string Category
        {
            get
            {
                return this.realPropertyDescriptor.Category;
            }
        }

        public override Type ComponentType
        {
            get
            {
                return this.realPropertyDescriptor.ComponentType;
            }
        }

        public override TypeConverter Converter
        {
            get
            {
                return this.realPropertyDescriptor.Converter;
            }
        }

        public override string Description
        {
            get
            {
                return this.realPropertyDescriptor.Description;
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
                return this.realPropertyDescriptor.PropertyType;
            }
        }
    }
}

