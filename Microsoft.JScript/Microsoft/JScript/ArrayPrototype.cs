namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Globalization;
    using System.Text;

    public class ArrayPrototype : ArrayObject
    {
        internal static ArrayConstructor _constructor;
        internal static readonly ArrayPrototype ob = new ArrayPrototype(ObjectPrototype.ob);

        internal ArrayPrototype(ObjectPrototype parent) : base(parent)
        {
            base.noExpando = true;
        }

        [JSFunction(JSFunctionAttributeEnum.HasEngine | JSFunctionAttributeEnum.HasVarArgs | JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_concat)]
        public static ArrayObject concat(object thisob, VsaEngine engine, params object[] args)
        {
            ArrayObject obj2 = engine.GetOriginalArrayConstructor().Construct();
            if (thisob is ArrayObject)
            {
                obj2.Concat((ArrayObject) thisob);
            }
            else
            {
                obj2.Concat(thisob);
            }
            for (int i = 0; i < args.Length; i++)
            {
                object obj3 = args[i];
                if (obj3 is ArrayObject)
                {
                    obj2.Concat((ArrayObject) obj3);
                }
                else
                {
                    obj2.Concat(obj3);
                }
            }
            return obj2;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_join)]
        public static string join(object thisob, object separator)
        {
            if (separator is Missing)
            {
                return Join(thisob, ",", false);
            }
            return Join(thisob, Microsoft.JScript.Convert.ToString(separator), false);
        }

        internal static string Join(object thisob, string separator, bool localize)
        {
            StringBuilder builder = new StringBuilder();
            uint num = Microsoft.JScript.Convert.ToUint32(LateBinding.GetMemberValue(thisob, "length"));
            if (num > 0x7fffffff)
            {
                throw new JScriptException(JSError.OutOfMemory);
            }
            if (num > builder.Capacity)
            {
                builder.Capacity = (int) num;
            }
            for (uint i = 0; i < num; i++)
            {
                object valueAtIndex = LateBinding.GetValueAtIndex(thisob, (ulong) i);
                if ((valueAtIndex != null) && !(valueAtIndex is Missing))
                {
                    if (localize)
                    {
                        builder.Append(Microsoft.JScript.Convert.ToLocaleString(valueAtIndex));
                    }
                    else
                    {
                        builder.Append(Microsoft.JScript.Convert.ToString(valueAtIndex));
                    }
                }
                if (i < (num - 1))
                {
                    builder.Append(separator);
                }
            }
            return builder.ToString();
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_pop)]
        public static object pop(object thisob)
        {
            uint num = Microsoft.JScript.Convert.ToUint32(LateBinding.GetMemberValue(thisob, "length"));
            if (num == 0)
            {
                LateBinding.SetMemberValue(thisob, "length", 0);
                return null;
            }
            object valueAtIndex = LateBinding.GetValueAtIndex(thisob, (ulong) (num - 1));
            LateBinding.DeleteValueAtIndex(thisob, (ulong) (num - 1));
            LateBinding.SetMemberValue(thisob, "length", num - 1);
            return valueAtIndex;
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs | JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_push)]
        public static long push(object thisob, params object[] args)
        {
            uint num = Microsoft.JScript.Convert.ToUint32(LateBinding.GetMemberValue(thisob, "length"));
            for (uint i = 0; i < args.Length; i++)
            {
                LateBinding.SetValueAtIndex(thisob, i + num, args[i]);
            }
            long num3 = num + args.Length;
            LateBinding.SetMemberValue(thisob, "length", num3);
            return num3;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_reverse)]
        public static object reverse(object thisob)
        {
            uint num = Microsoft.JScript.Convert.ToUint32(LateBinding.GetMemberValue(thisob, "length"));
            uint num2 = num / 2;
            uint left = 0;
            for (uint i = num - 1; left < num2; i--)
            {
                LateBinding.SwapValues(thisob, left, i);
                left++;
            }
            return thisob;
        }

        internal override void SetMemberValue(string name, object value)
        {
            if (base.noExpando)
            {
                throw new JScriptException(JSError.OLENoPropOrMethod);
            }
            base.SetMemberValue(name, value);
        }

        internal override void SetValueAtIndex(uint index, object value)
        {
            if (base.noExpando)
            {
                throw new JScriptException(JSError.OLENoPropOrMethod);
            }
            base.SetValueAtIndex(index, value);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_shift)]
        public static object shift(object thisob)
        {
            object valueAtIndex = null;
            if (thisob is ArrayObject)
            {
                return ((ArrayObject) thisob).Shift();
            }
            uint num = Microsoft.JScript.Convert.ToUint32(LateBinding.GetMemberValue(thisob, "length"));
            if (num == 0)
            {
                LateBinding.SetMemberValue(thisob, "length", 0);
                return valueAtIndex;
            }
            valueAtIndex = LateBinding.GetValueAtIndex(thisob, 0L);
            for (uint i = 1; i < num; i++)
            {
                object obj3 = LateBinding.GetValueAtIndex(thisob, (ulong) i);
                if (obj3 is Missing)
                {
                    LateBinding.DeleteValueAtIndex(thisob, (ulong) (i - 1));
                }
                else
                {
                    LateBinding.SetValueAtIndex(thisob, (ulong) (i - 1), obj3);
                }
            }
            LateBinding.DeleteValueAtIndex(thisob, (ulong) (num - 1));
            LateBinding.SetMemberValue(thisob, "length", num - 1);
            return valueAtIndex;
        }

        [JSFunction(JSFunctionAttributeEnum.HasEngine | JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_slice)]
        public static ArrayObject slice(object thisob, VsaEngine engine, double start, object end)
        {
            ArrayObject obj2 = engine.GetOriginalArrayConstructor().Construct();
            uint num = Microsoft.JScript.Convert.ToUint32(LateBinding.GetMemberValue(thisob, "length"));
            long num2 = Runtime.DoubleToInt64(Microsoft.JScript.Convert.ToInteger(start));
            if (num2 < 0L)
            {
                num2 = num + num2;
                if (num2 < 0L)
                {
                    num2 = 0L;
                }
            }
            else if (num2 > num)
            {
                num2 = num;
            }
            long num3 = num;
            if ((end != null) && !(end is Missing))
            {
                num3 = Runtime.DoubleToInt64(Microsoft.JScript.Convert.ToInteger(end));
                if (num3 < 0L)
                {
                    num3 = num + num3;
                    if (num3 < 0L)
                    {
                        num3 = 0L;
                    }
                }
                else if (num3 > num)
                {
                    num3 = num;
                }
            }
            if (num3 > num2)
            {
                obj2.length = num3 - num2;
                ulong index = (ulong) num2;
                for (ulong i = 0L; index < num3; i += (ulong) 1L)
                {
                    object valueAtIndex = LateBinding.GetValueAtIndex(thisob, index);
                    if (!(valueAtIndex is Missing))
                    {
                        LateBinding.SetValueAtIndex(obj2, i, valueAtIndex);
                    }
                    index += (ulong) 1L;
                }
            }
            return obj2;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_sort)]
        public static object sort(object thisob, object function)
        {
            ScriptFunction compareFn = null;
            if (function is ScriptFunction)
            {
                compareFn = (ScriptFunction) function;
            }
            uint num = Microsoft.JScript.Convert.ToUint32(LateBinding.GetMemberValue(thisob, "length"));
            if (thisob is ArrayObject)
            {
                ((ArrayObject) thisob).Sort(compareFn);
                return thisob;
            }
            if (num <= 0x7fffffff)
            {
                new QuickSort(thisob, compareFn).SortObject(0L, (long) num);
            }
            return thisob;
        }

        [JSFunction(JSFunctionAttributeEnum.HasEngine | JSFunctionAttributeEnum.HasVarArgs | JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_splice)]
        public static ArrayObject splice(object thisob, VsaEngine engine, double start, double deleteCnt, params object[] args)
        {
            uint oldLength = Microsoft.JScript.Convert.ToUint32(LateBinding.GetMemberValue(thisob, "length"));
            long num2 = Runtime.DoubleToInt64(Microsoft.JScript.Convert.ToInteger(start));
            if (num2 < 0L)
            {
                num2 = oldLength + num2;
                if (num2 < 0L)
                {
                    num2 = 0L;
                }
            }
            else if (num2 > oldLength)
            {
                num2 = oldLength;
            }
            long num3 = Runtime.DoubleToInt64(Microsoft.JScript.Convert.ToInteger(deleteCnt));
            if (num3 < 0L)
            {
                num3 = 0L;
            }
            else if (num3 > (oldLength - num2))
            {
                num3 = oldLength - num2;
            }
            long num4 = (oldLength + args.Length) - num3;
            ArrayObject outArray = engine.GetOriginalArrayConstructor().Construct();
            outArray.length = num3;
            if (thisob is ArrayObject)
            {
                ((ArrayObject) thisob).Splice((uint) num2, (uint) num3, args, outArray, oldLength, (uint) num4);
                return outArray;
            }
            for (ulong i = 0L; i < num3; i += (ulong) 1L)
            {
                outArray.SetValueAtIndex((uint) i, LateBinding.GetValueAtIndex(thisob, i + ((ulong) num2)));
            }
            long num6 = (oldLength - num2) - num3;
            if (num4 < oldLength)
            {
                for (long k = 0L; k < num6; k += 1L)
                {
                    LateBinding.SetValueAtIndex(thisob, (ulong) ((k + num2) + args.Length), LateBinding.GetValueAtIndex(thisob, (ulong) ((k + num2) + num3)));
                }
                LateBinding.SetMemberValue(thisob, "length", num4);
            }
            else
            {
                LateBinding.SetMemberValue(thisob, "length", num4);
                for (long m = num6 - 1L; m >= 0L; m -= 1L)
                {
                    LateBinding.SetValueAtIndex(thisob, (ulong) ((m + num2) + args.Length), LateBinding.GetValueAtIndex(thisob, (ulong) ((m + num2) + num3)));
                }
            }
            int num9 = (args == null) ? 0 : args.Length;
            for (uint j = 0; j < num9; j++)
            {
                LateBinding.SetValueAtIndex(thisob, (ulong) (j + num2), args[j]);
            }
            return outArray;
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_toLocaleString)]
        public static string toLocaleString(object thisob)
        {
            if (!(thisob is ArrayObject))
            {
                throw new JScriptException(JSError.NeedArrayObject);
            }
            StringBuilder builder = new StringBuilder(CultureInfo.CurrentCulture.TextInfo.ListSeparator);
            if (builder[builder.Length - 1] != ' ')
            {
                builder.Append(' ');
            }
            return Join(thisob, builder.ToString(), true);
        }

        [JSFunction(JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_toString)]
        public static string toString(object thisob)
        {
            if (!(thisob is ArrayObject))
            {
                throw new JScriptException(JSError.NeedArrayObject);
            }
            return Join(thisob, ",", false);
        }

        [JSFunction(JSFunctionAttributeEnum.HasVarArgs | JSFunctionAttributeEnum.HasThisObject, JSBuiltin.Array_unshift)]
        public static object unshift(object thisob, params object[] args)
        {
            if ((args != null) && (args.Length != 0))
            {
                if (thisob is ArrayObject)
                {
                    return ((ArrayObject) thisob).Unshift(args);
                }
                uint num = Microsoft.JScript.Convert.ToUint32(LateBinding.GetMemberValue(thisob, "length"));
                long num2 = num + args.Length;
                LateBinding.SetMemberValue(thisob, "length", num2);
                for (long i = num - 1; i >= 0L; i -= 1L)
                {
                    object valueAtIndex = LateBinding.GetValueAtIndex(thisob, (ulong) i);
                    if (valueAtIndex is Missing)
                    {
                        LateBinding.DeleteValueAtIndex(thisob, (ulong) (i + args.Length));
                    }
                    else
                    {
                        LateBinding.SetValueAtIndex(thisob, (ulong) (i + args.Length), valueAtIndex);
                    }
                }
                for (uint j = 0; j < args.Length; j++)
                {
                    LateBinding.SetValueAtIndex(thisob, (ulong) j, args[j]);
                }
            }
            return thisob;
        }

        public static ArrayConstructor constructor
        {
            get
            {
                return _constructor;
            }
        }
    }
}

