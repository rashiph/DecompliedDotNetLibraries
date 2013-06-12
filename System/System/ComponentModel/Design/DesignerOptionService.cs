namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public abstract class DesignerOptionService : IDesignerOptionService
    {
        private DesignerOptionCollection _options;

        protected DesignerOptionService()
        {
        }

        protected DesignerOptionCollection CreateOptionCollection(DesignerOptionCollection parent, string name, object value)
        {
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                object[] args = new object[] { name.Length.ToString(CultureInfo.CurrentCulture), 0.ToString(CultureInfo.CurrentCulture) };
                throw new ArgumentException(SR.GetString("InvalidArgument", args), "name.Length");
            }
            return new DesignerOptionCollection(this, parent, name, value);
        }

        private PropertyDescriptor GetOptionProperty(string pageName, string valueName)
        {
            if (pageName == null)
            {
                throw new ArgumentNullException("pageName");
            }
            if (valueName == null)
            {
                throw new ArgumentNullException("valueName");
            }
            string[] strArray = pageName.Split(new char[] { '\\' });
            DesignerOptionCollection options = this.Options;
            foreach (string str in strArray)
            {
                options = options[str];
                if (options == null)
                {
                    return null;
                }
            }
            return options.Properties[valueName];
        }

        protected virtual void PopulateOptionCollection(DesignerOptionCollection options)
        {
        }

        protected virtual bool ShowDialog(DesignerOptionCollection options, object optionObject)
        {
            return false;
        }

        object IDesignerOptionService.GetOptionValue(string pageName, string valueName)
        {
            PropertyDescriptor optionProperty = this.GetOptionProperty(pageName, valueName);
            if (optionProperty != null)
            {
                return optionProperty.GetValue(null);
            }
            return null;
        }

        void IDesignerOptionService.SetOptionValue(string pageName, string valueName, object value)
        {
            PropertyDescriptor optionProperty = this.GetOptionProperty(pageName, valueName);
            if (optionProperty != null)
            {
                optionProperty.SetValue(null, value);
            }
        }

        public DesignerOptionCollection Options
        {
            get
            {
                if (this._options == null)
                {
                    this._options = new DesignerOptionCollection(this, null, string.Empty, null);
                }
                return this._options;
            }
        }

        [Editor("", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), TypeConverter(typeof(DesignerOptionService.DesignerOptionConverter))]
        public sealed class DesignerOptionCollection : IList, ICollection, IEnumerable
        {
            private ArrayList _children;
            private string _name;
            private DesignerOptionService.DesignerOptionCollection _parent;
            private PropertyDescriptorCollection _properties;
            private DesignerOptionService _service;
            private object _value;

            internal DesignerOptionCollection(DesignerOptionService service, DesignerOptionService.DesignerOptionCollection parent, string name, object value)
            {
                this._service = service;
                this._parent = parent;
                this._name = name;
                this._value = value;
                if (this._parent != null)
                {
                    if (this._parent._children == null)
                    {
                        this._parent._children = new ArrayList(1);
                    }
                    this._parent._children.Add(this);
                }
            }

            public void CopyTo(Array array, int index)
            {
                this.EnsurePopulated();
                this._children.CopyTo(array, index);
            }

            private void EnsurePopulated()
            {
                if (this._children == null)
                {
                    this._service.PopulateOptionCollection(this);
                    if (this._children == null)
                    {
                        this._children = new ArrayList(1);
                    }
                }
            }

            public IEnumerator GetEnumerator()
            {
                this.EnsurePopulated();
                return this._children.GetEnumerator();
            }

            public int IndexOf(DesignerOptionService.DesignerOptionCollection value)
            {
                this.EnsurePopulated();
                return this._children.IndexOf(value);
            }

            private static object RecurseFindValue(DesignerOptionService.DesignerOptionCollection options)
            {
                if (options._value != null)
                {
                    return options._value;
                }
                foreach (DesignerOptionService.DesignerOptionCollection options2 in options)
                {
                    object obj2 = RecurseFindValue(options2);
                    if (obj2 != null)
                    {
                        return obj2;
                    }
                }
                return null;
            }

            public bool ShowDialog()
            {
                object optionObject = RecurseFindValue(this);
                if (optionObject == null)
                {
                    return false;
                }
                return this._service.ShowDialog(this, optionObject);
            }

            int IList.Add(object value)
            {
                throw new NotSupportedException();
            }

            void IList.Clear()
            {
                throw new NotSupportedException();
            }

            bool IList.Contains(object value)
            {
                this.EnsurePopulated();
                return this._children.Contains(value);
            }

            int IList.IndexOf(object value)
            {
                this.EnsurePopulated();
                return this._children.IndexOf(value);
            }

            void IList.Insert(int index, object value)
            {
                throw new NotSupportedException();
            }

            void IList.Remove(object value)
            {
                throw new NotSupportedException();
            }

            void IList.RemoveAt(int index)
            {
                throw new NotSupportedException();
            }

            public int Count
            {
                get
                {
                    this.EnsurePopulated();
                    return this._children.Count;
                }
            }

            public DesignerOptionService.DesignerOptionCollection this[int index]
            {
                get
                {
                    this.EnsurePopulated();
                    if ((index < 0) || (index >= this._children.Count))
                    {
                        throw new IndexOutOfRangeException("index");
                    }
                    return (DesignerOptionService.DesignerOptionCollection) this._children[index];
                }
            }

            public DesignerOptionService.DesignerOptionCollection this[string name]
            {
                get
                {
                    this.EnsurePopulated();
                    foreach (DesignerOptionService.DesignerOptionCollection options in this._children)
                    {
                        if (string.Compare(options.Name, name, true, CultureInfo.InvariantCulture) == 0)
                        {
                            return options;
                        }
                    }
                    return null;
                }
            }

            public string Name
            {
                get
                {
                    return this._name;
                }
            }

            public DesignerOptionService.DesignerOptionCollection Parent
            {
                get
                {
                    return this._parent;
                }
            }

            public PropertyDescriptorCollection Properties
            {
                get
                {
                    if (this._properties == null)
                    {
                        ArrayList list;
                        if (this._value != null)
                        {
                            PropertyDescriptorCollection descriptors = TypeDescriptor.GetProperties(this._value);
                            list = new ArrayList(descriptors.Count);
                            foreach (PropertyDescriptor descriptor in descriptors)
                            {
                                list.Add(new WrappedPropertyDescriptor(descriptor, this._value));
                            }
                        }
                        else
                        {
                            list = new ArrayList(1);
                        }
                        this.EnsurePopulated();
                        foreach (DesignerOptionService.DesignerOptionCollection options in this._children)
                        {
                            list.AddRange(options.Properties);
                        }
                        PropertyDescriptor[] properties = (PropertyDescriptor[]) list.ToArray(typeof(PropertyDescriptor));
                        this._properties = new PropertyDescriptorCollection(properties, true);
                    }
                    return this._properties;
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
                    return this;
                }
            }

            bool IList.IsFixedSize
            {
                get
                {
                    return true;
                }
            }

            bool IList.IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            object IList.this[int index]
            {
                get
                {
                    return this[index];
                }
                set
                {
                    throw new NotSupportedException();
                }
            }

            private sealed class WrappedPropertyDescriptor : PropertyDescriptor
            {
                private PropertyDescriptor property;
                private object target;

                internal WrappedPropertyDescriptor(PropertyDescriptor property, object target) : base(property.Name, null)
                {
                    this.property = property;
                    this.target = target;
                }

                public override bool CanResetValue(object component)
                {
                    return this.property.CanResetValue(this.target);
                }

                public override object GetValue(object component)
                {
                    return this.property.GetValue(this.target);
                }

                public override void ResetValue(object component)
                {
                    this.property.ResetValue(this.target);
                }

                public override void SetValue(object component, object value)
                {
                    this.property.SetValue(this.target, value);
                }

                public override bool ShouldSerializeValue(object component)
                {
                    return this.property.ShouldSerializeValue(this.target);
                }

                public override AttributeCollection Attributes
                {
                    get
                    {
                        return this.property.Attributes;
                    }
                }

                public override Type ComponentType
                {
                    get
                    {
                        return this.property.ComponentType;
                    }
                }

                public override bool IsReadOnly
                {
                    get
                    {
                        return this.property.IsReadOnly;
                    }
                }

                public override Type PropertyType
                {
                    get
                    {
                        return this.property.PropertyType;
                    }
                }
            }
        }

        internal sealed class DesignerOptionConverter : TypeConverter
        {
            public override object ConvertTo(ITypeDescriptorContext cxt, CultureInfo culture, object value, Type destinationType)
            {
                if (destinationType == typeof(string))
                {
                    return SR.GetString("CollectionConverterText");
                }
                return base.ConvertTo(cxt, culture, value, destinationType);
            }

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext cxt, object value, Attribute[] attributes)
            {
                PropertyDescriptorCollection descriptors = new PropertyDescriptorCollection(null);
                DesignerOptionService.DesignerOptionCollection options = value as DesignerOptionService.DesignerOptionCollection;
                if (options != null)
                {
                    foreach (DesignerOptionService.DesignerOptionCollection options2 in options)
                    {
                        descriptors.Add(new OptionPropertyDescriptor(options2));
                    }
                    foreach (PropertyDescriptor descriptor in options.Properties)
                    {
                        descriptors.Add(descriptor);
                    }
                }
                return descriptors;
            }

            public override bool GetPropertiesSupported(ITypeDescriptorContext cxt)
            {
                return true;
            }

            private class OptionPropertyDescriptor : PropertyDescriptor
            {
                private DesignerOptionService.DesignerOptionCollection _option;

                internal OptionPropertyDescriptor(DesignerOptionService.DesignerOptionCollection option) : base(option.Name, null)
                {
                    this._option = option;
                }

                public override bool CanResetValue(object component)
                {
                    return false;
                }

                public override object GetValue(object component)
                {
                    return this._option;
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
                        return this._option.GetType();
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
                        return this._option.GetType();
                    }
                }
            }
        }
    }
}

