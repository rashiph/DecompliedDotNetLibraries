namespace System.Configuration.Provider
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Runtime;

    public abstract class ProviderBase
    {
        private string _Description;
        private bool _Initialized;
        private string _name;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ProviderBase()
        {
        }

        public virtual void Initialize(string name, NameValueCollection config)
        {
            lock (this)
            {
                if (this._Initialized)
                {
                    throw new InvalidOperationException(System.Configuration.SR.GetString("Provider_Already_Initialized"));
                }
                this._Initialized = true;
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(System.Configuration.SR.GetString("Config_provider_name_null_or_empty"), "name");
            }
            this._name = name;
            if (config != null)
            {
                this._Description = config["description"];
                config.Remove("description");
            }
        }

        public virtual string Description
        {
            get
            {
                if (!string.IsNullOrEmpty(this._Description))
                {
                    return this._Description;
                }
                return this.Name;
            }
        }

        public virtual string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._name;
            }
        }
    }
}

