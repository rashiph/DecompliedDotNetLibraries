namespace System.Dynamic
{
    using System;
    using System.Dynamic.Utils;
    using System.Linq.Expressions;

    public abstract class UnaryOperationBinder : DynamicMetaObjectBinder
    {
        private ExpressionType _operation;

        protected UnaryOperationBinder(ExpressionType operation)
        {
            ContractUtils.Requires(OperationIsValid(operation), "operation");
            this._operation = operation;
        }

        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.Requires((args == null) || (args.Length == 0), "args");
            return target.BindUnaryOperation(this);
        }

        public DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target)
        {
            return this.FallbackUnaryOperation(target, null);
        }

        public abstract DynamicMetaObject FallbackUnaryOperation(DynamicMetaObject target, DynamicMetaObject errorSuggestion);
        internal static bool OperationIsValid(ExpressionType operation)
        {
            switch (operation)
            {
                case ExpressionType.Negate:
                case ExpressionType.UnaryPlus:
                case ExpressionType.Not:
                case ExpressionType.Extension:
                case ExpressionType.Increment:
                case ExpressionType.Decrement:
                case ExpressionType.OnesComplement:
                case ExpressionType.IsTrue:
                case ExpressionType.IsFalse:
                    return true;
            }
            return false;
        }

        internal sealed override bool IsStandardBinder
        {
            get
            {
                return true;
            }
        }

        public ExpressionType Operation
        {
            get
            {
                return this._operation;
            }
        }

        public sealed override Type ReturnType
        {
            get
            {
                switch (this._operation)
                {
                    case ExpressionType.IsTrue:
                    case ExpressionType.IsFalse:
                        return typeof(bool);
                }
                return typeof(object);
            }
        }
    }
}

