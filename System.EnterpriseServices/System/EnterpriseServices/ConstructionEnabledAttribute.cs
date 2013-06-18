namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.EnterpriseServices.Admin;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Class, Inherited=true), ComVisible(false)]
    public sealed class ConstructionEnabledAttribute : Attribute, IConfigurationAttribute
    {
        private string _default;
        private bool _enabled;

        public ConstructionEnabledAttribute()
        {
            this._enabled = true;
            this._default = "";
        }

        public ConstructionEnabledAttribute(bool val)
        {
            this._enabled = val;
            this._default = "";
        }

        bool IConfigurationAttribute.AfterSaveChanges(Hashtable info)
        {
            return false;
        }

        bool IConfigurationAttribute.Apply(Hashtable info)
        {
            ICatalogObject obj2 = (ICatalogObject) info["Component"];
            obj2.SetValue("ConstructionEnabled", this._enabled);
            if ((this._default != null) && (this._default != ""))
            {
                obj2.SetValue("ConstructorString", this._default);
            }
            return true;
        }

        bool IConfigurationAttribute.IsValidTarget(string s)
        {
            return (s == "Component");
        }

        public string Default
        {
            get
            {
                return this._default;
            }
            set
            {
                this._default = value;
            }
        }

        public bool Enabled
        {
            get
            {
                return this._enabled;
            }
            set
            {
                this._enabled = value;
            }
        }
    }
}

