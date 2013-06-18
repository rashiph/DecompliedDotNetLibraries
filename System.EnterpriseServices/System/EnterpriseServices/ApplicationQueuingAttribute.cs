namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.EnterpriseServices.Admin;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Assembly, Inherited=true), ComVisible(false)]
    public sealed class ApplicationQueuingAttribute : Attribute, IConfigurationAttribute
    {
        private bool _enabled = true;
        private bool _listen = false;
        private int _maxthreads = 0;

        bool IConfigurationAttribute.AfterSaveChanges(Hashtable info)
        {
            return false;
        }

        bool IConfigurationAttribute.Apply(Hashtable info)
        {
            ICatalogObject obj2 = (ICatalogObject) info["Application"];
            obj2.SetValue("QueuingEnabled", this._enabled);
            obj2.SetValue("QueueListenerEnabled", this._listen);
            if (this._maxthreads != 0)
            {
                obj2.SetValue("QCListenerMaxThreads", this._maxthreads);
            }
            return true;
        }

        bool IConfigurationAttribute.IsValidTarget(string s)
        {
            return (s == "Application");
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

        public int MaxListenerThreads
        {
            get
            {
                return this._maxthreads;
            }
            set
            {
                this._maxthreads = value;
            }
        }

        public bool QueueListenerEnabled
        {
            get
            {
                return this._listen;
            }
            set
            {
                this._listen = value;
            }
        }
    }
}

