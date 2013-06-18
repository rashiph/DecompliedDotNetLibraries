namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.EnterpriseServices.Admin;
    using System.Runtime.InteropServices;

    [ComVisible(false), AttributeUsage(AttributeTargets.Class, Inherited=true)]
    public sealed class SynchronizationAttribute : Attribute, IConfigurationAttribute
    {
        private SynchronizationOption _value;

        public SynchronizationAttribute() : this(SynchronizationOption.Required)
        {
        }

        public SynchronizationAttribute(SynchronizationOption val)
        {
            this._value = val;
        }

        bool IConfigurationAttribute.AfterSaveChanges(Hashtable info)
        {
            return false;
        }

        bool IConfigurationAttribute.Apply(Hashtable info)
        {
            ((ICatalogObject) info["Component"]).SetValue("Synchronization", this._value);
            return true;
        }

        bool IConfigurationAttribute.IsValidTarget(string s)
        {
            return (s == "Component");
        }

        public SynchronizationOption Value
        {
            get
            {
                return this._value;
            }
        }
    }
}

