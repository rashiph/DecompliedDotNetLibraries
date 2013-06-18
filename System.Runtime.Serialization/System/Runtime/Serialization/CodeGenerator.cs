namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime;
    using System.Security;

    internal class CodeGenerator
    {
        private ArrayList argList;
        [SecurityCritical]
        private static MethodInfo arraySetValue;
        private Stack blockStack;
        private CodeGenTrace codeGenTrace;
        private Type delegateType;
        private DynamicMethod dynamicMethod;
        [SecurityCritical]
        private static MethodInfo getTypeFromHandle;
        private ILGenerator ilGen;
        private int lineNo = 1;
        private Hashtable localNames;
        private Label methodEndLabel;
        [SecurityCritical]
        private static MethodInfo objectEquals;
        [SecurityCritical]
        private static MethodInfo objectToString;
        [SecurityCritical]
        private static Module serializationModule;
        [SecurityCritical]
        private static MethodInfo stringConcat2;
        [SecurityCritical]
        private static MethodInfo stringConcat3;
        [SecurityCritical]
        private static MethodInfo stringFormat;
        private LocalBuilder stringFormatArray;
        private static MethodInfo stringLength = typeof(string).GetProperty("Length").GetGetMethod();

        internal CodeGenerator()
        {
            SourceSwitch codeGenerationSwitch = SerializationTrace.CodeGenerationSwitch;
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

        internal void Add()
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Add");
            }
            this.ilGen.Emit(OpCodes.Add);
        }

        internal void And()
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("And");
            }
            this.ilGen.Emit(OpCodes.And);
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
            this.InitILGeneration(methodName, argTypes);
        }

        internal void BeginMethod(DynamicMethod dynamicMethod, Type delegateType, string methodName, Type[] argTypes, bool allowPrivateMemberAccess)
        {
            this.dynamicMethod = dynamicMethod;
            this.ilGen = this.dynamicMethod.GetILGenerator();
            this.delegateType = delegateType;
            this.InitILGeneration(methodName, argTypes);
        }

        internal void Bgt(Label label)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Bgt " + label.GetHashCode());
            }
            this.ilGen.Emit(OpCodes.Bgt, label);
        }

        internal void Ble(Label label)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Ble " + label.GetHashCode());
            }
            this.ilGen.Emit(OpCodes.Ble, label);
        }

        internal void Blt(Label label)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Blt " + label.GetHashCode());
            }
            this.ilGen.Emit(OpCodes.Blt, label);
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

        internal void Break(object forState)
        {
            this.InternalBreakFor(forState, OpCodes.Br);
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

        internal void Call(ConstructorInfo ctor)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Call " + ctor.ToString() + " on type " + ctor.DeclaringType.ToString());
            }
            this.ilGen.Emit(OpCodes.Call, ctor);
        }

        internal void Call(MethodInfo methodInfo)
        {
            if (methodInfo.IsVirtual && !methodInfo.DeclaringType.IsValueType)
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

        internal void Call(object thisObj, MethodInfo methodInfo)
        {
            this.VerifyParameterCount(methodInfo, 0);
            this.LoadThis(thisObj, methodInfo);
            this.Call(methodInfo);
        }

        internal void Call(object thisObj, MethodInfo methodInfo, object param1)
        {
            this.VerifyParameterCount(methodInfo, 1);
            this.LoadThis(thisObj, methodInfo);
            this.LoadParam(param1, 1, methodInfo);
            this.Call(methodInfo);
        }

        internal void Call(object thisObj, MethodInfo methodInfo, object param1, object param2)
        {
            this.VerifyParameterCount(methodInfo, 2);
            this.LoadThis(thisObj, methodInfo);
            this.LoadParam(param1, 1, methodInfo);
            this.LoadParam(param2, 2, methodInfo);
            this.Call(methodInfo);
        }

        internal void Call(object thisObj, MethodInfo methodInfo, object param1, object param2, object param3)
        {
            this.VerifyParameterCount(methodInfo, 3);
            this.LoadThis(thisObj, methodInfo);
            this.LoadParam(param1, 1, methodInfo);
            this.LoadParam(param2, 2, methodInfo);
            this.LoadParam(param3, 3, methodInfo);
            this.Call(methodInfo);
        }

        internal void Call(object thisObj, MethodInfo methodInfo, object param1, object param2, object param3, object param4)
        {
            this.VerifyParameterCount(methodInfo, 4);
            this.LoadThis(thisObj, methodInfo);
            this.LoadParam(param1, 1, methodInfo);
            this.LoadParam(param2, 2, methodInfo);
            this.LoadParam(param3, 3, methodInfo);
            this.LoadParam(param4, 4, methodInfo);
            this.Call(methodInfo);
        }

        internal void Call(object thisObj, MethodInfo methodInfo, object param1, object param2, object param3, object param4, object param5)
        {
            this.VerifyParameterCount(methodInfo, 5);
            this.LoadThis(thisObj, methodInfo);
            this.LoadParam(param1, 1, methodInfo);
            this.LoadParam(param2, 2, methodInfo);
            this.LoadParam(param3, 3, methodInfo);
            this.LoadParam(param4, 4, methodInfo);
            this.LoadParam(param5, 5, methodInfo);
            this.Call(methodInfo);
        }

        internal void Call(object thisObj, MethodInfo methodInfo, object param1, object param2, object param3, object param4, object param5, object param6)
        {
            this.VerifyParameterCount(methodInfo, 6);
            this.LoadThis(thisObj, methodInfo);
            this.LoadParam(param1, 1, methodInfo);
            this.LoadParam(param2, 2, methodInfo);
            this.LoadParam(param3, 3, methodInfo);
            this.LoadParam(param4, 4, methodInfo);
            this.LoadParam(param5, 5, methodInfo);
            this.LoadParam(param6, 6, methodInfo);
            this.Call(methodInfo);
        }

        internal void CallStringFormat(string msg, params object[] values)
        {
            this.NewArray(typeof(object), values.Length);
            if (this.stringFormatArray == null)
            {
                this.stringFormatArray = this.DeclareLocal(typeof(object[]), "stringFormatArray");
            }
            this.Stloc(this.stringFormatArray);
            for (int i = 0; i < values.Length; i++)
            {
                this.StoreArrayElement(this.stringFormatArray, i, values[i]);
            }
            this.Load(msg);
            this.Load(this.stringFormatArray);
            this.Call(StringFormat);
        }

        internal void Case(Label caseLabel1, string caseLabelName)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("case " + caseLabelName + "{");
            }
            this.MarkLabel(caseLabel1);
        }

        internal void Castclass(Type target)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Castclass " + target);
            }
            this.ilGen.Emit(OpCodes.Castclass, target);
        }

        internal void Ceq()
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Ceq");
            }
            this.ilGen.Emit(OpCodes.Ceq);
        }

        internal void Concat2()
        {
            this.Call(StringConcat2);
        }

        internal void Concat3()
        {
            this.Call(StringConcat3);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal void ConvertAddress(Type source, Type target)
        {
            this.InternalConvert(source, target, true);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal void ConvertValue(Type source, Type target)
        {
            this.InternalConvert(source, target, false);
        }

        internal void Dec(object var)
        {
            this.Load(var);
            this.Load(1);
            this.Subtract();
            this.Store(var);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
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

        internal LocalBuilder DeclareLocal(Type type, string name, object initialValue)
        {
            LocalBuilder var = this.DeclareLocal(type, name);
            this.Load(initialValue);
            this.Store(var);
            return var;
        }

        internal void DefaultCase()
        {
            object expected = this.blockStack.Peek();
            SwitchState state = expected as SwitchState;
            if (state == null)
            {
                this.ThrowMismatchException(expected);
            }
            this.MarkLabel(state.DefaultLabel);
            state.DefaultDefined = true;
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

        internal void ElseIf(object value1, Cmp cmpOp, object value2)
        {
            IfState state = (IfState) this.blockStack.Pop();
            this.Br(state.EndIf);
            this.MarkLabel(state.ElseBegin);
            this.Load(value1);
            this.Load(value2);
            state.ElseBegin = this.DefineLabel();
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Branch if " + this.GetCmpInverse(cmpOp).ToString() + " to " + state.ElseBegin.GetHashCode().ToString(NumberFormatInfo.InvariantInfo));
            }
            this.ilGen.Emit(this.GetBranchCode(cmpOp), state.ElseBegin);
            this.blockStack.Push(state);
        }

        internal void ElseIfIsEmptyString(LocalBuilder strLocal)
        {
            IfState state = (IfState) this.blockStack.Pop();
            this.Br(state.EndIf);
            this.MarkLabel(state.ElseBegin);
            this.Load(strLocal);
            this.Call(stringLength);
            this.Load(0);
            state.ElseBegin = this.DefineLabel();
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Branch if " + this.GetCmpInverse(Cmp.EqualTo).ToString() + " to " + state.ElseBegin.GetHashCode().ToString(NumberFormatInfo.InvariantInfo));
            }
            this.ilGen.Emit(this.GetBranchCode(Cmp.EqualTo), state.ElseBegin);
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

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal void EmitSourceLabel(string line)
        {
            this.EmitSourceLine(line);
        }

        internal void EmitSourceLine(string line)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                SerializationTrace.WriteInstruction(this.lineNo++, line);
            }
            if ((this.ilGen != null) && (this.codeGenTrace == CodeGenTrace.Tron))
            {
                this.ilGen.Emit(OpCodes.Ldstr, string.Format(CultureInfo.InvariantCulture, "{0:00000}: {1}", new object[] { this.lineNo - 1, line }));
                this.ilGen.Emit(OpCodes.Call, XmlFormatGeneratorStatics.TraceInstructionMethod);
            }
        }

        internal void EmitStackTop(Type stackTopType)
        {
            if (this.codeGenTrace == CodeGenTrace.Tron)
            {
                this.codeGenTrace = CodeGenTrace.None;
                this.Dup();
                this.ToString(stackTopType);
                LocalBuilder var = this.DeclareLocal(Globals.TypeOfString, "topValue");
                this.Store(var);
                this.Load("//value = ");
                this.Load(var);
                this.Concat2();
                this.Call(XmlFormatGeneratorStatics.TraceInstructionMethod);
                this.codeGenTrace = CodeGenTrace.Tron;
            }
        }

        internal void EndCase()
        {
            object expected = this.blockStack.Peek();
            SwitchState state = expected as SwitchState;
            if (state == null)
            {
                this.ThrowMismatchException(expected);
            }
            this.Br(state.EndOfSwitchLabel);
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("} //end case ");
            }
        }

        internal void EndFor()
        {
            object expected = this.blockStack.Pop();
            ForState state = expected as ForState;
            if (state == null)
            {
                this.ThrowMismatchException(expected);
            }
            if (state.Index != null)
            {
                this.Ldloc(state.Index);
                this.Ldc(1);
                this.Add();
                this.Stloc(state.Index);
                this.MarkLabel(state.TestLabel);
                this.Ldloc(state.Index);
                this.Load(state.End);
                if (this.GetVariableType(state.End).IsArray)
                {
                    this.Ldlen();
                }
                this.Blt(state.BeginLabel);
            }
            else
            {
                this.Br(state.BeginLabel);
            }
            if (state.RequiresEndLabel)
            {
                this.MarkLabel(state.EndLabel);
            }
        }

        internal void EndForEach(MethodInfo moveNextMethod)
        {
            object expected = this.blockStack.Pop();
            ForState state = expected as ForState;
            if (state == null)
            {
                this.ThrowMismatchException(expected);
            }
            this.MarkLabel(state.TestLabel);
            object end = state.End;
            this.Call(end, moveNextMethod);
            this.Brtrue(state.BeginLabel);
            if (state.RequiresEndLabel)
            {
                this.MarkLabel(state.EndLabel);
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

        internal void EndSwitch()
        {
            object expected = this.blockStack.Pop();
            SwitchState state = expected as SwitchState;
            if (state == null)
            {
                this.ThrowMismatchException(expected);
            }
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("} //end switch");
            }
            if (!state.DefaultDefined)
            {
                this.MarkLabel(state.DefaultLabel);
            }
            this.MarkLabel(state.EndOfSwitchLabel);
        }

        internal object For(LocalBuilder local, object start, object end)
        {
            ForState state = new ForState(local, this.DefineLabel(), this.DefineLabel(), end);
            if (state.Index != null)
            {
                this.Load(start);
                this.Stloc(state.Index);
                this.Br(state.TestLabel);
            }
            this.MarkLabel(state.BeginLabel);
            this.blockStack.Push(state);
            return state;
        }

        internal void ForEach(LocalBuilder local, Type elementType, Type enumeratorType, LocalBuilder enumerator, MethodInfo getCurrentMethod)
        {
            ForState state = new ForState(local, this.DefineLabel(), this.DefineLabel(), enumerator);
            this.Br(state.TestLabel);
            this.MarkLabel(state.BeginLabel);
            this.Call(enumerator, getCurrentMethod);
            this.ConvertValue(elementType, this.GetVariableType(local));
            this.Stloc(local);
            this.blockStack.Push(state);
        }

        internal ArgBuilder GetArg(int index)
        {
            return (ArgBuilder) this.argList[index];
        }

        private OpCode GetBranchCode(Cmp cmp)
        {
            switch (cmp)
            {
                case Cmp.LessThan:
                    return OpCodes.Bge;

                case Cmp.EqualTo:
                    return OpCodes.Bne_Un;

                case Cmp.LessThanOrEqualTo:
                    return OpCodes.Bgt;

                case Cmp.GreaterThan:
                    return OpCodes.Ble;

                case Cmp.NotEqualTo:
                    return OpCodes.Beq;
            }
            return OpCodes.Blt;
        }

        private Cmp GetCmpInverse(Cmp cmp)
        {
            switch (cmp)
            {
                case Cmp.LessThan:
                    return Cmp.GreaterThanOrEqualTo;

                case Cmp.EqualTo:
                    return Cmp.NotEqualTo;

                case Cmp.LessThanOrEqualTo:
                    return Cmp.GreaterThan;

                case Cmp.GreaterThan:
                    return Cmp.LessThanOrEqualTo;

                case Cmp.NotEqualTo:
                    return Cmp.EqualTo;
            }
            return Cmp.LessThan;
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
                case TypeCode.DBNull:
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
                case TypeCode.DBNull:
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

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal void If()
        {
            this.InternalIf(false);
        }

        internal void If(Cmp cmpOp)
        {
            IfState state = new IfState {
                EndIf = this.DefineLabel(),
                ElseBegin = this.DefineLabel()
            };
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Branch if " + this.GetCmpInverse(cmpOp).ToString() + " to " + state.ElseBegin.GetHashCode().ToString(NumberFormatInfo.InvariantInfo));
            }
            this.ilGen.Emit(this.GetBranchCode(cmpOp), state.ElseBegin);
            this.blockStack.Push(state);
        }

        internal void If(object value1, Cmp cmpOp, object value2)
        {
            this.Load(value1);
            this.Load(value2);
            this.If(cmpOp);
        }

        internal void IfFalseBreak(object forState)
        {
            this.InternalBreakFor(forState, OpCodes.Brfalse);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal void IfNot()
        {
            this.InternalIf(true);
        }

        internal void IfNotDefaultValue(object value)
        {
            Type variableType = this.GetVariableType(value);
            TypeCode typeCode = Type.GetTypeCode(variableType);
            if (((typeCode == TypeCode.Object) && variableType.IsValueType) || ((typeCode == TypeCode.DateTime) || (typeCode == TypeCode.Decimal)))
            {
                this.LoadDefaultValue(variableType);
                this.ConvertValue(variableType, Globals.TypeOfObject);
                this.Load(value);
                this.ConvertValue(variableType, Globals.TypeOfObject);
                this.Call(ObjectEquals);
                this.IfNot();
            }
            else
            {
                this.LoadDefaultValue(variableType);
                this.Load(value);
                this.If(Cmp.NotEqualTo);
            }
        }

        internal void IfNotIsEmptyString(LocalBuilder strLocal)
        {
            this.Load(strLocal);
            this.Call(stringLength);
            this.Load(0);
            this.If(Cmp.NotEqualTo);
        }

        internal void IfTrueBreak(object forState)
        {
            this.InternalBreakFor(forState, OpCodes.Brtrue);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal void IgnoreReturnValue()
        {
            this.Pop();
        }

        internal void Inc(object var)
        {
            this.Load(var);
            this.Load(1);
            this.Add();
            this.Store(var);
        }

        private void InitILGeneration(string methodName, Type[] argTypes)
        {
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

        internal void InitObj(Type valueType)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Initobj " + valueType);
            }
            this.ilGen.Emit(OpCodes.Initobj, valueType);
        }

        internal void InternalBreakFor(object userForState, OpCode branchInstruction)
        {
            foreach (object obj2 in this.blockStack)
            {
                ForState state = obj2 as ForState;
                if ((state != null) && (state == userForState))
                {
                    if (!state.RequiresEndLabel)
                    {
                        state.EndLabel = this.DefineLabel();
                        state.RequiresEndLabel = true;
                    }
                    if (this.codeGenTrace != CodeGenTrace.None)
                    {
                        this.EmitSourceInstruction(branchInstruction + " " + state.EndLabel.GetHashCode());
                    }
                    this.ilGen.Emit(branchInstruction, state.EndLabel);
                    break;
                }
            }
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
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("NoConversionPossibleTo", new object[] { DataContract.GetClrTypeFullName(target) })));
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
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("IsNotAssignableFrom", new object[] { DataContract.GetClrTypeFullName(target), DataContract.GetClrTypeFullName(source) })));
                        }
                        this.Unbox(target);
                        if (!isAddress)
                        {
                            this.Ldobj(target);
                        }
                    }
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
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("IsNotAssignableFrom", new object[] { DataContract.GetClrTypeFullName(target), DataContract.GetClrTypeFullName(source) })));
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

        internal void Ldc(double d)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Ldc.r8 " + d);
            }
            this.ilGen.Emit(OpCodes.Ldc_R8, d);
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

        internal void Ldc(long l)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Ldc.i8 " + l);
            }
            this.ilGen.Emit(OpCodes.Ldc_I8, l);
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
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.Runtime.Serialization.SR.GetString("CharIsInvalidPrimitive")));

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

                    case TypeCode.Int64:
                        this.Ldc((long) o);
                        return;

                    case TypeCode.UInt64:
                        this.Ldc((long) ((ulong) o));
                        return;

                    case TypeCode.Single:
                        this.Ldc((float) o);
                        return;

                    case TypeCode.Double:
                        this.Ldc((double) o);
                        return;

                    case TypeCode.String:
                        this.Ldstr((string) o);
                        return;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("UnknownConstantType", new object[] { DataContract.GetClrTypeFullName(enumType) })));
            }
        }

        internal void Ldc(float f)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Ldc.r4 " + f);
            }
            this.ilGen.Emit(OpCodes.Ldc_R4, f);
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
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("ArrayTypeIsNotSupported", new object[] { DataContract.GetClrTypeFullName(arrayElementType) })));
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

        internal void Ldlen()
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Ldlen");
            }
            this.ilGen.Emit(OpCodes.Ldlen);
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Conv.i4");
            }
            this.ilGen.Emit(OpCodes.Conv_I4);
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

        internal void LoadDefaultValue(Type type)
        {
            if (!type.IsValueType)
            {
                this.Load(null);
            }
            else
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                        this.Ldc(false);
                        return;

                    case TypeCode.Char:
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                        this.Ldc(0);
                        return;

                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        this.Ldc((long) 0L);
                        return;

                    case TypeCode.Single:
                        this.Ldc((float) 0f);
                        return;

                    case TypeCode.Double:
                        this.Ldc((double) 0.0);
                        return;
                }
                LocalBuilder builder = this.DeclareLocal(type, "zero");
                this.LoadAddress(builder);
                this.InitObj(type);
                this.Load(builder);
            }
        }

        internal Type LoadMember(MemberInfo memberInfo)
        {
            Type stackTopType = null;
            if (memberInfo.MemberType == MemberTypes.Field)
            {
                FieldInfo field = (FieldInfo) memberInfo;
                stackTopType = field.FieldType;
                if (field.IsStatic)
                {
                    if (this.codeGenTrace != CodeGenTrace.None)
                    {
                        this.EmitSourceInstruction(string.Concat(new object[] { "Ldsfld ", field, " on type ", field.DeclaringType }));
                    }
                    this.ilGen.Emit(OpCodes.Ldsfld, field);
                }
                else
                {
                    if (this.codeGenTrace != CodeGenTrace.None)
                    {
                        this.EmitSourceInstruction(string.Concat(new object[] { "Ldfld ", field, " on type ", field.DeclaringType }));
                    }
                    this.ilGen.Emit(OpCodes.Ldfld, field);
                }
            }
            else if (memberInfo.MemberType == MemberTypes.Property)
            {
                PropertyInfo info2 = memberInfo as PropertyInfo;
                stackTopType = info2.PropertyType;
                if (info2 != null)
                {
                    MethodInfo getMethod = info2.GetGetMethod(true);
                    if (getMethod == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("NoGetMethodForProperty", new object[] { info2.DeclaringType, info2 })));
                    }
                    this.Call(getMethod);
                }
            }
            else
            {
                if (memberInfo.MemberType != MemberTypes.Method)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("CannotLoadMemberType", new object[] { memberInfo.MemberType, memberInfo.DeclaringType, memberInfo.Name })));
                }
                MethodInfo methodInfo = (MethodInfo) memberInfo;
                stackTopType = methodInfo.ReturnType;
                this.Call(methodInfo);
            }
            this.EmitStackTop(stackTopType);
            return stackTopType;
        }

        private void LoadParam(object arg, int oneBasedArgIndex, MethodBase methodInfo)
        {
            this.Load(arg);
            if (arg != null)
            {
                this.ConvertValue(this.GetVariableType(arg), methodInfo.GetParameters()[oneBasedArgIndex - 1].ParameterType);
            }
        }

        private void LoadThis(object thisObj, MethodInfo methodInfo)
        {
            if ((thisObj != null) && !methodInfo.IsStatic)
            {
                this.LoadAddress(thisObj);
                this.ConvertAddress(this.GetVariableType(thisObj), methodInfo.DeclaringType);
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

        internal void New(ConstructorInfo constructorInfo)
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Newobj " + constructorInfo.ToString() + " on type " + constructorInfo.DeclaringType.ToString());
            }
            this.ilGen.Emit(OpCodes.Newobj, constructorInfo);
        }

        internal void New(ConstructorInfo constructorInfo, object param1)
        {
            this.LoadParam(param1, 1, constructorInfo);
            this.New(constructorInfo);
        }

        internal void NewArray(Type elementType, object len)
        {
            this.Load(len);
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Newarr " + elementType);
            }
            this.ilGen.Emit(OpCodes.Newarr, elementType);
        }

        internal void Not()
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Not");
            }
            this.ilGen.Emit(OpCodes.Not);
        }

        internal void Or()
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Or");
            }
            this.ilGen.Emit(OpCodes.Or);
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

        internal void Set(LocalBuilder local, object value)
        {
            this.Load(value);
            this.Store(local);
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
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("ArrayTypeIsNotSupported", new object[] { DataContract.GetClrTypeFullName(arrayElementType) })));
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
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("CanOnlyStoreIntoArgOrLocGot0", new object[] { DataContract.GetClrTypeFullName(var.GetType()) })));
                }
                this.Stloc((LocalBuilder) var);
            }
        }

        internal void StoreArrayElement(object obj, object arrayIndex, object value)
        {
            Type variableType = this.GetVariableType(obj);
            if (variableType == Globals.TypeOfArray)
            {
                this.Call(obj, ArraySetValue, value, arrayIndex);
            }
            else
            {
                Type elementType = variableType.GetElementType();
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
        }

        internal void StoreMember(MemberInfo memberInfo)
        {
            if (memberInfo.MemberType == MemberTypes.Field)
            {
                FieldInfo field = (FieldInfo) memberInfo;
                if (field.IsStatic)
                {
                    if (this.codeGenTrace != CodeGenTrace.None)
                    {
                        this.EmitSourceInstruction(string.Concat(new object[] { "Stsfld ", field, " on type ", field.DeclaringType }));
                    }
                    this.ilGen.Emit(OpCodes.Stsfld, field);
                }
                else
                {
                    if (this.codeGenTrace != CodeGenTrace.None)
                    {
                        this.EmitSourceInstruction(string.Concat(new object[] { "Stfld ", field, " on type ", field.DeclaringType }));
                    }
                    this.ilGen.Emit(OpCodes.Stfld, field);
                }
            }
            else if (memberInfo.MemberType == MemberTypes.Property)
            {
                PropertyInfo info2 = memberInfo as PropertyInfo;
                if (info2 != null)
                {
                    MethodInfo setMethod = info2.GetSetMethod(true);
                    if (setMethod == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("NoSetMethodForProperty", new object[] { info2.DeclaringType, info2 })));
                    }
                    this.Call(setMethod);
                }
            }
            else
            {
                if (memberInfo.MemberType != MemberTypes.Method)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("CannotLoadMemberType", new object[] { memberInfo.MemberType })));
                }
                this.Call((MethodInfo) memberInfo);
            }
        }

        internal void Subtract()
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Sub");
            }
            this.ilGen.Emit(OpCodes.Sub);
        }

        internal Label[] Switch(int labelCount)
        {
            SwitchState state = new SwitchState(this.DefineLabel(), this.DefineLabel());
            Label[] labels = new Label[labelCount];
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i] = this.DefineLabel();
            }
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("switch (");
                foreach (Label label in labels)
                {
                    this.EmitSourceInstruction("    " + label.GetHashCode());
                }
                this.EmitSourceInstruction(") {");
            }
            this.ilGen.Emit(OpCodes.Switch, labels);
            this.Br(state.DefaultLabel);
            this.blockStack.Push(state);
            return labels;
        }

        internal void Throw()
        {
            if (this.codeGenTrace != CodeGenTrace.None)
            {
                this.EmitSourceInstruction("Throw");
            }
            this.ilGen.Emit(OpCodes.Throw);
        }

        private void ThrowMismatchException(object expected)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("ExpectingEnd", new object[] { expected.ToString() })));
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
                this.Load(null);
                this.If(Cmp.EqualTo);
                this.Pop();
                this.Load("<null>");
                this.Else();
                if (type.IsArray)
                {
                    LocalBuilder var = this.DeclareLocal(type, "arrayVar");
                    this.Store(var);
                    this.Load("{ ");
                    LocalBuilder builder2 = this.DeclareLocal(typeof(string), "arrayValueString");
                    this.Store(builder2);
                    LocalBuilder local = this.DeclareLocal(typeof(int), "i");
                    this.For(local, 0, var);
                    this.Load(builder2);
                    this.LoadArrayElement(var, local);
                    this.ToString(var.LocalType.GetElementType());
                    this.Load(", ");
                    this.Concat3();
                    this.Store(builder2);
                    this.EndFor();
                    this.Load(builder2);
                    this.Load("}");
                    this.Concat2();
                }
                else
                {
                    this.Call(ObjectToString);
                }
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

        internal void VerifyParameterCount(MethodInfo methodInfo, int expectedCount)
        {
            if (methodInfo.GetParameters().Length != expectedCount)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("ParameterCountMismatch", new object[] { methodInfo.Name, methodInfo.GetParameters().Length, expectedCount })));
            }
        }

        private static MethodInfo ArraySetValue
        {
            [SecuritySafeCritical]
            get
            {
                if (arraySetValue == null)
                {
                    arraySetValue = typeof(Array).GetMethod("SetValue", new Type[] { typeof(object), typeof(int) });
                }
                return arraySetValue;
            }
        }

        internal MethodInfo CurrentMethod
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.dynamicMethod;
            }
        }

        private static MethodInfo GetTypeFromHandle
        {
            [SecuritySafeCritical]
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

        private static MethodInfo ObjectEquals
        {
            [SecuritySafeCritical]
            get
            {
                if (objectEquals == null)
                {
                    objectEquals = Globals.TypeOfObject.GetMethod("Equals", BindingFlags.Public | BindingFlags.Static);
                }
                return objectEquals;
            }
        }

        private static MethodInfo ObjectToString
        {
            [SecuritySafeCritical]
            get
            {
                if (objectToString == null)
                {
                    objectToString = typeof(object).GetMethod("ToString", new Type[0]);
                }
                return objectToString;
            }
        }

        private static Module SerializationModule
        {
            [SecuritySafeCritical]
            get
            {
                if (serializationModule == null)
                {
                    serializationModule = typeof(CodeGenerator).Module;
                }
                return serializationModule;
            }
        }

        private static MethodInfo StringConcat2
        {
            [SecuritySafeCritical]
            get
            {
                if (stringConcat2 == null)
                {
                    stringConcat2 = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
                }
                return stringConcat2;
            }
        }

        private static MethodInfo StringConcat3
        {
            [SecuritySafeCritical]
            get
            {
                if (stringConcat3 == null)
                {
                    stringConcat3 = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string), typeof(string) });
                }
                return stringConcat3;
            }
        }

        private static MethodInfo StringFormat
        {
            [SecuritySafeCritical]
            get
            {
                if (stringFormat == null)
                {
                    stringFormat = typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(object[]) });
                }
                return stringFormat;
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

