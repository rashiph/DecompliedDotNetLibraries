namespace System.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class DynamicActivityTypeDescriptor : ICustomTypeDescriptor
    {
        private PropertyDescriptorCollection cachedProperties;
        private Activity owner;

        public DynamicActivityTypeDescriptor(Activity owner)
        {
            this.owner = owner;
            this.Properties = new ActivityPropertyCollection(this);
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this.owner, true);
        }

        public string GetClassName()
        {
            if (this.Name != null)
            {
                return this.Name;
            }
            return TypeDescriptor.GetClassName(this.owner, true);
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this.owner, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this.owner, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this.owner, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this.owner, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this.owner, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this.owner, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this.owner, attributes, true);
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return this.GetProperties(null);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection cachedProperties = this.cachedProperties;
            if (cachedProperties == null)
            {
                PropertyDescriptorCollection properties;
                if (attributes != null)
                {
                    properties = TypeDescriptor.GetProperties(this.owner, attributes, true);
                }
                else
                {
                    properties = TypeDescriptor.GetProperties(this.owner, true);
                }
                List<PropertyDescriptor> list = new List<PropertyDescriptor>(this.Properties.Count + 2);
                for (int i = 0; i < properties.Count; i++)
                {
                    PropertyDescriptor item = properties[i];
                    if (item.IsBrowsable)
                    {
                        list.Add(item);
                    }
                }
                foreach (DynamicActivityProperty property in this.Properties)
                {
                    if (string.IsNullOrEmpty(property.Name))
                    {
                        throw FxTrace.Exception.AsError(new ValidationException(System.Activities.SR.ActivityPropertyRequiresName(this.owner.DisplayName)));
                    }
                    if (property.Type == null)
                    {
                        throw FxTrace.Exception.AsError(new ValidationException(System.Activities.SR.ActivityPropertyRequiresType(this.owner.DisplayName)));
                    }
                    list.Add(new DynamicActivityPropertyDescriptor(property, this.owner.GetType()));
                }
                cachedProperties = new PropertyDescriptorCollection(list.ToArray());
                this.cachedProperties = cachedProperties;
            }
            return cachedProperties;
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this.owner;
        }

        public string Name { get; set; }

        public KeyedCollection<string, DynamicActivityProperty> Properties { get; private set; }

        private class ActivityPropertyCollection : KeyedCollection<string, DynamicActivityProperty>
        {
            private DynamicActivityTypeDescriptor parent;

            public ActivityPropertyCollection(DynamicActivityTypeDescriptor parent)
            {
                this.parent = parent;
            }

            protected override void ClearItems()
            {
                this.InvalidateCache();
                base.ClearItems();
            }

            protected override string GetKeyForItem(DynamicActivityProperty item)
            {
                return item.Name;
            }

            protected override void InsertItem(int index, DynamicActivityProperty item)
            {
                if (item == null)
                {
                    throw FxTrace.Exception.ArgumentNull("item");
                }
                if (base.Contains(item.Name))
                {
                    throw FxTrace.Exception.AsError(new ArgumentException(System.Activities.SR.DynamicActivityDuplicatePropertyDetected(item.Name), "item"));
                }
                this.InvalidateCache();
                base.InsertItem(index, item);
            }

            private void InvalidateCache()
            {
                this.parent.cachedProperties = null;
            }

            protected override void RemoveItem(int index)
            {
                this.InvalidateCache();
                base.RemoveItem(index);
            }

            protected override void SetItem(int index, DynamicActivityProperty item)
            {
                if (item == null)
                {
                    throw FxTrace.Exception.ArgumentNull("item");
                }
                if (!base[index].Name.Equals(item.Name) && base.Contains(item.Name))
                {
                    throw FxTrace.Exception.AsError(new ArgumentException(System.Activities.SR.DynamicActivityDuplicatePropertyDetected(item.Name), "item"));
                }
                this.InvalidateCache();
                base.SetItem(index, item);
            }
        }

        private class DynamicActivityPropertyDescriptor : PropertyDescriptor
        {
            private DynamicActivityProperty activityProperty;
            private AttributeCollection attributes;
            private Type componentType;

            public DynamicActivityPropertyDescriptor(DynamicActivityProperty activityProperty, Type componentType) : base(activityProperty.Name, null)
            {
                this.activityProperty = activityProperty;
                this.componentType = componentType;
            }

            public override bool CanResetValue(object component)
            {
                return false;
            }

            protected override void FillAttributes(IList attributeList)
            {
                if (attributeList == null)
                {
                    throw FxTrace.Exception.ArgumentNull("attributeList");
                }
                attributeList.Add(new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden));
            }

            public override object GetValue(object component)
            {
                IDynamicActivity activity = component as IDynamicActivity;
                if ((activity == null) || !activity.Properties.Contains(this.activityProperty))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.InvalidDynamicActivityProperty(this.Name)));
                }
                return this.activityProperty.Value;
            }

            public override void ResetValue(object component)
            {
            }

            public override void SetValue(object component, object value)
            {
                IDynamicActivity activity = component as IDynamicActivity;
                if ((activity == null) || !activity.Properties.Contains(this.activityProperty))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.InvalidDynamicActivityProperty(this.Name)));
                }
                this.activityProperty.Value = value;
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }

            public override AttributeCollection Attributes
            {
                get
                {
                    if (this.attributes == null)
                    {
                        AttributeCollection attributes = base.Attributes;
                        Collection<Attribute> collection = this.activityProperty.Attributes;
                        Attribute[] array = new Attribute[(attributes.Count + collection.Count) + 1];
                        attributes.CopyTo(array, 0);
                        collection.CopyTo(array, attributes.Count);
                        array[attributes.Count + collection.Count] = new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden);
                        this.attributes = new AttributeCollection(array);
                    }
                    return this.attributes;
                }
            }

            public override Type ComponentType
            {
                get
                {
                    return this.componentType;
                }
            }

            public override bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public override Type PropertyType
            {
                get
                {
                    return this.activityProperty.Type;
                }
            }
        }
    }
}

