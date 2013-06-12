namespace System.Dynamic
{
    using System;
    using System.Dynamic.Utils;

    public abstract class InvokeMemberBinder : DynamicMetaObjectBinder
    {
        private readonly System.Dynamic.CallInfo _callInfo;
        private readonly bool _ignoreCase;
        private readonly string _name;

        protected InvokeMemberBinder(string name, bool ignoreCase, System.Dynamic.CallInfo callInfo)
        {
            ContractUtils.RequiresNotNull(name, "name");
            ContractUtils.RequiresNotNull(callInfo, "callInfo");
            this._name = name;
            this._ignoreCase = ignoreCase;
            this._callInfo = callInfo;
        }

        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNullItems<DynamicMetaObject>(args, "args");
            return target.BindInvokeMember(this, args);
        }

        public abstract DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion);
        public DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            return this.FallbackInvokeMember(target, args, null);
        }

        public abstract DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion);

        public System.Dynamic.CallInfo CallInfo
        {
            get
            {
                return this._callInfo;
            }
        }

        public bool IgnoreCase
        {
            get
            {
                return this._ignoreCase;
            }
        }

        internal sealed override bool IsStandardBinder
        {
            get
            {
                return true;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public sealed override Type ReturnType
        {
            get
            {
                return typeof(object);
            }
        }
    }
}

