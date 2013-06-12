namespace System.Runtime.CompilerServices
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Dynamic.Utils;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;

    public abstract class CallSiteBinder
    {
        private static readonly LabelTarget _updateLabel = Expression.Label("CallSiteBinder.UpdateLabel");
        internal Dictionary<Type, object> Cache;

        protected CallSiteBinder()
        {
        }

        public abstract Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel);
        internal T BindCore<T>(CallSite<T> site, object[] args) where T: class
        {
            T local = this.BindDelegate<T>(site, args);
            if (local != null)
            {
                return local;
            }
            LambdaSignature<T> instance = LambdaSignature<T>.Instance;
            Expression binding = this.Bind(args, instance.Parameters, instance.ReturnLabel);
            if (binding == null)
            {
                throw Error.NoOrInvalidRuleProduced();
            }
            if (!AppDomain.CurrentDomain.IsHomogenous)
            {
                throw Error.HomogenousAppDomainRequired();
            }
            T target = Stitch<T>(binding, instance).Compile();
            this.CacheTarget<T>(target);
            return target;
        }

        public virtual T BindDelegate<T>(CallSite<T> site, object[] args) where T: class
        {
            return default(T);
        }

        protected void CacheTarget<T>(T target) where T: class
        {
            this.GetRuleCache<T>().AddRule(target);
        }

        internal RuleCache<T> GetRuleCache<T>() where T: class
        {
            object obj2;
            if (this.Cache == null)
            {
                Interlocked.CompareExchange<Dictionary<Type, object>>(ref this.Cache, new Dictionary<Type, object>(), null);
            }
            Dictionary<Type, object> cache = this.Cache;
            lock (cache)
            {
                if (!cache.TryGetValue(typeof(T), out obj2))
                {
                    cache[typeof(T)] = obj2 = new RuleCache<T>();
                }
            }
            return (obj2 as RuleCache<T>);
        }

        private static Expression<T> Stitch<T>(Expression binding, LambdaSignature<T> signature) where T: class
        {
            ParameterExpression expression;
            Type type = typeof(CallSite<T>);
            ReadOnlyCollectionBuilder<Expression> expressions = new ReadOnlyCollectionBuilder<Expression>(3) {
                binding
            };
            ParameterExpression[] source = signature.Parameters.AddFirst<ParameterExpression>(expression = Expression.Parameter(typeof(CallSite), "$site"));
            Expression item = Expression.Label(UpdateLabel);
            expressions.Add(item);
            expressions.Add(Expression.Label(signature.ReturnLabel, Expression.Condition(Expression.Call(typeof(CallSiteOps).GetMethod("SetNotMatched"), source.First<ParameterExpression>()), Expression.Default(signature.ReturnLabel.Type), Expression.Invoke(Expression.Property(Expression.Convert(expression, type), typeof(CallSite<T>).GetProperty("Update")), new TrueReadOnlyCollection<Expression>(source)))));
            return new Expression<T>(Expression.Block(expressions), "CallSite.Target", true, new TrueReadOnlyCollection<ParameterExpression>(source));
        }

        public static LabelTarget UpdateLabel
        {
            get
            {
                return _updateLabel;
            }
        }

        private sealed class LambdaSignature<T> where T: class
        {
            internal static readonly CallSiteBinder.LambdaSignature<T> Instance;
            internal readonly ReadOnlyCollection<ParameterExpression> Parameters;
            internal readonly LabelTarget ReturnLabel;

            static LambdaSignature()
            {
                CallSiteBinder.LambdaSignature<T>.Instance = new CallSiteBinder.LambdaSignature<T>();
            }

            private LambdaSignature()
            {
                Type c = typeof(T);
                if (!typeof(Delegate).IsAssignableFrom(c))
                {
                    throw Error.TypeParameterIsNotDelegate(c);
                }
                MethodInfo method = c.GetMethod("Invoke");
                ParameterInfo[] parametersCached = method.GetParametersCached();
                if (parametersCached[0].ParameterType != typeof(CallSite))
                {
                    throw Error.FirstArgumentMustBeCallSite();
                }
                ParameterExpression[] list = new ParameterExpression[parametersCached.Length - 1];
                for (int i = 0; i < list.Length; i++)
                {
                    list[i] = Expression.Parameter(parametersCached[i + 1].ParameterType, "$arg" + i);
                }
                this.Parameters = new TrueReadOnlyCollection<ParameterExpression>(list);
                this.ReturnLabel = Expression.Label(method.GetReturnType());
            }
        }
    }
}

