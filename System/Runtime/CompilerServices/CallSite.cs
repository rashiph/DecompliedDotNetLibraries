namespace System.Runtime.CompilerServices
{
    using System;
    using System.Dynamic.Utils;
    using System.Linq.Expressions;
    using System.Reflection;

    public class CallSite
    {
        internal readonly CallSiteBinder _binder;
        internal bool _match;
        private static CacheDict<Type, Func<CallSiteBinder, CallSite>> _SiteCtors;

        internal CallSite(CallSiteBinder binder)
        {
            this._binder = binder;
        }

        public static CallSite Create(Type delegateType, CallSiteBinder binder)
        {
            Func<CallSiteBinder, CallSite> func;
            ContractUtils.RequiresNotNull(delegateType, "delegateType");
            ContractUtils.RequiresNotNull(binder, "binder");
            if (!delegateType.IsSubclassOf(typeof(Delegate)))
            {
                throw Error.TypeMustBeDerivedFromSystemDelegate();
            }
            if (_SiteCtors == null)
            {
                _SiteCtors = new CacheDict<Type, Func<CallSiteBinder, CallSite>>(100);
            }
            MethodInfo method = null;
            CacheDict<Type, Func<CallSiteBinder, CallSite>> dict = _SiteCtors;
            lock (dict)
            {
                if (!dict.TryGetValue(delegateType, out func))
                {
                    method = typeof(CallSite<>).MakeGenericType(new Type[] { delegateType }).GetMethod("Create");
                    if (delegateType.CanCache())
                    {
                        func = (Func<CallSiteBinder, CallSite>) Delegate.CreateDelegate(typeof(Func<CallSiteBinder, CallSite>), method);
                        dict.Add(delegateType, func);
                    }
                }
            }
            if (func != null)
            {
                return func(binder);
            }
            return (CallSite) method.Invoke(null, new object[] { binder });
        }

        public CallSiteBinder Binder
        {
            get
            {
                return this._binder;
            }
        }
    }
}

