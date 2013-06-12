namespace System.Security.AccessControl
{
    using System;
    using System.Collections;
    using System.Reflection;

    public sealed class AuthorizationRuleCollection : ReadOnlyCollectionBase
    {
        internal AuthorizationRuleCollection()
        {
        }

        internal void AddRule(AuthorizationRule rule)
        {
            base.InnerList.Add(rule);
        }

        public void CopyTo(AuthorizationRule[] rules, int index)
        {
            ((ICollection) this).CopyTo(rules, index);
        }

        public AuthorizationRule this[int index]
        {
            get
            {
                return (base.InnerList[index] as AuthorizationRule);
            }
        }
    }
}

