namespace System.Dynamic
{
    using System;
    using System.Dynamic.Utils;

    public abstract class DeleteMemberBinder : DynamicMetaObjectBinder
    {
        private readonly bool _ignoreCase;
        private readonly string _name;

        protected DeleteMemberBinder(string name, bool ignoreCase)
        {
            ContractUtils.RequiresNotNull(name, "name");
            this._name = name;
            this._ignoreCase = ignoreCase;
        }

        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.Requires((args == null) || (args.Length == 0));
            return target.BindDeleteMember(this);
        }

        public DynamicMetaObject FallbackDeleteMember(DynamicMetaObject target)
        {
            return this.FallbackDeleteMember(target, null);
        }

        public abstract DynamicMetaObject FallbackDeleteMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion);

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
                return typeof(void);
            }
        }
    }
}

