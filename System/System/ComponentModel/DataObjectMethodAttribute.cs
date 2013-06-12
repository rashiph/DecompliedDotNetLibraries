namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class DataObjectMethodAttribute : Attribute
    {
        private bool _isDefault;
        private DataObjectMethodType _methodType;

        public DataObjectMethodAttribute(DataObjectMethodType methodType) : this(methodType, false)
        {
        }

        public DataObjectMethodAttribute(DataObjectMethodType methodType, bool isDefault)
        {
            this._methodType = methodType;
            this._isDefault = isDefault;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            DataObjectMethodAttribute attribute = obj as DataObjectMethodAttribute;
            return (((attribute != null) && (attribute.MethodType == this.MethodType)) && (attribute.IsDefault == this.IsDefault));
        }

        public override int GetHashCode()
        {
            return (((int) this._methodType).GetHashCode() ^ this._isDefault.GetHashCode());
        }

        public override bool Match(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            DataObjectMethodAttribute attribute = obj as DataObjectMethodAttribute;
            return ((attribute != null) && (attribute.MethodType == this.MethodType));
        }

        public bool IsDefault
        {
            get
            {
                return this._isDefault;
            }
        }

        public DataObjectMethodType MethodType
        {
            get
            {
                return this._methodType;
            }
        }
    }
}

