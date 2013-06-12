namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Security.Principal;

    public sealed class AuthorizationSection : ConfigurationSection
    {
        private bool _EveryoneAllowed;
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propRules = new ConfigurationProperty(null, typeof(AuthorizationRuleCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

        static AuthorizationSection()
        {
            _properties.Add(_propRules);
        }

        internal bool IsUserAllowed(IPrincipal user, string verb)
        {
            return this.Rules.IsUserAllowed(user, verb);
        }

        protected override void PostDeserialize()
        {
            if (this.Rules.Count > 0)
            {
                this._EveryoneAllowed = (this.Rules[0].Action == AuthorizationRuleAction.Allow) && this.Rules[0].Everyone;
            }
        }

        internal bool EveryoneAllowed
        {
            get
            {
                return this._EveryoneAllowed;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection=true)]
        public AuthorizationRuleCollection Rules
        {
            get
            {
                return (AuthorizationRuleCollection) base[_propRules];
            }
        }
    }
}

