namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.Expando;
    using System.Security;
    using System.Security.Permissions;

    internal sealed class TypeReferences
    {
        private Module _jscriptReferenceModule;
        private static readonly Microsoft.JScript.SimpleHashtable _predefinedTypeTable = new Microsoft.JScript.SimpleHashtable(0x22);
        private System.Type[] _typeTable;
        private const int TypeReferenceArrayLength = 0x53;
        private const int TypeReferenceStartOfSpecialCases = 0x51;

        static TypeReferences()
        {
            _predefinedTypeTable["boolean"] = typeof(bool);
            _predefinedTypeTable["byte"] = typeof(byte);
            _predefinedTypeTable["char"] = typeof(char);
            _predefinedTypeTable["decimal"] = typeof(decimal);
            _predefinedTypeTable["double"] = typeof(double);
            _predefinedTypeTable["float"] = typeof(float);
            _predefinedTypeTable["int"] = typeof(int);
            _predefinedTypeTable["long"] = typeof(long);
            _predefinedTypeTable["sbyte"] = typeof(sbyte);
            _predefinedTypeTable["short"] = typeof(short);
            _predefinedTypeTable["void"] = typeof(void);
            _predefinedTypeTable["uint"] = typeof(uint);
            _predefinedTypeTable["ulong"] = typeof(ulong);
            _predefinedTypeTable["ushort"] = typeof(ushort);
            _predefinedTypeTable["ActiveXObject"] = typeof(object);
            _predefinedTypeTable["Boolean"] = typeof(bool);
            _predefinedTypeTable["Number"] = typeof(double);
            _predefinedTypeTable["Object"] = typeof(object);
            _predefinedTypeTable["String"] = typeof(string);
            _predefinedTypeTable["Type"] = typeof(System.Type);
            _predefinedTypeTable["Array"] = TypeReference.ArrayObject;
            _predefinedTypeTable["Date"] = TypeReference.DateObject;
            _predefinedTypeTable["Enumerator"] = TypeReference.EnumeratorObject;
            _predefinedTypeTable["Error"] = TypeReference.ErrorObject;
            _predefinedTypeTable["EvalError"] = TypeReference.EvalErrorObject;
            _predefinedTypeTable["Function"] = TypeReference.ScriptFunction;
            _predefinedTypeTable["RangeError"] = TypeReference.RangeErrorObject;
            _predefinedTypeTable["ReferenceError"] = TypeReference.ReferenceErrorObject;
            _predefinedTypeTable["RegExp"] = TypeReference.RegExpObject;
            _predefinedTypeTable["SyntaxError"] = TypeReference.SyntaxErrorObject;
            _predefinedTypeTable["TypeError"] = TypeReference.TypeErrorObject;
            _predefinedTypeTable["URIError"] = TypeReference.URIErrorObject;
            _predefinedTypeTable["VBArray"] = TypeReference.VBArrayObject;
        }

        internal TypeReferences(Module jscriptReferenceModule)
        {
            this._jscriptReferenceModule = jscriptReferenceModule;
            this._typeTable = new System.Type[0x53];
        }

        internal static object GetConstantValue(System.Reflection.FieldInfo field)
        {
            if ((field.GetType().Assembly == typeof(TypeReferences).Assembly) || !field.DeclaringType.Assembly.ReflectionOnly)
            {
                return field.GetValue(null);
            }
            System.Type fieldType = field.FieldType;
            object rawConstantValue = field.GetRawConstantValue();
            if (fieldType.IsEnum)
            {
                return MetadataEnumValue.GetEnumValue(fieldType, rawConstantValue);
            }
            return rawConstantValue;
        }

        internal static object GetDefaultParameterValue(ParameterInfo parameter)
        {
            if (!(parameter.GetType().Assembly == typeof(TypeReferences).Assembly) && parameter.Member.DeclaringType.Assembly.ReflectionOnly)
            {
                return parameter.RawDefaultValue;
            }
            return parameter.DefaultValue;
        }

        internal System.Type GetPredefinedType(string typeName)
        {
            object obj2 = _predefinedTypeTable[typeName];
            System.Type typeReference = obj2 as System.Type;
            if ((typeReference == null) && (obj2 is TypeReference))
            {
                typeReference = this.GetTypeReference((TypeReference) obj2);
            }
            return typeReference;
        }

        private System.Type GetTypeReference(TypeReference typeRef)
        {
            System.Type type = this._typeTable[(int) typeRef];
            if (null == type)
            {
                string str = "Microsoft.JScript.";
                if (typeRef >= TypeReference.BaseVsaStartup)
                {
                    switch (typeRef)
                    {
                        case TypeReference.BaseVsaStartup:
                            str = "Microsoft.JScript.Vsa.";
                            break;

                        case TypeReference.VsaEngine:
                            str = "Microsoft.JScript.Vsa.";
                            break;
                    }
                }
                type = this.JScriptReferenceModule.GetType(str + System.Enum.GetName(typeof(TypeReference), (int) typeRef));
                this._typeTable[(int) typeRef] = type;
            }
            return type;
        }

        internal static bool InExecutionContext(System.Type type)
        {
            if (type != null)
            {
                Assembly assembly = type.Assembly;
                if (assembly.ReflectionOnly)
                {
                    return (assembly.Location != typeof(TypeReferences).Assembly.Location);
                }
            }
            return true;
        }

        internal bool InReferenceContext(IReflect ireflect)
        {
            if ((ireflect != null) && (ireflect is System.Type))
            {
                return this.InReferenceContext((System.Type) ireflect);
            }
            return true;
        }

        internal bool InReferenceContext(MemberInfo member)
        {
            if (member == null)
            {
                return true;
            }
            if (member is JSMethod)
            {
                member = ((JSMethod) member).GetMethodInfo(null);
            }
            else if (member is JSMethodInfo)
            {
                member = ((JSMethodInfo) member).method;
            }
            return this.InReferenceContext(member.DeclaringType);
        }

        internal bool InReferenceContext(System.Type type)
        {
            if (type != null)
            {
                Assembly assembly = type.Assembly;
                if (!assembly.ReflectionOnly && !(assembly != typeof(TypeReferences).Assembly))
                {
                    return !this.JScriptReferenceModule.Assembly.ReflectionOnly;
                }
            }
            return true;
        }

        private static MemberInfo MapMemberInfoToExecutionContext(MemberInfo member)
        {
            if (InExecutionContext(member.DeclaringType))
            {
                return member;
            }
            return typeof(TypeReferences).Module.ResolveMember(member.MetadataToken);
        }

        private MemberInfo MapMemberInfoToReferenceContext(MemberInfo member)
        {
            if (this.InReferenceContext(member.DeclaringType))
            {
                return member;
            }
            return this.JScriptReferenceModule.ResolveMember(member.MetadataToken);
        }

        internal static ConstructorInfo ToExecutionContext(ConstructorInfo constructor)
        {
            return (ConstructorInfo) MapMemberInfoToExecutionContext(constructor);
        }

        internal static System.Reflection.FieldInfo ToExecutionContext(System.Reflection.FieldInfo field)
        {
            return (System.Reflection.FieldInfo) MapMemberInfoToExecutionContext(field);
        }

        internal static IReflect ToExecutionContext(IReflect ireflect)
        {
            if (ireflect is System.Type)
            {
                return ToExecutionContext((System.Type) ireflect);
            }
            return ireflect;
        }

        internal static MethodInfo ToExecutionContext(MethodInfo method)
        {
            if (method is JSMethod)
            {
                method = ((JSMethod) method).GetMethodInfo(null);
            }
            else if (method is JSMethodInfo)
            {
                method = ((JSMethodInfo) method).method;
            }
            return (MethodInfo) MapMemberInfoToExecutionContext(method);
        }

        internal static PropertyInfo ToExecutionContext(PropertyInfo property)
        {
            return (PropertyInfo) MapMemberInfoToExecutionContext(property);
        }

        internal static System.Type ToExecutionContext(System.Type type)
        {
            if (InExecutionContext(type))
            {
                return type;
            }
            return typeof(TypeReferences).Module.ResolveType(type.MetadataToken, null, null);
        }

        internal ConstructorInfo ToReferenceContext(ConstructorInfo constructor)
        {
            return (ConstructorInfo) this.MapMemberInfoToReferenceContext(constructor);
        }

        internal System.Reflection.FieldInfo ToReferenceContext(System.Reflection.FieldInfo field)
        {
            return (System.Reflection.FieldInfo) this.MapMemberInfoToReferenceContext(field);
        }

        internal IReflect ToReferenceContext(IReflect ireflect)
        {
            if (ireflect is System.Type)
            {
                return this.ToReferenceContext((System.Type) ireflect);
            }
            return ireflect;
        }

        internal MethodInfo ToReferenceContext(MethodInfo method)
        {
            if (method is JSMethod)
            {
                method = ((JSMethod) method).GetMethodInfo(null);
            }
            else if (method is JSMethodInfo)
            {
                method = ((JSMethodInfo) method).method;
            }
            return (MethodInfo) this.MapMemberInfoToReferenceContext(method);
        }

        internal PropertyInfo ToReferenceContext(PropertyInfo property)
        {
            return (PropertyInfo) this.MapMemberInfoToReferenceContext(property);
        }

        internal System.Type ToReferenceContext(System.Type type)
        {
            if (this.InReferenceContext(type))
            {
                return type;
            }
            if (type.IsArray)
            {
                return Microsoft.JScript.Convert.ToType(Microsoft.JScript.TypedArray.ToRankString(type.GetArrayRank()), this.ToReferenceContext(type.GetElementType()));
            }
            return this.JScriptReferenceModule.ResolveType(type.MetadataToken, null, null);
        }

        internal System.Type AllowPartiallyTrustedCallersAttribute
        {
            get
            {
                return typeof(System.Security.AllowPartiallyTrustedCallersAttribute);
            }
        }

        internal System.Type ArgumentsObject
        {
            get
            {
                return this.GetTypeReference(TypeReference.ArgumentsObject);
            }
        }

        internal System.Type Array
        {
            get
            {
                return typeof(System.Array);
            }
        }

        internal System.Type ArrayConstructor
        {
            get
            {
                return this.GetTypeReference(TypeReference.ArrayConstructor);
            }
        }

        internal System.Type ArrayObject
        {
            get
            {
                return this.GetTypeReference(TypeReference.ArrayObject);
            }
        }

        internal System.Type ArrayOfObject
        {
            get
            {
                return typeof(object[]);
            }
        }

        internal System.Type ArrayOfString
        {
            get
            {
                return typeof(string[]);
            }
        }

        internal System.Type ArrayWrapper
        {
            get
            {
                return this.GetTypeReference(TypeReference.ArrayWrapper);
            }
        }

        internal System.Type Attribute
        {
            get
            {
                return typeof(System.Attribute);
            }
        }

        internal System.Type AttributeUsageAttribute
        {
            get
            {
                return typeof(System.AttributeUsageAttribute);
            }
        }

        internal System.Type BaseVsaStartup
        {
            get
            {
                return this.GetTypeReference(TypeReference.BaseVsaStartup);
            }
        }

        internal System.Type Binding
        {
            get
            {
                return this.GetTypeReference(TypeReference.Binding);
            }
        }

        internal System.Type BitwiseBinary
        {
            get
            {
                return this.GetTypeReference(TypeReference.BitwiseBinary);
            }
        }

        internal ConstructorInfo bitwiseBinaryConstructor
        {
            get
            {
                return this.BitwiseBinary.GetConstructor(new System.Type[] { this.Int32 });
            }
        }

        internal System.Type Boolean
        {
            get
            {
                return typeof(bool);
            }
        }

        internal System.Type BooleanObject
        {
            get
            {
                return this.GetTypeReference(TypeReference.BooleanObject);
            }
        }

        internal System.Type BreakOutOfFinally
        {
            get
            {
                return this.GetTypeReference(TypeReference.BreakOutOfFinally);
            }
        }

        internal ConstructorInfo breakOutOfFinallyConstructor
        {
            get
            {
                return this.BreakOutOfFinally.GetConstructor(new System.Type[] { this.Int32 });
            }
        }

        internal System.Type BuiltinFunction
        {
            get
            {
                return this.GetTypeReference(TypeReference.BuiltinFunction);
            }
        }

        internal System.Type Byte
        {
            get
            {
                return typeof(byte);
            }
        }

        internal MethodInfo callMethod
        {
            get
            {
                return this.LateBinding.GetMethod("Call", new System.Type[] { this.ArrayOfObject, this.Boolean, this.Boolean, this.VsaEngine });
            }
        }

        internal MethodInfo callValue2Method
        {
            get
            {
                return this.LateBinding.GetMethod("CallValue2", new System.Type[] { this.Object, this.Object, this.ArrayOfObject, this.Boolean, this.Boolean, this.VsaEngine });
            }
        }

        internal MethodInfo callValueMethod
        {
            get
            {
                return this.LateBinding.GetMethod("CallValue", new System.Type[] { this.Object, this.Object, this.ArrayOfObject, this.Boolean, this.Boolean, this.VsaEngine });
            }
        }

        internal MethodInfo changeTypeMethod
        {
            get
            {
                return this.SystemConvert.GetMethod("ChangeType", new System.Type[] { this.Object, this.TypeCode });
            }
        }

        internal System.Type Char
        {
            get
            {
                return typeof(char);
            }
        }

        internal MethodInfo checkIfDoubleIsIntegerMethod
        {
            get
            {
                return this.Convert.GetMethod("CheckIfDoubleIsInteger");
            }
        }

        internal MethodInfo checkIfSingleIsIntegerMethod
        {
            get
            {
                return this.Convert.GetMethod("CheckIfSingleIsInteger");
            }
        }

        internal System.Type ClassScope
        {
            get
            {
                return this.GetTypeReference(TypeReference.ClassScope);
            }
        }

        internal System.Type Closure
        {
            get
            {
                return this.GetTypeReference(TypeReference.Closure);
            }
        }

        internal ConstructorInfo closureConstructor
        {
            get
            {
                return this.Closure.GetConstructor(new System.Type[] { this.FunctionObject });
            }
        }

        internal System.Reflection.FieldInfo closureInstanceField
        {
            get
            {
                return this.StackFrame.GetField("closureInstance");
            }
        }

        internal System.Type CLSCompliantAttribute
        {
            get
            {
                return typeof(System.CLSCompliantAttribute);
            }
        }

        internal ConstructorInfo clsCompliantAttributeCtor
        {
            get
            {
                return this.CLSCompliantAttribute.GetConstructor(new System.Type[] { this.Boolean });
            }
        }

        internal System.Type CoClassAttribute
        {
            get
            {
                return typeof(System.Runtime.InteropServices.CoClassAttribute);
            }
        }

        internal System.Type CodeAccessSecurityAttribute
        {
            get
            {
                return typeof(System.Security.Permissions.CodeAccessSecurityAttribute);
            }
        }

        internal MethodInfo coerce2Method
        {
            get
            {
                return this.Convert.GetMethod("Coerce2");
            }
        }

        internal MethodInfo coerceTMethod
        {
            get
            {
                return this.Convert.GetMethod("CoerceT");
            }
        }

        internal System.Type CompilerGlobalScopeAttribute
        {
            get
            {
                return typeof(System.Runtime.CompilerServices.CompilerGlobalScopeAttribute);
            }
        }

        internal ConstructorInfo compilerGlobalScopeAttributeCtor
        {
            get
            {
                return this.CompilerGlobalScopeAttribute.GetConstructor(new System.Type[0]);
            }
        }

        internal MethodInfo constructArrayMethod
        {
            get
            {
                return this.ArrayConstructor.GetMethod("ConstructArray");
            }
        }

        internal MethodInfo constructObjectMethod
        {
            get
            {
                return this.ObjectConstructor.GetMethod("ConstructObject");
            }
        }

        internal System.Reflection.FieldInfo contextEngineField
        {
            get
            {
                return this.Globals.GetField("contextEngine");
            }
        }

        internal System.Type ContextStaticAttribute
        {
            get
            {
                return typeof(System.ContextStaticAttribute);
            }
        }

        internal ConstructorInfo contextStaticAttributeCtor
        {
            get
            {
                return this.ContextStaticAttribute.GetConstructor(new System.Type[0]);
            }
        }

        internal System.Type ContinueOutOfFinally
        {
            get
            {
                return this.GetTypeReference(TypeReference.ContinueOutOfFinally);
            }
        }

        internal ConstructorInfo continueOutOfFinallyConstructor
        {
            get
            {
                return this.ContinueOutOfFinally.GetConstructor(new System.Type[] { this.Int32 });
            }
        }

        internal System.Type Convert
        {
            get
            {
                return this.GetTypeReference(TypeReference.Convert);
            }
        }

        internal MethodInfo convertCharToStringMethod
        {
            get
            {
                return this.SystemConvert.GetMethod("ToString", new System.Type[] { this.Char });
            }
        }

        internal MethodInfo createVsaEngine
        {
            get
            {
                return this.VsaEngine.GetMethod("CreateEngine", new System.Type[0]);
            }
        }

        internal MethodInfo createVsaEngineWithType
        {
            get
            {
                return this.VsaEngine.GetMethod("CreateEngineWithType", new System.Type[] { this.RuntimeTypeHandle });
            }
        }

        internal System.Type DateObject
        {
            get
            {
                return this.GetTypeReference(TypeReference.DateObject);
            }
        }

        internal System.Type DateTime
        {
            get
            {
                return typeof(System.DateTime);
            }
        }

        internal ConstructorInfo dateTimeConstructor
        {
            get
            {
                return this.DateTime.GetConstructor(new System.Type[] { this.Int64 });
            }
        }

        internal MethodInfo dateTimeToInt64Method
        {
            get
            {
                return this.DateTime.GetProperty("Ticks").GetGetMethod();
            }
        }

        internal MethodInfo dateTimeToStringMethod
        {
            get
            {
                return this.DateTime.GetMethod("ToString", new System.Type[0]);
            }
        }

        internal System.Type DBNull
        {
            get
            {
                return typeof(System.DBNull);
            }
        }

        internal MethodInfo debugBreak
        {
            get
            {
                return this.Debugger.GetMethod("Break", new System.Type[0]);
            }
        }

        internal System.Type DebuggableAttribute
        {
            get
            {
                return typeof(System.Diagnostics.DebuggableAttribute);
            }
        }

        internal System.Type Debugger
        {
            get
            {
                return typeof(System.Diagnostics.Debugger);
            }
        }

        internal System.Type DebuggerHiddenAttribute
        {
            get
            {
                return typeof(System.Diagnostics.DebuggerHiddenAttribute);
            }
        }

        internal ConstructorInfo debuggerHiddenAttributeCtor
        {
            get
            {
                return this.DebuggerHiddenAttribute.GetConstructor(new System.Type[0]);
            }
        }

        internal System.Type DebuggerStepThroughAttribute
        {
            get
            {
                return typeof(System.Diagnostics.DebuggerStepThroughAttribute);
            }
        }

        internal ConstructorInfo debuggerStepThroughAttributeCtor
        {
            get
            {
                return this.DebuggerStepThroughAttribute.GetConstructor(new System.Type[0]);
            }
        }

        internal System.Type Decimal
        {
            get
            {
                return typeof(decimal);
            }
        }

        internal MethodInfo decimalCompare
        {
            get
            {
                return this.Decimal.GetMethod("Compare", new System.Type[] { this.Decimal, this.Decimal });
            }
        }

        internal ConstructorInfo decimalConstructor
        {
            get
            {
                return this.Decimal.GetConstructor(new System.Type[] { this.Int32, this.Int32, this.Int32, this.Boolean, this.Byte });
            }
        }

        internal MethodInfo decimalToDoubleMethod
        {
            get
            {
                return this.Decimal.GetMethod("ToDouble", new System.Type[] { this.Decimal });
            }
        }

        internal MethodInfo decimalToInt32Method
        {
            get
            {
                return this.Decimal.GetMethod("ToInt32", new System.Type[] { this.Decimal });
            }
        }

        internal MethodInfo decimalToInt64Method
        {
            get
            {
                return this.Decimal.GetMethod("ToInt64", new System.Type[] { this.Decimal });
            }
        }

        internal MethodInfo decimalToStringMethod
        {
            get
            {
                return this.Decimal.GetMethod("ToString", new System.Type[0]);
            }
        }

        internal MethodInfo decimalToUInt32Method
        {
            get
            {
                return this.Decimal.GetMethod("ToUInt32", new System.Type[] { this.Decimal });
            }
        }

        internal MethodInfo decimalToUInt64Method
        {
            get
            {
                return this.Decimal.GetMethod("ToUInt64", new System.Type[] { this.Decimal });
            }
        }

        internal System.Reflection.FieldInfo decimalZeroField
        {
            get
            {
                return this.Decimal.GetField("Zero");
            }
        }

        internal System.Type DefaultMemberAttribute
        {
            get
            {
                return typeof(System.Reflection.DefaultMemberAttribute);
            }
        }

        internal ConstructorInfo defaultMemberAttributeCtor
        {
            get
            {
                return this.DefaultMemberAttribute.GetConstructor(new System.Type[] { this.String });
            }
        }

        internal System.Type Delegate
        {
            get
            {
                return typeof(System.Delegate);
            }
        }

        internal MethodInfo deleteMemberMethod
        {
            get
            {
                return this.LateBinding.GetMethod("DeleteMember");
            }
        }

        internal MethodInfo deleteMethod
        {
            get
            {
                return this.LateBinding.GetMethod("Delete");
            }
        }

        internal System.Type Double
        {
            get
            {
                return typeof(double);
            }
        }

        internal MethodInfo doubleToBooleanMethod
        {
            get
            {
                return this.Convert.GetMethod("ToBoolean", new System.Type[] { this.Double });
            }
        }

        internal MethodInfo doubleToDecimalMethod
        {
            get
            {
                return this.Decimal.GetMethod("op_Explicit", new System.Type[] { this.Double });
            }
        }

        internal MethodInfo doubleToInt64
        {
            get
            {
                return this.Runtime.GetMethod("DoubleToInt64");
            }
        }

        internal MethodInfo doubleToStringMethod
        {
            get
            {
                return this.Convert.GetMethod("ToString", new System.Type[] { this.Double });
            }
        }

        internal System.Type Empty
        {
            get
            {
                return this.GetTypeReference(TypeReference.Empty);
            }
        }

        internal System.Reflection.FieldInfo engineField
        {
            get
            {
                return this.ScriptObject.GetField("engine");
            }
        }

        internal System.Type Enum
        {
            get
            {
                return typeof(System.Enum);
            }
        }

        internal System.Type EnumeratorObject
        {
            get
            {
                return this.GetTypeReference(TypeReference.EnumeratorObject);
            }
        }

        internal System.Type Equality
        {
            get
            {
                return this.GetTypeReference(TypeReference.Equality);
            }
        }

        internal ConstructorInfo equalityConstructor
        {
            get
            {
                return this.Equality.GetConstructor(new System.Type[] { this.Int32 });
            }
        }

        internal MethodInfo equalsMethod
        {
            get
            {
                return this.Object.GetMethod("Equals", new System.Type[] { this.Object });
            }
        }

        internal System.Type ErrorObject
        {
            get
            {
                return this.GetTypeReference(TypeReference.ErrorObject);
            }
        }

        internal System.Type Eval
        {
            get
            {
                return this.GetTypeReference(TypeReference.Eval);
            }
        }

        internal System.Type EvalErrorObject
        {
            get
            {
                return this.GetTypeReference(TypeReference.EvalErrorObject);
            }
        }

        internal MethodInfo evaluateBitwiseBinaryMethod
        {
            get
            {
                return this.BitwiseBinary.GetMethod("EvaluateBitwiseBinary");
            }
        }

        internal MethodInfo evaluateEqualityMethod
        {
            get
            {
                return this.Equality.GetMethod("EvaluateEquality", new System.Type[] { this.Object, this.Object });
            }
        }

        internal MethodInfo evaluateNumericBinaryMethod
        {
            get
            {
                return this.NumericBinary.GetMethod("EvaluateNumericBinary");
            }
        }

        internal MethodInfo evaluatePlusMethod
        {
            get
            {
                return this.Plus.GetMethod("EvaluatePlus");
            }
        }

        internal MethodInfo evaluatePostOrPrefixOperatorMethod
        {
            get
            {
                return this.PostOrPrefixOperator.GetMethod("EvaluatePostOrPrefix");
            }
        }

        internal MethodInfo evaluateRelationalMethod
        {
            get
            {
                return this.Relational.GetMethod("EvaluateRelational");
            }
        }

        internal MethodInfo evaluateUnaryMethod
        {
            get
            {
                return this.NumericUnary.GetMethod("EvaluateUnary");
            }
        }

        internal System.Type EventInfo
        {
            get
            {
                return typeof(System.Reflection.EventInfo);
            }
        }

        internal System.Type Exception
        {
            get
            {
                return typeof(System.Exception);
            }
        }

        internal System.Type Expando
        {
            get
            {
                return this.GetTypeReference(TypeReference.Expando);
            }
        }

        internal MethodInfo fastConstructArrayLiteralMethod
        {
            get
            {
                return this.Globals.GetMethod("ConstructArrayLiteral");
            }
        }

        internal System.Type FieldAccessor
        {
            get
            {
                return this.GetTypeReference(TypeReference.FieldAccessor);
            }
        }

        internal System.Type FieldInfo
        {
            get
            {
                return typeof(System.Reflection.FieldInfo);
            }
        }

        internal System.Type ForIn
        {
            get
            {
                return this.GetTypeReference(TypeReference.ForIn);
            }
        }

        internal System.Type FunctionDeclaration
        {
            get
            {
                return this.GetTypeReference(TypeReference.FunctionDeclaration);
            }
        }

        internal System.Type FunctionExpression
        {
            get
            {
                return this.GetTypeReference(TypeReference.FunctionExpression);
            }
        }

        internal System.Type FunctionObject
        {
            get
            {
                return this.GetTypeReference(TypeReference.FunctionObject);
            }
        }

        internal System.Type FunctionWrapper
        {
            get
            {
                return this.GetTypeReference(TypeReference.FunctionWrapper);
            }
        }

        internal MethodInfo getCurrentMethod
        {
            get
            {
                return this.IEnumerator.GetProperty("Current", System.Type.EmptyTypes).GetGetMethod();
            }
        }

        internal MethodInfo getDefaultThisObjectMethod
        {
            get
            {
                return this.IActivationObject.GetMethod("GetDefaultThisObject");
            }
        }

        internal MethodInfo getEngineMethod
        {
            get
            {
                return this.INeedEngine.GetMethod("GetEngine");
            }
        }

        internal MethodInfo getEnumeratorMethod
        {
            get
            {
                return this.IEnumerable.GetMethod("GetEnumerator", System.Type.EmptyTypes);
            }
        }

        internal MethodInfo getFieldMethod
        {
            get
            {
                return this.IActivationObject.GetMethod("GetField", new System.Type[] { this.String, this.Int32 });
            }
        }

        internal MethodInfo getFieldValueMethod
        {
            get
            {
                return this.FieldInfo.GetMethod("GetValue", new System.Type[] { this.Object });
            }
        }

        internal MethodInfo getGlobalScopeMethod
        {
            get
            {
                return this.IActivationObject.GetMethod("GetGlobalScope");
            }
        }

        internal MethodInfo getLenientGlobalObjectMethod
        {
            get
            {
                return this.VsaEngine.GetProperty("LenientGlobalObject").GetGetMethod();
            }
        }

        internal MethodInfo getMemberValueMethod
        {
            get
            {
                return this.IActivationObject.GetMethod("GetMemberValue", new System.Type[] { this.String, this.Int32 });
            }
        }

        internal MethodInfo getMethodMethod
        {
            get
            {
                return this.Type.GetMethod("GetMethod", new System.Type[] { this.String });
            }
        }

        internal MethodInfo getNamespaceMethod
        {
            get
            {
                return this.Namespace.GetMethod("GetNamespace");
            }
        }

        internal MethodInfo getNonMissingValueMethod
        {
            get
            {
                return this.LateBinding.GetMethod("GetNonMissingValue");
            }
        }

        internal MethodInfo getOriginalArrayConstructorMethod
        {
            get
            {
                return this.VsaEngine.GetMethod("GetOriginalArrayConstructor");
            }
        }

        internal MethodInfo getOriginalObjectConstructorMethod
        {
            get
            {
                return this.VsaEngine.GetMethod("GetOriginalObjectConstructor");
            }
        }

        internal MethodInfo getOriginalRegExpConstructorMethod
        {
            get
            {
                return this.VsaEngine.GetMethod("GetOriginalRegExpConstructor");
            }
        }

        internal MethodInfo getParentMethod
        {
            get
            {
                return this.ScriptObject.GetMethod("GetParent");
            }
        }

        internal MethodInfo getTypeFromHandleMethod
        {
            get
            {
                return this.Type.GetMethod("GetTypeFromHandle", new System.Type[] { this.RuntimeTypeHandle });
            }
        }

        internal MethodInfo getTypeMethod
        {
            get
            {
                return this.Type.GetMethod("GetType", new System.Type[] { this.String });
            }
        }

        internal MethodInfo getValue2Method
        {
            get
            {
                return this.LateBinding.GetMethod("GetValue2");
            }
        }

        internal System.Type GlobalObject
        {
            get
            {
                return this.GetTypeReference(TypeReference.GlobalObject);
            }
        }

        internal System.Type Globals
        {
            get
            {
                return this.GetTypeReference(TypeReference.Globals);
            }
        }

        internal System.Type GlobalScope
        {
            get
            {
                return this.GetTypeReference(TypeReference.GlobalScope);
            }
        }

        internal ConstructorInfo globalScopeConstructor
        {
            get
            {
                return this.GlobalScope.GetConstructor(new System.Type[] { this.GlobalScope, this.VsaEngine });
            }
        }

        internal ConstructorInfo hashtableCtor
        {
            get
            {
                return this.SimpleHashtable.GetConstructor(new System.Type[] { this.UInt32 });
            }
        }

        internal MethodInfo hashTableGetEnumerator
        {
            get
            {
                return this.SimpleHashtable.GetMethod("GetEnumerator", System.Type.EmptyTypes);
            }
        }

        internal MethodInfo hashtableGetItem
        {
            get
            {
                return this.SimpleHashtable.GetMethod("get_Item", new System.Type[] { this.Object });
            }
        }

        internal MethodInfo hashtableRemove
        {
            get
            {
                return this.SimpleHashtable.GetMethod("Remove", new System.Type[] { this.Object });
            }
        }

        internal MethodInfo hashtableSetItem
        {
            get
            {
                return this.SimpleHashtable.GetMethod("set_Item", new System.Type[] { this.Object, this.Object });
            }
        }

        internal System.Type Hide
        {
            get
            {
                return this.GetTypeReference(TypeReference.Hide);
            }
        }

        internal System.Type IActivationObject
        {
            get
            {
                return this.GetTypeReference(TypeReference.IActivationObject);
            }
        }

        internal System.Type IConvertible
        {
            get
            {
                return typeof(System.IConvertible);
            }
        }

        internal System.Type IEnumerable
        {
            get
            {
                return typeof(System.Collections.IEnumerable);
            }
        }

        internal System.Type IEnumerator
        {
            get
            {
                return typeof(System.Collections.IEnumerator);
            }
        }

        internal System.Type IExpando
        {
            get
            {
                return typeof(System.Runtime.InteropServices.Expando.IExpando);
            }
        }

        internal System.Type IList
        {
            get
            {
                return typeof(System.Collections.IList);
            }
        }

        internal System.Type Import
        {
            get
            {
                return this.GetTypeReference(TypeReference.Import);
            }
        }

        internal System.Type In
        {
            get
            {
                return this.GetTypeReference(TypeReference.In);
            }
        }

        internal System.Type INeedEngine
        {
            get
            {
                return this.GetTypeReference(TypeReference.INeedEngine);
            }
        }

        internal System.Type Instanceof
        {
            get
            {
                return this.GetTypeReference(TypeReference.Instanceof);
            }
        }

        internal System.Type Int16
        {
            get
            {
                return typeof(short);
            }
        }

        internal System.Type Int32
        {
            get
            {
                return typeof(int);
            }
        }

        internal MethodInfo int32ToDecimalMethod
        {
            get
            {
                return this.Decimal.GetMethod("op_Implicit", new System.Type[] { this.Int32 });
            }
        }

        internal MethodInfo int32ToStringMethod
        {
            get
            {
                return this.Int32.GetMethod("ToString", new System.Type[0]);
            }
        }

        internal System.Type Int64
        {
            get
            {
                return typeof(long);
            }
        }

        internal MethodInfo int64ToDecimalMethod
        {
            get
            {
                return this.Decimal.GetMethod("op_Implicit", new System.Type[] { this.Int64 });
            }
        }

        internal MethodInfo int64ToStringMethod
        {
            get
            {
                return this.Int64.GetMethod("ToString", new System.Type[0]);
            }
        }

        internal System.Type IntPtr
        {
            get
            {
                return typeof(System.IntPtr);
            }
        }

        internal MethodInfo isMissingMethod
        {
            get
            {
                return this.Binding.GetMethod("IsMissing");
            }
        }

        internal MethodInfo jScriptCompareMethod
        {
            get
            {
                return this.Relational.GetMethod("JScriptCompare");
            }
        }

        internal MethodInfo jScriptEqualsMethod
        {
            get
            {
                return this.Equality.GetMethod("JScriptEquals");
            }
        }

        internal MethodInfo jScriptEvaluateMethod1
        {
            get
            {
                return this.Eval.GetMethod("JScriptEvaluate", new System.Type[] { this.Object, this.VsaEngine });
            }
        }

        internal MethodInfo jScriptEvaluateMethod2
        {
            get
            {
                return this.Eval.GetMethod("JScriptEvaluate", new System.Type[] { this.Object, this.Object, this.VsaEngine });
            }
        }

        internal System.Type JScriptException
        {
            get
            {
                return this.GetTypeReference(TypeReference.JScriptException);
            }
        }

        internal MethodInfo jScriptExceptionValueMethod
        {
            get
            {
                return this.Try.GetMethod("JScriptExceptionValue");
            }
        }

        internal MethodInfo jScriptFunctionDeclarationMethod
        {
            get
            {
                return this.FunctionDeclaration.GetMethod("JScriptFunctionDeclaration");
            }
        }

        internal MethodInfo jScriptFunctionExpressionMethod
        {
            get
            {
                return this.FunctionExpression.GetMethod("JScriptFunctionExpression");
            }
        }

        internal MethodInfo jScriptGetEnumeratorMethod
        {
            get
            {
                return this.ForIn.GetMethod("JScriptGetEnumerator");
            }
        }

        internal MethodInfo jScriptImportMethod
        {
            get
            {
                return this.Import.GetMethod("JScriptImport");
            }
        }

        internal MethodInfo jScriptInMethod
        {
            get
            {
                return this.In.GetMethod("JScriptIn");
            }
        }

        internal MethodInfo jScriptInstanceofMethod
        {
            get
            {
                return this.Instanceof.GetMethod("JScriptInstanceof");
            }
        }

        internal MethodInfo jScriptPackageMethod
        {
            get
            {
                return this.Package.GetMethod("JScriptPackage");
            }
        }

        private Module JScriptReferenceModule
        {
            get
            {
                return this._jscriptReferenceModule;
            }
        }

        internal MethodInfo jScriptStrictEqualsMethod
        {
            get
            {
                return this.StrictEquality.GetMethod("JScriptStrictEquals", new System.Type[] { this.Object, this.Object });
            }
        }

        internal MethodInfo jScriptThrowMethod
        {
            get
            {
                return this.Throw.GetMethod("JScriptThrow");
            }
        }

        internal MethodInfo jScriptTypeofMethod
        {
            get
            {
                return this.Typeof.GetMethod("JScriptTypeof");
            }
        }

        internal MethodInfo jScriptWithMethod
        {
            get
            {
                return this.With.GetMethod("JScriptWith");
            }
        }

        internal System.Type JSError
        {
            get
            {
                return this.GetTypeReference(TypeReference.JSError);
            }
        }

        internal System.Type JSFunctionAttribute
        {
            get
            {
                return this.GetTypeReference(TypeReference.JSFunctionAttribute);
            }
        }

        internal ConstructorInfo jsFunctionAttributeConstructor
        {
            get
            {
                return this.JSFunctionAttribute.GetConstructor(new System.Type[] { this.JSFunctionAttributeEnum });
            }
        }

        internal System.Type JSFunctionAttributeEnum
        {
            get
            {
                return this.GetTypeReference(TypeReference.JSFunctionAttributeEnum);
            }
        }

        internal System.Type JSLocalField
        {
            get
            {
                return this.GetTypeReference(TypeReference.JSLocalField);
            }
        }

        internal ConstructorInfo jsLocalFieldConstructor
        {
            get
            {
                return this.JSLocalField.GetConstructor(new System.Type[] { this.String, this.RuntimeTypeHandle, this.Int32 });
            }
        }

        internal System.Type JSObject
        {
            get
            {
                return this.GetTypeReference(TypeReference.JSObject);
            }
        }

        internal System.Type LateBinding
        {
            get
            {
                return this.GetTypeReference(TypeReference.LateBinding);
            }
        }

        internal ConstructorInfo lateBindingConstructor
        {
            get
            {
                return this.LateBinding.GetConstructor(new System.Type[] { this.String });
            }
        }

        internal ConstructorInfo lateBindingConstructor2
        {
            get
            {
                return this.LateBinding.GetConstructor(new System.Type[] { this.String, this.Object });
            }
        }

        internal System.Type LenientGlobalObject
        {
            get
            {
                return this.GetTypeReference(TypeReference.LenientGlobalObject);
            }
        }

        internal System.Reflection.FieldInfo localVarsField
        {
            get
            {
                return this.StackFrame.GetField("localVars");
            }
        }

        internal System.Type MathObject
        {
            get
            {
                return this.GetTypeReference(TypeReference.MathObject);
            }
        }

        internal System.Type MethodInvoker
        {
            get
            {
                return this.GetTypeReference(TypeReference.MethodInvoker);
            }
        }

        internal System.Type Missing
        {
            get
            {
                return this.GetTypeReference(TypeReference.Missing);
            }
        }

        internal System.Reflection.FieldInfo missingField
        {
            get
            {
                return this.Missing.GetField("Value");
            }
        }

        internal MethodInfo moveNextMethod
        {
            get
            {
                return this.IEnumerator.GetMethod("MoveNext", System.Type.EmptyTypes);
            }
        }

        internal System.Type Namespace
        {
            get
            {
                return this.GetTypeReference(TypeReference.Namespace);
            }
        }

        internal System.Type NotRecommended
        {
            get
            {
                return this.GetTypeReference(TypeReference.NotRecommended);
            }
        }

        internal System.Type NumberObject
        {
            get
            {
                return this.GetTypeReference(TypeReference.NumberObject);
            }
        }

        internal System.Type NumericBinary
        {
            get
            {
                return this.GetTypeReference(TypeReference.NumericBinary);
            }
        }

        internal ConstructorInfo numericBinaryConstructor
        {
            get
            {
                return this.NumericBinary.GetConstructor(new System.Type[] { this.Int32 });
            }
        }

        internal MethodInfo numericbinaryDoOpMethod
        {
            get
            {
                return this.NumericBinary.GetMethod("DoOp");
            }
        }

        internal System.Type NumericUnary
        {
            get
            {
                return this.GetTypeReference(TypeReference.NumericUnary);
            }
        }

        internal ConstructorInfo numericUnaryConstructor
        {
            get
            {
                return this.NumericUnary.GetConstructor(new System.Type[] { this.Int32 });
            }
        }

        internal System.Type Object
        {
            get
            {
                return typeof(object);
            }
        }

        internal System.Type ObjectConstructor
        {
            get
            {
                return this.GetTypeReference(TypeReference.ObjectConstructor);
            }
        }

        internal System.Reflection.FieldInfo objectField
        {
            get
            {
                return this.LateBinding.GetField("obj");
            }
        }

        internal System.Type ObsoleteAttribute
        {
            get
            {
                return typeof(System.ObsoleteAttribute);
            }
        }

        internal System.Type Override
        {
            get
            {
                return this.GetTypeReference(TypeReference.Override);
            }
        }

        internal System.Type Package
        {
            get
            {
                return this.GetTypeReference(TypeReference.Package);
            }
        }

        internal System.Type ParamArrayAttribute
        {
            get
            {
                return typeof(System.ParamArrayAttribute);
            }
        }

        internal System.Type Plus
        {
            get
            {
                return this.GetTypeReference(TypeReference.Plus);
            }
        }

        internal ConstructorInfo plusConstructor
        {
            get
            {
                return this.Plus.GetConstructor(new System.Type[0]);
            }
        }

        internal MethodInfo plusDoOpMethod
        {
            get
            {
                return this.Plus.GetMethod("DoOp");
            }
        }

        internal MethodInfo popScriptObjectMethod
        {
            get
            {
                return this.VsaEngine.GetMethod("PopScriptObject");
            }
        }

        internal ConstructorInfo postOrPrefixConstructor
        {
            get
            {
                return this.PostOrPrefixOperator.GetConstructor(new System.Type[] { this.Int32 });
            }
        }

        internal System.Type PostOrPrefixOperator
        {
            get
            {
                return this.GetTypeReference(TypeReference.PostOrPrefixOperator);
            }
        }

        internal MethodInfo pushScriptObjectMethod
        {
            get
            {
                return this.VsaEngine.GetMethod("PushScriptObject");
            }
        }

        internal MethodInfo pushStackFrameForMethod
        {
            get
            {
                return this.StackFrame.GetMethod("PushStackFrameForMethod");
            }
        }

        internal MethodInfo pushStackFrameForStaticMethod
        {
            get
            {
                return this.StackFrame.GetMethod("PushStackFrameForStaticMethod");
            }
        }

        internal System.Type RangeErrorObject
        {
            get
            {
                return this.GetTypeReference(TypeReference.RangeErrorObject);
            }
        }

        internal System.Type ReferenceAttribute
        {
            get
            {
                return this.GetTypeReference(TypeReference.ReferenceAttribute);
            }
        }

        internal ConstructorInfo referenceAttributeConstructor
        {
            get
            {
                return this.ReferenceAttribute.GetConstructor(new System.Type[] { this.String });
            }
        }

        internal System.Type ReferenceErrorObject
        {
            get
            {
                return this.GetTypeReference(TypeReference.ReferenceErrorObject);
            }
        }

        internal System.Type ReflectionMissing
        {
            get
            {
                return typeof(System.Reflection.Missing);
            }
        }

        internal MethodInfo regExpConstructMethod
        {
            get
            {
                return this.RegExpConstructor.GetMethod("Construct", new System.Type[] { this.String, this.Boolean, this.Boolean, this.Boolean });
            }
        }

        internal System.Type RegExpConstructor
        {
            get
            {
                return this.GetTypeReference(TypeReference.RegExpConstructor);
            }
        }

        internal System.Type RegExpObject
        {
            get
            {
                return this.GetTypeReference(TypeReference.RegExpObject);
            }
        }

        internal System.Type Relational
        {
            get
            {
                return this.GetTypeReference(TypeReference.Relational);
            }
        }

        internal ConstructorInfo relationalConstructor
        {
            get
            {
                return this.Relational.GetConstructor(new System.Type[] { this.Int32 });
            }
        }

        internal System.Type RequiredAttributeAttribute
        {
            get
            {
                return typeof(System.Runtime.CompilerServices.RequiredAttributeAttribute);
            }
        }

        internal System.Type ReturnOutOfFinally
        {
            get
            {
                return this.GetTypeReference(TypeReference.ReturnOutOfFinally);
            }
        }

        internal ConstructorInfo returnOutOfFinallyConstructor
        {
            get
            {
                return this.ReturnOutOfFinally.GetConstructor(new System.Type[0]);
            }
        }

        internal System.Type Runtime
        {
            get
            {
                return this.GetTypeReference(TypeReference.Runtime);
            }
        }

        internal System.Type RuntimeTypeHandle
        {
            get
            {
                return typeof(System.RuntimeTypeHandle);
            }
        }

        internal System.Type SByte
        {
            get
            {
                return typeof(sbyte);
            }
        }

        internal ConstructorInfo scriptExceptionConstructor
        {
            get
            {
                return this.JScriptException.GetConstructor(new System.Type[] { this.JSError });
            }
        }

        internal System.Type ScriptFunction
        {
            get
            {
                return this.GetTypeReference(TypeReference.ScriptFunction);
            }
        }

        internal System.Type ScriptObject
        {
            get
            {
                return this.GetTypeReference(TypeReference.ScriptObject);
            }
        }

        internal MethodInfo scriptObjectStackTopMethod
        {
            get
            {
                return this.VsaEngine.GetMethod("ScriptObjectStackTop");
            }
        }

        internal System.Type ScriptStream
        {
            get
            {
                return this.GetTypeReference(TypeReference.ScriptStream);
            }
        }

        internal MethodInfo setEngineMethod
        {
            get
            {
                return this.INeedEngine.GetMethod("SetEngine");
            }
        }

        internal MethodInfo setFieldValueMethod
        {
            get
            {
                return this.FieldInfo.GetMethod("SetValue", new System.Type[] { this.Object, this.Object });
            }
        }

        internal MethodInfo setIndexedPropertyValueStaticMethod
        {
            get
            {
                return this.LateBinding.GetMethod("SetIndexedPropertyValueStatic");
            }
        }

        internal MethodInfo setMemberValue2Method
        {
            get
            {
                return this.JSObject.GetMethod("SetMemberValue2", new System.Type[] { this.String, this.Object });
            }
        }

        internal MethodInfo setValueMethod
        {
            get
            {
                return this.LateBinding.GetMethod("SetValue");
            }
        }

        internal System.Type SimpleHashtable
        {
            get
            {
                return this.GetTypeReference(TypeReference.SimpleHashtable);
            }
        }

        internal System.Type Single
        {
            get
            {
                return typeof(float);
            }
        }

        internal System.Type StackFrame
        {
            get
            {
                return this.GetTypeReference(TypeReference.StackFrame);
            }
        }

        internal System.Type STAThreadAttribute
        {
            get
            {
                return typeof(System.STAThreadAttribute);
            }
        }

        internal System.Type StrictEquality
        {
            get
            {
                return this.GetTypeReference(TypeReference.StrictEquality);
            }
        }

        internal System.Type String
        {
            get
            {
                return typeof(string);
            }
        }

        internal MethodInfo stringConcat2Method
        {
            get
            {
                return this.String.GetMethod("Concat", new System.Type[] { this.String, this.String });
            }
        }

        internal MethodInfo stringConcat3Method
        {
            get
            {
                return this.String.GetMethod("Concat", new System.Type[] { this.String, this.String, this.String });
            }
        }

        internal MethodInfo stringConcat4Method
        {
            get
            {
                return this.String.GetMethod("Concat", new System.Type[] { this.String, this.String, this.String, this.String });
            }
        }

        internal MethodInfo stringConcatArrMethod
        {
            get
            {
                return this.String.GetMethod("Concat", new System.Type[] { this.ArrayOfString });
            }
        }

        internal MethodInfo stringEqualsMethod
        {
            get
            {
                return this.String.GetMethod("Equals", new System.Type[] { this.String, this.String });
            }
        }

        internal MethodInfo stringLengthMethod
        {
            get
            {
                return this.String.GetProperty("Length").GetGetMethod();
            }
        }

        internal System.Type StringObject
        {
            get
            {
                return this.GetTypeReference(TypeReference.StringObject);
            }
        }

        internal System.Type SyntaxErrorObject
        {
            get
            {
                return this.GetTypeReference(TypeReference.SyntaxErrorObject);
            }
        }

        internal System.Type SystemConvert
        {
            get
            {
                return typeof(System.Convert);
            }
        }

        internal System.Reflection.FieldInfo systemReflectionMissingField
        {
            get
            {
                return this.ReflectionMissing.GetField("Value");
            }
        }

        internal System.Type Throw
        {
            get
            {
                return this.GetTypeReference(TypeReference.Throw);
            }
        }

        internal MethodInfo throwTypeMismatch
        {
            get
            {
                return this.Convert.GetMethod("ThrowTypeMismatch");
            }
        }

        internal MethodInfo toBooleanMethod
        {
            get
            {
                return this.Convert.GetMethod("ToBoolean", new System.Type[] { this.Object, this.Boolean });
            }
        }

        internal MethodInfo toForInObjectMethod
        {
            get
            {
                return this.Convert.GetMethod("ToForInObject", new System.Type[] { this.Object, this.VsaEngine });
            }
        }

        internal MethodInfo toInt32Method
        {
            get
            {
                return this.Convert.GetMethod("ToInt32", new System.Type[] { this.Object });
            }
        }

        internal MethodInfo toNativeArrayMethod
        {
            get
            {
                return this.Convert.GetMethod("ToNativeArray");
            }
        }

        internal MethodInfo toNumberMethod
        {
            get
            {
                return this.Convert.GetMethod("ToNumber", new System.Type[] { this.Object });
            }
        }

        internal MethodInfo toObject2Method
        {
            get
            {
                return this.Convert.GetMethod("ToObject2", new System.Type[] { this.Object, this.VsaEngine });
            }
        }

        internal MethodInfo toObjectMethod
        {
            get
            {
                return this.Convert.GetMethod("ToObject", new System.Type[] { this.Object, this.VsaEngine });
            }
        }

        internal MethodInfo toStringMethod
        {
            get
            {
                return this.Convert.GetMethod("ToString", new System.Type[] { this.Object, this.Boolean });
            }
        }

        internal System.Type Try
        {
            get
            {
                return this.GetTypeReference(TypeReference.Try);
            }
        }

        internal System.Type Type
        {
            get
            {
                return typeof(System.Type);
            }
        }

        internal System.Type TypeCode
        {
            get
            {
                return typeof(System.TypeCode);
            }
        }

        internal System.Type TypedArray
        {
            get
            {
                return this.GetTypeReference(TypeReference.TypedArray);
            }
        }

        internal System.Type TypeErrorObject
        {
            get
            {
                return this.GetTypeReference(TypeReference.TypeErrorObject);
            }
        }

        internal System.Type Typeof
        {
            get
            {
                return this.GetTypeReference(TypeReference.Typeof);
            }
        }

        internal System.Type UInt16
        {
            get
            {
                return typeof(ushort);
            }
        }

        internal System.Type UInt32
        {
            get
            {
                return typeof(uint);
            }
        }

        internal MethodInfo uint32ToDecimalMethod
        {
            get
            {
                return this.Decimal.GetMethod("op_Implicit", new System.Type[] { this.UInt32 });
            }
        }

        internal MethodInfo uint32ToStringMethod
        {
            get
            {
                return this.UInt32.GetMethod("ToString", new System.Type[0]);
            }
        }

        internal System.Type UInt64
        {
            get
            {
                return typeof(ulong);
            }
        }

        internal MethodInfo uint64ToDecimalMethod
        {
            get
            {
                return this.Decimal.GetMethod("op_Implicit", new System.Type[] { this.UInt64 });
            }
        }

        internal MethodInfo uint64ToStringMethod
        {
            get
            {
                return this.UInt64.GetMethod("ToString", new System.Type[0]);
            }
        }

        internal System.Type UIntPtr
        {
            get
            {
                return typeof(System.UIntPtr);
            }
        }

        internal MethodInfo uncheckedDecimalToInt64Method
        {
            get
            {
                return this.Runtime.GetMethod("UncheckedDecimalToInt64");
            }
        }

        internal System.Reflection.FieldInfo undefinedField
        {
            get
            {
                return this.Empty.GetField("Value");
            }
        }

        internal System.Type URIErrorObject
        {
            get
            {
                return this.GetTypeReference(TypeReference.URIErrorObject);
            }
        }

        internal System.Type ValueType
        {
            get
            {
                return typeof(System.ValueType);
            }
        }

        internal System.Type VBArrayObject
        {
            get
            {
                return this.GetTypeReference(TypeReference.VBArrayObject);
            }
        }

        internal System.Type Void
        {
            get
            {
                return typeof(void);
            }
        }

        internal System.Type VsaEngine
        {
            get
            {
                return this.GetTypeReference(TypeReference.VsaEngine);
            }
        }

        internal ConstructorInfo vsaEngineConstructor
        {
            get
            {
                return this.VsaEngine.GetConstructor(new System.Type[0]);
            }
        }

        internal System.Type With
        {
            get
            {
                return this.GetTypeReference(TypeReference.With);
            }
        }

        internal MethodInfo writeLineMethod
        {
            get
            {
                return this.ScriptStream.GetMethod("WriteLine");
            }
        }

        internal MethodInfo writeMethod
        {
            get
            {
                return this.ScriptStream.GetMethod("Write");
            }
        }

        private enum TypeReference
        {
            ArgumentsObject,
            ArrayConstructor,
            ArrayObject,
            ArrayWrapper,
            Binding,
            BitwiseBinary,
            BooleanObject,
            BreakOutOfFinally,
            BuiltinFunction,
            ClassScope,
            Closure,
            ContinueOutOfFinally,
            Convert,
            DateObject,
            Empty,
            EnumeratorObject,
            Equality,
            ErrorObject,
            Eval,
            EvalErrorObject,
            Expando,
            FieldAccessor,
            ForIn,
            FunctionDeclaration,
            FunctionExpression,
            FunctionObject,
            FunctionWrapper,
            GlobalObject,
            GlobalScope,
            Globals,
            Hide,
            IActivationObject,
            INeedEngine,
            Import,
            In,
            Instanceof,
            JSError,
            JSFunctionAttribute,
            JSFunctionAttributeEnum,
            JSLocalField,
            JSObject,
            JScriptException,
            LateBinding,
            LenientGlobalObject,
            MathObject,
            MethodInvoker,
            Missing,
            Namespace,
            NotRecommended,
            NumberObject,
            NumericBinary,
            NumericUnary,
            ObjectConstructor,
            Override,
            Package,
            Plus,
            PostOrPrefixOperator,
            RangeErrorObject,
            ReferenceAttribute,
            ReferenceErrorObject,
            RegExpConstructor,
            RegExpObject,
            Relational,
            ReturnOutOfFinally,
            Runtime,
            ScriptFunction,
            ScriptObject,
            ScriptStream,
            SimpleHashtable,
            StackFrame,
            StrictEquality,
            StringObject,
            SyntaxErrorObject,
            Throw,
            Try,
            TypedArray,
            TypeErrorObject,
            Typeof,
            URIErrorObject,
            VBArrayObject,
            With,
            BaseVsaStartup,
            VsaEngine
        }
    }
}

