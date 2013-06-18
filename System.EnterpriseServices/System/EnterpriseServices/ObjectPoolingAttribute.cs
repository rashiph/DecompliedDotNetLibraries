namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.EnterpriseServices.Admin;
    using System.Runtime.InteropServices;

    [ComVisible(false), AttributeUsage(AttributeTargets.Class, Inherited=true)]
    public sealed class ObjectPoolingAttribute : Attribute, IConfigurationAttribute
    {
        private bool _enable;
        private int _maxSize;
        private int _minSize;
        private int _timeout;

        public ObjectPoolingAttribute()
        {
            this._enable = true;
            this._maxSize = -1;
            this._minSize = -1;
            this._timeout = -1;
        }

        public ObjectPoolingAttribute(bool enable)
        {
            this._enable = enable;
            this._maxSize = -1;
            this._minSize = -1;
            this._timeout = -1;
        }

        public ObjectPoolingAttribute(int minPoolSize, int maxPoolSize)
        {
            this._enable = true;
            this._maxSize = maxPoolSize;
            this._minSize = minPoolSize;
            this._timeout = -1;
        }

        public ObjectPoolingAttribute(bool enable, int minPoolSize, int maxPoolSize)
        {
            this._enable = enable;
            this._maxSize = maxPoolSize;
            this._minSize = minPoolSize;
            this._timeout = -1;
        }

        public bool AfterSaveChanges(Hashtable info)
        {
            return false;
        }

        public bool Apply(Hashtable info)
        {
            ICatalogObject obj2 = (ICatalogObject) info["Component"];
            obj2.SetValue("ObjectPoolingEnabled", this._enable);
            if (this._minSize >= 0)
            {
                obj2.SetValue("MinPoolSize", this._minSize);
            }
            if (this._maxSize >= 0)
            {
                obj2.SetValue("MaxPoolSize", this._maxSize);
            }
            if (this._timeout >= 0)
            {
                obj2.SetValue("CreationTimeout", this._timeout);
            }
            return true;
        }

        public bool IsValidTarget(string s)
        {
            return (s == "Component");
        }

        public int CreationTimeout
        {
            get
            {
                return this._timeout;
            }
            set
            {
                this._timeout = value;
            }
        }

        public bool Enabled
        {
            get
            {
                return this._enable;
            }
            set
            {
                this._enable = value;
            }
        }

        public int MaxPoolSize
        {
            get
            {
                return this._maxSize;
            }
            set
            {
                this._maxSize = value;
            }
        }

        public int MinPoolSize
        {
            get
            {
                return this._minSize;
            }
            set
            {
                this._minSize = value;
            }
        }
    }
}

