namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Dynamic;
    using System.Reflection;
    using System.Runtime;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class NewLateBinding
    {
        private NewLateBinding()
        {
        }

        private static object CallMethod(Symbols.Container BaseReference, string MethodName, object[] Arguments, string[] ArgumentNames, Type[] TypeArguments, bool[] CopyBack, BindingFlags InvocationFlags, bool ReportErrors, ref OverloadResolution.ResolutionFailure Failure)
        {
            Failure = OverloadResolution.ResolutionFailure.None;
            if ((ArgumentNames.Length > Arguments.Length) || ((CopyBack != null) && (CopyBack.Length != Arguments.Length)))
            {
                Failure = OverloadResolution.ResolutionFailure.InvalidArgument;
                if (ReportErrors)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue"));
                }
                return null;
            }
            if (Symbols.HasFlag(InvocationFlags, BindingFlags.SetProperty) && (Arguments.Length < 1))
            {
                Failure = OverloadResolution.ResolutionFailure.InvalidArgument;
                if (ReportErrors)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue"));
                }
                return null;
            }
            MemberInfo[] members = BaseReference.GetMembers(ref MethodName, ReportErrors);
            if ((members == null) || (members.Length == 0))
            {
                Failure = OverloadResolution.ResolutionFailure.MissingMember;
                if (ReportErrors)
                {
                    members = BaseReference.GetMembers(ref MethodName, true);
                }
                return null;
            }
            Symbols.Method targetProcedure = ResolveCall(BaseReference, MethodName, members, Arguments, ArgumentNames, TypeArguments, InvocationFlags, ReportErrors, ref Failure);
            if (Failure == OverloadResolution.ResolutionFailure.None)
            {
                return BaseReference.InvokeMethod(targetProcedure, Arguments, CopyBack, InvocationFlags);
            }
            return null;
        }

        internal static bool CanBindCall(object Instance, string MemberName, object[] Arguments, string[] ArgumentNames, bool IgnoreReturn)
        {
            OverloadResolution.ResolutionFailure failure;
            Symbols.Container baseReference = new Symbols.Container(Instance);
            BindingFlags lookupFlags = BindingFlags.GetProperty | BindingFlags.InvokeMethod;
            if (IgnoreReturn)
            {
                lookupFlags |= BindingFlags.IgnoreReturn;
            }
            MemberInfo[] members = baseReference.GetMembers(ref MemberName, false);
            if ((members == null) || (members.Length == 0))
            {
                return false;
            }
            Symbols.Method method = ResolveCall(baseReference, MemberName, members, Arguments, ArgumentNames, Symbols.NoTypeArguments, lookupFlags, false, ref failure);
            return (failure == OverloadResolution.ResolutionFailure.None);
        }

        internal static bool CanBindGet(object Instance, string MemberName, object[] Arguments, string[] ArgumentNames)
        {
            Symbols.Container baseReference = new Symbols.Container(Instance);
            BindingFlags lookupFlags = BindingFlags.GetProperty | BindingFlags.InvokeMethod;
            MemberInfo[] members = baseReference.GetMembers(ref MemberName, false);
            if ((members != null) && (members.Length != 0))
            {
                OverloadResolution.ResolutionFailure failure;
                if (members[0].MemberType == MemberTypes.Field)
                {
                    return true;
                }
                Symbols.Method method = ResolveCall(baseReference, MemberName, members, Arguments, ArgumentNames, Symbols.NoTypeArguments, lookupFlags, false, ref failure);
                if (failure == OverloadResolution.ResolutionFailure.None)
                {
                    return true;
                }
                if (((Arguments.Length > 0) && (members.Length == 1)) && IsZeroArgumentCall(members[0]))
                {
                    method = ResolveCall(baseReference, MemberName, members, Symbols.NoArguments, Symbols.NoArgumentNames, Symbols.NoTypeArguments, lookupFlags, false, ref failure);
                    if (failure == OverloadResolution.ResolutionFailure.None)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static bool CanBindInvokeDefault(object Instance, object[] Arguments, string[] ArgumentNames, bool ReportErrors)
        {
            Symbols.Container container = new Symbols.Container(Instance);
            ReportErrors = (ReportErrors || (Arguments.Length != 0)) || container.IsArray;
            if (!ReportErrors)
            {
                return true;
            }
            if (container.IsArray)
            {
                return (ArgumentNames.Length == 0);
            }
            return CanBindCall(Instance, "", Arguments, ArgumentNames, false);
        }

        internal static bool CanBindSet(object Instance, string MemberName, object Value, bool OptimisticSet, bool RValueBase)
        {
            OverloadResolution.ResolutionFailure failure;
            Symbols.Container baseReference = new Symbols.Container(Instance);
            object[] arguments = new object[] { Value };
            MemberInfo[] members = baseReference.GetMembers(ref MemberName, false);
            if ((members == null) || (members.Length == 0))
            {
                return false;
            }
            if (members[0].MemberType == MemberTypes.Field)
            {
                if (((arguments.Length == 1) && RValueBase) && baseReference.IsValueType)
                {
                    return false;
                }
                return true;
            }
            Symbols.Method method = ResolveCall(baseReference, MemberName, members, arguments, Symbols.NoArgumentNames, Symbols.NoTypeArguments, BindingFlags.SetProperty, false, ref failure);
            if (failure == OverloadResolution.ResolutionFailure.None)
            {
                if (RValueBase && baseReference.IsValueType)
                {
                    return false;
                }
                return true;
            }
            BindingFlags lookupFlags = BindingFlags.GetProperty | BindingFlags.InvokeMethod;
            if (failure == OverloadResolution.ResolutionFailure.MissingMember)
            {
                method = ResolveCall(baseReference, MemberName, members, Symbols.NoArguments, Symbols.NoArgumentNames, Symbols.NoTypeArguments, lookupFlags, false, ref failure);
                if (failure == OverloadResolution.ResolutionFailure.None)
                {
                    return true;
                }
            }
            return OptimisticSet;
        }

        internal static bool CanIndexSetComplex(object Instance, object[] Arguments, string[] ArgumentNames, bool OptimisticSet, bool RValueBase)
        {
            OverloadResolution.ResolutionFailure failure;
            Symbols.Container baseReference = new Symbols.Container(Instance);
            if (baseReference.IsArray)
            {
                return (ArgumentNames.Length == 0);
            }
            string memberName = "";
            BindingFlags setProperty = BindingFlags.SetProperty;
            MemberInfo[] members = baseReference.GetMembers(ref memberName, false);
            if ((members == null) || (members.Length == 0))
            {
                return false;
            }
            Symbols.Method method = ResolveCall(baseReference, memberName, members, Arguments, ArgumentNames, Symbols.NoTypeArguments, setProperty, false, ref failure);
            if (failure != OverloadResolution.ResolutionFailure.None)
            {
                return OptimisticSet;
            }
            if (RValueBase && baseReference.IsValueType)
            {
                return false;
            }
            return true;
        }

        internal static object[] ConstructCallArguments(Symbols.Method TargetProcedure, object[] Arguments, BindingFlags LookupFlags)
        {
            ParameterInfo[] parameters = GetCallTarget(TargetProcedure, LookupFlags).GetParameters();
            object[] matchedArguments = new object[(parameters.Length - 1) + 1];
            int length = Arguments.Length;
            object argument = null;
            if (Symbols.HasFlag(LookupFlags, BindingFlags.SetProperty))
            {
                object[] sourceArray = Arguments;
                Arguments = new object[(length - 2) + 1];
                Array.Copy(sourceArray, Arguments, Arguments.Length);
                argument = sourceArray[length - 1];
            }
            OverloadResolution.MatchArguments(TargetProcedure, Arguments, matchedArguments);
            if (Symbols.HasFlag(LookupFlags, BindingFlags.SetProperty))
            {
                ParameterInfo parameter = parameters[parameters.Length - 1];
                matchedArguments[parameters.Length - 1] = OverloadResolution.PassToParameter(argument, parameter, parameter.ParameterType);
            }
            return matchedArguments;
        }

        [DebuggerStepThrough, Obsolete("do not use this method", true), DebuggerHidden, EditorBrowsable(EditorBrowsableState.Never)]
        public static object FallbackCall(object Instance, string MemberName, object[] Arguments, string[] ArgumentNames, bool IgnoreReturn)
        {
            return ObjectLateCall(Instance, null, MemberName, Arguments, ArgumentNames, Symbols.NoTypeArguments, IDOBinder.GetCopyBack(), IgnoreReturn);
        }

        [DebuggerHidden, EditorBrowsable(EditorBrowsableState.Never), Obsolete("do not use this method", true), DebuggerStepThrough]
        public static object FallbackGet(object Instance, string MemberName, object[] Arguments, string[] ArgumentNames)
        {
            return ObjectLateGet(Instance, null, MemberName, Arguments, ArgumentNames, Symbols.NoTypeArguments, IDOBinder.GetCopyBack());
        }

        [DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never), Obsolete("do not use this method", true), DebuggerHidden, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void FallbackIndexSet(object Instance, object[] Arguments, string[] ArgumentNames)
        {
            ObjectLateIndexSet(Instance, Arguments, ArgumentNames);
        }

        [DebuggerStepThrough, Obsolete("do not use this method", true), EditorBrowsable(EditorBrowsableState.Never), DebuggerHidden, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void FallbackIndexSetComplex(object Instance, object[] Arguments, string[] ArgumentNames, bool OptimisticSet, bool RValueBase)
        {
            ObjectLateIndexSetComplex(Instance, Arguments, ArgumentNames, OptimisticSet, RValueBase);
        }

        [DebuggerStepThrough, DebuggerHidden, EditorBrowsable(EditorBrowsableState.Never), Obsolete("do not use this method", true)]
        public static object FallbackInvokeDefault1(object Instance, object[] Arguments, string[] ArgumentNames, bool ReportErrors)
        {
            return IDOBinder.IDOFallbackInvokeDefault((IDynamicMetaObjectProvider) Instance, Arguments, ArgumentNames, ReportErrors, IDOBinder.GetCopyBack());
        }

        [DebuggerStepThrough, DebuggerHidden, EditorBrowsable(EditorBrowsableState.Never), Obsolete("do not use this method", true)]
        public static object FallbackInvokeDefault2(object Instance, object[] Arguments, string[] ArgumentNames, bool ReportErrors)
        {
            return ObjectLateInvokeDefault(Instance, Arguments, ArgumentNames, ReportErrors, IDOBinder.GetCopyBack());
        }

        [DebuggerHidden, DebuggerStepThrough, Obsolete("do not use this method", true), EditorBrowsable(EditorBrowsableState.Never)]
        public static void FallbackSet(object Instance, string MemberName, object[] Arguments)
        {
            ObjectLateSet(Instance, null, MemberName, Arguments, Symbols.NoArgumentNames, Symbols.NoTypeArguments);
        }

        [EditorBrowsable(EditorBrowsableState.Never), DebuggerStepThrough, DebuggerHidden, Obsolete("do not use this method", true)]
        public static void FallbackSetComplex(object Instance, string MemberName, object[] Arguments, bool OptimisticSet, bool RValueBase)
        {
            ObjectLateSetComplex(Instance, null, MemberName, Arguments, new string[0], Symbols.NoTypeArguments, OptimisticSet, RValueBase);
        }

        internal static MethodBase GetCallTarget(Symbols.Method TargetProcedure, BindingFlags Flags)
        {
            if (TargetProcedure.IsMethod)
            {
                return TargetProcedure.AsMethod();
            }
            if (TargetProcedure.IsProperty)
            {
                return MatchesPropertyRequirements(TargetProcedure, Flags);
            }
            return null;
        }

        private static object InternalLateIndexGet(object Instance, object[] Arguments, string[] ArgumentNames, bool ReportErrors, ref OverloadResolution.ResolutionFailure Failure, bool[] CopyBack)
        {
            Failure = OverloadResolution.ResolutionFailure.None;
            if (Arguments == null)
            {
                Arguments = Symbols.NoArguments;
            }
            if (ArgumentNames == null)
            {
                ArgumentNames = Symbols.NoArgumentNames;
            }
            Symbols.Container baseReference = new Symbols.Container(Instance);
            if (baseReference.IsCOMObject)
            {
                return LateBinding.LateIndexGet(Instance, Arguments, ArgumentNames);
            }
            if (!baseReference.IsArray)
            {
                return CallMethod(baseReference, "", Arguments, ArgumentNames, Symbols.NoTypeArguments, CopyBack, BindingFlags.GetProperty | BindingFlags.InvokeMethod, ReportErrors, ref Failure);
            }
            if (ArgumentNames.Length > 0)
            {
                Failure = OverloadResolution.ResolutionFailure.InvalidArgument;
                if (ReportErrors)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidNamedArgs"));
                }
                return null;
            }
            ResetCopyback(CopyBack);
            return baseReference.GetArrayValue(Arguments);
        }

        private static object InternalLateInvokeDefault(object Instance, object[] Arguments, string[] ArgumentNames, bool ReportErrors, bool[] CopyBack)
        {
            IDynamicMetaObjectProvider instance = IDOUtils.TryCastToIDMOP(Instance);
            if (instance != null)
            {
                return IDOBinder.IDOInvokeDefault(instance, Arguments, ArgumentNames, ReportErrors, CopyBack);
            }
            return ObjectLateInvokeDefault(Instance, Arguments, ArgumentNames, ReportErrors, CopyBack);
        }

        internal static bool IsZeroArgumentCall(MemberInfo Member)
        {
            if (((Member.MemberType != MemberTypes.Method) || (((MethodInfo) Member).GetParameters().Length != 0)) && ((Member.MemberType != MemberTypes.Property) || (((PropertyInfo) Member).GetIndexParameters().Length != 0)))
            {
                return false;
            }
            return true;
        }

        [DebuggerStepThrough, DebuggerHidden]
        public static object LateCall(object Instance, Type Type, string MemberName, object[] Arguments, string[] ArgumentNames, Type[] TypeArguments, bool[] CopyBack, bool IgnoreReturn)
        {
            Symbols.Container container;
            if (Arguments == null)
            {
                Arguments = Symbols.NoArguments;
            }
            if (ArgumentNames == null)
            {
                ArgumentNames = Symbols.NoArgumentNames;
            }
            if (TypeArguments == null)
            {
                TypeArguments = Symbols.NoTypeArguments;
            }
            if (Type != null)
            {
                container = new Symbols.Container(Type);
            }
            else
            {
                container = new Symbols.Container(Instance);
            }
            if (container.IsCOMObject)
            {
                return LateBinding.InternalLateCall(Instance, Type, MemberName, Arguments, ArgumentNames, CopyBack, IgnoreReturn);
            }
            IDynamicMetaObjectProvider instance = IDOUtils.TryCastToIDMOP(Instance);
            if ((instance != null) && (TypeArguments == Symbols.NoTypeArguments))
            {
                return IDOBinder.IDOCall(instance, MemberName, Arguments, ArgumentNames, CopyBack, IgnoreReturn);
            }
            return ObjectLateCall(Instance, Type, MemberName, Arguments, ArgumentNames, TypeArguments, CopyBack, IgnoreReturn);
        }

        [EditorBrowsable(EditorBrowsableState.Never), DebuggerStepThrough, DebuggerHidden]
        public static object LateCallInvokeDefault(object Instance, object[] Arguments, string[] ArgumentNames, bool ReportErrors)
        {
            return InternalLateInvokeDefault(Instance, Arguments, ArgumentNames, ReportErrors, IDOBinder.GetCopyBack());
        }

        [DebuggerHidden, DebuggerStepThrough]
        public static bool LateCanEvaluate(object instance, Type type, string memberName, object[] arguments, bool allowFunctionEvaluation, bool allowPropertyEvaluation)
        {
            Symbols.Container container;
            if (type != null)
            {
                container = new Symbols.Container(type);
            }
            else
            {
                container = new Symbols.Container(instance);
            }
            MemberInfo[] members = container.GetMembers(ref memberName, false);
            if (members.Length != 0)
            {
                if (members[0].MemberType == MemberTypes.Field)
                {
                    if (arguments.Length == 0)
                    {
                        return true;
                    }
                    container = new Symbols.Container(container.GetFieldValue((FieldInfo) members[0]));
                    return (container.IsArray || allowPropertyEvaluation);
                }
                if (members[0].MemberType == MemberTypes.Method)
                {
                    return allowFunctionEvaluation;
                }
                if (members[0].MemberType == MemberTypes.Property)
                {
                    return allowPropertyEvaluation;
                }
            }
            return true;
        }

        [DebuggerHidden, DebuggerStepThrough]
        public static object LateGet(object Instance, Type Type, string MemberName, object[] Arguments, string[] ArgumentNames, Type[] TypeArguments, bool[] CopyBack)
        {
            Symbols.Container container;
            if (Arguments == null)
            {
                Arguments = Symbols.NoArguments;
            }
            if (ArgumentNames == null)
            {
                ArgumentNames = Symbols.NoArgumentNames;
            }
            if (TypeArguments == null)
            {
                TypeArguments = Symbols.NoTypeArguments;
            }
            if (Type != null)
            {
                container = new Symbols.Container(Type);
            }
            else
            {
                container = new Symbols.Container(Instance);
            }
            if (container.IsCOMObject)
            {
                return LateBinding.LateGet(Instance, Type, MemberName, Arguments, ArgumentNames, CopyBack);
            }
            IDynamicMetaObjectProvider instance = IDOUtils.TryCastToIDMOP(Instance);
            if ((instance != null) && (TypeArguments == Symbols.NoTypeArguments))
            {
                return IDOBinder.IDOGet(instance, MemberName, Arguments, ArgumentNames, CopyBack);
            }
            return ObjectLateGet(Instance, Type, MemberName, Arguments, ArgumentNames, TypeArguments, CopyBack);
        }

        [DebuggerHidden, EditorBrowsable(EditorBrowsableState.Never), DebuggerStepThrough]
        public static object LateGetInvokeDefault(object Instance, object[] Arguments, string[] ArgumentNames, bool ReportErrors)
        {
            if ((IDOUtils.TryCastToIDMOP(Instance) == null) && ((Arguments == null) || (Arguments.Length <= 0)))
            {
                return Instance;
            }
            return InternalLateInvokeDefault(Instance, Arguments, ArgumentNames, ReportErrors, IDOBinder.GetCopyBack());
        }

        [DebuggerStepThrough, DebuggerHidden, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static object LateIndexGet(object Instance, object[] Arguments, string[] ArgumentNames)
        {
            return InternalLateInvokeDefault(Instance, Arguments, ArgumentNames, true, null);
        }

        private static object LateIndexGet(object Instance, object[] Arguments, string[] ArgumentNames, bool[] CopyBack)
        {
            return InternalLateInvokeDefault(Instance, Arguments, ArgumentNames, true, CopyBack);
        }

        [DebuggerHidden, DebuggerStepThrough]
        public static void LateIndexSet(object Instance, object[] Arguments, string[] ArgumentNames)
        {
            IDynamicMetaObjectProvider instance = IDOUtils.TryCastToIDMOP(Instance);
            if (instance != null)
            {
                IDOBinder.IDOIndexSet(instance, Arguments, ArgumentNames);
            }
            else
            {
                ObjectLateIndexSet(Instance, Arguments, ArgumentNames);
            }
        }

        [DebuggerStepThrough, DebuggerHidden]
        public static void LateIndexSetComplex(object Instance, object[] Arguments, string[] ArgumentNames, bool OptimisticSet, bool RValueBase)
        {
            IDynamicMetaObjectProvider instance = IDOUtils.TryCastToIDMOP(Instance);
            if (instance != null)
            {
                IDOBinder.IDOIndexSetComplex(instance, Arguments, ArgumentNames, OptimisticSet, RValueBase);
            }
            else
            {
                ObjectLateIndexSetComplex(Instance, Arguments, ArgumentNames, OptimisticSet, RValueBase);
            }
        }

        [DebuggerHidden, DebuggerStepThrough]
        public static void LateSet(object Instance, Type Type, string MemberName, object[] Arguments, string[] ArgumentNames, Type[] TypeArguments)
        {
            IDynamicMetaObjectProvider instance = IDOUtils.TryCastToIDMOP(Instance);
            if ((instance != null) && (TypeArguments == null))
            {
                IDOBinder.IDOSet(instance, MemberName, ArgumentNames, Arguments);
            }
            else
            {
                ObjectLateSet(Instance, Type, MemberName, Arguments, ArgumentNames, TypeArguments);
            }
        }

        [DebuggerHidden, DebuggerStepThrough]
        public static void LateSet(object Instance, Type Type, string MemberName, object[] Arguments, string[] ArgumentNames, Type[] TypeArguments, bool OptimisticSet, bool RValueBase, CallType CallType)
        {
            Symbols.Container container;
            if (Arguments == null)
            {
                Arguments = Symbols.NoArguments;
            }
            if (ArgumentNames == null)
            {
                ArgumentNames = Symbols.NoArgumentNames;
            }
            if (TypeArguments == null)
            {
                TypeArguments = Symbols.NoTypeArguments;
            }
            if (Type != null)
            {
                container = new Symbols.Container(Type);
            }
            else
            {
                container = new Symbols.Container(Instance);
            }
            if (container.IsCOMObject)
            {
                try
                {
                    LateBinding.InternalLateSet(Instance, ref Type, MemberName, Arguments, ArgumentNames, OptimisticSet, CallType);
                    if (RValueBase && Type.IsValueType)
                    {
                        throw new Exception(Utils.GetResourceString("RValueBaseForValueType", new string[] { Utils.VBFriendlyName(Type, Instance), Utils.VBFriendlyName(Type, Instance) }));
                    }
                }
                catch when (?)
                {
                }
            }
            else
            {
                MemberInfo[] members = container.GetMembers(ref MemberName, true);
                if (members[0].MemberType == MemberTypes.Field)
                {
                    if (TypeArguments.Length > 0)
                    {
                        throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue"));
                    }
                    if (Arguments.Length == 1)
                    {
                        if (RValueBase && container.IsValueType)
                        {
                            throw new Exception(Utils.GetResourceString("RValueBaseForValueType", new string[] { container.VBFriendlyName, container.VBFriendlyName }));
                        }
                        container.SetFieldValue((FieldInfo) members[0], Arguments[0]);
                    }
                    else
                    {
                        LateIndexSetComplex(container.GetFieldValue((FieldInfo) members[0]), Arguments, ArgumentNames, OptimisticSet, true);
                    }
                }
                else
                {
                    OverloadResolution.ResolutionFailure failure;
                    Symbols.Method method;
                    BindingFlags setProperty = BindingFlags.SetProperty;
                    if (ArgumentNames.Length > Arguments.Length)
                    {
                        throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue"));
                    }
                    if (TypeArguments.Length == 0)
                    {
                        method = ResolveCall(container, MemberName, members, Arguments, ArgumentNames, Symbols.NoTypeArguments, setProperty, false, ref failure);
                        if (failure == OverloadResolution.ResolutionFailure.None)
                        {
                            if (RValueBase && container.IsValueType)
                            {
                                throw new Exception(Utils.GetResourceString("RValueBaseForValueType", new string[] { container.VBFriendlyName, container.VBFriendlyName }));
                            }
                            container.InvokeMethod(method, Arguments, null, setProperty);
                            return;
                        }
                    }
                    BindingFlags lookupFlags = BindingFlags.GetProperty | BindingFlags.InvokeMethod;
                    switch (failure)
                    {
                        case OverloadResolution.ResolutionFailure.None:
                        case OverloadResolution.ResolutionFailure.MissingMember:
                            method = ResolveCall(container, MemberName, members, Symbols.NoArguments, Symbols.NoArgumentNames, TypeArguments, lookupFlags, false, ref failure);
                            if (failure == OverloadResolution.ResolutionFailure.None)
                            {
                                object instance = container.InvokeMethod(method, Symbols.NoArguments, null, lookupFlags);
                                if (instance == null)
                                {
                                    throw new MissingMemberException(Utils.GetResourceString("IntermediateLateBoundNothingResult1", new string[] { method.ToString(), container.VBFriendlyName }));
                                }
                                LateIndexSetComplex(instance, Arguments, ArgumentNames, OptimisticSet, true);
                                return;
                            }
                            break;
                    }
                    if (!OptimisticSet)
                    {
                        if (TypeArguments.Length == 0)
                        {
                            ResolveCall(container, MemberName, members, Arguments, ArgumentNames, TypeArguments, setProperty, true, ref failure);
                        }
                        else
                        {
                            ResolveCall(container, MemberName, members, Symbols.NoArguments, Symbols.NoArgumentNames, TypeArguments, lookupFlags, true, ref failure);
                        }
                        throw new InternalErrorException();
                    }
                }
            }
        }

        [DebuggerHidden, DebuggerStepThrough]
        public static void LateSetComplex(object Instance, Type Type, string MemberName, object[] Arguments, string[] ArgumentNames, Type[] TypeArguments, bool OptimisticSet, bool RValueBase)
        {
            IDynamicMetaObjectProvider instance = IDOUtils.TryCastToIDMOP(Instance);
            if ((instance != null) && (TypeArguments == null))
            {
                IDOBinder.IDOSetComplex(instance, MemberName, Arguments, ArgumentNames, OptimisticSet, RValueBase);
            }
            else
            {
                ObjectLateSetComplex(Instance, Type, MemberName, Arguments, ArgumentNames, TypeArguments, OptimisticSet, RValueBase);
            }
        }

        internal static MethodInfo MatchesPropertyRequirements(Symbols.Method TargetProcedure, BindingFlags Flags)
        {
            if (Symbols.HasFlag(Flags, BindingFlags.SetProperty))
            {
                return TargetProcedure.AsProperty().GetSetMethod();
            }
            return TargetProcedure.AsProperty().GetGetMethod();
        }

        [DebuggerStepThrough, DebuggerHidden]
        private static object ObjectLateCall(object Instance, Type Type, string MemberName, object[] Arguments, string[] ArgumentNames, Type[] TypeArguments, bool[] CopyBack, bool IgnoreReturn)
        {
            Symbols.Container container;
            OverloadResolution.ResolutionFailure failure;
            if (Type != null)
            {
                container = new Symbols.Container(Type);
            }
            else
            {
                container = new Symbols.Container(Instance);
            }
            BindingFlags invocationFlags = BindingFlags.GetProperty | BindingFlags.InvokeMethod;
            if (IgnoreReturn)
            {
                invocationFlags |= BindingFlags.IgnoreReturn;
            }
            return CallMethod(container, MemberName, Arguments, ArgumentNames, TypeArguments, CopyBack, invocationFlags, true, ref failure);
        }

        [DebuggerHidden, DebuggerStepThrough]
        private static object ObjectLateGet(object Instance, Type Type, string MemberName, object[] Arguments, string[] ArgumentNames, Type[] TypeArguments, bool[] CopyBack)
        {
            Symbols.Container container;
            OverloadResolution.ResolutionFailure failure;
            if (Type != null)
            {
                container = new Symbols.Container(Type);
            }
            else
            {
                container = new Symbols.Container(Instance);
            }
            BindingFlags lookupFlags = BindingFlags.GetProperty | BindingFlags.InvokeMethod;
            MemberInfo[] members = container.GetMembers(ref MemberName, true);
            if (members[0].MemberType == MemberTypes.Field)
            {
                if (TypeArguments.Length > 0)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue"));
                }
                object fieldValue = container.GetFieldValue((FieldInfo) members[0]);
                if (Arguments.Length == 0)
                {
                    return fieldValue;
                }
                return LateIndexGet(fieldValue, Arguments, ArgumentNames, CopyBack);
            }
            if ((ArgumentNames.Length > Arguments.Length) || ((CopyBack != null) && (CopyBack.Length != Arguments.Length)))
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue"));
            }
            Symbols.Method targetProcedure = ResolveCall(container, MemberName, members, Arguments, ArgumentNames, TypeArguments, lookupFlags, false, ref failure);
            if (failure == OverloadResolution.ResolutionFailure.None)
            {
                return container.InvokeMethod(targetProcedure, Arguments, CopyBack, lookupFlags);
            }
            if (((Arguments.Length > 0) && (members.Length == 1)) && IsZeroArgumentCall(members[0]))
            {
                targetProcedure = ResolveCall(container, MemberName, members, Symbols.NoArguments, Symbols.NoArgumentNames, TypeArguments, lookupFlags, false, ref failure);
                if (failure == OverloadResolution.ResolutionFailure.None)
                {
                    object instance = container.InvokeMethod(targetProcedure, Symbols.NoArguments, null, lookupFlags);
                    if (instance == null)
                    {
                        throw new MissingMemberException(Utils.GetResourceString("IntermediateLateBoundNothingResult1", new string[] { targetProcedure.ToString(), container.VBFriendlyName }));
                    }
                    instance = InternalLateIndexGet(instance, Arguments, ArgumentNames, false, ref failure, CopyBack);
                    if (failure == OverloadResolution.ResolutionFailure.None)
                    {
                        return instance;
                    }
                }
            }
            ResolveCall(container, MemberName, members, Arguments, ArgumentNames, TypeArguments, lookupFlags, true, ref failure);
            throw new InternalErrorException();
        }

        [DebuggerStepThrough, DebuggerHidden, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private static void ObjectLateIndexSet(object Instance, object[] Arguments, string[] ArgumentNames)
        {
            ObjectLateIndexSetComplex(Instance, Arguments, ArgumentNames, false, false);
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal static void ObjectLateIndexSetComplex(object Instance, object[] Arguments, string[] ArgumentNames, bool OptimisticSet, bool RValueBase)
        {
            if (Arguments == null)
            {
                Arguments = Symbols.NoArguments;
            }
            if (ArgumentNames == null)
            {
                ArgumentNames = Symbols.NoArgumentNames;
            }
            Symbols.Container baseReference = new Symbols.Container(Instance);
            if (baseReference.IsArray)
            {
                if (ArgumentNames.Length > 0)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidNamedArgs"));
                }
                baseReference.SetArrayValue(Arguments);
            }
            else
            {
                if (ArgumentNames.Length > Arguments.Length)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue"));
                }
                if (Arguments.Length < 1)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue"));
                }
                string memberName = "";
                if (baseReference.IsCOMObject)
                {
                    LateBinding.LateIndexSetComplex(Instance, Arguments, ArgumentNames, OptimisticSet, RValueBase);
                }
                else
                {
                    OverloadResolution.ResolutionFailure failure;
                    BindingFlags setProperty = BindingFlags.SetProperty;
                    MemberInfo[] members = baseReference.GetMembers(ref memberName, true);
                    Symbols.Method targetProcedure = ResolveCall(baseReference, memberName, members, Arguments, ArgumentNames, Symbols.NoTypeArguments, setProperty, false, ref failure);
                    if (failure == OverloadResolution.ResolutionFailure.None)
                    {
                        if (RValueBase && baseReference.IsValueType)
                        {
                            throw new Exception(Utils.GetResourceString("RValueBaseForValueType", new string[] { baseReference.VBFriendlyName, baseReference.VBFriendlyName }));
                        }
                        baseReference.InvokeMethod(targetProcedure, Arguments, null, setProperty);
                    }
                    else if (!OptimisticSet)
                    {
                        ResolveCall(baseReference, memberName, members, Arguments, ArgumentNames, Symbols.NoTypeArguments, setProperty, true, ref failure);
                        throw new InternalErrorException();
                    }
                }
            }
        }

        [DebuggerStepThrough, DebuggerHidden]
        private static object ObjectLateInvokeDefault(object Instance, object[] Arguments, string[] ArgumentNames, bool ReportErrors, bool[] CopyBack)
        {
            OverloadResolution.ResolutionFailure failure;
            Symbols.Container container = new Symbols.Container(Instance);
            object obj3 = InternalLateIndexGet(Instance, Arguments, ArgumentNames, (ReportErrors || (Arguments.Length != 0)) || container.IsArray, ref failure, CopyBack);
            if (failure != OverloadResolution.ResolutionFailure.None)
            {
                return Instance;
            }
            return obj3;
        }

        internal static void ObjectLateSet(object Instance, Type Type, string MemberName, object[] Arguments, string[] ArgumentNames, Type[] TypeArguments)
        {
            LateSet(Instance, Type, MemberName, Arguments, ArgumentNames, TypeArguments, false, false, (CallType) 0);
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal static void ObjectLateSetComplex(object Instance, Type Type, string MemberName, object[] Arguments, string[] ArgumentNames, Type[] TypeArguments, bool OptimisticSet, bool RValueBase)
        {
            LateSet(Instance, Type, MemberName, Arguments, ArgumentNames, TypeArguments, OptimisticSet, RValueBase, (CallType) 0);
        }

        internal static Exception ReportPropertyMismatch(Symbols.Method TargetProcedure, BindingFlags Flags)
        {
            if (Symbols.HasFlag(Flags, BindingFlags.SetProperty))
            {
                return new MissingMemberException(Utils.GetResourceString("NoSetProperty1", new string[] { TargetProcedure.AsProperty().Name }));
            }
            return new MissingMemberException(Utils.GetResourceString("NoGetProperty1", new string[] { TargetProcedure.AsProperty().Name }));
        }

        internal static void ResetCopyback(bool[] CopyBack)
        {
            if (CopyBack != null)
            {
                int num2 = CopyBack.Length - 1;
                for (int i = 0; i <= num2; i++)
                {
                    CopyBack[i] = false;
                }
            }
        }

        internal static Symbols.Method ResolveCall(Symbols.Container BaseReference, string MethodName, MemberInfo[] Members, object[] Arguments, string[] ArgumentNames, Type[] TypeArguments, BindingFlags LookupFlags, bool ReportErrors, ref OverloadResolution.ResolutionFailure Failure)
        {
            Failure = OverloadResolution.ResolutionFailure.None;
            if ((Members[0].MemberType != MemberTypes.Method) && (Members[0].MemberType != MemberTypes.Property))
            {
                Failure = OverloadResolution.ResolutionFailure.InvalidTarget;
                if (ReportErrors)
                {
                    throw new ArgumentException(Utils.GetResourceString("ExpressionNotProcedure", new string[] { MethodName, BaseReference.VBFriendlyName }));
                }
                return null;
            }
            int length = Arguments.Length;
            object argument = null;
            if (Symbols.HasFlag(LookupFlags, BindingFlags.SetProperty))
            {
                if (Arguments.Length == 0)
                {
                    Failure = OverloadResolution.ResolutionFailure.InvalidArgument;
                    if (ReportErrors)
                    {
                        throw new InvalidCastException(Utils.GetResourceString("PropertySetMissingArgument1", new string[] { MethodName }));
                    }
                    return null;
                }
                object[] sourceArray = Arguments;
                Arguments = new object[(length - 2) + 1];
                Array.Copy(sourceArray, Arguments, Arguments.Length);
                argument = sourceArray[length - 1];
            }
            Symbols.Method targetProcedure = OverloadResolution.ResolveOverloadedCall(MethodName, Members, Arguments, ArgumentNames, TypeArguments, LookupFlags, ReportErrors, ref Failure);
            if (Failure != OverloadResolution.ResolutionFailure.None)
            {
                return null;
            }
            if (!targetProcedure.ArgumentsValidated && !OverloadResolution.CanMatchArguments(targetProcedure, Arguments, ArgumentNames, TypeArguments, false, null))
            {
                Failure = OverloadResolution.ResolutionFailure.InvalidArgument;
                if (!ReportErrors)
                {
                    return null;
                }
                string str = "";
                List<string> list = new List<string>();
                bool flag = OverloadResolution.CanMatchArguments(targetProcedure, Arguments, ArgumentNames, TypeArguments, false, list);
                foreach (string str2 in list)
                {
                    str = str + "\r\n    " + str2;
                }
                throw new InvalidCastException(Utils.GetResourceString("MatchArgumentFailure2", new string[] { targetProcedure.ToString(), str }));
            }
            if (targetProcedure.IsProperty)
            {
                if (MatchesPropertyRequirements(targetProcedure, LookupFlags) == null)
                {
                    Failure = OverloadResolution.ResolutionFailure.InvalidTarget;
                    if (ReportErrors)
                    {
                        throw ReportPropertyMismatch(targetProcedure, LookupFlags);
                    }
                    return null;
                }
            }
            else if (Symbols.HasFlag(LookupFlags, BindingFlags.SetProperty))
            {
                Failure = OverloadResolution.ResolutionFailure.InvalidTarget;
                if (ReportErrors)
                {
                    throw new MissingMemberException(Utils.GetResourceString("MethodAssignment1", new string[] { targetProcedure.AsMethod().Name }));
                }
                return null;
            }
            if (!Symbols.HasFlag(LookupFlags, BindingFlags.SetProperty))
            {
                return targetProcedure;
            }
            ParameterInfo[] parameters = GetCallTarget(targetProcedure, LookupFlags).GetParameters();
            ParameterInfo parameter = parameters[parameters.Length - 1];
            bool requiresNarrowingConversion = false;
            bool allNarrowingIsFromObject = false;
            if (OverloadResolution.CanPassToParameter(targetProcedure, argument, parameter, false, false, null, ref requiresNarrowingConversion, ref allNarrowingIsFromObject))
            {
                return targetProcedure;
            }
            Failure = OverloadResolution.ResolutionFailure.InvalidArgument;
            if (!ReportErrors)
            {
                return null;
            }
            string str3 = "";
            List<string> errors = new List<string>();
            allNarrowingIsFromObject = false;
            requiresNarrowingConversion = false;
            bool flag2 = OverloadResolution.CanPassToParameter(targetProcedure, argument, parameter, false, false, errors, ref allNarrowingIsFromObject, ref requiresNarrowingConversion);
            foreach (string str4 in errors)
            {
                str3 = str3 + "\r\n    " + str4;
            }
            throw new InvalidCastException(Utils.GetResourceString("MatchArgumentFailure2", new string[] { targetProcedure.ToString(), str3 }));
        }
    }
}

