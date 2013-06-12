namespace System.Dynamic
{
    using System;
    using System.Dynamic.Utils;
    using System.Linq.Expressions;

    public abstract class BinaryOperationBinder : DynamicMetaObjectBinder
    {
        private ExpressionType _operation;

        protected BinaryOperationBinder(ExpressionType operation)
        {
            ContractUtils.Requires(OperationIsValid(operation), "operation");
            this._operation = operation;
        }

        public sealed override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            ContractUtils.RequiresNotNull(target, "target");
            ContractUtils.RequiresNotNull(args, "args");
            ContractUtils.Requires(args.Length == 1, "args");
            DynamicMetaObject obj2 = args[0];
            ContractUtils.RequiresNotNull(obj2, "args");
            return target.BindBinaryOperation(this, obj2);
        }

        public DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg)
        {
            return this.FallbackBinaryOperation(target, arg, null);
        }

        public abstract DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion);
        internal static bool OperationIsValid(ExpressionType operation)
        {
            switch (operation)
            {
                case ExpressionType.Add:
                case ExpressionType.And:
                case ExpressionType.Divide:
                case ExpressionType.Equal:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LeftShift:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.NotEqual:
                case ExpressionType.Or:
                case ExpressionType.Power:
                case ExpressionType.RightShift:
                case ExpressionType.Subtract:
                case ExpressionType.Extension:
                case ExpressionType.AddAssign:
                case ExpressionType.AndAssign:
                case ExpressionType.DivideAssign:
                case ExpressionType.ExclusiveOrAssign:
                case ExpressionType.LeftShiftAssign:
                case ExpressionType.ModuloAssign:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.OrAssign:
                case ExpressionType.PowerAssign:
                case ExpressionType.RightShiftAssign:
                case ExpressionType.SubtractAssign:
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
                return typeof(object);
            }
        }
    }
}

