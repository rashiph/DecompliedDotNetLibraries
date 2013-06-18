namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Threading;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class ObjectFlowControl
    {
        private ObjectFlowControl()
        {
        }

        public static void CheckForSyncLockOnValueType(object Expression)
        {
            if ((Expression != null) && Expression.GetType().IsValueType)
            {
                throw new ArgumentException(Utils.GetResourceString("SyncLockRequiresReferenceType1", new string[] { Utils.VBFriendlyName(Expression.GetType()) }));
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed class ForLoopControl
        {
            private object Counter;
            private Type EnumType;
            private object Limit;
            private Symbols.Method OperatorGreaterEqual;
            private Symbols.Method OperatorLessEqual;
            private Symbols.Method OperatorPlus;
            private bool PositiveStep;
            private object StepValue;
            private bool UseUserDefinedOperators;
            private Type WidestType;
            private TypeCode WidestTypeCode;

            private ForLoopControl()
            {
            }

            private static bool CheckContinueLoop(ObjectFlowControl.ForLoopControl LoopFor)
            {
                if (!LoopFor.UseUserDefinedOperators)
                {
                    try
                    {
                        int num = ((IComparable) LoopFor.Counter).CompareTo(LoopFor.Limit);
                        if (LoopFor.PositiveStep)
                        {
                            return (num <= 0);
                        }
                        return (num >= 0);
                    }
                    catch (InvalidCastException)
                    {
                        throw new ArgumentException(Utils.GetResourceString("Argument_IComparable2", new string[] { "loop control variable", Utils.VBFriendlyName(LoopFor.Counter) }));
                    }
                }
                if (LoopFor.PositiveStep)
                {
                    return Conversions.ToBoolean(Operators.InvokeUserDefinedOperator(LoopFor.OperatorLessEqual, true, new object[] { LoopFor.Counter, LoopFor.Limit }));
                }
                return Conversions.ToBoolean(Operators.InvokeUserDefinedOperator(LoopFor.OperatorGreaterEqual, true, new object[] { LoopFor.Counter, LoopFor.Limit }));
            }

            private static object ConvertLoopElement(string ElementName, object Value, Type SourceType, Type TargetType)
            {
                object obj2;
                try
                {
                    obj2 = Conversions.ChangeType(Value, TargetType);
                }
                catch (AccessViolationException exception)
                {
                    throw exception;
                }
                catch (StackOverflowException exception2)
                {
                    throw exception2;
                }
                catch (OutOfMemoryException exception3)
                {
                    throw exception3;
                }
                catch (ThreadAbortException exception4)
                {
                    throw exception4;
                }
                catch (Exception)
                {
                    throw new ArgumentException(Utils.GetResourceString("ForLoop_ConvertToType3", new string[] { ElementName, Utils.VBFriendlyName(SourceType), Utils.VBFriendlyName(TargetType) }));
                }
                return obj2;
            }

            public static bool ForLoopInitObj(object Counter, object Start, object Limit, object StepValue, ref object LoopForResult, ref object CounterResult)
            {
                if (Start == null)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidNullValue1", new string[] { "Start" }));
                }
                if (Limit == null)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidNullValue1", new string[] { "Limit" }));
                }
                if (StepValue == null)
                {
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidNullValue1", new string[] { "Step" }));
                }
                Type type3 = Start.GetType();
                Type type2 = Limit.GetType();
                Type type4 = StepValue.GetType();
                Type type5 = GetWidestType(type4, type3, type2);
                if (type5 == null)
                {
                    throw new ArgumentException(Utils.GetResourceString("ForLoop_CommonType3", new string[] { Utils.VBFriendlyName(type3), Utils.VBFriendlyName(type2), Utils.VBFriendlyName(StepValue) }));
                }
                ObjectFlowControl.ForLoopControl loopFor = new ObjectFlowControl.ForLoopControl();
                TypeCode typeCode = Symbols.GetTypeCode(type5);
                switch (typeCode)
                {
                    case TypeCode.Object:
                        loopFor.UseUserDefinedOperators = true;
                        break;

                    case TypeCode.String:
                        typeCode = TypeCode.Double;
                        break;
                }
                TypeCode code2 = Type.GetTypeCode(type3);
                TypeCode code = Type.GetTypeCode(type2);
                TypeCode code3 = Type.GetTypeCode(type4);
                Type type = null;
                if ((code2 == typeCode) && type3.IsEnum)
                {
                    type = type3;
                }
                if ((code == typeCode) && type2.IsEnum)
                {
                    if ((type != null) && (type != type2))
                    {
                        type = null;
                        goto Label_015E;
                    }
                    type = type2;
                }
                if ((code3 == typeCode) && type4.IsEnum)
                {
                    if ((type != null) && (type != type4))
                    {
                        type = null;
                    }
                    else
                    {
                        type = type4;
                    }
                }
            Label_015E:
                loopFor.EnumType = type;
                if (!loopFor.UseUserDefinedOperators)
                {
                    loopFor.WidestType = Symbols.MapTypeCodeToType(typeCode);
                }
                else
                {
                    loopFor.WidestType = type5;
                }
                loopFor.WidestTypeCode = typeCode;
                loopFor.Counter = ConvertLoopElement("Start", Start, type3, loopFor.WidestType);
                loopFor.Limit = ConvertLoopElement("Limit", Limit, type2, loopFor.WidestType);
                loopFor.StepValue = ConvertLoopElement("Step", StepValue, type4, loopFor.WidestType);
                if (loopFor.UseUserDefinedOperators)
                {
                    loopFor.OperatorPlus = VerifyForLoopOperator(Symbols.UserDefinedOperator.Plus, loopFor.Counter, loopFor.WidestType);
                    VerifyForLoopOperator(Symbols.UserDefinedOperator.Minus, loopFor.Counter, loopFor.WidestType);
                    loopFor.OperatorLessEqual = VerifyForLoopOperator(Symbols.UserDefinedOperator.LessEqual, loopFor.Counter, loopFor.WidestType);
                    loopFor.OperatorGreaterEqual = VerifyForLoopOperator(Symbols.UserDefinedOperator.GreaterEqual, loopFor.Counter, loopFor.WidestType);
                }
                loopFor.PositiveStep = Operators.ConditionalCompareObjectGreaterEqual(loopFor.StepValue, Operators.SubtractObject(loopFor.StepValue, loopFor.StepValue), false);
                LoopForResult = loopFor;
                if (loopFor.EnumType != null)
                {
                    CounterResult = Enum.ToObject(loopFor.EnumType, loopFor.Counter);
                }
                else
                {
                    CounterResult = loopFor.Counter;
                }
                return CheckContinueLoop(loopFor);
            }

            public static bool ForNextCheckDec(decimal count, decimal limit, decimal StepValue)
            {
                if (decimal.Compare(StepValue, decimal.Zero) >= 0)
                {
                    return (decimal.Compare(count, limit) <= 0);
                }
                return (decimal.Compare(count, limit) >= 0);
            }

            public static bool ForNextCheckObj(object Counter, object LoopObj, ref object CounterResult)
            {
                if (LoopObj == null)
                {
                    throw ExceptionUtils.VbMakeException(0x5c);
                }
                if (Counter == null)
                {
                    throw new NullReferenceException(Utils.GetResourceString("Argument_InvalidNullValue1", new string[] { "Counter" }));
                }
                ObjectFlowControl.ForLoopControl loopFor = (ObjectFlowControl.ForLoopControl) LoopObj;
                bool flag2 = false;
                if (!loopFor.UseUserDefinedOperators)
                {
                    TypeCode typeCode = ((IConvertible) Counter).GetTypeCode();
                    if ((typeCode != loopFor.WidestTypeCode) || (typeCode == TypeCode.String))
                    {
                        if (typeCode == TypeCode.Object)
                        {
                            throw new ArgumentException(Utils.GetResourceString("ForLoop_CommonType2", new string[] { Utils.VBFriendlyName(Symbols.MapTypeCodeToType(typeCode)), Utils.VBFriendlyName(loopFor.WidestType) }));
                        }
                        TypeCode code2 = Symbols.GetTypeCode(GetWidestType(Symbols.MapTypeCodeToType(typeCode), loopFor.WidestType));
                        if (code2 == TypeCode.String)
                        {
                            code2 = TypeCode.Double;
                        }
                        loopFor.WidestTypeCode = code2;
                        loopFor.WidestType = Symbols.MapTypeCodeToType(code2);
                        flag2 = true;
                    }
                }
                if (flag2 || loopFor.UseUserDefinedOperators)
                {
                    Counter = ConvertLoopElement("Start", Counter, Counter.GetType(), loopFor.WidestType);
                    if (!loopFor.UseUserDefinedOperators)
                    {
                        loopFor.Limit = ConvertLoopElement("Limit", loopFor.Limit, loopFor.Limit.GetType(), loopFor.WidestType);
                        loopFor.StepValue = ConvertLoopElement("Step", loopFor.StepValue, loopFor.StepValue.GetType(), loopFor.WidestType);
                    }
                }
                if (!loopFor.UseUserDefinedOperators)
                {
                    loopFor.Counter = Operators.AddObject(Counter, loopFor.StepValue);
                    TypeCode code3 = ((IConvertible) loopFor.Counter).GetTypeCode();
                    if (loopFor.EnumType != null)
                    {
                        CounterResult = Enum.ToObject(loopFor.EnumType, loopFor.Counter);
                    }
                    else
                    {
                        CounterResult = loopFor.Counter;
                    }
                    if (code3 != loopFor.WidestTypeCode)
                    {
                        loopFor.Limit = Conversions.ChangeType(loopFor.Limit, Symbols.MapTypeCodeToType(code3));
                        loopFor.StepValue = Conversions.ChangeType(loopFor.StepValue, Symbols.MapTypeCodeToType(code3));
                        return false;
                    }
                }
                else
                {
                    loopFor.Counter = Operators.InvokeUserDefinedOperator(loopFor.OperatorPlus, true, new object[] { Counter, loopFor.StepValue });
                    if (loopFor.Counter.GetType() != loopFor.WidestType)
                    {
                        loopFor.Counter = ConvertLoopElement("Start", loopFor.Counter, loopFor.Counter.GetType(), loopFor.WidestType);
                    }
                    CounterResult = loopFor.Counter;
                }
                return CheckContinueLoop(loopFor);
            }

            public static bool ForNextCheckR4(float count, float limit, float StepValue)
            {
                if (StepValue >= 0f)
                {
                    return (count <= limit);
                }
                return (count >= limit);
            }

            public static bool ForNextCheckR8(double count, double limit, double StepValue)
            {
                if (StepValue >= 0.0)
                {
                    return (count <= limit);
                }
                return (count >= limit);
            }

            private static Type GetWidestType(Type Type1, Type Type2)
            {
                if ((Type1 != null) && (Type2 != null))
                {
                    if (!Type1.IsEnum && !Type2.IsEnum)
                    {
                        TypeCode typeCode = Symbols.GetTypeCode(Type1);
                        TypeCode code2 = Symbols.GetTypeCode(Type2);
                        if (Symbols.IsNumericType(typeCode) && Symbols.IsNumericType(code2))
                        {
                            return Symbols.MapTypeCodeToType(ConversionResolution.ForLoopWidestTypeCode[(int) typeCode][(int) code2]);
                        }
                    }
                    Symbols.Method operatorMethod = null;
                    switch (ConversionResolution.ClassifyConversion(Type2, Type1, ref operatorMethod))
                    {
                        case ConversionResolution.ConversionClass.Identity:
                        case ConversionResolution.ConversionClass.Widening:
                            return Type2;
                    }
                    operatorMethod = null;
                    if (ConversionResolution.ClassifyConversion(Type1, Type2, ref operatorMethod) == ConversionResolution.ConversionClass.Widening)
                    {
                        return Type1;
                    }
                }
                return null;
            }

            private static Type GetWidestType(Type Type1, Type Type2, Type Type3)
            {
                return GetWidestType(Type1, GetWidestType(Type2, Type3));
            }

            private static Symbols.Method VerifyForLoopOperator(Symbols.UserDefinedOperator Op, object ForLoopArgument, Type ForLoopArgumentType)
            {
                Symbols.Method callableUserDefinedOperator = Operators.GetCallableUserDefinedOperator(Op, new object[] { ForLoopArgument, ForLoopArgument });
                if (callableUserDefinedOperator == null)
                {
                    throw new ArgumentException(Utils.GetResourceString("ForLoop_OperatorRequired2", new string[] { Utils.VBFriendlyNameOfType(ForLoopArgumentType, true), Symbols.OperatorNames[(int) Op] }));
                }
                MethodInfo info = callableUserDefinedOperator.AsMethod() as MethodInfo;
                ParameterInfo[] parameters = info.GetParameters();
                switch (Op)
                {
                    case Symbols.UserDefinedOperator.Plus:
                    case Symbols.UserDefinedOperator.Minus:
                        if (((parameters.Length != 2) || (parameters[0].ParameterType != ForLoopArgumentType)) || ((parameters[1].ParameterType != ForLoopArgumentType) || (info.ReturnType != ForLoopArgumentType)))
                        {
                            throw new ArgumentException(Utils.GetResourceString("ForLoop_UnacceptableOperator2", new string[] { callableUserDefinedOperator.ToString(), Utils.VBFriendlyNameOfType(ForLoopArgumentType, true) }));
                        }
                        return callableUserDefinedOperator;

                    case Symbols.UserDefinedOperator.Multiply:
                    case Symbols.UserDefinedOperator.Divide:
                    case Symbols.UserDefinedOperator.Power:
                    case Symbols.UserDefinedOperator.IntegralDivide:
                    case Symbols.UserDefinedOperator.Concatenate:
                    case Symbols.UserDefinedOperator.ShiftLeft:
                    case Symbols.UserDefinedOperator.ShiftRight:
                    case Symbols.UserDefinedOperator.Modulus:
                    case Symbols.UserDefinedOperator.Or:
                    case Symbols.UserDefinedOperator.Xor:
                    case Symbols.UserDefinedOperator.And:
                    case Symbols.UserDefinedOperator.Like:
                    case Symbols.UserDefinedOperator.Equal:
                    case Symbols.UserDefinedOperator.NotEqual:
                    case Symbols.UserDefinedOperator.Less:
                        return callableUserDefinedOperator;

                    case Symbols.UserDefinedOperator.LessEqual:
                    case Symbols.UserDefinedOperator.GreaterEqual:
                        if (((parameters.Length != 2) || (parameters[0].ParameterType != ForLoopArgumentType)) || (parameters[1].ParameterType != ForLoopArgumentType))
                        {
                            throw new ArgumentException(Utils.GetResourceString("ForLoop_UnacceptableRelOperator2", new string[] { callableUserDefinedOperator.ToString(), Utils.VBFriendlyNameOfType(ForLoopArgumentType, true) }));
                        }
                        return callableUserDefinedOperator;
                }
                return callableUserDefinedOperator;
            }
        }
    }
}

