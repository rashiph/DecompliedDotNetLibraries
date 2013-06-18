namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Utils
    {
        internal const char chBackslash = '\\';
        internal const char chCharH0A = '\n';
        internal const char chCharH0B = '\v';
        internal const char chCharH0C = '\f';
        internal const char chCharH0D = '\r';
        internal const char chColon = ':';
        internal const char chDblQuote = '"';
        internal const char chGenericManglingChar = '`';
        internal const char chHyphen = '-';
        internal const char chIntlSpace = '　';
        internal const char chLetterA = 'A';
        internal const char chLetterZ = 'Z';
        internal const char chLineFeed = '\n';
        internal const char chPeriod = '.';
        internal const char chPlus = '+';
        internal const char chSlash = '/';
        internal const char chSpace = ' ';
        internal const char chTab = '\t';
        internal const char chZero = '0';
        private const int ERROR_INVALID_PARAMETER = 0x57;
        internal const int FACILITY_CONTROL = 0xa0000;
        internal const int FACILITY_ITF = 0x40000;
        internal const int FACILITY_RPC = 0x10000;
        internal static char[] m_achIntlSpace = new char[] { ' ', '　' };
        private static bool m_TriedLoadingResourceManager;
        private static ResourceManager m_VBAResourceManager;
        private static Assembly m_VBRuntimeAssembly;
        internal const CompareOptions OptionCompareTextFlags = (CompareOptions.IgnoreWidth | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreCase);
        private static readonly object ResourceManagerSyncObj = new object();
        private const string ResourceMsgDefault = "Message text unavailable.  Resource file 'Microsoft.VisualBasic resources' not found.";
        internal const int SCODE_FACILITY = 0x1fff0000;
        internal const int SEVERITY_ERROR = -2147483648;
        private const string VBDefaultErrorID = "ID95";
        private static readonly Type VoidType = Type.GetType("System.Void");

        private Utils()
        {
        }

        internal static string AdjustArraySuffix(string sRank)
        {
            string str2 = null;
            for (int i = sRank.Length; i > 0; i--)
            {
                char ch = sRank[i - 1];
                switch (ch)
                {
                    case '(':
                    {
                        str2 = str2 + ")";
                        continue;
                    }
                    case ')':
                    {
                        str2 = str2 + "(";
                        continue;
                    }
                    case ',':
                    {
                        str2 = str2 + Conversions.ToString(ch);
                        continue;
                    }
                }
                str2 = Conversions.ToString(ch) + str2;
            }
            return str2;
        }

        public static Array CopyArray(Array arySrc, Array aryDest)
        {
            if (arySrc != null)
            {
                int length = arySrc.Length;
                if (length == 0)
                {
                    return aryDest;
                }
                if (aryDest.Rank != arySrc.Rank)
                {
                    throw ExceptionUtils.VbMakeException(new InvalidCastException(GetResourceString("Array_RankMismatch")), 9);
                }
                int num8 = aryDest.Rank - 2;
                for (int i = 0; i <= num8; i++)
                {
                    if (aryDest.GetUpperBound(i) != arySrc.GetUpperBound(i))
                    {
                        throw ExceptionUtils.VbMakeException(new ArrayTypeMismatchException(GetResourceString("Array_TypeMismatch")), 9);
                    }
                }
                if (length > aryDest.Length)
                {
                    length = aryDest.Length;
                }
                if (arySrc.Rank > 1)
                {
                    int rank = arySrc.Rank;
                    int num7 = arySrc.GetLength(rank - 1);
                    int num6 = aryDest.GetLength(rank - 1);
                    if (num6 != 0)
                    {
                        int num5 = Math.Min(num7, num6);
                        int num9 = (arySrc.Length / num7) - 1;
                        for (int j = 0; j <= num9; j++)
                        {
                            Array.Copy(arySrc, j * num7, aryDest, j * num6, num5);
                        }
                    }
                    return aryDest;
                }
                Array.Copy(arySrc, aryDest, length);
            }
            return aryDest;
        }

        internal static string FieldToString(FieldInfo Field)
        {
            string str = "";
            Type fieldType = Field.FieldType;
            if (Field.IsPublic)
            {
                str = str + "Public ";
            }
            else if (Field.IsPrivate)
            {
                str = str + "Private ";
            }
            else if (Field.IsAssembly)
            {
                str = str + "Friend ";
            }
            else if (Field.IsFamily)
            {
                str = str + "Protected ";
            }
            else if (Field.IsFamilyOrAssembly)
            {
                str = str + "Protected Friend ";
            }
            return ((str + Field.Name) + " As " + VBFriendlyNameOfType(fieldType, true));
        }

        private static string GetArraySuffixAndElementType(ref Type typ)
        {
            if (!typ.IsArray)
            {
                return null;
            }
            StringBuilder builder = new StringBuilder();
            do
            {
                builder.Append("(");
                builder.Append(',', typ.GetArrayRank() - 1);
                builder.Append(")");
                typ = typ.GetElementType();
            }
            while (typ.IsArray);
            return builder.ToString();
        }

        internal static CultureInfo GetCultureInfo()
        {
            return Thread.CurrentThread.CurrentCulture;
        }

        internal static DateTimeFormatInfo GetDateTimeFormatInfo()
        {
            return Thread.CurrentThread.CurrentCulture.DateTimeFormat;
        }

        internal static Encoding GetFileIOEncoding()
        {
            return Encoding.Default;
        }

        private static string GetGenericArgsSuffix(Type typ)
        {
            if (!typ.IsGenericType)
            {
                return null;
            }
            Type[] genericArguments = typ.GetGenericArguments();
            int length = genericArguments.Length;
            int num2 = length;
            if (typ.IsNested && typ.DeclaringType.IsGenericType)
            {
                num2 -= typ.DeclaringType.GetGenericArguments().Length;
            }
            if (num2 == 0)
            {
                return null;
            }
            StringBuilder builder = new StringBuilder();
            builder.Append("(Of ");
            int num4 = length - 1;
            for (int i = length - num2; i <= num4; i++)
            {
                builder.Append(VBFriendlyNameOfType(genericArguments[i], false));
                if (i != (length - 1))
                {
                    builder.Append(',');
                }
            }
            builder.Append(")");
            return builder.ToString();
        }

        internal static CultureInfo GetInvariantCultureInfo()
        {
            return CultureInfo.InvariantCulture;
        }

        internal static int GetLocaleCodePage()
        {
            return Thread.CurrentThread.CurrentCulture.TextInfo.ANSICodePage;
        }

        internal static string GetResourceString(vbErrors ResourceId)
        {
            return GetResourceString("ID" + Conversions.ToString((int) ResourceId));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static string GetResourceString(string ResourceKey)
        {
            string str2;
            if (VBAResourceManager == null)
            {
                return "Message text unavailable.  Resource file 'Microsoft.VisualBasic resources' not found.";
            }
            try
            {
                str2 = VBAResourceManager.GetString(ResourceKey, GetCultureInfo());
                if (str2 == null)
                {
                    str2 = VBAResourceManager.GetString("ID95");
                }
            }
            catch (StackOverflowException exception)
            {
                throw exception;
            }
            catch (OutOfMemoryException exception2)
            {
                throw exception2;
            }
            catch (ThreadAbortException exception3)
            {
                throw exception3;
            }
            catch (Exception)
            {
                str2 = "Message text unavailable.  Resource file 'Microsoft.VisualBasic resources' not found.";
            }
            return str2;
        }

        public static string GetResourceString(string ResourceKey, params string[] Args)
        {
            string str = null;
            string format = null;
            try
            {
                format = GetResourceString(ResourceKey);
                str = string.Format(Thread.CurrentThread.CurrentCulture, format, Args);
            }
            catch (StackOverflowException exception)
            {
                throw exception;
            }
            catch (OutOfMemoryException exception2)
            {
                throw exception2;
            }
            catch (ThreadAbortException exception3)
            {
                throw exception3;
            }
            catch (Exception)
            {
            }
            if (str != "")
            {
                return str;
            }
            return format;
        }

        internal static string GetResourceString(string ResourceKey, bool NotUsed)
        {
            string str2;
            if (VBAResourceManager == null)
            {
                return "Message text unavailable.  Resource file 'Microsoft.VisualBasic resources' not found.";
            }
            try
            {
                str2 = VBAResourceManager.GetString(ResourceKey, GetCultureInfo());
                if (str2 == null)
                {
                    str2 = VBAResourceManager.GetString(ResourceKey);
                }
            }
            catch (StackOverflowException exception)
            {
                throw exception;
            }
            catch (OutOfMemoryException exception2)
            {
                throw exception2;
            }
            catch (ThreadAbortException exception3)
            {
                throw exception3;
            }
            catch (Exception)
            {
                str2 = null;
            }
            return str2;
        }

        internal static bool IsHexOrOctValue(string Value, ref long i64Value)
        {
            int num;
            int length = Value.Length;
            while (num < length)
            {
                char ch = Value[num];
                if ((ch == '&') && ((num + 2) < length))
                {
                    ch = char.ToLower(Value[num + 1], CultureInfo.InvariantCulture);
                    string str = ToHalfwidthNumbers(Value.Substring(num + 2), GetCultureInfo());
                    switch (ch)
                    {
                        case 'h':
                            i64Value = Convert.ToInt64(str, 0x10);
                            goto Label_0087;

                        case 'o':
                            i64Value = Convert.ToInt64(str, 8);
                            goto Label_0087;
                    }
                    throw new FormatException();
                }
                if ((ch != ' ') && (ch != '　'))
                {
                    return false;
                }
                num++;
            }
            return false;
        Label_0087:
            return true;
        }

        internal static bool IsHexOrOctValue(string Value, ref ulong ui64Value)
        {
            int num;
            int length = Value.Length;
            while (num < length)
            {
                char ch = Value[num];
                if ((ch == '&') && ((num + 2) < length))
                {
                    ch = char.ToLower(Value[num + 1], CultureInfo.InvariantCulture);
                    string str = ToHalfwidthNumbers(Value.Substring(num + 2), GetCultureInfo());
                    switch (ch)
                    {
                        case 'h':
                            ui64Value = Convert.ToUInt64(str, 0x10);
                            goto Label_0087;

                        case 'o':
                            ui64Value = Convert.ToUInt64(str, 8);
                            goto Label_0087;
                    }
                    throw new FormatException();
                }
                if ((ch != ' ') && (ch != '　'))
                {
                    return false;
                }
                num++;
            }
            return false;
        Label_0087:
            return true;
        }

        internal static int MapHRESULT(int lNumber)
        {
            if (lNumber <= 0)
            {
                if ((lNumber & 0x1fff0000) == 0xa0000)
                {
                    return (lNumber & 0xffff);
                }
                switch (lNumber)
                {
                    case -2147467263:
                        return 0x8000;

                    case -2147467262:
                        return 430;

                    case -2147467260:
                        return 0x11f;

                    case -2147352575:
                        return 0x1b6;

                    case -2147352573:
                        return 0x1b6;

                    case -2147352572:
                        return 0x1c0;

                    case -2147352571:
                        return 13;

                    case -2147352570:
                        return 0x1b6;

                    case -2147352569:
                        return 0x1be;

                    case -2147352568:
                        return 0x1ca;

                    case -2147352566:
                        return 6;

                    case -2147352565:
                        return 9;

                    case -2147352564:
                        return 0x1bf;

                    case -2147352563:
                        return 10;

                    case -2147352562:
                        return 450;

                    case -2147352561:
                        return 0x1c1;

                    case -2147352559:
                        return 0x1c3;

                    case -2147352558:
                        return 11;

                    case -2147319786:
                        return 0x8016;

                    case -2147319785:
                        return 0x1cd;

                    case -2147319784:
                        return 0x8018;

                    case -2147319783:
                        return 0x8019;

                    case -2147319780:
                        return 0x801c;

                    case -2147319779:
                        return 0x801d;

                    case -2147319769:
                        return 0x8027;

                    case -2147319768:
                        return 0x8028;

                    case -2147319767:
                        return 0x8029;

                    case -2147319766:
                        return 0x802a;

                    case -2147319765:
                        return 0x802b;

                    case -2147319764:
                        return 0x802c;

                    case -2147319763:
                        return 0x802d;

                    case -2147319762:
                        return 0x802e;

                    case -2147319761:
                        return 0x1c5;

                    case -2147317571:
                        return 0x88bd;

                    case -2147317563:
                        return 0x88c5;

                    case -2147316576:
                        return 13;

                    case -2147316575:
                        return 9;

                    case -2147316574:
                        return 0x39;

                    case -2147316573:
                        return 0x142;

                    case -2147312566:
                        return 0x30;

                    case -2147312509:
                        return 0x9c83;

                    case -2147312508:
                        return 0x9c84;

                    case -2147287039:
                        return 0x8006;

                    case -2147287038:
                        return 0x35;

                    case -2147287037:
                        return 0x4c;

                    case -2147287036:
                        return 0x43;

                    case -2147287035:
                        return 70;

                    case -2147287034:
                        return 0x8004;

                    case -2147287032:
                        return 7;

                    case -2147287022:
                        return 0x43;

                    case -2147287021:
                        return 70;

                    case -2147287015:
                        return 0x8003;

                    case -2147287011:
                        return 0x8005;

                    case -2147287010:
                        return 0x8004;

                    case -2147287008:
                        return 0x4b;

                    case -2147287007:
                        return 70;

                    case -2147286960:
                        return 0x3a;

                    case -2147286928:
                        return 0x3d;

                    case -2147286789:
                        return 0x8018;

                    case -2147286788:
                        return 0x35;

                    case -2147286787:
                        return 0x8018;

                    case -2147286786:
                        return 0x8000;

                    case -2147286784:
                        return 70;

                    case -2147286783:
                        return 70;

                    case -2147286782:
                        return 0x8005;

                    case -2147286781:
                        return 0x39;

                    case -2147286780:
                        return 0x8019;

                    case -2147286779:
                        return 0x8019;

                    case -2147286778:
                        return 0x8015;

                    case -2147286777:
                        return 0x8019;

                    case -2147286776:
                        return 0x8019;

                    case -2147221230:
                        return 0x1ad;

                    case -2147221164:
                        return 0x1ad;

                    case -2147221021:
                        return 0x1ad;

                    case -2147221018:
                        return 0x1b0;

                    case -2147221014:
                        return 0x1b0;

                    case -2147221005:
                        return 0x1ad;

                    case -2147221003:
                        return 0x1ad;

                    case -2147220994:
                        return 0x1ad;

                    case -2147024891:
                        return 70;

                    case -2147024882:
                        return 7;

                    case -2147024809:
                        return 5;

                    case -2147023174:
                        return 0x1ce;

                    case -2146959355:
                        return 0x1ad;
                }
            }
            return lNumber;
        }

        internal static string MemberToString(MemberInfo Member)
        {
            switch (Member.MemberType)
            {
                case MemberTypes.Constructor:
                case MemberTypes.Method:
                    return MethodToString((MethodBase) Member);

                case MemberTypes.Field:
                    return FieldToString((FieldInfo) Member);

                case MemberTypes.Property:
                    return PropertyToString((PropertyInfo) Member);
            }
            return Member.Name;
        }

        public static string MethodToString(MethodBase Method)
        {
            bool flag;
            Type typ = null;
            string str = "";
            if (Method.MemberType == MemberTypes.Method)
            {
                typ = ((MethodInfo) Method).ReturnType;
            }
            if (Method.IsPublic)
            {
                str = str + "Public ";
            }
            else if (Method.IsPrivate)
            {
                str = str + "Private ";
            }
            else if (Method.IsAssembly)
            {
                str = str + "Friend ";
            }
            if ((Method.Attributes & MethodAttributes.Virtual) != MethodAttributes.PrivateScope)
            {
                if (!Method.DeclaringType.IsInterface)
                {
                    str = str + "Overrides ";
                }
            }
            else if (Symbols.IsShared(Method))
            {
                str = str + "Shared ";
            }
            Symbols.UserDefinedOperator uNDEF = Symbols.UserDefinedOperator.UNDEF;
            if (Symbols.IsUserDefinedOperator(Method))
            {
                uNDEF = Symbols.MapToUserDefinedOperator(Method);
            }
            if (uNDEF != Symbols.UserDefinedOperator.UNDEF)
            {
                if (uNDEF == Symbols.UserDefinedOperator.Narrow)
                {
                    str = str + "Narrowing ";
                }
                else if (uNDEF == Symbols.UserDefinedOperator.Widen)
                {
                    str = str + "Widening ";
                }
                str = str + "Operator ";
            }
            else if ((typ == null) || (typ == VoidType))
            {
                str = str + "Sub ";
            }
            else
            {
                str = str + "Function ";
            }
            if (uNDEF != Symbols.UserDefinedOperator.UNDEF)
            {
                str = str + Symbols.OperatorNames[(int) uNDEF];
            }
            else if (Method.MemberType == MemberTypes.Constructor)
            {
                str = str + "New";
            }
            else
            {
                str = str + Method.Name;
            }
            if (Symbols.IsGeneric(Method))
            {
                str = str + "(Of ";
                flag = true;
                foreach (Type type2 in Symbols.GetTypeParameters(Method))
                {
                    if (!flag)
                    {
                        str = str + ", ";
                    }
                    else
                    {
                        flag = false;
                    }
                    str = str + VBFriendlyNameOfType(type2, false);
                }
                str = str + ")";
            }
            str = str + "(";
            flag = true;
            foreach (ParameterInfo info in Method.GetParameters())
            {
                if (!flag)
                {
                    str = str + ", ";
                }
                else
                {
                    flag = false;
                }
                str = str + ParameterToString(info);
            }
            str = str + ")";
            if ((typ != null) && (typ != VoidType))
            {
                str = str + " As " + VBFriendlyNameOfType(typ, true);
            }
            return str;
        }

        internal static string OctFromLong(long Val)
        {
            bool flag;
            string expression = "";
            int num = Convert.ToInt32('0');
            if (Val < 0L)
            {
                Val = (0x7fffffffffffffffL + Val) + 1L;
                flag = true;
            }
            do
            {
                int num2 = (int) (Val % 8L);
                Val = Val >> 3;
                expression = expression + Conversions.ToString(Strings.ChrW(num2 + num));
            }
            while (Val > 0L);
            expression = Strings.StrReverse(expression);
            if (flag)
            {
                expression = "1" + expression;
            }
            return expression;
        }

        internal static string OctFromULong(ulong Val)
        {
            string expression = "";
            int num = Convert.ToInt32('0');
            do
            {
                int num2 = (int) (Val % ((ulong) 8L));
                Val = Val >> 3;
                expression = expression + Conversions.ToString(Strings.ChrW(num2 + num));
            }
            while (Val != 0L);
            return Strings.StrReverse(expression);
        }

        internal static string ParameterToString(ParameterInfo Parameter)
        {
            string str2 = "";
            Type parameterType = Parameter.ParameterType;
            if (Parameter.IsOptional)
            {
                str2 = str2 + "[";
            }
            if (parameterType.IsByRef)
            {
                str2 = str2 + "ByRef ";
                parameterType = parameterType.GetElementType();
            }
            else if (Symbols.IsParamArray(Parameter))
            {
                str2 = str2 + "ParamArray ";
            }
            str2 = str2 + Parameter.Name + " As " + VBFriendlyNameOfType(parameterType, true);
            if (!Parameter.IsOptional)
            {
                return str2;
            }
            object defaultValue = Parameter.DefaultValue;
            if (defaultValue == null)
            {
                str2 = str2 + " = Nothing";
            }
            else
            {
                Type type = defaultValue.GetType();
                if (type != VoidType)
                {
                    if (Symbols.IsEnum(type))
                    {
                        str2 = str2 + " = " + Enum.GetName(type, defaultValue);
                    }
                    else
                    {
                        str2 = str2 + " = " + Conversions.ToString(defaultValue);
                    }
                }
            }
            return (str2 + "]");
        }

        internal static string PropertyToString(PropertyInfo Prop)
        {
            ParameterInfo[] parameters;
            Type returnType;
            string str2 = "";
            PropertyKind readWrite = PropertyKind.ReadWrite;
            MethodInfo getMethod = Prop.GetGetMethod();
            if (getMethod != null)
            {
                if (Prop.GetSetMethod() != null)
                {
                    readWrite = PropertyKind.ReadWrite;
                }
                else
                {
                    readWrite = PropertyKind.ReadOnly;
                }
                parameters = getMethod.GetParameters();
                returnType = getMethod.ReturnType;
            }
            else
            {
                readWrite = PropertyKind.WriteOnly;
                getMethod = Prop.GetSetMethod();
                ParameterInfo[] sourceArray = getMethod.GetParameters();
                parameters = new ParameterInfo[(sourceArray.Length - 2) + 1];
                Array.Copy(sourceArray, parameters, parameters.Length);
                returnType = sourceArray[sourceArray.Length - 1].ParameterType;
            }
            str2 = str2 + "Public ";
            if ((getMethod.Attributes & MethodAttributes.Virtual) != MethodAttributes.PrivateScope)
            {
                if (!Prop.DeclaringType.IsInterface)
                {
                    str2 = str2 + "Overrides ";
                }
            }
            else if (Symbols.IsShared(getMethod))
            {
                str2 = str2 + "Shared ";
            }
            switch (readWrite)
            {
                case PropertyKind.ReadOnly:
                    str2 = str2 + "ReadOnly ";
                    break;

                case PropertyKind.WriteOnly:
                    str2 = str2 + "WriteOnly ";
                    break;
            }
            str2 = str2 + "Property " + Prop.Name + "(";
            bool flag = true;
            foreach (ParameterInfo info2 in parameters)
            {
                if (!flag)
                {
                    str2 = str2 + ", ";
                }
                else
                {
                    flag = false;
                }
                str2 = str2 + ParameterToString(info2);
            }
            return (str2 + ") As " + VBFriendlyNameOfType(returnType, true));
        }

        [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.SelfAffectingThreading)]
        public static object SetCultureInfo(CultureInfo Culture)
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = Culture;
            return currentCulture;
        }

        [DebuggerHidden, SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        internal static void SetDate(DateTime vDate)
        {
            NativeTypes.SystemTime systime = new NativeTypes.SystemTime();
            SafeNativeMethods.GetLocalTime(systime);
            systime.wYear = (short) vDate.Year;
            systime.wMonth = (short) vDate.Month;
            systime.wDay = (short) vDate.Day;
            if (Microsoft.VisualBasic.CompilerServices.UnsafeNativeMethods.SetLocalTime(systime) == 0)
            {
                if (Marshal.GetLastWin32Error() == 0x57)
                {
                    throw new ArgumentException(GetResourceString("Argument_InvalidValue"));
                }
                throw new SecurityException(GetResourceString("SetLocalDateFailure"));
            }
        }

        [SecuritySafeCritical, DebuggerHidden, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        internal static void SetTime(DateTime dtTime)
        {
            NativeTypes.SystemTime systime = new NativeTypes.SystemTime();
            SafeNativeMethods.GetLocalTime(systime);
            systime.wHour = (short) dtTime.Hour;
            systime.wMinute = (short) dtTime.Minute;
            systime.wSecond = (short) dtTime.Second;
            systime.wMilliseconds = (short) dtTime.Millisecond;
            if (Microsoft.VisualBasic.CompilerServices.UnsafeNativeMethods.SetLocalTime(systime) == 0)
            {
                if (Marshal.GetLastWin32Error() == 0x57)
                {
                    throw new ArgumentException(GetResourceString("Argument_InvalidValue"));
                }
                throw new SecurityException(GetResourceString("SetLocalTimeFailure"));
            }
        }

        internal static string StdFormat(string s)
        {
            char ch;
            char ch2;
            char ch3;
            NumberFormatInfo numberFormat = Thread.CurrentThread.CurrentCulture.NumberFormat;
            int index = s.IndexOf(numberFormat.NumberDecimalSeparator);
            if (index == -1)
            {
                return s;
            }
            try
            {
                ch = s[0];
                ch2 = s[1];
                ch3 = s[2];
            }
            catch (StackOverflowException exception)
            {
                throw exception;
            }
            catch (OutOfMemoryException exception2)
            {
                throw exception2;
            }
            catch (ThreadAbortException exception3)
            {
                throw exception3;
            }
            catch (Exception)
            {
            }
            if (s[index] == '.')
            {
                if ((ch == '0') && (ch2 == '.'))
                {
                    return s.Substring(1);
                }
                if (((ch != '-') && (ch != '+')) && (ch != ' '))
                {
                    return s;
                }
                if ((ch2 != '0') || (ch3 != '.'))
                {
                    return s;
                }
            }
            StringBuilder builder = new StringBuilder(s);
            builder[index] = '.';
            if ((ch == '0') && (ch2 == '.'))
            {
                return builder.ToString(1, builder.Length - 1);
            }
            if ((((ch == '-') || (ch == '+')) || (ch == ' ')) && ((ch2 == '0') && (ch3 == '.')))
            {
                builder.Remove(1, 1);
                return builder.ToString();
            }
            return builder.ToString();
        }

        public static void ThrowException(int hr)
        {
            throw ExceptionUtils.VbMakeException(hr);
        }

        internal static string ToHalfwidthNumbers(string s, CultureInfo culture)
        {
            int num = culture.LCID & 0x3ff;
            if (((num != 4) && (num != 0x11)) && (num != 0x12))
            {
                return s;
            }
            return Strings.vbLCMapString(culture, 0x400000, s);
        }

        internal static string VBFriendlyName(object Obj)
        {
            if (Obj == null)
            {
                return "Nothing";
            }
            return VBFriendlyName(Obj.GetType(), Obj);
        }

        internal static string VBFriendlyName(Type typ)
        {
            return VBFriendlyNameOfType(typ, false);
        }

        internal static string VBFriendlyName(Type typ, object o)
        {
            if (typ.IsCOMObject && (typ.FullName == "System.__ComObject"))
            {
                return Information.TypeNameOfCOMObject(o, false);
            }
            return VBFriendlyNameOfType(typ, false);
        }

        internal static string VBFriendlyNameOfType(Type typ, bool FullName = false)
        {
            string name;
            TypeCode typeCode;
            string arraySuffixAndElementType = GetArraySuffixAndElementType(ref typ);
            if (typ.IsEnum)
            {
                typeCode = TypeCode.Object;
            }
            else
            {
                typeCode = Type.GetTypeCode(typ);
            }
            switch (typeCode)
            {
                case TypeCode.DBNull:
                    name = "DBNull";
                    break;

                case TypeCode.Boolean:
                    name = "Boolean";
                    break;

                case TypeCode.Char:
                    name = "Char";
                    break;

                case TypeCode.SByte:
                    name = "SByte";
                    break;

                case TypeCode.Byte:
                    name = "Byte";
                    break;

                case TypeCode.Int16:
                    name = "Short";
                    break;

                case TypeCode.UInt16:
                    name = "UShort";
                    break;

                case TypeCode.Int32:
                    name = "Integer";
                    break;

                case TypeCode.UInt32:
                    name = "UInteger";
                    break;

                case TypeCode.Int64:
                    name = "Long";
                    break;

                case TypeCode.UInt64:
                    name = "ULong";
                    break;

                case TypeCode.Single:
                    name = "Single";
                    break;

                case TypeCode.Double:
                    name = "Double";
                    break;

                case TypeCode.Decimal:
                    name = "Decimal";
                    break;

                case TypeCode.DateTime:
                    name = "Date";
                    break;

                case TypeCode.String:
                    name = "String";
                    break;

                default:
                    if (Symbols.IsGenericParameter(typ))
                    {
                        name = typ.Name;
                    }
                    else
                    {
                        string fullName;
                        string str6 = null;
                        string genericArgsSuffix = GetGenericArgsSuffix(typ);
                        if (FullName)
                        {
                            if (typ.IsNested)
                            {
                                str6 = VBFriendlyNameOfType(typ.DeclaringType, true);
                                fullName = typ.Name;
                            }
                            else
                            {
                                fullName = typ.FullName;
                            }
                        }
                        else
                        {
                            fullName = typ.Name;
                        }
                        if (genericArgsSuffix != null)
                        {
                            int length = fullName.LastIndexOf('`');
                            if (length != -1)
                            {
                                fullName = fullName.Substring(0, length);
                            }
                            name = fullName + genericArgsSuffix;
                        }
                        else
                        {
                            name = fullName;
                        }
                        if (str6 != null)
                        {
                            name = str6 + "." + name;
                        }
                    }
                    break;
            }
            if (arraySuffixAndElementType != null)
            {
                name = name + arraySuffixAndElementType;
            }
            return name;
        }

        internal static ResourceManager VBAResourceManager
        {
            get
            {
                if (m_VBAResourceManager == null)
                {
                    object resourceManagerSyncObj = ResourceManagerSyncObj;
                    ObjectFlowControl.CheckForSyncLockOnValueType(resourceManagerSyncObj);
                    lock (resourceManagerSyncObj)
                    {
                        if (!m_TriedLoadingResourceManager)
                        {
                            try
                            {
                                m_VBAResourceManager = new ResourceManager("Microsoft.VisualBasic", Assembly.GetExecutingAssembly());
                            }
                            catch (StackOverflowException exception)
                            {
                                throw exception;
                            }
                            catch (OutOfMemoryException exception2)
                            {
                                throw exception2;
                            }
                            catch (ThreadAbortException exception3)
                            {
                                throw exception3;
                            }
                            catch (Exception)
                            {
                            }
                            m_TriedLoadingResourceManager = true;
                        }
                    }
                }
                return m_VBAResourceManager;
            }
        }

        internal static Assembly VBRuntimeAssembly
        {
            get
            {
                if (m_VBRuntimeAssembly == null)
                {
                    m_VBRuntimeAssembly = Assembly.GetExecutingAssembly();
                }
                return m_VBRuntimeAssembly;
            }
        }

        private enum PropertyKind
        {
            ReadWrite,
            ReadOnly,
            WriteOnly
        }
    }
}

