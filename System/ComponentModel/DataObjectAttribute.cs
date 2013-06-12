namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DataObjectAttribute : Attribute
    {
        private bool _isDataObject;
        public static readonly DataObjectAttribute DataObject = new DataObjectAttribute(true);
        public static readonly DataObjectAttribute Default = NonDataObject;
        public static readonly DataObjectAttribute NonDataObject = new DataObjectAttribute(false);

        public DataObjectAttribute() : this(true)
        {
        }

        public DataObjectAttribute(bool isDataObject)
        {
            this._isDataObject = isDataObject;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            DataObjectAttribute attribute = obj as DataObjectAttribute;
            return ((attribute != null) && (attribute.IsDataObject == this.IsDataObject));
        }

        public override int GetHashCode()
        {
            return this._isDataObject.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }

        public bool IsDataObject
        {
            get
            {
                return this._isDataObject;
            }
        }
    }
}

