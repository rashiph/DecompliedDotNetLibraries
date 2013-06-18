namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Reflection.Emit;

    public sealed class FunctionObject : ScriptFunction
    {
        private int argumentsSlotNumber;
        internal MethodAttributes attributes;
        private Block body;
        private ConstructorBuilder cb;
        internal TypeBuilder classwriter;
        private CLSComplianceSpec clsCompliance;
        internal CustomAttributeList customAttributes;
        internal ScriptObject enclosing_scope;
        private LocalBuilder engineLocal;
        internal JSLocalField[] fields;
        internal string[] formal_parameters;
        internal Context funcContext;
        internal Globals globals;
        internal bool hasArgumentsObject;
        internal IReflect implementedIface;
        internal MethodInfo implementedIfaceMethod;
        internal bool isConstructor;
        internal bool isExpandoMethod;
        internal bool isImplicitCtor;
        internal bool isMethod;
        internal bool isStatic;
        private MethodBuilder mb;
        private MethodInfo method;
        internal bool must_save_stack_locals;
        internal bool noVersionSafeAttributeSpecified;
        internal FunctionScope own_scope;
        internal ParameterDeclaration[] parameter_declarations;
        private ParameterInfo[] parameterInfos;
        private bool partiallyEvaluated;
        internal TypeExpression return_type_expr;
        internal Label returnLabel;
        private ConstructorInfo superConstructor;
        internal ConstructorCall superConstructorCall;
        internal bool suppressIL;
        internal string text;

        internal FunctionObject(string name, ParameterDeclaration[] parameter_declarations, TypeExpression return_type_expr, Block body, FunctionScope own_scope, ScriptObject enclosing_scope, Context funcContext, MethodAttributes attributes) : this(name, parameter_declarations, return_type_expr, body, own_scope, enclosing_scope, funcContext, attributes, null, false)
        {
        }

        internal FunctionObject(Type t, string name, string method_name, string[] formal_parameters, JSLocalField[] fields, bool must_save_stack_locals, bool hasArgumentsObject, string text, VsaEngine engine) : base(engine.Globals.globalObject.originalFunction.originalPrototype, name, formal_parameters.Length)
        {
            base.engine = engine;
            this.formal_parameters = formal_parameters;
            this.argumentsSlotNumber = 0;
            this.body = null;
            this.method = TypeReflector.GetTypeReflectorFor(Globals.TypeRefs.ToReferenceContext(t)).GetMethod(method_name, BindingFlags.Public | BindingFlags.Static);
            this.parameterInfos = this.method.GetParameters();
            if (!Microsoft.JScript.CustomAttribute.IsDefined(this.method, typeof(JSFunctionAttribute), false))
            {
                this.isMethod = true;
            }
            else
            {
                JSFunctionAttributeEnum attributeValue = ((JSFunctionAttribute) Microsoft.JScript.CustomAttribute.GetCustomAttributes(this.method, typeof(JSFunctionAttribute), false)[0]).attributeValue;
                this.isExpandoMethod = (attributeValue & JSFunctionAttributeEnum.IsExpandoMethod) != JSFunctionAttributeEnum.None;
            }
            this.funcContext = null;
            this.own_scope = null;
            this.fields = fields;
            this.must_save_stack_locals = must_save_stack_locals;
            this.hasArgumentsObject = hasArgumentsObject;
            this.text = text;
            this.attributes = MethodAttributes.Public;
            this.globals = engine.Globals;
            this.superConstructor = null;
            this.superConstructorCall = null;
            this.enclosing_scope = this.globals.ScopeStack.Peek();
            base.noExpando = false;
            this.clsCompliance = CLSComplianceSpec.NotAttributed;
        }

        internal FunctionObject(string name, ParameterDeclaration[] parameter_declarations, TypeExpression return_type_expr, Block body, FunctionScope own_scope, ScriptObject enclosing_scope, Context funcContext, MethodAttributes attributes, CustomAttributeList customAttributes, bool isMethod) : base(body.Globals.globalObject.originalFunction.originalPrototype, name, parameter_declarations.Length)
        {
            this.parameter_declarations = parameter_declarations;
            int length = parameter_declarations.Length;
            this.formal_parameters = new string[length];
            for (int i = 0; i < length; i++)
            {
                this.formal_parameters[i] = parameter_declarations[i].identifier;
            }
            this.argumentsSlotNumber = 0;
            this.return_type_expr = return_type_expr;
            if (this.return_type_expr != null)
            {
                own_scope.AddReturnValueField();
            }
            this.body = body;
            this.method = null;
            this.parameterInfos = null;
            this.funcContext = funcContext;
            this.own_scope = own_scope;
            this.own_scope.owner = this;
            if ((!(enclosing_scope is ActivationObject) || !((ActivationObject) enclosing_scope).fast) && !isMethod)
            {
                this.argumentsSlotNumber = this.own_scope.GetNextSlotNumber();
                JSLocalField field = (JSLocalField) this.own_scope.AddNewField("arguments", null, FieldAttributes.PrivateScope);
                field.type = new TypeExpression(new ConstantWrapper(Typeob.Object, funcContext));
                field.isDefined = true;
                this.hasArgumentsObject = true;
            }
            else
            {
                this.hasArgumentsObject = false;
            }
            this.implementedIface = null;
            this.implementedIfaceMethod = null;
            this.isMethod = isMethod;
            this.isExpandoMethod = (customAttributes != null) && customAttributes.ContainsExpandoAttribute();
            this.isStatic = this.own_scope.isStatic = (attributes & MethodAttributes.Static) != MethodAttributes.PrivateScope;
            this.suppressIL = false;
            this.noVersionSafeAttributeSpecified = true;
            this.fields = this.own_scope.GetLocalFields();
            this.enclosing_scope = enclosing_scope;
            this.must_save_stack_locals = false;
            this.text = null;
            this.mb = null;
            this.cb = null;
            this.attributes = attributes;
            if (!this.isStatic)
            {
                this.attributes |= MethodAttributes.HideBySig;
            }
            this.globals = body.Globals;
            this.superConstructor = null;
            this.superConstructorCall = null;
            this.customAttributes = customAttributes;
            base.noExpando = false;
            this.clsCompliance = CLSComplianceSpec.NotAttributed;
            this.engineLocal = null;
            this.partiallyEvaluated = false;
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal override object Call(object[] args, object thisob)
        {
            return this.Call(args, thisob, null, null);
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal override object Call(object[] args, object thisob, Binder binder, CultureInfo culture)
        {
            object obj2;
            if (this.body == null)
            {
                return this.Call(args, thisob, this.enclosing_scope, new Closure(this), binder, culture);
            }
            Microsoft.JScript.StackFrame item = new Microsoft.JScript.StackFrame((thisob is JSObject) ? ((JSObject) thisob) : this.enclosing_scope, this.fields, new object[this.fields.Length], thisob);
            if (this.isConstructor)
            {
                item.closureInstance = thisob;
                if (this.superConstructor != null)
                {
                    if (this.superConstructorCall == null)
                    {
                        if (this.superConstructor is JSConstructor)
                        {
                            this.superConstructor.Invoke(thisob, new object[0]);
                        }
                    }
                    else
                    {
                        ASTList arguments = this.superConstructorCall.arguments;
                        int count = arguments.count;
                        object[] parameters = new object[count];
                        for (int i = 0; i < count; i++)
                        {
                            parameters[i] = arguments[i].Evaluate();
                        }
                        this.superConstructor.Invoke(thisob, BindingFlags.Default, binder, parameters, culture);
                    }
                }
                this.globals.ScopeStack.GuardedPush((thisob is JSObject) ? ((JSObject) thisob) : this.enclosing_scope);
                try
                {
                    ((ClassScope) this.enclosing_scope).owner.body.EvaluateInstanceVariableInitializers();
                }
                finally
                {
                    this.globals.ScopeStack.Pop();
                }
            }
            else if (this.isMethod && !this.isStatic)
            {
                if (!((ClassScope) this.enclosing_scope).HasInstance(thisob))
                {
                    throw new JScriptException(JSError.TypeMismatch);
                }
                item.closureInstance = thisob;
            }
            this.globals.ScopeStack.GuardedPush(item);
            try
            {
                this.own_scope.CloseNestedFunctions(item);
                this.ConvertArguments(args, item.localVars, 0, args.Length, this.formal_parameters.Length, binder, culture);
                Completion completion = (Completion) this.body.Evaluate();
                if (completion.Return)
                {
                    return completion.value;
                }
                obj2 = null;
            }
            finally
            {
                this.globals.ScopeStack.Pop();
            }
            return obj2;
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal override object Call(object[] args, object thisob, ScriptObject enclosing_scope, Closure calleeClosure, Binder binder, CultureInfo culture)
        {
            object obj7;
            if (this.body != null)
            {
                return this.CallASTFunc(args, thisob, enclosing_scope, calleeClosure, binder, culture);
            }
            object caller = calleeClosure.caller;
            calleeClosure.caller = this.globals.caller;
            this.globals.caller = calleeClosure;
            object arguments = calleeClosure.arguments;
            ScriptObject obj4 = this.globals.ScopeStack.Peek();
            ArgumentsObject obj5 = (obj4 is Microsoft.JScript.StackFrame) ? ((Microsoft.JScript.StackFrame) obj4).caller_arguments : null;
            Microsoft.JScript.StackFrame item = new Microsoft.JScript.StackFrame(enclosing_scope, this.fields, this.must_save_stack_locals ? new object[this.fields.Length] : null, thisob);
            this.globals.ScopeStack.GuardedPush(item);
            ArgumentsObject obj6 = new ArgumentsObject(this.globals.globalObject.originalObjectPrototype, args, this, calleeClosure, item, obj5);
            item.caller_arguments = obj6;
            calleeClosure.arguments = obj6;
            try
            {
                int length = this.formal_parameters.Length;
                int num2 = args.Length;
                if (this.hasArgumentsObject)
                {
                    object[] objArray = new object[length + 3];
                    objArray[0] = thisob;
                    objArray[1] = base.engine;
                    objArray[2] = obj6;
                    this.ConvertArguments(args, objArray, 3, num2, length, binder, culture);
                    return this.method.Invoke(thisob, BindingFlags.SuppressChangeType, null, objArray, null);
                }
                if (!this.isMethod)
                {
                    object[] objArray2 = new object[length + 2];
                    objArray2[0] = thisob;
                    objArray2[1] = base.engine;
                    this.ConvertArguments(args, objArray2, 2, num2, length, binder, culture);
                    return this.method.Invoke(thisob, BindingFlags.SuppressChangeType, null, objArray2, null);
                }
                if (length == num2)
                {
                    this.ConvertArguments(args, args, 0, num2, length, binder, culture);
                    return this.method.Invoke(thisob, BindingFlags.SuppressChangeType, null, args, null);
                }
                object[] newargs = new object[length];
                this.ConvertArguments(args, newargs, 0, num2, length, binder, culture);
                obj7 = this.method.Invoke(thisob, BindingFlags.SuppressChangeType, null, newargs, null);
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException;
            }
            finally
            {
                this.globals.ScopeStack.Pop();
                calleeClosure.arguments = arguments;
                this.globals.caller = calleeClosure.caller;
                calleeClosure.caller = caller;
            }
            return obj7;
        }

        private object CallASTFunc(object[] args, object thisob, ScriptObject enclosing_scope, Closure calleeClosure, Binder binder, CultureInfo culture)
        {
            object obj7;
            object caller = calleeClosure.caller;
            calleeClosure.caller = this.globals.caller;
            this.globals.caller = calleeClosure;
            object arguments = calleeClosure.arguments;
            ScriptObject obj4 = this.globals.ScopeStack.Peek();
            ArgumentsObject obj5 = (obj4 is Microsoft.JScript.StackFrame) ? ((Microsoft.JScript.StackFrame) obj4).caller_arguments : null;
            Microsoft.JScript.StackFrame item = new Microsoft.JScript.StackFrame(enclosing_scope, this.fields, new object[this.fields.Length], thisob);
            if (this.isMethod && !this.isStatic)
            {
                item.closureInstance = thisob;
            }
            this.globals.ScopeStack.GuardedPush(item);
            try
            {
                this.own_scope.CloseNestedFunctions(item);
                ArgumentsObject obj6 = null;
                if (this.hasArgumentsObject)
                {
                    obj6 = new ArgumentsObject(this.globals.globalObject.originalObjectPrototype, args, this, calleeClosure, item, obj5);
                    item.localVars[this.argumentsSlotNumber] = obj6;
                }
                item.caller_arguments = obj6;
                calleeClosure.arguments = obj6;
                this.ConvertArguments(args, item.localVars, 0, args.Length, this.formal_parameters.Length, binder, culture);
                Completion completion = (Completion) this.body.Evaluate();
                if (completion.Return)
                {
                    return completion.value;
                }
                obj7 = null;
            }
            finally
            {
                this.globals.ScopeStack.Pop();
                calleeClosure.arguments = arguments;
                this.globals.caller = calleeClosure.caller;
                calleeClosure.caller = caller;
            }
            return obj7;
        }

        internal void CheckCLSCompliance(bool classIsCLSCompliant)
        {
            if (classIsCLSCompliant)
            {
                if (this.clsCompliance != CLSComplianceSpec.NonCLSCompliant)
                {
                    int index = 0;
                    int length = this.parameter_declarations.Length;
                    while (index < length)
                    {
                        IReflect parameterIReflect = this.parameter_declarations[index].ParameterIReflect;
                        if ((parameterIReflect != null) && !TypeExpression.TypeIsCLSCompliant(parameterIReflect))
                        {
                            this.clsCompliance = CLSComplianceSpec.NonCLSCompliant;
                            this.funcContext.HandleError(JSError.NonCLSCompliantMember);
                            return;
                        }
                        index++;
                    }
                    if ((this.return_type_expr != null) && !this.return_type_expr.IsCLSCompliant())
                    {
                        this.clsCompliance = CLSComplianceSpec.NonCLSCompliant;
                        this.funcContext.HandleError(JSError.NonCLSCompliantMember);
                    }
                }
            }
            else if (this.clsCompliance == CLSComplianceSpec.CLSCompliant)
            {
                this.funcContext.HandleError(JSError.MemberTypeCLSCompliantMismatch);
            }
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal object Construct(JSObject thisob, object[] args)
        {
            JSObject obj2 = new JSObject(null, false);
            obj2.SetParent(base.GetPrototypeForConstructedObject());
            obj2.outer_class_instance = thisob;
            object obj3 = this.Call(args, obj2);
            if (obj3 is ScriptObject)
            {
                return obj3;
            }
            return obj2;
        }

        private void ConvertArguments(object[] args, object[] newargs, int offset, int length, int n, Binder binder, CultureInfo culture)
        {
            ParameterInfo[] parameterInfos = this.parameterInfos;
            if (parameterInfos != null)
            {
                int num = 0;
                for (int i = offset; num < n; i++)
                {
                    Type parameterType = parameterInfos[i].ParameterType;
                    if ((num == (n - 1)) && Microsoft.JScript.CustomAttribute.IsDefined(parameterInfos[i], typeof(ParamArrayAttribute), false))
                    {
                        int num3 = length - num;
                        if (num3 < 0)
                        {
                            num3 = 0;
                        }
                        newargs[i] = CopyToNewParamArray(parameterType.GetElementType(), num3, args, num, binder, culture);
                        return;
                    }
                    object obj2 = (num < length) ? args[num] : null;
                    if (parameterType == Typeob.Object)
                    {
                        newargs[i] = obj2;
                    }
                    else if (binder != null)
                    {
                        newargs[i] = binder.ChangeType(obj2, parameterType, culture);
                    }
                    else
                    {
                        newargs[i] = Microsoft.JScript.Convert.CoerceT(obj2, parameterType);
                    }
                    num++;
                }
            }
            else
            {
                ParameterDeclaration[] declarationArray = this.parameter_declarations;
                int index = 0;
                for (int j = offset; index < n; j++)
                {
                    IReflect parameterIReflect = declarationArray[index].ParameterIReflect;
                    if ((index == (n - 1)) && Microsoft.JScript.CustomAttribute.IsDefined(declarationArray[j], typeof(ParamArrayAttribute), false))
                    {
                        int num6 = length - index;
                        if (num6 < 0)
                        {
                            num6 = 0;
                        }
                        newargs[j] = CopyToNewParamArray(((TypedArray) parameterIReflect).elementType, num6, args, index);
                        return;
                    }
                    object obj3 = (index < length) ? args[index] : null;
                    if (parameterIReflect == Typeob.Object)
                    {
                        newargs[j] = obj3;
                    }
                    else if (parameterIReflect is ClassScope)
                    {
                        newargs[j] = Microsoft.JScript.Convert.Coerce(obj3, parameterIReflect);
                    }
                    else if (binder != null)
                    {
                        newargs[j] = binder.ChangeType(obj3, Microsoft.JScript.Convert.ToType(parameterIReflect), culture);
                    }
                    else
                    {
                        newargs[j] = Microsoft.JScript.Convert.CoerceT(obj3, Microsoft.JScript.Convert.ToType(parameterIReflect));
                    }
                    index++;
                }
            }
        }

        private static object[] CopyToNewParamArray(IReflect ir, int n, object[] args, int offset)
        {
            object[] objArray = new object[n];
            for (int i = 0; i < n; i++)
            {
                objArray[i] = Microsoft.JScript.Convert.Coerce(args[i + offset], ir);
            }
            return objArray;
        }

        private static Array CopyToNewParamArray(Type t, int n, object[] args, int offset, Binder binder, CultureInfo culture)
        {
            Array array = Array.CreateInstance(t, n);
            for (int i = 0; i < n; i++)
            {
                array.SetValue(binder.ChangeType(args[i + offset], t, culture), i);
            }
            return array;
        }

        internal void EmitLastLineInfo(ILGenerator il)
        {
            if (!this.isImplicitCtor)
            {
                int endLine = this.body.context.EndLine;
                int endColumn = this.body.context.EndColumn;
                this.body.context.document.EmitLineInfo(il, endLine, endColumn, endLine, endColumn + 1);
            }
        }

        internal ConstructorInfo GetConstructorInfo(CompilerGlobals compilerGlobals)
        {
            return (ConstructorInfo) this.GetMethodBase(compilerGlobals);
        }

        internal MethodBase GetMethodBase(CompilerGlobals compilerGlobals)
        {
            if (this.mb != null)
            {
                return this.mb;
            }
            if (this.cb == null)
            {
                JSFunctionAttributeEnum none = JSFunctionAttributeEnum.None;
                int num = 3;
                if (this.isMethod)
                {
                    if (this.isConstructor && (((ClassScope) this.enclosing_scope).outerClassField != null))
                    {
                        num = 1;
                        none |= JSFunctionAttributeEnum.IsInstanceNestedClassConstructor;
                    }
                    else
                    {
                        num = 0;
                    }
                }
                else if (!this.hasArgumentsObject)
                {
                    num = 2;
                }
                int iSequence = this.formal_parameters.Length + num;
                Type[] parameterTypes = new Type[iSequence];
                Type returnType = Microsoft.JScript.Convert.ToType(this.ReturnType(null));
                if (num > 0)
                {
                    if (this.isConstructor)
                    {
                        parameterTypes[iSequence - 1] = ((ClassScope) this.enclosing_scope).outerClassField.FieldType;
                    }
                    else
                    {
                        parameterTypes[0] = Typeob.Object;
                    }
                    none |= JSFunctionAttributeEnum.HasThisObject;
                }
                if (num > 1)
                {
                    parameterTypes[1] = Typeob.VsaEngine;
                    none |= JSFunctionAttributeEnum.HasEngine;
                }
                if (num > 2)
                {
                    parameterTypes[2] = Typeob.Object;
                    none |= JSFunctionAttributeEnum.HasArguments;
                }
                if (this.must_save_stack_locals)
                {
                    none |= JSFunctionAttributeEnum.HasStackFrame;
                }
                if (this.isExpandoMethod)
                {
                    none |= JSFunctionAttributeEnum.IsExpandoMethod;
                }
                if (this.isConstructor)
                {
                    for (int j = 0; j < (iSequence - num); j++)
                    {
                        parameterTypes[j] = this.parameter_declarations[j].ParameterType;
                    }
                }
                else
                {
                    for (int k = num; k < iSequence; k++)
                    {
                        parameterTypes[k] = this.parameter_declarations[k - num].ParameterType;
                    }
                }
                if (this.enclosing_scope is ClassScope)
                {
                    if (this.isConstructor)
                    {
                        this.cb = ((ClassScope) this.enclosing_scope).GetTypeBuilder().DefineConstructor(this.attributes & MethodAttributes.MemberAccessMask, CallingConventions.Standard, parameterTypes);
                    }
                    else
                    {
                        string name = base.name;
                        if (this.implementedIfaceMethod != null)
                        {
                            JSMethod implementedIfaceMethod = this.implementedIfaceMethod as JSMethod;
                            if (implementedIfaceMethod != null)
                            {
                                this.implementedIfaceMethod = implementedIfaceMethod.GetMethodInfo(compilerGlobals);
                            }
                            name = this.implementedIfaceMethod.DeclaringType.FullName + "." + name;
                        }
                        TypeBuilder typeBuilder = ((ClassScope) this.enclosing_scope).GetTypeBuilder();
                        if (this.mb != null)
                        {
                            return this.mb;
                        }
                        this.mb = typeBuilder.DefineMethod(name, this.attributes, returnType, parameterTypes);
                        if (this.implementedIfaceMethod != null)
                        {
                            ((ClassScope) this.enclosing_scope).GetTypeBuilder().DefineMethodOverride(this.mb, this.implementedIfaceMethod);
                        }
                    }
                }
                else
                {
                    if (this.enclosing_scope is FunctionScope)
                    {
                        if (((FunctionScope) this.enclosing_scope).owner != null)
                        {
                            base.name = ((FunctionScope) this.enclosing_scope).owner.name + "." + base.name;
                            none |= JSFunctionAttributeEnum.IsNested;
                        }
                        else
                        {
                            for (ScriptObject obj2 = this.enclosing_scope; obj2 != null; obj2 = obj2.GetParent())
                            {
                                if ((obj2 is FunctionScope) && (((FunctionScope) obj2).owner != null))
                                {
                                    base.name = ((FunctionScope) obj2).owner.name + "." + base.name;
                                    none |= JSFunctionAttributeEnum.IsNested;
                                    break;
                                }
                            }
                        }
                    }
                    if (compilerGlobals.usedNames[base.name] != null)
                    {
                        base.name = base.name + ":" + compilerGlobals.usedNames.count.ToString(CultureInfo.InvariantCulture);
                    }
                    compilerGlobals.usedNames[base.name] = this;
                    ScriptObject parent = this.enclosing_scope;
                    while ((parent != null) && !(parent is ClassScope))
                    {
                        parent = parent.GetParent();
                    }
                    this.classwriter = (parent == null) ? compilerGlobals.globalScopeClassWriter : compilerGlobals.classwriter;
                    this.mb = this.classwriter.DefineMethod(base.name, this.attributes, returnType, parameterTypes);
                }
                if (num > 0)
                {
                    if (this.mb != null)
                    {
                        this.mb.DefineParameter(1, ParameterAttributes.None, "this");
                    }
                    else
                    {
                        this.cb.DefineParameter(iSequence, ParameterAttributes.None, "this").SetConstant(null);
                        num = 0;
                        iSequence--;
                    }
                }
                if (num > 1)
                {
                    this.mb.DefineParameter(2, ParameterAttributes.None, "vsa Engine");
                }
                if (num > 2)
                {
                    this.mb.DefineParameter(3, ParameterAttributes.None, "arguments");
                }
                for (int i = num; i < iSequence; i++)
                {
                    ParameterBuilder builder3 = (this.mb != null) ? this.mb.DefineParameter(i + 1, ParameterAttributes.None, this.parameter_declarations[i - num].identifier) : this.cb.DefineParameter(i + 1, ParameterAttributes.None, this.parameter_declarations[i - num].identifier);
                    CustomAttributeList customAttributes = this.parameter_declarations[i - num].customAttributes;
                    if (customAttributes != null)
                    {
                        CustomAttributeBuilder[] customAttributeBuilders = customAttributes.GetCustomAttributeBuilders(false);
                        for (int m = 0; m < customAttributeBuilders.Length; m++)
                        {
                            builder3.SetCustomAttribute(customAttributeBuilders[m]);
                        }
                    }
                }
                if (none > JSFunctionAttributeEnum.None)
                {
                    CustomAttributeBuilder customBuilder = new CustomAttributeBuilder(CompilerGlobals.jsFunctionAttributeConstructor, new object[] { none });
                    if (this.mb != null)
                    {
                        this.mb.SetCustomAttribute(customBuilder);
                    }
                    else
                    {
                        this.cb.SetCustomAttribute(customBuilder);
                    }
                }
                if (this.customAttributes != null)
                {
                    CustomAttributeBuilder[] builderArray2 = this.customAttributes.GetCustomAttributeBuilders(false);
                    for (int n = 0; n < builderArray2.Length; n++)
                    {
                        if (this.mb != null)
                        {
                            this.mb.SetCustomAttribute(builderArray2[n]);
                        }
                        else
                        {
                            this.cb.SetCustomAttribute(builderArray2[n]);
                        }
                    }
                }
                if (this.clsCompliance == CLSComplianceSpec.CLSCompliant)
                {
                    if (this.mb != null)
                    {
                        this.mb.SetCustomAttribute(new CustomAttributeBuilder(CompilerGlobals.clsCompliantAttributeCtor, new object[] { true }));
                    }
                    else
                    {
                        this.cb.SetCustomAttribute(new CustomAttributeBuilder(CompilerGlobals.clsCompliantAttributeCtor, new object[] { true }));
                    }
                }
                else if (this.clsCompliance == CLSComplianceSpec.NonCLSCompliant)
                {
                    if (this.mb != null)
                    {
                        this.mb.SetCustomAttribute(new CustomAttributeBuilder(CompilerGlobals.clsCompliantAttributeCtor, new object[] { false }));
                    }
                    else
                    {
                        this.cb.SetCustomAttribute(new CustomAttributeBuilder(CompilerGlobals.clsCompliantAttributeCtor, new object[] { false }));
                    }
                }
                if (this.mb != null)
                {
                    this.mb.InitLocals = true;
                    return this.mb;
                }
                this.cb.InitLocals = true;
            }
            return this.cb;
        }

        internal MethodInfo GetMethodInfo(CompilerGlobals compilerGlobals)
        {
            return (MethodInfo) this.GetMethodBase(compilerGlobals);
        }

        internal string GetName()
        {
            return base.name;
        }

        internal override int GetNumberOfFormalParameters()
        {
            return this.formal_parameters.Length;
        }

        private bool IsNestedFunctionField(JSLocalField field)
        {
            return ((field.value != null) && (field.value is FunctionObject));
        }

        private static bool IsPresentIn(FieldInfo field, FieldInfo[] fields)
        {
            int index = 0;
            int length = fields.Length;
            while (index < length)
            {
                if (field == fields[index])
                {
                    return true;
                }
                index++;
            }
            return false;
        }

        internal void PartiallyEvaluate()
        {
            if (!this.partiallyEvaluated)
            {
                ClassScope scope = this.enclosing_scope as ClassScope;
                if (scope != null)
                {
                    scope.owner.PartiallyEvaluate();
                }
                if (!this.partiallyEvaluated)
                {
                    this.partiallyEvaluated = true;
                    this.clsCompliance = CLSComplianceSpec.NotAttributed;
                    if (this.customAttributes != null)
                    {
                        this.customAttributes.PartiallyEvaluate();
                        Microsoft.JScript.CustomAttribute elem = this.customAttributes.GetAttribute(Typeob.CLSCompliantAttribute);
                        if (elem != null)
                        {
                            this.clsCompliance = elem.GetCLSComplianceValue();
                            this.customAttributes.Remove(elem);
                        }
                        elem = this.customAttributes.GetAttribute(Typeob.Override);
                        if (elem != null)
                        {
                            if (this.isStatic)
                            {
                                elem.context.HandleError(JSError.StaticMethodsCannotOverride);
                            }
                            else
                            {
                                this.attributes &= ~MethodAttributes.NewSlot;
                            }
                            this.noVersionSafeAttributeSpecified = false;
                            this.customAttributes.Remove(elem);
                        }
                        elem = this.customAttributes.GetAttribute(Typeob.Hide);
                        if (elem != null)
                        {
                            if (!this.noVersionSafeAttributeSpecified)
                            {
                                elem.context.HandleError(JSError.OverrideAndHideUsedTogether);
                                this.attributes |= MethodAttributes.NewSlot;
                                this.noVersionSafeAttributeSpecified = true;
                            }
                            else
                            {
                                if (this.isStatic)
                                {
                                    elem.context.HandleError(JSError.StaticMethodsCannotHide);
                                }
                                this.noVersionSafeAttributeSpecified = false;
                            }
                            this.customAttributes.Remove(elem);
                        }
                        Microsoft.JScript.CustomAttribute attribute = this.customAttributes.GetAttribute(Typeob.Expando);
                        if (attribute != null)
                        {
                            if (!this.noVersionSafeAttributeSpecified && ((this.attributes & MethodAttributes.NewSlot) == MethodAttributes.PrivateScope))
                            {
                                attribute.context.HandleError(JSError.ExpandoPrecludesOverride);
                                this.attributes |= MethodAttributes.NewSlot;
                                this.noVersionSafeAttributeSpecified = true;
                            }
                            if (this.isConstructor)
                            {
                                attribute.context.HandleError(JSError.NotValidForConstructor);
                            }
                            else if ((this.attributes & MethodAttributes.Abstract) != MethodAttributes.PrivateScope)
                            {
                                attribute.context.HandleError(JSError.ExpandoPrecludesAbstract);
                            }
                            else if ((this.attributes & MethodAttributes.Static) != MethodAttributes.PrivateScope)
                            {
                                attribute.context.HandleError(JSError.ExpandoPrecludesStatic);
                            }
                            else if ((this.attributes & MethodAttributes.MemberAccessMask) != MethodAttributes.Public)
                            {
                                attribute.context.HandleError(JSError.ExpandoMustBePublic);
                            }
                            else
                            {
                                this.own_scope.isMethod = false;
                                this.isMethod = false;
                                this.isExpandoMethod = true;
                                this.isStatic = true;
                                this.attributes &= ~MethodAttributes.Virtual;
                                this.attributes &= ~MethodAttributes.NewSlot;
                                this.attributes |= MethodAttributes.Static;
                            }
                        }
                    }
                    int index = 0;
                    int length = this.parameter_declarations.Length;
                    while (index < length)
                    {
                        this.parameter_declarations[index].PartiallyEvaluate();
                        JSLocalField field = (JSLocalField) this.own_scope.name_table[this.formal_parameters[index]];
                        field.type = this.parameter_declarations[index].type;
                        if (field.type == null)
                        {
                            field.type = new TypeExpression(new ConstantWrapper(Typeob.Object, this.parameter_declarations[index].context));
                        }
                        field.isDefined = true;
                        index++;
                    }
                    if (this.return_type_expr != null)
                    {
                        this.return_type_expr.PartiallyEvaluate();
                        this.own_scope.returnVar.type = this.return_type_expr;
                        if (this.own_scope.returnVar.type.ToIReflect() == Typeob.Void)
                        {
                            this.own_scope.returnVar.type = null;
                            this.own_scope.returnVar = null;
                        }
                    }
                    this.globals.ScopeStack.Push(this.own_scope);
                    if (!this.own_scope.isKnownAtCompileTime)
                    {
                        int num3 = 0;
                        int num4 = this.fields.Length;
                        while (num3 < num4)
                        {
                            this.fields[num3].SetInferredType(Typeob.Object, null);
                            num3++;
                        }
                    }
                    if (!this.isConstructor)
                    {
                        this.body.PartiallyEvaluate();
                    }
                    else
                    {
                        this.body.MarkSuperOKIfIsFirstStatement();
                        this.body.PartiallyEvaluate();
                        ClassScope scope2 = (ClassScope) this.enclosing_scope;
                        int num5 = (this.superConstructorCall == null) ? 0 : this.superConstructorCall.arguments.count;
                        if (num5 == 0)
                        {
                            Type[] emptyTypes = Type.EmptyTypes;
                        }
                        IReflect[] argIRs = new IReflect[num5];
                        for (int i = 0; i < num5; i++)
                        {
                            argIRs[i] = this.superConstructorCall.arguments[i].InferType(null);
                        }
                        Context context = (this.superConstructorCall == null) ? this.funcContext : this.superConstructorCall.context;
                        try
                        {
                            if ((this.superConstructorCall != null) && !this.superConstructorCall.isSuperConstructorCall)
                            {
                                this.superConstructor = JSBinder.SelectConstructor(scope2.constructors, argIRs);
                            }
                            else
                            {
                                this.superConstructor = scope2.owner.GetSuperConstructor(argIRs);
                            }
                            if (this.superConstructor == null)
                            {
                                context.HandleError(JSError.SuperClassConstructorNotAccessible);
                            }
                            else
                            {
                                ConstructorInfo superConstructor = this.superConstructor;
                                if (((!superConstructor.IsPublic && !superConstructor.IsFamily) && !superConstructor.IsFamilyOrAssembly) && (!(this.superConstructor is JSConstructor) || !((JSConstructor) this.superConstructor).IsAccessibleFrom(this.enclosing_scope)))
                                {
                                    context.HandleError(JSError.SuperClassConstructorNotAccessible);
                                    this.superConstructor = null;
                                }
                                else if ((num5 > 0) && !Binding.CheckParameters(superConstructor.GetParameters(), argIRs, this.superConstructorCall.arguments, this.superConstructorCall.context))
                                {
                                    this.superConstructor = null;
                                }
                            }
                        }
                        catch (AmbiguousMatchException)
                        {
                            context.HandleError(JSError.AmbiguousConstructorCall);
                        }
                    }
                    this.own_scope.HandleUnitializedVariables();
                    this.globals.ScopeStack.Pop();
                    this.must_save_stack_locals = this.own_scope.mustSaveStackLocals;
                    this.fields = this.own_scope.GetLocalFields();
                }
            }
        }

        internal IReflect ReturnType(JSField inference_target)
        {
            if (!this.partiallyEvaluated)
            {
                this.PartiallyEvaluate();
            }
            if (this.own_scope.returnVar == null)
            {
                return Typeob.Void;
            }
            if (this.return_type_expr != null)
            {
                return this.return_type_expr.ToIReflect();
            }
            return this.own_scope.returnVar.GetInferredType(inference_target);
        }

        public override string ToString()
        {
            if (this.text != null)
            {
                return this.text;
            }
            return this.funcContext.GetCode();
        }

        internal void TranslateBodyToIL(ILGenerator il, CompilerGlobals compilerGlobals)
        {
            this.returnLabel = il.DefineLabel();
            if (this.body.Engine.GenerateDebugInfo)
            {
                for (ScriptObject obj2 = this.enclosing_scope.GetParent(); obj2 != null; obj2 = obj2.GetParent())
                {
                    if (obj2 is PackageScope)
                    {
                        il.UsingNamespace(((PackageScope) obj2).name);
                    }
                    else if ((obj2 is WrappedNamespace) && !((WrappedNamespace) obj2).name.Equals(""))
                    {
                        il.UsingNamespace(((WrappedNamespace) obj2).name);
                    }
                }
            }
            int startLine = this.body.context.StartLine;
            int startColumn = this.body.context.StartColumn;
            this.body.context.document.EmitLineInfo(il, startLine, startColumn, startLine, startColumn + 1);
            if (this.body.context.document.debugOn)
            {
                il.Emit(OpCodes.Nop);
            }
            int length = this.fields.Length;
            for (int i = 0; i < length; i++)
            {
                if (!this.fields[i].IsLiteral || (this.fields[i].value is FunctionObject))
                {
                    Type fieldType = this.fields[i].FieldType;
                    LocalBuilder builder = il.DeclareLocal(fieldType);
                    if (this.fields[i].debugOn)
                    {
                        builder.SetLocalSymInfo(this.fields[i].debuggerName);
                    }
                    this.fields[i].metaData = builder;
                }
            }
            this.globals.ScopeStack.Push(this.own_scope);
            try
            {
                if (this.must_save_stack_locals)
                {
                    this.TranslateToMethodWithStackFrame(il, compilerGlobals, true);
                }
                else
                {
                    this.body.TranslateToILInitializer(il);
                    this.body.TranslateToIL(il, Typeob.Void);
                    il.MarkLabel(this.returnLabel);
                }
            }
            finally
            {
                this.globals.ScopeStack.Pop();
            }
        }

        internal void TranslateToIL(CompilerGlobals compilerGlobals)
        {
            if (!this.suppressIL)
            {
                this.globals.ScopeStack.Push(this.own_scope);
                try
                {
                    if ((this.mb == null) && (this.cb == null))
                    {
                        this.GetMethodBase(compilerGlobals);
                    }
                    int num = ((this.attributes & MethodAttributes.Static) == MethodAttributes.Static) ? 0 : 1;
                    int num2 = 3;
                    if (this.isMethod)
                    {
                        num2 = 0;
                    }
                    else if (!this.hasArgumentsObject)
                    {
                        num2 = 2;
                    }
                    ILGenerator ilgen = (this.mb != null) ? this.mb.GetILGenerator() : this.cb.GetILGenerator();
                    this.returnLabel = ilgen.DefineLabel();
                    if (this.body.Engine.GenerateDebugInfo)
                    {
                        for (ScriptObject obj2 = this.enclosing_scope.GetParent(); obj2 != null; obj2 = obj2.GetParent())
                        {
                            if (obj2 is PackageScope)
                            {
                                ilgen.UsingNamespace(((PackageScope) obj2).name);
                            }
                            else if ((obj2 is WrappedNamespace) && !((WrappedNamespace) obj2).name.Equals(""))
                            {
                                ilgen.UsingNamespace(((WrappedNamespace) obj2).name);
                            }
                        }
                    }
                    if (!this.isImplicitCtor && (this.body != null))
                    {
                        int startLine = this.body.context.StartLine;
                        int startColumn = this.body.context.StartColumn;
                        this.body.context.document.EmitLineInfo(ilgen, startLine, startColumn, startLine, startColumn + 1);
                        if (this.body.context.document.debugOn)
                        {
                            ilgen.Emit(OpCodes.Nop);
                        }
                    }
                    int length = this.fields.Length;
                    for (int i = 0; i < length; i++)
                    {
                        int num7 = this.IsNestedFunctionField(this.fields[i]) ? -1 : Array.IndexOf<string>(this.formal_parameters, this.fields[i].Name);
                        if (num7 >= 0)
                        {
                            this.fields[i].metaData = (short) ((num7 + num2) + num);
                        }
                        else if (this.hasArgumentsObject && this.fields[i].Name.Equals("arguments"))
                        {
                            this.fields[i].metaData = (short) (2 + num);
                        }
                        else if (!this.fields[i].IsLiteral || (this.fields[i].value is FunctionObject))
                        {
                            Type fieldType = this.fields[i].FieldType;
                            LocalBuilder builder = ilgen.DeclareLocal(fieldType);
                            if (this.fields[i].debugOn)
                            {
                                builder.SetLocalSymInfo(this.fields[i].debuggerName);
                            }
                            this.fields[i].metaData = builder;
                        }
                        else if (this.own_scope.mustSaveStackLocals)
                        {
                            LocalBuilder builder2 = ilgen.DeclareLocal(this.fields[i].FieldType);
                            this.fields[i].metaData = builder2;
                        }
                    }
                    if (this.isConstructor)
                    {
                        int callerParameterCount = this.formal_parameters.Length + 1;
                        ClassScope scope = (ClassScope) this.enclosing_scope;
                        if (this.superConstructor == null)
                        {
                            scope.owner.EmitInitialCalls(ilgen, null, null, null, 0);
                        }
                        else
                        {
                            ParameterInfo[] parameters = this.superConstructor.GetParameters();
                            if (this.superConstructorCall != null)
                            {
                                scope.owner.EmitInitialCalls(ilgen, this.superConstructor, parameters, this.superConstructorCall.arguments, callerParameterCount);
                            }
                            else
                            {
                                scope.owner.EmitInitialCalls(ilgen, this.superConstructor, parameters, null, callerParameterCount);
                            }
                        }
                    }
                    if ((this.isMethod || this.isConstructor) && this.must_save_stack_locals)
                    {
                        this.TranslateToMethodWithStackFrame(ilgen, compilerGlobals, false);
                    }
                    else
                    {
                        this.TranslateToILToCopyOuterScopeLocals(ilgen, true, null);
                        bool insideProtectedRegion = compilerGlobals.InsideProtectedRegion;
                        compilerGlobals.InsideProtectedRegion = false;
                        bool insideFinally = compilerGlobals.InsideFinally;
                        int finallyStackTop = compilerGlobals.FinallyStackTop;
                        compilerGlobals.InsideFinally = false;
                        this.body.TranslateToILInitializer(ilgen);
                        this.body.TranslateToIL(ilgen, Typeob.Void);
                        compilerGlobals.InsideProtectedRegion = insideProtectedRegion;
                        compilerGlobals.InsideFinally = insideFinally;
                        compilerGlobals.FinallyStackTop = finallyStackTop;
                        ilgen.MarkLabel(this.returnLabel);
                        if (this.body.context.document.debugOn)
                        {
                            this.EmitLastLineInfo(ilgen);
                            ilgen.Emit(OpCodes.Nop);
                        }
                        this.TranslateToILToSaveLocals(ilgen);
                        if (this.own_scope.returnVar != null)
                        {
                            ilgen.Emit(OpCodes.Ldloc, (LocalBuilder) this.own_scope.returnVar.GetMetaData());
                        }
                        ilgen.Emit(OpCodes.Ret);
                    }
                }
                finally
                {
                    this.globals.ScopeStack.Pop();
                }
            }
        }

        private void TranslateToILToCopyLocalsFromNestedScope(ILGenerator il, FunctionScope nestedScope)
        {
            int length = this.fields.Length;
            for (int i = 0; i < length; i++)
            {
                JSLocalField outerLocalField = nestedScope.GetOuterLocalField(this.fields[i].Name);
                if ((outerLocalField != null) && (outerLocalField.outerField == this.fields[i]))
                {
                    il.Emit(OpCodes.Dup);
                    ConstantWrapper.TranslateToILInt(il, this.fields[i].slotNumber);
                    il.Emit(OpCodes.Ldloc, (LocalBuilder) outerLocalField.metaData);
                    Microsoft.JScript.Convert.Emit(this.body, il, this.fields[i].FieldType, Typeob.Object);
                    il.Emit(OpCodes.Stelem_Ref);
                }
            }
            il.Emit(OpCodes.Pop);
        }

        private void TranslateToILToCopyLocalsToNestedScope(ILGenerator il, FunctionScope nestedScope, JSLocalField[] notToBeRestored)
        {
            int length = this.fields.Length;
            for (int i = 0; i < length; i++)
            {
                JSLocalField outerLocalField = nestedScope.GetOuterLocalField(this.fields[i].Name);
                if (((outerLocalField != null) && (outerLocalField.outerField == this.fields[i])) && ((notToBeRestored == null) || !IsPresentIn(outerLocalField, notToBeRestored)))
                {
                    il.Emit(OpCodes.Dup);
                    ConstantWrapper.TranslateToILInt(il, this.fields[i].slotNumber);
                    il.Emit(OpCodes.Ldelem_Ref);
                    Microsoft.JScript.Convert.Emit(this.body, il, Typeob.Object, this.fields[i].FieldType);
                    il.Emit(OpCodes.Stloc, (LocalBuilder) outerLocalField.metaData);
                }
            }
            il.Emit(OpCodes.Pop);
        }

        private void TranslateToILToCopyOuterScopeLocals(ILGenerator il, bool copyToNested, JSLocalField[] notToBeRestored)
        {
            if ((this.own_scope.ProvidesOuterScopeLocals != null) && (this.own_scope.ProvidesOuterScopeLocals.count != 0))
            {
                ScriptObject obj2;
                this.TranslateToILToLoadEngine(il);
                il.Emit(OpCodes.Call, CompilerGlobals.scriptObjectStackTopMethod);
                for (obj2 = this.globals.ScopeStack.Peek(); (obj2 is WithObject) || (obj2 is BlockScope); obj2 = obj2.GetParent())
                {
                    il.Emit(OpCodes.Call, CompilerGlobals.getParentMethod);
                }
                for (obj2 = this.enclosing_scope; obj2 != null; obj2 = obj2.GetParent())
                {
                    il.Emit(OpCodes.Call, CompilerGlobals.getParentMethod);
                    if (((obj2 is FunctionScope) && (((FunctionScope) obj2).owner != null)) && (this.own_scope.ProvidesOuterScopeLocals[obj2] != null))
                    {
                        il.Emit(OpCodes.Dup);
                        il.Emit(OpCodes.Castclass, Typeob.StackFrame);
                        il.Emit(OpCodes.Ldfld, CompilerGlobals.localVarsField);
                        if (copyToNested)
                        {
                            ((FunctionScope) obj2).owner.TranslateToILToCopyLocalsToNestedScope(il, this.own_scope, notToBeRestored);
                        }
                        else
                        {
                            ((FunctionScope) obj2).owner.TranslateToILToCopyLocalsFromNestedScope(il, this.own_scope);
                        }
                    }
                    else if ((obj2 is GlobalScope) || (obj2 is ClassScope))
                    {
                        break;
                    }
                }
                il.Emit(OpCodes.Pop);
            }
        }

        internal void TranslateToILToLoadEngine(ILGenerator il)
        {
            this.TranslateToILToLoadEngine(il, false);
        }

        private void TranslateToILToLoadEngine(ILGenerator il, bool allocateLocal)
        {
            if (this.isMethod)
            {
                if (this.isStatic)
                {
                    if (this.body.Engine.doCRS)
                    {
                        il.Emit(OpCodes.Ldsfld, CompilerGlobals.contextEngineField);
                    }
                    else
                    {
                        if (this.engineLocal == null)
                        {
                            if (allocateLocal)
                            {
                                this.engineLocal = il.DeclareLocal(Typeob.VsaEngine);
                            }
                            if (this.body.Engine.PEFileKind == PEFileKinds.Dll)
                            {
                                il.Emit(OpCodes.Ldtoken, ((ClassScope) this.own_scope.GetParent()).GetTypeBuilder());
                                il.Emit(OpCodes.Call, CompilerGlobals.createVsaEngineWithType);
                            }
                            else
                            {
                                il.Emit(OpCodes.Call, CompilerGlobals.createVsaEngine);
                            }
                            if (allocateLocal)
                            {
                                il.Emit(OpCodes.Stloc, this.engineLocal);
                            }
                            else
                            {
                                return;
                            }
                        }
                        il.Emit(OpCodes.Ldloc, this.engineLocal);
                    }
                }
                else
                {
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Callvirt, CompilerGlobals.getEngineMethod);
                }
            }
            else
            {
                il.Emit(OpCodes.Ldarg_1);
            }
        }

        internal void TranslateToILToRestoreLocals(ILGenerator il)
        {
            this.TranslateToILToRestoreLocals(il, null);
        }

        internal void TranslateToILToRestoreLocals(ILGenerator il, JSLocalField[] notToBeRestored)
        {
            this.TranslateToILToCopyOuterScopeLocals(il, true, notToBeRestored);
            if (this.must_save_stack_locals)
            {
                int num = ((this.attributes & MethodAttributes.Static) == MethodAttributes.Static) ? 0 : 1;
                int num2 = 3;
                if (this.isMethod)
                {
                    num2 = 0;
                }
                else if (!this.hasArgumentsObject)
                {
                    num2 = 2;
                }
                int length = this.fields.Length;
                this.TranslateToILToLoadEngine(il);
                il.Emit(OpCodes.Call, CompilerGlobals.scriptObjectStackTopMethod);
                for (ScriptObject obj2 = this.globals.ScopeStack.Peek(); (obj2 is WithObject) || (obj2 is BlockScope); obj2 = obj2.GetParent())
                {
                    il.Emit(OpCodes.Call, CompilerGlobals.getParentMethod);
                }
                il.Emit(OpCodes.Castclass, Typeob.StackFrame);
                il.Emit(OpCodes.Ldfld, CompilerGlobals.localVarsField);
                for (int i = 0; i < length; i++)
                {
                    if (((notToBeRestored == null) || !IsPresentIn(this.fields[i], notToBeRestored)) && !this.fields[i].IsLiteral)
                    {
                        il.Emit(OpCodes.Dup);
                        int index = Array.IndexOf<string>(this.formal_parameters, this.fields[i].Name);
                        ConstantWrapper.TranslateToILInt(il, this.fields[i].slotNumber);
                        il.Emit(OpCodes.Ldelem_Ref);
                        Microsoft.JScript.Convert.Emit(this.body, il, Typeob.Object, this.fields[i].FieldType);
                        if ((index >= 0) || (this.fields[i].Name.Equals("arguments") && this.hasArgumentsObject))
                        {
                            il.Emit(OpCodes.Starg, (short) ((index + num2) + num));
                        }
                        else
                        {
                            il.Emit(OpCodes.Stloc, (LocalBuilder) this.fields[i].metaData);
                        }
                    }
                }
                il.Emit(OpCodes.Pop);
            }
        }

        internal void TranslateToILToSaveLocals(ILGenerator il)
        {
            this.TranslateToILToCopyOuterScopeLocals(il, false, null);
            if (this.must_save_stack_locals)
            {
                int num = ((this.attributes & MethodAttributes.Static) == MethodAttributes.Static) ? 0 : 1;
                int num2 = 3;
                if (this.isMethod)
                {
                    num2 = 0;
                }
                else if (!this.hasArgumentsObject)
                {
                    num2 = 2;
                }
                int length = this.fields.Length;
                this.TranslateToILToLoadEngine(il);
                il.Emit(OpCodes.Call, CompilerGlobals.scriptObjectStackTopMethod);
                for (ScriptObject obj2 = this.globals.ScopeStack.Peek(); (obj2 is WithObject) || (obj2 is BlockScope); obj2 = obj2.GetParent())
                {
                    il.Emit(OpCodes.Call, CompilerGlobals.getParentMethod);
                }
                il.Emit(OpCodes.Castclass, Typeob.StackFrame);
                il.Emit(OpCodes.Ldfld, CompilerGlobals.localVarsField);
                for (int i = 0; i < length; i++)
                {
                    JSLocalField field = this.fields[i];
                    if (!field.IsLiteral || (field.value is FunctionObject))
                    {
                        il.Emit(OpCodes.Dup);
                        ConstantWrapper.TranslateToILInt(il, field.slotNumber);
                        int index = Array.IndexOf<string>(this.formal_parameters, field.Name);
                        if ((index >= 0) || (field.Name.Equals("arguments") && this.hasArgumentsObject))
                        {
                            Microsoft.JScript.Convert.EmitLdarg(il, (short) ((index + num2) + num));
                        }
                        else
                        {
                            il.Emit(OpCodes.Ldloc, (LocalBuilder) field.metaData);
                        }
                        Microsoft.JScript.Convert.Emit(this.body, il, field.FieldType, Typeob.Object);
                        il.Emit(OpCodes.Stelem_Ref);
                    }
                }
                il.Emit(OpCodes.Pop);
            }
        }

        private void TranslateToMethodWithStackFrame(ILGenerator il, CompilerGlobals compilerGlobals, bool staticInitializer)
        {
            if (this.isStatic)
            {
                il.Emit(OpCodes.Ldtoken, ((ClassScope) this.own_scope.GetParent()).GetTypeBuilder());
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
            }
            int length = this.fields.Length;
            ConstantWrapper.TranslateToILInt(il, length);
            il.Emit(OpCodes.Newarr, Typeob.JSLocalField);
            for (int i = 0; i < length; i++)
            {
                JSLocalField field = this.fields[i];
                il.Emit(OpCodes.Dup);
                ConstantWrapper.TranslateToILInt(il, i);
                il.Emit(OpCodes.Ldstr, field.Name);
                il.Emit(OpCodes.Ldtoken, field.FieldType);
                ConstantWrapper.TranslateToILInt(il, field.slotNumber);
                il.Emit(OpCodes.Newobj, CompilerGlobals.jsLocalFieldConstructor);
                il.Emit(OpCodes.Stelem_Ref);
            }
            this.TranslateToILToLoadEngine(il, true);
            if (this.isStatic)
            {
                il.Emit(OpCodes.Call, CompilerGlobals.pushStackFrameForStaticMethod);
            }
            else
            {
                il.Emit(OpCodes.Call, CompilerGlobals.pushStackFrameForMethod);
            }
            bool insideProtectedRegion = compilerGlobals.InsideProtectedRegion;
            compilerGlobals.InsideProtectedRegion = true;
            il.BeginExceptionBlock();
            this.body.TranslateToILInitializer(il);
            this.body.TranslateToIL(il, Typeob.Void);
            il.MarkLabel(this.returnLabel);
            this.TranslateToILToSaveLocals(il);
            Label label = il.DefineLabel();
            il.Emit(OpCodes.Leave, label);
            il.BeginFinallyBlock();
            this.TranslateToILToLoadEngine(il);
            il.Emit(OpCodes.Call, CompilerGlobals.popScriptObjectMethod);
            il.Emit(OpCodes.Pop);
            il.EndExceptionBlock();
            il.MarkLabel(label);
            if (!staticInitializer)
            {
                if (this.body.context.document.debugOn)
                {
                    this.EmitLastLineInfo(il);
                    il.Emit(OpCodes.Nop);
                }
                if (this.own_scope.returnVar != null)
                {
                    il.Emit(OpCodes.Ldloc, (LocalBuilder) this.own_scope.returnVar.GetMetaData());
                }
                il.Emit(OpCodes.Ret);
            }
            compilerGlobals.InsideProtectedRegion = insideProtectedRegion;
        }

        internal bool Must_save_stack_locals
        {
            get
            {
                if (!this.partiallyEvaluated)
                {
                    this.PartiallyEvaluate();
                }
                return this.must_save_stack_locals;
            }
        }
    }
}

