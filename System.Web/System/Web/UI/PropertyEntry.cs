namespace System.Web.UI
{
    using System;
    using System.Reflection;

    public abstract class PropertyEntry
    {
        private string _filter;
        private int _index;
        private string _name;
        private int _order;
        private System.Reflection.PropertyInfo _propertyInfo;
        private System.Type _type;

        internal PropertyEntry()
        {
        }

        public System.Type DeclaringType
        {
            get
            {
                if (this._propertyInfo == null)
                {
                    return null;
                }
                return this._propertyInfo.DeclaringType;
            }
        }

        public string Filter
        {
            get
            {
                return this._filter;
            }
            set
            {
                this._filter = value;
            }
        }

        internal int Index
        {
            get
            {
                return this._index;
            }
            set
            {
                this._index = value;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        internal int Order
        {
            get
            {
                return this._order;
            }
            set
            {
                this._order = value;
            }
        }

        public System.Reflection.PropertyInfo PropertyInfo
        {
            get
            {
                return this._propertyInfo;
            }
            set
            {
                this._propertyInfo = value;
            }
        }

        public System.Type Type
        {
            get
            {
                return this._type;
            }
            set
            {
                this._type = value;
            }
        }
    }
}

