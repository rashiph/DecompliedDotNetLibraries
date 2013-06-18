namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;

    internal class IDOUtils
    {
        private static CacheSet<CallSiteBinder> binderCache = new CacheSet<CallSiteBinder>(0x40);

        private IDOUtils()
        {
            throw new InternalErrorException();
        }

        public static Expression ConvertToObject(Expression valueExpression)
        {
            if (!valueExpression.Type.Equals(typeof(object)))
            {
                return (Expression) Expression.Convert(valueExpression, typeof(object));
            }
            return valueExpression;
        }

        public static void CopyBackArguments(CallInfo callInfo, object[] packedArgs, object[] args)
        {
            if (packedArgs != args)
            {
                int length = packedArgs.Length;
                int argumentCount = callInfo.ArgumentCount;
                int num3 = length - callInfo.ArgumentNames.Count;
                int num5 = length - 1;
                for (int i = 0; i <= num5; i++)
                {
                    args[i] = packedArgs[(i < argumentCount) ? ((i + num3) % argumentCount) : i];
                }
            }
        }

        public static object CreateConvertCallSiteAndInvoke(ConvertBinder Action, object Instance)
        {
            object obj2;
            CallSite site = CallSite.Create(Expression.GetFuncType(new Type[] { typeof(CallSite), typeof(object), Action.Type }), GetCachedBinder(Action));
            object[] args = new object[] { site, Instance };
            Delegate delegate2 = (Delegate) site.GetType().GetField("Target").GetValue(site);
            try
            {
                obj2 = delegate2.DynamicInvoke(args);
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException;
            }
            return obj2;
        }

        public static object CreateFuncCallSiteAndInvoke(CallSiteBinder Action, object Instance, object[] Arguments)
        {
            object obj2;
            Action = GetCachedBinder(Action);
            switch (Arguments.Length)
            {
                case 0:
                {
                    CallSite<Func<CallSite, object, object>> site = CallSite<Func<CallSite, object, object>>.Create(Action);
                    return site.Target(site, Instance);
                }
                case 1:
                {
                    CallSite<Func<CallSite, object, object, object>> site2 = CallSite<Func<CallSite, object, object, object>>.Create(Action);
                    return site2.Target(site2, Instance, Arguments[0]);
                }
                case 2:
                {
                    CallSite<Func<CallSite, object, object, object, object>> site3 = CallSite<Func<CallSite, object, object, object, object>>.Create(Action);
                    return site3.Target(site3, Instance, Arguments[0], Arguments[1]);
                }
                case 3:
                {
                    CallSite<Func<CallSite, object, object, object, object, object>> site4 = CallSite<Func<CallSite, object, object, object, object, object>>.Create(Action);
                    return site4.Target(site4, Instance, Arguments[0], Arguments[1], Arguments[2]);
                }
                case 4:
                {
                    CallSite<Func<CallSite, object, object, object, object, object, object>> site5 = CallSite<Func<CallSite, object, object, object, object, object, object>>.Create(Action);
                    return site5.Target(site5, Instance, Arguments[0], Arguments[1], Arguments[2], Arguments[3]);
                }
                case 5:
                {
                    CallSite<Func<CallSite, object, object, object, object, object, object, object>> site6 = CallSite<Func<CallSite, object, object, object, object, object, object, object>>.Create(Action);
                    return site6.Target(site6, Instance, Arguments[0], Arguments[1], Arguments[2], Arguments[3], Arguments[4]);
                }
                case 6:
                {
                    CallSite<Func<CallSite, object, object, object, object, object, object, object, object>> site7 = CallSite<Func<CallSite, object, object, object, object, object, object, object, object>>.Create(Action);
                    return site7.Target(site7, Instance, Arguments[0], Arguments[1], Arguments[2], Arguments[3], Arguments[4], Arguments[5]);
                }
                case 7:
                {
                    CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object>> site8 = CallSite<Func<CallSite, object, object, object, object, object, object, object, object, object>>.Create(Action);
                    return site8.Target(site8, Instance, Arguments[0], Arguments[1], Arguments[2], Arguments[3], Arguments[4], Arguments[5], Arguments[6]);
                }
            }
            Type[] typeArgs = new Type[(Arguments.Length + 2) + 1];
            typeArgs[0] = typeof(CallSite);
            int num3 = typeArgs.Length - 1;
            for (int i = 1; i <= num3; i++)
            {
                typeArgs[i] = typeof(object);
            }
            CallSite site9 = CallSite.Create(Expression.GetDelegateType(typeArgs), Action);
            object[] array = new object[(Arguments.Length + 1) + 1];
            array[0] = site9;
            array[1] = Instance;
            Arguments.CopyTo(array, 2);
            Delegate delegate2 = (Delegate) site9.GetType().GetField("Target").GetValue(site9);
            try
            {
                obj2 = delegate2.DynamicInvoke(array);
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException;
            }
            return obj2;
        }

        public static object CreateRefCallSiteAndInvoke(CallSiteBinder Action, object Instance, object[] Arguments)
        {
            object obj2;
            Action = GetCachedBinder(Action);
            switch (Arguments.Length)
            {
                case 0:
                {
                    CallSite<SiteDelegate0> site = CallSite<SiteDelegate0>.Create(Action);
                    return site.Target(site, Instance);
                }
                case 1:
                {
                    CallSite<SiteDelegate1> site2 = CallSite<SiteDelegate1>.Create(Action);
                    return site2.Target(site2, Instance, ref Arguments[0]);
                }
                case 2:
                {
                    CallSite<SiteDelegate2> site3 = CallSite<SiteDelegate2>.Create(Action);
                    return site3.Target(site3, Instance, ref Arguments[0], ref Arguments[1]);
                }
                case 3:
                {
                    CallSite<SiteDelegate3> site4 = CallSite<SiteDelegate3>.Create(Action);
                    return site4.Target(site4, Instance, ref Arguments[0], ref Arguments[1], ref Arguments[2]);
                }
                case 4:
                {
                    CallSite<SiteDelegate4> site5 = CallSite<SiteDelegate4>.Create(Action);
                    return site5.Target(site5, Instance, ref Arguments[0], ref Arguments[1], ref Arguments[2], ref Arguments[3]);
                }
                case 5:
                {
                    CallSite<SiteDelegate5> site6 = CallSite<SiteDelegate5>.Create(Action);
                    return site6.Target(site6, Instance, ref Arguments[0], ref Arguments[1], ref Arguments[2], ref Arguments[3], ref Arguments[4]);
                }
                case 6:
                {
                    CallSite<SiteDelegate6> site7 = CallSite<SiteDelegate6>.Create(Action);
                    return site7.Target(site7, Instance, ref Arguments[0], ref Arguments[1], ref Arguments[2], ref Arguments[3], ref Arguments[4], ref Arguments[5]);
                }
                case 7:
                {
                    CallSite<SiteDelegate7> site8 = CallSite<SiteDelegate7>.Create(Action);
                    return site8.Target(site8, Instance, ref Arguments[0], ref Arguments[1], ref Arguments[2], ref Arguments[3], ref Arguments[4], ref Arguments[5], ref Arguments[6]);
                }
            }
            Type[] typeArgs = new Type[(Arguments.Length + 2) + 1];
            Type type = typeof(object).MakeByRefType();
            typeArgs[0] = typeof(CallSite);
            typeArgs[1] = typeof(object);
            typeArgs[typeArgs.Length - 1] = typeof(object);
            int num3 = typeArgs.Length - 2;
            for (int i = 2; i <= num3; i++)
            {
                typeArgs[i] = type;
            }
            CallSite site9 = CallSite.Create(Expression.GetDelegateType(typeArgs), Action);
            object[] array = new object[(Arguments.Length + 1) + 1];
            array[0] = site9;
            array[1] = Instance;
            Arguments.CopyTo(array, 2);
            Delegate delegate2 = (Delegate) site9.GetType().GetField("Target").GetValue(site9);
            try
            {
                object obj3 = delegate2.DynamicInvoke(array);
                Array.Copy(array, 2, Arguments, 0, Arguments.Length);
                obj2 = obj3;
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException;
            }
            return obj2;
        }

        private static BindingRestrictions CreateRestriction(DynamicMetaObject metaObject)
        {
            if (metaObject.Value == null)
            {
                return metaObject.Restrictions.Merge(BindingRestrictions.GetInstanceRestriction(metaObject.Expression, null));
            }
            return metaObject.Restrictions.Merge(BindingRestrictions.GetTypeRestriction(metaObject.Expression, metaObject.LimitType));
        }

        internal static BindingRestrictions CreateRestrictions(DynamicMetaObject target, DynamicMetaObject[] args = null, DynamicMetaObject value = null)
        {
            BindingRestrictions restrictions2 = CreateRestriction(target);
            if (args != null)
            {
                foreach (DynamicMetaObject obj2 in args)
                {
                    restrictions2 = restrictions2.Merge(CreateRestriction(obj2));
                }
            }
            if (value != null)
            {
                restrictions2 = restrictions2.Merge(CreateRestriction(value));
            }
            return restrictions2;
        }

        private static CallSiteBinder GetCachedBinder(CallSiteBinder Action)
        {
            return binderCache.GetExistingOrAdd(Action);
        }

        public static Expression GetWriteBack(Expression[] arguments, ParameterExpression array)
        {
            List<Expression> expressions = new List<Expression>();
            int num2 = arguments.Length - 1;
            for (int i = 0; i <= num2; i++)
            {
                ParameterExpression left = arguments[i] as ParameterExpression;
                if ((left != null) && left.IsByRef)
                {
                    expressions.Add(Expression.Assign(left, Expression.ArrayIndex(array, Expression.Constant(i))));
                }
            }
            switch (expressions.Count)
            {
                case 0:
                    return Expression.Empty();

                case 1:
                    return expressions[0];
            }
            return Expression.Block(expressions);
        }

        internal static ExpressionType? LinqOperator(Symbols.UserDefinedOperator vbOperator)
        {
            ExpressionType? nullable2;
            switch (vbOperator)
            {
                case Symbols.UserDefinedOperator.Negate:
                    return 0x1c;

                case Symbols.UserDefinedOperator.Not:
                    return 0x22;

                case Symbols.UserDefinedOperator.UnaryPlus:
                    return 0x1d;

                case Symbols.UserDefinedOperator.Plus:
                    return 0;

                case Symbols.UserDefinedOperator.Minus:
                    return 0x2a;

                case Symbols.UserDefinedOperator.Multiply:
                    return 0x1a;

                case Symbols.UserDefinedOperator.Divide:
                    return 12;

                case Symbols.UserDefinedOperator.Power:
                    return 0x27;

                case Symbols.UserDefinedOperator.IntegralDivide:
                case Symbols.UserDefinedOperator.Concatenate:
                case Symbols.UserDefinedOperator.Like:
                    return nullable2;

                case Symbols.UserDefinedOperator.ShiftLeft:
                    return 0x13;

                case Symbols.UserDefinedOperator.ShiftRight:
                    return 0x29;

                case Symbols.UserDefinedOperator.Modulus:
                    return 0x19;

                case Symbols.UserDefinedOperator.Or:
                    return 0x24;

                case Symbols.UserDefinedOperator.Xor:
                    return 14;

                case Symbols.UserDefinedOperator.And:
                    return 2;

                case Symbols.UserDefinedOperator.Equal:
                    return 13;

                case Symbols.UserDefinedOperator.NotEqual:
                    return 0x23;

                case Symbols.UserDefinedOperator.Less:
                    return 20;

                case Symbols.UserDefinedOperator.LessEqual:
                    return 0x15;

                case Symbols.UserDefinedOperator.GreaterEqual:
                    return 0x10;

                case Symbols.UserDefinedOperator.Greater:
                    return 15;
            }
            return nullable2;
        }

        internal static bool NeedsDeferral(DynamicMetaObject target, DynamicMetaObject[] args = null, DynamicMetaObject value = null)
        {
            if (!target.HasValue)
            {
                return true;
            }
            if ((value != null) && !value.HasValue)
            {
                return true;
            }
            if (args != null)
            {
                foreach (DynamicMetaObject obj2 in args)
                {
                    if (!obj2.HasValue)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void PackArguments(int valueArgs, string[] argNames, object[] args, ref object[] packedArgs, ref CallInfo callInfo)
        {
            if (argNames == null)
            {
                argNames = new string[0];
            }
            callInfo = new CallInfo(args.Length - valueArgs, argNames);
            if (argNames.Length > 0)
            {
                packedArgs = new object[(args.Length - 1) + 1];
                int num = args.Length - valueArgs;
                int num4 = num - 1;
                for (int i = 0; i <= num4; i++)
                {
                    packedArgs[i] = args[(i + argNames.Length) % num];
                }
                int num5 = args.Length - 1;
                for (int j = num; j <= num5; j++)
                {
                    packedArgs[j] = args[j];
                }
            }
            else
            {
                packedArgs = args;
            }
        }

        internal static IDynamicMetaObjectProvider TryCastToIDMOP(object o)
        {
            IDynamicMetaObjectProvider provider = o as IDynamicMetaObjectProvider;
            if ((provider != null) && !RemotingServices.IsObjectOutOfAppDomain(o))
            {
                return provider;
            }
            return null;
        }

        public static void UnpackArguments(DynamicMetaObject[] packedArgs, CallInfo callInfo, ref Expression[] args, ref string[] argNames, ref object[] argValues)
        {
            int length = packedArgs.Length;
            int argumentCount = callInfo.ArgumentCount;
            args = new Expression[(length - 1) + 1];
            argValues = new object[(length - 1) + 1];
            int count = callInfo.ArgumentNames.Count;
            int num4 = length - count;
            int num7 = argumentCount - 1;
            for (int i = 0; i <= num7; i++)
            {
                DynamicMetaObject obj2 = packedArgs[(i + num4) % argumentCount];
                args[i] = obj2.Expression;
                argValues[i] = obj2.Value;
            }
            int num8 = length - 1;
            for (int j = argumentCount; j <= num8; j++)
            {
                DynamicMetaObject obj3 = packedArgs[j];
                args[j] = obj3.Expression;
                argValues[j] = obj3.Value;
            }
            argNames = new string[(count - 1) + 1];
            callInfo.ArgumentNames.CopyTo(argNames, 0);
        }

        internal delegate object SiteDelegate0(CallSite Site, object Instance);

        internal delegate object SiteDelegate1(CallSite Site, object Instance, ref object Arg0);

        internal delegate object SiteDelegate2(CallSite Site, object Instance, ref object Arg0, ref object Arg1);

        internal delegate object SiteDelegate3(CallSite Site, object Instance, ref object Arg0, ref object Arg1, ref object Arg2);

        internal delegate object SiteDelegate4(CallSite Site, object Instance, ref object Arg0, ref object Arg1, ref object Arg2, ref object Arg3);

        internal delegate object SiteDelegate5(CallSite Site, object Instance, ref object Arg0, ref object Arg1, ref object Arg2, ref object Arg3, ref object Arg4);

        internal delegate object SiteDelegate6(CallSite Site, object Instance, ref object Arg0, ref object Arg1, ref object Arg2, ref object Arg3, ref object Arg4, ref object Arg5);

        internal delegate object SiteDelegate7(CallSite Site, object Instance, ref object Arg0, ref object Arg1, ref object Arg2, ref object Arg3, ref object Arg4, ref object Arg5, ref object Arg6);
    }
}

