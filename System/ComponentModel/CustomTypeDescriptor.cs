namespace System.ComponentModel
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public abstract class CustomTypeDescriptor : ICustomTypeDescriptor
    {
        private ICustomTypeDescriptor _parent;

        protected CustomTypeDescriptor()
        {
        }

        protected CustomTypeDescriptor(ICustomTypeDescriptor parent)
        {
            this._parent = parent;
        }

        public virtual AttributeCollection GetAttributes()
        {
            if (this._parent != null)
            {
                return this._parent.GetAttributes();
            }
            return AttributeCollection.Empty;
        }

        public virtual string GetClassName()
        {
            if (this._parent != null)
            {
                return this._parent.GetClassName();
            }
            return null;
        }

        public virtual string GetComponentName()
        {
            if (this._parent != null)
            {
                return this._parent.GetComponentName();
            }
            return null;
        }

        public virtual TypeConverter GetConverter()
        {
            if (this._parent != null)
            {
                return this._parent.GetConverter();
            }
            return new TypeConverter();
        }

        public virtual EventDescriptor GetDefaultEvent()
        {
            if (this._parent != null)
            {
                return this._parent.GetDefaultEvent();
            }
            return null;
        }

        public virtual PropertyDescriptor GetDefaultProperty()
        {
            if (this._parent != null)
            {
                return this._parent.GetDefaultProperty();
            }
            return null;
        }

        public virtual object GetEditor(Type editorBaseType)
        {
            if (this._parent != null)
            {
                return this._parent.GetEditor(editorBaseType);
            }
            return null;
        }

        public virtual EventDescriptorCollection GetEvents()
        {
            if (this._parent != null)
            {
                return this._parent.GetEvents();
            }
            return EventDescriptorCollection.Empty;
        }

        public virtual EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            if (this._parent != null)
            {
                return this._parent.GetEvents(attributes);
            }
            return EventDescriptorCollection.Empty;
        }

        public virtual PropertyDescriptorCollection GetProperties()
        {
            if (this._parent != null)
            {
                return this._parent.GetProperties();
            }
            return PropertyDescriptorCollection.Empty;
        }

        public virtual PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            if (this._parent != null)
            {
                return this._parent.GetProperties(attributes);
            }
            return PropertyDescriptorCollection.Empty;
        }

        public virtual object GetPropertyOwner(PropertyDescriptor pd)
        {
            if (this._parent != null)
            {
                return this._parent.GetPropertyOwner(pd);
            }
            return null;
        }
    }
}

