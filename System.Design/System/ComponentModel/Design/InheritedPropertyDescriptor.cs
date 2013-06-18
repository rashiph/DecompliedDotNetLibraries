namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Reflection;

    internal class InheritedPropertyDescriptor : System.ComponentModel.PropertyDescriptor
    {
        private object defaultValue;
        private bool initShouldSerialize;
        private static object noDefault = new object();
        private object originalValue;
        private System.ComponentModel.PropertyDescriptor propertyDescriptor;

        public InheritedPropertyDescriptor(System.ComponentModel.PropertyDescriptor propertyDescriptor, object component, bool rootComponent) : base(propertyDescriptor, new Attribute[0])
        {
            this.propertyDescriptor = propertyDescriptor;
            this.InitInheritedDefaultValue(component, rootComponent);
            bool flag = false;
            if (typeof(ICollection).IsAssignableFrom(propertyDescriptor.PropertyType) && propertyDescriptor.Attributes.Contains(DesignerSerializationVisibilityAttribute.Content))
            {
                ICollection instance = propertyDescriptor.GetValue(component) as ICollection;
                if ((instance != null) && (instance.Count > 0))
                {
                    bool flag2 = false;
                    bool flag3 = false;
                    foreach (MethodInfo info in TypeDescriptor.GetReflectionType(instance).GetMethods(BindingFlags.Public | BindingFlags.Instance))
                    {
                        ParameterInfo[] parameters = info.GetParameters();
                        if (parameters.Length == 1)
                        {
                            string name = info.Name;
                            Type c = null;
                            if (name.Equals("AddRange") && parameters[0].ParameterType.IsArray)
                            {
                                c = parameters[0].ParameterType.GetElementType();
                            }
                            else if (name.Equals("Add"))
                            {
                                c = parameters[0].ParameterType;
                            }
                            if (c != null)
                            {
                                if (!typeof(IComponent).IsAssignableFrom(c))
                                {
                                    flag3 = true;
                                }
                                else
                                {
                                    flag2 = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (flag3 && !flag2)
                    {
                        ArrayList list = new ArrayList(this.AttributeArray);
                        list.Add(DesignerSerializationVisibilityAttribute.Hidden);
                        list.Add(ReadOnlyAttribute.Yes);
                        list.Add(new EditorAttribute(typeof(UITypeEditor), typeof(UITypeEditor)));
                        list.Add(new TypeConverterAttribute(typeof(ReadOnlyCollectionConverter)));
                        Attribute[] attributeArray = (Attribute[]) list.ToArray(typeof(Attribute));
                        this.AttributeArray = attributeArray;
                        flag = true;
                    }
                }
            }
            if (!flag && (this.defaultValue != noDefault))
            {
                ArrayList list2 = new ArrayList(this.AttributeArray);
                list2.Add(new DefaultValueAttribute(this.defaultValue));
                Attribute[] array = new Attribute[list2.Count];
                list2.CopyTo(array, 0);
                this.AttributeArray = array;
            }
        }

        public override bool CanResetValue(object component)
        {
            if (this.defaultValue == noDefault)
            {
                return this.propertyDescriptor.CanResetValue(component);
            }
            return !object.Equals(this.GetValue(component), this.defaultValue);
        }

        private object ClonedDefaultValue(object value)
        {
            DesignerSerializationVisibility visible;
            DesignerSerializationVisibilityAttribute attribute = (DesignerSerializationVisibilityAttribute) this.propertyDescriptor.Attributes[typeof(DesignerSerializationVisibilityAttribute)];
            if (attribute == null)
            {
                visible = DesignerSerializationVisibility.Visible;
            }
            else
            {
                visible = attribute.Visibility;
            }
            if ((value != null) && (visible == DesignerSerializationVisibility.Content))
            {
                if (value is ICloneable)
                {
                    value = ((ICloneable) value).Clone();
                    return value;
                }
                value = noDefault;
            }
            return value;
        }

        protected override void FillAttributes(IList attributeList)
        {
            base.FillAttributes(attributeList);
            foreach (Attribute attribute in this.propertyDescriptor.Attributes)
            {
                attributeList.Add(attribute);
            }
        }

        public override object GetValue(object component)
        {
            return this.propertyDescriptor.GetValue(component);
        }

        private void InitInheritedDefaultValue(object component, bool rootComponent)
        {
            try
            {
                object defaultValue;
                if (!this.propertyDescriptor.ShouldSerializeValue(component))
                {
                    DefaultValueAttribute attribute = (DefaultValueAttribute) this.propertyDescriptor.Attributes[typeof(DefaultValueAttribute)];
                    if (attribute != null)
                    {
                        this.defaultValue = attribute.Value;
                        defaultValue = this.defaultValue;
                    }
                    else
                    {
                        this.defaultValue = noDefault;
                        defaultValue = this.propertyDescriptor.GetValue(component);
                    }
                }
                else
                {
                    this.defaultValue = this.propertyDescriptor.GetValue(component);
                    defaultValue = this.defaultValue;
                    this.defaultValue = this.ClonedDefaultValue(this.defaultValue);
                }
                this.SaveOriginalValue(defaultValue);
            }
            catch
            {
                this.defaultValue = noDefault;
            }
            this.initShouldSerialize = this.ShouldSerializeValue(component);
        }

        public override void ResetValue(object component)
        {
            if (this.defaultValue == noDefault)
            {
                this.propertyDescriptor.ResetValue(component);
            }
            else
            {
                this.SetValue(component, this.defaultValue);
            }
        }

        private void SaveOriginalValue(object value)
        {
            if (value is ICollection)
            {
                this.originalValue = new object[((ICollection) value).Count];
                ((ICollection) value).CopyTo((Array) this.originalValue, 0);
            }
            else
            {
                this.originalValue = value;
            }
        }

        public override void SetValue(object component, object value)
        {
            this.propertyDescriptor.SetValue(component, value);
        }

        public override bool ShouldSerializeValue(object component)
        {
            if (this.IsReadOnly)
            {
                return (this.propertyDescriptor.ShouldSerializeValue(component) && this.Attributes.Contains(DesignerSerializationVisibilityAttribute.Content));
            }
            if (this.defaultValue == noDefault)
            {
                return this.propertyDescriptor.ShouldSerializeValue(component);
            }
            return !object.Equals(this.GetValue(component), this.defaultValue);
        }

        public override Type ComponentType
        {
            get
            {
                return this.propertyDescriptor.ComponentType;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                if (!this.propertyDescriptor.IsReadOnly)
                {
                    return this.Attributes[typeof(ReadOnlyAttribute)].Equals(ReadOnlyAttribute.Yes);
                }
                return true;
            }
        }

        internal object OriginalValue
        {
            get
            {
                return this.originalValue;
            }
        }

        internal System.ComponentModel.PropertyDescriptor PropertyDescriptor
        {
            get
            {
                return this.propertyDescriptor;
            }
            set
            {
                this.propertyDescriptor = value;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return this.propertyDescriptor.PropertyType;
            }
        }

        private class ReadOnlyCollectionConverter : TypeConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                {
                    return System.Design.SR.GetString("InheritanceServiceReadOnlyCollection");
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}

