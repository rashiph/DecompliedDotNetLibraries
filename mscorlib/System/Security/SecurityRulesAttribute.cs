namespace System.Security
{
    using System;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=false)]
    public sealed class SecurityRulesAttribute : Attribute
    {
        private SecurityRuleSet m_ruleSet;
        private bool m_skipVerificationInFullTrust;

        public SecurityRulesAttribute(SecurityRuleSet ruleSet)
        {
            this.m_ruleSet = ruleSet;
        }

        public SecurityRuleSet RuleSet
        {
            get
            {
                return this.m_ruleSet;
            }
        }

        public bool SkipVerificationInFullTrust
        {
            get
            {
                return this.m_skipVerificationInFullTrust;
            }
            set
            {
                this.m_skipVerificationInFullTrust = value;
            }
        }
    }
}

