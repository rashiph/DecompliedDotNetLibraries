namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Assembly, Inherited=true), ComVisible(false)]
    public sealed class ApplicationIDAttribute : Attribute, IConfigurationAttribute
    {
        private Guid _value;

        public ApplicationIDAttribute(string guid)
        {
            this._value = new Guid(guid);
        }

        bool IConfigurationAttribute.AfterSaveChanges(Hashtable info)
        {
            return false;
        }

        bool IConfigurationAttribute.Apply(Hashtable info)
        {
            return false;
        }

        bool IConfigurationAttribute.IsValidTarget(string s)
        {
            return (s == "Application");
        }

        public Guid Value
        {
            get
            {
                return this._value;
            }
        }
    }
}

