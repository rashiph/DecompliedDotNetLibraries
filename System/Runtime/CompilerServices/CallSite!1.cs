namespace System.Runtime.CompilerServices
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Dynamic;
    using System.Dynamic.Utils;
    using System.Linq.Expressions;
    using System.Linq.Expressions.Compiler;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public sealed class CallSite<T> : CallSite where T: class
    {
        private static T _CachedNoMatch;
        private static T _CachedUpdate;
        private const int MaxRules = 10;
        internal T[] Rules;
        public T Target;

        private CallSite() : base(null)
        {
        }

        private CallSite(CallSiteBinder binder) : base(binder)
        {
            this.Target = this.GetUpdateDelegate();
        }

        internal void AddRule(T newRule)
        {
            T[] rules = this.Rules;
            if (rules == null)
            {
                this.Rules = new T[] { newRule };
            }
            else
            {
                T[] localArray2;
                if (rules.Length < 9)
                {
                    localArray2 = new T[rules.Length + 1];
                    Array.Copy(rules, 0, localArray2, 1, rules.Length);
                }
                else
                {
                    localArray2 = new T[10];
                    Array.Copy(rules, 0, localArray2, 1, 9);
                }
                localArray2[0] = newRule;
                this.Rules = localArray2;
            }
        }

        private void ClearRuleCache()
        {
            base.Binder.GetRuleCache<T>();
            Dictionary<Type, object> cache = base.Binder.Cache;
            if (cache != null)
            {
                lock (cache)
                {
                    cache.Clear();
                }
            }
        }

        private static Expression Convert(Expression arg, Type type)
        {
            if (TypeUtils.AreReferenceAssignable(type, arg.Type))
            {
                return arg;
            }
            return Expression.Convert(arg, type);
        }

        public static CallSite<T> Create(CallSiteBinder binder)
        {
            return new CallSite<T>(binder);
        }

        private T CreateCustomNoMatchDelegate(MethodInfo invoke)
        {
            ParameterExpression[] parameters = invoke.GetParametersCached().Map<ParameterInfo, ParameterExpression>(p => Expression.Parameter(p.ParameterType, p.Name));
            ParameterExpression expression1 = parameters[0];
            return Expression.Lambda<T>(Expression.Block(Expression.Call(typeof(CallSiteOps).GetMethod("SetNotMatched"), parameters.First<ParameterExpression>()), Expression.Default(invoke.GetReturnType())), parameters).Compile();
        }

        private T CreateCustomUpdateDelegate(MethodInfo invoke)
        {
            Expression expression9;
            List<Expression> list = new List<Expression>();
            List<ParameterExpression> list2 = new List<ParameterExpression>();
            ParameterExpression[] array = invoke.GetParametersCached().Map<ParameterInfo, ParameterExpression>(p => Expression.Parameter(p.ParameterType, p.Name));
            LabelTarget target = Expression.Label(invoke.GetReturnType());
            Type[] typeArguments = new Type[] { typeof(T) };
            ParameterExpression expression = array[0];
            ParameterExpression[] collection = array.RemoveFirst<ParameterExpression>();
            ParameterExpression item = Expression.Variable(typeof(CallSite<T>), "this");
            list2.Add(item);
            list.Add(Expression.Assign(item, Expression.Convert(expression, item.Type)));
            ParameterExpression expression3 = Expression.Variable(typeof(T[]), "applicable");
            list2.Add(expression3);
            ParameterExpression expression4 = Expression.Variable(typeof(T), "rule");
            list2.Add(expression4);
            ParameterExpression expression5 = Expression.Variable(typeof(T), "originalRule");
            list2.Add(expression5);
            list.Add(Expression.Assign(expression5, Expression.Field(item, "Target")));
            ParameterExpression left = null;
            if (target.Type != typeof(void))
            {
                list2.Add(left = Expression.Variable(target.Type, "result"));
            }
            ParameterExpression expression7 = Expression.Variable(typeof(int), "count");
            list2.Add(expression7);
            ParameterExpression expression8 = Expression.Variable(typeof(int), "index");
            list2.Add(expression8);
            list.Add(Expression.Assign(expression, Expression.Call(typeof(CallSiteOps), "CreateMatchmaker", typeArguments, new Expression[] { item })));
            Expression test = Expression.Call(typeof(CallSiteOps).GetMethod("GetMatch"), expression);
            Expression expression11 = Expression.Call(typeof(CallSiteOps).GetMethod("ClearMatch"), expression);
            MethodCallExpression expression12 = Expression.Call(typeof(CallSiteOps), "UpdateRules", typeArguments, new Expression[] { item, expression8 });
            if (target.Type == typeof(void))
            {
                expression9 = Expression.Block(Expression.Invoke(expression4, new TrueReadOnlyCollection<Expression>(array)), Expression.IfThen(test, Expression.Block(expression12, Expression.Return(target))));
            }
            else
            {
                expression9 = Expression.Block(Expression.Assign(left, Expression.Invoke(expression4, new TrueReadOnlyCollection<Expression>(array))), Expression.IfThen(test, Expression.Block(expression12, Expression.Return(target, left))));
            }
            Expression expression13 = Expression.Assign(expression4, Expression.ArrayAccess(expression3, new Expression[] { expression8 }));
            LabelTarget target2 = Expression.Label();
            ConditionalExpression expression14 = Expression.IfThen(Expression.Equal(expression8, expression7), Expression.Break(target2));
            UnaryExpression expression15 = Expression.PreIncrementAssign(expression8);
            list.Add(Expression.IfThen(Expression.NotEqual(Expression.Assign(expression3, Expression.Call(typeof(CallSiteOps), "GetRules", typeArguments, new Expression[] { item })), Expression.Constant(null, expression3.Type)), Expression.Block(Expression.Assign(expression7, Expression.ArrayLength(expression3)), Expression.Assign(expression8, Expression.Constant(0)), Expression.Loop(Expression.Block(expression14, expression13, Expression.IfThen(Expression.NotEqual(Expression.Convert(expression4, typeof(object)), Expression.Convert(expression5, typeof(object))), Expression.Block(Expression.Assign(Expression.Field(item, "Target"), expression4), expression9, expression11)), expression15), target2, null))));
            ParameterExpression expression16 = Expression.Variable(typeof(RuleCache<T>), "cache");
            list2.Add(expression16);
            list.Add(Expression.Assign(expression16, Expression.Call(typeof(CallSiteOps), "GetRuleCache", typeArguments, new Expression[] { item })));
            list.Add(Expression.Assign(expression3, Expression.Call(typeof(CallSiteOps), "GetCachedRules", typeArguments, new Expression[] { expression16 })));
            if (target.Type == typeof(void))
            {
                expression9 = Expression.Block(Expression.Invoke(expression4, new TrueReadOnlyCollection<Expression>(array)), Expression.IfThen(test, Expression.Return(target)));
            }
            else
            {
                expression9 = Expression.Block(Expression.Assign(left, Expression.Invoke(expression4, new TrueReadOnlyCollection<Expression>(array))), Expression.IfThen(test, Expression.Return(target, left)));
            }
            TryExpression expression17 = Expression.TryFinally(expression9, Expression.IfThen(test, Expression.Block(Expression.Call(typeof(CallSiteOps), "AddRule", typeArguments, new Expression[] { item, expression4 }), Expression.Call(typeof(CallSiteOps), "MoveRule", typeArguments, new Expression[] { expression16, expression4, expression8 }))));
            expression13 = Expression.Assign(Expression.Field(item, "Target"), Expression.Assign(expression4, Expression.ArrayAccess(expression3, new Expression[] { expression8 })));
            list.Add(Expression.Assign(expression8, Expression.Constant(0)));
            list.Add(Expression.Assign(expression7, Expression.ArrayLength(expression3)));
            list.Add(Expression.Loop(Expression.Block(expression14, expression13, expression17, expression11, expression15), target2, null));
            list.Add(Expression.Assign(expression4, Expression.Constant(null, expression4.Type)));
            ParameterExpression expression18 = Expression.Variable(typeof(object[]), "args");
            list2.Add(expression18);
            list.Add(Expression.Assign(expression18, Expression.NewArrayInit(typeof(object), collection.Map<ParameterExpression, Expression>(p => CallSite<T>.Convert(p, typeof(object))))));
            Expression expression19 = Expression.Assign(Expression.Field(item, "Target"), expression5);
            expression13 = Expression.Assign(Expression.Field(item, "Target"), Expression.Assign(expression4, Expression.Call(typeof(CallSiteOps), "Bind", typeArguments, new Expression[] { Expression.Property(item, "Binder"), item, expression18 })));
            expression17 = Expression.TryFinally(expression9, Expression.IfThen(test, Expression.Call(typeof(CallSiteOps), "AddRule", typeArguments, new Expression[] { item, expression4 })));
            list.Add(Expression.Loop(Expression.Block(expression19, expression13, expression17, expression11), null, null));
            list.Add(Expression.Default(target.Type));
            return Expression.Lambda<T>(Expression.Label(target, Expression.Block((IEnumerable<ParameterExpression>) new ReadOnlyCollection<ParameterExpression>(list2), (IEnumerable<Expression>) new ReadOnlyCollection<Expression>(list))), "CallSite.Target", true, new ReadOnlyCollection<ParameterExpression>(array)).Compile();
        }

        internal CallSite<T> CreateMatchMaker()
        {
            return new CallSite<T>();
        }

        private T GetUpdateDelegate()
        {
            return this.GetUpdateDelegate(ref CallSite<T>._CachedUpdate);
        }

        private T GetUpdateDelegate(ref T addr)
        {
            if (((T) addr) == null)
            {
                addr = this.MakeUpdateDelegate();
            }
            return addr;
        }

        private static bool IsSimpleSignature(MethodInfo invoke, out Type[] sig)
        {
            ParameterInfo[] parametersCached = invoke.GetParametersCached();
            ContractUtils.Requires((parametersCached.Length > 0) && (parametersCached[0].ParameterType == typeof(CallSite)), "T");
            Type[] typeArray = new Type[(invoke.ReturnType != typeof(void)) ? parametersCached.Length : (parametersCached.Length - 1)];
            bool flag = true;
            for (int i = 1; i < parametersCached.Length; i++)
            {
                ParameterInfo pi = parametersCached[i];
                if (pi.IsByRefParameter())
                {
                    flag = false;
                }
                typeArray[i - 1] = pi.ParameterType;
            }
            if (invoke.ReturnType != typeof(void))
            {
                typeArray[typeArray.Length - 1] = invoke.ReturnType;
            }
            sig = typeArray;
            return flag;
        }

        internal T MakeUpdateDelegate()
        {
            Type[] typeArray;
            Type delegateType = typeof(T);
            MethodInfo method = delegateType.GetMethod("Invoke");
            if (delegateType.IsGenericType && CallSite<T>.IsSimpleSignature(method, out typeArray))
            {
                MethodInfo info2 = null;
                MethodInfo info3 = null;
                if (method.ReturnType == typeof(void))
                {
                    if (delegateType == DelegateHelpers.GetActionType(typeArray.AddFirst<Type>(typeof(CallSite))))
                    {
                        info2 = typeof(UpdateDelegates).GetMethod("UpdateAndExecuteVoid" + typeArray.Length, BindingFlags.NonPublic | BindingFlags.Static);
                        info3 = typeof(UpdateDelegates).GetMethod("NoMatchVoid" + typeArray.Length, BindingFlags.NonPublic | BindingFlags.Static);
                    }
                }
                else if (delegateType == DelegateHelpers.GetFuncType(typeArray.AddFirst<Type>(typeof(CallSite))))
                {
                    info2 = typeof(UpdateDelegates).GetMethod("UpdateAndExecute" + (typeArray.Length - 1), BindingFlags.NonPublic | BindingFlags.Static);
                    info3 = typeof(UpdateDelegates).GetMethod("NoMatch" + (typeArray.Length - 1), BindingFlags.NonPublic | BindingFlags.Static);
                }
                if (info2 != null)
                {
                    CallSite<T>._CachedNoMatch = (T) info3.MakeGenericMethod(typeArray).CreateDelegate(delegateType);
                    return (T) info2.MakeGenericMethod(typeArray).CreateDelegate(delegateType);
                }
            }
            CallSite<T>._CachedNoMatch = this.CreateCustomNoMatchDelegate(method);
            return this.CreateCustomUpdateDelegate(method);
        }

        internal void MoveRule(int i)
        {
            T[] rules = this.Rules;
            T local = rules[i];
            rules[i] = rules[i - 1];
            rules[i - 1] = rules[i - 2];
            rules[i - 2] = local;
        }

        public T Update
        {
            get
            {
                if (base._match)
                {
                    return CallSite<T>._CachedNoMatch;
                }
                return CallSite<T>._CachedUpdate;
            }
        }
    }
}

