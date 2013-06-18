namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.Remoting;
    using System.Security;
    using System.Security.Permissions;

    internal sealed class VBBinder : Binder
    {
        private const int ARG_MISSING = -1;
        internal string m_BindToName;
        private bool[] m_ByRefFlags;
        private MemberInfo m_CachedMember;
        internal Type m_objType;
        private VBBinderState m_state;
        private const int PARAMARRAY_NONE = -1;

        public VBBinder(bool[] CopyBack)
        {
            this.m_ByRefFlags = CopyBack;
        }

        private BindScore BindingScore(ParameterInfo[] Parameters, int[] paramOrder, Type[] ArgTypes, bool IsPropertySet, int ParamArrayIndex)
        {
            BindScore exact = BindScore.Exact;
            int upperBound = ArgTypes.GetUpperBound(0);
            int num3 = Parameters.GetUpperBound(0);
            if (IsPropertySet)
            {
                num3--;
                upperBound--;
            }
            int num5 = Math.Max(upperBound, num3);
            for (int i = 0; i <= num5; i++)
            {
                int num;
                Type type;
                if (paramOrder == null)
                {
                    num = i;
                }
                else
                {
                    num = paramOrder[i];
                }
                if (num == -1)
                {
                    type = null;
                }
                else
                {
                    type = ArgTypes[num];
                }
                if (type != null)
                {
                    Type parameterType;
                    if (i > num3)
                    {
                        parameterType = Parameters[ParamArrayIndex].ParameterType;
                    }
                    else
                    {
                        parameterType = Parameters[i].ParameterType;
                    }
                    if (((i != ParamArrayIndex) || !type.IsArray) || (parameterType != type))
                    {
                        if (((i == ParamArrayIndex) && type.IsArray) && (((this.m_state.m_OriginalArgs == null) || (this.m_state.m_OriginalArgs[num] == null)) || parameterType.IsInstanceOfType(this.m_state.m_OriginalArgs[num])))
                        {
                            if (exact < BindScore.Widening1)
                            {
                                exact = BindScore.Widening1;
                            }
                        }
                        else
                        {
                            if (((ParamArrayIndex != -1) && (i >= ParamArrayIndex)) || parameterType.IsByRef)
                            {
                                parameterType = parameterType.GetElementType();
                            }
                            if (type != parameterType)
                            {
                                if (ObjectType.IsWideningConversion(type, parameterType))
                                {
                                    if (exact < BindScore.Widening1)
                                    {
                                        exact = BindScore.Widening1;
                                    }
                                }
                                else if (type.IsArray && (((this.m_state.m_OriginalArgs == null) || (this.m_state.m_OriginalArgs[num] == null)) || parameterType.IsInstanceOfType(this.m_state.m_OriginalArgs[num])))
                                {
                                    if (exact < BindScore.Widening1)
                                    {
                                        exact = BindScore.Widening1;
                                    }
                                }
                                else
                                {
                                    exact = BindScore.Narrowing;
                                }
                            }
                        }
                    }
                }
            }
            return exact;
        }

        public override FieldInfo BindToField(BindingFlags bindingAttr, FieldInfo[] match, object value, CultureInfo culture)
        {
            if (((this.m_CachedMember != null) && (this.m_CachedMember.MemberType == MemberTypes.Field)) && ((match[0] != null) && (match[0].Name == this.m_CachedMember.Name)))
            {
                return (FieldInfo) this.m_CachedMember;
            }
            FieldInfo info = match[0];
            int upperBound = match.GetUpperBound(0);
            for (int i = 1; i <= upperBound; i++)
            {
                if (match[i].DeclaringType.IsSubclassOf(info.DeclaringType))
                {
                    info = match[i];
                }
            }
            return info;
        }

        public override MethodBase BindToMethod(BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] names, ref object ObjState)
        {
            int num;
            int[] numArray;
            Type type = null;
            bool flag;
            int num3;
            ParameterInfo info;
            int num4;
            int num5;
            Type elementType = null;
            int num8;
            ParameterInfo[] infoArray;
            int num9;
            Type parameterType = null;
            int num11;
            MethodBase base4;
            if ((match == null) || (match.Length == 0))
            {
                throw ExceptionUtils.VbMakeException(0x1b6);
            }
            if (((this.m_CachedMember != null) && (this.m_CachedMember.MemberType == MemberTypes.Method)) && ((match[0] != null) && (match[0].Name == this.m_CachedMember.Name)))
            {
                return (MethodBase) this.m_CachedMember;
            }
            bool isPropertySet = (bindingAttr & BindingFlags.SetProperty) != BindingFlags.Default;
            if ((names != null) && (names.Length == 0))
            {
                names = null;
            }
            int length = match.Length;
            if (length > 1)
            {
                int num25 = match.GetUpperBound(0);
                for (num5 = 0; num5 <= num25; num5++)
                {
                    base4 = match[num5];
                    if ((base4 != null) && !base4.IsHideBySig)
                    {
                        if (base4.IsVirtual)
                        {
                            if ((base4.Attributes & MethodAttributes.NewSlot) != MethodAttributes.PrivateScope)
                            {
                                int num26 = match.GetUpperBound(0);
                                for (int i = 0; i <= num26; i++)
                                {
                                    if (((num5 != i) && (match[i] != null)) && base4.DeclaringType.IsSubclassOf(match[i].DeclaringType))
                                    {
                                        match[i] = null;
                                        length--;
                                    }
                                }
                            }
                        }
                        else
                        {
                            int num27 = match.GetUpperBound(0);
                            for (int j = 0; j <= num27; j++)
                            {
                                if (((num5 != j) && (match[j] != null)) && base4.DeclaringType.IsSubclassOf(match[j].DeclaringType))
                                {
                                    match[j] = null;
                                    length--;
                                }
                            }
                        }
                    }
                }
            }
            int num2 = length;
            if (names != null)
            {
                int num28 = match.GetUpperBound(0);
                for (num5 = 0; num5 <= num28; num5++)
                {
                    base4 = match[num5];
                    if (base4 != null)
                    {
                        infoArray = base4.GetParameters();
                        num4 = infoArray.GetUpperBound(0);
                        if (isPropertySet)
                        {
                            num4--;
                        }
                        if (num4 >= 0)
                        {
                            info = infoArray[num4];
                            num8 = -1;
                            if (info.ParameterType.IsArray)
                            {
                                object[] customAttributes = info.GetCustomAttributes(typeof(ParamArrayAttribute), false);
                                if ((customAttributes != null) && (customAttributes.Length > 0))
                                {
                                    num8 = num4;
                                }
                                else
                                {
                                    num8 = -1;
                                }
                            }
                        }
                        int num29 = names.GetUpperBound(0);
                        for (int k = 0; k <= num29; k++)
                        {
                            int num30 = num4;
                            num9 = 0;
                            while (num9 <= num30)
                            {
                                if (Strings.StrComp(names[k], infoArray[num9].Name, CompareMethod.Text) == 0)
                                {
                                    if ((num9 == num8) && (length == 1))
                                    {
                                        throw ExceptionUtils.VbMakeExceptionEx(0x1be, Utils.GetResourceString("NamedArgumentOnParamArray"));
                                    }
                                    if (num9 == num8)
                                    {
                                        num9 = num4 + 1;
                                    }
                                    break;
                                }
                                num9++;
                            }
                            if (num9 > num4)
                            {
                                if (length == 1)
                                {
                                    throw new MissingMemberException(Utils.GetResourceString("Argument_InvalidNamedArg2", new string[] { names[k], this.CalledMethodName() }));
                                }
                                match[num5] = null;
                                length--;
                                break;
                            }
                        }
                    }
                }
            }
            int[] numArray2 = new int[(match.Length - 1) + 1];
            int upperBound = match.GetUpperBound(0);
            for (num5 = 0; num5 <= upperBound; num5++)
            {
                base4 = match[num5];
                if (base4 != null)
                {
                    num8 = -1;
                    infoArray = base4.GetParameters();
                    num4 = infoArray.GetUpperBound(0);
                    if (isPropertySet)
                    {
                        num4--;
                    }
                    if (num4 >= 0)
                    {
                        info = infoArray[num4];
                        if (info.ParameterType.IsArray)
                        {
                            object[] objArray3 = info.GetCustomAttributes(typeof(ParamArrayAttribute), false);
                            if ((objArray3 != null) && (objArray3.Length > 0))
                            {
                                num8 = num4;
                            }
                        }
                    }
                    numArray2[num5] = num8;
                    if ((num8 == -1) && (args.Length > infoArray.Length))
                    {
                        if (length == 1)
                        {
                            throw new MissingMemberException(Utils.GetResourceString("NoMethodTakingXArguments2", new string[] { this.CalledMethodName(), Conversions.ToString(this.GetPropArgCount(args, isPropertySet)) }));
                        }
                        match[num5] = null;
                        length--;
                    }
                    int num14 = num4;
                    if (num8 != -1)
                    {
                        num14--;
                    }
                    if (args.Length < num14)
                    {
                        int num32 = num14 - 1;
                        int index = args.Length;
                        while (index <= num32)
                        {
                            if (infoArray[index].DefaultValue == DBNull.Value)
                            {
                                break;
                            }
                            index++;
                        }
                        if (index != num14)
                        {
                            if (length == 1)
                            {
                                throw new MissingMemberException(Utils.GetResourceString("NoMethodTakingXArguments2", new string[] { this.CalledMethodName(), Conversions.ToString(this.GetPropArgCount(args, isPropertySet)) }));
                            }
                            match[num5] = null;
                            length--;
                        }
                    }
                }
            }
            object[] paramOrder = new object[(match.Length - 1) + 1];
            int num33 = match.GetUpperBound(0);
            for (num5 = 0; num5 <= num33; num5++)
            {
                base4 = match[num5];
                if (base4 != null)
                {
                    infoArray = base4.GetParameters();
                    if (args.Length > infoArray.Length)
                    {
                        numArray = new int[(args.Length - 1) + 1];
                    }
                    else
                    {
                        numArray = new int[(infoArray.Length - 1) + 1];
                    }
                    paramOrder[num5] = numArray;
                    if (names == null)
                    {
                        int num16 = args.GetUpperBound(0);
                        if (isPropertySet)
                        {
                            num16--;
                        }
                        int num34 = num16;
                        num = 0;
                        while (num <= num34)
                        {
                            if ((args[num] is Missing) && ((num > infoArray.GetUpperBound(0)) || infoArray[num].IsOptional))
                            {
                                numArray[num] = -1;
                            }
                            else
                            {
                                numArray[num] = num;
                            }
                            num++;
                        }
                        num16 = numArray.GetUpperBound(0);
                        int num35 = num16;
                        for (num = num; num <= num35; num++)
                        {
                            numArray[num] = -1;
                        }
                        if (isPropertySet)
                        {
                            numArray[num16] = args.GetUpperBound(0);
                        }
                    }
                    else
                    {
                        Exception exception = this.CreateParamOrder(isPropertySet, numArray, base4.GetParameters(), args, names);
                        if (exception != null)
                        {
                            if (length == 1)
                            {
                                throw exception;
                            }
                            match[num5] = null;
                            length--;
                        }
                    }
                }
            }
            Type[] argTypes = new Type[(args.Length - 1) + 1];
            int num36 = args.GetUpperBound(0);
            num = 0;
            while (num <= num36)
            {
                if (args[num] != null)
                {
                    argTypes[num] = args[num].GetType();
                }
                num++;
            }
            int num37 = match.GetUpperBound(0);
            for (num5 = 0; num5 <= num37; num5++)
            {
                base4 = match[num5];
                if (base4 != null)
                {
                    infoArray = base4.GetParameters();
                    numArray = (int[]) paramOrder[num5];
                    num4 = numArray.GetUpperBound(0);
                    if (isPropertySet)
                    {
                        num4--;
                    }
                    num8 = numArray2[num5];
                    if (num8 != -1)
                    {
                        elementType = infoArray[num8].ParameterType.GetElementType();
                    }
                    else if (numArray.Length > infoArray.Length)
                    {
                        goto Label_08D3;
                    }
                    int num38 = num4;
                    num9 = 0;
                    while (num9 <= num38)
                    {
                        TypeCode empty;
                        num = numArray[num9];
                        if (num == -1)
                        {
                            if (!infoArray[num9].IsOptional && (num9 != numArray2[num5]))
                            {
                                if (length == 1)
                                {
                                    throw new MissingMemberException(Utils.GetResourceString("NoMethodTakingXArguments2", new string[] { this.CalledMethodName(), Conversions.ToString(this.GetPropArgCount(args, isPropertySet)) }));
                                }
                                goto Label_08D3;
                            }
                        }
                        else
                        {
                            type = argTypes[num];
                            if (type != null)
                            {
                                if ((num8 != -1) && (num9 > num8))
                                {
                                    parameterType = infoArray[num8].ParameterType.GetElementType();
                                }
                                else
                                {
                                    parameterType = infoArray[num9].ParameterType;
                                    if (parameterType.IsByRef)
                                    {
                                        parameterType = parameterType.GetElementType();
                                    }
                                    if (num9 == num8)
                                    {
                                        if (parameterType.IsInstanceOfType(args[num]) && (num9 == num4))
                                        {
                                            goto Label_08C2;
                                        }
                                        parameterType = elementType;
                                    }
                                }
                                if ((parameterType != type) && ((type != Type.Missing) || !infoArray[num9].IsOptional))
                                {
                                    if (args[num] == Missing.Value)
                                    {
                                        goto Label_08D3;
                                    }
                                    if ((parameterType != typeof(object)) && !parameterType.IsInstanceOfType(args[num]))
                                    {
                                        TypeCode typeCode = Type.GetTypeCode(parameterType);
                                        if (type == null)
                                        {
                                            empty = TypeCode.Empty;
                                        }
                                        else
                                        {
                                            empty = Type.GetTypeCode(type);
                                        }
                                        switch (typeCode)
                                        {
                                            case TypeCode.Boolean:
                                            case TypeCode.Byte:
                                            case TypeCode.Int16:
                                            case TypeCode.Int32:
                                            case TypeCode.Int64:
                                            case TypeCode.Single:
                                            case TypeCode.Double:
                                            case TypeCode.Decimal:
                                                switch (empty)
                                                {
                                                    case TypeCode.Boolean:
                                                    case TypeCode.Byte:
                                                    case TypeCode.Int16:
                                                    case TypeCode.Int32:
                                                    case TypeCode.Int64:
                                                    case TypeCode.Single:
                                                    case TypeCode.Double:
                                                    case TypeCode.Decimal:
                                                    case TypeCode.String:
                                                        goto Label_08C2;
                                                }
                                                goto Label_08D3;

                                            case TypeCode.Char:
                                                switch (empty)
                                                {
                                                    case TypeCode.String:
                                                        goto Label_08D3;
                                                }
                                                goto Label_08C2;

                                            case TypeCode.SByte:
                                            case TypeCode.UInt16:
                                            case TypeCode.UInt32:
                                            case TypeCode.UInt64:
                                            case (TypeCode.DateTime | TypeCode.Object):
                                                goto Label_0896;

                                            case TypeCode.DateTime:
                                                switch (empty)
                                                {
                                                    case TypeCode.String:
                                                        goto Label_08D3;
                                                }
                                                goto Label_08C2;

                                            case TypeCode.String:
                                                switch (empty)
                                                {
                                                    case TypeCode.Empty:
                                                    case TypeCode.Boolean:
                                                    case TypeCode.Char:
                                                    case TypeCode.Byte:
                                                    case TypeCode.Int16:
                                                    case TypeCode.Int32:
                                                    case TypeCode.Int64:
                                                    case TypeCode.Single:
                                                    case TypeCode.Double:
                                                    case TypeCode.Decimal:
                                                    case TypeCode.String:
                                                        goto Label_08C2;
                                                }
                                                break;

                                            default:
                                                goto Label_0896;
                                        }
                                        if (type != typeof(char[]))
                                        {
                                            goto Label_08D3;
                                        }
                                    }
                                }
                            }
                        }
                        goto Label_08C2;
                    Label_0896:
                        if (parameterType != typeof(char[]))
                        {
                            goto Label_08D3;
                        }
                        TypeCode code10 = empty;
                        if ((code10 != TypeCode.String) && ((code10 != TypeCode.Object) || (type != typeof(char[]))))
                        {
                            goto Label_08D3;
                        }
                    Label_08C2:
                        num9++;
                    }
                }
                continue;
            Label_08D3:
                if (length == 1)
                {
                    if (num2 != 1)
                    {
                        throw new AmbiguousMatchException(Utils.GetResourceString("AmbiguousMatch_NarrowingConversion1", new string[] { this.CalledMethodName() }));
                    }
                    this.ThrowInvalidCast(type, parameterType, num9);
                }
                match[num5] = null;
                length--;
            }
            length = 0;
            int num39 = match.GetUpperBound(0);
            for (num5 = 0; num5 <= num39; num5++)
            {
                base4 = match[num5];
                if (base4 != null)
                {
                    numArray = (int[]) paramOrder[num5];
                    infoArray = base4.GetParameters();
                    bool flag3 = false;
                    num4 = infoArray.GetUpperBound(0);
                    if (isPropertySet)
                    {
                        num4--;
                    }
                    num3 = args.GetUpperBound(0);
                    if (isPropertySet)
                    {
                        num3--;
                    }
                    num8 = numArray2[num5];
                    if (num8 != -1)
                    {
                        elementType = infoArray[num4].ParameterType.GetElementType();
                    }
                    int num40 = num4;
                    num9 = 0;
                    while (num9 <= num40)
                    {
                        if (num9 == num8)
                        {
                            parameterType = elementType;
                        }
                        else
                        {
                            parameterType = infoArray[num9].ParameterType;
                        }
                        if (parameterType.IsByRef)
                        {
                            flag3 = true;
                            parameterType = parameterType.GetElementType();
                        }
                        num = numArray[num9];
                        if (((num != -1) || !infoArray[num9].IsOptional) && (num9 != numArray2[num5]))
                        {
                            type = argTypes[num];
                            if (((type != null) && ((type != Type.Missing) || !infoArray[num9].IsOptional)) && ((parameterType != type) && (parameterType != typeof(object))))
                            {
                                TypeCode code3;
                                TypeCode code4 = Type.GetTypeCode(parameterType);
                                if (type == null)
                                {
                                    code3 = TypeCode.Empty;
                                }
                                else
                                {
                                    code3 = Type.GetTypeCode(type);
                                }
                                switch (code4)
                                {
                                    case TypeCode.Boolean:
                                    case TypeCode.Byte:
                                    case TypeCode.Int16:
                                    case TypeCode.Int32:
                                    case TypeCode.Int64:
                                    case TypeCode.Single:
                                    case TypeCode.Double:
                                    case TypeCode.Decimal:
                                        switch (code3)
                                        {
                                            case TypeCode.Boolean:
                                            case TypeCode.Byte:
                                            case TypeCode.Int16:
                                            case TypeCode.Int32:
                                            case TypeCode.Int64:
                                            case TypeCode.Single:
                                            case TypeCode.Double:
                                            case TypeCode.Decimal:
                                            case TypeCode.String:
                                                goto Label_0B0F;
                                        }
                                        goto Label_0AF9;
                                }
                            }
                        }
                        goto Label_0B0F;
                    Label_0AF9:
                        if (length == 0)
                        {
                            this.ThrowInvalidCast(type, parameterType, num9);
                        }
                    Label_0B0F:
                        num9++;
                    }
                    if (num9 > num4)
                    {
                        if (num5 != length)
                        {
                            match[length] = match[num5];
                            paramOrder[length] = paramOrder[num5];
                            numArray2[length] = numArray2[num5];
                            match[num5] = null;
                        }
                        length++;
                        if (flag3)
                        {
                            flag = true;
                        }
                    }
                    else
                    {
                        match[num5] = null;
                    }
                }
            }
            if (length == 0)
            {
                throw new MissingMemberException(Utils.GetResourceString("NoMethodTakingXArguments2", new string[] { this.CalledMethodName(), Conversions.ToString(this.GetPropArgCount(args, isPropertySet)) }));
            }
            VBBinderState state = new VBBinderState();
            this.m_state = state;
            ObjState = state;
            state.m_OriginalArgs = args;
            if (length == 1)
            {
                num11 = 0;
            }
            else
            {
                num11 = 0;
                BindScore unknown = BindScore.Unknown;
                int num17 = 0;
                int num41 = length - 1;
                for (num5 = 0; num5 <= num41; num5++)
                {
                    base4 = match[num5];
                    if (base4 != null)
                    {
                        numArray = (int[]) paramOrder[num5];
                        BindScore score2 = this.BindingScore(base4.GetParameters(), numArray, argTypes, isPropertySet, numArray2[num5]);
                        if (score2 < unknown)
                        {
                            if (num5 != 0)
                            {
                                match[0] = match[num5];
                                paramOrder[0] = paramOrder[num5];
                                numArray2[0] = numArray2[num5];
                                match[num5] = null;
                            }
                            num17 = 1;
                            unknown = score2;
                            continue;
                        }
                        if (score2 == unknown)
                        {
                            if ((score2 == BindScore.Exact) || (score2 == BindScore.Widening1))
                            {
                                bool flag4;
                                switch (this.GetMostSpecific(match[0], base4, numArray, paramOrder, isPropertySet, numArray2[0], numArray2[num5], args))
                                {
                                    case -1:
                                    {
                                        if (num17 != num5)
                                        {
                                            match[num17] = match[num5];
                                            paramOrder[num17] = paramOrder[num5];
                                            numArray2[num17] = numArray2[num5];
                                            match[num5] = null;
                                        }
                                        num17++;
                                        continue;
                                    }
                                    case 0:
                                    {
                                        continue;
                                    }
                                    default:
                                    {
                                        flag4 = true;
                                        int num42 = num17 - 1;
                                        for (int m = 1; m <= num42; m++)
                                        {
                                            if (this.GetMostSpecific(match[m], base4, numArray, paramOrder, isPropertySet, numArray2[m], numArray2[num5], args) != 1)
                                            {
                                                flag4 = false;
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                }
                                if (flag4)
                                {
                                    num17 = 0;
                                }
                                if (num5 != num17)
                                {
                                    match[num17] = match[num5];
                                    paramOrder[num17] = paramOrder[num5];
                                    numArray2[num17] = numArray2[num5];
                                    match[num5] = null;
                                }
                                num17++;
                                continue;
                            }
                            if (num17 != num5)
                            {
                                match[num17] = match[num5];
                                paramOrder[num17] = paramOrder[num5];
                                numArray2[num17] = numArray2[num5];
                                match[num5] = null;
                            }
                            num17++;
                            continue;
                        }
                        match[num5] = null;
                    }
                }
                if (num17 > 1)
                {
                    int num43 = match.GetUpperBound(0);
                    for (num5 = 0; num5 <= num43; num5++)
                    {
                        base4 = match[num5];
                        if (base4 != null)
                        {
                            int num44 = match.GetUpperBound(0);
                            for (int n = 0; n <= num44; n++)
                            {
                                if (((num5 != n) && (match[n] != null)) && ((base4 == match[n]) || (base4.DeclaringType.IsSubclassOf(match[n].DeclaringType) && this.MethodsDifferOnlyByReturnType(base4, match[n]))))
                                {
                                    match[n] = null;
                                    num17--;
                                }
                            }
                        }
                    }
                    int num45 = match.GetUpperBound(0);
                    for (num5 = 0; num5 <= num45; num5++)
                    {
                        if (match[num5] == null)
                        {
                            int num46 = match.GetUpperBound(0);
                            for (int num20 = num5 + 1; num20 <= num46; num20++)
                            {
                                MethodBase base5 = match[num20];
                                if (base5 != null)
                                {
                                    match[num5] = base5;
                                    paramOrder[num5] = paramOrder[num20];
                                    numArray2[num5] = numArray2[num20];
                                    match[num20] = null;
                                }
                            }
                        }
                    }
                }
                if (num17 > 1)
                {
                    string str = "\r\n    " + Utils.MethodToString(match[0]);
                    int num47 = num17 - 1;
                    for (num5 = 1; num5 <= num47; num5++)
                    {
                        str = str + "\r\n    " + Utils.MethodToString(match[num5]);
                    }
                    switch (unknown)
                    {
                        case BindScore.Exact:
                            throw new AmbiguousMatchException(Utils.GetResourceString("AmbiguousCall_ExactMatch2", new string[] { this.CalledMethodName(), str }));

                        case BindScore.Widening0:
                        case BindScore.Widening1:
                            throw new AmbiguousMatchException(Utils.GetResourceString("AmbiguousCall_WideningConversion2", new string[] { this.CalledMethodName(), str }));
                    }
                    throw new AmbiguousMatchException(Utils.GetResourceString("AmbiguousCall2", new string[] { this.CalledMethodName(), str }));
                }
            }
            MethodBase base3 = match[num11];
            numArray = (int[]) paramOrder[num11];
            if (names != null)
            {
                this.ReorderParams(numArray, args, state);
            }
            ParameterInfo[] parameters = base3.GetParameters();
            if (args.Length > 0)
            {
                state.m_ByRefFlags = new bool[args.GetUpperBound(0) + 1];
                flag = false;
                int num48 = parameters.GetUpperBound(0);
                for (num9 = 0; num9 <= num48; num9++)
                {
                    if (parameters[num9].ParameterType.IsByRef)
                    {
                        if (state.m_OriginalParamOrder == null)
                        {
                            if (num9 < state.m_ByRefFlags.Length)
                            {
                                state.m_ByRefFlags[num9] = true;
                            }
                        }
                        else if (num9 < state.m_OriginalParamOrder.Length)
                        {
                            int num21 = state.m_OriginalParamOrder[num9];
                            if (num21 >= 0)
                            {
                                state.m_ByRefFlags[num21] = true;
                            }
                        }
                        flag = true;
                    }
                }
                if (!flag)
                {
                    state.m_ByRefFlags = null;
                }
            }
            else
            {
                state.m_ByRefFlags = null;
            }
            num8 = numArray2[num11];
            if (num8 != -1)
            {
                num4 = parameters.GetUpperBound(0);
                if (isPropertySet)
                {
                    num4--;
                }
                num3 = args.GetUpperBound(0);
                if (isPropertySet)
                {
                    num3--;
                }
                object[] objArray4 = new object[(parameters.Length - 1) + 1];
                int num49 = Math.Min(num3, num8) - 1;
                for (num9 = 0; num9 <= num49; num9++)
                {
                    objArray4[num9] = ObjectType.CTypeHelper(args[num9], parameters[num9].ParameterType);
                }
                if (num3 < num8)
                {
                    int num50 = num8 - 1;
                    for (num9 = num3 + 1; num9 <= num50; num9++)
                    {
                        objArray4[num9] = ObjectType.CTypeHelper(parameters[num9].DefaultValue, parameters[num9].ParameterType);
                    }
                }
                if (isPropertySet)
                {
                    int num22 = objArray4.GetUpperBound(0);
                    objArray4[num22] = ObjectType.CTypeHelper(args[args.GetUpperBound(0)], parameters[num22].ParameterType);
                }
                if (num3 == -1)
                {
                    objArray4[num8] = Array.CreateInstance(elementType, 0);
                }
                else
                {
                    elementType = parameters[num4].ParameterType.GetElementType();
                    int num23 = (args.Length - parameters.Length) + 1;
                    parameterType = parameters[num4].ParameterType;
                    if (((num23 == 1) && parameterType.IsArray) && ((args[num8] == null) || parameterType.IsInstanceOfType(args[num8])))
                    {
                        objArray4[num8] = args[num8];
                    }
                    else if (elementType == typeof(object))
                    {
                        object[] objArray5 = new object[(num23 - 1) + 1];
                        int num51 = num23 - 1;
                        for (num = 0; num <= num51; num++)
                        {
                            objArray5[num] = ObjectType.CTypeHelper(args[num + num8], elementType);
                        }
                        objArray4[num8] = objArray5;
                    }
                    else
                    {
                        Array array = Array.CreateInstance(elementType, num23);
                        int num52 = num23 - 1;
                        for (num = 0; num <= num52; num++)
                        {
                            array.SetValue(ObjectType.CTypeHelper(args[num + num8], elementType), num);
                        }
                        objArray4[num8] = array;
                    }
                }
                args = objArray4;
            }
            else
            {
                object[] objArray6 = new object[(parameters.Length - 1) + 1];
                int num53 = objArray6.GetUpperBound(0);
                num = 0;
                while (num <= num53)
                {
                    int num24 = numArray[num];
                    if ((num24 >= 0) && (num24 <= args.GetUpperBound(0)))
                    {
                        objArray6[num] = ObjectType.CTypeHelper(args[num24], parameters[num].ParameterType);
                    }
                    else
                    {
                        objArray6[num] = ObjectType.CTypeHelper(parameters[num].DefaultValue, parameters[num].ParameterType);
                    }
                    num++;
                }
                int num54 = parameters.GetUpperBound(0);
                for (num9 = num; num9 <= num54; num9++)
                {
                    objArray6[num9] = ObjectType.CTypeHelper(parameters[num9].DefaultValue, parameters[num9].ParameterType);
                }
                args = objArray6;
            }
            if (base3 == null)
            {
                throw new MissingMemberException(Utils.GetResourceString("NoMethodTakingXArguments2", new string[] { this.CalledMethodName(), Conversions.ToString(this.GetPropArgCount(args, isPropertySet)) }));
            }
            return base3;
        }

        internal void CacheMember(MemberInfo member)
        {
            this.m_CachedMember = member;
        }

        internal string CalledMethodName()
        {
            return (this.m_objType.Name + "." + this.m_BindToName);
        }

        public override object ChangeType(object value, Type typ, CultureInfo culture)
        {
            object obj2;
            try
            {
                if ((typ == typeof(object)) || (typ.IsByRef && (typ.GetElementType() == typeof(object))))
                {
                    return value;
                }
                obj2 = ObjectType.CTypeHelper(value, typ);
            }
            catch (Exception)
            {
                throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromTo", new string[] { Utils.VBFriendlyName(value), Utils.VBFriendlyName(typ) }));
            }
            return obj2;
        }

        private Exception CreateParamOrder(bool SetProp, int[] paramOrder, ParameterInfo[] pars, object[] args, string[] names)
        {
            int num;
            bool[] flagArray = new bool[(pars.Length - 1) + 1];
            int num4 = (args.Length - names.Length) - 1;
            int upperBound = pars.GetUpperBound(0);
            int num5 = pars.GetUpperBound(0);
            for (num = 0; num <= num5; num++)
            {
                paramOrder[num] = -1;
            }
            if (SetProp)
            {
                paramOrder[pars.GetUpperBound(0)] = args.GetUpperBound(0);
                num4--;
                upperBound--;
            }
            int num6 = num4;
            for (num = 0; num <= num6; num++)
            {
                paramOrder[num] = names.Length + num;
            }
            int num7 = names.GetUpperBound(0);
            for (num = 0; num <= num7; num++)
            {
                int num8 = upperBound;
                int index = 0;
                while (index <= num8)
                {
                    if (Strings.StrComp(names[num], pars[index].Name, CompareMethod.Text) == 0)
                    {
                        if (paramOrder[index] != -1)
                        {
                            return new ArgumentException(Utils.GetResourceString("NamedArgumentAlreadyUsed1", new string[] { pars[index].Name }));
                        }
                        paramOrder[index] = num;
                        flagArray[num] = true;
                        break;
                    }
                    index++;
                }
                if (index > upperBound)
                {
                    return new MissingMemberException(Utils.GetResourceString("Argument_InvalidNamedArg2", new string[] { names[num], this.CalledMethodName() }));
                }
            }
            return null;
        }

        private string GetDefaultMemberName(Type typ)
        {
            do
            {
                object[] customAttributes = typ.GetCustomAttributes(typeof(DefaultMemberAttribute), false);
                if ((customAttributes != null) && (customAttributes.Length != 0))
                {
                    return ((DefaultMemberAttribute) customAttributes[0]).MemberName;
                }
                typ = typ.BaseType;
            }
            while (typ != null);
            return null;
        }

        private MethodBase[] GetMethodsByName(Type objType, IReflect objIReflect, string name, BindingFlags invokeAttr)
        {
            int num3;
            MemberInfo[] nonGenericMembers = LateBinding.GetNonGenericMembers(objIReflect.GetMember(name, invokeAttr));
            if (nonGenericMembers == null)
            {
                return null;
            }
            int upperBound = nonGenericMembers.GetUpperBound(0);
            for (int i = 0; i <= upperBound; i++)
            {
                MemberInfo info = nonGenericMembers[i];
                if (info != null)
                {
                    Type declaringType;
                    if (info.MemberType == MemberTypes.Field)
                    {
                        declaringType = info.DeclaringType;
                        int num12 = nonGenericMembers.GetUpperBound(0);
                        for (int k = 0; k <= num12; k++)
                        {
                            if (((i != k) && (nonGenericMembers[k] != null)) && declaringType.IsSubclassOf(nonGenericMembers[k].DeclaringType))
                            {
                                nonGenericMembers[k] = null;
                                num3++;
                            }
                        }
                    }
                    else
                    {
                        MethodInfo getMethod;
                        if (info.MemberType == MemberTypes.Method)
                        {
                            getMethod = (MethodInfo) info;
                            if (!getMethod.IsHideBySig && ((!getMethod.IsVirtual || (getMethod.IsVirtual && ((getMethod.Attributes & MethodAttributes.NewSlot) != MethodAttributes.PrivateScope))) || (getMethod.IsVirtual && ((getMethod.GetBaseDefinition().Attributes & MethodAttributes.NewSlot) != MethodAttributes.PrivateScope))))
                            {
                                declaringType = info.DeclaringType;
                                int num13 = nonGenericMembers.GetUpperBound(0);
                                for (int m = 0; m <= num13; m++)
                                {
                                    if (((i != m) && (nonGenericMembers[m] != null)) && declaringType.IsSubclassOf(nonGenericMembers[m].DeclaringType))
                                    {
                                        nonGenericMembers[m] = null;
                                        num3++;
                                    }
                                }
                            }
                        }
                        else if (info.MemberType == MemberTypes.Property)
                        {
                            PropertyInfo info3 = (PropertyInfo) info;
                            int num7 = 1;
                            do
                            {
                                if (num7 == 1)
                                {
                                    getMethod = info3.GetGetMethod();
                                }
                                else
                                {
                                    getMethod = info3.GetSetMethod();
                                }
                                if (((getMethod != null) && !getMethod.IsHideBySig) && (!getMethod.IsVirtual || (getMethod.IsVirtual && ((getMethod.Attributes & MethodAttributes.NewSlot) != MethodAttributes.PrivateScope))))
                                {
                                    declaringType = info.DeclaringType;
                                    int num14 = nonGenericMembers.GetUpperBound(0);
                                    for (int n = 0; n <= num14; n++)
                                    {
                                        if (((i != n) && (nonGenericMembers[n] != null)) && declaringType.IsSubclassOf(nonGenericMembers[n].DeclaringType))
                                        {
                                            nonGenericMembers[n] = null;
                                            num3++;
                                        }
                                    }
                                }
                                num7++;
                            }
                            while (num7 <= 2);
                            if ((invokeAttr & BindingFlags.GetProperty) != BindingFlags.Default)
                            {
                                getMethod = info3.GetGetMethod();
                            }
                            else if ((invokeAttr & BindingFlags.SetProperty) != BindingFlags.Default)
                            {
                                getMethod = info3.GetSetMethod();
                            }
                            else
                            {
                                getMethod = null;
                            }
                            if (getMethod == null)
                            {
                                num3++;
                            }
                            nonGenericMembers[i] = getMethod;
                        }
                        else if (info.MemberType == MemberTypes.NestedType)
                        {
                            declaringType = info.DeclaringType;
                            int num15 = nonGenericMembers.GetUpperBound(0);
                            for (int num9 = 0; num9 <= num15; num9++)
                            {
                                if (((i != num9) && (nonGenericMembers[num9] != null)) && declaringType.IsSubclassOf(nonGenericMembers[num9].DeclaringType))
                                {
                                    nonGenericMembers[num9] = null;
                                    num3++;
                                }
                            }
                            if (num3 == (nonGenericMembers.Length - 1))
                            {
                                throw new ArgumentException(Utils.GetResourceString("Argument_IllegalNestedType2", new string[] { name, Utils.VBFriendlyName(objType) }));
                            }
                            nonGenericMembers[i] = null;
                            num3++;
                        }
                    }
                }
            }
            int num2 = nonGenericMembers.Length - num3;
            MethodBase[] baseArray2 = new MethodBase[(num2 - 1) + 1];
            int index = 0;
            int num16 = nonGenericMembers.Length - 1;
            for (int j = 0; j <= num16; j++)
            {
                if (nonGenericMembers[j] != null)
                {
                    baseArray2[index] = (MethodBase) nonGenericMembers[j];
                    index++;
                }
            }
            return baseArray2;
        }

        private int GetMostSpecific(MethodBase match0, MethodBase ThisMethod, int[] ArgIndexes, object[] ParamOrder, bool IsPropertySet, int ParamArrayIndex0, int ParamArrayIndex1, object[] args)
        {
            bool flag;
            bool flag2;
            int num8 = -1;
            Type fromType = null;
            Type toType = null;
            int upperBound = args.GetUpperBound(0);
            ParameterInfo[] parameters = ThisMethod.GetParameters();
            ParameterInfo[] infoArray = match0.GetParameters();
            int[] numArray = (int[]) ParamOrder[0];
            num8 = -1;
            int index = args.GetUpperBound(0);
            int num5 = infoArray.GetUpperBound(0);
            int num6 = parameters.GetUpperBound(0);
            if (IsPropertySet)
            {
                num5--;
                num6--;
                index--;
                upperBound--;
            }
            if (ParamArrayIndex0 == -1)
            {
                flag = false;
            }
            else
            {
                fromType = infoArray[ParamArrayIndex0].ParameterType.GetElementType();
                flag = true;
                if ((index != -1) && (index == num5))
                {
                    object o = args[index];
                    if ((o == null) || infoArray[num5].ParameterType.IsInstanceOfType(o))
                    {
                        flag = false;
                    }
                }
            }
            if (ParamArrayIndex1 == -1)
            {
                flag2 = false;
            }
            else
            {
                toType = parameters[ParamArrayIndex1].ParameterType.GetElementType();
                flag2 = true;
                if ((index != -1) && (index == num6))
                {
                    object obj3 = args[index];
                    if ((obj3 == null) || parameters[num6].ParameterType.IsInstanceOfType(obj3))
                    {
                        flag2 = false;
                    }
                }
            }
            int num10 = Math.Min(upperBound, Math.Max(num5, num6));
            for (int i = 0; i <= num10; i++)
            {
                int num3;
                int num4;
                if (i <= num5)
                {
                    num3 = numArray[i];
                }
                else
                {
                    num3 = -1;
                }
                if (i <= num6)
                {
                    num4 = ArgIndexes[i];
                }
                else
                {
                    num4 = -1;
                }
                if ((num3 != -1) || (num4 != -1))
                {
                    Type parameterType;
                    Type elementType;
                    if ((flag2 && (ParamArrayIndex1 != -1)) && (i >= ParamArrayIndex1))
                    {
                        if ((flag && (ParamArrayIndex0 != -1)) && (i >= ParamArrayIndex0))
                        {
                            parameterType = fromType;
                        }
                        else
                        {
                            parameterType = infoArray[num3].ParameterType;
                            if (parameterType.IsByRef)
                            {
                                parameterType = parameterType.GetElementType();
                            }
                        }
                        if (toType == parameterType)
                        {
                            if (((num8 == -1) && (ParamArrayIndex0 == -1)) && ((i == num5) && (args[num5] != null)))
                            {
                                num8 = 0;
                            }
                            continue;
                        }
                        if (ObjectType.IsWideningConversion(parameterType, toType))
                        {
                            if (num8 != 1)
                            {
                                num8 = 0;
                                continue;
                            }
                            num8 = -1;
                        }
                        else
                        {
                            if (!ObjectType.IsWideningConversion(toType, parameterType))
                            {
                                continue;
                            }
                            if (num8 != 0)
                            {
                                num8 = 1;
                                continue;
                            }
                            num8 = -1;
                        }
                        break;
                    }
                    if ((flag && (ParamArrayIndex0 != -1)) && (i >= ParamArrayIndex0))
                    {
                        if ((flag2 && (ParamArrayIndex1 != -1)) && (i >= ParamArrayIndex1))
                        {
                            elementType = toType;
                        }
                        else
                        {
                            elementType = parameters[num4].ParameterType;
                            if (elementType.IsByRef)
                            {
                                elementType = elementType.GetElementType();
                            }
                        }
                        if (fromType == elementType)
                        {
                            if (((num8 == -1) && (ParamArrayIndex1 == -1)) && ((i == num6) && (args[num6] != null)))
                            {
                                num8 = 1;
                            }
                            continue;
                        }
                        if (ObjectType.IsWideningConversion(fromType, elementType))
                        {
                            if (num8 != 1)
                            {
                                num8 = 0;
                                continue;
                            }
                            num8 = -1;
                        }
                        else
                        {
                            if (!ObjectType.IsWideningConversion(elementType, fromType))
                            {
                                continue;
                            }
                            if (num8 != 0)
                            {
                                num8 = 1;
                                continue;
                            }
                            num8 = -1;
                        }
                        break;
                    }
                    parameterType = infoArray[numArray[i]].ParameterType;
                    elementType = parameters[ArgIndexes[i]].ParameterType;
                    if (parameterType != elementType)
                    {
                        if (ObjectType.IsWideningConversion(parameterType, elementType))
                        {
                            if (num8 != 1)
                            {
                                num8 = 0;
                                continue;
                            }
                            num8 = -1;
                            break;
                        }
                        if (ObjectType.IsWideningConversion(elementType, parameterType))
                        {
                            if (num8 != 0)
                            {
                                num8 = 1;
                                continue;
                            }
                            num8 = -1;
                            break;
                        }
                        if (ObjectType.IsWiderNumeric(parameterType, elementType))
                        {
                            if (num8 != 0)
                            {
                                num8 = 1;
                                continue;
                            }
                            num8 = -1;
                            break;
                        }
                        if (ObjectType.IsWiderNumeric(elementType, parameterType))
                        {
                            if (num8 != 1)
                            {
                                num8 = 0;
                                continue;
                            }
                            num8 = -1;
                            break;
                        }
                        num8 = -1;
                    }
                }
            }
            if (num8 == -1)
            {
                if (((ParamArrayIndex0 == -1) || !flag) && (ParamArrayIndex1 != -1))
                {
                    if (flag2 && this.MatchesParamArraySignature(infoArray, parameters, ParamArrayIndex1, IsPropertySet, upperBound))
                    {
                        num8 = 0;
                    }
                    return num8;
                }
                if ((ParamArrayIndex1 != -1) && flag2)
                {
                    return num8;
                }
                if (((ParamArrayIndex0 != -1) && flag) && this.MatchesParamArraySignature(parameters, infoArray, ParamArrayIndex0, IsPropertySet, upperBound))
                {
                    num8 = 1;
                }
            }
            return num8;
        }

        private int GetPropArgCount(object[] args, bool IsPropertySet)
        {
            if (IsPropertySet)
            {
                return (args.Length - 1);
            }
            return args.Length;
        }

        [DebuggerHidden, DebuggerStepThrough, SecuritySafeCritical]
        internal object InvokeMember(string name, BindingFlags invokeAttr, Type objType, IReflect objIReflect, object target, object[] args, string[] namedParameters)
        {
            object obj4;
            if (objType.IsCOMObject)
            {
                ParameterModifier[] modifiers = null;
                if (((this.m_ByRefFlags != null) && (target != null)) && !RemotingServices.IsTransparentProxy(target))
                {
                    ParameterModifier modifier = new ParameterModifier(args.Length);
                    modifiers = new ParameterModifier[] { modifier };
                    object obj5 = Missing.Value;
                    int upperBound = args.GetUpperBound(0);
                    for (int i = 0; i <= upperBound; i++)
                    {
                        if (args[i] != obj5)
                        {
                            modifier[i] = this.m_ByRefFlags[i];
                        }
                    }
                }
                try
                {
                    new SecurityPermission(PermissionState.Unrestricted).Demand();
                    return objIReflect.InvokeMember(name, invokeAttr, null, target, args, modifiers, null, namedParameters);
                }
                catch (MissingMemberException)
                {
                    throw new MissingMemberException(Utils.GetResourceString("MissingMember_MemberNotFoundOnType2", new string[] { name, Utils.VBFriendlyName(objType) }));
                }
            }
            this.m_BindToName = name;
            this.m_objType = objType;
            if (name.Length == 0)
            {
                if (objType == objIReflect)
                {
                    name = this.GetDefaultMemberName(objType);
                    if (name == null)
                    {
                        throw new MissingMemberException(Utils.GetResourceString("MissingMember_NoDefaultMemberFound1", new string[] { Utils.VBFriendlyName(objType) }));
                    }
                }
                else
                {
                    name = "";
                }
            }
            MethodBase[] match = this.GetMethodsByName(objType, objIReflect, name, invokeAttr);
            if (args == null)
            {
                args = new object[0];
            }
            object objState = null;
            MethodBase member = this.BindToMethod(invokeAttr, match, ref args, null, null, namedParameters, ref objState);
            if (member == null)
            {
                throw new MissingMemberException(Utils.GetResourceString("NoMethodTakingXArguments2", new string[] { this.CalledMethodName(), Conversions.ToString(this.GetPropArgCount(args, (invokeAttr & BindingFlags.SetProperty) != BindingFlags.Default)) }));
            }
            SecurityCheckForLateboundCalls(member, objType, objIReflect);
            MethodInfo info = (MethodInfo) member;
            if (((objType == objIReflect) || info.IsStatic) || LateBinding.DoesTargetObjectMatch(target, info))
            {
                LateBinding.VerifyObjRefPresentForInstanceCall(target, info);
                obj4 = info.Invoke(target, args);
            }
            else
            {
                obj4 = LateBinding.InvokeMemberOnIReflect(objIReflect, info, BindingFlags.InvokeMethod, target, args);
            }
            if (objState != null)
            {
                this.ReorderArgumentArray(ref args, objState);
            }
            return obj4;
        }

        private static bool IsMemberPublic(MemberInfo Member)
        {
            switch (Member.MemberType)
            {
                case MemberTypes.Constructor:
                    return ((ConstructorInfo) Member).IsPublic;

                case MemberTypes.Field:
                    return ((FieldInfo) Member).IsPublic;

                case MemberTypes.Method:
                    return ((MethodInfo) Member).IsPublic;

                case MemberTypes.Property:
                    return false;
            }
            return false;
        }

        private bool MatchesParamArraySignature(ParameterInfo[] param0, ParameterInfo[] param1, int ParamArrayIndex1, bool IsPropertySet, int ArgCountUpperBound)
        {
            int upperBound = param0.GetUpperBound(0);
            if (IsPropertySet)
            {
                upperBound--;
            }
            upperBound = Math.Min(upperBound, ArgCountUpperBound);
            int num3 = upperBound;
            for (int i = 0; i <= num3; i++)
            {
                Type elementType;
                Type parameterType = param0[i].ParameterType;
                if (parameterType.IsByRef)
                {
                    parameterType = parameterType.GetElementType();
                }
                if (i >= ParamArrayIndex1)
                {
                    elementType = param1[ParamArrayIndex1].ParameterType.GetElementType();
                }
                else
                {
                    elementType = param1[i].ParameterType;
                    if (elementType.IsByRef)
                    {
                        elementType = elementType.GetElementType();
                    }
                }
                if (parameterType != elementType)
                {
                    return false;
                }
            }
            return true;
        }

        private bool MethodsDifferOnlyByReturnType(MethodBase match1, MethodBase match2)
        {
            int num;
            if (match1 == match2)
            {
            }
            ParameterInfo[] parameters = match1.GetParameters();
            ParameterInfo[] infoArray2 = match2.GetParameters();
            int num2 = Math.Min(parameters.GetUpperBound(0), infoArray2.GetUpperBound(0));
            int num3 = num2;
            for (num = 0; num <= num3; num++)
            {
                Type parameterType = parameters[num].ParameterType;
                if (parameterType.IsByRef)
                {
                    parameterType = parameterType.GetElementType();
                }
                Type elementType = infoArray2[num].ParameterType;
                if (elementType.IsByRef)
                {
                    elementType = elementType.GetElementType();
                }
                if (parameterType != elementType)
                {
                    return false;
                }
            }
            if (parameters.Length > infoArray2.Length)
            {
                int upperBound = infoArray2.GetUpperBound(0);
                for (num = num2 + 1; num <= upperBound; num++)
                {
                    if (!parameters[num].IsOptional)
                    {
                        return false;
                    }
                }
            }
            else if (infoArray2.Length > parameters.Length)
            {
                int num5 = parameters.GetUpperBound(0);
                for (num = num2 + 1; num <= num5; num++)
                {
                    if (!infoArray2[num].IsOptional)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override void ReorderArgumentArray(ref object[] args, object objState)
        {
            VBBinderState state = (VBBinderState) objState;
            if ((args != null) && (state != null))
            {
                int num;
                if (state.m_OriginalParamOrder != null)
                {
                    if (this.m_ByRefFlags != null)
                    {
                        if (state.m_ByRefFlags == null)
                        {
                            int upperBound = this.m_ByRefFlags.GetUpperBound(0);
                            for (num = 0; num <= upperBound; num++)
                            {
                                this.m_ByRefFlags[num] = false;
                            }
                        }
                        else
                        {
                            int num4 = state.m_OriginalParamOrder.GetUpperBound(0);
                            for (num = 0; num <= num4; num++)
                            {
                                int index = state.m_OriginalParamOrder[num];
                                if ((index >= 0) && (index <= args.GetUpperBound(0)))
                                {
                                    this.m_ByRefFlags[index] = state.m_ByRefFlags[index];
                                    state.m_OriginalArgs[index] = args[num];
                                }
                            }
                        }
                    }
                }
                else if (this.m_ByRefFlags != null)
                {
                    if (state.m_ByRefFlags == null)
                    {
                        int num5 = this.m_ByRefFlags.GetUpperBound(0);
                        for (num = 0; num <= num5; num++)
                        {
                            this.m_ByRefFlags[num] = false;
                        }
                    }
                    else
                    {
                        int num6 = this.m_ByRefFlags.GetUpperBound(0);
                        for (num = 0; num <= num6; num++)
                        {
                            if (this.m_ByRefFlags[num])
                            {
                                bool flag = state.m_ByRefFlags[num];
                                this.m_ByRefFlags[num] = flag;
                                if (flag)
                                {
                                    state.m_OriginalArgs[num] = args[num];
                                }
                            }
                        }
                    }
                }
            }
            if (state != null)
            {
                state.m_OriginalParamOrder = null;
                state.m_ByRefFlags = null;
            }
        }

        private void ReorderParams(int[] paramOrder, object[] vars, VBBinderState state)
        {
            int num = Math.Max(vars.GetUpperBound(0), paramOrder.GetUpperBound(0));
            state.m_OriginalParamOrder = new int[num + 1];
            int num3 = num;
            for (int i = 0; i <= num3; i++)
            {
                state.m_OriginalParamOrder[i] = paramOrder[i];
            }
        }

        internal static void SecurityCheckForLateboundCalls(MemberInfo member, Type objType, IReflect objIReflect)
        {
            if ((objType != objIReflect) && !IsMemberPublic(member))
            {
                throw new MissingMethodException();
            }
            Type declaringType = member.DeclaringType;
            if (!declaringType.IsPublic && (declaringType.Assembly == Utils.VBRuntimeAssembly))
            {
                throw new MissingMethodException();
            }
        }

        public override MethodBase SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotSupportedException();
        }

        public override PropertyInfo SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers)
        {
            BindScore unknown = BindScore.Unknown;
            int index = 0;
            int upperBound = match.GetUpperBound(0);
            for (int i = 0; i <= upperBound; i++)
            {
                PropertyInfo info2 = match[i];
                if (info2 != null)
                {
                    BindScore score2 = this.BindingScore(info2.GetIndexParameters(), null, indexes, false, -1);
                    if (score2 < unknown)
                    {
                        if (i != 0)
                        {
                            match[0] = match[i];
                            match[i] = null;
                        }
                        index = 1;
                        unknown = score2;
                        continue;
                    }
                    if (score2 == unknown)
                    {
                        if (score2 == BindScore.Widening1)
                        {
                            int num6 = -1;
                            ParameterInfo[] indexParameters = info2.GetIndexParameters();
                            ParameterInfo[] infoArray = match[0].GetIndexParameters();
                            num6 = -1;
                            int num8 = indexParameters.GetUpperBound(0);
                            for (int j = 0; j <= num8; j++)
                            {
                                int num4 = j;
                                int num5 = j;
                                if ((num4 != -1) && (num5 != -1))
                                {
                                    Type parameterType = infoArray[num4].ParameterType;
                                    Type toType = indexParameters[num5].ParameterType;
                                    if (ObjectType.IsWideningConversion(parameterType, toType))
                                    {
                                        if (num6 != 1)
                                        {
                                            num6 = 0;
                                            continue;
                                        }
                                        num6 = -1;
                                        break;
                                    }
                                    if (ObjectType.IsWideningConversion(toType, parameterType))
                                    {
                                        if (num6 != 0)
                                        {
                                            num6 = 1;
                                        }
                                        else
                                        {
                                            num6 = -1;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (num6 == -1)
                            {
                                if (index != i)
                                {
                                    match[index] = match[i];
                                    match[i] = null;
                                }
                                index++;
                            }
                            else if (num6 == 0)
                            {
                                index = 1;
                            }
                            else
                            {
                                if (i != 0)
                                {
                                    match[0] = match[i];
                                    match[i] = null;
                                }
                                index = 1;
                            }
                            continue;
                        }
                        if (score2 == BindScore.Exact)
                        {
                            if (info2.DeclaringType.IsSubclassOf(match[0].DeclaringType))
                            {
                                if (i != 0)
                                {
                                    match[0] = match[i];
                                    match[i] = null;
                                }
                                index = 1;
                            }
                            else if (!match[0].DeclaringType.IsSubclassOf(info2.DeclaringType))
                            {
                                if (index != i)
                                {
                                    match[index] = match[i];
                                    match[i] = null;
                                }
                                index++;
                            }
                        }
                        else
                        {
                            if (index != i)
                            {
                                match[index] = match[i];
                                match[i] = null;
                            }
                            index++;
                        }
                        continue;
                    }
                    match[i] = null;
                }
            }
            if (index == 1)
            {
                return match[0];
            }
            return null;
        }

        private void ThrowInvalidCast(Type ArgType, Type ParmType, int ParmIndex)
        {
            throw new InvalidCastException(Utils.GetResourceString("InvalidCast_FromToArg4", new string[] { this.CalledMethodName(), Conversions.ToString((int) (ParmIndex + 1)), Utils.VBFriendlyName(ArgType), Utils.VBFriendlyName(ParmType) }));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public enum BindScore
        {
            Exact,
            Widening0,
            Widening1,
            Narrowing,
            Unknown
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal sealed class VBBinderState
        {
            internal bool[] m_ByRefFlags;
            internal object[] m_OriginalArgs;
            internal bool[] m_OriginalByRefFlags;
            internal int[] m_OriginalParamOrder;

            internal VBBinderState()
            {
            }
        }
    }
}

