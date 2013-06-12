namespace System.Dynamic
{
    using System;
    using System.Dynamic.Utils;

    public abstract class InvokeBinder : DynamicMetaObjectBinder
    {
        private readonly System.Dynamic.CallInfo _callInfo;

        protected InvokeBinder(System.Dynamic.CallInfo callInfo)
        {
            ContractUtils.RequiresNotNull(callInfo, "callInfo");
            this._callInfo = callInfo;
        }

        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNullItems<DynamicMetaObject>(args, "args");
            return target.BindInvoke(this, args);
        }

        public DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            return this.FallbackInvoke(target, args, null);
        }

        public abstract DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion);

        public System.Dynamic.CallInfo CallInfo
        {
            get
            {
                return this._callInfo;
            }
        }

        internal sealed override bool IsStandardBinder
        {
            get
            {
                return true;
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

