namespace System.Dynamic
{
    using System;
    using System.Dynamic.Utils;

    public abstract class SetIndexBinder : DynamicMetaObjectBinder
    {
        private readonly System.Dynamic.CallInfo _callInfo;

        protected SetIndexBinder(System.Dynamic.CallInfo callInfo)
        {
            ContractUtils.RequiresNotNull(callInfo, "callInfo");
            this._callInfo = callInfo;
        }

        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNull(args, "args");
            ContractUtils.Requires(args.Length >= 2, "args");
            DynamicMetaObject obj2 = args[args.Length - 1];
            DynamicMetaObject[] array = args.RemoveLast<DynamicMetaObject>();
            ContractUtils.RequiresNotNull(obj2, "args");
            ContractUtils.RequiresNotNullItems<DynamicMetaObject>(array, "args");
            return target.BindSetIndex(this, array, obj2);
        }

        public DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value)
        {
            return this.FallbackSetIndex(target, indexes, value, null);
        }

        public abstract DynamicMetaObject FallbackSetIndex(DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion);

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

