namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    [SecurityCritical(SecurityCriticalScope.Everything)]
    internal class CodeGenerator
    {
        private ArrayList argList;
        private Stack blockStack;
        private static MethodInfo boxPointer;
        private CodeGenTrace codeGenTrace;
        private Type delegateType;
        private DynamicMethod dynamicMethod;
        private static MethodInfo getTypeFromHandle;
        private ILGenerator ilGen;
        private int lineNo = 1;
        private Hashtable localNames;
        private Label methodEndLabel;
        private static MethodInfo objectToString;
        private static Module SerializationModule = typeof(CodeGenerator).Module;
        private static MethodInfo stringConcat2;
        private static MethodInfo unboxPointer;

        internal CodeGenerator()
        {
            SourceSwitch codeGenerationSwitch = OperationInvokerTrace.CodeGenerationSwitch;
            if ((codeGenerationSwitch.Level & SourceLevels.Verbose) == SourceLevels.Verbose)
            {
                this.codeGenTrace = CodeGenTrace.Tron;
            }
            else if ((codeGenerationSwitch.Level & SourceLevels.Information) == SourceLevels.Information)
            {
                this.codeGenTrace = CodeGenTrace.Save;
            }
            else
            {
                this.codeGenTrace = CodeGenTrace.None;
            }
        }

        internal void BeginMethod(string methodName, Type delegateType, bool allowPrivateMemberAccess)
        {
            MethodInfo method = delegateType.GetMethod("Invoke");
            ParameterInfo[] parameters = method.GetParameters();
            Type[] argTypes = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                argTypes[i] = parameters[i].ParameterType;
            }
            this.BeginMethod(method.ReturnType, methodName, argTypes, allowPrivateMemberAccess);
            this.delegateType = delegateType;
        }

        private void BeginMethod(Type returnType, string methodName, Type[] argTypes, bool allowPrivateMemberAccess)
        {
            this.dynamicMethod = new DynamicMethod(methodName, returnType, argTypes, SerializationModule, allowPrivateMemberAccess);
            this.ilGen = this.dynamicMethod.GetILGenerator();
            this.methodEndLabel = this.ilGen.DefineLabel();
            this.blockStack = new Stack();
            this.argList = new ArrayList();
            for (int i = 0; i < argTypes.Length; i++)
            {
                this.argList.Add(new ArgBuilder(i, argTypes[i]));
            }
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceLabel("Begin method " + methodName + " {");
            }
        }

        internal void Box(Type type)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Box " + type);
            }
            this.ilGen.Emit(OpCodes.Box, type);
        }

        internal void Br(Label label)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Br " + label.GetHashCode());
            }
            this.ilGen.Emit(OpCodes.Br, label);
        }

        internal void Brfalse(Label label)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Brfalse " + label.GetHashCode());
            }
            this.ilGen.Emit(OpCodes.Brfalse, label);
        }

        internal void Brtrue(Label label)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Brtrue " + label.GetHashCode());
            }
            this.ilGen.Emit(OpCodes.Brtrue, label);
        }

        internal void Call(MethodInfo methodInfo)
        {
            if (methodInfo.IsVirtual)
            {
                if (this.codeGenTrace != CodeGenTrace.None)
                {
                    this.EmitSourceInstruction("Callvirt " + methodInfo.ToString() + " on type " + methodInfo.DeclaringType.ToString());
                }
                this.ilGen.Emit(OpCodes.Callvirt, methodInfo);
            }
            else if (methodInfo.IsStatic)
            {
                if (this.codeGenTrace != CodeGenTrace.None)
                {
                    this.EmitSourceInstruction("Static Call " + methodInfo.ToString() + " on type " + methodInfo.DeclaringType.ToString());
                }
                this.ilGen.Emit(OpCodes.Call, methodInfo);
            }
            else
            {
                if (this.codeGenTrace != CodeGenTrace.None)
                {
                    this.EmitSourceInstruction("Call " + methodInfo.ToString() + " on type " + methodInfo.DeclaringType.ToString());
                }
                this.ilGen.Emit(OpCodes.Call, methodInfo);
            }
        }

        internal void Castclass(Type target)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Castclass " + target);
            }
            this.ilGen.Emit(OpCodes.Castclass, target);
        }

        internal void Concat2()
        {
            this.Call(StringConcat2);
        }

        internal void ConvertAddress(Type source, Type target)
        {
            this.InternalConvert(source, target, true);
        }

        internal void ConvertValue(Type source, Type target)
        {
            this.InternalConvert(source, target, false);
        }

        internal LocalBuilder DeclareLocal(Type type, string name)
        {
            return this.DeclareLocal(type, name, false);
        }

        internal LocalBuilder DeclareLocal(Type type, string name, bool isPinned)
        {
            LocalBuilder builder = this.ilGen.DeclareLocal(type, isPinned);
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.LocalNames[builder] = name;
                this.EmitSourceComment(string.Concat(new object[] { "Declare local '", name, "' of type ", type }));
            }
            return builder;
        }

        internal Label DefineLabel()
        {
            return this.ilGen.DefineLabel();
        }

        internal void Dup()
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Dup");
            }
            this.ilGen.Emit(OpCodes.Dup);
        }

        internal void Else()
        {
            IfState state = this.PopIfState();
            this.Br(state.EndIf);
            this.MarkLabel(state.ElseBegin);
            state.ElseBegin = state.EndIf;
            this.blockStack.Push(state);
        }

        internal void EmitSourceComment(string comment)
        {
            this.EmitSourceInstruction("// " + comment);
        }

        internal void EmitSourceInstruction(string line)
        {
            this.EmitSourceLine("    " + line);
        }

        internal void EmitSourceLabel(string line)
        {
            this.EmitSourceLine(line);
        }

        internal void EmitSourceLine(string line)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                OperationInvokerTrace.WriteInstruction(this.lineNo++, line);
            }
            if ((this.ilGen != null) && (this.codeGenTrace == CodeGenTrace.Tron))
            {
                this.ilGen.Emit(OpCodes.Ldstr, string.Format(CultureInfo.InvariantCulture, "{0:00000}: {1}", new object[] { this.lineNo - 1, line }));
                this.ilGen.Emit(OpCodes.Call, OperationInvokerTrace.TraceInstructionMethod);
            }
        }

        internal void EmitStackTop(Type stackTopType)
        {
            if (this.codeGenTrace == CodeGenTrace.Tron)
            {
                this.codeGenTrace = CodeGenTrace.None;
                this.Dup();
                this.ToString(stackTopType);
                LocalBuilder var = this.DeclareLocal(typeof(string), "topValue");
                this.Store(var);
                this.Load("//value = ");
                this.Load(var);
                this.Concat2();
                this.Call(OperationInvokerTrace.TraceInstructionMethod);
                this.codeGenTrace = CodeGenTrace.Tron;
            }
        }

        internal void EndIf()
        {
            IfState state = this.PopIfState();
            if (!state.ElseBegin.Equals(state.EndIf))
            {
                this.MarkLabel(state.ElseBegin);
            }
            this.MarkLabel(state.EndIf);
        }

        internal Delegate EndMethod()
        {
            this.MarkLabel(this.methodEndLabel);
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceLabel("} End method");
            }
            this.Ret();
            Delegate delegate2 = null;
            delegate2 = this.dynamicMethod.CreateDelegate(this.delegateType);
            this.dynamicMethod = null;
            this.delegateType = null;
            this.ilGen = null;
            this.blockStack = null;
            this.argList = null;
            return delegate2;
        }

        internal ArgBuilder GetArg(int index)
        {
            return (ArgBuilder) this.argList[index];
        }

        private OpCode GetConvOpCode(TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return OpCodes.Conv_I1;

                case TypeCode.Char:
                    return OpCodes.Conv_I2;

                case TypeCode.SByte:
                    return OpCodes.Conv_I1;

                case TypeCode.Byte:
                    return OpCodes.Conv_U1;

                case TypeCode.Int16:
                    return OpCodes.Conv_I2;

                case TypeCode.UInt16:
                    return OpCodes.Conv_U2;

                case TypeCode.Int32:
                    return OpCodes.Conv_I4;

                case TypeCode.UInt32:
                    return OpCodes.Conv_U4;

                case TypeCode.Int64:
                    return OpCodes.Conv_I8;

                case TypeCode.UInt64:
                    return OpCodes.Conv_I8;

                case TypeCode.Single:
                    return OpCodes.Conv_R4;

                case TypeCode.Double:
                    return OpCodes.Conv_R8;
            }
            return OpCodes.Nop;
        }

        private OpCode GetLdelemOpCode(TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.Object:
                    return OpCodes.Ldelem_Ref;

                case TypeCode.Boolean:
                    return OpCodes.Ldelem_I1;

                case TypeCode.Char:
                    return OpCodes.Ldelem_I2;

                case TypeCode.SByte:
                    return OpCodes.Ldelem_I1;

                case TypeCode.Byte:
                    return OpCodes.Ldelem_U1;

                case TypeCode.Int16:
                    return OpCodes.Ldelem_I2;

                case TypeCode.UInt16:
                    return OpCodes.Ldelem_U2;

                case TypeCode.Int32:
                    return OpCodes.Ldelem_I4;

                case TypeCode.UInt32:
                    return OpCodes.Ldelem_U4;

                case TypeCode.Int64:
                    return OpCodes.Ldelem_I8;

                case TypeCode.UInt64:
                    return OpCodes.Ldelem_I8;

                case TypeCode.Single:
                    return OpCodes.Ldelem_R4;

                case TypeCode.Double:
                    return OpCodes.Ldelem_R8;

                case TypeCode.String:
                    return OpCodes.Ldelem_Ref;
            }
            return OpCodes.Nop;
        }

        private OpCode GetLdindOpCode(TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return OpCodes.Ldind_I1;

                case TypeCode.Char:
                    return OpCodes.Ldind_I2;

                case TypeCode.SByte:
                    return OpCodes.Ldind_I1;

                case TypeCode.Byte:
                    return OpCodes.Ldind_U1;

                case TypeCode.Int16:
                    return OpCodes.Ldind_I2;

                case TypeCode.UInt16:
                    return OpCodes.Ldind_U2;

                case TypeCode.Int32:
                    return OpCodes.Ldind_I4;

                case TypeCode.UInt32:
                    return OpCodes.Ldind_U4;

                case TypeCode.Int64:
                    return OpCodes.Ldind_I8;

                case TypeCode.UInt64:
                    return OpCodes.Ldind_I8;

                case TypeCode.Single:
                    return OpCodes.Ldind_R4;

                case TypeCode.Double:
                    return OpCodes.Ldind_R8;

                case TypeCode.String:
                    return OpCodes.Ldind_Ref;
            }
            return OpCodes.Nop;
        }

        private OpCode GetStelemOpCode(TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.Object:
                    return OpCodes.Stelem_Ref;

                case TypeCode.Boolean:
                    return OpCodes.Stelem_I1;

                case TypeCode.Char:
                    return OpCodes.Stelem_I2;

                case TypeCode.SByte:
                    return OpCodes.Stelem_I1;

                case TypeCode.Byte:
                    return OpCodes.Stelem_I1;

                case TypeCode.Int16:
                    return OpCodes.Stelem_I2;

                case TypeCode.UInt16:
                    return OpCodes.Stelem_I2;

                case TypeCode.Int32:
                    return OpCodes.Stelem_I4;

                case TypeCode.UInt32:
                    return OpCodes.Stelem_I4;

                case TypeCode.Int64:
                    return OpCodes.Stelem_I8;

                case TypeCode.UInt64:
                    return OpCodes.Stelem_I8;

                case TypeCode.Single:
                    return OpCodes.Stelem_R4;

                case TypeCode.Double:
                    return OpCodes.Stelem_R8;

                case TypeCode.String:
                    return OpCodes.Stelem_Ref;
            }
            return OpCodes.Nop;
        }

        internal Type GetVariableType(object var)
        {
            if (var is ArgBuilder)
            {
                return ((ArgBuilder) var).ArgType;
            }
            if (var is LocalBuilder)
            {
                return ((LocalBuilder) var).LocalType;
            }
            return var.GetType();
        }

        internal void If()
        {
            this.InternalIf(false);
        }

        internal void IfNot()
        {
            this.InternalIf(true);
        }

        internal void InitObj(Type valueType)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Initobj " + valueType);
            }
            this.ilGen.Emit(OpCodes.Initobj, valueType);
        }

        private void InternalConvert(Type source, Type target, bool isAddress)
        {
            if (target != source)
            {
                if (target.IsValueType)
                {
                    if (source.IsValueType)
                    {
                        OpCode convOpCode = this.GetConvOpCode(Type.GetTypeCode(target));
                        if (convOpCode.Equals(OpCodes.Nop))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxCodeGenNoConversionPossibleTo", new object[] { target.FullName })));
                        }
                        if (this.codeGenTrace != CodeGenTrace.None)
                        {
                            this.EmitSourceInstruction(convOpCode.ToString());
                        }
                        this.ilGen.Emit(convOpCode);
                    }
                    else
                    {
                        if (!source.IsAssignableFrom(target))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxCodeGenIsNotAssignableFrom", new object[] { target.FullName, source.FullName })));
                        }
                        this.Unbox(target);
                        if (!isAddress)
                        {
                            this.Ldobj(target);
                        }
                    }
                }
                else if (target.IsPointer)
                {
                    this.Call(UnboxPointer);
                }
                else if (source.IsPointer)
                {
                    this.Load(source);
                    this.Call(BoxPointer);
                }
                else if (target.IsAssignableFrom(source))
                {
                    if (source.IsValueType)
                    {
                        if (isAddress)
                        {
                            this.Ldobj(source);
                        }
                        this.Box(source);
                    }
                }
                else if (source.IsAssignableFrom(target))
                {
                    this.Castclass(target);
                }
                else
                {
                    if (!target.IsInterface && !source.IsInterface)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxCodeGenIsNotAssignableFrom", new object[] { target.FullName, source.FullName })));
                    }
                    this.Castclass(target);
                }
            }
        }

        private void InternalIf(bool negate)
        {
            IfState state = new IfState {
                EndIf = this.DefineLabel(),
                ElseBegin = this.DefineLabel()
            };
            if (negate)
            {
                this.Brtrue(state.ElseBegin);
            }
            else
            {
                this.Brfalse(state.ElseBegin);
            }
            this.blockStack.Push(state);
        }

        private static bool IsStruct(Type objType)
        {
            return (objType.IsValueType && !objType.IsPrimitive);
        }

        internal void Ldarg(int slot)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Ldarg " + slot);
            }
            switch (slot)
            {
                case 0:
                    this.ilGen.Emit(OpCodes.Ldarg_0);
                    return;

                case 1:
                    this.ilGen.Emit(OpCodes.Ldarg_1);
                    return;

                case 2:
                    this.ilGen.Emit(OpCodes.Ldarg_2);
                    return;

                case 3:
                    this.ilGen.Emit(OpCodes.Ldarg_3);
                    return;
            }
            if (slot <= 0xff)
            {
                this.ilGen.Emit(OpCodes.Ldarg_S, slot);
            }
            else
            {
                this.ilGen.Emit(OpCodes.Ldarg, slot);
            }
        }

        internal void Ldarg(ArgBuilder arg)
        {
            this.Ldarg(arg.Index);
        }

        internal void Ldarga(int slot)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Ldarga " + slot);
            }
            if (slot <= 0xff)
            {
                this.ilGen.Emit(OpCodes.Ldarga_S, slot);
            }
            else
            {
                this.ilGen.Emit(OpCodes.Ldarga, slot);
            }
        }

        internal void Ldarga(ArgBuilder argBuilder)
        {
            this.Ldarga(argBuilder.Index);
        }

        internal void LdargAddress(ArgBuilder argBuilder)
        {
            if (argBuilder.ArgType.IsValueType)
            {
                this.Ldarga(argBuilder);
            }
            else
            {
                this.Ldarg(argBuilder);
            }
        }

        internal void Ldc(bool boolVar)
        {
            if (boolVar)
            {
                if (this.codeGenTrace != CodeGenTrace.None)
                {
                    this.EmitSourceInstruction("Ldc.i4 1");
                }
                this.ilGen.Emit(OpCodes.Ldc_I4_1);
            }
            else
            {
                if (this.codeGenTrace != CodeGenTrace.None)
                {
                    this.EmitSourceInstruction("Ldc.i4 0");
                }
                this.ilGen.Emit(OpCodes.Ldc_I4_0);
            }
        }

        internal void Ldc(int intVar)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Ldc.i4 " + intVar);
            }
            switch (intVar)
            {
                case -1:
                    this.ilGen.Emit(OpCodes.Ldc_I4_M1);
                    return;

                case 0:
                    this.ilGen.Emit(OpCodes.Ldc_I4_0);
                    return;

                case 1:
                    this.ilGen.Emit(OpCodes.Ldc_I4_1);
                    return;

                case 2:
                    this.ilGen.Emit(OpCodes.Ldc_I4_2);
                    return;

                case 3:
                    this.ilGen.Emit(OpCodes.Ldc_I4_3);
                    return;

                case 4:
                    this.ilGen.Emit(OpCodes.Ldc_I4_4);
                    return;

                case 5:
                    this.ilGen.Emit(OpCodes.Ldc_I4_5);
                    return;

                case 6:
                    this.ilGen.Emit(OpCodes.Ldc_I4_6);
                    return;

                case 7:
                    this.ilGen.Emit(OpCodes.Ldc_I4_7);
                    return;

                case 8:
                    this.ilGen.Emit(OpCodes.Ldc_I4_8);
                    return;
            }
            this.ilGen.Emit(OpCodes.Ldc_I4, intVar);
        }

        internal void Ldc(object o)
        {
            Type enumType = o.GetType();
            if (o is Type)
            {
                this.Ldtoken((Type) o);
                this.Call(GetTypeFromHandle);
            }
            else if (enumType.IsEnum)
            {
                if (this.codeGenTrace != CodeGenTrace.None)
                {
                    this.EmitSourceComment(string.Concat(new object[] { "Ldc ", o.GetType(), ".", o }));
                }
                this.Ldc(((IConvertible) o).ToType(Enum.GetUnderlyingType(enumType), null));
            }
            else
            {
                switch (Type.GetTypeCode(enumType))
                {
                    case TypeCode.Boolean:
                        this.Ldc((bool) o);
                        return;

                    case TypeCode.Char:
                        this.Ldc((int) ((char) o));
                        return;

                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                        this.Ldc(((IConvertible) o).ToInt32(CultureInfo.InvariantCulture));
                        return;

                    case TypeCode.Int32:
                        this.Ldc((int) o);
                        return;

                    case TypeCode.UInt32:
                        this.Ldc((int) ((uint) o));
                        return;

                    case TypeCode.String:
                        this.Ldstr((string) o);
                        return;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxCodeGenUnknownConstantType", new object[] { enumType.FullName })));
            }
        }

        internal void Ldelem(Type arrayElementType)
        {
            if (arrayElementType.IsEnum)
            {
                this.Ldelem(Enum.GetUnderlyingType(arrayElementType));
            }
            else
            {
                OpCode ldelemOpCode = this.GetLdelemOpCode(Type.GetTypeCode(arrayElementType));
                if (ldelemOpCode.Equals(OpCodes.Nop))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxCodeGenArrayTypeIsNotSupported", new object[] { arrayElementType.FullName })));
                }
                if (this.codeGenTrace != CodeGenTrace.None)
                {
                    this.EmitSourceInstruction(ldelemOpCode.ToString());
                }
                this.ilGen.Emit(ldelemOpCode);
                this.EmitStackTop(arrayElementType);
            }
        }

        internal void Ldelema(Type arrayElementType)
        {
            OpCode ldelema = OpCodes.Ldelema;
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction(ldelema.ToString());
            }
            this.ilGen.Emit(ldelema, arrayElementType);
            this.EmitStackTop(arrayElementType);
        }

        internal void Ldloc(int slot)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Ldloc " + slot);
            }
            switch (slot)
            {
                case 0:
                    this.ilGen.Emit(OpCodes.Ldloc_0);
                    return;

                case 1:
                    this.ilGen.Emit(OpCodes.Ldloc_1);
                    return;

                case 2:
                    this.ilGen.Emit(OpCodes.Ldloc_2);
                    return;

                case 3:
                    this.ilGen.Emit(OpCodes.Ldloc_3);
                    return;
            }
            if (slot <= 0xff)
            {
                this.ilGen.Emit(OpCodes.Ldloc_S, slot);
            }
            else
            {
                this.ilGen.Emit(OpCodes.Ldloc, slot);
            }
        }

        internal void Ldloc(LocalBuilder localBuilder)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Ldloc " + this.LocalNames[localBuilder]);
            }
            this.ilGen.Emit(OpCodes.Ldloc, localBuilder);
            this.EmitStackTop(localBuilder.LocalType);
        }

        internal void Ldloca(int slot)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Ldloca " + slot);
            }
            if (slot <= 0xff)
            {
                this.ilGen.Emit(OpCodes.Ldloca_S, slot);
            }
            else
            {
                this.ilGen.Emit(OpCodes.Ldloca, slot);
            }
        }

        internal void Ldloca(LocalBuilder localBuilder)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Ldloca " + this.LocalNames[localBuilder]);
            }
            this.ilGen.Emit(OpCodes.Ldloca, localBuilder);
            this.EmitStackTop(localBuilder.LocalType);
        }

        internal void LdlocAddress(LocalBuilder localBuilder)
        {
            if (localBuilder.LocalType.IsValueType)
            {
                this.Ldloca(localBuilder);
            }
            else
            {
                this.Ldloc(localBuilder);
            }
        }

        internal void Ldobj(Type type)
        {
            OpCode ldindOpCode = this.GetLdindOpCode(Type.GetTypeCode(type));
            if (!ldindOpCode.Equals(OpCodes.Nop))
            {
                if (this.codeGenTrace != CodeGenTrace.None)
                {
                    this.EmitSourceInstruction(ldindOpCode.ToString());
                }
                this.ilGen.Emit(ldindOpCode);
            }
            else
            {
                if (this.codeGenTrace != CodeGenTrace.None)
                {
                    this.EmitSourceInstruction("Ldobj " + type);
                }
                this.ilGen.Emit(OpCodes.Ldobj, type);
            }
        }

        internal void Ldstr(string strVar)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Ldstr " + strVar);
            }
            this.ilGen.Emit(OpCodes.Ldstr, strVar);
        }

        internal void Ldtoken(Type t)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Ldtoken " + t);
            }
            this.ilGen.Emit(OpCodes.Ldtoken, t);
        }

        internal void Load(object obj)
        {
            if (obj == null)
            {
                if (this.codeGenTrace != CodeGenTrace.None)
                {
                    this.EmitSourceInstruction("Ldnull");
                }
                this.ilGen.Emit(OpCodes.Ldnull);
            }
            else if (obj is ArgBuilder)
            {
                this.Ldarg((ArgBuilder) obj);
            }
            else if (obj is LocalBuilder)
            {
                this.Ldloc((LocalBuilder) obj);
            }
            else
            {
                this.Ldc(obj);
            }
        }

        internal void LoadAddress(object obj)
        {
            if (obj is ArgBuilder)
            {
                this.LdargAddress((ArgBuilder) obj);
            }
            else if (obj is LocalBuilder)
            {
                this.LdlocAddress((LocalBuilder) obj);
            }
            else
            {
                this.Load(obj);
            }
        }

        internal void LoadArrayElement(object obj, object arrayIndex)
        {
            Type elementType = this.GetVariableType(obj).GetElementType();
            this.Load(obj);
            this.Load(arrayIndex);
            if (IsStruct(elementType))
            {
                this.Ldelema(elementType);
                this.Ldobj(elementType);
            }
            else
            {
                this.Ldelem(elementType);
            }
        }

        internal void LoadZeroValueIntoLocal(Type type, LocalBuilder local)
        {
            if (!type.IsValueType)
            {
                this.Load(null);
                this.Store(local);
            }
            else
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                    case TypeCode.Char:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                        this.ilGen.Emit(OpCodes.Ldc_I4_0);
                        this.Store(local);
                        return;

                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        this.ilGen.Emit(OpCodes.Ldc_I4_0);
                        this.ilGen.Emit(OpCodes.Conv_I8);
                        this.Store(local);
                        return;

                    case TypeCode.Single:
                        this.ilGen.Emit(OpCodes.Ldc_R4, (float) 0f);
                        this.Store(local);
                        return;

                    case TypeCode.Double:
                        this.ilGen.Emit(OpCodes.Ldc_R8, (double) 0.0);
                        this.Store(local);
                        return;
                }
                this.LoadAddress(local);
                this.InitObj(type);
            }
        }

        internal void MarkLabel(Label label)
        {
            this.ilGen.MarkLabel(label);
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceLabel(label.GetHashCode() + ":");
            }
        }

        internal void New(ConstructorInfo constructor)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Newobj " + constructor.ToString() + " on type " + constructor.DeclaringType.ToString());
            }
            this.ilGen.Emit(OpCodes.Newobj, constructor);
        }

        internal void Pop()
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Pop");
            }
            this.ilGen.Emit(OpCodes.Pop);
        }

        private IfState PopIfState()
        {
            object expected = this.blockStack.Pop();
            IfState state = expected as IfState;
            if (state == null)
            {
                this.ThrowMismatchException(expected);
            }
            return state;
        }

        internal void Ret()
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Ret");
            }
            this.ilGen.Emit(OpCodes.Ret);
        }

        internal void Starg(int slot)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Starg " + slot);
            }
            if (slot <= 0xff)
            {
                this.ilGen.Emit(OpCodes.Starg_S, slot);
            }
            else
            {
                this.ilGen.Emit(OpCodes.Starg, slot);
            }
        }

        internal void Starg(ArgBuilder arg)
        {
            this.Starg(arg.Index);
        }

        internal void Stelem(Type arrayElementType)
        {
            if (arrayElementType.IsEnum)
            {
                this.Stelem(Enum.GetUnderlyingType(arrayElementType));
            }
            else
            {
                OpCode stelemOpCode = this.GetStelemOpCode(Type.GetTypeCode(arrayElementType));
                if (stelemOpCode.Equals(OpCodes.Nop))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxCodeGenArrayTypeIsNotSupported", new object[] { arrayElementType.FullName })));
                }
                if (this.codeGenTrace != CodeGenTrace.None)
                {
                    this.EmitSourceInstruction(stelemOpCode.ToString());
                }
                this.EmitStackTop(arrayElementType);
                this.ilGen.Emit(stelemOpCode);
            }
        }

        internal void Stloc(int slot)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Stloc " + slot);
            }
            switch (slot)
            {
                case 0:
                    this.ilGen.Emit(OpCodes.Stloc_0);
                    return;

                case 1:
                    this.ilGen.Emit(OpCodes.Stloc_1);
                    return;

                case 2:
                    this.ilGen.Emit(OpCodes.Stloc_2);
                    return;

                case 3:
                    this.ilGen.Emit(OpCodes.Stloc_3);
                    return;
            }
            if (slot <= 0xff)
            {
                this.ilGen.Emit(OpCodes.Stloc_S, slot);
            }
            else
            {
                this.ilGen.Emit(OpCodes.Stloc, slot);
            }
        }

        internal void Stloc(LocalBuilder local)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Stloc " + this.LocalNames[local]);
            }
            this.EmitStackTop(local.LocalType);
            this.ilGen.Emit(OpCodes.Stloc, local);
        }

        internal void Stobj(Type type)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Stobj " + type);
            }
            this.ilGen.Emit(OpCodes.Stobj, type);
        }

        internal void Store(object var)
        {
            if (var is ArgBuilder)
            {
                this.Starg((ArgBuilder) var);
            }
            else
            {
                if (!(var is LocalBuilder))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxCodeGenCanOnlyStoreIntoArgOrLocGot0", new object[] { var.GetType().FullName })));
                }
                this.Stloc((LocalBuilder) var);
            }
        }

        internal void StoreArrayElement(object obj, object arrayIndex, object value)
        {
            Type elementType = this.GetVariableType(obj).GetElementType();
            this.Load(obj);
            this.Load(arrayIndex);
            if (IsStruct(elementType))
            {
                this.Ldelema(elementType);
            }
            this.Load(value);
            this.ConvertValue(this.GetVariableType(value), elementType);
            if (IsStruct(elementType))
            {
                this.Stobj(elementType);
            }
            else
            {
                this.Stelem(elementType);
            }
        }

        private void ThrowMismatchException(object expected)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxCodeGenExpectingEnd", new object[] { expected.ToString() })));
        }

        internal void ToString(Type type)
        {
            if (type.IsValueType)
            {
                this.Box(type);
                this.Call(ObjectToString);
            }
            else
            {
                this.Dup();
                this.IfNot();
                this.Pop();
                this.Load("<null>");
                this.Else();
                this.Call(ObjectToString);
                this.EndIf();
            }
        }

        internal void Unbox(Type type)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Unbox " + type);
            }
            this.ilGen.Emit(OpCodes.Unbox, type);
        }

        private static MethodInfo BoxPointer
        {
            get
            {
                if (boxPointer == null)
                {
                    boxPointer = typeof(Pointer).GetMethod("Box");
                }
                return boxPointer;
            }
        }

        internal MethodInfo CurrentMethod
        {
            get
            {
                return this.dynamicMethod;
            }
        }

        private static MethodInfo GetTypeFromHandle
        {
            get
            {
                if (getTypeFromHandle == null)
                {
                    getTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle");
                }
                return getTypeFromHandle;
            }
        }

        private Hashtable LocalNames
        {
            get
            {
                if (this.localNames == null)
                {
                    this.localNames = new Hashtable();
                }
                return this.localNames;
            }
        }

        private static MethodInfo ObjectToString
        {
            get
            {
                if (objectToString == null)
                {
                    objectToString = typeof(object).GetMethod("ToString", new Type[0]);
                }
                return objectToString;
            }
        }

        private static MethodInfo StringConcat2
        {
            get
            {
                if (stringConcat2 == null)
                {
                    stringConcat2 = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
                }
                return stringConcat2;
            }
        }

        private static MethodInfo UnboxPointer
        {
            get
            {
                if (unboxPointer == null)
                {
                    unboxPointer = typeof(Pointer).GetMethod("Unbox");
                }
                return unboxPointer;
            }
        }

        private enum CodeGenTrace
        {
            None,
            Save,
            Tron
        }
    }
}

