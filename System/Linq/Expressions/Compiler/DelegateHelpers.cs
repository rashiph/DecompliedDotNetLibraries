namespace System.Linq.Expressions.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Dynamic;
    using System.Dynamic.Utils;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;

    internal static class DelegateHelpers
    {
        private static TypeInfo _DelegateCache = new TypeInfo();
        private static readonly Type[] _DelegateCtorSignature = new Type[] { typeof(object), typeof(IntPtr) };
        private const MethodAttributes CtorAttributes = (MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public);
        private const MethodImplAttributes ImplAttributes = MethodImplAttributes.CodeTypeMask;
        private const MethodAttributes InvokeAttributes = (MethodAttributes.NewSlot | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public);
        private const int MaximumArity = 0x11;

        internal static Type GetActionType(Type[] types)
        {
            switch (types.Length)
            {
                case 0:
                    return typeof(Action);

                case 1:
                    return typeof(Action<>).MakeGenericType(types);

                case 2:
                    return typeof(Action<,>).MakeGenericType(types);

                case 3:
                    return typeof(Action<,,>).MakeGenericType(types);

                case 4:
                    return typeof(Action<,,,>).MakeGenericType(types);

                case 5:
                    return typeof(Action<,,,,>).MakeGenericType(types);

                case 6:
                    return typeof(Action<,,,,,>).MakeGenericType(types);

                case 7:
                    return typeof(Action<,,,,,,>).MakeGenericType(types);

                case 8:
                    return typeof(Action<,,,,,,,>).MakeGenericType(types);

                case 9:
                    return typeof(Action<,,,,,,,,>).MakeGenericType(types);

                case 10:
                    return typeof(Action<,,,,,,,,,>).MakeGenericType(types);

                case 11:
                    return typeof(Action<,,,,,,,,,,>).MakeGenericType(types);

                case 12:
                    return typeof(Action<,,,,,,,,,,,>).MakeGenericType(types);

                case 13:
                    return typeof(Action<,,,,,,,,,,,,>).MakeGenericType(types);

                case 14:
                    return typeof(Action<,,,,,,,,,,,,,>).MakeGenericType(types);

                case 15:
                    return typeof(Action<,,,,,,,,,,,,,,>).MakeGenericType(types);

                case 0x10:
                    return typeof(Action<,,,,,,,,,,,,,,,>).MakeGenericType(types);
            }
            return null;
        }

        internal static Type GetFuncType(Type[] types)
        {
            switch (types.Length)
            {
                case 1:
                    return typeof(Func<>).MakeGenericType(types);

                case 2:
                    return typeof(Func<,>).MakeGenericType(types);

                case 3:
                    return typeof(Func<,,>).MakeGenericType(types);

                case 4:
                    return typeof(Func<,,,>).MakeGenericType(types);

                case 5:
                    return typeof(Func<,,,,>).MakeGenericType(types);

                case 6:
                    return typeof(Func<,,,,,>).MakeGenericType(types);

                case 7:
                    return typeof(Func<,,,,,,>).MakeGenericType(types);

                case 8:
                    return typeof(Func<,,,,,,,>).MakeGenericType(types);

                case 9:
                    return typeof(Func<,,,,,,,,>).MakeGenericType(types);

                case 10:
                    return typeof(Func<,,,,,,,,,>).MakeGenericType(types);

                case 11:
                    return typeof(Func<,,,,,,,,,,>).MakeGenericType(types);

                case 12:
                    return typeof(Func<,,,,,,,,,,,>).MakeGenericType(types);

                case 13:
                    return typeof(Func<,,,,,,,,,,,,>).MakeGenericType(types);

                case 14:
                    return typeof(Func<,,,,,,,,,,,,,>).MakeGenericType(types);

                case 15:
                    return typeof(Func<,,,,,,,,,,,,,,>).MakeGenericType(types);

                case 0x10:
                    return typeof(Func<,,,,,,,,,,,,,,,>).MakeGenericType(types);

                case 0x11:
                    return typeof(Func<,,,,,,,,,,,,,,,,>).MakeGenericType(types);
            }
            return null;
        }

        internal static TypeInfo GetNextTypeInfo(Type initialArg, TypeInfo curTypeInfo)
        {
            lock (_DelegateCache)
            {
                return NextTypeInfo(initialArg, curTypeInfo);
            }
        }

        private static bool IsByRef(DynamicMetaObject mo)
        {
            ParameterExpression expression = mo.Expression as ParameterExpression;
            return ((expression != null) && expression.IsByRef);
        }

        internal static Type MakeCallSiteDelegate(ReadOnlyCollection<Expression> types, Type returnType)
        {
            lock (_DelegateCache)
            {
                TypeInfo curTypeInfo = _DelegateCache;
                curTypeInfo = NextTypeInfo(typeof(CallSite), curTypeInfo);
                for (int i = 0; i < types.Count; i++)
                {
                    curTypeInfo = NextTypeInfo(types[i].Type, curTypeInfo);
                }
                curTypeInfo = NextTypeInfo(returnType, curTypeInfo);
                if (curTypeInfo.DelegateType == null)
                {
                    curTypeInfo.MakeDelegateType(returnType, types);
                }
                return curTypeInfo.DelegateType;
            }
        }

        internal static Type MakeDeferredSiteDelegate(DynamicMetaObject[] args, Type returnType)
        {
            lock (_DelegateCache)
            {
                TypeInfo curTypeInfo = _DelegateCache;
                curTypeInfo = NextTypeInfo(typeof(CallSite), curTypeInfo);
                for (int i = 0; i < args.Length; i++)
                {
                    DynamicMetaObject mo = args[i];
                    Type initialArg = mo.Expression.Type;
                    if (IsByRef(mo))
                    {
                        initialArg = initialArg.MakeByRefType();
                    }
                    curTypeInfo = NextTypeInfo(initialArg, curTypeInfo);
                }
                curTypeInfo = NextTypeInfo(returnType, curTypeInfo);
                if (curTypeInfo.DelegateType == null)
                {
                    Type[] types = new Type[args.Length + 2];
                    types[0] = typeof(CallSite);
                    types[types.Length - 1] = returnType;
                    for (int j = 0; j < args.Length; j++)
                    {
                        DynamicMetaObject obj3 = args[j];
                        Type type = obj3.Expression.Type;
                        if (IsByRef(obj3))
                        {
                            type = type.MakeByRefType();
                        }
                        types[j + 1] = type;
                    }
                    curTypeInfo.DelegateType = MakeNewDelegate(types);
                }
                return curTypeInfo.DelegateType;
            }
        }

        internal static Type MakeDelegateType(Type[] types)
        {
            lock (_DelegateCache)
            {
                TypeInfo curTypeInfo = _DelegateCache;
                for (int i = 0; i < types.Length; i++)
                {
                    curTypeInfo = NextTypeInfo(types[i], curTypeInfo);
                }
                if (curTypeInfo.DelegateType == null)
                {
                    curTypeInfo.DelegateType = MakeNewDelegate((Type[]) types.Clone());
                }
                return curTypeInfo.DelegateType;
            }
        }

        private static Type MakeNewCustomDelegate(Type[] types)
        {
            Type returnType = types[types.Length - 1];
            Type[] parameterTypes = types.RemoveLast<Type>();
            TypeBuilder builder = AssemblyGen.DefineDelegateType("Delegate" + types.Length);
            builder.DefineConstructor(MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public, CallingConventions.Standard, _DelegateCtorSignature).SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
            builder.DefineMethod("Invoke", MethodAttributes.NewSlot | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public, returnType, parameterTypes).SetImplementationFlags(MethodImplAttributes.CodeTypeMask);
            return builder.CreateType();
        }

        private static Type MakeNewDelegate(Type[] types)
        {
            if ((types.Length <= 0x11) && !types.Any<Type>(t => t.IsByRef))
            {
                if (types[types.Length - 1] == typeof(void))
                {
                    return GetActionType(types.RemoveLast<Type>());
                }
                return GetFuncType(types);
            }
            return MakeNewCustomDelegate(types);
        }

        internal static TypeInfo NextTypeInfo(Type initialArg)
        {
            lock (_DelegateCache)
            {
                return NextTypeInfo(initialArg, _DelegateCache);
            }
        }

        private static TypeInfo NextTypeInfo(Type initialArg, TypeInfo curTypeInfo)
        {
            TypeInfo info;
            Type key = initialArg;
            if (curTypeInfo.TypeChain == null)
            {
                curTypeInfo.TypeChain = new Dictionary<Type, TypeInfo>();
            }
            if (!curTypeInfo.TypeChain.TryGetValue(key, out info))
            {
                info = new TypeInfo();
                if (key.CanCache())
                {
                    curTypeInfo.TypeChain[key] = info;
                }
            }
            return info;
        }

        internal class TypeInfo
        {
            public Type DelegateType;
            public Dictionary<Type, DelegateHelpers.TypeInfo> TypeChain;

            public Type MakeDelegateType(Type retType, params Expression[] args)
            {
                return this.MakeDelegateType(retType, (IList<Expression>) args);
            }

            public Type MakeDelegateType(Type retType, IList<Expression> args)
            {
                Type[] types = new Type[args.Count + 2];
                types[0] = typeof(CallSite);
                types[types.Length - 1] = retType;
                for (int i = 0; i < args.Count; i++)
                {
                    types[i + 1] = args[i].Type;
                }
                return (this.DelegateType = DelegateHelpers.MakeNewDelegate(types));
            }
        }
    }
}

