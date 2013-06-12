namespace System.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic.Utils;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class DynamicObject : IDynamicMetaObjectProvider
    {
        protected DynamicObject()
        {
        }

        public virtual IEnumerable<string> GetDynamicMemberNames()
        {
            return new string[0];
        }

        public virtual DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new MetaDynamic(parameter, this);
        }

        public virtual bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
        {
            result = null;
            return false;
        }

        public virtual bool TryConvert(ConvertBinder binder, out object result)
        {
            result = null;
            return false;
        }

        public virtual bool TryCreateInstance(CreateInstanceBinder binder, object[] args, out object result)
        {
            result = null;
            return false;
        }

        public virtual bool TryDeleteIndex(DeleteIndexBinder binder, object[] indexes)
        {
            return false;
        }

        public virtual bool TryDeleteMember(DeleteMemberBinder binder)
        {
            return false;
        }

        public virtual bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            result = null;
            return false;
        }

        public virtual bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            return false;
        }

        public virtual bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            result = null;
            return false;
        }

        public virtual bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = null;
            return false;
        }

        public virtual bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            return false;
        }

        public virtual bool TrySetMember(SetMemberBinder binder, object value)
        {
            return false;
        }

        public virtual bool TryUnaryOperation(UnaryOperationBinder binder, out object result)
        {
            result = null;
            return false;
        }

        private sealed class MetaDynamic : DynamicMetaObject
        {
            private static readonly Expression[] NoArgs = new Expression[0];

            internal MetaDynamic(Expression expression, DynamicObject value) : base(expression, BindingRestrictions.Empty, value)
            {
            }

            public override DynamicMetaObject BindBinaryOperation(BinaryOperationBinder binder, DynamicMetaObject arg)
            {
                Fallback fallback = null;
                if (!this.IsOverridden("TryBinaryOperation"))
                {
                    return base.BindBinaryOperation(binder, arg);
                }
                if (fallback == null)
                {
                    fallback = e => binder.FallbackBinaryOperation(this, arg, e);
                }
                return this.CallMethodWithResult("TryBinaryOperation", binder, GetArgs(new DynamicMetaObject[] { arg }), fallback);
            }

            public override DynamicMetaObject BindConvert(ConvertBinder binder)
            {
                Fallback fallback = null;
                if (!this.IsOverridden("TryConvert"))
                {
                    return base.BindConvert(binder);
                }
                if (fallback == null)
                {
                    fallback = e => binder.FallbackConvert(this, e);
                }
                return this.CallMethodWithResult("TryConvert", binder, NoArgs, fallback);
            }

            public override DynamicMetaObject BindCreateInstance(CreateInstanceBinder binder, DynamicMetaObject[] args)
            {
                Fallback fallback = null;
                if (!this.IsOverridden("TryCreateInstance"))
                {
                    return base.BindCreateInstance(binder, args);
                }
                if (fallback == null)
                {
                    fallback = e => binder.FallbackCreateInstance(this, args, e);
                }
                return this.CallMethodWithResult("TryCreateInstance", binder, GetArgArray(args), fallback);
            }

            public override DynamicMetaObject BindDeleteIndex(DeleteIndexBinder binder, DynamicMetaObject[] indexes)
            {
                Fallback fallback = null;
                if (!this.IsOverridden("TryDeleteIndex"))
                {
                    return base.BindDeleteIndex(binder, indexes);
                }
                if (fallback == null)
                {
                    fallback = e => binder.FallbackDeleteIndex(this, indexes, e);
                }
                return this.CallMethodNoResult("TryDeleteIndex", binder, GetArgArray(indexes), fallback);
            }

            public override DynamicMetaObject BindDeleteMember(DeleteMemberBinder binder)
            {
                Fallback fallback = null;
                if (!this.IsOverridden("TryDeleteMember"))
                {
                    return base.BindDeleteMember(binder);
                }
                if (fallback == null)
                {
                    fallback = e => binder.FallbackDeleteMember(this, e);
                }
                return this.CallMethodNoResult("TryDeleteMember", binder, NoArgs, fallback);
            }

            public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
            {
                Fallback fallback = null;
                if (!this.IsOverridden("TryGetIndex"))
                {
                    return base.BindGetIndex(binder, indexes);
                }
                if (fallback == null)
                {
                    fallback = e => binder.FallbackGetIndex(this, indexes, e);
                }
                return this.CallMethodWithResult("TryGetIndex", binder, GetArgArray(indexes), fallback);
            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
            {
                Fallback fallback = null;
                if (!this.IsOverridden("TryGetMember"))
                {
                    return base.BindGetMember(binder);
                }
                if (fallback == null)
                {
                    fallback = e => binder.FallbackGetMember(this, e);
                }
                return this.CallMethodWithResult("TryGetMember", binder, NoArgs, fallback);
            }

            public override DynamicMetaObject BindInvoke(InvokeBinder binder, DynamicMetaObject[] args)
            {
                Fallback fallback = null;
                if (!this.IsOverridden("TryInvoke"))
                {
                    return base.BindInvoke(binder, args);
                }
                if (fallback == null)
                {
                    fallback = e => binder.FallbackInvoke(this, args, e);
                }
                return this.CallMethodWithResult("TryInvoke", binder, GetArgArray(args), fallback);
            }

            public override DynamicMetaObject BindInvokeMember(InvokeMemberBinder binder, DynamicMetaObject[] args)
            {
                Fallback fallback = e => binder.FallbackInvokeMember(this, args, e);
                DynamicMetaObject errorSuggestion = this.BuildCallMethodWithResult("TryInvokeMember", binder, GetArgArray(args), this.BuildCallMethodWithResult("TryGetMember", new GetBinderAdapter(binder), NoArgs, fallback(null), e => binder.FallbackInvoke(e, args, null)), null);
                return fallback(errorSuggestion);
            }

            public override DynamicMetaObject BindSetIndex(SetIndexBinder binder, DynamicMetaObject[] indexes, DynamicMetaObject value)
            {
                Fallback fallback = null;
                if (!this.IsOverridden("TrySetIndex"))
                {
                    return base.BindSetIndex(binder, indexes, value);
                }
                if (fallback == null)
                {
                    fallback = e => binder.FallbackSetIndex(this, indexes, value, e);
                }
                return this.CallMethodReturnLast("TrySetIndex", binder, GetArgArray(indexes, value), fallback);
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
            {
                Fallback fallback = null;
                if (!this.IsOverridden("TrySetMember"))
                {
                    return base.BindSetMember(binder, value);
                }
                if (fallback == null)
                {
                    fallback = e => binder.FallbackSetMember(this, value, e);
                }
                return this.CallMethodReturnLast("TrySetMember", binder, GetArgs(new DynamicMetaObject[] { value }), fallback);
            }

            public override DynamicMetaObject BindUnaryOperation(UnaryOperationBinder binder)
            {
                Fallback fallback = null;
                if (!this.IsOverridden("TryUnaryOperation"))
                {
                    return base.BindUnaryOperation(binder);
                }
                if (fallback == null)
                {
                    fallback = e => binder.FallbackUnaryOperation(this, e);
                }
                return this.CallMethodWithResult("TryUnaryOperation", binder, NoArgs, fallback);
            }

            private DynamicMetaObject BuildCallMethodWithResult(string methodName, DynamicMetaObjectBinder binder, Expression[] args, DynamicMetaObject fallbackResult, Fallback fallbackInvoke)
            {
                if (!this.IsOverridden(methodName))
                {
                    return fallbackResult;
                }
                ParameterExpression expression = Expression.Parameter(typeof(object), null);
                Expression[] destinationArray = new Expression[args.Length + 2];
                Array.Copy(args, 0, destinationArray, 1, args.Length);
                destinationArray[0] = Constant(binder);
                destinationArray[destinationArray.Length - 1] = expression;
                DynamicMetaObject errorSuggestion = new DynamicMetaObject(expression, BindingRestrictions.Empty);
                if (binder.ReturnType != typeof(object))
                {
                    errorSuggestion = new DynamicMetaObject(Expression.Convert(errorSuggestion.Expression, binder.ReturnType), errorSuggestion.Restrictions);
                }
                if (fallbackInvoke != null)
                {
                    errorSuggestion = fallbackInvoke(errorSuggestion);
                }
                return new DynamicMetaObject(Expression.Block(new ParameterExpression[] { expression }, new Expression[] { Expression.Condition(Expression.Call(this.GetLimitedSelf(), typeof(DynamicObject).GetMethod(methodName), destinationArray), errorSuggestion.Expression, fallbackResult.Expression, binder.ReturnType) }), this.GetRestrictions().Merge(errorSuggestion.Restrictions).Merge(fallbackResult.Restrictions));
            }

            private DynamicMetaObject CallMethodNoResult(string methodName, DynamicMetaObjectBinder binder, Expression[] args, Fallback fallback)
            {
                DynamicMetaObject obj2 = fallback(null);
                DynamicMetaObject errorSuggestion = new DynamicMetaObject(Expression.Condition(Expression.Call(this.GetLimitedSelf(), typeof(DynamicObject).GetMethod(methodName), args.AddFirst<Expression>(Constant(binder))), Expression.Empty(), obj2.Expression, typeof(void)), this.GetRestrictions().Merge(obj2.Restrictions));
                return fallback(errorSuggestion);
            }

            private DynamicMetaObject CallMethodReturnLast(string methodName, DynamicMetaObjectBinder binder, Expression[] args, Fallback fallback)
            {
                DynamicMetaObject obj2 = fallback(null);
                ParameterExpression left = Expression.Parameter(typeof(object), null);
                Expression[] arguments = args.AddFirst<Expression>(Constant(binder));
                arguments[args.Length] = Expression.Assign(left, arguments[args.Length]);
                DynamicMetaObject errorSuggestion = new DynamicMetaObject(Expression.Block(new ParameterExpression[] { left }, new Expression[] { Expression.Condition(Expression.Call(this.GetLimitedSelf(), typeof(DynamicObject).GetMethod(methodName), arguments), left, obj2.Expression, typeof(object)) }), this.GetRestrictions().Merge(obj2.Restrictions));
                return fallback(errorSuggestion);
            }

            private DynamicMetaObject CallMethodWithResult(string methodName, DynamicMetaObjectBinder binder, Expression[] args, Fallback fallback)
            {
                return this.CallMethodWithResult(methodName, binder, args, fallback, null);
            }

            private DynamicMetaObject CallMethodWithResult(string methodName, DynamicMetaObjectBinder binder, Expression[] args, Fallback fallback, Fallback fallbackInvoke)
            {
                DynamicMetaObject fallbackResult = fallback(null);
                DynamicMetaObject errorSuggestion = this.BuildCallMethodWithResult(methodName, binder, args, fallbackResult, fallbackInvoke);
                return fallback(errorSuggestion);
            }

            private static ConstantExpression Constant(DynamicMetaObjectBinder binder)
            {
                Type baseType = binder.GetType();
                while (!baseType.IsVisible)
                {
                    baseType = baseType.BaseType;
                }
                return Expression.Constant(binder, baseType);
            }

            private static Expression[] GetArgArray(DynamicMetaObject[] args)
            {
                return new NewArrayExpression[] { Expression.NewArrayInit(typeof(object), GetArgs(args)) };
            }

            private static Expression[] GetArgArray(DynamicMetaObject[] args, DynamicMetaObject value)
            {
                return new Expression[] { Expression.NewArrayInit(typeof(object), GetArgs(args)), Expression.Convert(value.Expression, typeof(object)) };
            }

            private static Expression[] GetArgs(params DynamicMetaObject[] args)
            {
                Expression[] expressions = DynamicMetaObject.GetExpressions(args);
                for (int i = 0; i < expressions.Length; i++)
                {
                    expressions[i] = Expression.Convert(args[i].Expression, typeof(object));
                }
                return expressions;
            }

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return this.Value.GetDynamicMemberNames();
            }

            private Expression GetLimitedSelf()
            {
                if (TypeUtils.AreEquivalent(base.Expression.Type, typeof(DynamicObject)))
                {
                    return base.Expression;
                }
                return Expression.Convert(base.Expression, typeof(DynamicObject));
            }

            private BindingRestrictions GetRestrictions()
            {
                return BindingRestrictions.GetTypeRestriction(this);
            }

            private bool IsOverridden(string method)
            {
                foreach (MethodInfo info in this.Value.GetType().GetMember(method, MemberTypes.Method, BindingFlags.Public | BindingFlags.Instance))
                {
                    if ((info.DeclaringType != typeof(DynamicObject)) && (info.GetBaseDefinition().DeclaringType == typeof(DynamicObject)))
                    {
                        return true;
                    }
                }
                return false;
            }

            private DynamicObject Value
            {
                get
                {
                    return (DynamicObject) base.Value;
                }
            }

            private delegate DynamicMetaObject Fallback(DynamicMetaObject errorSuggestion);

            private sealed class GetBinderAdapter : GetMemberBinder
            {
                internal GetBinderAdapter(InvokeMemberBinder binder) : base(binder.Name, binder.IgnoreCase)
                {
                }

                public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}

