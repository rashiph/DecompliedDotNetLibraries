namespace Microsoft.VisualBasic.CompilerServices
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Threading;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class FlowControl
    {
        private FlowControl()
        {
        }

        private static bool CheckContinueLoop(ObjectFor LoopFor)
        {
            bool flag;
            try
            {
                int num = ((IComparable) LoopFor.Counter).CompareTo(LoopFor.Limit);
                if (LoopFor.PositiveStep)
                {
                    return (num <= 0);
                }
                if (num >= 0)
                {
                    return true;
                }
                flag = false;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_IComparable2", new string[] { "loop control variable", Utils.VBFriendlyName(LoopFor.Counter) }));
            }
            return flag;
        }

        public static void CheckForSyncLockOnValueType(object obj)
        {
            if ((obj != null) && obj.GetType().IsValueType)
            {
                throw new ArgumentException(Utils.GetResourceString("SyncLockRequiresReferenceType1", new string[] { Utils.VBFriendlyName(obj.GetType()) }));
            }
        }

        public static IEnumerator ForEachInArr(Array ary)
        {
            IEnumerator enumerator = ary.GetEnumerator();
            if (enumerator == null)
            {
                throw ExceptionUtils.VbMakeException(0x5c);
            }
            return enumerator;
        }

        public static IEnumerator ForEachInObj(object obj)
        {
            IEnumerable enumerable;
            if (obj == null)
            {
                throw ExceptionUtils.VbMakeException(0x5b);
            }
            try
            {
                enumerable = (IEnumerable) obj;
            }
            catch (StackOverflowException exception)
            {
                throw exception;
            }
            catch (OutOfMemoryException exception2)
            {
                throw exception2;
            }
            catch (ThreadAbortException exception3)
            {
                throw exception3;
            }
            catch (Exception)
            {
                throw ExceptionUtils.MakeException1(100, obj.GetType().ToString());
            }
            IEnumerator enumerator = enumerable.GetEnumerator();
            if (enumerator == null)
            {
                throw ExceptionUtils.MakeException1(100, obj.GetType().ToString());
            }
            return enumerator;
        }

        public static bool ForEachNextObj(ref object obj, IEnumerator enumerator)
        {
            if (enumerator.MoveNext())
            {
                obj = enumerator.Current;
                return true;
            }
            obj = null;
            return false;
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
            Type typ = Start.GetType();
            Type type2 = Limit.GetType();
            Type type4 = StepValue.GetType();
            TypeCode widestType = ObjectType.GetWidestType(Start, Limit, false);
            widestType = ObjectType.GetWidestType(StepValue, widestType);
            switch (widestType)
            {
                case TypeCode.String:
                    widestType = TypeCode.Double;
                    break;

                case TypeCode.Object:
                    throw new ArgumentException(Utils.GetResourceString("ForLoop_CommonType3", new string[] { Utils.VBFriendlyName(typ), Utils.VBFriendlyName(type2), Utils.VBFriendlyName(StepValue) }));
            }
            ObjectFor loopFor = new ObjectFor();
            TypeCode typeCode = Type.GetTypeCode(typ);
            TypeCode code = Type.GetTypeCode(type2);
            TypeCode code3 = Type.GetTypeCode(type4);
            Type type = null;
            if ((typeCode == widestType) && typ.IsEnum)
            {
                type = typ;
            }
            if ((code == widestType) && type2.IsEnum)
            {
                if ((type != null) && (type != type2))
                {
                    type = null;
                    goto Label_0159;
                }
                type = type2;
            }
            if ((code3 == widestType) && type4.IsEnum)
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
        Label_0159:
            loopFor.EnumType = type;
            try
            {
                loopFor.Counter = ObjectType.CTypeHelper(Start, widestType);
            }
            catch (StackOverflowException exception)
            {
                throw exception;
            }
            catch (OutOfMemoryException exception2)
            {
                throw exception2;
            }
            catch (ThreadAbortException exception3)
            {
                throw exception3;
            }
            catch (Exception)
            {
                throw new ArgumentException(Utils.GetResourceString("ForLoop_ConvertToType3", new string[] { "Start", Utils.VBFriendlyName(typ), Utils.VBFriendlyName(ObjectType.TypeFromTypeCode(widestType)) }));
            }
            try
            {
                loopFor.Limit = ObjectType.CTypeHelper(Limit, widestType);
            }
            catch (StackOverflowException exception4)
            {
                throw exception4;
            }
            catch (OutOfMemoryException exception5)
            {
                throw exception5;
            }
            catch (ThreadAbortException exception6)
            {
                throw exception6;
            }
            catch (Exception)
            {
                throw new ArgumentException(Utils.GetResourceString("ForLoop_ConvertToType3", new string[] { "Limit", Utils.VBFriendlyName(type2), Utils.VBFriendlyName(ObjectType.TypeFromTypeCode(widestType)) }));
            }
            try
            {
                loopFor.StepValue = ObjectType.CTypeHelper(StepValue, widestType);
            }
            catch (StackOverflowException exception7)
            {
                throw exception7;
            }
            catch (OutOfMemoryException exception8)
            {
                throw exception8;
            }
            catch (ThreadAbortException exception9)
            {
                throw exception9;
            }
            catch (Exception)
            {
                throw new ArgumentException(Utils.GetResourceString("ForLoop_ConvertToType3", new string[] { "Step", Utils.VBFriendlyName(type4), Utils.VBFriendlyName(ObjectType.TypeFromTypeCode(widestType)) }));
            }
            object obj2 = ObjectType.CTypeHelper(0, widestType);
            IComparable stepValue = (IComparable) loopFor.StepValue;
            if (stepValue.CompareTo(obj2) >= 0)
            {
                loopFor.PositiveStep = true;
            }
            else
            {
                loopFor.PositiveStep = false;
            }
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
            if (StepValue < decimal.Zero)
            {
                return (count >= limit);
            }
            return (count <= limit);
        }

        public static bool ForNextCheckObj(object Counter, object LoopObj, ref object CounterResult)
        {
            TypeCode code;
            TypeCode widestType;
            if (LoopObj == null)
            {
                throw ExceptionUtils.VbMakeException(0x5c);
            }
            if (Counter == null)
            {
                throw new NullReferenceException(Utils.GetResourceString("Argument_InvalidNullValue1", new string[] { "Counter" }));
            }
            ObjectFor loopFor = (ObjectFor) LoopObj;
            TypeCode typeCode = ((IConvertible) Counter).GetTypeCode();
            TypeCode code3 = ((IConvertible) loopFor.StepValue).GetTypeCode();
            if ((typeCode == code3) && (typeCode != TypeCode.String))
            {
                widestType = typeCode;
            }
            else
            {
                widestType = ObjectType.GetWidestType(typeCode, code3);
                if (widestType == TypeCode.String)
                {
                    widestType = TypeCode.Double;
                }
                if (code == TypeCode.Object)
                {
                    throw new ArgumentException(Utils.GetResourceString("ForLoop_CommonType2", new string[] { Utils.VBFriendlyName(ObjectType.TypeFromTypeCode(typeCode)), Utils.VBFriendlyName(ObjectType.TypeFromTypeCode(code3)) }));
                }
                try
                {
                    Counter = ObjectType.CTypeHelper(Counter, widestType);
                }
                catch (StackOverflowException exception)
                {
                    throw exception;
                }
                catch (OutOfMemoryException exception2)
                {
                    throw exception2;
                }
                catch (ThreadAbortException exception3)
                {
                    throw exception3;
                }
                catch (Exception)
                {
                    throw new ArgumentException(Utils.GetResourceString("ForLoop_ConvertToType3", new string[] { "Start", Utils.VBFriendlyName(Counter.GetType()), Utils.VBFriendlyName(ObjectType.TypeFromTypeCode(widestType)) }));
                }
                try
                {
                    loopFor.Limit = ObjectType.CTypeHelper(loopFor.Limit, widestType);
                }
                catch (StackOverflowException exception4)
                {
                    throw exception4;
                }
                catch (OutOfMemoryException exception5)
                {
                    throw exception5;
                }
                catch (ThreadAbortException exception6)
                {
                    throw exception6;
                }
                catch (Exception)
                {
                    throw new ArgumentException(Utils.GetResourceString("ForLoop_ConvertToType3", new string[] { "Limit", Utils.VBFriendlyName(loopFor.Limit.GetType()), Utils.VBFriendlyName(ObjectType.TypeFromTypeCode(widestType)) }));
                }
                try
                {
                    loopFor.StepValue = ObjectType.CTypeHelper(loopFor.StepValue, widestType);
                }
                catch (StackOverflowException exception7)
                {
                    throw exception7;
                }
                catch (OutOfMemoryException exception8)
                {
                    throw exception8;
                }
                catch (ThreadAbortException exception9)
                {
                    throw exception9;
                }
                catch (Exception)
                {
                    throw new ArgumentException(Utils.GetResourceString("ForLoop_ConvertToType3", new string[] { "Step", Utils.VBFriendlyName(loopFor.StepValue.GetType()), Utils.VBFriendlyName(ObjectType.TypeFromTypeCode(widestType)) }));
                }
            }
            loopFor.Counter = ObjectType.AddObj(Counter, loopFor.StepValue);
            code = ((IConvertible) loopFor.Counter).GetTypeCode();
            if (loopFor.EnumType != null)
            {
                CounterResult = Enum.ToObject(loopFor.EnumType, loopFor.Counter);
            }
            else
            {
                CounterResult = loopFor.Counter;
            }
            if (code != widestType)
            {
                loopFor.Limit = ObjectType.CTypeHelper(loopFor.Limit, code);
                loopFor.StepValue = ObjectType.CTypeHelper(loopFor.StepValue, code);
                return false;
            }
            return CheckContinueLoop(loopFor);
        }

        public static bool ForNextCheckR4(float count, float limit, float StepValue)
        {
            if (StepValue > 0f)
            {
                return (count <= limit);
            }
            return (count >= limit);
        }

        public static bool ForNextCheckR8(double count, double limit, double StepValue)
        {
            if (StepValue > 0.0)
            {
                return (count <= limit);
            }
            return (count >= limit);
        }

        private sealed class ObjectFor
        {
            public object Counter;
            public Type EnumType;
            public object Limit;
            public bool PositiveStep;
            public object StepValue;

            internal ObjectFor()
            {
            }
        }
    }
}

