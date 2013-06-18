namespace System.EnterpriseServices
{
    using System;
    using System.Collections;
    using System.EnterpriseServices.Admin;
    using System.Runtime.InteropServices;

    [ComVisible(false), AttributeUsage(AttributeTargets.Class, Inherited=true)]
    public sealed class TransactionAttribute : Attribute, IConfigurationAttribute
    {
        private TransactionIsolationLevel _isolation;
        private int _timeout;
        private TransactionOption _value;

        public TransactionAttribute() : this(TransactionOption.Required)
        {
        }

        public TransactionAttribute(TransactionOption val)
        {
            this._value = val;
            this._isolation = TransactionIsolationLevel.Serializable;
            this._timeout = -1;
        }

        bool IConfigurationAttribute.AfterSaveChanges(Hashtable info)
        {
            return false;
        }

        bool IConfigurationAttribute.Apply(Hashtable info)
        {
            object obj2 = this._value;
            ICatalogObject obj3 = (ICatalogObject) info["Component"];
            obj3.SetValue("Transaction", obj2);
            if (this._isolation != TransactionIsolationLevel.Serializable)
            {
                obj3.SetValue("TxIsolationLevel", this._isolation);
            }
            if (this._timeout != -1)
            {
                obj3.SetValue("ComponentTransactionTimeout", this._timeout);
                obj3.SetValue("ComponentTransactionTimeoutEnabled", true);
            }
            return true;
        }

        bool IConfigurationAttribute.IsValidTarget(string s)
        {
            return (s == "Component");
        }

        public TransactionIsolationLevel Isolation
        {
            get
            {
                return this._isolation;
            }
            set
            {
                this._isolation = value;
            }
        }

        public int Timeout
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

        public TransactionOption Value
        {
            get
            {
                return this._value;
            }
        }
    }
}

