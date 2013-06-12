namespace System.Dynamic
{
    using System;
    using System.Dynamic.Utils;

    public abstract class ConvertBinder : DynamicMetaObjectBinder
    {
        private readonly bool _explicit;
        private readonly System.Type _type;

        protected ConvertBinder(System.Type type, bool @explicit)
        {
            ContractUtils.RequiresNotNull(type, "type");
            this._type = type;
            this._explicit = @explicit;
        }

        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.Requires((args == null) || (args.Length == 0), "args");
            return target.BindConvert(this);
        }

        public DynamicMetaObject FallbackConvert(DynamicMetaObject target)
        {
            return this.FallbackConvert(target, null);
        }

        public abstract DynamicMetaObject FallbackConvert(DynamicMetaObject target, DynamicMetaObject errorSuggestion);

        public bool Explicit
        {
            get
            {
                return this._explicit;
            }
        }

        internal sealed override bool IsStandardBinder
        {
            get
            {
                return true;
            }
        }

        public sealed override System.Type ReturnType
        {
            get
            {
                return this._type;
            }
        }

        public System.Type Type
        {
            get
            {
                return this._type;
            }
        }
    }
}

