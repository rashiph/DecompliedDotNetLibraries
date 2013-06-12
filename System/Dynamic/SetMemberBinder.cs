namespace System.Dynamic
{
    using System;
    using System.Dynamic.Utils;

    public abstract class SetMemberBinder : DynamicMetaObjectBinder
    {
        private readonly bool _ignoreCase;
        private readonly string _name;

        protected SetMemberBinder(string name, bool ignoreCase)
        {
            ContractUtils.RequiresNotNull(name, "name");
            this._name = name;
            this._ignoreCase = ignoreCase;
        }

        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNull(args, "args");
            ContractUtils.Requires(args.Length == 1, "args");
            DynamicMetaObject obj2 = args[0];
            ContractUtils.RequiresNotNull(obj2, "args");
            return target.BindSetMember(this, obj2);
        }

        public DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value)
        {
            return this.FallbackSetMember(target, value, null);
        }

        public abstract DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion);

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

