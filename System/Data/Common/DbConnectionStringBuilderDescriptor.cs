namespace System.Data.Common
{
    using System;
    using System.ComponentModel;

    internal class DbConnectionStringBuilderDescriptor : PropertyDescriptor
    {
        private Type _componentType;
        private bool _isReadOnly;
        private Type _propertyType;
        private bool _refreshOnChange;

        internal DbConnectionStringBuilderDescriptor(string propertyName, Type componentType, Type propertyType, bool isReadOnly, Attribute[] attributes) : base(propertyName, attributes)
        {
            this._componentType = componentType;
            this._propertyType = propertyType;
            this._isReadOnly = isReadOnly;
        }

        public override bool CanResetValue(object component)
        {
            DbConnectionStringBuilder builder = component as DbConnectionStringBuilder;
            return ((builder != null) && builder.ShouldSerialize(this.DisplayName));
        }

        public override object GetValue(object component)
        {
            object obj2;
            DbConnectionStringBuilder builder = component as DbConnectionStringBuilder;
            if ((builder != null) && builder.TryGetValue(this.DisplayName, out obj2))
            {
                return obj2;
            }
            return null;
        }

        public override void ResetValue(object component)
        {
            DbConnectionStringBuilder builder = component as DbConnectionStringBuilder;
            if (builder != null)
            {
                builder.Remove(this.DisplayName);
                if (this.RefreshOnChange)
                {
                    builder.ClearPropertyDescriptors();
                }
            }
        }

        public override void SetValue(object component, object value)
        {
            DbConnectionStringBuilder builder = component as DbConnectionStringBuilder;
            if (builder != null)
            {
                if ((typeof(string) == this.PropertyType) && string.Empty.Equals(value))
                {
                    value = null;
                }
                builder[this.DisplayName] = value;
                if (this.RefreshOnChange)
                {
                    builder.ClearPropertyDescriptors();
                }
            }
        }

        public override bool ShouldSerializeValue(object component)
        {
            DbConnectionStringBuilder builder = component as DbConnectionStringBuilder;
            return ((builder != null) && builder.ShouldSerialize(this.DisplayName));
        }

        public override Type ComponentType
        {
            get
            {
                return this._componentType;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return this._isReadOnly;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return this._propertyType;
            }
        }

        internal bool RefreshOnChange
        {
            get
            {
                return this._refreshOnChange;
            }
            set
            {
                this._refreshOnChange = value;
            }
        }
    }
}

