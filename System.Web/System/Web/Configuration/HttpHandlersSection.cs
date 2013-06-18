namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Web;

    public sealed class HttpHandlersSection : ConfigurationSection
    {
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propHandlers = new ConfigurationProperty(null, typeof(HttpHandlerActionCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        private bool _validated;

        static HttpHandlersSection()
        {
            _properties.Add(_propHandlers);
        }

        internal HttpHandlerAction FindMapping(string verb, VirtualPath path)
        {
            this.ValidateHandlers();
            for (int i = 0; i < this.Handlers.Count; i++)
            {
                HttpHandlerAction action = this.Handlers[i];
                if (action.IsMatch(verb, path))
                {
                    return action;
                }
            }
            return null;
        }

        internal bool ValidateHandlers()
        {
            if (!this._validated)
            {
                lock (this)
                {
                    if (!this._validated)
                    {
                        foreach (HttpHandlerAction action in this.Handlers)
                        {
                            action.InitValidateInternal();
                        }
                        this._validated = true;
                    }
                }
            }
            return this._validated;
        }

        [ConfigurationProperty("", IsDefaultCollection=true)]
        public HttpHandlerActionCollection Handlers
        {
            get
            {
                return (HttpHandlerActionCollection) base[_propHandlers];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }
    }
}

