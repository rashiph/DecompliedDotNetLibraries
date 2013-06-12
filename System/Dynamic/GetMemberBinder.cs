namespace System.Dynamic
{
    using System;
    using System.Dynamic.Utils;

    public abstract class GetMemberBinder : DynamicMetaObjectBinder
    {
        private readonly bool _ignoreCase;
        private readonly string _name;

        protected GetMemberBinder(string name, bool ignoreCase)
        {
            ContractUtils.RequiresNotNull(name, "name");
            this._name = name;
            this._ignoreCase = ignoreCase;
        }

        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.Requires((args == null) || (args.Length == 0), "args");
            return target.BindGetMember(this);
        }

        public DynamicMetaObject FallbackGetMember(DynamicMetaObject target)
        {
            return this.FallbackGetMember(target, null);
        }

        public abstract DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion);

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

