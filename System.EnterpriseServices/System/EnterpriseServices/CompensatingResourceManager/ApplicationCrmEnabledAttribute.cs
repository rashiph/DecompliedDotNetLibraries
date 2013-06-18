namespace System.EnterpriseServices.CompensatingResourceManager
{
    using System;
    using System.Collections;
    using System.EnterpriseServices;
    using System.EnterpriseServices.Admin;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Assembly, Inherited=true), ComVisible(false), ProgId("System.EnterpriseServices.Crm.ApplicationCrmEnabledAttribute")]
    public sealed class ApplicationCrmEnabledAttribute : Attribute, IConfigurationAttribute
    {
        private bool _value;

        public ApplicationCrmEnabledAttribute() : this(true)
        {
        }

        public ApplicationCrmEnabledAttribute(bool val)
        {
            this._value = val;
        }

        bool IConfigurationAttribute.AfterSaveChanges(Hashtable info)
        {
            return false;
        }

        bool IConfigurationAttribute.Apply(Hashtable info)
        {
            ((ICatalogObject) info["Application"]).SetValue("CRMEnabled", this._value);
            return true;
        }

        bool IConfigurationAttribute.IsValidTarget(string s)
        {
            return (s == "Application");
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

