namespace System.ComponentModel
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public abstract class PropertyDescriptor : MemberDescriptor
    {
        private TypeConverter converter;
        private int editorCount;
        private object[] editors;
        private Type[] editorTypes;
        private Hashtable valueChangedHandlers;

        protected PropertyDescriptor(MemberDescriptor descr) : base(descr)
        {
        }

        protected PropertyDescriptor(MemberDescriptor descr, Attribute[] attrs) : base(descr, attrs)
        {
        }

        protected PropertyDescriptor(string name, Attribute[] attrs) : base(name, attrs)
        {
        }

        public virtual void AddValueChanged(object component, EventHandler handler)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            if (this.valueChangedHandlers == null)
            {
                this.valueChangedHandlers = new Hashtable();
            }
            EventHandler a = (EventHandler) this.valueChangedHandlers[component];
            this.valueChangedHandlers[component] = Delegate.Combine(a, handler);
        }

        public abstract bool CanResetValue(object component);
        protected object CreateInstance(Type type)
        {
            Type[] types = new Type[] { typeof(Type) };
            if (type.GetConstructor(types) != null)
            {
                return TypeDescriptor.CreateInstance(null, type, types, new object[] { this.PropertyType });
            }
            return TypeDescriptor.CreateInstance(null, type, null, null);
        }

        public override bool Equals(object obj)
        {
            try
            {
                if (obj == this)
                {
                    return true;
                }
                if (obj == null)
                {
                    return false;
                }
                PropertyDescriptor descriptor = obj as PropertyDescriptor;
                if (((descriptor != null) && (descriptor.NameHashCode == this.NameHashCode)) && ((descriptor.PropertyType == this.PropertyType) && descriptor.Name.Equals(this.Name)))
                {
                    return true;
                }
            }
            catch
            {
            }
            return false;
        }

        protected override void FillAttributes(IList attributeList)
        {
            this.converter = null;
            this.editors = null;
            this.editorTypes = null;
            this.editorCount = 0;
            base.FillAttributes(attributeList);
        }

        public PropertyDescriptorCollection GetChildProperties()
        {
            return this.GetChildProperties(null, null);
        }

        public PropertyDescriptorCollection GetChildProperties(Attribute[] filter)
        {
            return this.GetChildProperties(null, filter);
        }

        public PropertyDescriptorCollection GetChildProperties(object instance)
        {
            return this.GetChildProperties(instance, null);
        }

        public virtual PropertyDescriptorCollection GetChildProperties(object instance, Attribute[] filter)
        {
            if (instance == null)
            {
                return TypeDescriptor.GetProperties(this.PropertyType, filter);
            }
            return TypeDescriptor.GetProperties(instance, filter);
        }

        public virtual object GetEditor(Type editorBaseType)
        {
            object editor = null;
            AttributeCollection attributes = this.Attributes;
            if (this.editorTypes != null)
            {
                for (int i = 0; i < this.editorCount; i++)
                {
                    if (this.editorTypes[i] == editorBaseType)
                    {
                        return this.editors[i];
                    }
                }
            }
            if (editor == null)
            {
                for (int j = 0; j < attributes.Count; j++)
                {
                    EditorAttribute attribute = attributes[j] as EditorAttribute;
                    if (attribute != null)
                    {
                        Type typeFromName = this.GetTypeFromName(attribute.EditorBaseTypeName);
                        if (editorBaseType == typeFromName)
                        {
                            Type type = this.GetTypeFromName(attribute.EditorTypeName);
                            if (type != null)
                            {
                                editor = this.CreateInstance(type);
                                break;
                            }
                        }
                    }
                }
                if (editor == null)
                {
                    editor = TypeDescriptor.GetEditor(this.PropertyType, editorBaseType);
                }
                if (this.editorTypes == null)
                {
                    this.editorTypes = new Type[5];
                    this.editors = new object[5];
                }
                if (this.editorCount >= this.editorTypes.Length)
                {
                    Type[] destinationArray = new Type[this.editorTypes.Length * 2];
                    object[] objArray = new object[this.editors.Length * 2];
                    Array.Copy(this.editorTypes, destinationArray, this.editorTypes.Length);
                    Array.Copy(this.editors, objArray, this.editors.Length);
                    this.editorTypes = destinationArray;
                    this.editors = objArray;
                }
                this.editorTypes[this.editorCount] = editorBaseType;
                this.editors[this.editorCount++] = editor;
            }
            return editor;
        }

        public override int GetHashCode()
        {
            return (this.NameHashCode ^ this.PropertyType.GetHashCode());
        }

        protected override object GetInvocationTarget(Type type, object instance)
        {
            object invocationTarget = base.GetInvocationTarget(type, instance);
            ICustomTypeDescriptor descriptor = invocationTarget as ICustomTypeDescriptor;
            if (descriptor != null)
            {
                invocationTarget = descriptor.GetPropertyOwner(this);
            }
            return invocationTarget;
        }

        protected Type GetTypeFromName(string typeName)
        {
            if ((typeName == null) || (typeName.Length == 0))
            {
                return null;
            }
            Type type = Type.GetType(typeName);
            Type type2 = null;
            if ((this.ComponentType != null) && ((type == null) || this.ComponentType.Assembly.FullName.Equals(type.Assembly.FullName)))
            {
                int index = typeName.IndexOf(',');
                if (index != -1)
                {
                    typeName = typeName.Substring(0, index);
                }
                type2 = this.ComponentType.Assembly.GetType(typeName);
            }
            return (type2 ?? type);
        }

        public abstract object GetValue(object component);
        protected internal EventHandler GetValueChangedHandler(object component)
        {
            if ((component != null) && (this.valueChangedHandlers != null))
            {
                return (EventHandler) this.valueChangedHandlers[component];
            }
            return null;
        }

        protected virtual void OnValueChanged(object component, EventArgs e)
        {
            if ((component != null) && (this.valueChangedHandlers != null))
            {
                EventHandler handler = (EventHandler) this.valueChangedHandlers[component];
                if (handler != null)
                {
                    handler(component, e);
                }
            }
        }

        public virtual void RemoveValueChanged(object component, EventHandler handler)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            if (this.valueChangedHandlers != null)
            {
                EventHandler source = (EventHandler) this.valueChangedHandlers[component];
                source = (EventHandler) Delegate.Remove(source, handler);
                if (source != null)
                {
                    this.valueChangedHandlers[component] = source;
                }
                else
                {
                    this.valueChangedHandlers.Remove(component);
                }
            }
        }

        public abstract void ResetValue(object component);
        public abstract void SetValue(object component, object value);
        public abstract bool ShouldSerializeValue(object component);

        public abstract Type ComponentType { get; }

        public virtual TypeConverter Converter
        {
            get
            {
                AttributeCollection attributes = this.Attributes;
                if (this.converter == null)
                {
                    TypeConverterAttribute attribute = (TypeConverterAttribute) attributes[typeof(TypeConverterAttribute)];
                    if ((attribute.ConverterTypeName != null) && (attribute.ConverterTypeName.Length > 0))
                    {
                        Type typeFromName = this.GetTypeFromName(attribute.ConverterTypeName);
                        if ((typeFromName != null) && typeof(TypeConverter).IsAssignableFrom(typeFromName))
                        {
                            this.converter = (TypeConverter) this.CreateInstance(typeFromName);
                        }
                    }
                    if (this.converter == null)
                    {
                        this.converter = TypeDescriptor.GetConverter(this.PropertyType);
                    }
                }
                return this.converter;
            }
        }

        public virtual bool IsLocalizable
        {
            get
            {
                return LocalizableAttribute.Yes.Equals(this.Attributes[typeof(LocalizableAttribute)]);
            }
        }

        public abstract bool IsReadOnly { get; }

        public abstract Type PropertyType { get; }

        public DesignerSerializationVisibility SerializationVisibility
        {
            get
            {
                DesignerSerializationVisibilityAttribute attribute = (DesignerSerializationVisibilityAttribute) this.Attributes[typeof(DesignerSerializationVisibilityAttribute)];
                return attribute.Visibility;
            }
        }

        public virtual bool SupportsChangeEvents
        {
            get
            {
                return false;
            }
        }
    }
}

