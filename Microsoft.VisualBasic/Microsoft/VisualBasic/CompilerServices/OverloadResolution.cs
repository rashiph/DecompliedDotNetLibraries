namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class OverloadResolution
    {
        private OverloadResolution()
        {
        }

        private static bool CanConvert(Type TargetType, Type SourceType, bool RejectNarrowingConversion, List<string> Errors, string ParameterName, bool IsByRefCopyBackContext, ref bool RequiresNarrowingConversion, ref bool AllNarrowingIsFromObject)
        {
            Symbols.Method operatorMethod = null;
            ConversionResolution.ConversionClass class2 = ConversionResolution.ClassifyConversion(TargetType, SourceType, ref operatorMethod);
            switch (class2)
            {
                case ConversionResolution.ConversionClass.Identity:
                case ConversionResolution.ConversionClass.Widening:
                    return true;

                case ConversionResolution.ConversionClass.Narrowing:
                    if (!RejectNarrowingConversion)
                    {
                        RequiresNarrowingConversion = true;
                        if (SourceType != typeof(object))
                        {
                            AllNarrowingIsFromObject = false;
                        }
                        return true;
                    }
                    if (Errors != null)
                    {
                        ReportError(Errors, Interaction.IIf<string>(IsByRefCopyBackContext, "ArgumentNarrowingCopyBack3", "ArgumentNarrowing3"), ParameterName, SourceType, TargetType);
                    }
                    return false;
            }
            if (Errors != null)
            {
                ReportError(Errors, Interaction.IIf<string>(class2 == ConversionResolution.ConversionClass.Ambiguous, Interaction.IIf<string>(IsByRefCopyBackContext, "ArgumentMismatchAmbiguousCopyBack3", "ArgumentMismatchAmbiguous3"), Interaction.IIf<string>(IsByRefCopyBackContext, "ArgumentMismatchCopyBack3", "ArgumentMismatch3")), ParameterName, SourceType, TargetType);
            }
            return false;
        }

        private static bool CandidateIsNarrowing(Symbols.Method Candidate)
        {
            return (!Candidate.NotCallable && Candidate.RequiresNarrowingConversion);
        }

        private static bool CandidateIsNotCallable(Symbols.Method Candidate)
        {
            return Candidate.NotCallable;
        }

        private static bool CandidateIsUnspecific(Symbols.Method Candidate)
        {
            return ((!Candidate.NotCallable && !Candidate.RequiresNarrowingConversion) && !Candidate.LessSpecific);
        }

        internal static bool CanMatchArguments(Symbols.Method TargetProcedure, object[] Arguments, string[] ArgumentNames, Type[] TypeArguments, bool RejectNarrowingConversions, List<string> Errors)
        {
            bool flag2 = Errors != null;
            TargetProcedure.ArgumentsValidated = true;
            if (TargetProcedure.IsMethod && Symbols.IsRawGeneric(TargetProcedure.AsMethod()))
            {
                if (TypeArguments.Length == 0)
                {
                    TypeArguments = new Type[(TargetProcedure.TypeParameters.Length - 1) + 1];
                    TargetProcedure.TypeArguments = TypeArguments;
                    if (!InferTypeArguments(TargetProcedure, Arguments, ArgumentNames, TypeArguments, Errors))
                    {
                        return false;
                    }
                }
                else
                {
                    TargetProcedure.TypeArguments = TypeArguments;
                }
                if (!InstantiateGenericMethod(TargetProcedure, TypeArguments, Errors))
                {
                    return false;
                }
            }
            ParameterInfo[] parameters = TargetProcedure.Parameters;
            int length = ArgumentNames.Length;
            int index = 0;
            while (length < Arguments.Length)
            {
                if (index == TargetProcedure.ParamArrayIndex)
                {
                    break;
                }
                if (!CanPassToParameter(TargetProcedure, Arguments[length], parameters[index], false, RejectNarrowingConversions, Errors, ref TargetProcedure.RequiresNarrowingConversion, ref TargetProcedure.AllNarrowingIsFromObject) && !flag2)
                {
                    return false;
                }
                length++;
                index++;
            }
            if (TargetProcedure.HasParamArray)
            {
                if (!TargetProcedure.ParamArrayExpanded)
                {
                    if ((Arguments.Length - length) != 1)
                    {
                        return false;
                    }
                    if (!CanPassToParamArray(TargetProcedure, Arguments[length], parameters[index]))
                    {
                        if (flag2)
                        {
                            ReportError(Errors, "ArgumentMismatch3", parameters[index].Name, GetArgumentTypeInContextOfParameterType(Arguments[length], parameters[index].ParameterType), parameters[index].ParameterType);
                        }
                        return false;
                    }
                }
                else if ((length != (Arguments.Length - 1)) || (Arguments[length] != null))
                {
                    while (length < Arguments.Length)
                    {
                        if (!CanPassToParameter(TargetProcedure, Arguments[length], parameters[index], true, RejectNarrowingConversions, Errors, ref TargetProcedure.RequiresNarrowingConversion, ref TargetProcedure.AllNarrowingIsFromObject) && !flag2)
                        {
                            return false;
                        }
                        length++;
                    }
                }
                else
                {
                    return false;
                }
                index++;
            }
            bool[] flagArray = null;
            if ((ArgumentNames.Length > 0) || (index < parameters.Length))
            {
                flagArray = CreateMatchTable(parameters.Length, index - 1);
            }
            if (ArgumentNames.Length > 0)
            {
                int[] numArray = new int[(ArgumentNames.Length - 1) + 1];
                for (length = 0; length < ArgumentNames.Length; length++)
                {
                    if (!FindParameterByName(parameters, ArgumentNames[length], ref index))
                    {
                        if (!flag2)
                        {
                            return false;
                        }
                        ReportError(Errors, "NamedParamNotFound2", ArgumentNames[length], TargetProcedure);
                    }
                    else if (index == TargetProcedure.ParamArrayIndex)
                    {
                        if (!flag2)
                        {
                            return false;
                        }
                        ReportError(Errors, "NamedParamArrayArgument1", ArgumentNames[length]);
                    }
                    else if (flagArray[index])
                    {
                        if (!flag2)
                        {
                            return false;
                        }
                        ReportError(Errors, "NamedArgUsedTwice2", ArgumentNames[length], TargetProcedure);
                    }
                    else
                    {
                        if (!CanPassToParameter(TargetProcedure, Arguments[length], parameters[index], false, RejectNarrowingConversions, Errors, ref TargetProcedure.RequiresNarrowingConversion, ref TargetProcedure.AllNarrowingIsFromObject) && !flag2)
                        {
                            return false;
                        }
                        flagArray[index] = true;
                        numArray[length] = index;
                    }
                }
                TargetProcedure.NamedArgumentMapping = numArray;
            }
            if (flagArray != null)
            {
                int num4 = flagArray.Length - 1;
                for (int i = 0; i <= num4; i++)
                {
                    if (!flagArray[i] && !parameters[i].IsOptional)
                    {
                        if (!flag2)
                        {
                            return false;
                        }
                        ReportError(Errors, "OmittedArgument1", parameters[i].Name);
                    }
                }
            }
            if ((Errors != null) && (Errors.Count > 0))
            {
                return false;
            }
            return true;
        }

        private static bool CanPassToParamArray(Symbols.Method TargetProcedure, object Argument, ParameterInfo Parameter)
        {
            if (Argument != null)
            {
                Type parameterType = Parameter.ParameterType;
                Type argumentTypeInContextOfParameterType = GetArgumentTypeInContextOfParameterType(Argument, parameterType);
                Symbols.Method operatorMethod = null;
                ConversionResolution.ConversionClass class2 = ConversionResolution.ClassifyConversion(parameterType, argumentTypeInContextOfParameterType, ref operatorMethod);
                if ((class2 != ConversionResolution.ConversionClass.Widening) && (class2 != ConversionResolution.ConversionClass.Identity))
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool CanPassToParameter(Symbols.Method TargetProcedure, object Argument, ParameterInfo Parameter, bool IsExpandedParamArray, bool RejectNarrowingConversions, List<string> Errors, ref bool RequiresNarrowingConversion, ref bool AllNarrowingIsFromObject)
        {
            if (Argument == null)
            {
                return true;
            }
            Type parameterType = Parameter.ParameterType;
            bool isByRef = parameterType.IsByRef;
            if (isByRef || IsExpandedParamArray)
            {
                parameterType = Symbols.GetElementType(parameterType);
            }
            Type argumentTypeInContextOfParameterType = GetArgumentTypeInContextOfParameterType(Argument, parameterType);
            if (Argument == Missing.Value)
            {
                if (Parameter.IsOptional)
                {
                    return true;
                }
                if (!Symbols.IsRootObjectType(parameterType) || !IsExpandedParamArray)
                {
                    if (Errors != null)
                    {
                        if (IsExpandedParamArray)
                        {
                            ReportError(Errors, "OmittedParamArrayArgument");
                        }
                        else
                        {
                            ReportError(Errors, "OmittedArgument1", Parameter.Name);
                        }
                    }
                    return false;
                }
            }
            bool flag = CanConvert(parameterType, argumentTypeInContextOfParameterType, RejectNarrowingConversions, Errors, Parameter.Name, false, ref RequiresNarrowingConversion, ref AllNarrowingIsFromObject);
            if (isByRef && flag)
            {
                return CanConvert(argumentTypeInContextOfParameterType, parameterType, RejectNarrowingConversions, Errors, Parameter.Name, true, ref RequiresNarrowingConversion, ref AllNarrowingIsFromObject);
            }
            return flag;
        }

        internal static List<Symbols.Method> CollectOverloadCandidates(MemberInfo[] Members, object[] Arguments, int ArgumentCount, string[] ArgumentNames, Type[] TypeArguments, bool CollectOnlyOperators, Type TerminatingScope, ref int RejectedForArgumentCount, ref int RejectedForTypeArgumentCount)
        {
            Type type;
            MemberInfo info;
            int num5;
            int length = 0;
            if (TypeArguments != null)
            {
                length = TypeArguments.Length;
            }
            List<Symbols.Method> candidates = new List<Symbols.Method>(Members.Length);
            if (Members.Length == 0)
            {
                return candidates;
            }
            bool flag = true;
            int index = 0;
        Label_0022:
            type = Members[index].DeclaringType;
            if ((TerminatingScope != null) && Symbols.IsOrInheritsFrom(TerminatingScope, type))
            {
                goto Label_01ED;
            }
        Label_0040:
            info = Members[index];
            ParameterInfo[] destinationArray = null;
            int num6 = 0;
            switch (info.MemberType)
            {
                case MemberTypes.Constructor:
                case MemberTypes.Method:
                {
                    MethodBase method = (MethodBase) info;
                    if (CollectOnlyOperators && !Symbols.IsUserDefinedOperator(method))
                    {
                        break;
                    }
                    destinationArray = method.GetParameters();
                    num6 = Symbols.GetTypeParameters(method).Length;
                    if (Symbols.IsShadows(method))
                    {
                        flag = false;
                    }
                    goto Label_0146;
                }
                case MemberTypes.Property:
                {
                    if (CollectOnlyOperators)
                    {
                        break;
                    }
                    PropertyInfo info3 = (PropertyInfo) info;
                    MethodInfo getMethod = info3.GetGetMethod();
                    if (getMethod != null)
                    {
                        destinationArray = getMethod.GetParameters();
                        if (Symbols.IsShadows(getMethod))
                        {
                            flag = false;
                        }
                    }
                    else
                    {
                        MethodInfo setMethod = info3.GetSetMethod();
                        ParameterInfo[] parameters = setMethod.GetParameters();
                        destinationArray = new ParameterInfo[(parameters.Length - 2) + 1];
                        Array.Copy(parameters, destinationArray, destinationArray.Length);
                        if (Symbols.IsShadows(setMethod))
                        {
                            flag = false;
                        }
                    }
                    goto Label_0146;
                }
                case MemberTypes.Custom:
                case MemberTypes.Event:
                case MemberTypes.Field:
                case MemberTypes.TypeInfo:
                case MemberTypes.NestedType:
                    if (!CollectOnlyOperators)
                    {
                        flag = false;
                    }
                    break;
            }
            goto Label_01C8;
        Label_0146:
            num5 = 0;
            int maximumParameterCount = 0;
            int paramArrayIndex = -1;
            Symbols.GetAllParameterCounts(destinationArray, ref num5, ref maximumParameterCount, ref paramArrayIndex);
            bool flag2 = paramArrayIndex >= 0;
            if ((ArgumentCount < num5) || (!flag2 && (ArgumentCount > maximumParameterCount)))
            {
                RejectedForArgumentCount++;
            }
            else if ((length > 0) && (length != num6))
            {
                RejectedForTypeArgumentCount++;
            }
            else
            {
                if (!flag2 || (ArgumentCount == maximumParameterCount))
                {
                    InsertIfMethodAvailable(info, destinationArray, paramArrayIndex, false, Arguments, ArgumentCount, ArgumentNames, TypeArguments, CollectOnlyOperators, candidates);
                }
                if (flag2)
                {
                    InsertIfMethodAvailable(info, destinationArray, paramArrayIndex, true, Arguments, ArgumentCount, ArgumentNames, TypeArguments, CollectOnlyOperators, candidates);
                }
            }
        Label_01C8:
            index++;
            if ((index < Members.Length) && (Members[index].DeclaringType == type))
            {
                goto Label_0040;
            }
            if (flag && (index < Members.Length))
            {
                goto Label_0022;
            }
        Label_01ED:
            index = 0;
            while (index < candidates.Count)
            {
                if (candidates[index] == null)
                {
                    int num7 = index + 1;
                    while ((num7 < candidates.Count) && (candidates[num7] == null))
                    {
                        num7++;
                    }
                    candidates.RemoveRange(index, num7 - index);
                }
                index++;
            }
            return candidates;
        }

        private static void CompareGenericityBasedOnMethodGenericParams(ParameterInfo LeftParameter, ParameterInfo RawLeftParameter, Symbols.Method LeftMember, bool ExpandLeftParamArray, ParameterInfo RightParameter, ParameterInfo RawRightParameter, Symbols.Method RightMember, bool ExpandRightParamArray, ref bool LeftIsLessGeneric, ref bool RightIsLessGeneric, ref bool SignatureMismatch)
        {
            if (LeftMember.IsMethod && RightMember.IsMethod)
            {
                Type parameterType = LeftParameter.ParameterType;
                Type type = RightParameter.ParameterType;
                Type elementType = RawLeftParameter.ParameterType;
                Type type3 = RawRightParameter.ParameterType;
                if (parameterType.IsByRef)
                {
                    parameterType = Symbols.GetElementType(parameterType);
                    elementType = Symbols.GetElementType(elementType);
                }
                if (type.IsByRef)
                {
                    type = Symbols.GetElementType(type);
                    type3 = Symbols.GetElementType(type3);
                }
                if (ExpandLeftParamArray && Symbols.IsParamArray(LeftParameter))
                {
                    parameterType = Symbols.GetElementType(parameterType);
                    elementType = Symbols.GetElementType(elementType);
                }
                if (ExpandRightParamArray && Symbols.IsParamArray(RightParameter))
                {
                    type = Symbols.GetElementType(type);
                    type3 = Symbols.GetElementType(type3);
                }
                if ((parameterType != type) && !Symbols.IsEquivalentType(parameterType, type))
                {
                    SignatureMismatch = true;
                }
                else
                {
                    MethodBase method = LeftMember.AsMethod();
                    MethodBase genericMethodDefinition = RightMember.AsMethod();
                    if (Symbols.IsGeneric(method))
                    {
                        method = ((MethodInfo) method).GetGenericMethodDefinition();
                    }
                    if (Symbols.IsGeneric(genericMethodDefinition))
                    {
                        genericMethodDefinition = ((MethodInfo) genericMethodDefinition).GetGenericMethodDefinition();
                    }
                    if (Symbols.RefersToGenericParameter(elementType, method))
                    {
                        if (!Symbols.RefersToGenericParameter(type3, genericMethodDefinition))
                        {
                            RightIsLessGeneric = true;
                        }
                    }
                    else if (Symbols.RefersToGenericParameter(type3, genericMethodDefinition) && !Symbols.RefersToGenericParameter(elementType, method))
                    {
                        LeftIsLessGeneric = true;
                    }
                }
            }
        }

        private static void CompareGenericityBasedOnTypeGenericParams(ParameterInfo LeftParameter, ParameterInfo RawLeftParameter, Symbols.Method LeftMember, bool ExpandLeftParamArray, ParameterInfo RightParameter, ParameterInfo RawRightParameter, Symbols.Method RightMember, bool ExpandRightParamArray, ref bool LeftIsLessGeneric, ref bool RightIsLessGeneric, ref bool SignatureMismatch)
        {
            Type parameterType = LeftParameter.ParameterType;
            Type type6 = RightParameter.ParameterType;
            Type elementType = RawLeftParameter.ParameterType;
            Type type4 = RawRightParameter.ParameterType;
            if (parameterType.IsByRef)
            {
                parameterType = Symbols.GetElementType(parameterType);
                elementType = Symbols.GetElementType(elementType);
            }
            if (type6.IsByRef)
            {
                type6 = Symbols.GetElementType(type6);
                type4 = Symbols.GetElementType(type4);
            }
            if (ExpandLeftParamArray && Symbols.IsParamArray(LeftParameter))
            {
                parameterType = Symbols.GetElementType(parameterType);
                elementType = Symbols.GetElementType(elementType);
            }
            if (ExpandRightParamArray && Symbols.IsParamArray(RightParameter))
            {
                type6 = Symbols.GetElementType(type6);
                type4 = Symbols.GetElementType(type4);
            }
            if ((parameterType != type6) && !Symbols.IsEquivalentType(parameterType, type6))
            {
                SignatureMismatch = true;
            }
            else
            {
                Type rawDeclaringType = LeftMember.RawDeclaringType;
                Type typ = RightMember.RawDeclaringType;
                if (Symbols.RefersToGenericParameterCLRSemantics(elementType, rawDeclaringType))
                {
                    if (!Symbols.RefersToGenericParameterCLRSemantics(type4, typ))
                    {
                        RightIsLessGeneric = true;
                    }
                }
                else if (Symbols.RefersToGenericParameterCLRSemantics(type4, typ))
                {
                    LeftIsLessGeneric = true;
                }
            }
        }

        private static void CompareNumericTypeSpecificity(Type LeftType, Type RightType, ref bool LeftWins, ref bool RightWins)
        {
            if (LeftType != RightType)
            {
                if (ConversionResolution.NumericSpecificityRank[(int) Symbols.GetTypeCode(LeftType)] < ConversionResolution.NumericSpecificityRank[(int) Symbols.GetTypeCode(RightType)])
                {
                    LeftWins = true;
                }
                else
                {
                    RightWins = true;
                }
            }
        }

        private static void CompareParameterSpecificity(Type ArgumentType, ParameterInfo LeftParameter, MethodBase LeftProcedure, bool ExpandLeftParamArray, ParameterInfo RightParameter, MethodBase RightProcedure, bool ExpandRightParamArray, ref bool LeftWins, ref bool RightWins, ref bool BothLose)
        {
            BothLose = false;
            Type parameterType = LeftParameter.ParameterType;
            Type type = RightParameter.ParameterType;
            if (parameterType.IsByRef)
            {
                parameterType = Symbols.GetElementType(parameterType);
            }
            if (type.IsByRef)
            {
                type = Symbols.GetElementType(type);
            }
            if (ExpandLeftParamArray && Symbols.IsParamArray(LeftParameter))
            {
                parameterType = Symbols.GetElementType(parameterType);
            }
            if (ExpandRightParamArray && Symbols.IsParamArray(RightParameter))
            {
                type = Symbols.GetElementType(type);
            }
            if ((Symbols.IsNumericType(parameterType) && Symbols.IsNumericType(type)) && (!Symbols.IsEnum(parameterType) && !Symbols.IsEnum(type)))
            {
                CompareNumericTypeSpecificity(parameterType, type, ref LeftWins, ref RightWins);
            }
            else
            {
                if (((LeftProcedure != null) && (RightProcedure != null)) && (Symbols.IsRawGeneric(LeftProcedure) && Symbols.IsRawGeneric(RightProcedure)))
                {
                    if (parameterType == type)
                    {
                        return;
                    }
                    int num = Symbols.IndexIn(parameterType, LeftProcedure);
                    int num2 = Symbols.IndexIn(type, RightProcedure);
                    if ((num == num2) && (num >= 0))
                    {
                        return;
                    }
                }
                Symbols.Method operatorMethod = null;
                switch (ConversionResolution.ClassifyConversion(type, parameterType, ref operatorMethod))
                {
                    case ConversionResolution.ConversionClass.Identity:
                        return;

                    case ConversionResolution.ConversionClass.Widening:
                        if ((operatorMethod != null) && (ConversionResolution.ClassifyConversion(parameterType, type, ref operatorMethod) == ConversionResolution.ConversionClass.Widening))
                        {
                            if ((ArgumentType != null) && (ArgumentType == parameterType))
                            {
                                LeftWins = true;
                                return;
                            }
                            if ((ArgumentType != null) && (ArgumentType == type))
                            {
                                RightWins = true;
                                return;
                            }
                            BothLose = true;
                            return;
                        }
                        LeftWins = true;
                        return;
                }
                if (ConversionResolution.ClassifyConversion(parameterType, type, ref operatorMethod) == ConversionResolution.ConversionClass.Widening)
                {
                    RightWins = true;
                }
                else
                {
                    BothLose = true;
                }
            }
        }

        private static bool[] CreateMatchTable(int Size, int LastPositionalMatchIndex)
        {
            bool[] flagArray2 = new bool[(Size - 1) + 1];
            int num2 = LastPositionalMatchIndex;
            for (int i = 0; i <= num2; i++)
            {
                flagArray2[i] = true;
            }
            return flagArray2;
        }

        private static bool DetectArgumentErrors(Symbols.Method TargetProcedure, object[] Arguments, string[] ArgumentNames, Type[] TypeArguments, List<string> Errors)
        {
            return CanMatchArguments(TargetProcedure, Arguments, ArgumentNames, TypeArguments, false, Errors);
        }

        private static bool DetectArgumentNarrowing(Symbols.Method TargetProcedure, object[] Arguments, string[] ArgumentNames, Type[] TypeArguments, List<string> Errors)
        {
            return CanMatchArguments(TargetProcedure, Arguments, ArgumentNames, TypeArguments, true, Errors);
        }

        private static bool DetectUnspecificity(Symbols.Method TargetProcedure, object[] Arguments, string[] ArgumentNames, Type[] TypeArguments, List<string> Errors)
        {
            ReportError(Errors, "NotMostSpecificOverload");
            return false;
        }

        private static bool FindParameterByName(ParameterInfo[] Parameters, string Name, ref int Index)
        {
            for (int i = 0; i < Parameters.Length; i++)
            {
                if (Operators.CompareString(Name, Parameters[i].Name, true) == 0)
                {
                    Index = i;
                    return true;
                }
            }
            return false;
        }

        private static Type GetArgumentType(object Argument)
        {
            if (Argument == null)
            {
                return null;
            }
            Symbols.TypedNothing nothing = Argument as Symbols.TypedNothing;
            if (nothing != null)
            {
                return nothing.Type;
            }
            return Argument.GetType();
        }

        private static Type GetArgumentTypeInContextOfParameterType(object Argument, Type ParameterType)
        {
            Type argumentType = GetArgumentType(Argument);
            if ((argumentType == null) || (ParameterType == null))
            {
                return argumentType;
            }
            if (((!ParameterType.IsImport || !ParameterType.IsInterface) || !ParameterType.IsInstanceOfType(Argument)) && !Symbols.IsEquivalentType(argumentType, ParameterType))
            {
                return argumentType;
            }
            return ParameterType;
        }

        private static bool InferTypeArguments(Symbols.Method TargetProcedure, object[] Arguments, string[] ArgumentNames, Type[] TypeArguments, List<string> Errors)
        {
            bool flag2 = Errors != null;
            ParameterInfo[] rawParameters = TargetProcedure.RawParameters;
            int length = ArgumentNames.Length;
            int index = 0;
            while (length < Arguments.Length)
            {
                if (index == TargetProcedure.ParamArrayIndex)
                {
                    break;
                }
                if (!InferTypeArgumentsFromArgument(TargetProcedure, Arguments[length], rawParameters[index], false, Errors) && !flag2)
                {
                    return false;
                }
                length++;
                index++;
            }
            if (TargetProcedure.HasParamArray)
            {
                if (TargetProcedure.ParamArrayExpanded)
                {
                    while (length < Arguments.Length)
                    {
                        if (!InferTypeArgumentsFromArgument(TargetProcedure, Arguments[length], rawParameters[index], true, Errors) && !flag2)
                        {
                            return false;
                        }
                        length++;
                    }
                }
                else
                {
                    if ((Arguments.Length - length) != 1)
                    {
                        return true;
                    }
                    if (!InferTypeArgumentsFromArgument(TargetProcedure, Arguments[length], rawParameters[index], false, Errors))
                    {
                        return false;
                    }
                }
                index++;
            }
            if (ArgumentNames.Length > 0)
            {
                for (length = 0; length < ArgumentNames.Length; length++)
                {
                    if ((FindParameterByName(rawParameters, ArgumentNames[length], ref index) && (index != TargetProcedure.ParamArrayIndex)) && (!InferTypeArgumentsFromArgument(TargetProcedure, Arguments[length], rawParameters[index], false, Errors) && !flag2))
                    {
                        return false;
                    }
                }
            }
            if ((Errors != null) && (Errors.Count > 0))
            {
                return false;
            }
            return true;
        }

        internal static bool InferTypeArgumentsFromArgument(Symbols.Method TargetProcedure, object Argument, ParameterInfo Parameter, bool IsExpandedParamArray, List<string> Errors)
        {
            if (Argument == null)
            {
                return true;
            }
            Type parameterType = Parameter.ParameterType;
            if (parameterType.IsByRef || IsExpandedParamArray)
            {
                parameterType = Symbols.GetElementType(parameterType);
            }
            if (InferTypeArgumentsFromArgument(GetArgumentTypeInContextOfParameterType(Argument, parameterType), parameterType, TargetProcedure.TypeArguments, TargetProcedure.AsMethod(), true))
            {
                return true;
            }
            if (Errors != null)
            {
                ReportError(Errors, "TypeInferenceFails1", Parameter.Name);
            }
            return false;
        }

        private static bool InferTypeArgumentsFromArgument(Type ArgumentType, Type ParameterType, Type[] TypeInferenceArguments, MethodBase TargetProcedure, bool DigThroughToBasesAndImplements)
        {
            bool flag = InferTypeArgumentsFromArgumentDirectly(ArgumentType, ParameterType, TypeInferenceArguments, TargetProcedure, DigThroughToBasesAndImplements);
            if ((flag || !DigThroughToBasesAndImplements) || (!Symbols.IsInstantiatedGeneric(ParameterType) || (!ParameterType.IsClass && !ParameterType.IsInterface)))
            {
                return flag;
            }
            Type genericTypeDefinition = ParameterType.GetGenericTypeDefinition();
            if (Symbols.IsArrayType(ArgumentType))
            {
                if ((ArgumentType.GetArrayRank() > 1) || ParameterType.IsClass)
                {
                    return false;
                }
                ArgumentType = typeof(IList<>).MakeGenericType(new Type[] { ArgumentType.GetElementType() });
                if (typeof(IList<>) != genericTypeDefinition)
                {
                    goto Label_00AF;
                }
                goto Label_0137;
            }
            if (!ArgumentType.IsClass && !ArgumentType.IsInterface)
            {
                return false;
            }
            if (Symbols.IsInstantiatedGeneric(ArgumentType) && (ArgumentType.GetGenericTypeDefinition() == genericTypeDefinition))
            {
                return false;
            }
        Label_00AF:
            if (!ParameterType.IsClass)
            {
                Type type3 = null;
                foreach (Type type4 in ArgumentType.GetInterfaces())
                {
                    if (Symbols.IsInstantiatedGeneric(type4) && (type4.GetGenericTypeDefinition() == genericTypeDefinition))
                    {
                        if (type3 != null)
                        {
                            return false;
                        }
                        type3 = type4;
                    }
                }
                ArgumentType = type3;
            }
            else
            {
                if (!ArgumentType.IsClass)
                {
                    return false;
                }
                Type baseType = ArgumentType.BaseType;
                while (baseType != null)
                {
                    if (Symbols.IsInstantiatedGeneric(baseType) && (baseType.GetGenericTypeDefinition() == genericTypeDefinition))
                    {
                        break;
                    }
                    baseType = baseType.BaseType;
                }
                ArgumentType = baseType;
            }
            if (ArgumentType == null)
            {
                return false;
            }
        Label_0137:
            return InferTypeArgumentsFromArgumentDirectly(ArgumentType, ParameterType, TypeInferenceArguments, TargetProcedure, DigThroughToBasesAndImplements);
        }

        private static bool InferTypeArgumentsFromArgumentDirectly(Type ArgumentType, Type ParameterType, Type[] TypeInferenceArguments, MethodBase TargetProcedure, bool DigThroughToBasesAndImplements)
        {
            if (Symbols.RefersToGenericParameter(ParameterType, TargetProcedure))
            {
                if (Symbols.IsGenericParameter(ParameterType))
                {
                    if (Symbols.AreGenericMethodDefsEqual(ParameterType.DeclaringMethod, TargetProcedure))
                    {
                        int genericParameterPosition = ParameterType.GenericParameterPosition;
                        if (TypeInferenceArguments[genericParameterPosition] != null)
                        {
                            if (TypeInferenceArguments[genericParameterPosition] != ArgumentType)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            TypeInferenceArguments[genericParameterPosition] = ArgumentType;
                        }
                    }
                }
                else
                {
                    if (Symbols.IsInstantiatedGeneric(ParameterType))
                    {
                        Type type = null;
                        if (Symbols.IsInstantiatedGeneric(ArgumentType) && (ArgumentType.GetGenericTypeDefinition() == ParameterType.GetGenericTypeDefinition()))
                        {
                            type = ArgumentType;
                        }
                        if ((type == null) && DigThroughToBasesAndImplements)
                        {
                            foreach (Type type2 in ArgumentType.GetInterfaces())
                            {
                                if (Symbols.IsInstantiatedGeneric(type2) && (type2.GetGenericTypeDefinition() == ParameterType.GetGenericTypeDefinition()))
                                {
                                    if (type != null)
                                    {
                                        return false;
                                    }
                                    type = type2;
                                }
                            }
                        }
                        if (type == null)
                        {
                            return false;
                        }
                        Type[] typeArguments = Symbols.GetTypeArguments(ParameterType);
                        Type[] typeArray = Symbols.GetTypeArguments(type);
                        int num4 = typeArray.Length - 1;
                        for (int i = 0; i <= num4; i++)
                        {
                            if (!InferTypeArgumentsFromArgument(typeArray[i], typeArguments[i], TypeInferenceArguments, TargetProcedure, false))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                    if (Symbols.IsArrayType(ParameterType))
                    {
                        return ((Symbols.IsArrayType(ArgumentType) && (ParameterType.GetArrayRank() == ArgumentType.GetArrayRank())) && InferTypeArgumentsFromArgument(Symbols.GetElementType(ArgumentType), Symbols.GetElementType(ParameterType), TypeInferenceArguments, TargetProcedure, DigThroughToBasesAndImplements));
                    }
                }
            }
            return true;
        }

        private static void InsertIfMethodAvailable(MemberInfo NewCandidate, ParameterInfo[] NewCandidateSignature, int NewCandidateParamArrayIndex, bool ExpandNewCandidateParamArray, object[] Arguments, int ArgumentCount, string[] ArgumentNames, Type[] TypeArguments, bool CollectOnlyOperators, List<Symbols.Method> Candidates)
        {
            Symbols.Method candidate = null;
            if (!CollectOnlyOperators)
            {
                MethodBase method = NewCandidate as MethodBase;
                bool flag = false;
                if ((NewCandidate.MemberType == MemberTypes.Method) && Symbols.IsRawGeneric(method))
                {
                    candidate = new Symbols.Method(method, NewCandidateSignature, NewCandidateParamArrayIndex, ExpandNewCandidateParamArray);
                    RejectUncallableProcedure(candidate, Arguments, ArgumentNames, TypeArguments);
                    NewCandidate = candidate.AsMethod();
                    NewCandidateSignature = candidate.Parameters;
                }
                if (((NewCandidate != null) && (NewCandidate.MemberType == MemberTypes.Method)) && Symbols.IsRawGeneric(NewCandidate as MethodBase))
                {
                    flag = true;
                }
                int num5 = Candidates.Count - 1;
                for (int i = 0; i <= num5; i++)
                {
                    Symbols.Method method2 = Candidates[i];
                    if (method2 != null)
                    {
                        MethodBase base3;
                        ParameterInfo[] parameters = method2.Parameters;
                        if (method2.IsMethod)
                        {
                            base3 = method2.AsMethod();
                        }
                        else
                        {
                            base3 = null;
                        }
                        if (NewCandidate != method2)
                        {
                            int index = 0;
                            int num = 0;
                            int num6 = ArgumentCount;
                            for (int j = 1; j <= num6; j++)
                            {
                                bool bothLose = false;
                                bool leftWins = false;
                                bool rightWins = false;
                                CompareParameterSpecificity(null, NewCandidateSignature[index], method, ExpandNewCandidateParamArray, parameters[num], base3, method2.ParamArrayExpanded, ref leftWins, ref rightWins, ref bothLose);
                                if (!((bothLose | leftWins) | rightWins))
                                {
                                    if ((index != NewCandidateParamArrayIndex) || !ExpandNewCandidateParamArray)
                                    {
                                        index++;
                                    }
                                    if ((num != method2.ParamArrayIndex) || !method2.ParamArrayExpanded)
                                    {
                                        num++;
                                    }
                                }
                            }
                            if (!IsExactSignatureMatch(NewCandidateSignature, Symbols.GetTypeParameters(NewCandidate).Length, method2.Parameters, method2.TypeParameters.Length))
                            {
                                if (!flag && ((base3 == null) || !Symbols.IsRawGeneric(base3)))
                                {
                                    if (ExpandNewCandidateParamArray || !method2.ParamArrayExpanded)
                                    {
                                        if (ExpandNewCandidateParamArray && !method2.ParamArrayExpanded)
                                        {
                                            return;
                                        }
                                        if (ExpandNewCandidateParamArray || method2.ParamArrayExpanded)
                                        {
                                            if (index <= num)
                                            {
                                                if (num > index)
                                                {
                                                    return;
                                                }
                                            }
                                            else
                                            {
                                                Candidates[i] = null;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Candidates[i] = null;
                                    }
                                }
                            }
                            else
                            {
                                if (NewCandidate.DeclaringType == method2.DeclaringType)
                                {
                                    break;
                                }
                                if ((flag || (base3 == null)) || !Symbols.IsRawGeneric(base3))
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            if (candidate != null)
            {
                Candidates.Add(candidate);
            }
            else if (NewCandidate.MemberType == MemberTypes.Property)
            {
                Candidates.Add(new Symbols.Method((PropertyInfo) NewCandidate, NewCandidateSignature, NewCandidateParamArrayIndex, ExpandNewCandidateParamArray));
            }
            else
            {
                Candidates.Add(new Symbols.Method((MethodBase) NewCandidate, NewCandidateSignature, NewCandidateParamArrayIndex, ExpandNewCandidateParamArray));
            }
        }

        private static bool InstantiateGenericMethod(Symbols.Method TargetProcedure, Type[] TypeArguments, List<string> Errors)
        {
            bool flag2 = Errors != null;
            int num2 = TypeArguments.Length - 1;
            for (int i = 0; i <= num2; i++)
            {
                if (TypeArguments[i] == null)
                {
                    if (!flag2)
                    {
                        return false;
                    }
                    ReportError(Errors, "UnboundTypeParam1", TargetProcedure.TypeParameters[i].Name);
                }
            }
            if (((Errors == null) || (Errors.Count == 0)) && !TargetProcedure.BindGenericArguments())
            {
                if (!flag2)
                {
                    return false;
                }
                ReportError(Errors, "FailedTypeArgumentBinding");
            }
            if ((Errors != null) && (Errors.Count > 0))
            {
                return false;
            }
            return true;
        }

        private static bool IsExactSignatureMatch(ParameterInfo[] LeftSignature, int LeftTypeParameterCount, ParameterInfo[] RightSignature, int RightTypeParameterCount)
        {
            ParameterInfo[] infoArray;
            ParameterInfo[] infoArray2;
            if (LeftSignature.Length >= RightSignature.Length)
            {
                infoArray = LeftSignature;
                infoArray2 = RightSignature;
            }
            else
            {
                infoArray = RightSignature;
                infoArray2 = LeftSignature;
            }
            int num3 = infoArray.Length - 1;
            for (int i = infoArray2.Length; i <= num3; i++)
            {
                if (!infoArray[i].IsOptional)
                {
                    return false;
                }
            }
            int num4 = infoArray2.Length - 1;
            for (int j = 0; j <= num4; j++)
            {
                Type parameterType = infoArray2[j].ParameterType;
                Type elementType = infoArray[j].ParameterType;
                if (parameterType.IsByRef)
                {
                    parameterType = parameterType.GetElementType();
                }
                if (elementType.IsByRef)
                {
                    elementType = elementType.GetElementType();
                }
                if ((parameterType != elementType) && (!infoArray2[j].IsOptional || !infoArray[j].IsOptional))
                {
                    return false;
                }
            }
            return true;
        }

        internal static Symbols.Method LeastGenericProcedure(Symbols.Method Left, Symbols.Method Right)
        {
            if ((!Left.IsGeneric && !Right.IsGeneric) && (!Symbols.IsGeneric(Left.DeclaringType) && !Symbols.IsGeneric(Right.DeclaringType)))
            {
                return null;
            }
            bool signatureMismatch = false;
            Symbols.Method method = LeastGenericProcedure(Left, Right, ComparisonType.GenericSpecificityBasedOnMethodGenericParams, ref signatureMismatch);
            if ((method == null) && !signatureMismatch)
            {
                method = LeastGenericProcedure(Left, Right, ComparisonType.GenericSpecificityBasedOnTypeGenericParams, ref signatureMismatch);
            }
            return method;
        }

        private static Symbols.Method LeastGenericProcedure(Symbols.Method Left, Symbols.Method Right, ComparisonType CompareGenericity, ref bool SignatureMismatch)
        {
            bool leftIsLessGeneric = false;
            bool rightIsLessGeneric = false;
            SignatureMismatch = false;
            if (Left.IsMethod && Right.IsMethod)
            {
                int index = 0;
                int length = Left.Parameters.Length;
                int num3 = Right.Parameters.Length;
                while ((index < length) && (index < num3))
                {
                    switch (CompareGenericity)
                    {
                        case ComparisonType.GenericSpecificityBasedOnMethodGenericParams:
                            CompareGenericityBasedOnMethodGenericParams(Left.Parameters[index], Left.RawParameters[index], Left, Left.ParamArrayExpanded, Right.Parameters[index], Right.RawParameters[index], Right, false, ref leftIsLessGeneric, ref rightIsLessGeneric, ref SignatureMismatch);
                            break;

                        case ComparisonType.GenericSpecificityBasedOnTypeGenericParams:
                            CompareGenericityBasedOnTypeGenericParams(Left.Parameters[index], Left.RawParameters[index], Left, Left.ParamArrayExpanded, Right.Parameters[index], Right.RawParameters[index], Right, false, ref leftIsLessGeneric, ref rightIsLessGeneric, ref SignatureMismatch);
                            break;
                    }
                    if (SignatureMismatch || (leftIsLessGeneric && rightIsLessGeneric))
                    {
                        return null;
                    }
                    index++;
                }
                if ((index < length) || (index < num3))
                {
                    return null;
                }
                if (leftIsLessGeneric)
                {
                    return Left;
                }
                if (rightIsLessGeneric)
                {
                    return Right;
                }
            }
            return null;
        }

        internal static void MatchArguments(Symbols.Method TargetProcedure, object[] Arguments, object[] MatchedArguments)
        {
            ParameterInfo[] parameters = TargetProcedure.Parameters;
            int[] namedArgumentMapping = TargetProcedure.NamedArgumentMapping;
            int index = 0;
            if (namedArgumentMapping != null)
            {
                index = namedArgumentMapping.Length;
            }
            int num2 = 0;
            while (index < Arguments.Length)
            {
                if (num2 == TargetProcedure.ParamArrayIndex)
                {
                    break;
                }
                MatchedArguments[num2] = PassToParameter(Arguments[index], parameters[num2], parameters[num2].ParameterType);
                index++;
                num2++;
            }
            if (TargetProcedure.HasParamArray)
            {
                if (TargetProcedure.ParamArrayExpanded)
                {
                    int length = Arguments.Length - index;
                    ParameterInfo parameter = parameters[num2];
                    Type elementType = parameter.ParameterType.GetElementType();
                    Array array = Array.CreateInstance(elementType, length);
                    for (int i = 0; index < Arguments.Length; i++)
                    {
                        array.SetValue(PassToParameter(Arguments[index], parameter, elementType), i);
                        index++;
                    }
                    MatchedArguments[num2] = array;
                }
                else
                {
                    MatchedArguments[num2] = PassToParameter(Arguments[index], parameters[num2], parameters[num2].ParameterType);
                }
                num2++;
            }
            bool[] flagArray = null;
            if ((namedArgumentMapping != null) || (num2 < parameters.Length))
            {
                flagArray = CreateMatchTable(parameters.Length, num2 - 1);
            }
            if (namedArgumentMapping != null)
            {
                for (index = 0; index < namedArgumentMapping.Length; index++)
                {
                    num2 = namedArgumentMapping[index];
                    MatchedArguments[num2] = PassToParameter(Arguments[index], parameters[num2], parameters[num2].ParameterType);
                    flagArray[num2] = true;
                }
            }
            if (flagArray != null)
            {
                int num6 = flagArray.Length - 1;
                for (int j = 0; j <= num6; j++)
                {
                    if (!flagArray[j])
                    {
                        MatchedArguments[j] = PassToParameter(Missing.Value, parameters[j], parameters[j].ParameterType);
                    }
                }
            }
        }

        private static Symbols.Method MoreSpecificProcedure(Symbols.Method Left, Symbols.Method Right, object[] Arguments, string[] ArgumentNames, ComparisonType CompareGenericity, ref bool BothLose = false, bool ContinueWhenBothLose = false)
        {
            int num;
            MethodBase base2;
            MethodBase base3;
            BothLose = false;
            bool leftWins = false;
            bool rightWins = false;
            if (Left.IsMethod)
            {
                base2 = Left.AsMethod();
            }
            else
            {
                base2 = null;
            }
            if (Right.IsMethod)
            {
                base3 = Right.AsMethod();
            }
            else
            {
                base3 = null;
            }
            int index = 0;
            int num3 = 0;
            for (num = ArgumentNames.Length; num < Arguments.Length; num++)
            {
                Type argumentType = GetArgumentType(Arguments[num]);
                switch (CompareGenericity)
                {
                    case ComparisonType.ParameterSpecificty:
                        CompareParameterSpecificity(argumentType, Left.Parameters[index], base2, Left.ParamArrayExpanded, Right.Parameters[num3], base3, Right.ParamArrayExpanded, ref leftWins, ref rightWins, ref BothLose);
                        break;

                    case ComparisonType.GenericSpecificityBasedOnMethodGenericParams:
                        CompareGenericityBasedOnMethodGenericParams(Left.Parameters[index], Left.RawParameters[index], Left, Left.ParamArrayExpanded, Right.Parameters[num3], Right.RawParameters[num3], Right, Right.ParamArrayExpanded, ref leftWins, ref rightWins, ref BothLose);
                        break;

                    case ComparisonType.GenericSpecificityBasedOnTypeGenericParams:
                        CompareGenericityBasedOnTypeGenericParams(Left.Parameters[index], Left.RawParametersFromType[index], Left, Left.ParamArrayExpanded, Right.Parameters[num3], Right.RawParametersFromType[num3], Right, Right.ParamArrayExpanded, ref leftWins, ref rightWins, ref BothLose);
                        break;
                }
                if ((BothLose && !ContinueWhenBothLose) || (leftWins && rightWins))
                {
                    return null;
                }
                if (index != Left.ParamArrayIndex)
                {
                    index++;
                }
                if (num3 != Right.ParamArrayIndex)
                {
                    num3++;
                }
            }
            for (num = 0; num < ArgumentNames.Length; num++)
            {
                bool flag3 = FindParameterByName(Left.Parameters, ArgumentNames[num], ref index);
                bool flag4 = FindParameterByName(Right.Parameters, ArgumentNames[num], ref num3);
                if (!flag3 || !flag4)
                {
                    throw new InternalErrorException();
                }
                Type type2 = GetArgumentType(Arguments[num]);
                switch (CompareGenericity)
                {
                    case ComparisonType.ParameterSpecificty:
                        CompareParameterSpecificity(type2, Left.Parameters[index], base2, true, Right.Parameters[num3], base3, true, ref leftWins, ref rightWins, ref BothLose);
                        break;

                    case ComparisonType.GenericSpecificityBasedOnMethodGenericParams:
                        CompareGenericityBasedOnMethodGenericParams(Left.Parameters[index], Left.RawParameters[index], Left, true, Right.Parameters[num3], Right.RawParameters[num3], Right, true, ref leftWins, ref rightWins, ref BothLose);
                        break;

                    case ComparisonType.GenericSpecificityBasedOnTypeGenericParams:
                        CompareGenericityBasedOnTypeGenericParams(Left.Parameters[index], Left.RawParameters[index], Left, true, Right.Parameters[num3], Right.RawParameters[num3], Right, true, ref leftWins, ref rightWins, ref BothLose);
                        break;
                }
                if ((BothLose && !ContinueWhenBothLose) || (leftWins && rightWins))
                {
                    return null;
                }
            }
            if (leftWins)
            {
                return Left;
            }
            if (rightWins)
            {
                return Right;
            }
            return null;
        }

        private static Symbols.Method MostSpecificProcedure(List<Symbols.Method> Candidates, ref int CandidateCount, object[] Arguments, string[] ArgumentNames)
        {
            foreach (Symbols.Method method2 in Candidates)
            {
                if (!method2.NotCallable && !method2.RequiresNarrowingConversion)
                {
                    bool flag = true;
                    foreach (Symbols.Method method4 in Candidates)
                    {
                        if ((!method4.NotCallable && !method4.RequiresNarrowingConversion) && ((method4 != method2) || (method4.ParamArrayExpanded != method2.ParamArrayExpanded)))
                        {
                            bool bothLose = false;
                            Symbols.Method method3 = MoreSpecificProcedure(method2, method4, Arguments, ArgumentNames, ComparisonType.ParameterSpecificty, ref bothLose, true);
                            if (method3 == method2)
                            {
                                if (!method4.LessSpecific)
                                {
                                    method4.LessSpecific = true;
                                    CandidateCount--;
                                }
                            }
                            else
                            {
                                flag = false;
                                if ((method3 == method4) && !method2.LessSpecific)
                                {
                                    method2.LessSpecific = true;
                                    CandidateCount--;
                                }
                            }
                        }
                    }
                    if (flag)
                    {
                        return method2;
                    }
                }
            }
            return null;
        }

        internal static object PassToParameter(object Argument, ParameterInfo Parameter, Type ParameterType)
        {
            bool isByRef = ParameterType.IsByRef;
            if (isByRef)
            {
                ParameterType = ParameterType.GetElementType();
            }
            if (Argument is Symbols.TypedNothing)
            {
                Argument = null;
            }
            if ((Argument == Missing.Value) && Parameter.IsOptional)
            {
                Argument = Parameter.DefaultValue;
            }
            if (isByRef)
            {
                Type argumentTypeInContextOfParameterType = GetArgumentTypeInContextOfParameterType(Argument, ParameterType);
                if ((argumentTypeInContextOfParameterType != null) && Symbols.IsValueType(argumentTypeInContextOfParameterType))
                {
                    Argument = Conversions.ForceValueCopy(Argument, argumentTypeInContextOfParameterType);
                }
            }
            return Conversions.ChangeType(Argument, ParameterType);
        }

        private static void RejectUncallableProcedure(Symbols.Method Candidate, object[] Arguments, string[] ArgumentNames, Type[] TypeArguments)
        {
            if (!CanMatchArguments(Candidate, Arguments, ArgumentNames, TypeArguments, false, null))
            {
                Candidate.NotCallable = true;
            }
            Candidate.ArgumentMatchingDone = true;
        }

        private static Symbols.Method RejectUncallableProcedures(List<Symbols.Method> Candidates, object[] Arguments, string[] ArgumentNames, Type[] TypeArguments, ref int CandidateCount, ref bool SomeCandidatesAreGeneric)
        {
            Symbols.Method method = null;
            int num2 = Candidates.Count - 1;
            for (int i = 0; i <= num2; i++)
            {
                Symbols.Method candidate = Candidates[i];
                if (!candidate.ArgumentMatchingDone)
                {
                    RejectUncallableProcedure(candidate, Arguments, ArgumentNames, TypeArguments);
                }
                if (candidate.NotCallable)
                {
                    CandidateCount--;
                }
                else
                {
                    method = candidate;
                    if (candidate.IsGeneric || Symbols.IsGeneric(candidate.DeclaringType))
                    {
                        SomeCandidatesAreGeneric = true;
                    }
                }
            }
            return method;
        }

        private static Symbols.Method RemoveRedundantGenericProcedures(List<Symbols.Method> Candidates, ref int CandidateCount, object[] Arguments, string[] ArgumentNames)
        {
            int num3 = Candidates.Count - 1;
            for (int i = 0; i <= num3; i++)
            {
                Symbols.Method left = Candidates[i];
                if (!left.NotCallable)
                {
                    int num4 = Candidates.Count - 1;
                    for (int j = i + 1; j <= num4; j++)
                    {
                        Symbols.Method right = Candidates[j];
                        if (!right.NotCallable && (left.RequiresNarrowingConversion == right.RequiresNarrowingConversion))
                        {
                            Symbols.Method method4 = null;
                            bool bothLose = false;
                            if (left.IsGeneric || right.IsGeneric)
                            {
                                method4 = MoreSpecificProcedure(left, right, Arguments, ArgumentNames, ComparisonType.GenericSpecificityBasedOnMethodGenericParams, ref bothLose, false);
                                if (method4 != null)
                                {
                                    CandidateCount--;
                                    if (CandidateCount == 1)
                                    {
                                        return method4;
                                    }
                                    if (method4 == left)
                                    {
                                        right.NotCallable = true;
                                    }
                                    else
                                    {
                                        left.NotCallable = true;
                                        break;
                                    }
                                }
                            }
                            if ((!bothLose && (method4 == null)) && (Symbols.IsGeneric(left.DeclaringType) || Symbols.IsGeneric(right.DeclaringType)))
                            {
                                method4 = MoreSpecificProcedure(left, right, Arguments, ArgumentNames, ComparisonType.GenericSpecificityBasedOnTypeGenericParams, ref bothLose, false);
                                if (method4 != null)
                                {
                                    CandidateCount--;
                                    if (CandidateCount == 1)
                                    {
                                        return method4;
                                    }
                                    if (method4 == left)
                                    {
                                        right.NotCallable = true;
                                    }
                                    else
                                    {
                                        left.NotCallable = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        internal static void ReorderArgumentArray(Symbols.Method TargetProcedure, object[] ParameterResults, object[] Arguments, bool[] CopyBack, BindingFlags LookupFlags)
        {
            if (CopyBack != null)
            {
                int num4 = CopyBack.Length - 1;
                for (int i = 0; i <= num4; i++)
                {
                    CopyBack[i] = false;
                }
                if (!Symbols.HasFlag(LookupFlags, BindingFlags.SetProperty) && TargetProcedure.HasByRefParameter)
                {
                    ParameterInfo[] parameters = TargetProcedure.Parameters;
                    int[] namedArgumentMapping = TargetProcedure.NamedArgumentMapping;
                    int index = 0;
                    if (namedArgumentMapping != null)
                    {
                        index = namedArgumentMapping.Length;
                    }
                    int num2 = 0;
                    while (index < Arguments.Length)
                    {
                        if (num2 == TargetProcedure.ParamArrayIndex)
                        {
                            break;
                        }
                        if (parameters[num2].ParameterType.IsByRef)
                        {
                            Arguments[index] = ParameterResults[num2];
                            CopyBack[index] = true;
                        }
                        index++;
                        num2++;
                    }
                    if (namedArgumentMapping != null)
                    {
                        for (index = 0; index < namedArgumentMapping.Length; index++)
                        {
                            num2 = namedArgumentMapping[index];
                            if (parameters[num2].ParameterType.IsByRef)
                            {
                                Arguments[index] = ParameterResults[num2];
                                CopyBack[index] = true;
                            }
                        }
                    }
                }
            }
        }

        private static void ReportError(List<string> Errors, string ResourceID)
        {
            Errors.Add(Utils.GetResourceString(ResourceID));
        }

        private static void ReportError(List<string> Errors, string ResourceID, string Substitution1)
        {
            Errors.Add(Utils.GetResourceString(ResourceID, new string[] { Substitution1 }));
        }

        private static void ReportError(List<string> Errors, string ResourceID, string Substitution1, Symbols.Method Substitution2)
        {
            Errors.Add(Utils.GetResourceString(ResourceID, new string[] { Substitution1, Substitution2.ToString() }));
        }

        private static void ReportError(List<string> Errors, string ResourceID, string Substitution1, Type Substitution2, Type Substitution3)
        {
            Errors.Add(Utils.GetResourceString(ResourceID, new string[] { Substitution1, Utils.VBFriendlyName(Substitution2), Utils.VBFriendlyName(Substitution3) }));
        }

        private static Exception ReportNarrowingProcedures(string OverloadedProcedureName, List<Symbols.Method> Candidates, object[] Arguments, string[] ArgumentNames, Type[] TypeArguments, ResolutionFailure Failure)
        {
            return ReportOverloadResolutionFailure(OverloadedProcedureName, Candidates, Arguments, ArgumentNames, TypeArguments, "NoNonNarrowingOverloadCandidates2", Failure, new ArgumentDetector(OverloadResolution.DetectArgumentNarrowing), new CandidateProperty(OverloadResolution.CandidateIsNarrowing));
        }

        private static Exception ReportOverloadResolutionFailure(string OverloadedProcedureName, List<Symbols.Method> Candidates, object[] Arguments, string[] ArgumentNames, Type[] TypeArguments, string ErrorID, ResolutionFailure Failure, ArgumentDetector Detector, CandidateProperty CandidateFilter)
        {
            StringBuilder builder = new StringBuilder();
            List<string> errors = new List<string>();
            int num = 0;
            int num4 = Candidates.Count - 1;
            for (int i = 0; i <= num4; i++)
            {
                Symbols.Method candidate = Candidates[i];
                if (CandidateFilter(candidate))
                {
                    if (candidate.HasParamArray)
                    {
                        for (int j = i + 1; j < Candidates.Count; j++)
                        {
                            if (CandidateFilter(Candidates[j]) && (Candidates[j] == candidate))
                            {
                                continue;
                            }
                        }
                    }
                    num++;
                    errors.Clear();
                    bool flag = Detector(candidate, Arguments, ArgumentNames, TypeArguments, errors);
                    builder.Append("\r\n    '");
                    builder.Append(candidate.ToString());
                    builder.Append("':");
                    foreach (string str2 in errors)
                    {
                        builder.Append("\r\n        ");
                        builder.Append(str2);
                    }
                }
            }
            string resourceString = Utils.GetResourceString(ErrorID, new string[] { OverloadedProcedureName, builder.ToString() });
            if (num == 1)
            {
                return new InvalidCastException(resourceString);
            }
            return new AmbiguousMatchException(resourceString);
        }

        private static Exception ReportUncallableProcedures(string OverloadedProcedureName, List<Symbols.Method> Candidates, object[] Arguments, string[] ArgumentNames, Type[] TypeArguments, ResolutionFailure Failure)
        {
            return ReportOverloadResolutionFailure(OverloadedProcedureName, Candidates, Arguments, ArgumentNames, TypeArguments, "NoCallableOverloadCandidates2", Failure, new ArgumentDetector(OverloadResolution.DetectArgumentErrors), new CandidateProperty(OverloadResolution.CandidateIsNotCallable));
        }

        private static Exception ReportUnspecificProcedures(string OverloadedProcedureName, List<Symbols.Method> Candidates, ResolutionFailure Failure)
        {
            return ReportOverloadResolutionFailure(OverloadedProcedureName, Candidates, null, null, null, "NoMostSpecificOverload2", Failure, new ArgumentDetector(OverloadResolution.DetectUnspecificity), new CandidateProperty(OverloadResolution.CandidateIsUnspecific));
        }

        internal static Symbols.Method ResolveOverloadedCall(string MethodName, List<Symbols.Method> Candidates, object[] Arguments, string[] ArgumentNames, Type[] TypeArguments, BindingFlags LookupFlags, bool ReportErrors, ref ResolutionFailure Failure)
        {
            Failure = ResolutionFailure.None;
            int count = Candidates.Count;
            bool someCandidatesAreGeneric = false;
            Symbols.Method method = RejectUncallableProcedures(Candidates, Arguments, ArgumentNames, TypeArguments, ref count, ref someCandidatesAreGeneric);
            switch (count)
            {
                case 1:
                    return method;

                case 0:
                    Failure = ResolutionFailure.InvalidArgument;
                    if (ReportErrors)
                    {
                        throw ReportUncallableProcedures(MethodName, Candidates, Arguments, ArgumentNames, TypeArguments, Failure);
                    }
                    return null;
            }
            if (someCandidatesAreGeneric)
            {
                method = RemoveRedundantGenericProcedures(Candidates, ref count, Arguments, ArgumentNames);
                if (count == 1)
                {
                    return method;
                }
            }
            int num2 = 0;
            Symbols.Method method2 = null;
            foreach (Symbols.Method method4 in Candidates)
            {
                if (!method4.NotCallable)
                {
                    if (method4.RequiresNarrowingConversion)
                    {
                        count--;
                        if (method4.AllNarrowingIsFromObject)
                        {
                            num2++;
                            method2 = method4;
                        }
                    }
                    else
                    {
                        method = method4;
                    }
                }
            }
            switch (count)
            {
                case 1:
                    return method;

                case 0:
                    if (num2 == 1)
                    {
                        return method2;
                    }
                    Failure = ResolutionFailure.AmbiguousMatch;
                    if (ReportErrors)
                    {
                        throw ReportNarrowingProcedures(MethodName, Candidates, Arguments, ArgumentNames, TypeArguments, Failure);
                    }
                    return null;
            }
            method = MostSpecificProcedure(Candidates, ref count, Arguments, ArgumentNames);
            if (method != null)
            {
                return method;
            }
            Failure = ResolutionFailure.AmbiguousMatch;
            if (ReportErrors)
            {
                throw ReportUnspecificProcedures(MethodName, Candidates, Failure);
            }
            return null;
        }

        internal static Symbols.Method ResolveOverloadedCall(string MethodName, MemberInfo[] Members, object[] Arguments, string[] ArgumentNames, Type[] TypeArguments, BindingFlags LookupFlags, bool ReportErrors, ref ResolutionFailure Failure)
        {
            int rejectedForArgumentCount = 0;
            int rejectedForTypeArgumentCount = 0;
            List<Symbols.Method> candidates = CollectOverloadCandidates(Members, Arguments, Arguments.Length, ArgumentNames, TypeArguments, false, null, ref rejectedForArgumentCount, ref rejectedForTypeArgumentCount);
            if ((candidates.Count == 1) && !candidates[0].NotCallable)
            {
                return candidates[0];
            }
            if (candidates.Count != 0)
            {
                return ResolveOverloadedCall(MethodName, candidates, Arguments, ArgumentNames, TypeArguments, LookupFlags, ReportErrors, ref Failure);
            }
            Failure = ResolutionFailure.MissingMember;
            if (!ReportErrors)
            {
                return null;
            }
            string resourceKey = "NoViableOverloadCandidates1";
            if (rejectedForArgumentCount > 0)
            {
                resourceKey = "NoArgumentCountOverloadCandidates1";
            }
            else if (rejectedForTypeArgumentCount > 0)
            {
                resourceKey = "NoTypeArgumentCountOverloadCandidates1";
            }
            throw new MissingMemberException(Utils.GetResourceString(resourceKey, new string[] { MethodName }));
        }

        private delegate bool ArgumentDetector(Symbols.Method TargetProcedure, object[] Arguments, string[] ArgumentNames, Type[] TypeArguments, List<string> Errors);

        private delegate bool CandidateProperty(Symbols.Method Candidate);

        private enum ComparisonType
        {
            ParameterSpecificty,
            GenericSpecificityBasedOnMethodGenericParams,
            GenericSpecificityBasedOnTypeGenericParams
        }

        internal enum ResolutionFailure
        {
            None,
            MissingMember,
            InvalidArgument,
            AmbiguousMatch,
            InvalidTarget
        }
    }
}

