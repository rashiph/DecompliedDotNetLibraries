namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.EnterpriseServices.Admin;
    using System.Runtime.InteropServices;

    [ComVisible(false), AttributeUsage(AttributeTargets.Assembly, Inherited=true)]
    public sealed class ApplicationAccessControlAttribute : Attribute, IConfigurationAttribute
    {
        private AuthenticationOption _authLevel;
        private AccessChecksLevelOption _checkLevel;
        private ImpersonationLevelOption _impLevel;
        private bool _val;

        public ApplicationAccessControlAttribute() : this(true)
        {
        }

        public ApplicationAccessControlAttribute(bool val)
        {
            this._val = val;
            this._authLevel = ~AuthenticationOption.Default;
            this._impLevel = ~ImpersonationLevelOption.Default;
            if (this._val)
            {
                this._checkLevel = AccessChecksLevelOption.ApplicationComponent;
            }
            else
            {
                this._checkLevel = AccessChecksLevelOption.Application;
            }
        }

        bool IConfigurationAttribute.AfterSaveChanges(Hashtable info)
        {
            return false;
        }

        bool IConfigurationAttribute.Apply(Hashtable cache)
        {
            ICatalogObject obj2 = (ICatalogObject) cache["Application"];
            obj2.SetValue("ApplicationAccessChecksEnabled", this._val);
            obj2.SetValue("AccessChecksLevel", this._checkLevel);
            if (this._authLevel != ~AuthenticationOption.Default)
            {
                obj2.SetValue("Authentication", this._authLevel);
            }
            if (this._impLevel != ~ImpersonationLevelOption.Default)
            {
                obj2.SetValue("ImpersonationLevel", this._impLevel);
            }
            return true;
        }

        bool IConfigurationAttribute.IsValidTarget(string s)
        {
            return (s == "Application");
        }

        public AccessChecksLevelOption AccessChecksLevel
        {
            get
            {
                return this._checkLevel;
            }
            set
            {
                this._checkLevel = value;
            }
        }

        public AuthenticationOption Authentication
        {
            get
            {
                return this._authLevel;
            }
            set
            {
                this._authLevel = value;
            }
        }

        public ImpersonationLevelOption ImpersonationLevel
        {
            get
            {
                return this._impLevel;
            }
            set
            {
                this._impLevel = value;
            }
        }

        public bool Value
        {
            get
            {
                return this._val;
            }
            set
            {
                this._val = value;
            }
        }
    }
}

