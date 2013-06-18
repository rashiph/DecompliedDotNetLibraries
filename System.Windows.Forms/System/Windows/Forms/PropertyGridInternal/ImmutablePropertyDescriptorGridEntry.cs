namespace System.Windows.Forms.PropertyGridInternal
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;
    using System.Windows.Forms;

    internal class ImmutablePropertyDescriptorGridEntry : PropertyDescriptorGridEntry
    {
        internal ImmutablePropertyDescriptorGridEntry(PropertyGrid ownerGrid, GridEntry peParent, PropertyDescriptor propInfo, bool hide) : base(ownerGrid, peParent, propInfo, hide)
        {
        }

        internal override bool NotifyValueGivenParent(object obj, int type)
        {
            return this.ParentGridEntry.NotifyValue(type);
        }

        private GridEntry InstanceParentGridEntry
        {
            get
            {
                GridEntry parentGridEntry = this.ParentGridEntry;
                if (parentGridEntry is CategoryGridEntry)
                {
                    parentGridEntry = parentGridEntry.ParentGridEntry;
                }
                return parentGridEntry;
            }
        }

        protected override bool IsPropertyReadOnly
        {
            get
            {
                return this.ShouldRenderReadOnly;
            }
        }

        public override object PropertyValue
        {
            get
            {
                return base.PropertyValue;
            }
            set
            {
                object valueOwner = this.GetValueOwner();
                GridEntry instanceParentGridEntry = this.InstanceParentGridEntry;
                TypeConverter typeConverter = instanceParentGridEntry.TypeConverter;
                PropertyDescriptorCollection properties = typeConverter.GetProperties(instanceParentGridEntry, valueOwner);
                IDictionary propertyValues = new Hashtable(properties.Count);
                object obj3 = null;
                for (int i = 0; i < properties.Count; i++)
                {
                    if ((base.propertyInfo.Name != null) && base.propertyInfo.Name.Equals(properties[i].Name))
                    {
                        propertyValues[properties[i].Name] = value;
                    }
                    else
                    {
                        propertyValues[properties[i].Name] = properties[i].GetValue(valueOwner);
                    }
                }
                try
                {
                    obj3 = typeConverter.CreateInstance(instanceParentGridEntry, propertyValues);
                }
                catch (Exception exception)
                {
                    if (string.IsNullOrEmpty(exception.Message))
                    {
                        throw new TargetInvocationException(System.Windows.Forms.SR.GetString("ExceptionCreatingObject", new object[] { this.InstanceParentGridEntry.PropertyType.FullName, exception.ToString() }), exception);
                    }
                    throw;
                }
                if (obj3 != null)
                {
                    instanceParentGridEntry.PropertyValue = obj3;
                }
            }
        }

        public override bool ShouldRenderReadOnly
        {
            get
            {
                return this.InstanceParentGridEntry.ShouldRenderReadOnly;
            }
        }
    }
}

