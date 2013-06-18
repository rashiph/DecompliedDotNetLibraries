namespace Microsoft.JScript
{
    using Microsoft.JScript.Vsa;
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public abstract class ScriptObject : IReflect
    {
        public VsaEngine engine;
        protected ScriptObject parent;
        internal SimpleHashtable wrappedMemberCache;

        internal ScriptObject(ScriptObject parent)
        {
            this.parent = parent;
            this.wrappedMemberCache = null;
            if (this.parent != null)
            {
                this.engine = parent.engine;
            }
            else
            {
                this.engine = null;
            }
        }

        internal virtual bool DeleteMember(string name)
        {
            return false;
        }

        internal virtual object GetDefaultValue(PreferredType preferred_type)
        {
            throw new JScriptException(JSError.InternalError);
        }

        public FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            foreach (MemberInfo info in this.GetMember(name, bindingAttr))
            {
                if (info.MemberType == MemberTypes.Field)
                {
                    return (FieldInfo) info;
                }
            }
            return null;
        }

        public virtual FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            ArrayObject obj2 = this as ArrayObject;
            if ((obj2 != null) && (obj2.denseArrayLength > 0))
            {
                uint denseArrayLength = obj2.denseArrayLength;
                if (denseArrayLength > obj2.len)
                {
                    denseArrayLength = obj2.len;
                }
                for (uint i = 0; i < denseArrayLength; i++)
                {
                    object obj3 = obj2.denseArray[i];
                    if (obj3 != Microsoft.JScript.Missing.Value)
                    {
                        obj2.SetMemberValue2(i.ToString(CultureInfo.InvariantCulture), obj3);
                    }
                }
                obj2.denseArrayLength = 0;
                obj2.denseArray = null;
            }
            MemberInfo[] members = this.GetMembers(bindingAttr);
            if (members == null)
            {
                return new FieldInfo[0];
            }
            int num3 = 0;
            foreach (MemberInfo info in members)
            {
                if (info.MemberType == MemberTypes.Field)
                {
                    num3++;
                }
            }
            FieldInfo[] infoArray2 = new FieldInfo[num3];
            num3 = 0;
            foreach (MemberInfo info2 in members)
            {
                if (info2.MemberType == MemberTypes.Field)
                {
                    infoArray2[num3++] = (FieldInfo) info2;
                }
            }
            return infoArray2;
        }

        public abstract MemberInfo[] GetMember(string name, BindingFlags bindingAttr);
        public abstract MemberInfo[] GetMembers(BindingFlags bindingAttr);
        internal virtual object GetMemberValue(string name)
        {
            MemberInfo[] member = this.GetMember(name, BindingFlags.Public | BindingFlags.Instance);
            if (member.Length == 0)
            {
                return Microsoft.JScript.Missing.Value;
            }
            return LateBinding.GetMemberValue(this, name, LateBinding.SelectMember(member), member);
        }

        public MethodInfo GetMethod(string name, BindingFlags bindingAttr)
        {
            return this.GetMethod(name, bindingAttr, JSBinder.ob, Type.EmptyTypes, null);
        }

        public MethodInfo GetMethod(string name, BindingFlags bindingAttr, Binder binder, Type[] types, ParameterModifier[] modifiers)
        {
            MemberInfo[] member = this.GetMember(name, bindingAttr);
            if (member.Length == 1)
            {
                return (member[0] as MethodInfo);
            }
            int num = 0;
            foreach (MemberInfo info in member)
            {
                if (info.MemberType == MemberTypes.Method)
                {
                    num++;
                }
            }
            if (num == 0)
            {
                return null;
            }
            MethodInfo[] match = new MethodInfo[num];
            num = 0;
            foreach (MemberInfo info2 in member)
            {
                if (info2.MemberType == MemberTypes.Method)
                {
                    match[num++] = (MethodInfo) info2;
                }
            }
            if (binder == null)
            {
                binder = JSBinder.ob;
            }
            return (MethodInfo) binder.SelectMethod(bindingAttr, match, types, modifiers);
        }

        public virtual MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            MemberInfo[] members = this.GetMembers(bindingAttr);
            if (members == null)
            {
                return new MethodInfo[0];
            }
            int num = 0;
            foreach (MemberInfo info in members)
            {
                if (info.MemberType == MemberTypes.Method)
                {
                    num++;
                }
            }
            MethodInfo[] infoArray2 = new MethodInfo[num];
            num = 0;
            foreach (MemberInfo info2 in members)
            {
                if (info2.MemberType == MemberTypes.Method)
                {
                    infoArray2[num++] = (MethodInfo) info2;
                }
            }
            return infoArray2;
        }

        public ScriptObject GetParent()
        {
            return this.parent;
        }

        public virtual PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            MemberInfo[] members = this.GetMembers(bindingAttr);
            if (members == null)
            {
                return new PropertyInfo[0];
            }
            int num = 0;
            foreach (MemberInfo info in members)
            {
                if (info.MemberType == MemberTypes.Property)
                {
                    num++;
                }
            }
            PropertyInfo[] infoArray2 = new PropertyInfo[num];
            num = 0;
            foreach (MemberInfo info2 in members)
            {
                if (info2.MemberType == MemberTypes.Property)
                {
                    infoArray2[num++] = (PropertyInfo) info2;
                }
            }
            return infoArray2;
        }

        public PropertyInfo GetProperty(string name, BindingFlags bindingAttr)
        {
            return this.GetProperty(name, bindingAttr, JSBinder.ob, null, Type.EmptyTypes, null);
        }

        public PropertyInfo GetProperty(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            MemberInfo[] member = this.GetMember(name, bindingAttr);
            if (member.Length == 1)
            {
                return (member[0] as PropertyInfo);
            }
            int num = 0;
            foreach (MemberInfo info in member)
            {
                if (info.MemberType == MemberTypes.Property)
                {
                    num++;
                }
            }
            if (num == 0)
            {
                return null;
            }
            PropertyInfo[] match = new PropertyInfo[num];
            num = 0;
            foreach (MemberInfo info2 in member)
            {
                if (info2.MemberType == MemberTypes.Property)
                {
                    match[num++] = (PropertyInfo) info2;
                }
            }
            if (binder == null)
            {
                binder = JSBinder.ob;
            }
            return binder.SelectProperty(bindingAttr, match, returnType, types, modifiers);
        }

        internal virtual void GetPropertyEnumerator(ArrayList enums, ArrayList objects)
        {
            MemberInfo[] members = this.GetMembers(BindingFlags.Public | BindingFlags.Instance);
            if (members.Length > 0)
            {
                enums.Add(members.GetEnumerator());
                objects.Add(this);
            }
            ScriptObject parent = this.GetParent();
            if (parent != null)
            {
                parent.GetPropertyEnumerator(enums, objects);
            }
        }

        internal virtual object GetValueAtIndex(uint index)
        {
            return this.GetMemberValue(index.ToString(CultureInfo.CurrentUICulture));
        }

        [DebuggerHidden, DebuggerStepThrough]
        public virtual object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo locale, string[] namedParameters)
        {
            if (target != this)
            {
                throw new TargetException();
            }
            bool flag = name.StartsWith("< JScript-", StringComparison.Ordinal);
            bool flag2 = (((name == null) || (name == string.Empty)) || name.Equals("[DISPID=0]")) || flag;
            if ((invokeAttr & BindingFlags.CreateInstance) != BindingFlags.Default)
            {
                if ((invokeAttr & (BindingFlags.PutDispProperty | BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.SetField | BindingFlags.GetField | BindingFlags.InvokeMethod)) != BindingFlags.Default)
                {
                    throw new ArgumentException(JScriptException.Localize("Bad binding flags", locale));
                }
                if (flag2)
                {
                    throw new MissingMethodException();
                }
                LateBinding binding = new LateBinding(name, this);
                return binding.Call(binder, args, modifiers, locale, namedParameters, true, false, this.engine);
            }
            if (name == null)
            {
                throw new ArgumentException(JScriptException.Localize("Bad name", locale));
            }
            if ((invokeAttr & (BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.InvokeMethod)) != BindingFlags.Default)
            {
                if ((invokeAttr & (BindingFlags.PutDispProperty | BindingFlags.SetProperty | BindingFlags.SetField)) != BindingFlags.Default)
                {
                    throw new ArgumentException(JScriptException.Localize("Bad binding flags", locale));
                }
                if (flag2)
                {
                    if ((invokeAttr & (BindingFlags.GetProperty | BindingFlags.GetField)) == BindingFlags.Default)
                    {
                        throw new MissingMethodException();
                    }
                    if ((args == null) || (args.Length == 0))
                    {
                        if ((!(this is JSObject) && !(this is GlobalScope)) && !(this is ClassScope))
                        {
                            throw new MissingFieldException();
                        }
                        PreferredType either = PreferredType.Either;
                        if (flag)
                        {
                            if (name.StartsWith("< JScript-Number", StringComparison.Ordinal))
                            {
                                either = PreferredType.Number;
                            }
                            else if (name.StartsWith("< JScript-String", StringComparison.Ordinal))
                            {
                                either = PreferredType.String;
                            }
                            else if (name.StartsWith("< JScript-LocaleString", StringComparison.Ordinal))
                            {
                                either = PreferredType.LocaleString;
                            }
                        }
                        return this.GetDefaultValue(either);
                    }
                    if (args.Length > 1)
                    {
                        throw new ArgumentException(JScriptException.Localize("Too many arguments", locale));
                    }
                    object ob = args[0];
                    if (ob is int)
                    {
                        return this[(int) ob];
                    }
                    IConvertible iConvertible = Microsoft.JScript.Convert.GetIConvertible(ob);
                    if ((iConvertible != null) && Microsoft.JScript.Convert.IsPrimitiveNumericTypeCode(iConvertible.GetTypeCode()))
                    {
                        double a = iConvertible.ToDouble(null);
                        if (((a >= 0.0) && (a <= 2147483647.0)) && (a == Math.Round(a)))
                        {
                            return this[(int) a];
                        }
                    }
                    return this[Microsoft.JScript.Convert.ToString(ob)];
                }
                if (((args == null) || (args.Length == 0)) && ((invokeAttr & (BindingFlags.GetProperty | BindingFlags.GetField)) != BindingFlags.Default))
                {
                    object memberValue = this.GetMemberValue(name);
                    if (memberValue != Microsoft.JScript.Missing.Value)
                    {
                        return memberValue;
                    }
                    if ((invokeAttr & BindingFlags.InvokeMethod) == BindingFlags.Default)
                    {
                        throw new MissingFieldException();
                    }
                }
                LateBinding binding2 = new LateBinding(name, this);
                return binding2.Call(binder, args, modifiers, locale, namedParameters, false, false, this.engine);
            }
            if ((invokeAttr & (BindingFlags.PutDispProperty | BindingFlags.SetProperty | BindingFlags.SetField)) == BindingFlags.Default)
            {
                throw new ArgumentException(JScriptException.Localize("Bad binding flags", locale));
            }
            if (flag2)
            {
                if ((args == null) || (args.Length < 2))
                {
                    throw new ArgumentException(JScriptException.Localize("Too few arguments", locale));
                }
                if (args.Length > 2)
                {
                    throw new ArgumentException(JScriptException.Localize("Too many arguments", locale));
                }
                object obj4 = args[0];
                if (obj4 is int)
                {
                    this[(int) obj4] = args[1];
                    return null;
                }
                IConvertible convertible2 = Microsoft.JScript.Convert.GetIConvertible(obj4);
                if ((convertible2 != null) && Microsoft.JScript.Convert.IsPrimitiveNumericTypeCode(convertible2.GetTypeCode()))
                {
                    double num2 = convertible2.ToDouble(null);
                    if (((num2 >= 0.0) && (num2 <= 2147483647.0)) && (num2 == Math.Round(num2)))
                    {
                        this[(int) num2] = args[1];
                        return null;
                    }
                }
                this[Microsoft.JScript.Convert.ToString(obj4)] = args[1];
                return null;
            }
            if ((args == null) || (args.Length < 1))
            {
                throw new ArgumentException(JScriptException.Localize("Too few arguments", locale));
            }
            if (args.Length > 1)
            {
                throw new ArgumentException(JScriptException.Localize("Too many arguments", locale));
            }
            this.SetMemberValue(name, args[0]);
            return null;
        }

        internal virtual void SetMemberValue(string name, object value)
        {
            MemberInfo[] member = this.GetMember(name, BindingFlags.Public | BindingFlags.Instance);
            LateBinding.SetMemberValue(this, name, value, LateBinding.SelectMember(member), member);
        }

        internal void SetParent(ScriptObject parent)
        {
            this.parent = parent;
            if (parent != null)
            {
                this.engine = parent.engine;
            }
        }

        internal virtual void SetValueAtIndex(uint index, object value)
        {
            this.SetMemberValue(index.ToString(CultureInfo.InvariantCulture), value);
        }

        internal static MemberInfo WrapMember(MemberInfo member, object obj)
        {
            MemberTypes memberType = member.MemberType;
            if (memberType != MemberTypes.Field)
            {
                if (memberType != MemberTypes.Method)
                {
                    if (memberType != MemberTypes.Property)
                    {
                        return member;
                    }
                    PropertyInfo prop = (PropertyInfo) member;
                    if (prop is JSWrappedProperty)
                    {
                        return prop;
                    }
                    MethodInfo getMethod = JSProperty.GetGetMethod(prop, true);
                    MethodInfo setMethod = JSProperty.GetSetMethod(prop, true);
                    if (((getMethod == null) || getMethod.IsStatic) && ((setMethod == null) || setMethod.IsStatic))
                    {
                        return prop;
                    }
                    return new JSWrappedProperty(prop, obj);
                }
            }
            else
            {
                FieldInfo field = (FieldInfo) member;
                if (field.IsStatic || field.IsLiteral)
                {
                    return field;
                }
                if (field is JSWrappedField)
                {
                    return field;
                }
                return new JSWrappedField(field, obj);
            }
            MethodInfo method = (MethodInfo) member;
            if (method.IsStatic)
            {
                return method;
            }
            if (method is JSWrappedMethod)
            {
                return method;
            }
            return new JSWrappedMethod(method, obj);
        }

        protected static MemberInfo[] WrapMembers(MemberInfo[] members, object obj)
        {
            if (members == null)
            {
                return null;
            }
            int length = members.Length;
            if (length == 0)
            {
                return members;
            }
            MemberInfo[] infoArray = new MemberInfo[length];
            for (int i = 0; i < length; i++)
            {
                infoArray[i] = WrapMember(members[i], obj);
            }
            return infoArray;
        }

        protected static MemberInfo[] WrapMembers(MemberInfo member, object obj)
        {
            return new MemberInfo[] { WrapMember(member, obj) };
        }

        protected static MemberInfo[] WrapMembers(MemberInfo[] members, object obj, SimpleHashtable cache)
        {
            if (members == null)
            {
                return null;
            }
            int length = members.Length;
            if (length == 0)
            {
                return members;
            }
            MemberInfo[] infoArray = new MemberInfo[length];
            for (int i = 0; i < length; i++)
            {
                MemberInfo info = (MemberInfo) cache[members[i]];
                if (null == info)
                {
                    info = WrapMember(members[i], obj);
                    cache[members[i]] = info;
                }
                infoArray[i] = info;
            }
            return infoArray;
        }

        public object this[double index]
        {
            get
            {
                object valueAtIndex;
                if (this == null)
                {
                    throw new JScriptException(JSError.ObjectExpected);
                }
                if (((index >= 0.0) && (index <= 4294967295)) && (index == Math.Round(index)))
                {
                    valueAtIndex = this.GetValueAtIndex((uint) index);
                }
                else
                {
                    valueAtIndex = this.GetMemberValue(Microsoft.JScript.Convert.ToString(index));
                }
                if (valueAtIndex is Microsoft.JScript.Missing)
                {
                    return null;
                }
                return valueAtIndex;
            }
            set
            {
                if (((index >= 0.0) && (index <= 4294967295)) && (index == Math.Round(index)))
                {
                    this.SetValueAtIndex((uint) index, value);
                }
                else
                {
                    this.SetMemberValue(Microsoft.JScript.Convert.ToString(index), value);
                }
            }
        }

        public object this[int index]
        {
            get
            {
                object valueAtIndex;
                if (this == null)
                {
                    throw new JScriptException(JSError.ObjectExpected);
                }
                if (index >= 0)
                {
                    valueAtIndex = this.GetValueAtIndex((uint) index);
                }
                else
                {
                    valueAtIndex = this.GetMemberValue(Microsoft.JScript.Convert.ToString((double) index));
                }
                if (valueAtIndex is Microsoft.JScript.Missing)
                {
                    return null;
                }
                return valueAtIndex;
            }
            set
            {
                if (this == null)
                {
                    throw new JScriptException(JSError.ObjectExpected);
                }
                if (index >= 0)
                {
                    this.SetValueAtIndex((uint) index, value);
                }
                else
                {
                    this.SetMemberValue(Microsoft.JScript.Convert.ToString((double) index), value);
                }
            }
        }

        public object this[string name]
        {
            get
            {
                if (this == null)
                {
                    throw new JScriptException(JSError.ObjectExpected);
                }
                object memberValue = this.GetMemberValue(name);
                if (memberValue is Microsoft.JScript.Missing)
                {
                    return null;
                }
                return memberValue;
            }
            set
            {
                if (this == null)
                {
                    throw new JScriptException(JSError.ObjectExpected);
                }
                this.SetMemberValue(name, value);
            }
        }

        public object this[object[] pars]
        {
            get
            {
                int length = pars.Length;
                if (length == 0)
                {
                    if (!(this is ScriptFunction) && (this != null))
                    {
                        throw new JScriptException(JSError.TooFewParameters);
                    }
                    throw new JScriptException(JSError.FunctionExpected);
                }
                if (this == null)
                {
                    throw new JScriptException(JSError.ObjectExpected);
                }
                object ob = pars[length - 1];
                if (ob is int)
                {
                    return this[(int) ob];
                }
                IConvertible iConvertible = Microsoft.JScript.Convert.GetIConvertible(ob);
                if ((iConvertible != null) && Microsoft.JScript.Convert.IsPrimitiveNumericTypeCode(iConvertible.GetTypeCode()))
                {
                    double a = iConvertible.ToDouble(null);
                    if (((a >= 0.0) && (a <= 2147483647.0)) && (a == Math.Round(a)))
                    {
                        return this[(int) a];
                    }
                }
                return this[Microsoft.JScript.Convert.ToString(ob)];
            }
            set
            {
                int length = pars.Length;
                if (length == 0)
                {
                    if (this == null)
                    {
                        throw new JScriptException(JSError.FunctionExpected);
                    }
                    if (this is ScriptFunction)
                    {
                        throw new JScriptException(JSError.CannotAssignToFunctionResult);
                    }
                    throw new JScriptException(JSError.TooFewParameters);
                }
                if (this == null)
                {
                    throw new JScriptException(JSError.ObjectExpected);
                }
                object ob = pars[length - 1];
                if (ob is int)
                {
                    this[(int) ob] = value;
                }
                else
                {
                    IConvertible iConvertible = Microsoft.JScript.Convert.GetIConvertible(ob);
                    if ((iConvertible != null) && Microsoft.JScript.Convert.IsPrimitiveNumericTypeCode(iConvertible.GetTypeCode()))
                    {
                        double a = iConvertible.ToDouble(null);
                        if (((a >= 0.0) && (a <= 2147483647.0)) && (a == Math.Round(a)))
                        {
                            this[(int) a] = value;
                            return;
                        }
                    }
                    this[Microsoft.JScript.Convert.ToString(ob)] = value;
                }
            }
        }

        public virtual Type UnderlyingSystemType
        {
            get
            {
                return base.GetType();
            }
        }
    }
}

