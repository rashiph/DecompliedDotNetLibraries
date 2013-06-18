namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal sealed class TypeFieldSchema : IDataSourceFieldSchema
    {
        private PropertyDescriptor _fieldDescriptor;
        private bool _isIdentity;
        private bool _isNullable;
        private int _length = -1;
        private bool _primaryKey;
        private bool _retrievedMetaData;

        public TypeFieldSchema(PropertyDescriptor fieldDescriptor)
        {
            if (fieldDescriptor == null)
            {
                throw new ArgumentNullException("fieldDescriptor");
            }
            this._fieldDescriptor = fieldDescriptor;
        }

        private void EnsureMetaData()
        {
            if (!this._retrievedMetaData)
            {
                DataObjectFieldAttribute attribute = (DataObjectFieldAttribute) this._fieldDescriptor.Attributes[typeof(DataObjectFieldAttribute)];
                if (attribute != null)
                {
                    this._primaryKey = attribute.PrimaryKey;
                    this._isIdentity = attribute.IsIdentity;
                    this._isNullable = attribute.IsNullable;
                    this._length = attribute.Length;
                }
                this._retrievedMetaData = true;
            }
        }

        public Type DataType
        {
            get
            {
                Type propertyType = this._fieldDescriptor.PropertyType;
                if (propertyType.IsGenericType && (propertyType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                {
                    return propertyType.GetGenericArguments()[0];
                }
                return propertyType;
            }
        }

        public bool Identity
        {
            get
            {
                this.EnsureMetaData();
                return this._isIdentity;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this._fieldDescriptor.IsReadOnly;
            }
        }

        public bool IsUnique
        {
            get
            {
                return false;
            }
        }

        public int Length
        {
            get
            {
                this.EnsureMetaData();
                return this._length;
            }
        }

        public string Name
        {
            get
            {
                return this._fieldDescriptor.Name;
            }
        }

        public bool Nullable
        {
            get
            {
                this.EnsureMetaData();
                Type propertyType = this._fieldDescriptor.PropertyType;
                return ((!propertyType.IsValueType || this._isNullable) || (propertyType.IsGenericType && (propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))));
            }
        }

        public int Precision
        {
            get
            {
                return -1;
            }
        }

        public bool PrimaryKey
        {
            get
            {
                this.EnsureMetaData();
                return this._primaryKey;
            }
        }

        public int Scale
        {
            get
            {
                return -1;
            }
        }
    }
}

