namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.EnterpriseServices.Admin;
    using System.Runtime.InteropServices;

    [ComVisible(false), AttributeUsage(AttributeTargets.Class, Inherited=true)]
    public sealed class JustInTimeActivationAttribute : Attribute, IConfigurationAttribute
    {
        private bool _enabled;

        public JustInTimeActivationAttribute() : this(true)
        {
        }

        public JustInTimeActivationAttribute(bool val)
        {
            this._enabled = val;
        }

        bool IConfigurationAttribute.AfterSaveChanges(Hashtable info)
        {
            return false;
        }

        bool IConfigurationAttribute.Apply(Hashtable info)
        {
            ICatalogObject obj2 = (ICatalogObject) info["Component"];
            obj2.SetValue("JustInTimeActivation", this._enabled);
            if (this._enabled && (((int) obj2.GetValue("Synchronization")) == 0))
            {
                obj2.SetValue("Synchronization", SynchronizationOption.Required);
            }
            return true;
        }

        bool IConfigurationAttribute.IsValidTarget(string s)
        {
            return (s == "Component");
        }

        public bool Value
        {
            get
            {
                return this._enabled;
            }
        }
    }
}

