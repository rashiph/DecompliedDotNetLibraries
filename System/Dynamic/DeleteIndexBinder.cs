namespace System.Dynamic
{
    using System;
    using System.Dynamic.Utils;

    public abstract class DeleteIndexBinder : DynamicMetaObjectBinder
    {
        private readonly System.Dynamic.CallInfo _callInfo;

        protected DeleteIndexBinder(System.Dynamic.CallInfo callInfo)
        {
            ContractUtils.RequiresNotNull(callInfo, "callInfo");
            this._callInfo = callInfo;
        }

        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNullItems<DynamicMetaObject>(args, "args");
            return target.BindDeleteIndex(this, args);
        }

        public DynamicMetaObject FallbackDeleteIndex(DynamicMetaObject target, DynamicMetaObject[] indexes)
        {
            return this.FallbackDeleteIndex(target, indexes, null);
        }

        public abstract DynamicMetaObject FallbackDeleteIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject errorSuggestion);

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
                return typeof(void);
            }
        }
    }
}

