namespace Microsoft.JScript
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [Serializable]
    internal sealed class JSBinder : Binder
    {
        internal static readonly JSBinder ob = new JSBinder();

        internal static object[] ArrangeNamedArguments(MethodBase method, object[] args, string[] namedParameters)
        {
            ParameterInfo[] parameters = method.GetParameters();
            int length = parameters.Length;
            if (length == 0)
            {
                throw new JScriptException(JSError.MissingNameParameter);
            }
            object[] target = new object[length];
            int num2 = args.Length;
            int i = namedParameters.Length;
            int n = num2 - i;
            ArrayObject.Copy(args, i, target, 0, n);
            for (int j = 0; j < i; j++)
            {
                string str = namedParameters[j];
                if ((str == null) || str.Equals(""))
                {
                    throw new JScriptException(JSError.MustProvideNameForNamedParameter);
                }
                int index = n;
                while (index < length)
                {
                    if (str.Equals(parameters[index].Name))
                    {
                        if (target[index] is Microsoft.JScript.Empty)
                        {
                            throw new JScriptException(JSError.DuplicateNamedParameter);
                        }
                        target[index] = args[j];
                        break;
                    }
                    index++;
                }
                if (index == length)
                {
                    throw new JScriptException(JSError.MissingNameParameter);
                }
            }
            if (!(method is JSMethod))
            {
                for (int k = 0; k < length; k++)
                {
                    if ((target[k] == null) || (target[k] == Microsoft.JScript.Missing.Value))
                    {
                        object defaultParameterValue = TypeReferences.GetDefaultParameterValue(parameters[k]);
                        if (defaultParameterValue == System.Convert.DBNull)
                        {
                            throw new ArgumentException(parameters[k].Name);
                        }
                        target[k] = defaultParameterValue;
                    }
                }
            }
            return target;
        }

        public override FieldInfo BindToField(BindingFlags bindAttr, FieldInfo[] match, object value, CultureInfo locale)
        {
            if (value == null)
            {
                value = DBNull.Value;
            }
            int num = 0x7fffffff;
            int num2 = 0;
            FieldInfo info = null;
            Type actual = value.GetType();
            int index = 0;
            int length = match.Length;
            while (index < length)
            {
                FieldInfo info2 = match[index];
                int num5 = TypeDistance(Runtime.TypeRefs, info2.FieldType, actual);
                if (num5 < num)
                {
                    num = num5;
                    info = info2;
                    num2 = 0;
                }
                else if (num5 == num)
                {
                    num2++;
                }
                index++;
            }
            if (num2 > 0)
            {
                throw new AmbiguousMatchException();
            }
            return info;
        }

        public override MethodBase BindToMethod(BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers, CultureInfo locale, string[] namedParameters, out object state)
        {
            state = null;
            return SelectMethodBase(Runtime.TypeRefs, match, ref args, modifiers, namedParameters);
        }

        public override object ChangeType(object value, Type target_type, CultureInfo locale)
        {
            return Microsoft.JScript.Convert.CoerceT(value, target_type);
        }

        private static bool FormalParamTypeIsObject(ParameterInfo par)
        {
            ParameterDeclaration declaration = par as ParameterDeclaration;
            if (declaration != null)
            {
                return (declaration.ParameterIReflect == Typeob.Object);
            }
            return (par.ParameterType == Typeob.Object);
        }

        internal static MemberInfo[] GetDefaultMembers(IReflect ir)
        {
            return GetDefaultMembers(Globals.TypeRefs, ir);
        }

        internal static MemberInfo[] GetDefaultMembers(Type t)
        {
            while ((t != typeof(object)) && (t != null))
            {
                MemberInfo[] defaultMembers = t.GetDefaultMembers();
                if ((defaultMembers != null) && (defaultMembers.Length > 0))
                {
                    return defaultMembers;
                }
                t = t.BaseType;
            }
            return null;
        }

        internal static MemberInfo[] GetDefaultMembers(TypeReferences typeRefs, IReflect ir)
        {
            while (ir is ClassScope)
            {
                ClassScope scope = (ClassScope) ir;
                scope.owner.IsExpando();
                if (scope.itemProp != null)
                {
                    return new MemberInfo[] { scope.itemProp };
                }
                ir = scope.GetParent();
                if (ir is WithObject)
                {
                    ir = (IReflect) ((WithObject) ir).contained_object;
                }
            }
            if (ir is Type)
            {
                return GetDefaultMembers((Type) ir);
            }
            if (ir is JSObject)
            {
                return typeRefs.ScriptObject.GetDefaultMembers();
            }
            return null;
        }

        internal static MethodInfo GetDefaultPropertyForArrayIndex(Type t, int index, Type elementType, bool getSetter)
        {
            try
            {
                MemberInfo[] defaultMembers = GetDefaultMembers(Runtime.TypeRefs, t);
                int num = 0;
                if ((defaultMembers == null) || ((num = defaultMembers.Length) == 0))
                {
                    return null;
                }
                for (int i = 0; i < num; i++)
                {
                    MemberInfo info = defaultMembers[i];
                    MemberTypes memberType = info.MemberType;
                    MethodInfo getMethod = null;
                    MemberTypes types2 = memberType;
                    if (types2 != MemberTypes.Method)
                    {
                        if (types2 != MemberTypes.Property)
                        {
                            continue;
                        }
                        getMethod = ((PropertyInfo) info).GetGetMethod();
                    }
                    else
                    {
                        getMethod = (MethodInfo) info;
                    }
                    if (getMethod != null)
                    {
                        ParameterInfo[] parameters = getMethod.GetParameters();
                        if ((parameters == null) || (parameters.Length == 0))
                        {
                            Type returnType = getMethod.ReturnType;
                            if (typeof(Array).IsAssignableFrom(returnType) || typeof(IList).IsAssignableFrom(returnType))
                            {
                                return getMethod;
                            }
                        }
                        else if ((parameters.Length == 1) && (memberType == MemberTypes.Property))
                        {
                            PropertyInfo info3 = (PropertyInfo) info;
                            if ((elementType == null) || info3.PropertyType.IsAssignableFrom(elementType))
                            {
                                try
                                {
                                    Microsoft.JScript.Convert.CoerceT(index, parameters[0].ParameterType);
                                    if (getSetter)
                                    {
                                        return info3.GetSetMethod();
                                    }
                                    return getMethod;
                                }
                                catch (JScriptException)
                                {
                                }
                            }
                        }
                    }
                }
            }
            catch (InvalidOperationException)
            {
            }
            return null;
        }

        internal static MemberInfo[] GetInterfaceMembers(string name, Type t)
        {
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            MemberInfo[] member = t.GetMember(name, bindingAttr);
            Type[] interfaces = t.GetInterfaces();
            if ((interfaces == null) || (interfaces.Length == 0))
            {
                return member;
            }
            ArrayList list = new ArrayList(interfaces);
            MemberInfoList list2 = new MemberInfoList();
            list2.AddRange(member);
            for (int i = 0; i < list.Count; i++)
            {
                Type type = (Type) list[i];
                member = type.GetMember(name, bindingAttr);
                if (member != null)
                {
                    list2.AddRange(member);
                }
                foreach (Type type2 in type.GetInterfaces())
                {
                    if (list.IndexOf(type2) == -1)
                    {
                        list.Add(type2);
                    }
                }
            }
            return list2.ToArray();
        }

        internal static Type HandleCoClassAttribute(Type t)
        {
            object[] objArray = Microsoft.JScript.CustomAttribute.GetCustomAttributes(t, typeof(CoClassAttribute), false);
            if ((objArray != null) && (objArray.Length == 1))
            {
                t = ((CoClassAttribute) objArray[0]).CoClass;
                if (!t.IsPublic)
                {
                    throw new JScriptException(JSError.NotAccessible, new Context(new DocumentContext("", null), t.ToString()));
                }
            }
            return t;
        }

        public override void ReorderArgumentArray(ref object[] args, object state)
        {
        }

        internal static MemberInfo Select(TypeReferences typeRefs, MemberInfo[] match, int matches, IReflect[] argIRs, MemberTypes memberType)
        {
            int candidates = 0;
            ParameterInfo[][] fparams = new ParameterInfo[matches][];
            bool flag = memberType == MemberTypes.Method;
            for (int i = 0; i < matches; i++)
            {
                MemberInfo getMethod = match[i];
                if ((getMethod is PropertyInfo) && flag)
                {
                    getMethod = ((PropertyInfo) getMethod).GetGetMethod(true);
                }
                if ((getMethod != null) && (getMethod.MemberType == memberType))
                {
                    if (getMethod is PropertyInfo)
                    {
                        fparams[i] = ((PropertyInfo) getMethod).GetIndexParameters();
                    }
                    else
                    {
                        fparams[i] = ((MethodBase) getMethod).GetParameters();
                    }
                    candidates++;
                }
            }
            int index = SelectBest(typeRefs, match, matches, argIRs, fparams, null, candidates, argIRs.Length);
            if (index < 0)
            {
                return null;
            }
            return match[index];
        }

        internal static MemberInfo Select(TypeReferences typeRefs, MemberInfo[] match, int matches, ref object[] args, string[] namedParameters, MemberTypes memberType)
        {
            bool flag = false;
            if ((namedParameters != null) && (namedParameters.Length > 0))
            {
                if (args.Length < namedParameters.Length)
                {
                    throw new JScriptException(JSError.MoreNamedParametersThanArguments);
                }
                flag = true;
            }
            int candidates = 0;
            ParameterInfo[][] fparams = new ParameterInfo[matches][];
            object[][] aparams = new object[matches][];
            bool flag2 = memberType == MemberTypes.Method;
            for (int i = 0; i < matches; i++)
            {
                MemberInfo getMethod = match[i];
                if (flag2 && (getMethod.MemberType == MemberTypes.Property))
                {
                    getMethod = ((PropertyInfo) getMethod).GetGetMethod(true);
                }
                if (getMethod.MemberType == memberType)
                {
                    if (memberType == MemberTypes.Property)
                    {
                        fparams[i] = ((PropertyInfo) getMethod).GetIndexParameters();
                    }
                    else
                    {
                        fparams[i] = ((MethodBase) getMethod).GetParameters();
                    }
                    if (flag)
                    {
                        aparams[i] = ArrangeNamedArguments((MethodBase) getMethod, args, namedParameters);
                    }
                    else
                    {
                        aparams[i] = args;
                    }
                    candidates++;
                }
            }
            int index = SelectBest(typeRefs, match, matches, null, fparams, aparams, candidates, args.Length);
            if (index < 0)
            {
                return null;
            }
            args = aparams[index];
            MemberInfo info2 = match[index];
            if (flag2 && (info2.MemberType == MemberTypes.Property))
            {
                info2 = ((PropertyInfo) info2).GetGetMethod(true);
            }
            return info2;
        }

        private static int SelectBest(TypeReferences typeRefs, MemberInfo[] match, int matches, IReflect[] argIRs, ParameterInfo[][] fparams, object[][] aparams, int candidates, int parameters)
        {
            if (candidates == 0)
            {
                return -1;
            }
            if (candidates == 1)
            {
                for (int m = 0; m < matches; m++)
                {
                    if (fparams[m] != null)
                    {
                        return m;
                    }
                }
            }
            bool[] flagArray = new bool[matches];
            int[] numArray = new int[matches];
            for (int i = 0; i < matches; i++)
            {
                ParameterInfo[] infoArray = fparams[i];
                if (infoArray != null)
                {
                    int length = infoArray.Length;
                    int num4 = (argIRs == null) ? aparams[i].Length : argIRs.Length;
                    if ((num4 > length) && ((length == 0) || !Microsoft.JScript.CustomAttribute.IsDefined(infoArray[length - 1], typeof(ParamArrayAttribute), false)))
                    {
                        fparams[i] = null;
                        candidates--;
                    }
                    else
                    {
                        for (int n = parameters; n < length; n++)
                        {
                            ParameterInfo target = infoArray[n];
                            if ((n == (length - 1)) && Microsoft.JScript.CustomAttribute.IsDefined(target, typeof(ParamArrayAttribute), false))
                            {
                                break;
                            }
                            if (TypeReferences.GetDefaultParameterValue(target) is DBNull)
                            {
                                numArray[i] = 50;
                            }
                        }
                    }
                }
            }
            for (int j = 0; candidates > 1; j++)
            {
                int num7 = 0;
                int num8 = 0x7fffffff;
                bool flag = false;
                for (int num9 = 0; num9 < matches; num9++)
                {
                    int num10 = 0;
                    ParameterInfo[] infoArray2 = fparams[num9];
                    if (infoArray2 != null)
                    {
                        IReflect missing = typeRefs.Missing;
                        if (argIRs == null)
                        {
                            if (aparams[num9].Length > j)
                            {
                                object obj3 = aparams[num9][j];
                                if (obj3 == null)
                                {
                                    obj3 = DBNull.Value;
                                }
                                missing = typeRefs.ToReferenceContext(obj3.GetType());
                            }
                        }
                        else if (j < parameters)
                        {
                            missing = argIRs[j];
                        }
                        int num11 = infoArray2.Length;
                        if ((num11 - 1) > j)
                        {
                            num7++;
                        }
                        IReflect formal = typeRefs.Missing;
                        if ((((num11 > 0) && (j >= (num11 - 1))) && (Microsoft.JScript.CustomAttribute.IsDefined(infoArray2[num11 - 1], typeof(ParamArrayAttribute), false) && !(missing is TypedArray))) && ((missing != typeRefs.ArrayObject) && (!(missing is Type) || !((Type) missing).IsArray)))
                        {
                            ParameterInfo info2 = infoArray2[num11 - 1];
                            if (info2 is ParameterDeclaration)
                            {
                                formal = ((TypedArray) ((ParameterDeclaration) info2).ParameterIReflect).elementType;
                            }
                            else
                            {
                                formal = info2.ParameterType.GetElementType();
                            }
                            if (j == (num11 - 1))
                            {
                                numArray[num9]++;
                            }
                        }
                        else if (j < num11)
                        {
                            ParameterInfo parameter = infoArray2[j];
                            formal = (parameter is ParameterDeclaration) ? ((ParameterDeclaration) parameter).ParameterIReflect : parameter.ParameterType;
                            if ((missing == typeRefs.Missing) && !(TypeReferences.GetDefaultParameterValue(parameter) is DBNull))
                            {
                                missing = formal;
                                num10 = 1;
                            }
                        }
                        int num12 = (TypeDistance(typeRefs, formal, missing) + numArray[num9]) + num10;
                        if (num12 == num8)
                        {
                            if ((j == (num11 - 1)) && flagArray[num9])
                            {
                                candidates--;
                                fparams[num9] = null;
                            }
                            flag = flag && flagArray[num9];
                        }
                        else if (num12 > num8)
                        {
                            if ((flag && (j < num11)) && FormalParamTypeIsObject(fparams[num9][j]))
                            {
                                num8 = num12;
                            }
                            else if (((j <= (num11 - 1)) || (missing != typeRefs.Missing)) || !Microsoft.JScript.CustomAttribute.IsDefined(infoArray2[num11 - 1], typeof(ParamArrayAttribute), false))
                            {
                                flagArray[num9] = true;
                            }
                        }
                        else
                        {
                            if ((candidates == 1) && !flagArray[num9])
                            {
                                return num9;
                            }
                            flag = flagArray[num9];
                            for (int num13 = 0; num13 < num9; num13++)
                            {
                                if ((fparams[num13] != null) && !flagArray[num13])
                                {
                                    bool flag2 = fparams[num13].Length <= j;
                                    if ((!flag2 || (parameters > j)) && ((flag2 || !flag) || !FormalParamTypeIsObject(fparams[num13][j])))
                                    {
                                        flagArray[num13] = true;
                                    }
                                }
                            }
                            num8 = num12;
                        }
                    }
                }
                if ((j >= (parameters - 1)) && (num7 < 1))
                {
                    break;
                }
            }
            int index = -1;
            for (int k = 0; (k < matches) && (candidates > 0); k++)
            {
                ParameterInfo[] suppars = fparams[k];
                if (suppars != null)
                {
                    if (flagArray[k])
                    {
                        candidates--;
                        fparams[k] = null;
                    }
                    else if (index == -1)
                    {
                        index = k;
                    }
                    else if (Class.ParametersMatch(suppars, fparams[index]))
                    {
                        MemberInfo info4 = match[index];
                        JSWrappedMethod method = match[index] as JSWrappedMethod;
                        if (method != null)
                        {
                            info4 = method.method;
                        }
                        if (((info4 is JSFieldMethod) || (info4 is JSConstructor)) || (info4 is JSProperty))
                        {
                            candidates--;
                            fparams[k] = null;
                        }
                        else
                        {
                            Type declaringType = match[index].DeclaringType;
                            Type c = match[k].DeclaringType;
                            if (declaringType != c)
                            {
                                if (c.IsAssignableFrom(declaringType))
                                {
                                    candidates--;
                                    fparams[k] = null;
                                }
                                else if (declaringType.IsAssignableFrom(c))
                                {
                                    fparams[index] = null;
                                    index = k;
                                    candidates--;
                                }
                            }
                        }
                    }
                }
            }
            if (candidates != 1)
            {
                throw new AmbiguousMatchException();
            }
            return index;
        }

        internal static MemberInfo SelectCallableMember(MemberInfo[] match, IReflect[] argIRs)
        {
            if (match == null)
            {
                return null;
            }
            int length = match.Length;
            if (length == 0)
            {
                return null;
            }
            return ((length == 1) ? match[0] : Select(Globals.TypeRefs, match, length, argIRs, MemberTypes.Method));
        }

        internal static ConstructorInfo SelectConstructor(MemberInfo[] match, IReflect[] argIRs)
        {
            return SelectConstructor(Globals.TypeRefs, match, argIRs);
        }

        internal static ConstructorInfo SelectConstructor(TypeReferences typeRefs, MemberInfo[] match, IReflect[] argIRs)
        {
            if (match == null)
            {
                return null;
            }
            int length = match.Length;
            if (length == 1)
            {
                object obj2 = match[0];
                if (obj2 is JSGlobalField)
                {
                    obj2 = ((JSGlobalField) obj2).GetValue(null);
                }
                Type t = obj2 as Type;
                if (t != null)
                {
                    if (t.IsInterface && t.IsImport)
                    {
                        t = HandleCoClassAttribute(t);
                    }
                    match = t.GetConstructors();
                }
                length = match.Length;
            }
            switch (length)
            {
                case 0:
                    return null;

                case 1:
                    return (match[0] as ConstructorInfo);
            }
            return (ConstructorInfo) Select(typeRefs, match, length, argIRs, MemberTypes.Constructor);
        }

        internal static ConstructorInfo SelectConstructor(MemberInfo[] match, ref object[] args, string[] namedParameters)
        {
            return SelectConstructor(Globals.TypeRefs, match, ref args, namedParameters);
        }

        internal static ConstructorInfo SelectConstructor(TypeReferences typeRefs, MemberInfo[] match, ref object[] args, string[] namedParameters)
        {
            if (match == null)
            {
                return null;
            }
            int length = match.Length;
            switch (length)
            {
                case 0:
                    return null;

                case 1:
                {
                    Type t = match[0] as Type;
                    if (t != null)
                    {
                        if (t.IsInterface && t.IsImport)
                        {
                            t = HandleCoClassAttribute(t);
                        }
                        match = t.GetConstructors();
                        length = match.Length;
                    }
                    break;
                }
            }
            if (length == 1)
            {
                return (match[0] as ConstructorInfo);
            }
            return (ConstructorInfo) Select(typeRefs, match, length, ref args, namedParameters, MemberTypes.Constructor);
        }

        internal static MethodInfo SelectMethod(MemberInfo[] match, IReflect[] argIRs)
        {
            return SelectMethod(Globals.TypeRefs, match, argIRs);
        }

        internal static MethodInfo SelectMethod(TypeReferences typeRefs, MemberInfo[] match, IReflect[] argIRs)
        {
            if (match == null)
            {
                return null;
            }
            int length = match.Length;
            if (length == 0)
            {
                return null;
            }
            MemberInfo info = (length == 1) ? match[0] : Select(typeRefs, match, length, argIRs, MemberTypes.Method);
            if ((info != null) && (info.MemberType == MemberTypes.Property))
            {
                return ((PropertyInfo) info).GetGetMethod(true);
            }
            return (info as MethodInfo);
        }

        internal static MethodInfo SelectMethod(MemberInfo[] match, ref object[] args, string[] namedParameters)
        {
            return SelectMethod(Globals.TypeRefs, match, ref args, namedParameters);
        }

        internal static MethodInfo SelectMethod(TypeReferences typeRefs, MemberInfo[] match, ref object[] args, string[] namedParameters)
        {
            if (match == null)
            {
                return null;
            }
            int length = match.Length;
            if (length == 0)
            {
                return null;
            }
            MemberInfo getMethod = (length == 1) ? match[0] : Select(typeRefs, match, length, ref args, namedParameters, MemberTypes.Method);
            if ((getMethod != null) && (getMethod.MemberType == MemberTypes.Property))
            {
                getMethod = ((PropertyInfo) getMethod).GetGetMethod(true);
            }
            return (getMethod as MethodInfo);
        }

        public override MethodBase SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
        {
            if (match == null)
            {
                return null;
            }
            int length = match.Length;
            switch (length)
            {
                case 0:
                    return null;

                case 1:
                    return match[0];
            }
            if (match[0].MemberType == MemberTypes.Constructor)
            {
                return (ConstructorInfo) Select(Runtime.TypeRefs, match, length, types, MemberTypes.Constructor);
            }
            return (MethodInfo) Select(Runtime.TypeRefs, match, length, types, MemberTypes.Method);
        }

        private static MethodBase SelectMethodBase(TypeReferences typeRefs, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers, string[] namedParameters)
        {
            if (match == null)
            {
                return null;
            }
            int length = match.Length;
            switch (length)
            {
                case 0:
                    return null;

                case 1:
                    return match[0];
            }
            MethodBase base2 = (MethodBase) Select(typeRefs, match, length, ref args, namedParameters, MemberTypes.Method);
            if (base2 == null)
            {
                base2 = (MethodBase) Select(typeRefs, match, length, ref args, namedParameters, MemberTypes.Constructor);
            }
            return base2;
        }

        internal static MethodInfo SelectOperator(MethodInfo op1, MethodInfo op2, Type t1, Type t2)
        {
            ParameterInfo[] infoArray = null;
            if (((op1 == null) || ((op1.Attributes & MethodAttributes.SpecialName) == MethodAttributes.PrivateScope)) || ((infoArray = op1.GetParameters()).Length != 2))
            {
                op1 = null;
            }
            ParameterInfo[] infoArray2 = null;
            if (((op2 == null) || ((op2.Attributes & MethodAttributes.SpecialName) == MethodAttributes.PrivateScope)) || ((infoArray2 = op2.GetParameters()).Length != 2))
            {
                op2 = null;
            }
            if (op1 != null)
            {
                if (op2 == null)
                {
                    return op1;
                }
                int num = TypeDistance(Globals.TypeRefs, infoArray[0].ParameterType, t1) + TypeDistance(Globals.TypeRefs, infoArray[1].ParameterType, t2);
                int num2 = TypeDistance(Globals.TypeRefs, infoArray2[0].ParameterType, t1) + TypeDistance(Globals.TypeRefs, infoArray2[1].ParameterType, t2);
                if (num <= num2)
                {
                    return op1;
                }
            }
            return op2;
        }

        internal static PropertyInfo SelectProperty(MemberInfo[] match, object[] args)
        {
            return SelectProperty(Globals.TypeRefs, match, args);
        }

        internal static PropertyInfo SelectProperty(MemberInfo[] match, IReflect[] argIRs)
        {
            return SelectProperty(Globals.TypeRefs, match, argIRs);
        }

        internal static PropertyInfo SelectProperty(TypeReferences typeRefs, MemberInfo[] match, object[] args)
        {
            if (match == null)
            {
                return null;
            }
            int length = match.Length;
            switch (length)
            {
                case 0:
                    return null;

                case 1:
                    return (match[0] as PropertyInfo);
            }
            int candidates = 0;
            PropertyInfo info = null;
            ParameterInfo[][] fparams = new ParameterInfo[length][];
            object[][] aparams = new object[length][];
            for (int i = 0; i < length; i++)
            {
                MemberInfo info2 = match[i];
                if (info2.MemberType == MemberTypes.Property)
                {
                    MethodInfo getMethod = (info = (PropertyInfo) info2).GetGetMethod(true);
                    if (getMethod == null)
                    {
                        fparams[i] = info.GetIndexParameters();
                    }
                    else
                    {
                        fparams[i] = getMethod.GetParameters();
                    }
                    aparams[i] = args;
                    candidates++;
                }
            }
            if (candidates <= 1)
            {
                return info;
            }
            int index = SelectBest(typeRefs, match, length, null, fparams, aparams, candidates, args.Length);
            if (index < 0)
            {
                return null;
            }
            return (PropertyInfo) match[index];
        }

        internal static PropertyInfo SelectProperty(TypeReferences typeRefs, MemberInfo[] match, IReflect[] argIRs)
        {
            if (match == null)
            {
                return null;
            }
            int length = match.Length;
            switch (length)
            {
                case 0:
                    return null;

                case 1:
                    return (match[0] as PropertyInfo);
            }
            return (PropertyInfo) Select(typeRefs, match, length, argIRs, MemberTypes.Property);
        }

        public override PropertyInfo SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type rtype, Type[] types, ParameterModifier[] modifiers)
        {
            if (match == null)
            {
                return null;
            }
            int length = match.Length;
            switch (length)
            {
                case 0:
                    return null;

                case 1:
                    return match[0];
            }
            int candidates = 0;
            PropertyInfo info = null;
            int num3 = 0x7fffffff;
            ParameterInfo[][] fparams = new ParameterInfo[length][];
            for (int i = 0; i < length; i++)
            {
                info = match[i];
                if (rtype != null)
                {
                    int num5 = TypeDistance(Globals.TypeRefs, info.PropertyType, rtype);
                    if (num5 > num3)
                    {
                        continue;
                    }
                    if (num5 < num3)
                    {
                        for (int j = 0; j < i; j++)
                        {
                            if (fparams[j] != null)
                            {
                                fparams[j] = null;
                                candidates--;
                            }
                        }
                    }
                }
                fparams[i] = info.GetIndexParameters();
                candidates++;
            }
            if (candidates <= 1)
            {
                return info;
            }
            int index = SelectBest(Globals.TypeRefs, match, length, types, fparams, null, candidates, types.Length);
            if (index < 0)
            {
                return null;
            }
            return match[index];
        }

        private static int TypeDistance(TypeReferences typeRefs, IReflect formal, IReflect actual)
        {
            if (formal is TypedArray)
            {
                if (actual is TypedArray)
                {
                    TypedArray array = (TypedArray) formal;
                    TypedArray array2 = (TypedArray) actual;
                    if (array.rank == array2.rank)
                    {
                        if (TypeDistance(typeRefs, array.elementType, array2.elementType) != 0)
                        {
                            return 100;
                        }
                        return 0;
                    }
                }
                else if (actual is Type)
                {
                    TypedArray array3 = (TypedArray) formal;
                    Type type = (Type) actual;
                    if (type.IsArray && (array3.rank == type.GetArrayRank()))
                    {
                        if (TypeDistance(typeRefs, array3.elementType, type.GetElementType()) != 0)
                        {
                            return 100;
                        }
                        return 0;
                    }
                    if ((type == typeRefs.Array) || (type == typeRefs.ArrayObject))
                    {
                        return 30;
                    }
                }
                return 100;
            }
            if (actual is TypedArray)
            {
                if (formal is Type)
                {
                    Type type2 = (Type) formal;
                    TypedArray array4 = (TypedArray) actual;
                    if (type2.IsArray && (type2.GetArrayRank() == array4.rank))
                    {
                        if (TypeDistance(typeRefs, type2.GetElementType(), array4.elementType) != 0)
                        {
                            return 100;
                        }
                        return 0;
                    }
                    if (type2 == typeRefs.Array)
                    {
                        return 30;
                    }
                    if (type2 == typeRefs.Object)
                    {
                        return 50;
                    }
                }
                return 100;
            }
            if (formal is ClassScope)
            {
                if (!(actual is ClassScope))
                {
                    return 100;
                }
                if (!((ClassScope) actual).IsSameOrDerivedFrom((ClassScope) formal))
                {
                    return 100;
                }
                return 0;
            }
            if (!(actual is ClassScope))
            {
                return TypeDistance(typeRefs, Microsoft.JScript.Convert.ToType(typeRefs, formal), Microsoft.JScript.Convert.ToType(typeRefs, actual));
            }
            if (!(formal is Type))
            {
                return 100;
            }
            if (!((ClassScope) actual).IsPromotableTo((Type) formal))
            {
                return 100;
            }
            return 0;
        }

        private static int TypeDistance(TypeReferences typeRefs, Type formal, Type actual)
        {
            TypeCode typeCode = Type.GetTypeCode(actual);
            TypeCode code2 = Type.GetTypeCode(formal);
            if (actual.IsEnum)
            {
                typeCode = TypeCode.Object;
            }
            if (formal.IsEnum)
            {
                code2 = TypeCode.Object;
            }
            switch (typeCode)
            {
                case TypeCode.Object:
                    if (!(formal == actual))
                    {
                        int num;
                        if (formal == typeRefs.Missing)
                        {
                            return 200;
                        }
                        if (!formal.IsAssignableFrom(actual))
                        {
                            if (typeRefs.Array.IsAssignableFrom(formal) && ((actual == typeRefs.Array) || typeRefs.ArrayObject.IsAssignableFrom(actual)))
                            {
                                return 10;
                            }
                            if (code2 == TypeCode.String)
                            {
                                return 20;
                            }
                            if ((actual == typeRefs.ScriptFunction) && typeRefs.Delegate.IsAssignableFrom(formal))
                            {
                                return 0x13;
                            }
                            return 100;
                        }
                        Type[] interfaces = actual.GetInterfaces();
                        int length = interfaces.Length;
                        for (num = 0; num < length; num++)
                        {
                            if (formal == interfaces[num])
                            {
                                return (num + 1);
                            }
                        }
                        num = 0;
                        while ((actual != typeRefs.Object) && (actual != null))
                        {
                            if (formal == actual)
                            {
                                return ((num + length) + 1);
                            }
                            actual = actual.BaseType;
                            num++;
                        }
                        return ((num + length) + 1);
                    }
                    return 0;

                case TypeCode.DBNull:
                    if (formal == typeRefs.Object)
                    {
                        return 0;
                    }
                    return 1;

                case TypeCode.Boolean:
                    switch (code2)
                    {
                        case TypeCode.Object:
                            return TypeDistance(typeRefs, formal, actual, 12);

                        case TypeCode.Boolean:
                            return 0;

                        case TypeCode.SByte:
                            return 5;

                        case TypeCode.Byte:
                            return 1;

                        case TypeCode.Int16:
                            return 6;

                        case TypeCode.UInt16:
                            return 2;

                        case TypeCode.Int32:
                            return 7;

                        case TypeCode.UInt32:
                            return 3;

                        case TypeCode.Int64:
                            return 8;

                        case TypeCode.UInt64:
                            return 4;

                        case TypeCode.Single:
                            return 9;

                        case TypeCode.Double:
                            return 10;

                        case TypeCode.Decimal:
                            return 11;

                        case TypeCode.String:
                            return 13;
                    }
                    break;

                case TypeCode.Char:
                    switch (code2)
                    {
                        case TypeCode.Object:
                            return TypeDistance(typeRefs, formal, actual, 9);

                        case TypeCode.Boolean:
                            return 14;

                        case TypeCode.Char:
                            return 0;

                        case TypeCode.SByte:
                            return 13;

                        case TypeCode.Byte:
                            return 12;

                        case TypeCode.Int16:
                            return 11;

                        case TypeCode.UInt16:
                            return 1;

                        case TypeCode.Int32:
                            return 3;

                        case TypeCode.UInt32:
                            return 2;

                        case TypeCode.Int64:
                            return 5;

                        case TypeCode.UInt64:
                            return 4;

                        case TypeCode.Single:
                            return 6;

                        case TypeCode.Double:
                            return 7;

                        case TypeCode.Decimal:
                            return 8;

                        case TypeCode.String:
                            return 10;
                    }
                    return 100;

                case TypeCode.SByte:
                    switch (code2)
                    {
                        case TypeCode.Object:
                            return TypeDistance(typeRefs, formal, actual, 7);

                        case TypeCode.Boolean:
                            return 14;

                        case TypeCode.SByte:
                            return 0;

                        case TypeCode.Byte:
                            return 9;

                        case TypeCode.Int16:
                            return 1;

                        case TypeCode.UInt16:
                            return 10;

                        case TypeCode.Int32:
                            return 2;

                        case TypeCode.UInt32:
                            return 12;

                        case TypeCode.Int64:
                            return 3;

                        case TypeCode.UInt64:
                            return 13;

                        case TypeCode.Single:
                            return 4;

                        case TypeCode.Double:
                            return 5;

                        case TypeCode.Decimal:
                            return 6;

                        case TypeCode.String:
                            return 8;
                    }
                    return 100;

                case TypeCode.Byte:
                    switch (code2)
                    {
                        case TypeCode.Object:
                            return TypeDistance(typeRefs, formal, actual, 11);

                        case TypeCode.Boolean:
                            return 14;

                        case TypeCode.SByte:
                            return 13;

                        case TypeCode.Byte:
                            return 0;

                        case TypeCode.Int16:
                            return 3;

                        case TypeCode.UInt16:
                            return 1;

                        case TypeCode.Int32:
                            return 5;

                        case TypeCode.UInt32:
                            return 4;

                        case TypeCode.Int64:
                            return 7;

                        case TypeCode.UInt64:
                            return 6;

                        case TypeCode.Single:
                            return 8;

                        case TypeCode.Double:
                            return 9;

                        case TypeCode.Decimal:
                            return 10;

                        case TypeCode.String:
                            return 12;
                    }
                    return 100;

                case TypeCode.Int16:
                    switch (code2)
                    {
                        case TypeCode.Object:
                            return TypeDistance(typeRefs, formal, actual, 6);

                        case TypeCode.Boolean:
                            return 14;

                        case TypeCode.SByte:
                            return 12;

                        case TypeCode.Byte:
                            return 13;

                        case TypeCode.Int16:
                            return 0;

                        case TypeCode.UInt16:
                            return 8;

                        case TypeCode.Int32:
                            return 1;

                        case TypeCode.UInt32:
                            return 10;

                        case TypeCode.Int64:
                            return 2;

                        case TypeCode.UInt64:
                            return 11;

                        case TypeCode.Single:
                            return 3;

                        case TypeCode.Double:
                            return 4;

                        case TypeCode.Decimal:
                            return 5;

                        case TypeCode.String:
                            return 7;
                    }
                    return 100;

                case TypeCode.UInt16:
                    switch (code2)
                    {
                        case TypeCode.Object:
                            return TypeDistance(typeRefs, formal, actual, 9);

                        case TypeCode.Boolean:
                            return 14;

                        case TypeCode.SByte:
                            return 13;

                        case TypeCode.Byte:
                            return 12;

                        case TypeCode.Int16:
                            return 11;

                        case TypeCode.UInt16:
                            return 0;

                        case TypeCode.Int32:
                            return 4;

                        case TypeCode.UInt32:
                            return 1;

                        case TypeCode.Int64:
                            return 5;

                        case TypeCode.UInt64:
                            return 2;

                        case TypeCode.Single:
                            return 6;

                        case TypeCode.Double:
                            return 7;

                        case TypeCode.Decimal:
                            return 8;

                        case TypeCode.String:
                            return 10;
                    }
                    return 100;

                case TypeCode.Int32:
                    switch (code2)
                    {
                        case TypeCode.Object:
                            return TypeDistance(typeRefs, formal, actual, 4);

                        case TypeCode.Boolean:
                            return 14;

                        case TypeCode.SByte:
                            return 12;

                        case TypeCode.Byte:
                            return 13;

                        case TypeCode.Int16:
                            return 9;

                        case TypeCode.UInt16:
                            return 10;

                        case TypeCode.Int32:
                            return 0;

                        case TypeCode.UInt32:
                            return 7;

                        case TypeCode.Int64:
                            return 1;

                        case TypeCode.UInt64:
                            return 6;

                        case TypeCode.Single:
                            return 8;

                        case TypeCode.Double:
                            return 2;

                        case TypeCode.Decimal:
                            return 3;

                        case TypeCode.String:
                            return 5;
                    }
                    return 100;

                case TypeCode.UInt32:
                    switch (code2)
                    {
                        case TypeCode.Object:
                            return TypeDistance(typeRefs, formal, actual, 5);

                        case TypeCode.Boolean:
                            return 14;

                        case TypeCode.SByte:
                            return 13;

                        case TypeCode.Byte:
                            return 12;

                        case TypeCode.Int16:
                            return 11;

                        case TypeCode.UInt16:
                            return 9;

                        case TypeCode.Int32:
                            return 7;

                        case TypeCode.UInt32:
                            return 0;

                        case TypeCode.Int64:
                            return 2;

                        case TypeCode.UInt64:
                            return 1;

                        case TypeCode.Single:
                            return 8;

                        case TypeCode.Double:
                            return 3;

                        case TypeCode.Decimal:
                            return 4;

                        case TypeCode.String:
                            return 6;
                    }
                    return 100;

                case TypeCode.Int64:
                    switch (code2)
                    {
                        case TypeCode.Object:
                            return TypeDistance(typeRefs, formal, actual, 2);

                        case TypeCode.Boolean:
                            return 14;

                        case TypeCode.SByte:
                            return 8;

                        case TypeCode.Byte:
                            return 13;

                        case TypeCode.Int16:
                            return 7;

                        case TypeCode.UInt16:
                            return 11;

                        case TypeCode.Int32:
                            return 6;

                        case TypeCode.UInt32:
                            return 10;

                        case TypeCode.Int64:
                            return 0;

                        case TypeCode.UInt64:
                            return 9;

                        case TypeCode.Single:
                            return 5;

                        case TypeCode.Double:
                            return 4;

                        case TypeCode.Decimal:
                            return 1;

                        case TypeCode.String:
                            return 3;
                    }
                    return 100;

                case TypeCode.UInt64:
                    switch (code2)
                    {
                        case TypeCode.Object:
                            return TypeDistance(typeRefs, formal, actual, 2);

                        case TypeCode.Boolean:
                            return 14;

                        case TypeCode.SByte:
                            return 13;

                        case TypeCode.Byte:
                            return 10;

                        case TypeCode.Int16:
                            return 12;

                        case TypeCode.UInt16:
                            return 8;

                        case TypeCode.Int32:
                            return 11;

                        case TypeCode.UInt32:
                            return 7;

                        case TypeCode.Int64:
                            return 4;

                        case TypeCode.UInt64:
                            return 0;

                        case TypeCode.Single:
                            return 6;

                        case TypeCode.Double:
                            return 5;

                        case TypeCode.Decimal:
                            return 1;

                        case TypeCode.String:
                            return 3;
                    }
                    return 100;

                case TypeCode.Single:
                    switch (code2)
                    {
                        case TypeCode.Object:
                            return TypeDistance(typeRefs, formal, actual, 12);

                        case TypeCode.Boolean:
                            return 14;

                        case TypeCode.SByte:
                            return 10;

                        case TypeCode.Byte:
                            return 11;

                        case TypeCode.Int16:
                            return 7;

                        case TypeCode.UInt16:
                            return 8;

                        case TypeCode.Int32:
                            return 5;

                        case TypeCode.UInt32:
                            return 6;

                        case TypeCode.Int64:
                            return 3;

                        case TypeCode.UInt64:
                            return 4;

                        case TypeCode.Single:
                            return 0;

                        case TypeCode.Double:
                            return 1;

                        case TypeCode.Decimal:
                            return 2;

                        case TypeCode.String:
                            return 13;
                    }
                    return 100;

                case TypeCode.Double:
                    switch (code2)
                    {
                        case TypeCode.Object:
                            return TypeDistance(typeRefs, formal, actual, 12);

                        case TypeCode.Boolean:
                            return 14;

                        case TypeCode.SByte:
                            return 10;

                        case TypeCode.Byte:
                            return 11;

                        case TypeCode.Int16:
                            return 7;

                        case TypeCode.UInt16:
                            return 8;

                        case TypeCode.Int32:
                            return 5;

                        case TypeCode.UInt32:
                            return 6;

                        case TypeCode.Int64:
                            return 3;

                        case TypeCode.UInt64:
                            return 4;

                        case TypeCode.Single:
                            return 2;

                        case TypeCode.Double:
                            return 0;

                        case TypeCode.Decimal:
                            return 1;

                        case TypeCode.String:
                            return 13;
                    }
                    return 100;

                case TypeCode.Decimal:
                    switch (code2)
                    {
                        case TypeCode.Object:
                            if (!(formal == typeRefs.Object))
                            {
                                return 100;
                            }
                            return 12;

                        case TypeCode.Boolean:
                            return 14;

                        case TypeCode.SByte:
                            return 10;

                        case TypeCode.Byte:
                            return 11;

                        case TypeCode.Int16:
                            return 7;

                        case TypeCode.UInt16:
                            return 8;

                        case TypeCode.Int32:
                            return 5;

                        case TypeCode.UInt32:
                            return 6;

                        case TypeCode.Int64:
                            return 3;

                        case TypeCode.UInt64:
                            return 4;

                        case TypeCode.Single:
                            return 2;

                        case TypeCode.Double:
                            return 1;

                        case TypeCode.Decimal:
                            return 0;

                        case TypeCode.String:
                            return 13;
                    }
                    return 100;

                case TypeCode.DateTime:
                    switch (code2)
                    {
                        case TypeCode.Int32:
                            return 9;

                        case TypeCode.UInt32:
                            return 8;

                        case TypeCode.Int64:
                            return 7;

                        case TypeCode.UInt64:
                            return 6;

                        case TypeCode.Double:
                            return 4;

                        case TypeCode.Decimal:
                            return 5;

                        case TypeCode.DateTime:
                            return 0;

                        case TypeCode.String:
                            return 3;

                        case TypeCode.Object:
                            if (!(formal == typeRefs.Object))
                            {
                                return 100;
                            }
                            return 1;
                    }
                    return 100;

                case TypeCode.String:
                {
                    TypeCode code18 = code2;
                    switch (code18)
                    {
                        case TypeCode.Object:
                            if (!(formal == typeRefs.Object))
                            {
                                return 100;
                            }
                            return 1;

                        case TypeCode.Char:
                            return 2;
                    }
                    if (code18 != TypeCode.String)
                    {
                        return 100;
                    }
                    return 0;
                }
                default:
                    return 0;
            }
            return 100;
        }

        private static int TypeDistance(TypeReferences typeRefs, Type formal, Type actual, int distFromObject)
        {
            if (formal == typeRefs.Object)
            {
                return distFromObject;
            }
            if (formal.IsEnum)
            {
                return (TypeDistance(typeRefs, Enum.GetUnderlyingType(formal), actual) + 10);
            }
            return 100;
        }
    }
}

