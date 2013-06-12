namespace System.Dynamic
{
    using System;
    using System.Collections.ObjectModel;
    using System.Dynamic.Utils;
    using System.Linq.Expressions;
    using System.Linq.Expressions.Compiler;
    using System.Runtime.CompilerServices;
    using System.Runtime.Remoting;

    public abstract class DynamicMetaObjectBinder : CallSiteBinder
    {
        private static readonly Type ComObjectType = typeof(object).Assembly.GetType("System.__ComObject");

        protected DynamicMetaObjectBinder()
        {
        }

        private static BindingRestrictions AddRemoteObjectRestrictions(BindingRestrictions restrictions, object[] args, ReadOnlyCollection<ParameterExpression> parameters)
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                ParameterExpression left = parameters[i];
                MarshalByRefObject obj2 = args[i] as MarshalByRefObject;
                if ((obj2 != null) && !IsComObject(obj2))
                {
                    BindingRestrictions expressionRestriction;
                    if (RemotingServices.IsObjectOutOfAppDomain(obj2))
                    {
                        expressionRestriction = BindingRestrictions.GetExpressionRestriction(Expression.AndAlso(Expression.NotEqual(left, Expression.Constant(null)), Expression.Call(typeof(RemotingServices).GetMethod("IsObjectOutOfAppDomain"), left)));
                    }
                    else
                    {
                        expressionRestriction = BindingRestrictions.GetExpressionRestriction(Expression.AndAlso(Expression.NotEqual(left, Expression.Constant(null)), Expression.Not(Expression.Call(typeof(RemotingServices).GetMethod("IsObjectOutOfAppDomain"), left))));
                    }
                    restrictions = restrictions.Merge(expressionRestriction);
                }
            }
            return restrictions;
        }

        public abstract DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args);
        public sealed override Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel)
        {
            Type returnType;
            ContractUtils.RequiresNotNull(args, "args");
            ContractUtils.RequiresNotNull(parameters, "parameters");
            ContractUtils.RequiresNotNull(returnLabel, "returnLabel");
            if (args.Length == 0)
            {
                throw Error.OutOfRange("args.Length", 1);
            }
            if (parameters.Count == 0)
            {
                throw Error.OutOfRange("parameters.Count", 1);
            }
            if (args.Length != parameters.Count)
            {
                throw new ArgumentOutOfRangeException("args");
            }
            if (this.IsStandardBinder)
            {
                returnType = this.ReturnType;
                if ((returnLabel.Type != typeof(void)) && !TypeUtils.AreReferenceAssignable(returnLabel.Type, returnType))
                {
                    throw Error.BinderNotCompatibleWithCallSite(returnType, this, returnLabel.Type);
                }
            }
            else
            {
                returnType = returnLabel.Type;
            }
            DynamicMetaObject target = DynamicMetaObject.Create(args[0], parameters[0]);
            DynamicMetaObject[] objArray = CreateArgumentMetaObjects(args, parameters);
            DynamicMetaObject obj3 = this.Bind(target, objArray);
            if (obj3 == null)
            {
                throw Error.BindingCannotBeNull();
            }
            Expression expression = obj3.Expression;
            BindingRestrictions restrictions = obj3.Restrictions;
            if ((returnType != typeof(void)) && !TypeUtils.AreReferenceAssignable(returnType, expression.Type))
            {
                if (target.Value is IDynamicMetaObjectProvider)
                {
                    throw Error.DynamicObjectResultNotAssignable(expression.Type, target.Value.GetType(), this, returnType);
                }
                throw Error.DynamicBinderResultNotAssignable(expression.Type, this, returnType);
            }
            if ((this.IsStandardBinder && (args[0] is IDynamicMetaObjectProvider)) && (restrictions == BindingRestrictions.Empty))
            {
                throw Error.DynamicBindingNeedsRestrictions(target.Value.GetType(), this);
            }
            restrictions = AddRemoteObjectRestrictions(restrictions, args, parameters);
            if (expression.NodeType != ExpressionType.Goto)
            {
                expression = Expression.Return(returnLabel, expression);
            }
            if (restrictions != BindingRestrictions.Empty)
            {
                expression = Expression.IfThen(restrictions.ToExpression(), expression);
            }
            return expression;
        }

        private static DynamicMetaObject[] CreateArgumentMetaObjects(object[] args, ReadOnlyCollection<ParameterExpression> parameters)
        {
            if (args.Length != 1)
            {
                DynamicMetaObject[] objArray = new DynamicMetaObject[args.Length - 1];
                for (int i = 1; i < args.Length; i++)
                {
                    objArray[i - 1] = DynamicMetaObject.Create(args[i], parameters[i]);
                }
                return objArray;
            }
            return DynamicMetaObject.EmptyMetaObjects;
        }

        public DynamicMetaObject Defer(params DynamicMetaObject[] args)
        {
            return this.MakeDeferred(BindingRestrictions.Combine(args), args);
        }

        public DynamicMetaObject Defer(DynamicMetaObject target, params DynamicMetaObject[] args)
        {
            ContractUtils.RequiresNotNull(target, "target");
            if (args == null)
            {
                return this.MakeDeferred(target.Restrictions, new DynamicMetaObject[] { target });
            }
            return this.MakeDeferred(target.Restrictions.Merge(BindingRestrictions.Combine(args)), args.AddFirst<DynamicMetaObject>(target));
        }

        public Expression GetUpdateExpression(Type type)
        {
            return Expression.Goto(CallSiteBinder.UpdateLabel, type);
        }

        private static bool IsComObject(object obj)
        {
            return ((obj != null) && ComObjectType.IsAssignableFrom(obj.GetType()));
        }

        private DynamicMetaObject MakeDeferred(BindingRestrictions rs, params DynamicMetaObject[] args)
        {
            Expression[] expressions = DynamicMetaObject.GetExpressions(args);
            Type delegateType = DelegateHelpers.MakeDeferredSiteDelegate(args, this.ReturnType);
            return new DynamicMetaObject(DynamicExpression.Make(this.ReturnType, delegateType, this, new TrueReadOnlyCollection<Expression>(expressions)), rs);
        }

        internal virtual bool IsStandardBinder
        {
            get
            {
                return false;
            }
        }

        public virtual Type ReturnType
        {
            get
            {
                return typeof(object);
            }
        }
    }
}

