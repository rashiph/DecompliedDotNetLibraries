namespace System.Runtime.CompilerServices
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    [EditorBrowsable(EditorBrowsableState.Never), DebuggerStepThrough]
    public static class CallSiteOps
    {
        [Obsolete("do not use this method", true), EditorBrowsable(EditorBrowsableState.Never)]
        public static void AddRule<T>(CallSite<T> site, T rule) where T: class
        {
            site.AddRule(rule);
        }

        [Obsolete("do not use this method", true), EditorBrowsable(EditorBrowsableState.Never)]
        public static T Bind<T>(CallSiteBinder binder, CallSite<T> site, object[] args) where T: class
        {
            return binder.BindCore<T>(site, args);
        }

        [Obsolete("do not use this method", true), EditorBrowsable(EditorBrowsableState.Never)]
        public static void ClearMatch(CallSite site)
        {
            site._match = true;
        }

        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("do not use this method", true)]
        public static CallSite<T> CreateMatchmaker<T>(CallSite<T> site) where T: class
        {
            CallSite<T> site2 = site.CreateMatchMaker();
            ClearMatch(site2);
            return site2;
        }

        [Obsolete("do not use this method", true), EditorBrowsable(EditorBrowsableState.Never)]
        public static T[] GetCachedRules<T>(RuleCache<T> cache) where T: class
        {
            return cache.GetRules();
        }

        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("do not use this method", true)]
        public static bool GetMatch(CallSite site)
        {
            return site._match;
        }

        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("do not use this method", true)]
        public static RuleCache<T> GetRuleCache<T>(CallSite<T> site) where T: class
        {
            return site.Binder.GetRuleCache<T>();
        }

        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("do not use this method", true)]
        public static T[] GetRules<T>(CallSite<T> site) where T: class
        {
            return site.Rules;
        }

        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("do not use this method", true)]
        public static void MoveRule<T>(RuleCache<T> cache, T rule, int i) where T: class
        {
            if (i > 1)
            {
                cache.MoveRule(rule, i);
            }
        }

        [Obsolete("do not use this method", true), EditorBrowsable(EditorBrowsableState.Never)]
        public static bool SetNotMatched(CallSite site)
        {
            bool flag = site._match;
            site._match = false;
            return flag;
        }

        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("do not use this method", true)]
        public static void UpdateRules<T>(CallSite<T> @this, int matched) where T: class
        {
            if (matched > 1)
            {
                @this.MoveRule(matched);
            }
        }
    }
}

