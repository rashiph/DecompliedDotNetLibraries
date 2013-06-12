namespace System.ComponentModel
{
    using System;
    using System.Collections;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public abstract class TypeDescriptionProvider
    {
        private EmptyCustomTypeDescriptor _emptyDescriptor;
        private TypeDescriptionProvider _parent;

        protected TypeDescriptionProvider()
        {
        }

        protected TypeDescriptionProvider(TypeDescriptionProvider parent)
        {
            this._parent = parent;
        }

        public virtual object CreateInstance(IServiceProvider provider, Type objectType, Type[] argTypes, object[] args)
        {
            if (this._parent != null)
            {
                return this._parent.CreateInstance(provider, objectType, argTypes, args);
            }
            if (objectType == null)
            {
                throw new ArgumentNullException("objectType");
            }
            return SecurityUtils.SecureCreateInstance(objectType, args);
        }

        public virtual IDictionary GetCache(object instance)
        {
            if (this._parent != null)
            {
                return this._parent.GetCache(instance);
            }
            return null;
        }

        public virtual ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
        {
            if (this._parent != null)
            {
                return this._parent.GetExtendedTypeDescriptor(instance);
            }
            if (this._emptyDescriptor == null)
            {
                this._emptyDescriptor = new EmptyCustomTypeDescriptor();
            }
            return this._emptyDescriptor;
        }

        protected internal virtual IExtenderProvider[] GetExtenderProviders(object instance)
        {
            if (this._parent != null)
            {
                return this._parent.GetExtenderProviders(instance);
            }
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            return new IExtenderProvider[0];
        }

        public virtual string GetFullComponentName(object component)
        {
            if (this._parent != null)
            {
                return this._parent.GetFullComponentName(component);
            }
            return this.GetTypeDescriptor(component).GetComponentName();
        }

        public Type GetReflectionType(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            return this.GetReflectionType(instance.GetType(), instance);
        }

        public Type GetReflectionType(Type objectType)
        {
            return this.GetReflectionType(objectType, null);
        }

        public virtual Type GetReflectionType(Type objectType, object instance)
        {
            if (this._parent != null)
            {
                return this._parent.GetReflectionType(objectType, instance);
            }
            return objectType;
        }

        public virtual Type GetRuntimeType(Type reflectionType)
        {
            if (this._parent != null)
            {
                return this._parent.GetRuntimeType(reflectionType);
            }
            if (reflectionType == null)
            {
                throw new ArgumentNullException("reflectionType");
            }
            if (reflectionType.GetType().Assembly == typeof(object).Assembly)
            {
                return reflectionType;
            }
            return reflectionType.UnderlyingSystemType;
        }

        public ICustomTypeDescriptor GetTypeDescriptor(object instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            return this.GetTypeDescriptor(instance.GetType(), instance);
        }

        public ICustomTypeDescriptor GetTypeDescriptor(Type objectType)
        {
            return this.GetTypeDescriptor(objectType, null);
        }

        public virtual ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            if (this._parent != null)
            {
                return this._parent.GetTypeDescriptor(objectType, instance);
            }
            if (this._emptyDescriptor == null)
            {
                this._emptyDescriptor = new EmptyCustomTypeDescriptor();
            }
            return this._emptyDescriptor;
        }

        public virtual bool IsSupportedType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (this._parent != null)
            {
                return this._parent.IsSupportedType(type);
            }
            return true;
        }

        private sealed class EmptyCustomTypeDescriptor : CustomTypeDescriptor
        {
        }
    }
}

