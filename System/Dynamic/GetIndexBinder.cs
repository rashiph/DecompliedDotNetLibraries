namespace System.Dynamic
{
    using System;
    using System.Dynamic.Utils;

    public abstract class GetIndexBinder : DynamicMetaObjectBinder
    {
        private readonly System.Dynamic.CallInfo _callInfo;

        protected GetIndexBinder(System.Dynamic.CallInfo callInfo)
        {
            ContractUtils.RequiresNotNull(callInfo, "callInfo");
            this._callInfo = callInfo;
        }

        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNullItems<DynamicMetaObject>(args, "args");
            return target.BindGetIndex(this, args);
        }

        public DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes)
        {
            return this.FallbackGetIndex(target, indexes, null);
        }

        public abstract DynamicMetaObject FallbackGetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion);

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

