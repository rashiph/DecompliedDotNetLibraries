namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.Dynamic;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class IDOBinder
    {
        internal static readonly object missingMemberSentinel = new object();

        private IDOBinder()
        {
            throw new InternalErrorException();
        }

        internal static bool[] GetCopyBack()
        {
            return SaveCopyBack.GetCopyBack();
        }

        internal static object IDOCall(IDynamicMetaObjectProvider Instance, string MemberName, object[] Arguments, string[] ArgumentNames, bool[] CopyBack, bool IgnoreReturn)
        {
            object obj2;
            SaveCopyBack back = new SaveCopyBack(CopyBack);
            using (back)
            {
                CallInfo callInfo = null;
                object[] packedArgs = null;
                IDOUtils.PackArguments(0, ArgumentNames, Arguments, ref packedArgs, ref callInfo);
                try
                {
                    return IDOUtils.CreateRefCallSiteAndInvoke(new VBCallBinder(MemberName, callInfo, IgnoreReturn), Instance, packedArgs);
                }
                finally
                {
                    IDOUtils.CopyBackArguments(callInfo, packedArgs, Arguments);
                }
            }
            return obj2;
        }

        internal static object IDOFallbackInvokeDefault(IDynamicMetaObjectProvider Instance, object[] Arguments, string[] ArgumentNames, bool ReportErrors, bool[] CopyBack)
        {
            object obj2;
            SaveCopyBack back = new SaveCopyBack(CopyBack);
            using (back)
            {
                object[] packedArgs = null;
                CallInfo callInfo = null;
                IDOUtils.PackArguments(0, ArgumentNames, Arguments, ref packedArgs, ref callInfo);
                try
                {
                    return IDOUtils.CreateRefCallSiteAndInvoke(new VBInvokeDefaultFallbackBinder(callInfo, ReportErrors), Instance, packedArgs);
                }
                finally
                {
                    IDOUtils.CopyBackArguments(callInfo, packedArgs, Arguments);
                }
            }
            return obj2;
        }

        internal static object IDOGet(IDynamicMetaObjectProvider Instance, string MemberName, object[] Arguments, string[] ArgumentNames, bool[] CopyBack)
        {
            object obj2;
            SaveCopyBack back = new SaveCopyBack(CopyBack);
            using (back)
            {
                object[] packedArgs = null;
                CallInfo callInfo = null;
                IDOUtils.PackArguments(0, ArgumentNames, Arguments, ref packedArgs, ref callInfo);
                try
                {
                    return IDOUtils.CreateRefCallSiteAndInvoke(new VBGetBinder(MemberName, callInfo), Instance, packedArgs);
                }
                finally
                {
                    IDOUtils.CopyBackArguments(callInfo, packedArgs, Arguments);
                }
            }
            return obj2;
        }

        internal static void IDOIndexSet(IDynamicMetaObjectProvider Instance, object[] Arguments, string[] ArgumentNames)
        {
            SaveCopyBack back = new SaveCopyBack(null);
            using (back)
            {
                object[] packedArgs = null;
                CallInfo callInfo = null;
                IDOUtils.PackArguments(1, ArgumentNames, Arguments, ref packedArgs, ref callInfo);
                IDOUtils.CreateFuncCallSiteAndInvoke(new VBIndexSetBinder(callInfo), Instance, packedArgs);
            }
        }

        internal static void IDOIndexSetComplex(IDynamicMetaObjectProvider Instance, object[] Arguments, string[] ArgumentNames, bool OptimisticSet, bool RValueBase)
        {
            SaveCopyBack back = new SaveCopyBack(null);
            using (back)
            {
                object[] packedArgs = null;
                CallInfo callInfo = null;
                IDOUtils.PackArguments(1, ArgumentNames, Arguments, ref packedArgs, ref callInfo);
                IDOUtils.CreateFuncCallSiteAndInvoke(new VBIndexSetComplexBinder(callInfo, OptimisticSet, RValueBase), Instance, packedArgs);
            }
        }

        internal static object IDOInvokeDefault(IDynamicMetaObjectProvider Instance, object[] Arguments, string[] ArgumentNames, bool ReportErrors, bool[] CopyBack)
        {
            object obj2;
            SaveCopyBack back = new SaveCopyBack(CopyBack);
            using (back)
            {
                object[] packedArgs = null;
                CallInfo callInfo = null;
                IDOUtils.PackArguments(0, ArgumentNames, Arguments, ref packedArgs, ref callInfo);
                try
                {
                    return IDOUtils.CreateRefCallSiteAndInvoke(new VBInvokeDefaultBinder(callInfo, ReportErrors), Instance, packedArgs);
                }
                finally
                {
                    IDOUtils.CopyBackArguments(callInfo, packedArgs, Arguments);
                }
            }
            return obj2;
        }

        internal static void IDOSet(IDynamicMetaObjectProvider Instance, string MemberName, string[] ArgumentNames, object[] Arguments)
        {
            SaveCopyBack back = new SaveCopyBack(null);
            using (back)
            {
                if (Arguments.Length == 1)
                {
                    IDOUtils.CreateFuncCallSiteAndInvoke(new VBSetBinder(MemberName), Instance, Arguments);
                }
                else
                {
                    object instance = IDOUtils.CreateFuncCallSiteAndInvoke(new VBGetMemberBinder(MemberName), Instance, Symbols.NoArguments);
                    if (instance == missingMemberSentinel)
                    {
                        NewLateBinding.ObjectLateSet(Instance, null, MemberName, Arguments, ArgumentNames, Symbols.NoTypeArguments);
                    }
                    else
                    {
                        NewLateBinding.LateIndexSet(instance, Arguments, ArgumentNames);
                    }
                }
            }
        }

        internal static void IDOSetComplex(IDynamicMetaObjectProvider Instance, string MemberName, object[] Arguments, string[] ArgumentNames, bool OptimisticSet, bool RValueBase)
        {
            SaveCopyBack back = new SaveCopyBack(null);
            using (back)
            {
                if (Arguments.Length == 1)
                {
                    IDOUtils.CreateFuncCallSiteAndInvoke(new VBSetComplexBinder(MemberName, OptimisticSet, RValueBase), Instance, Arguments);
                }
                else
                {
                    object instance = IDOUtils.CreateFuncCallSiteAndInvoke(new VBGetMemberBinder(MemberName), Instance, Symbols.NoArguments);
                    if (instance == missingMemberSentinel)
                    {
                        NewLateBinding.ObjectLateSetComplex(Instance, null, MemberName, Arguments, ArgumentNames, Symbols.NoTypeArguments, OptimisticSet, RValueBase);
                    }
                    else
                    {
                        NewLateBinding.LateIndexSetComplex(instance, Arguments, ArgumentNames, OptimisticSet, RValueBase);
                    }
                }
            }
        }

        internal static object InvokeUserDefinedOperator(Symbols.UserDefinedOperator Op, object[] Arguments)
        {
            CallSiteBinder binder;
            ExpressionType? nullable = IDOUtils.LinqOperator(Op);
            if (!nullable.HasValue)
            {
                return Operators.InvokeObjectUserDefinedOperator(Op, Arguments);
            }
            ExpressionType linqOp = (ExpressionType) nullable;
            if (Arguments.Length == 1)
            {
                binder = new VBUnaryOperatorBinder(Op, linqOp);
            }
            else
            {
                binder = new VBBinaryOperatorBinder(Op, linqOp);
            }
            object instance = Arguments[0];
            object[] arguments = (Arguments.Length == 1) ? Symbols.NoArguments : new object[] { Arguments[1] };
            return IDOUtils.CreateFuncCallSiteAndInvoke(binder, instance, arguments);
        }

        internal static object UserDefinedConversion(IDynamicMetaObjectProvider Expression, Type TargetType)
        {
            return IDOUtils.CreateConvertCallSiteAndInvoke(new VBConversionBinder(TargetType), Expression);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SaveCopyBack : IDisposable
        {
            [ThreadStatic]
            private static bool[] SavedCopyBack;
            private bool[] oldCopyBack;
            public SaveCopyBack(bool[] copyBack)
            {
                this = new IDOBinder.SaveCopyBack();
                this.oldCopyBack = SavedCopyBack;
                SavedCopyBack = copyBack;
            }

            public void Dispose()
            {
                SavedCopyBack = this.oldCopyBack;
            }

            internal static bool[] GetCopyBack()
            {
                return SavedCopyBack;
            }
        }
    }
}

