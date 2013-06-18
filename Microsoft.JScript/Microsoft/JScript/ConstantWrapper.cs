namespace Microsoft.JScript
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;

    internal class ConstantWrapper : AST
    {
        internal bool isNumericLiteral;
        internal object value;

        internal ConstantWrapper(object value, Context context) : base(context)
        {
            if (value is ConcatString)
            {
                value = value.ToString();
            }
            this.value = value;
            this.isNumericLiteral = false;
        }

        internal override object Evaluate()
        {
            return this.value;
        }

        internal override IReflect InferType(JSField inference_target)
        {
            if ((this.value == null) || (this.value is DBNull))
            {
                return Typeob.Object;
            }
            if ((this.value is ClassScope) || (this.value is TypedArray))
            {
                return Typeob.Type;
            }
            if (this.value is EnumWrapper)
            {
                return ((EnumWrapper) this.value).classScopeOrType;
            }
            return Globals.TypeRefs.ToReferenceContext(this.value.GetType());
        }

        internal bool IsAssignableTo(Type rtype)
        {
            try
            {
                Microsoft.JScript.Convert.CoerceT(this.value, rtype, false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal override AST PartiallyEvaluate()
        {
            return this;
        }

        public override string ToString()
        {
            return this.value.ToString();
        }

        internal override void TranslateToIL(ILGenerator il, Type rtype)
        {
            if (rtype != Typeob.Void)
            {
                object code = this.value;
                if (((code is EnumWrapper) && (rtype != Typeob.Object)) && (rtype != Typeob.String))
                {
                    code = ((EnumWrapper) code).value;
                }
                if (this.isNumericLiteral && (((rtype == Typeob.Decimal) || (rtype == Typeob.Int64)) || ((rtype == Typeob.UInt64) || (rtype == Typeob.Single))))
                {
                    code = base.context.GetCode();
                }
                if (!(rtype is TypeBuilder))
                {
                    try
                    {
                        code = Microsoft.JScript.Convert.CoerceT(code, rtype);
                    }
                    catch
                    {
                    }
                }
                this.TranslateToIL(il, code, rtype);
            }
        }

        private void TranslateToIL(ILGenerator il, object val, Type rtype)
        {
            long ticks;
            IConvertible iConvertible = Microsoft.JScript.Convert.GetIConvertible(val);
            switch (Microsoft.JScript.Convert.GetTypeCode(val, iConvertible))
            {
                case TypeCode.Empty:
                    il.Emit(OpCodes.Ldnull);
                    if (rtype.IsValueType)
                    {
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, rtype);
                    }
                    return;

                case TypeCode.DBNull:
                    il.Emit(OpCodes.Ldsfld, Typeob.Null.GetField("Value"));
                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Null, rtype);
                    return;

                case TypeCode.Boolean:
                    TranslateToILInt(il, iConvertible.ToInt32(null));
                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Boolean, rtype);
                    return;

                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                    TranslateToILInt(il, iConvertible.ToInt32(null));
                    if (!rtype.IsEnum)
                    {
                        if (val is EnumWrapper)
                        {
                            Microsoft.JScript.Convert.Emit(this, il, ((EnumWrapper) val).type, rtype);
                            return;
                        }
                        Microsoft.JScript.Convert.Emit(this, il, Globals.TypeRefs.ToReferenceContext(val.GetType()), rtype);
                        return;
                    }
                    return;

                case TypeCode.UInt32:
                    TranslateToILInt(il, (int) iConvertible.ToUInt32(null));
                    if (!rtype.IsEnum)
                    {
                        if (val is EnumWrapper)
                        {
                            Microsoft.JScript.Convert.Emit(this, il, ((EnumWrapper) val).type, rtype);
                            return;
                        }
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.UInt32, rtype);
                        return;
                    }
                    return;

                case TypeCode.Int64:
                    ticks = iConvertible.ToInt64(null);
                    if ((-2147483648L > ticks) || (ticks > 0x7fffffffL))
                    {
                        il.Emit(OpCodes.Ldc_I8, ticks);
                        break;
                    }
                    TranslateToILInt(il, (int) ticks);
                    il.Emit(OpCodes.Conv_I8);
                    break;

                case TypeCode.UInt64:
                {
                    ulong num2 = iConvertible.ToUInt64(null);
                    if (num2 > 0x7fffffffL)
                    {
                        il.Emit(OpCodes.Ldc_I8, (long) num2);
                    }
                    else
                    {
                        TranslateToILInt(il, (int) num2);
                        il.Emit(OpCodes.Conv_I8);
                    }
                    if (!rtype.IsEnum)
                    {
                        if (val is EnumWrapper)
                        {
                            Microsoft.JScript.Convert.Emit(this, il, ((EnumWrapper) val).type, rtype);
                            return;
                        }
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.UInt64, rtype);
                    }
                    return;
                }
                case TypeCode.Single:
                {
                    float arg = iConvertible.ToSingle(null);
                    if ((arg != arg) || ((arg == 0f) && float.IsNegativeInfinity(1f / arg)))
                    {
                        il.Emit(OpCodes.Ldc_R4, arg);
                    }
                    else
                    {
                        int i = (int) Runtime.DoubleToInt64((double) arg);
                        if (((-128 > i) || (i > 0x7f)) || (arg != i))
                        {
                            il.Emit(OpCodes.Ldc_R4, arg);
                        }
                        else
                        {
                            TranslateToILInt(il, i);
                            il.Emit(OpCodes.Conv_R4);
                        }
                    }
                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Single, rtype);
                    return;
                }
                case TypeCode.Double:
                {
                    double num5 = iConvertible.ToDouble(null);
                    if ((num5 != num5) || ((num5 == 0.0) && double.IsNegativeInfinity(1.0 / num5)))
                    {
                        il.Emit(OpCodes.Ldc_R8, num5);
                    }
                    else
                    {
                        int num6 = (int) Runtime.DoubleToInt64(num5);
                        if (((-128 > num6) || (num6 > 0x7f)) || (num5 != num6))
                        {
                            il.Emit(OpCodes.Ldc_R8, num5);
                        }
                        else
                        {
                            TranslateToILInt(il, num6);
                            il.Emit(OpCodes.Conv_R8);
                        }
                    }
                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Double, rtype);
                    return;
                }
                case TypeCode.Decimal:
                {
                    int[] bits = decimal.GetBits(iConvertible.ToDecimal(null));
                    TranslateToILInt(il, bits[0]);
                    TranslateToILInt(il, bits[1]);
                    TranslateToILInt(il, bits[2]);
                    il.Emit((bits[3] < 0) ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                    TranslateToILInt(il, (bits[3] & 0x7fffffff) >> 0x10);
                    il.Emit(OpCodes.Newobj, CompilerGlobals.decimalConstructor);
                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Decimal, rtype);
                    return;
                }
                case TypeCode.DateTime:
                    ticks = iConvertible.ToDateTime(null).Ticks;
                    il.Emit(OpCodes.Ldc_I8, ticks);
                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Int64, rtype);
                    return;

                case TypeCode.String:
                {
                    string str = iConvertible.ToString(null);
                    if (!(rtype == Typeob.Char) || (str.Length != 1))
                    {
                        il.Emit(OpCodes.Ldstr, str);
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.String, rtype);
                        return;
                    }
                    TranslateToILInt(il, str[0]);
                    return;
                }
                default:
                    if (val is Enum)
                    {
                        if (rtype == Typeob.String)
                        {
                            this.TranslateToIL(il, val.ToString(), rtype);
                            return;
                        }
                        if (rtype.IsPrimitive)
                        {
                            this.TranslateToIL(il, System.Convert.ChangeType(val, Enum.GetUnderlyingType(Globals.TypeRefs.ToReferenceContext(val.GetType())), CultureInfo.InvariantCulture), rtype);
                            return;
                        }
                        Type enumType = Globals.TypeRefs.ToReferenceContext(val.GetType());
                        Type underlyingType = Enum.GetUnderlyingType(enumType);
                        this.TranslateToIL(il, System.Convert.ChangeType(val, underlyingType, CultureInfo.InvariantCulture), underlyingType);
                        il.Emit(OpCodes.Box, enumType);
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, rtype);
                    }
                    else if (val is EnumWrapper)
                    {
                        if (rtype == Typeob.String)
                        {
                            this.TranslateToIL(il, val.ToString(), rtype);
                            return;
                        }
                        if (rtype.IsPrimitive)
                        {
                            this.TranslateToIL(il, ((EnumWrapper) val).ToNumericValue(), rtype);
                        }
                        else
                        {
                            Type type = ((EnumWrapper) val).type;
                            Type type4 = Globals.TypeRefs.ToReferenceContext(((EnumWrapper) val).value.GetType());
                            this.TranslateToIL(il, ((EnumWrapper) val).value, type4);
                            il.Emit(OpCodes.Box, type);
                            Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, rtype);
                        }
                    }
                    else if (val is Type)
                    {
                        il.Emit(OpCodes.Ldtoken, (Type) val);
                        il.Emit(OpCodes.Call, CompilerGlobals.getTypeFromHandleMethod);
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.Type, rtype);
                    }
                    else if (val is Namespace)
                    {
                        il.Emit(OpCodes.Ldstr, ((Namespace) val).Name);
                        base.EmitILToLoadEngine(il);
                        il.Emit(OpCodes.Call, CompilerGlobals.getNamespaceMethod);
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.Namespace, rtype);
                    }
                    else if (val is ClassScope)
                    {
                        il.Emit(OpCodes.Ldtoken, ((ClassScope) val).GetTypeBuilderOrEnumBuilder());
                        il.Emit(OpCodes.Call, CompilerGlobals.getTypeFromHandleMethod);
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.Type, rtype);
                    }
                    else if (val is TypedArray)
                    {
                        il.Emit(OpCodes.Ldtoken, Microsoft.JScript.Convert.ToType((TypedArray) val));
                        il.Emit(OpCodes.Call, CompilerGlobals.getTypeFromHandleMethod);
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.Type, rtype);
                    }
                    else if (val is NumberObject)
                    {
                        this.TranslateToIL(il, ((NumberObject) val).value, Typeob.Object);
                        base.EmitILToLoadEngine(il);
                        il.Emit(OpCodes.Call, CompilerGlobals.toObjectMethod);
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.NumberObject, rtype);
                    }
                    else if (val is StringObject)
                    {
                        il.Emit(OpCodes.Ldstr, ((StringObject) val).value);
                        base.EmitILToLoadEngine(il);
                        il.Emit(OpCodes.Call, CompilerGlobals.toObjectMethod);
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.StringObject, rtype);
                    }
                    else if (val is BooleanObject)
                    {
                        il.Emit(((BooleanObject) val).value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Box, Typeob.Boolean);
                        base.EmitILToLoadEngine(il);
                        il.Emit(OpCodes.Call, CompilerGlobals.toObjectMethod);
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.BooleanObject, rtype);
                    }
                    else if (val is ActiveXObjectConstructor)
                    {
                        il.Emit(OpCodes.Call, Typeob.GlobalObject.GetProperty("ActiveXObject").GetGetMethod());
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.ScriptFunction, rtype);
                    }
                    else if (val is ArrayConstructor)
                    {
                        il.Emit(OpCodes.Call, Typeob.GlobalObject.GetProperty("Array").GetGetMethod());
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.ScriptFunction, rtype);
                    }
                    else if (val is BooleanConstructor)
                    {
                        il.Emit(OpCodes.Call, Typeob.GlobalObject.GetProperty("Boolean").GetGetMethod());
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.ScriptFunction, rtype);
                    }
                    else if (val is DateConstructor)
                    {
                        il.Emit(OpCodes.Call, Typeob.GlobalObject.GetProperty("Date").GetGetMethod());
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.ScriptFunction, rtype);
                    }
                    else if (val is EnumeratorConstructor)
                    {
                        il.Emit(OpCodes.Call, Typeob.GlobalObject.GetProperty("Enumerator").GetGetMethod());
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.ScriptFunction, rtype);
                    }
                    else if (val is ErrorConstructor)
                    {
                        ErrorConstructor constructor = (ErrorConstructor) val;
                        if (constructor == ErrorConstructor.evalOb)
                        {
                            il.Emit(OpCodes.Call, Typeob.GlobalObject.GetProperty("EvalError").GetGetMethod());
                        }
                        else if (constructor == ErrorConstructor.rangeOb)
                        {
                            il.Emit(OpCodes.Call, Typeob.GlobalObject.GetProperty("RangeError").GetGetMethod());
                        }
                        else if (constructor == ErrorConstructor.referenceOb)
                        {
                            il.Emit(OpCodes.Call, Typeob.GlobalObject.GetProperty("ReferenceError").GetGetMethod());
                        }
                        else if (constructor == ErrorConstructor.syntaxOb)
                        {
                            il.Emit(OpCodes.Call, Typeob.GlobalObject.GetProperty("SyntaxError").GetGetMethod());
                        }
                        else if (constructor == ErrorConstructor.typeOb)
                        {
                            il.Emit(OpCodes.Call, Typeob.GlobalObject.GetProperty("TypeError").GetGetMethod());
                        }
                        else if (constructor == ErrorConstructor.uriOb)
                        {
                            il.Emit(OpCodes.Call, Typeob.GlobalObject.GetProperty("URIError").GetGetMethod());
                        }
                        else
                        {
                            il.Emit(OpCodes.Call, Typeob.GlobalObject.GetProperty("Error").GetGetMethod());
                        }
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.ScriptFunction, rtype);
                    }
                    else if (val is FunctionConstructor)
                    {
                        il.Emit(OpCodes.Call, Typeob.GlobalObject.GetProperty("Function").GetGetMethod());
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.ScriptFunction, rtype);
                    }
                    else if (val is MathObject)
                    {
                        il.Emit(OpCodes.Call, Typeob.GlobalObject.GetProperty("Math").GetGetMethod());
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.JSObject, rtype);
                    }
                    else if (val is NumberConstructor)
                    {
                        il.Emit(OpCodes.Call, Typeob.GlobalObject.GetProperty("Number").GetGetMethod());
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.ScriptFunction, rtype);
                    }
                    else if (val is ObjectConstructor)
                    {
                        il.Emit(OpCodes.Call, Typeob.GlobalObject.GetProperty("Object").GetGetMethod());
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.ScriptFunction, rtype);
                    }
                    else if (val is RegExpConstructor)
                    {
                        il.Emit(OpCodes.Call, Typeob.GlobalObject.GetProperty("RegExp").GetGetMethod());
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.ScriptFunction, rtype);
                    }
                    else if (val is StringConstructor)
                    {
                        il.Emit(OpCodes.Call, Typeob.GlobalObject.GetProperty("String").GetGetMethod());
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.ScriptFunction, rtype);
                    }
                    else if (val is VBArrayConstructor)
                    {
                        il.Emit(OpCodes.Call, Typeob.GlobalObject.GetProperty("VBArray").GetGetMethod());
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.ScriptFunction, rtype);
                    }
                    else if (val is IntPtr)
                    {
                        il.Emit(OpCodes.Ldc_I8, (long) ((IntPtr) val));
                        il.Emit(OpCodes.Conv_I);
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.IntPtr, rtype);
                    }
                    else if (val is UIntPtr)
                    {
                        il.Emit(OpCodes.Ldc_I8, (long) ((ulong) ((UIntPtr) val)));
                        il.Emit(OpCodes.Conv_U);
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.UIntPtr, rtype);
                    }
                    else if (val is Microsoft.JScript.Missing)
                    {
                        il.Emit(OpCodes.Ldsfld, CompilerGlobals.missingField);
                        Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, rtype);
                    }
                    else if (val is System.Reflection.Missing)
                    {
                        if (rtype.IsPrimitive)
                        {
                            this.TranslateToIL(il, (double) 1.0 / (double) 0.0, rtype);
                        }
                        else if ((rtype != Typeob.Object) && !rtype.IsValueType)
                        {
                            il.Emit(OpCodes.Ldnull);
                        }
                        else
                        {
                            il.Emit(OpCodes.Ldsfld, CompilerGlobals.systemReflectionMissingField);
                            Microsoft.JScript.Convert.Emit(this, il, Typeob.Object, rtype);
                        }
                    }
                    else
                    {
                        if (val == this.value)
                        {
                            throw new JScriptException(JSError.InternalError, base.context);
                        }
                        this.TranslateToIL(il, this.value, rtype);
                    }
                    return;
            }
            if (!rtype.IsEnum)
            {
                if (val is EnumWrapper)
                {
                    Microsoft.JScript.Convert.Emit(this, il, ((EnumWrapper) val).type, rtype);
                }
                else
                {
                    Microsoft.JScript.Convert.Emit(this, il, Typeob.Int64, rtype);
                }
            }
        }

        internal override void TranslateToILInitializer(ILGenerator il)
        {
        }

        internal static void TranslateToILInt(ILGenerator il, int i)
        {
            switch (i)
            {
                case -1:
                    il.Emit(OpCodes.Ldc_I4_M1);
                    return;

                case 0:
                    il.Emit(OpCodes.Ldc_I4_0);
                    return;

                case 1:
                    il.Emit(OpCodes.Ldc_I4_1);
                    return;

                case 2:
                    il.Emit(OpCodes.Ldc_I4_2);
                    return;

                case 3:
                    il.Emit(OpCodes.Ldc_I4_3);
                    return;

                case 4:
                    il.Emit(OpCodes.Ldc_I4_4);
                    return;

                case 5:
                    il.Emit(OpCodes.Ldc_I4_5);
                    return;

                case 6:
                    il.Emit(OpCodes.Ldc_I4_6);
                    return;

                case 7:
                    il.Emit(OpCodes.Ldc_I4_7);
                    return;

                case 8:
                    il.Emit(OpCodes.Ldc_I4_8);
                    return;
            }
            if ((-128 <= i) && (i <= 0x7f))
            {
                il.Emit(OpCodes.Ldc_I4_S, (sbyte) i);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4, i);
            }
        }
    }
}

