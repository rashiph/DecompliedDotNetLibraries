namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.Expando;

    public sealed class LateBinding
    {
        private bool checkForDebugger;
        private object last_ir;
        internal MemberInfo last_member;
        internal MemberInfo[] last_members;
        internal object last_object;
        private string name;
        public object obj;

        public LateBinding(string name) : this(name, null, false)
        {
        }

        public LateBinding(string name, object obj) : this(name, obj, false)
        {
        }

        internal LateBinding(string name, object obj, bool checkForDebugger)
        {
            this.last_ir = null;
            this.last_member = null;
            this.last_members = null;
            this.last_object = null;
            this.name = name;
            this.obj = obj;
            this.checkForDebugger = checkForDebugger;
        }

        internal MemberInfo BindToMember()
        {
            if ((this.obj != this.last_object) || (this.last_member == null))
            {
                BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance;
                object obj2 = this.obj;
                TypeReflector typeReflectorFor = TypeReflector.GetTypeReflectorFor(obj2.GetType());
                IReflect reflect = null;
                if (typeReflectorFor.Is__ComObject())
                {
                    if (!this.checkForDebugger)
                    {
                        return null;
                    }
                    IDebuggerObject obj3 = obj2 as IDebuggerObject;
                    if (obj3 == null)
                    {
                        return null;
                    }
                    if (obj3.IsCOMObject())
                    {
                        return null;
                    }
                    reflect = (IReflect) obj2;
                }
                else if (typeReflectorFor.ImplementsIReflect())
                {
                    reflect = obj2 as ScriptObject;
                    if (reflect != null)
                    {
                        if (obj2 is ClassScope)
                        {
                            bindingAttr = BindingFlags.Public | BindingFlags.Static;
                        }
                    }
                    else
                    {
                        reflect = obj2 as Type;
                        if (reflect != null)
                        {
                            bindingAttr = BindingFlags.Public | BindingFlags.Static;
                        }
                        else
                        {
                            reflect = (IReflect) obj2;
                        }
                    }
                }
                else
                {
                    reflect = typeReflectorFor;
                }
                this.last_object = this.obj;
                this.last_ir = reflect;
                MemberInfo[] mems = this.last_members = reflect.GetMember(this.name, bindingAttr);
                this.last_member = SelectMember(mems);
                if (this.obj is Type)
                {
                    MemberInfo[] member = typeof(Type).GetMember(this.name, BindingFlags.Public | BindingFlags.Instance);
                    int n = 0;
                    int num2 = 0;
                    if ((member != null) && ((n = member.Length) > 0))
                    {
                        if ((mems == null) || ((num2 = mems.Length) == 0))
                        {
                            this.last_member = SelectMember(this.last_members = member);
                        }
                        else
                        {
                            MemberInfo[] target = new MemberInfo[n + num2];
                            ArrayObject.Copy(mems, 0, target, 0, num2);
                            ArrayObject.Copy(member, 0, target, num2, n);
                            this.last_member = SelectMember(this.last_members = target);
                        }
                    }
                }
            }
            return this.last_member;
        }

        [DebuggerStepThrough, DebuggerHidden]
        public object Call(object[] arguments, bool construct, bool brackets, VsaEngine engine)
        {
            object obj2;
            try
            {
                if (this.name == null)
                {
                    return CallValue(this.obj, arguments, construct, brackets, engine, ((IActivationObject) engine.ScriptObjectStackTop()).GetDefaultThisObject(), JSBinder.ob, null, null);
                }
                obj2 = this.Call(JSBinder.ob, arguments, null, null, null, construct, brackets, engine);
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException;
            }
            return obj2;
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal object Call(Binder binder, object[] arguments, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters, bool construct, bool brackets, VsaEngine engine)
        {
            MemberInfo target = this.BindToMember();
            if ((this.obj is ScriptObject) || (this.obj is GlobalObject))
            {
                if (this.obj is WithObject)
                {
                    object obj2 = ((WithObject) this.obj).contained_object;
                    if (!(obj2 is ScriptObject))
                    {
                        IReflect ir = GetIRForObjectThatRequiresInvokeMember(obj2, VsaEngine.executeForJSEE);
                        if (ir != null)
                        {
                            return CallCOMObject(ir, this.name, obj2, binder, arguments, modifiers, culture, namedParameters, construct, brackets, engine);
                        }
                    }
                }
                if (target is FieldInfo)
                {
                    return CallValue(((FieldInfo) target).GetValue(this.obj), arguments, construct, brackets, engine, this.obj, JSBinder.ob, null, null);
                }
                if ((target is PropertyInfo) && !(target is JSProperty))
                {
                    if (!brackets)
                    {
                        JSWrappedPropertyAndMethod method = target as JSWrappedPropertyAndMethod;
                        if (method != null)
                        {
                            BindingFlags options = ((arguments == null) || (arguments.Length == 0)) ? BindingFlags.InvokeMethod : (BindingFlags.GetProperty | BindingFlags.InvokeMethod);
                            return method.Invoke(this.obj, options, JSBinder.ob, arguments, null);
                        }
                    }
                    return CallValue(JSProperty.GetValue((PropertyInfo) target, this.obj, null), arguments, construct, brackets, engine, this.obj, JSBinder.ob, null, null);
                }
                if (target is MethodInfo)
                {
                    if (target is JSMethod)
                    {
                        if (construct)
                        {
                            return ((JSMethod) target).Construct(arguments);
                        }
                        return ((JSMethod) target).Invoke(this.obj, this.obj, BindingFlags.Default, JSBinder.ob, arguments, null);
                    }
                    Type declaringType = target.DeclaringType;
                    if (declaringType == typeof(object))
                    {
                        return CallMethod((MethodInfo) target, arguments, this.obj, binder, culture, namedParameters);
                    }
                    if (declaringType == typeof(string))
                    {
                        return CallMethod((MethodInfo) target, arguments, Microsoft.JScript.Convert.ToString(this.obj), binder, culture, namedParameters);
                    }
                    if (Microsoft.JScript.Convert.IsPrimitiveNumericType(declaringType))
                    {
                        return CallMethod((MethodInfo) target, arguments, Microsoft.JScript.Convert.CoerceT(this.obj, declaringType), binder, culture, namedParameters);
                    }
                    if (declaringType == typeof(bool))
                    {
                        return CallMethod((MethodInfo) target, arguments, Microsoft.JScript.Convert.ToBoolean(this.obj), binder, culture, namedParameters);
                    }
                    if (((declaringType == typeof(StringObject)) || (declaringType == typeof(BooleanObject))) || ((declaringType == typeof(NumberObject)) || brackets))
                    {
                        return CallMethod((MethodInfo) target, arguments, Microsoft.JScript.Convert.ToObject(this.obj, engine), binder, culture, namedParameters);
                    }
                    if ((declaringType == typeof(GlobalObject)) && ((MethodInfo) target).IsSpecialName)
                    {
                        return CallValue(((MethodInfo) target).Invoke(this.obj, null), arguments, construct, false, engine, this.obj, JSBinder.ob, null, null);
                    }
                    if (!(this.obj is ClassScope))
                    {
                        if (Microsoft.JScript.CustomAttribute.IsDefined(target, typeof(JSFunctionAttribute), false))
                        {
                            FieldInfo info2 = SelectMember(this.last_members) as FieldInfo;
                            if (info2 != null)
                            {
                                object obj3 = this.obj;
                                if (!(obj3 is Closure))
                                {
                                    obj3 = info2.GetValue(this.obj);
                                }
                                return CallValue(obj3, arguments, construct, brackets, engine, this.obj, JSBinder.ob, null, null);
                            }
                        }
                        return CallValue(new BuiltinFunction(this.obj, (MethodInfo) target), arguments, construct, false, engine, this.obj, JSBinder.ob, null, null);
                    }
                }
            }
            MethodInfo info3 = target as MethodInfo;
            if (info3 != null)
            {
                return CallMethod(info3, arguments, this.obj, binder, culture, namedParameters);
            }
            JSConstructor constructor = target as JSConstructor;
            if (constructor != null)
            {
                return CallValue(constructor.cons, arguments, construct, brackets, engine, this.obj, JSBinder.ob, null, null);
            }
            if (target is Type)
            {
                return CallValue(target, arguments, construct, brackets, engine, this.obj, JSBinder.ob, null, null);
            }
            if (target is ConstructorInfo)
            {
                return CallOneOfTheMembers(new MemberInfo[] { this.last_member }, arguments, true, this.obj, binder, culture, namedParameters, engine);
            }
            if (!construct && (target is PropertyInfo))
            {
                if (target is COMPropertyInfo)
                {
                    return ((PropertyInfo) target).GetValue(this.obj, BindingFlags.OptionalParamBinding | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, binder, arguments, culture);
                }
                if (((PropertyInfo) target).GetIndexParameters().Length == 0)
                {
                    Type propertyType = ((PropertyInfo) target).PropertyType;
                    if (propertyType == typeof(object))
                    {
                        MethodInfo getMethod = JSProperty.GetGetMethod((PropertyInfo) target, false);
                        if (getMethod != null)
                        {
                            return CallValue(getMethod.Invoke(this.obj, null), arguments, construct, brackets, engine, this.obj, JSBinder.ob, null, null);
                        }
                    }
                    MemberInfo[] defaultMembers = TypeReflector.GetTypeReflectorFor(propertyType).GetDefaultMembers();
                    if ((defaultMembers != null) && (defaultMembers.Length > 0))
                    {
                        MethodInfo info5 = JSProperty.GetGetMethod((PropertyInfo) target, false);
                        if (info5 != null)
                        {
                            object thisob = info5.Invoke(this.obj, null);
                            return CallOneOfTheMembers(defaultMembers, arguments, false, thisob, binder, culture, namedParameters, engine);
                        }
                    }
                }
            }
            if ((this.last_members != null) && (this.last_members.Length > 0))
            {
                bool flag;
                object obj6 = CallOneOfTheMembers(this.last_members, arguments, construct, this.obj, binder, culture, namedParameters, engine, out flag);
                if (flag)
                {
                    return obj6;
                }
            }
            IReflect iRForObjectThatRequiresInvokeMember = GetIRForObjectThatRequiresInvokeMember(this.obj, VsaEngine.executeForJSEE);
            if (iRForObjectThatRequiresInvokeMember != null)
            {
                return CallCOMObject(iRForObjectThatRequiresInvokeMember, this.name, this.obj, binder, arguments, modifiers, culture, namedParameters, construct, brackets, engine);
            }
            object val = GetMemberValue(this.obj, this.name, this.last_member, this.last_members);
            if (!(val is Microsoft.JScript.Missing))
            {
                return CallValue(val, arguments, construct, brackets, engine, this.obj, JSBinder.ob, null, null);
            }
            if (!brackets)
            {
                throw new JScriptException(JSError.FunctionExpected);
            }
            if (this.obj is IActivationObject)
            {
                throw new JScriptException(JSError.ObjectExpected);
            }
            throw new JScriptException(JSError.OLENoPropOrMethod);
        }

        [DebuggerHidden, DebuggerStepThrough]
        private static object CallCOMObject(IReflect ir, string name, object ob, Binder binder, object[] arguments, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters, bool construct, bool brackets, VsaEngine engine)
        {
            object obj3;
            try
            {
                try
                {
                    Change64bitIntegersToDouble(arguments);
                    BindingFlags invokeAttr = BindingFlags.OptionalParamBinding | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
                    if (construct)
                    {
                        return ir.InvokeMember(name, invokeAttr | BindingFlags.CreateInstance, binder, ob, arguments, modifiers, culture, namedParameters);
                    }
                    if (brackets)
                    {
                        try
                        {
                            return ir.InvokeMember(name, (invokeAttr | BindingFlags.GetProperty) | BindingFlags.GetField, binder, ob, arguments, modifiers, culture, namedParameters);
                        }
                        catch (TargetInvocationException)
                        {
                            object val = ir.InvokeMember(name, (invokeAttr | BindingFlags.GetProperty) | BindingFlags.GetField, binder, ob, new object[0], modifiers, culture, new string[0]);
                            return CallValue(val, arguments, construct, brackets, engine, val, binder, culture, namedParameters);
                        }
                    }
                    int num = (arguments == null) ? 0 : arguments.Length;
                    if (((namedParameters != null) && (namedParameters.Length > 0)) && (namedParameters[0].Equals("[DISPID=-613]") || namedParameters[0].Equals("this")))
                    {
                        num--;
                    }
                    invokeAttr |= (num > 0) ? (BindingFlags.GetProperty | BindingFlags.InvokeMethod) : BindingFlags.InvokeMethod;
                    obj3 = ir.InvokeMember(name, invokeAttr, binder, ob, arguments, modifiers, culture, namedParameters);
                }
                catch (MissingMemberException)
                {
                    if (!brackets)
                    {
                        throw new JScriptException(JSError.FunctionExpected);
                    }
                    obj3 = null;
                }
                catch (COMException exception)
                {
                    int errorCode = exception.ErrorCode;
                    switch (errorCode)
                    {
                        case -2147352570:
                        case -2147352573:
                            if (!brackets)
                            {
                                throw new JScriptException(JSError.FunctionExpected);
                            }
                            return null;
                    }
                    if ((errorCode & 0xffff0000L) == 0x800a0000L)
                    {
                        string source = exception.Source;
                        if ((source != null) && (source.IndexOf("JScript") != -1))
                        {
                            throw new JScriptException(exception, null);
                        }
                    }
                    throw exception;
                }
            }
            catch (JScriptException exception2)
            {
                if ((exception2.Number & 0xffff) == 0x138a)
                {
                    MemberInfo[] member = typeof(object).GetMember(name, BindingFlags.Public | BindingFlags.Instance);
                    if ((member != null) && (member.Length > 0))
                    {
                        return CallOneOfTheMembers(member, arguments, construct, ob, binder, culture, namedParameters, engine);
                    }
                }
                throw exception2;
            }
            return obj3;
        }

        [DebuggerHidden, DebuggerStepThrough]
        private static object CallMethod(MethodInfo method, object[] arguments, object thisob, Binder binder, CultureInfo culture, string[] namedParameters)
        {
            if ((namedParameters != null) && (namedParameters.Length > 0))
            {
                if (arguments.Length < namedParameters.Length)
                {
                    throw new JScriptException(JSError.MoreNamedParametersThanArguments);
                }
                arguments = JSBinder.ArrangeNamedArguments(method, arguments, namedParameters);
            }
            object[] parameters = LickArgumentsIntoShape(method.GetParameters(), arguments, binder, culture);
            try
            {
                object obj2 = method.Invoke(thisob, BindingFlags.SuppressChangeType, null, parameters, null);
                if (((parameters != arguments) && (parameters != null)) && (arguments != null))
                {
                    int length = arguments.Length;
                    int num2 = parameters.Length;
                    if (num2 < length)
                    {
                        length = num2;
                    }
                    for (int i = 0; i < length; i++)
                    {
                        arguments[i] = parameters[i];
                    }
                }
                return obj2;
            }
            catch (TargetException exception)
            {
                ClassScope scope = thisob as ClassScope;
                if (scope == null)
                {
                    throw;
                }
                return scope.FakeCallToTypeMethod(method, parameters, exception);
            }
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal static object CallOneOfTheMembers(MemberInfo[] members, object[] arguments, bool construct, object thisob, Binder binder, CultureInfo culture, string[] namedParameters, VsaEngine engine)
        {
            bool flag;
            object obj2 = CallOneOfTheMembers(members, arguments, construct, thisob, binder, culture, namedParameters, engine, out flag);
            if (!flag)
            {
                throw new MissingMemberException();
            }
            return obj2;
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal static object CallOneOfTheMembers(MemberInfo[] members, object[] arguments, bool construct, object thisob, Binder binder, CultureInfo culture, string[] namedParameters, VsaEngine engine, out bool memberCalled)
        {
            memberCalled = true;
            if (construct)
            {
                ConstructorInfo target = JSBinder.SelectConstructor(Runtime.TypeRefs, members, ref arguments, namedParameters);
                if (target != null)
                {
                    if (Microsoft.JScript.CustomAttribute.IsDefined(target, typeof(JSFunctionAttribute), false))
                    {
                        if (thisob is Microsoft.JScript.StackFrame)
                        {
                            thisob = ((Microsoft.JScript.StackFrame) thisob).closureInstance;
                        }
                        int length = arguments.Length;
                        object[] objArray = new object[length + 1];
                        ArrayObject.Copy(arguments, 0, objArray, 0, length);
                        objArray[length] = thisob;
                        arguments = objArray;
                    }
                    object obj2 = null;
                    JSConstructor constructor = target as JSConstructor;
                    if (constructor != null)
                    {
                        obj2 = constructor.Construct(thisob, LickArgumentsIntoShape(target.GetParameters(), arguments, JSBinder.ob, culture));
                    }
                    else
                    {
                        obj2 = target.Invoke(BindingFlags.SuppressChangeType, null, LickArgumentsIntoShape(target.GetParameters(), arguments, JSBinder.ob, culture), null);
                    }
                    if (obj2 is INeedEngine)
                    {
                        ((INeedEngine) obj2).SetEngine(engine);
                    }
                    return obj2;
                }
            }
            else
            {
                object[] objArray2 = arguments;
                MethodInfo info2 = JSBinder.SelectMethod(Runtime.TypeRefs, members, ref arguments, namedParameters);
                if (info2 != null)
                {
                    if (info2 is JSMethod)
                    {
                        return ((JSMethod) info2).Invoke(thisob, thisob, BindingFlags.Default, JSBinder.ob, arguments, null);
                    }
                    if (Microsoft.JScript.CustomAttribute.IsDefined(info2, typeof(JSFunctionAttribute), false))
                    {
                        if (!construct)
                        {
                            JSBuiltin builtinFunction = ((JSFunctionAttribute) Microsoft.JScript.CustomAttribute.GetCustomAttributes(info2, typeof(JSFunctionAttribute), false)[0]).builtinFunction;
                            if (builtinFunction != JSBuiltin.None)
                            {
                                IActivationObject obj3 = thisob as IActivationObject;
                                if (obj3 != null)
                                {
                                    thisob = obj3.GetDefaultThisObject();
                                }
                                return BuiltinFunction.QuickCall(arguments, thisob, builtinFunction, null, engine);
                            }
                        }
                        return CallValue(new BuiltinFunction(thisob, info2), arguments, construct, false, engine, thisob, JSBinder.ob, null, null);
                    }
                    object[] parameters = LickArgumentsIntoShape(info2.GetParameters(), arguments, JSBinder.ob, culture);
                    if ((thisob != null) && !info2.DeclaringType.IsAssignableFrom(thisob.GetType()))
                    {
                        if (thisob is StringObject)
                        {
                            return info2.Invoke(((StringObject) thisob).value, BindingFlags.SuppressChangeType, null, parameters, null);
                        }
                        if (thisob is NumberObject)
                        {
                            return info2.Invoke(((NumberObject) thisob).value, BindingFlags.SuppressChangeType, null, parameters, null);
                        }
                        if (thisob is BooleanObject)
                        {
                            return info2.Invoke(((BooleanObject) thisob).value, BindingFlags.SuppressChangeType, null, parameters, null);
                        }
                        if (thisob is ArrayWrapper)
                        {
                            return info2.Invoke(((ArrayWrapper) thisob).value, BindingFlags.SuppressChangeType, null, parameters, null);
                        }
                    }
                    object obj4 = info2.Invoke(thisob, BindingFlags.SuppressChangeType, null, parameters, null);
                    if (((parameters != objArray2) && (arguments == objArray2)) && ((parameters != null) && (arguments != null)))
                    {
                        int num2 = arguments.Length;
                        int num3 = parameters.Length;
                        if (num3 < num2)
                        {
                            num2 = num3;
                        }
                        for (int i = 0; i < num2; i++)
                        {
                            arguments[i] = parameters[i];
                        }
                    }
                    return obj4;
                }
            }
            memberCalled = false;
            return null;
        }

        [DebuggerHidden, DebuggerStepThrough]
        public static object CallValue(object thisob, object val, object[] arguments, bool construct, bool brackets, VsaEngine engine)
        {
            object obj2;
            try
            {
                obj2 = CallValue(val, arguments, construct, brackets, engine, thisob, JSBinder.ob, null, null);
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException;
            }
            return obj2;
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal static object CallValue(object val, object[] arguments, bool construct, bool brackets, VsaEngine engine, object thisob, Binder binder, CultureInfo culture, string[] namedParameters)
        {
            FunctionObject obj4;
            if (!construct)
            {
                goto Label_029A;
            }
            if (!(val is ScriptFunction))
            {
                if (val is ClassScope)
                {
                    if (brackets)
                    {
                        return Array.CreateInstance(typeof(object), ToIndices(arguments));
                    }
                    JSObject obj7 = (JSObject) CallOneOfTheMembers(((ClassScope) val).constructors, arguments, construct, thisob, binder, culture, namedParameters, engine);
                    obj7.noExpando = ((ClassScope) val).noExpando;
                    return obj7;
                }
                if (val is Type)
                {
                    Type t = (Type) val;
                    if (t.IsInterface && t.IsImport)
                    {
                        t = JSBinder.HandleCoClassAttribute(t);
                    }
                    if (brackets)
                    {
                        return Array.CreateInstance(t, ToIndices(arguments));
                    }
                    ConstructorInfo[] constructors = t.GetConstructors();
                    object obj8 = null;
                    if ((constructors == null) || (constructors.Length == 0))
                    {
                        obj8 = Activator.CreateInstance(t, BindingFlags.Default, JSBinder.ob, arguments, null);
                    }
                    else
                    {
                        obj8 = CallOneOfTheMembers(constructors, arguments, construct, thisob, binder, culture, namedParameters, engine);
                    }
                    if (obj8 is INeedEngine)
                    {
                        ((INeedEngine) obj8).SetEngine(engine);
                    }
                    return obj8;
                }
                if ((val is TypedArray) && brackets)
                {
                    return Array.CreateInstance(typeof(object), ToIndices(arguments));
                }
                if (VsaEngine.executeForJSEE && (val is IDebuggerObject))
                {
                    IReflect reflect = val as IReflect;
                    if (reflect == null)
                    {
                        throw new JScriptException(JSError.FunctionExpected);
                    }
                    return reflect.InvokeMember(string.Empty, BindingFlags.OptionalParamBinding | BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, binder, thisob, arguments, null, culture, namedParameters);
                }
                goto Label_029A;
            }
            ScriptFunction function = (ScriptFunction) val;
            if (brackets)
            {
                object obj2;
                obj2 = obj2 = function[arguments];
                if (obj2 != null)
                {
                    return CallValue(obj2, new object[0], true, false, engine, thisob, binder, culture, namedParameters);
                }
                Type predefinedType = Runtime.TypeRefs.GetPredefinedType(function.name);
                if (predefinedType != null)
                {
                    int length = arguments.Length;
                    int[] lengths = new int[length];
                    length = 0;
                    foreach (object obj3 in arguments)
                    {
                        if (obj3 is int)
                        {
                            lengths[length++] = (int) obj3;
                        }
                        else
                        {
                            IConvertible iConvertible = Microsoft.JScript.Convert.GetIConvertible(obj3);
                            if ((iConvertible == null) || !Microsoft.JScript.Convert.IsPrimitiveNumericTypeCode(iConvertible.GetTypeCode()))
                            {
                                goto Label_00EE;
                            }
                            double num2 = iConvertible.ToDouble(null);
                            int num3 = (int) num2;
                            if (num2 != num3)
                            {
                                goto Label_00EE;
                            }
                            lengths[length++] = num3;
                        }
                    }
                    return Array.CreateInstance(predefinedType, lengths);
                }
            }
        Label_00EE:
            obj4 = function as FunctionObject;
            if (obj4 != null)
            {
                return obj4.Construct(thisob as JSObject, (arguments == null) ? new object[0] : arguments);
            }
            object obj5 = function.Construct((arguments == null) ? new object[0] : arguments);
            JSObject obj6 = obj5 as JSObject;
            if (obj6 != null)
            {
                obj6.outer_class_instance = thisob as JSObject;
            }
            return obj5;
        Label_029A:
            if (brackets)
            {
                ScriptObject obj9 = val as ScriptObject;
                if (obj9 != null)
                {
                    object obj10 = obj9[arguments];
                    if (construct)
                    {
                        return CallValue(thisob, obj10, new object[0], true, false, engine);
                    }
                    return obj10;
                }
            }
            else
            {
                if (val is ScriptFunction)
                {
                    if (thisob is IActivationObject)
                    {
                        thisob = ((IActivationObject) thisob).GetDefaultThisObject();
                    }
                    return ((ScriptFunction) val).Call((arguments == null) ? new object[0] : arguments, thisob, binder, culture);
                }
                if (val is Delegate)
                {
                    return CallMethod(((Delegate) val).Method, arguments, thisob, binder, culture, namedParameters);
                }
                if (val is MethodInfo)
                {
                    return CallMethod((MethodInfo) val, arguments, thisob, binder, culture, namedParameters);
                }
                if ((val is Type) && (arguments.Length == 1))
                {
                    return Microsoft.JScript.Convert.CoerceT(arguments[0], (Type) val, true);
                }
                if (VsaEngine.executeForJSEE && (val is IDebuggerObject))
                {
                    IReflect ir = val as IReflect;
                    if (ir == null)
                    {
                        throw new JScriptException(JSError.FunctionExpected);
                    }
                    object[] target = new object[((arguments != null) ? arguments.Length : 0) + 1];
                    target[0] = thisob;
                    if (arguments != null)
                    {
                        ArrayObject.Copy(arguments, 0, target, 1, arguments.Length);
                    }
                    string[] strArray = new string[((namedParameters != null) ? namedParameters.Length : 0) + 1];
                    strArray[0] = "this";
                    if (namedParameters != null)
                    {
                        ArrayObject.Copy(namedParameters, 0, strArray, 1, namedParameters.Length);
                    }
                    return CallCOMObject(ir, string.Empty, val, binder, target, null, culture, strArray, false, false, engine);
                }
                if (val is ClassScope)
                {
                    if ((arguments == null) || (arguments.Length != 1))
                    {
                        throw new JScriptException(JSError.FunctionExpected);
                    }
                    if (!((ClassScope) val).HasInstance(arguments[0]))
                    {
                        throw new InvalidCastException(null);
                    }
                    return arguments[0];
                }
                if ((val is TypedArray) && (arguments.Length == 1))
                {
                    return Microsoft.JScript.Convert.Coerce(arguments[0], val, true);
                }
                if (val is ScriptObject)
                {
                    throw new JScriptException(JSError.FunctionExpected);
                }
                if (val is MemberInfo[])
                {
                    return CallOneOfTheMembers((MemberInfo[]) val, arguments, construct, thisob, binder, culture, namedParameters, engine);
                }
            }
            if (val != null)
            {
                Array array = val as Array;
                if (array != null)
                {
                    if (arguments.Length != array.Rank)
                    {
                        throw new JScriptException(JSError.IncorrectNumberOfIndices);
                    }
                    return array.GetValue(ToIndices(arguments));
                }
                val = Microsoft.JScript.Convert.ToObject(val, engine);
                ScriptObject obj11 = val as ScriptObject;
                if (obj11 != null)
                {
                    if (brackets)
                    {
                        return obj11[arguments];
                    }
                    ScriptFunction function2 = obj11 as ScriptFunction;
                    if (function2 == null)
                    {
                        throw new JScriptException(JSError.InvalidCall);
                    }
                    IActivationObject obj12 = thisob as IActivationObject;
                    if (obj12 != null)
                    {
                        thisob = obj12.GetDefaultThisObject();
                    }
                    return function2.Call((arguments == null) ? new object[0] : arguments, thisob, binder, culture);
                }
                IReflect iRForObjectThatRequiresInvokeMember = GetIRForObjectThatRequiresInvokeMember(val, VsaEngine.executeForJSEE);
                if (iRForObjectThatRequiresInvokeMember != null)
                {
                    if (brackets)
                    {
                        string name = string.Empty;
                        int num4 = arguments.Length;
                        if (num4 > 0)
                        {
                            name = Microsoft.JScript.Convert.ToString(arguments[num4 - 1]);
                        }
                        return CallCOMObject(iRForObjectThatRequiresInvokeMember, name, val, binder, null, null, culture, namedParameters, false, true, engine);
                    }
                    if (!(val is IReflect))
                    {
                        return CallCOMObject(iRForObjectThatRequiresInvokeMember, string.Empty, val, binder, arguments, null, culture, namedParameters, false, brackets, engine);
                    }
                    object[] objArray2 = new object[((arguments != null) ? arguments.Length : 0) + 1];
                    objArray2[0] = thisob;
                    if (arguments != null)
                    {
                        ArrayObject.Copy(arguments, 0, objArray2, 1, arguments.Length);
                    }
                    string[] strArray2 = new string[((namedParameters != null) ? namedParameters.Length : 0) + 1];
                    strArray2[0] = "[DISPID=-613]";
                    if (namedParameters != null)
                    {
                        ArrayObject.Copy(namedParameters, 0, strArray2, 1, namedParameters.Length);
                    }
                    return CallCOMObject(iRForObjectThatRequiresInvokeMember, "[DISPID=0]", val, binder, objArray2, null, culture, strArray2, false, brackets, engine);
                }
                if ((VsaEngine.executeForJSEE && (val is IDebuggerObject)) && (val is IReflect))
                {
                    return CallCOMObject((IReflect) val, string.Empty, val, binder, arguments, null, culture, namedParameters, false, brackets, engine);
                }
                MemberInfo[] defaultMembers = TypeReflector.GetTypeReflectorFor(val.GetType()).GetDefaultMembers();
                if ((defaultMembers != null) && (defaultMembers.Length > 0))
                {
                    MethodInfo method = JSBinder.SelectMethod(Runtime.TypeRefs, defaultMembers, ref arguments, namedParameters);
                    if (method != null)
                    {
                        return CallMethod(method, arguments, val, binder, culture, namedParameters);
                    }
                }
            }
            throw new JScriptException(JSError.FunctionExpected);
        }

        [DebuggerHidden, DebuggerStepThrough]
        public static object CallValue2(object val, object thisob, object[] arguments, bool construct, bool brackets, VsaEngine engine)
        {
            object obj2;
            try
            {
                obj2 = CallValue(val, arguments, construct, brackets, engine, thisob, JSBinder.ob, null, null);
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException;
            }
            return obj2;
        }

        private static void Change64bitIntegersToDouble(object[] arguments)
        {
            if (arguments != null)
            {
                int index = 0;
                int length = arguments.Length;
                while (index < length)
                {
                    object ob = arguments[index];
                    IConvertible iConvertible = Microsoft.JScript.Convert.GetIConvertible(ob);
                    switch (Microsoft.JScript.Convert.GetTypeCode(ob, iConvertible))
                    {
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                            arguments[index] = iConvertible.ToDouble(null);
                            break;
                    }
                    index++;
                }
            }
        }

        public bool Delete()
        {
            return DeleteMember(this.obj, this.name);
        }

        public static bool DeleteMember(object obj, string name)
        {
            if ((name == null) || (obj == null))
            {
                return false;
            }
            if (obj is ScriptObject)
            {
                return ((ScriptObject) obj).DeleteMember(name);
            }
            if (obj is IExpando)
            {
                try
                {
                    IExpando expando = (IExpando) obj;
                    MemberInfo m = SelectMember(expando.GetMember(name, BindingFlags.Public | BindingFlags.Instance));
                    if (m != null)
                    {
                        expando.RemoveMember(m);
                        return true;
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }
            if (obj is IDictionary)
            {
                IDictionary dictionary = (IDictionary) obj;
                if (dictionary.Contains(name))
                {
                    dictionary.Remove(name);
                    return true;
                }
                return false;
            }
            Type type = obj.GetType();
            MethodInfo info2 = TypeReflector.GetTypeReflectorFor(type).GetMethod("op_Delete", BindingFlags.ExactBinding | BindingFlags.Public | BindingFlags.Static, null, new Type[] { type, typeof(object[]) }, null);
            if (((info2 == null) || ((info2.Attributes & MethodAttributes.SpecialName) == MethodAttributes.PrivateScope)) || (info2.ReturnType != typeof(bool)))
            {
                return false;
            }
            return (bool) info2.Invoke(null, new object[] { obj, new object[] { name } });
        }

        internal static bool DeleteValueAtIndex(object obj, ulong index)
        {
            if ((obj is ArrayObject) && (index < 0xffffffffL))
            {
                return ((ArrayObject) obj).DeleteValueAtIndex((uint) index);
            }
            return DeleteMember(obj, index.ToString(CultureInfo.InvariantCulture));
        }

        private static IReflect GetIRForObjectThatRequiresInvokeMember(object obj, bool checkForDebugger)
        {
            Type type = obj.GetType();
            if (!TypeReflector.GetTypeReflectorFor(type).Is__ComObject())
            {
                return null;
            }
            if (!checkForDebugger)
            {
                return type;
            }
            IDebuggerObject obj2 = obj as IDebuggerObject;
            if (obj2 == null)
            {
                return type;
            }
            if (!obj2.IsCOMObject())
            {
                return null;
            }
            return (IReflect) obj;
        }

        private static IReflect GetIRForObjectThatRequiresInvokeMember(object obj, bool checkForDebugger, TypeCode tcode)
        {
            if (tcode == TypeCode.Object)
            {
                return GetIRForObjectThatRequiresInvokeMember(obj, checkForDebugger);
            }
            return null;
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal static object GetMemberValue(object obj, string name)
        {
            if (obj is ScriptObject)
            {
                return ((ScriptObject) obj).GetMemberValue(name);
            }
            LateBinding binding = new LateBinding(name, obj);
            return binding.GetNonMissingValue();
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal static object GetMemberValue(object obj, string name, MemberInfo member, MemberInfo[] members)
        {
            if (member != null)
            {
                try
                {
                    switch (member.MemberType)
                    {
                        case MemberTypes.Event:
                            return null;

                        case (MemberTypes.Event | MemberTypes.Constructor):
                            goto Label_01A4;

                        case MemberTypes.Field:
                        {
                            object obj2 = ((FieldInfo) member).GetValue(obj);
                            Type enumType = obj as Type;
                            if ((enumType != null) && enumType.IsEnum)
                            {
                                try
                                {
                                    obj2 = Enum.ToObject(enumType, ((IConvertible) obj2).ToUInt64(null));
                                }
                                catch
                                {
                                }
                            }
                            return obj2;
                        }
                        case MemberTypes.Property:
                        {
                            PropertyInfo prop = (PropertyInfo) member;
                            if (prop.DeclaringType == typeof(ArrayObject))
                            {
                                ArrayObject obj3 = obj as ArrayObject;
                                if (obj3 != null)
                                {
                                    return obj3.length;
                                }
                            }
                            else if (prop.DeclaringType == typeof(StringObject))
                            {
                                StringObject obj4 = obj as StringObject;
                                if (obj4 != null)
                                {
                                    return obj4.length;
                                }
                            }
                            return JSProperty.GetValue(prop, obj, null);
                        }
                        case MemberTypes.NestedType:
                            return member;
                    }
                }
                catch
                {
                    if (obj is StringObject)
                    {
                        return GetMemberValue(((StringObject) obj).value, name, member, members);
                    }
                    if (obj is NumberObject)
                    {
                        return GetMemberValue(((NumberObject) obj).value, name, member, members);
                    }
                    if (obj is BooleanObject)
                    {
                        return GetMemberValue(((BooleanObject) obj).value, name, member, members);
                    }
                    if (!(obj is ArrayWrapper))
                    {
                        throw;
                    }
                    return GetMemberValue(((ArrayWrapper) obj).value, name, member, members);
                }
            }
        Label_01A4:
            if ((members != null) && (members.Length > 0))
            {
                if ((members.Length == 1) && (members[0].MemberType == MemberTypes.Method))
                {
                    MethodInfo meth = (MethodInfo) members[0];
                    Type declaringType = meth.DeclaringType;
                    if ((declaringType == typeof(GlobalObject)) || ((((declaringType != null) && (declaringType != typeof(StringObject))) && ((declaringType != typeof(NumberObject)) && (declaringType != typeof(BooleanObject)))) && declaringType.IsSubclassOf(typeof(JSObject))))
                    {
                        return Globals.BuiltinFunctionFor(obj, meth);
                    }
                }
                return new FunctionWrapper(name, obj, members);
            }
            if (obj is ScriptObject)
            {
                return ((ScriptObject) obj).GetMemberValue(name);
            }
            if (obj is Namespace)
            {
                Namespace namespace2 = (Namespace) obj;
                string typeName = namespace2.Name + "." + name;
                Type type = namespace2.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
                return Namespace.GetNamespace(typeName, namespace2.engine);
            }
            IReflect iRForObjectThatRequiresInvokeMember = GetIRForObjectThatRequiresInvokeMember(obj, true);
            if (iRForObjectThatRequiresInvokeMember != null)
            {
                try
                {
                    return iRForObjectThatRequiresInvokeMember.InvokeMember(name, BindingFlags.OptionalParamBinding | BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance, JSBinder.ob, obj, null, null, null, null);
                }
                catch (MissingMemberException)
                {
                }
                catch (COMException exception)
                {
                    int errorCode = exception.ErrorCode;
                    if ((errorCode != -2147352570) && (errorCode != -2147352573))
                    {
                        throw exception;
                    }
                }
            }
            return Microsoft.JScript.Missing.Value;
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal static object GetMemberValue2(object obj, string name)
        {
            if (obj is ScriptObject)
            {
                return ((ScriptObject) obj).GetMemberValue(name);
            }
            LateBinding binding = new LateBinding(name, obj);
            return binding.GetValue();
        }

        [DebuggerStepThrough, DebuggerHidden]
        public object GetNonMissingValue()
        {
            object obj2 = this.GetValue();
            if (obj2 is Microsoft.JScript.Missing)
            {
                return null;
            }
            return obj2;
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal object GetValue()
        {
            this.BindToMember();
            return GetMemberValue(this.obj, this.name, this.last_member, this.last_members);
        }

        [DebuggerStepThrough, DebuggerHidden]
        public object GetValue2()
        {
            object obj2 = this.GetValue();
            if (obj2 == Microsoft.JScript.Missing.Value)
            {
                throw new JScriptException(JSError.UndefinedIdentifier, new Context(new DocumentContext("", null), this.name));
            }
            return obj2;
        }

        [DebuggerHidden, DebuggerStepThrough]
        internal static object GetValueAtIndex(object obj, ulong index)
        {
            if (obj is ScriptObject)
            {
                if (index < 0xffffffffL)
                {
                    return ((ScriptObject) obj).GetValueAtIndex((uint) index);
                }
                return ((ScriptObject) obj).GetMemberValue(index.ToString(CultureInfo.InvariantCulture));
            }
        Label_0033:
            if (obj is IList)
            {
                return ((IList) obj)[(int) index];
            }
            if (obj is Array)
            {
                return ((Array) obj).GetValue((int) index);
            }
            Type t = obj.GetType();
            if ((t.IsCOMObject || (obj is IReflect)) || (index > 0x7fffffffL))
            {
                return GetMemberValue(obj, index.ToString(CultureInfo.InvariantCulture));
            }
            MethodInfo info = JSBinder.GetDefaultPropertyForArrayIndex(t, (int) index, null, false);
            if (info == null)
            {
                return Microsoft.JScript.Missing.Value;
            }
            ParameterInfo[] parameters = info.GetParameters();
            if ((parameters == null) || (parameters.Length == 0))
            {
                obj = info.Invoke(obj, BindingFlags.SuppressChangeType, null, null, null);
                goto Label_0033;
            }
            return info.Invoke(obj, BindingFlags.Default, JSBinder.ob, new object[] { (int) index }, null);
        }

        private static object[] LickArgumentsIntoShape(ParameterInfo[] pars, object[] arguments, Binder binder, CultureInfo culture)
        {
            if (arguments == null)
            {
                return null;
            }
            int length = pars.Length;
            if (length == 0)
            {
                return null;
            }
            object[] objArray = arguments;
            int num2 = arguments.Length;
            if (num2 != length)
            {
                objArray = new object[length];
            }
            int index = length - 1;
            int num4 = (num2 < index) ? num2 : index;
            for (int i = 0; i < num4; i++)
            {
                object obj2 = arguments[i];
                if (obj2 is DBNull)
                {
                    objArray[i] = null;
                }
                else
                {
                    objArray[i] = binder.ChangeType(arguments[i], pars[i].ParameterType, culture);
                }
            }
            for (int j = num4; j < index; j++)
            {
                object defaultParameterValue = TypeReferences.GetDefaultParameterValue(pars[j]);
                if (defaultParameterValue == System.Convert.DBNull)
                {
                    defaultParameterValue = binder.ChangeType(null, pars[j].ParameterType, culture);
                }
                objArray[j] = defaultParameterValue;
            }
            if (Microsoft.JScript.CustomAttribute.IsDefined(pars[index], typeof(ParamArrayAttribute), false))
            {
                int num7 = num2 - index;
                if (num7 < 0)
                {
                    num7 = 0;
                }
                Type elementType = pars[index].ParameterType.GetElementType();
                Array array = Array.CreateInstance(elementType, num7);
                for (int k = 0; k < num7; k++)
                {
                    array.SetValue(binder.ChangeType(arguments[k + index], elementType, culture), k);
                }
                objArray[index] = array;
                return objArray;
            }
            if (num2 < length)
            {
                object obj4 = TypeReferences.GetDefaultParameterValue(pars[index]);
                if (obj4 == System.Convert.DBNull)
                {
                    obj4 = binder.ChangeType(null, pars[index].ParameterType, culture);
                }
                objArray[index] = obj4;
                return objArray;
            }
            objArray[index] = binder.ChangeType(arguments[index], pars[index].ParameterType, culture);
            return objArray;
        }

        internal static MemberInfo SelectMember(MemberInfo[] mems)
        {
            if (mems == null)
            {
                return null;
            }
            MemberInfo info = null;
            foreach (MemberInfo info2 in mems)
            {
                switch (info2.MemberType)
                {
                    case MemberTypes.TypeInfo:
                    case MemberTypes.NestedType:
                        if (info == null)
                        {
                            info = info2;
                        }
                        break;

                    case MemberTypes.Field:
                        if ((info == null) || (info.MemberType != MemberTypes.Field))
                        {
                            info = info2;
                        }
                        break;

                    case MemberTypes.Property:
                        if ((info == null) || ((info.MemberType != MemberTypes.Field) && (info.MemberType != MemberTypes.Property)))
                        {
                            ParameterInfo[] indexParameters = ((PropertyInfo) info2).GetIndexParameters();
                            if ((indexParameters != null) && (indexParameters.Length == 0))
                            {
                                info = info2;
                            }
                        }
                        break;
                }
            }
            return info;
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal void SetIndexedDefaultPropertyValue(object ob, object[] arguments, object value)
        {
            ScriptObject obj2 = ob as ScriptObject;
            if (obj2 != null)
            {
                obj2[arguments] = value;
            }
            else
            {
                Array array = ob as Array;
                if (array != null)
                {
                    if (arguments.Length != array.Rank)
                    {
                        throw new JScriptException(JSError.IncorrectNumberOfIndices);
                    }
                    array.SetValue(value, ToIndices(arguments));
                }
                else
                {
                    TypeCode typeCode = Microsoft.JScript.Convert.GetTypeCode(ob);
                    if (!Microsoft.JScript.Convert.NeedsWrapper(typeCode))
                    {
                        IReflect reflect = GetIRForObjectThatRequiresInvokeMember(ob, this.checkForDebugger, typeCode);
                        if (((reflect == null) && this.checkForDebugger) && ((ob is IDebuggerObject) && (ob is IReflect)))
                        {
                            reflect = (IReflect) ob;
                        }
                        if (reflect != null)
                        {
                            try
                            {
                                int num = arguments.Length + 1;
                                object[] target = new object[num];
                                ArrayObject.Copy(arguments, 0, target, 0, num - 1);
                                target[num - 1] = value;
                                reflect.InvokeMember(string.Empty, BindingFlags.OptionalParamBinding | BindingFlags.SetProperty | BindingFlags.SetField | BindingFlags.Public | BindingFlags.Instance, JSBinder.ob, ob, target, null, null, null);
                                return;
                            }
                            catch (MissingMemberException)
                            {
                                throw new JScriptException(JSError.OLENoPropOrMethod);
                            }
                        }
                        MemberInfo[] defaultMembers = TypeReflector.GetTypeReflectorFor(ob.GetType()).GetDefaultMembers();
                        if ((defaultMembers != null) && (defaultMembers.Length > 0))
                        {
                            PropertyInfo prop = JSBinder.SelectProperty(Runtime.TypeRefs, defaultMembers, arguments);
                            if (prop != null)
                            {
                                MethodInfo setMethod = JSProperty.GetSetMethod(prop, false);
                                if (setMethod != null)
                                {
                                    arguments = LickArgumentsIntoShape(prop.GetIndexParameters(), arguments, JSBinder.ob, null);
                                    value = Microsoft.JScript.Convert.CoerceT(value, prop.PropertyType);
                                    int num2 = arguments.Length + 1;
                                    object[] objArray2 = new object[num2];
                                    ArrayObject.Copy(arguments, 0, objArray2, 0, num2 - 1);
                                    objArray2[num2 - 1] = value;
                                    setMethod.Invoke(ob, objArray2);
                                    return;
                                }
                            }
                        }
                        throw new JScriptException(JSError.OLENoPropOrMethod);
                    }
                }
            }
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal void SetIndexedPropertyValue(object[] arguments, object value)
        {
            if (this.obj == null)
            {
                throw new JScriptException(JSError.ObjectExpected);
            }
            if (this.name == null)
            {
                this.SetIndexedDefaultPropertyValue(this.obj, arguments, value);
            }
            else
            {
                this.BindToMember();
                if ((this.last_members != null) && (this.last_members.Length > 0))
                {
                    PropertyInfo prop = JSBinder.SelectProperty(Runtime.TypeRefs, this.last_members, arguments);
                    if (prop != null)
                    {
                        if (((arguments.Length > 0) && (prop.GetIndexParameters().Length == 0)) && !(prop is COMPropertyInfo))
                        {
                            MethodInfo getMethod = JSProperty.GetGetMethod(prop, false);
                            if (getMethod != null)
                            {
                                SetIndexedPropertyValueStatic(getMethod.Invoke(this.obj, null), arguments, value);
                                return;
                            }
                        }
                        arguments = LickArgumentsIntoShape(prop.GetIndexParameters(), arguments, JSBinder.ob, null);
                        value = Microsoft.JScript.Convert.CoerceT(value, prop.PropertyType);
                        JSProperty.SetValue(prop, this.obj, value, arguments);
                        return;
                    }
                }
                TypeCode typeCode = Microsoft.JScript.Convert.GetTypeCode(this.obj);
                if (!Microsoft.JScript.Convert.NeedsWrapper(typeCode))
                {
                    IReflect reflect = GetIRForObjectThatRequiresInvokeMember(this.obj, this.checkForDebugger, typeCode);
                    if (reflect != null)
                    {
                        int num = arguments.Length + 1;
                        object[] target = new object[num];
                        ArrayObject.Copy(arguments, 0, target, 0, num - 1);
                        target[num - 1] = value;
                        reflect.InvokeMember(this.name, BindingFlags.OptionalParamBinding | BindingFlags.SetProperty | BindingFlags.SetField | BindingFlags.Public | BindingFlags.Instance, JSBinder.ob, this.obj, target, null, null, null);
                    }
                    else
                    {
                        object ob = this.GetValue();
                        if ((ob == null) || (ob is Microsoft.JScript.Missing))
                        {
                            throw new JScriptException(JSError.OLENoPropOrMethod);
                        }
                        this.SetIndexedDefaultPropertyValue(ob, arguments, value);
                    }
                }
            }
        }

        [DebuggerHidden, DebuggerStepThrough]
        public static void SetIndexedPropertyValueStatic(object obj, object[] arguments, object value)
        {
            if (obj == null)
            {
                throw new JScriptException(JSError.ObjectExpected);
            }
            ScriptObject obj2 = obj as ScriptObject;
            if (obj2 != null)
            {
                obj2[arguments] = value;
            }
            else
            {
                Array array = obj as Array;
                if (array != null)
                {
                    if (arguments.Length != array.Rank)
                    {
                        throw new JScriptException(JSError.IncorrectNumberOfIndices);
                    }
                    array.SetValue(value, ToIndices(arguments));
                }
                else
                {
                    TypeCode typeCode = Microsoft.JScript.Convert.GetTypeCode(obj);
                    if (!Microsoft.JScript.Convert.NeedsWrapper(typeCode))
                    {
                        IReflect reflect = GetIRForObjectThatRequiresInvokeMember(obj, true, typeCode);
                        if (reflect != null)
                        {
                            string name = string.Empty;
                            int length = arguments.Length;
                            if (length > 0)
                            {
                                name = Microsoft.JScript.Convert.ToString(arguments[length - 1]);
                            }
                            reflect.InvokeMember(name, BindingFlags.OptionalParamBinding | BindingFlags.SetProperty | BindingFlags.SetField | BindingFlags.Public | BindingFlags.Instance, JSBinder.ob, obj, new object[] { value }, null, null, null);
                        }
                        else
                        {
                            MemberInfo[] defaultMembers = TypeReflector.GetTypeReflectorFor(obj.GetType()).GetDefaultMembers();
                            if ((defaultMembers != null) && (defaultMembers.Length > 0))
                            {
                                PropertyInfo prop = JSBinder.SelectProperty(Runtime.TypeRefs, defaultMembers, arguments);
                                if (prop != null)
                                {
                                    MethodInfo setMethod = JSProperty.GetSetMethod(prop, false);
                                    if (setMethod != null)
                                    {
                                        arguments = LickArgumentsIntoShape(prop.GetIndexParameters(), arguments, JSBinder.ob, null);
                                        value = Microsoft.JScript.Convert.CoerceT(value, prop.PropertyType);
                                        int num2 = arguments.Length + 1;
                                        object[] target = new object[num2];
                                        ArrayObject.Copy(arguments, 0, target, 0, num2 - 1);
                                        target[num2 - 1] = value;
                                        setMethod.Invoke(obj, target);
                                        return;
                                    }
                                }
                            }
                            throw new JScriptException(JSError.OLENoPropOrMethod);
                        }
                    }
                }
            }
        }

        [DebuggerHidden, DebuggerStepThrough]
        private static void SetMember(object obj, object value, MemberInfo member)
        {
            MemberTypes memberType = member.MemberType;
            if (memberType != MemberTypes.Field)
            {
                if (memberType != MemberTypes.Property)
                {
                    return;
                }
            }
            else
            {
                FieldInfo info = (FieldInfo) member;
                if (!info.IsLiteral && !info.IsInitOnly)
                {
                    if (info is JSField)
                    {
                        info.SetValue(obj, value);
                        return;
                    }
                    info.SetValue(obj, Microsoft.JScript.Convert.CoerceT(value, info.FieldType), BindingFlags.SuppressChangeType, null, null);
                }
                return;
            }
            PropertyInfo prop = (PropertyInfo) member;
            if ((prop is JSProperty) || (prop is JSWrappedProperty))
            {
                prop.SetValue(obj, value, null);
            }
            else
            {
                MethodInfo setMethod = JSProperty.GetSetMethod(prop, false);
                if (setMethod != null)
                {
                    try
                    {
                        setMethod.Invoke(obj, BindingFlags.SuppressChangeType, null, new object[] { Microsoft.JScript.Convert.CoerceT(value, prop.PropertyType) }, null);
                    }
                    catch (TargetInvocationException exception)
                    {
                        throw exception.InnerException;
                    }
                }
            }
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal static void SetMemberValue(object obj, string name, object value)
        {
            if (obj is ScriptObject)
            {
                ((ScriptObject) obj).SetMemberValue(name, value);
            }
            else
            {
                new LateBinding(name, obj).SetValue(value);
            }
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal static void SetMemberValue(object obj, string name, object value, MemberInfo member, MemberInfo[] members)
        {
            if (member != null)
            {
                SetMember(obj, value, member);
            }
            else if (obj is ScriptObject)
            {
                ((ScriptObject) obj).SetMemberValue(name, value);
            }
            else
            {
                TypeCode typeCode = Microsoft.JScript.Convert.GetTypeCode(obj);
                if (!Microsoft.JScript.Convert.NeedsWrapper(typeCode))
                {
                    IReflect reflect = GetIRForObjectThatRequiresInvokeMember(obj, true, typeCode);
                    if (reflect != null)
                    {
                        try
                        {
                            object[] args = new object[] { value };
                            BindingFlags invokeAttr = BindingFlags.OptionalParamBinding | BindingFlags.SetProperty | BindingFlags.SetField | BindingFlags.Public | BindingFlags.Instance;
                            reflect.InvokeMember(name, invokeAttr, JSBinder.ob, obj, args, null, null, null);
                            return;
                        }
                        catch (MissingMemberException)
                        {
                        }
                        catch (COMException exception)
                        {
                            int errorCode = exception.ErrorCode;
                            if ((errorCode != -2147352570) && (errorCode != -2147352573))
                            {
                                throw exception;
                            }
                        }
                    }
                    if (obj is IExpando)
                    {
                        PropertyInfo info = ((IExpando) obj).AddProperty(name);
                        if (info != null)
                        {
                            info.SetValue(obj, value, null);
                        }
                        else
                        {
                            FieldInfo info2 = ((IExpando) obj).AddField(name);
                            if (info2 != null)
                            {
                                info2.SetValue(obj, value);
                            }
                        }
                    }
                }
            }
        }

        [DebuggerHidden, DebuggerStepThrough]
        public void SetValue(object value)
        {
            this.BindToMember();
            SetMemberValue(this.obj, this.name, value, this.last_member, this.last_members);
        }

        [DebuggerStepThrough, DebuggerHidden]
        internal static void SetValueAtIndex(object obj, ulong index, object value)
        {
            if (obj is ScriptObject)
            {
                if (index < 0xffffffffL)
                {
                    ((ScriptObject) obj).SetValueAtIndex((uint) index, value);
                    return;
                }
                ((ScriptObject) obj).SetMemberValue(index.ToString(CultureInfo.InvariantCulture), value);
                return;
            }
        Label_0035:
            if (obj is IList)
            {
                IList list = (IList) obj;
                if (index < list.Count)
                {
                    list[(int) index] = value;
                    return;
                }
                list.Insert((int) index, value);
            }
            else if (obj is Array)
            {
                ((Array) obj).SetValue(Microsoft.JScript.Convert.CoerceT(value, obj.GetType().GetElementType()), (int) index);
            }
            else
            {
                Type t = obj.GetType();
                if ((t.IsCOMObject || (obj is IReflect)) || (index > 0x7fffffffL))
                {
                    SetMemberValue(obj, index.ToString(CultureInfo.InvariantCulture), value);
                }
                else
                {
                    MethodInfo info = JSBinder.GetDefaultPropertyForArrayIndex(t, (int) index, null, true);
                    if (info != null)
                    {
                        ParameterInfo[] parameters = info.GetParameters();
                        if ((parameters == null) || (parameters.Length == 0))
                        {
                            obj = info.Invoke(obj, BindingFlags.SuppressChangeType, null, null, null);
                            goto Label_0035;
                        }
                        info.Invoke(obj, BindingFlags.Default, JSBinder.ob, new object[] { (int) index, value }, null);
                    }
                }
            }
        }

        internal static void SwapValues(object obj, uint left, uint right)
        {
            object obj4;
            if (obj is JSObject)
            {
                ((JSObject) obj).SwapValues(left, right);
                return;
            }
            if (obj is IList)
            {
                IList list = (IList) obj;
                object obj2 = list[(int) left];
                list[(int) left] = list[(int) right];
                list[(int) right] = obj2;
                return;
            }
            if (obj is Array)
            {
                Array array = (Array) obj;
                object obj3 = array.GetValue((int) left);
                array.SetValue(array.GetValue((int) right), (int) left);
                array.SetValue(obj3, (int) right);
                return;
            }
            if (!(obj is IExpando))
            {
                object valueAtIndex = GetValueAtIndex(obj, (ulong) left);
                object obj6 = GetValueAtIndex(obj, (ulong) right);
                if (valueAtIndex is Microsoft.JScript.Missing)
                {
                    DeleteValueAtIndex(obj, (ulong) right);
                }
                else
                {
                    SetValueAtIndex(obj, (ulong) right, valueAtIndex);
                }
                if (obj6 is Microsoft.JScript.Missing)
                {
                    DeleteValueAtIndex(obj, (ulong) left);
                }
                else
                {
                    SetValueAtIndex(obj, (ulong) left, obj6);
                }
                return;
            }
            string name = System.Convert.ToString(left, CultureInfo.InvariantCulture);
            string str2 = System.Convert.ToString(right, CultureInfo.InvariantCulture);
            IExpando expando = (IExpando) obj;
            FieldInfo field = expando.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            FieldInfo m = expando.GetField(str2, BindingFlags.Public | BindingFlags.Instance);
            if (field == null)
            {
                if (m == null)
                {
                    return;
                }
                try
                {
                    field = expando.AddField(name);
                    field.SetValue(obj, m.GetValue(obj));
                    expando.RemoveMember(m);
                    goto Label_013C;
                }
                catch
                {
                    throw new JScriptException(JSError.ActionNotSupported);
                }
            }
            if (m == null)
            {
                try
                {
                    m = expando.AddField(str2);
                    m.SetValue(obj, field.GetValue(obj));
                    expando.RemoveMember(field);
                }
                catch
                {
                    throw new JScriptException(JSError.ActionNotSupported);
                }
            }
        Label_013C:
            obj4 = field.GetValue(obj);
            field.SetValue(obj, m.GetValue(obj));
            m.SetValue(obj, obj4);
        }

        private static int[] ToIndices(object[] arguments)
        {
            int length = arguments.Length;
            int[] numArray = new int[length];
            for (int i = 0; i < length; i++)
            {
                numArray[i] = Microsoft.JScript.Convert.ToInt32(arguments[i]);
            }
            return numArray;
        }
    }
}

