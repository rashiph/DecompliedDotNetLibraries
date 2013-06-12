namespace System
{
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable]
    internal class DefaultBinder : Binder
    {
        [SecuritySafeCritical]
        public override FieldInfo BindToField(BindingFlags bindingAttr, FieldInfo[] match, object value, CultureInfo cultureInfo)
        {
            int num;
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            int num2 = 0;
            Type c = null;
            FieldInfo[] infoArray = (FieldInfo[]) match.Clone();
            if ((bindingAttr & BindingFlags.SetField) != BindingFlags.Default)
            {
                c = value.GetType();
                for (num = 0; num < infoArray.Length; num++)
                {
                    Type fieldType = infoArray[num].FieldType;
                    if (fieldType == c)
                    {
                        infoArray[num2++] = infoArray[num];
                    }
                    else if ((value == Empty.Value) && fieldType.IsClass)
                    {
                        infoArray[num2++] = infoArray[num];
                    }
                    else if (fieldType == typeof(object))
                    {
                        infoArray[num2++] = infoArray[num];
                    }
                    else if (fieldType.IsPrimitive)
                    {
                        if (CanConvertPrimitiveObjectToType(value, (RuntimeType) fieldType))
                        {
                            infoArray[num2++] = infoArray[num];
                        }
                    }
                    else if (fieldType.IsAssignableFrom(c))
                    {
                        infoArray[num2++] = infoArray[num];
                    }
                }
                switch (num2)
                {
                    case 0:
                        throw new MissingFieldException(Environment.GetResourceString("MissingField"));

                    case 1:
                        return infoArray[0];
                }
            }
            int index = 0;
            bool flag = false;
            for (num = 1; num < num2; num++)
            {
                switch (FindMostSpecificField(infoArray[index], infoArray[num]))
                {
                    case 0:
                        flag = true;
                        break;

                    case 2:
                        index = num;
                        flag = false;
                        break;
                }
            }
            if (flag)
            {
                throw new AmbiguousMatchException(Environment.GetResourceString("Arg_AmbiguousMatchException"));
            }
            return infoArray[index];
        }

        [SecuritySafeCritical]
        public override MethodBase BindToMethod(BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers, CultureInfo cultureInfo, string[] names, out object state)
        {
            int num;
            int length;
            if ((match == null) || (match.Length == 0))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EmptyArray"), "match");
            }
            MethodBase[] baseArray = (MethodBase[]) match.Clone();
            state = null;
            int[][] numArray = new int[baseArray.Length][];
            for (num = 0; num < baseArray.Length; num++)
            {
                ParameterInfo[] pars = baseArray[num].GetParametersNoCopy();
                numArray[num] = new int[(pars.Length > args.Length) ? pars.Length : args.Length];
                if (names == null)
                {
                    length = 0;
                    while (length < args.Length)
                    {
                        numArray[num][length] = length;
                        length++;
                    }
                }
                else if (!CreateParamOrder(numArray[num], pars, names))
                {
                    baseArray[num] = null;
                }
            }
            Type[] typeArray = new Type[baseArray.Length];
            Type[] types = new Type[args.Length];
            for (num = 0; num < args.Length; num++)
            {
                if (args[num] != null)
                {
                    types[num] = args[num].GetType();
                }
            }
            int index = 0;
            bool flag = (bindingAttr & BindingFlags.OptionalParamBinding) != BindingFlags.Default;
            Type elementType = null;
            for (num = 0; num < baseArray.Length; num++)
            {
                elementType = null;
                if (baseArray[num] != null)
                {
                    ParameterInfo[] infoArray2 = baseArray[num].GetParametersNoCopy();
                    if (infoArray2.Length == 0)
                    {
                        if ((args.Length == 0) || ((baseArray[num].CallingConvention & CallingConventions.VarArgs) != 0))
                        {
                            numArray[index] = numArray[num];
                            baseArray[index++] = baseArray[num];
                        }
                        continue;
                    }
                    if (infoArray2.Length > args.Length)
                    {
                        length = args.Length;
                        while (length < (infoArray2.Length - 1))
                        {
                            if (infoArray2[length].DefaultValue == DBNull.Value)
                            {
                                break;
                            }
                            length++;
                        }
                        if (length != (infoArray2.Length - 1))
                        {
                            continue;
                        }
                        if (infoArray2[length].DefaultValue == DBNull.Value)
                        {
                            if (!infoArray2[length].ParameterType.IsArray || !infoArray2[length].IsDefined(typeof(ParamArrayAttribute), true))
                            {
                                continue;
                            }
                            elementType = infoArray2[length].ParameterType.GetElementType();
                        }
                    }
                    else if (infoArray2.Length < args.Length)
                    {
                        int num4 = infoArray2.Length - 1;
                        if ((!infoArray2[num4].ParameterType.IsArray || !infoArray2[num4].IsDefined(typeof(ParamArrayAttribute), true)) || (numArray[num][num4] != num4))
                        {
                            continue;
                        }
                        elementType = infoArray2[num4].ParameterType.GetElementType();
                    }
                    else
                    {
                        int num5 = infoArray2.Length - 1;
                        if ((infoArray2[num5].ParameterType.IsArray && infoArray2[num5].IsDefined(typeof(ParamArrayAttribute), true)) && ((numArray[num][num5] == num5) && !infoArray2[num5].ParameterType.IsAssignableFrom(types[num5])))
                        {
                            elementType = infoArray2[num5].ParameterType.GetElementType();
                        }
                    }
                    Type parameterType = null;
                    int num6 = (elementType != null) ? (infoArray2.Length - 1) : args.Length;
                    length = 0;
                    while (length < num6)
                    {
                        parameterType = infoArray2[length].ParameterType;
                        if (parameterType.IsByRef)
                        {
                            parameterType = parameterType.GetElementType();
                        }
                        if (((parameterType != types[numArray[num][length]]) && (!flag || (args[numArray[num][length]] != Type.Missing))) && ((args[numArray[num][length]] != null) && (parameterType != typeof(object))))
                        {
                            if (parameterType.IsPrimitive)
                            {
                                if ((types[numArray[num][length]] == null) || !CanConvertPrimitiveObjectToType(args[numArray[num][length]], (RuntimeType) parameterType))
                                {
                                    break;
                                }
                            }
                            else if (((types[numArray[num][length]] != null) && !parameterType.IsAssignableFrom(types[numArray[num][length]])) && (!types[numArray[num][length]].IsCOMObject || !parameterType.IsInstanceOfType(args[numArray[num][length]])))
                            {
                                break;
                            }
                        }
                        length++;
                    }
                    if ((elementType != null) && (length == (infoArray2.Length - 1)))
                    {
                        while (length < args.Length)
                        {
                            if (elementType.IsPrimitive)
                            {
                                if ((types[length] == null) || !CanConvertPrimitiveObjectToType(args[length], (RuntimeType) elementType))
                                {
                                    break;
                                }
                            }
                            else if (((types[length] != null) && !elementType.IsAssignableFrom(types[length])) && (!types[length].IsCOMObject || !elementType.IsInstanceOfType(args[length])))
                            {
                                break;
                            }
                            length++;
                        }
                    }
                    if (length == args.Length)
                    {
                        numArray[index] = numArray[num];
                        typeArray[index] = elementType;
                        baseArray[index++] = baseArray[num];
                    }
                }
            }
            switch (index)
            {
                case 0:
                    throw new MissingMethodException(Environment.GetResourceString("MissingMember"));

                case 1:
                {
                    if (names != null)
                    {
                        state = new BinderState((int[]) numArray[0].Clone(), args.Length, typeArray[0] != null);
                        ReorderParams(numArray[0], args);
                    }
                    ParameterInfo[] infoArray3 = baseArray[0].GetParametersNoCopy();
                    if (infoArray3.Length == args.Length)
                    {
                        if (typeArray[0] != null)
                        {
                            object[] destinationArray = new object[infoArray3.Length];
                            int num7 = infoArray3.Length - 1;
                            Array.Copy(args, 0, destinationArray, 0, num7);
                            destinationArray[num7] = Array.UnsafeCreateInstance(typeArray[0], 1);
                            ((Array) destinationArray[num7]).SetValue(args[num7], 0);
                            args = destinationArray;
                        }
                    }
                    else if (infoArray3.Length > args.Length)
                    {
                        object[] objArray2 = new object[infoArray3.Length];
                        num = 0;
                        while (num < args.Length)
                        {
                            objArray2[num] = args[num];
                            num++;
                        }
                        while (num < (infoArray3.Length - 1))
                        {
                            objArray2[num] = infoArray3[num].DefaultValue;
                            num++;
                        }
                        if (typeArray[0] != null)
                        {
                            objArray2[num] = Array.UnsafeCreateInstance(typeArray[0], 0);
                        }
                        else
                        {
                            objArray2[num] = infoArray3[num].DefaultValue;
                        }
                        args = objArray2;
                    }
                    else if ((baseArray[0].CallingConvention & CallingConventions.VarArgs) == 0)
                    {
                        object[] objArray3 = new object[infoArray3.Length];
                        int num8 = infoArray3.Length - 1;
                        Array.Copy(args, 0, objArray3, 0, num8);
                        objArray3[num8] = Array.UnsafeCreateInstance(typeArray[0], (int) (args.Length - num8));
                        Array.Copy(args, num8, (Array) objArray3[num8], 0, args.Length - num8);
                        args = objArray3;
                    }
                    return baseArray[0];
                }
            }
            int num9 = 0;
            bool flag2 = false;
            for (num = 1; num < index; num++)
            {
                switch (FindMostSpecificMethod(baseArray[num9], numArray[num9], typeArray[num9], baseArray[num], numArray[num], typeArray[num], types, args))
                {
                    case 0:
                        flag2 = true;
                        break;

                    case 2:
                        num9 = num;
                        flag2 = false;
                        break;
                }
            }
            if (flag2)
            {
                throw new AmbiguousMatchException(Environment.GetResourceString("Arg_AmbiguousMatchException"));
            }
            if (names != null)
            {
                state = new BinderState((int[]) numArray[num9].Clone(), args.Length, typeArray[num9] != null);
                ReorderParams(numArray[num9], args);
            }
            ParameterInfo[] parametersNoCopy = baseArray[num9].GetParametersNoCopy();
            if (parametersNoCopy.Length == args.Length)
            {
                if (typeArray[num9] != null)
                {
                    object[] objArray4 = new object[parametersNoCopy.Length];
                    int num11 = parametersNoCopy.Length - 1;
                    Array.Copy(args, 0, objArray4, 0, num11);
                    objArray4[num11] = Array.UnsafeCreateInstance(typeArray[num9], 1);
                    ((Array) objArray4[num11]).SetValue(args[num11], 0);
                    args = objArray4;
                }
            }
            else if (parametersNoCopy.Length > args.Length)
            {
                object[] objArray5 = new object[parametersNoCopy.Length];
                num = 0;
                while (num < args.Length)
                {
                    objArray5[num] = args[num];
                    num++;
                }
                while (num < (parametersNoCopy.Length - 1))
                {
                    objArray5[num] = parametersNoCopy[num].DefaultValue;
                    num++;
                }
                if (typeArray[num9] != null)
                {
                    objArray5[num] = Array.UnsafeCreateInstance(typeArray[num9], 0);
                }
                else
                {
                    objArray5[num] = parametersNoCopy[num].DefaultValue;
                }
                args = objArray5;
            }
            else if ((baseArray[num9].CallingConvention & CallingConventions.VarArgs) == 0)
            {
                object[] objArray6 = new object[parametersNoCopy.Length];
                int num12 = parametersNoCopy.Length - 1;
                Array.Copy(args, 0, objArray6, 0, num12);
                objArray6[num12] = Array.UnsafeCreateInstance(typeArray[num9], (int) (args.Length - num12));
                Array.Copy(args, num12, (Array) objArray6[num12], 0, args.Length - num12);
                args = objArray6;
            }
            return baseArray[num9];
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern bool CanConvertPrimitive(RuntimeType source, RuntimeType target);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool CanConvertPrimitiveObjectToType(object source, RuntimeType type);
        public override object ChangeType(object value, Type type, CultureInfo cultureInfo)
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_ChangeType"));
        }

        internal static bool CompareMethodSigAndName(MethodBase m1, MethodBase m2)
        {
            ParameterInfo[] parametersNoCopy = m1.GetParametersNoCopy();
            ParameterInfo[] infoArray2 = m2.GetParametersNoCopy();
            if (parametersNoCopy.Length != infoArray2.Length)
            {
                return false;
            }
            int length = parametersNoCopy.Length;
            for (int i = 0; i < length; i++)
            {
                if (parametersNoCopy[i].ParameterType != infoArray2[i].ParameterType)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool CreateParamOrder(int[] paramOrder, ParameterInfo[] pars, string[] names)
        {
            bool[] flagArray = new bool[pars.Length];
            for (int i = 0; i < pars.Length; i++)
            {
                paramOrder[i] = -1;
            }
            for (int j = 0; j < names.Length; j++)
            {
                int num3 = 0;
                while (num3 < pars.Length)
                {
                    if (names[j].Equals(pars[num3].Name))
                    {
                        paramOrder[num3] = j;
                        flagArray[j] = true;
                        break;
                    }
                    num3++;
                }
                if (num3 == pars.Length)
                {
                    return false;
                }
            }
            int index = 0;
            for (int k = 0; k < pars.Length; k++)
            {
                if (paramOrder[k] == -1)
                {
                    while (index < pars.Length)
                    {
                        if (!flagArray[index])
                        {
                            paramOrder[k] = index;
                            index++;
                            break;
                        }
                        index++;
                    }
                }
            }
            return true;
        }

        public static MethodBase ExactBinding(MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
        {
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            MethodBase[] baseArray = new MethodBase[match.Length];
            int index = 0;
            for (int i = 0; i < match.Length; i++)
            {
                ParameterInfo[] parametersNoCopy = match[i].GetParametersNoCopy();
                if (parametersNoCopy.Length != 0)
                {
                    int num3 = 0;
                    while (num3 < types.Length)
                    {
                        if (!parametersNoCopy[num3].ParameterType.Equals(types[num3]))
                        {
                            break;
                        }
                        num3++;
                    }
                    if (num3 >= types.Length)
                    {
                        baseArray[index] = match[i];
                        index++;
                    }
                }
            }
            switch (index)
            {
                case 0:
                    return null;

                case 1:
                    return baseArray[0];
            }
            return FindMostDerivedNewSlotMeth(baseArray, index);
        }

        public static PropertyInfo ExactPropertyBinding(PropertyInfo[] match, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            PropertyInfo info = null;
            int num = (types != null) ? types.Length : 0;
            for (int i = 0; i < match.Length; i++)
            {
                ParameterInfo[] indexParameters = match[i].GetIndexParameters();
                int index = 0;
                while (index < num)
                {
                    if (indexParameters[index].ParameterType != types[index])
                    {
                        break;
                    }
                    index++;
                }
                if ((index >= num) && ((returnType == null) || (returnType == match[i].PropertyType)))
                {
                    if (info != null)
                    {
                        throw new AmbiguousMatchException(Environment.GetResourceString("Arg_AmbiguousMatchException"));
                    }
                    info = match[i];
                }
            }
            return info;
        }

        internal static MethodBase FindMostDerivedNewSlotMeth(MethodBase[] match, int cMatches)
        {
            int num = 0;
            MethodBase base2 = null;
            for (int i = 0; i < cMatches; i++)
            {
                int hierarchyDepth = GetHierarchyDepth(match[i].DeclaringType);
                if (hierarchyDepth == num)
                {
                    throw new AmbiguousMatchException(Environment.GetResourceString("Arg_AmbiguousMatchException"));
                }
                if (hierarchyDepth > num)
                {
                    num = hierarchyDepth;
                    base2 = match[i];
                }
            }
            return base2;
        }

        private static int FindMostSpecific(ParameterInfo[] p1, int[] paramOrder1, Type paramArrayType1, ParameterInfo[] p2, int[] paramOrder2, Type paramArrayType2, Type[] types, object[] args)
        {
            if ((paramArrayType1 != null) && (paramArrayType2 == null))
            {
                return 2;
            }
            if ((paramArrayType2 == null) || (paramArrayType1 != null))
            {
                bool flag = false;
                bool flag2 = false;
                for (int i = 0; i < types.Length; i++)
                {
                    if ((args == null) || (args[i] != Type.Missing))
                    {
                        Type parameterType;
                        Type type2;
                        if ((paramArrayType1 != null) && (paramOrder1[i] >= (p1.Length - 1)))
                        {
                            parameterType = paramArrayType1;
                        }
                        else
                        {
                            parameterType = p1[paramOrder1[i]].ParameterType;
                        }
                        if ((paramArrayType2 != null) && (paramOrder2[i] >= (p2.Length - 1)))
                        {
                            type2 = paramArrayType2;
                        }
                        else
                        {
                            type2 = p2[paramOrder2[i]].ParameterType;
                        }
                        if (!(parameterType == type2))
                        {
                            switch (FindMostSpecificType(parameterType, type2, types[i]))
                            {
                                case 0:
                                    return 0;

                                case 1:
                                    flag = true;
                                    break;

                                case 2:
                                    flag2 = true;
                                    break;
                            }
                        }
                    }
                }
                if (flag == flag2)
                {
                    if (!flag && (args != null))
                    {
                        if (p1.Length > p2.Length)
                        {
                            return 1;
                        }
                        if (p2.Length > p1.Length)
                        {
                            return 2;
                        }
                    }
                    return 0;
                }
                if (!flag)
                {
                    return 2;
                }
            }
            return 1;
        }

        private static int FindMostSpecificField(FieldInfo cur1, FieldInfo cur2)
        {
            if (!(cur1.Name == cur2.Name))
            {
                return 0;
            }
            int hierarchyDepth = GetHierarchyDepth(cur1.DeclaringType);
            int num2 = GetHierarchyDepth(cur2.DeclaringType);
            if (hierarchyDepth == num2)
            {
                return 0;
            }
            if (hierarchyDepth < num2)
            {
                return 2;
            }
            return 1;
        }

        private static int FindMostSpecificMethod(MethodBase m1, int[] paramOrder1, Type paramArrayType1, MethodBase m2, int[] paramOrder2, Type paramArrayType2, Type[] types, object[] args)
        {
            int num = FindMostSpecific(m1.GetParametersNoCopy(), paramOrder1, paramArrayType1, m2.GetParametersNoCopy(), paramOrder2, paramArrayType2, types, args);
            if (num != 0)
            {
                return num;
            }
            if (!CompareMethodSigAndName(m1, m2))
            {
                return 0;
            }
            int hierarchyDepth = GetHierarchyDepth(m1.DeclaringType);
            int num3 = GetHierarchyDepth(m2.DeclaringType);
            if (hierarchyDepth == num3)
            {
                return 0;
            }
            if (hierarchyDepth < num3)
            {
                return 2;
            }
            return 1;
        }

        private static int FindMostSpecificProperty(PropertyInfo cur1, PropertyInfo cur2)
        {
            if (!(cur1.Name == cur2.Name))
            {
                return 0;
            }
            int hierarchyDepth = GetHierarchyDepth(cur1.DeclaringType);
            int num2 = GetHierarchyDepth(cur2.DeclaringType);
            if (hierarchyDepth == num2)
            {
                return 0;
            }
            if (hierarchyDepth < num2)
            {
                return 2;
            }
            return 1;
        }

        [SecuritySafeCritical]
        private static int FindMostSpecificType(Type c1, Type c2, Type t)
        {
            if (c1 == c2)
            {
                return 0;
            }
            if (c1 != t)
            {
                bool flag;
                bool flag2;
                if (c2 == t)
                {
                    return 2;
                }
                if (c1.IsByRef || c2.IsByRef)
                {
                    if (c1.IsByRef && c2.IsByRef)
                    {
                        c1 = c1.GetElementType();
                        c2 = c2.GetElementType();
                    }
                    else if (c1.IsByRef)
                    {
                        if (c1.GetElementType() == c2)
                        {
                            return 2;
                        }
                        c1 = c1.GetElementType();
                    }
                    else
                    {
                        if (c2.GetElementType() == c1)
                        {
                            return 1;
                        }
                        c2 = c2.GetElementType();
                    }
                }
                if (c1.IsPrimitive && c2.IsPrimitive)
                {
                    flag = CanConvertPrimitive((RuntimeType) c2, (RuntimeType) c1);
                    flag2 = CanConvertPrimitive((RuntimeType) c1, (RuntimeType) c2);
                }
                else
                {
                    flag = c1.IsAssignableFrom(c2);
                    flag2 = c2.IsAssignableFrom(c1);
                }
                if (flag == flag2)
                {
                    return 0;
                }
                if (flag)
                {
                    return 2;
                }
            }
            return 1;
        }

        internal static int GetHierarchyDepth(Type t)
        {
            int num = 0;
            Type baseType = t;
            do
            {
                num++;
                baseType = baseType.BaseType;
            }
            while (baseType != null);
            return num;
        }

        public override void ReorderArgumentArray(ref object[] args, object state)
        {
            BinderState state2 = (BinderState) state;
            ReorderParams(state2.m_argsMap, args);
            if (state2.m_isParamArray)
            {
                int index = args.Length - 1;
                if (args.Length == state2.m_originalSize)
                {
                    args[index] = ((object[]) args[index])[0];
                }
                else
                {
                    object[] destinationArray = new object[args.Length];
                    Array.Copy(args, 0, destinationArray, 0, index);
                    int num2 = index;
                    for (int i = 0; num2 < destinationArray.Length; i++)
                    {
                        destinationArray[num2] = ((object[]) args[index])[i];
                        num2++;
                    }
                    args = destinationArray;
                }
            }
            else if (args.Length > state2.m_originalSize)
            {
                object[] objArray2 = new object[state2.m_originalSize];
                Array.Copy(args, 0, objArray2, 0, state2.m_originalSize);
                args = objArray2;
            }
        }

        private static void ReorderParams(int[] paramOrder, object[] vars)
        {
            object[] objArray = new object[vars.Length];
            for (int i = 0; i < vars.Length; i++)
            {
                objArray[i] = vars[i];
            }
            for (int j = 0; j < vars.Length; j++)
            {
                vars[j] = objArray[paramOrder[j]];
            }
        }

        [SecuritySafeCritical]
        public override MethodBase SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
        {
            int num;
            Type[] typeArray = new Type[types.Length];
            for (num = 0; num < types.Length; num++)
            {
                typeArray[num] = types[num].UnderlyingSystemType;
                if (!(typeArray[num] is RuntimeType))
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_MustBeType"), "types");
                }
            }
            types = typeArray;
            if ((match == null) || (match.Length == 0))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EmptyArray"), "match");
            }
            MethodBase[] baseArray = (MethodBase[]) match.Clone();
            int num3 = 0;
            for (num = 0; num < baseArray.Length; num++)
            {
                ParameterInfo[] parametersNoCopy = baseArray[num].GetParametersNoCopy();
                if (parametersNoCopy.Length == types.Length)
                {
                    int num2 = 0;
                    while (num2 < types.Length)
                    {
                        Type parameterType = parametersNoCopy[num2].ParameterType;
                        if ((parameterType != types[num2]) && (parameterType != typeof(object)))
                        {
                            if (parameterType.IsPrimitive)
                            {
                                if (!(types[num2].UnderlyingSystemType is RuntimeType) || !CanConvertPrimitive((RuntimeType) types[num2].UnderlyingSystemType, (RuntimeType) parameterType.UnderlyingSystemType))
                                {
                                    break;
                                }
                            }
                            else if (!parameterType.IsAssignableFrom(types[num2]))
                            {
                                break;
                            }
                        }
                        num2++;
                    }
                    if (num2 == types.Length)
                    {
                        baseArray[num3++] = baseArray[num];
                    }
                }
            }
            switch (num3)
            {
                case 0:
                    return null;

                case 1:
                    return baseArray[0];
            }
            int index = 0;
            bool flag = false;
            int[] numArray = new int[types.Length];
            for (num = 0; num < types.Length; num++)
            {
                numArray[num] = num;
            }
            for (num = 1; num < num3; num++)
            {
                switch (FindMostSpecificMethod(baseArray[index], numArray, null, baseArray[num], numArray, null, types, null))
                {
                    case 0:
                        flag = true;
                        break;

                    case 2:
                        index = num;
                        flag = false;
                        index = num;
                        break;
                }
            }
            if (flag)
            {
                throw new AmbiguousMatchException(Environment.GetResourceString("Arg_AmbiguousMatchException"));
            }
            return baseArray[index];
        }

        [SecuritySafeCritical]
        public override PropertyInfo SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers)
        {
            int num;
            if ((indexes != null) && !Contract.ForAll<Type>(indexes, t => t != null))
            {
                throw new ArgumentNullException("indexes");
            }
            if ((match == null) || (match.Length == 0))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EmptyArray"), "match");
            }
            PropertyInfo[] infoArray = (PropertyInfo[]) match.Clone();
            int index = 0;
            int num3 = 0;
            int num4 = (indexes != null) ? indexes.Length : 0;
            for (num = 0; num < infoArray.Length; num++)
            {
                if (indexes != null)
                {
                    ParameterInfo[] indexParameters = infoArray[num].GetIndexParameters();
                    if (indexParameters.Length != num4)
                    {
                        continue;
                    }
                    index = 0;
                    while (index < num4)
                    {
                        Type parameterType = indexParameters[index].ParameterType;
                        if ((parameterType != indexes[index]) && (parameterType != typeof(object)))
                        {
                            if (parameterType.IsPrimitive)
                            {
                                if (!(indexes[index].UnderlyingSystemType is RuntimeType) || !CanConvertPrimitive((RuntimeType) indexes[index].UnderlyingSystemType, (RuntimeType) parameterType.UnderlyingSystemType))
                                {
                                    break;
                                }
                            }
                            else if (!parameterType.IsAssignableFrom(indexes[index]))
                            {
                                break;
                            }
                        }
                        index++;
                    }
                }
                if (index != num4)
                {
                    continue;
                }
                if (returnType != null)
                {
                    if (infoArray[num].PropertyType.IsPrimitive)
                    {
                        if ((returnType.UnderlyingSystemType is RuntimeType) && CanConvertPrimitive((RuntimeType) returnType.UnderlyingSystemType, (RuntimeType) infoArray[num].PropertyType.UnderlyingSystemType))
                        {
                            goto Label_0173;
                        }
                        continue;
                    }
                    if (!infoArray[num].PropertyType.IsAssignableFrom(returnType))
                    {
                        continue;
                    }
                }
            Label_0173:
                infoArray[num3++] = infoArray[num];
            }
            switch (num3)
            {
                case 0:
                    return null;

                case 1:
                    return infoArray[0];
            }
            int num5 = 0;
            bool flag = false;
            int[] numArray = new int[num4];
            for (num = 0; num < num4; num++)
            {
                numArray[num] = num;
            }
            for (num = 1; num < num3; num++)
            {
                int num6 = FindMostSpecificType(infoArray[num5].PropertyType, infoArray[num].PropertyType, returnType);
                if ((num6 == 0) && (indexes != null))
                {
                    num6 = FindMostSpecific(infoArray[num5].GetIndexParameters(), numArray, null, infoArray[num].GetIndexParameters(), numArray, null, indexes, null);
                }
                if (num6 == 0)
                {
                    num6 = FindMostSpecificProperty(infoArray[num5], infoArray[num]);
                    if (num6 == 0)
                    {
                        flag = true;
                    }
                }
                if (num6 == 2)
                {
                    flag = false;
                    num5 = num;
                }
            }
            if (flag)
            {
                throw new AmbiguousMatchException(Environment.GetResourceString("Arg_AmbiguousMatchException"));
            }
            return infoArray[num5];
        }

        internal class BinderState
        {
            internal int[] m_argsMap;
            internal bool m_isParamArray;
            internal int m_originalSize;

            internal BinderState(int[] argsMap, int originalSize, bool isParamArray)
            {
                this.m_argsMap = argsMap;
                this.m_originalSize = originalSize;
                this.m_isParamArray = isParamArray;
            }
        }
    }
}

