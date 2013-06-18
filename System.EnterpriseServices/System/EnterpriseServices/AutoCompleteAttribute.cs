namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.EnterpriseServices.Admin;
    using System.Runtime.InteropServices;

    [ComVisible(false), AttributeUsage(AttributeTargets.Method, Inherited=true)]
    public sealed class AutoCompleteAttribute : Attribute, IConfigurationAttribute
    {
        private bool _value;

        public AutoCompleteAttribute() : this(true)
        {
        }

        public AutoCompleteAttribute(bool val)
        {
            this._value = val;
        }

        bool IConfigurationAttribute.AfterSaveChanges(Hashtable info)
        {
            return false;
        }

        bool IConfigurationAttribute.Apply(Hashtable info)
        {
            ((ICatalogObject) info["Method"]).SetValue("AutoComplete", this._value);
            return true;
        }

        bool IConfigurationAttribute.IsValidTarget(string s)
        {
            return (s == "Method");
        }

        public bool Value
        {
            get
            {
                return this._value;
            }
        }
    }
}

