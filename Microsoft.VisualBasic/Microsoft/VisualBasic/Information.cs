namespace Microsoft.VisualBasic
{
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [StandardModule]
    public sealed class Information
    {
        internal const string COMObjectName = "__ComObject";
        private static readonly int[] QBColorTable = new int[] { 0, 0x800000, 0x8000, 0x808000, 0x80, 0x800080, 0x8080, 0xc0c0c0, 0x808080, 0xff0000, 0xff00, 0xffff00, 0xff, 0xff00ff, 0xffff, 0xffffff };

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static int Erl()
        {
            return ProjectData.GetProjectData().m_Err.Erl;
        }

        public static ErrObject Err()
        {
            ProjectData projectData = ProjectData.GetProjectData();
            if (projectData.m_Err == null)
            {
                projectData.m_Err = new ErrObject();
            }
            return projectData.m_Err;
        }

        public static bool IsArray(object VarName)
        {
            if (VarName == null)
            {
                return false;
            }
            return (VarName is Array);
        }

        public static bool IsDate(object Expression)
        {
            DateTime time;
            if (Expression == null)
            {
                return false;
            }
            if (Expression is DateTime)
            {
                return true;
            }
            string str = Expression as string;
            return ((str != null) && Conversions.TryParseDate(str, ref time));
        }

        public static bool IsDBNull(object Expression)
        {
            if (Expression == null)
            {
                return false;
            }
            return (Expression is DBNull);
        }

        public static bool IsError(object Expression)
        {
            if (Expression == null)
            {
                return false;
            }
            return (Expression is Exception);
        }

        public static bool IsNothing(object Expression)
        {
            return (Expression == null);
        }

        public static bool IsNumeric(object Expression)
        {
            double num;
            IConvertible convertible = Expression as IConvertible;
            if (convertible == null)
            {
                char[] chArray = Expression as char[];
                if (chArray == null)
                {
                    return false;
                }
                Expression = new string(chArray);
            }
            TypeCode typeCode = convertible.GetTypeCode();
            if ((typeCode != TypeCode.String) && (typeCode != TypeCode.Char))
            {
                return IsOldNumericTypeCode(typeCode);
            }
            string str = convertible.ToString(null);
            try
            {
                long num2;
                if (Utils.IsHexOrOctValue(str, ref num2))
                {
                    return true;
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
                return false;
            }
            return DoubleType.TryParse(str, ref num);
        }

        internal static bool IsOldNumericTypeCode(TypeCode TypCode)
        {
            switch (TypCode)
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
            }
            return false;
        }

        public static bool IsReference(object Expression)
        {
            return !(Expression is ValueType);
        }

        public static int LBound(Array Array, int Rank = 1)
        {
            if (Array == null)
            {
                throw ExceptionUtils.VbMakeException(new ArgumentNullException(Utils.GetResourceString("Argument_InvalidNullValue1", new string[] { "Array" })), 9);
            }
            if ((Rank < 1) || (Rank > Array.Rank))
            {
                throw new RankException(Utils.GetResourceString("Argument_InvalidRank1", new string[] { "Rank" }));
            }
            return Array.GetLowerBound(Rank - 1);
        }

        [SecuritySafeCritical]
        internal static string LegacyTypeNameOfCOMObject(object VarName, bool bThrowException)
        {
            int num;
            string str5 = "__ComObject";
            try
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
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
            catch (Exception exception4)
            {
                if (bThrowException)
                {
                    throw exception4;
                }
                goto Label_0072;
            }
            Microsoft.VisualBasic.CompilerServices.UnsafeNativeMethods.ITypeInfo pTypeInfo = null;
            string pBstrName = null;
            string pBstrDocString = null;
            string pBstrHelpFile = null;
            Microsoft.VisualBasic.CompilerServices.UnsafeNativeMethods.IDispatch dispatch = VarName as Microsoft.VisualBasic.CompilerServices.UnsafeNativeMethods.IDispatch;
            if (((dispatch != null) && (dispatch.GetTypeInfo(0, 0x409, out pTypeInfo) >= 0)) && (pTypeInfo.GetDocumentation(-1, out pBstrName, out pBstrDocString, out num, out pBstrHelpFile) >= 0))
            {
                str5 = pBstrName;
            }
        Label_0072:
            if (str5[0] == '_')
            {
                str5 = str5.Substring(1);
            }
            return str5;
        }

        internal static string OldVBFriendlyNameOfTypeName(string typename)
        {
            string sRank = null;
            int num = typename.Length - 1;
            if (typename[num] == ']')
            {
                int index = typename.IndexOf('[');
                if ((index + 1) == num)
                {
                    sRank = "()";
                }
                else
                {
                    sRank = typename.Substring(index, (num - index) + 1).Replace('[', '(').Replace(']', ')');
                }
                typename = typename.Substring(0, index);
            }
            string str2 = OldVbTypeName(typename);
            if (str2 == null)
            {
                str2 = typename;
            }
            if (sRank == null)
            {
                return str2;
            }
            return (str2 + Utils.AdjustArraySuffix(sRank));
        }

        internal static string OldVbTypeName(string UrtName)
        {
            UrtName = Strings.Trim(UrtName).ToUpperInvariant();
            if (Strings.Left(UrtName, 7) == "SYSTEM.")
            {
                UrtName = Strings.Mid(UrtName, 8);
            }
            switch (UrtName)
            {
                case "OBJECT":
                    return "Object";

                case "INT16":
                    return "Short";

                case "INT32":
                    return "Integer";

                case "SINGLE":
                    return "Single";

                case "DOUBLE":
                    return "Double";

                case "DATETIME":
                    return "Date";

                case "STRING":
                    return "String";

                case "BOOLEAN":
                    return "Boolean";

                case "DECIMAL":
                    return "Decimal";

                case "BYTE":
                    return "Byte";

                case "CHAR":
                    return "Char";

                case "INT64":
                    return "Long";
            }
            return null;
        }

        public static int QBColor(int Color)
        {
            if ((Color & 0xfff0) != 0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Color" }));
            }
            return QBColorTable[Color];
        }

        public static int RGB(int Red, int Green, int Blue)
        {
            if ((Red & -2147483648) != 0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Red" }));
            }
            if ((Green & -2147483648) != 0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Green" }));
            }
            if ((Blue & -2147483648) != 0)
            {
                throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "Blue" }));
            }
            if (Red > 0xff)
            {
                Red = 0xff;
            }
            if (Green > 0xff)
            {
                Green = 0xff;
            }
            if (Blue > 0xff)
            {
                Blue = 0xff;
            }
            return (((Blue * 0x10000) + (Green * 0x100)) + Red);
        }

        public static string SystemTypeName(string VbName)
        {
            switch (Strings.Trim(VbName).ToUpperInvariant())
            {
                case "OBJECT":
                    return "System.Object";

                case "SHORT":
                    return "System.Int16";

                case "INTEGER":
                    return "System.Int32";

                case "SINGLE":
                    return "System.Single";

                case "DOUBLE":
                    return "System.Double";

                case "DATE":
                    return "System.DateTime";

                case "STRING":
                    return "System.String";

                case "BOOLEAN":
                    return "System.Boolean";

                case "DECIMAL":
                    return "System.Decimal";

                case "BYTE":
                    return "System.Byte";

                case "CHAR":
                    return "System.Char";

                case "LONG":
                    return "System.Int64";
            }
            return null;
        }

        public static string TypeName(object VarName)
        {
            bool flag;
            string name;
            if (VarName == null)
            {
                return "Nothing";
            }
            Type type = VarName.GetType();
            if (type.IsArray)
            {
                flag = true;
                type = type.GetElementType();
            }
            if (type.IsEnum)
            {
                name = type.Name;
            }
            else
            {
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.DBNull:
                        name = "DBNull";
                        goto Label_0136;

                    case TypeCode.Boolean:
                        name = "Boolean";
                        goto Label_0136;

                    case TypeCode.Char:
                        name = "Char";
                        goto Label_0136;

                    case TypeCode.Byte:
                        name = "Byte";
                        goto Label_0136;

                    case TypeCode.Int16:
                        name = "Short";
                        goto Label_0136;

                    case TypeCode.Int32:
                        name = "Integer";
                        goto Label_0136;

                    case TypeCode.Int64:
                        name = "Long";
                        goto Label_0136;

                    case TypeCode.Single:
                        name = "Single";
                        goto Label_0136;

                    case TypeCode.Double:
                        name = "Double";
                        goto Label_0136;

                    case TypeCode.Decimal:
                        name = "Decimal";
                        goto Label_0136;

                    case TypeCode.DateTime:
                        name = "Date";
                        goto Label_0136;

                    case TypeCode.String:
                        name = "String";
                        goto Label_0136;
                }
                name = type.Name;
                if (type.IsCOMObject && (string.CompareOrdinal(name, "__ComObject") == 0))
                {
                    name = LegacyTypeNameOfCOMObject(VarName, true);
                }
            }
            int index = name.IndexOf('+');
            if (index >= 0)
            {
                name = name.Substring(index + 1);
            }
        Label_0136:
            if (!flag)
            {
                return name;
            }
            Array array = (Array) VarName;
            if (array.Rank == 1)
            {
                name = name + "[]";
            }
            else
            {
                name = name + "[" + new string(',', array.Rank - 1) + "]";
            }
            return OldVBFriendlyNameOfTypeName(name);
        }

        [SecuritySafeCritical]
        internal static string TypeNameOfCOMObject(object VarName, bool bThrowException)
        {
            int num;
            string str4 = "__ComObject";
            try
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
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
            catch (Exception exception4)
            {
                if (bThrowException)
                {
                    throw exception4;
                }
                goto Label_00BD;
            }
            Microsoft.VisualBasic.CompilerServices.UnsafeNativeMethods.ITypeInfo pTypeInfo = null;
            string pBstrName = null;
            string pBstrDocString = null;
            string pBstrHelpFile = null;
            Microsoft.VisualBasic.CompilerServices.UnsafeNativeMethods.IProvideClassInfo info2 = VarName as Microsoft.VisualBasic.CompilerServices.UnsafeNativeMethods.IProvideClassInfo;
            if (info2 != null)
            {
                try
                {
                    pTypeInfo = info2.GetClassInfo();
                    if (pTypeInfo.GetDocumentation(-1, out pBstrName, out pBstrDocString, out num, out pBstrHelpFile) >= 0)
                    {
                        str4 = pBstrName;
                        goto Label_00BD;
                    }
                    pTypeInfo = null;
                }
                catch (StackOverflowException exception5)
                {
                    throw exception5;
                }
                catch (OutOfMemoryException exception6)
                {
                    throw exception6;
                }
                catch (ThreadAbortException exception7)
                {
                    throw exception7;
                }
                catch (Exception)
                {
                }
            }
            Microsoft.VisualBasic.CompilerServices.UnsafeNativeMethods.IDispatch dispatch = VarName as Microsoft.VisualBasic.CompilerServices.UnsafeNativeMethods.IDispatch;
            if (((dispatch != null) && (dispatch.GetTypeInfo(0, 0x409, out pTypeInfo) >= 0)) && (pTypeInfo.GetDocumentation(-1, out pBstrName, out pBstrDocString, out num, out pBstrHelpFile) >= 0))
            {
                str4 = pBstrName;
            }
        Label_00BD:
            if (str4[0] == '_')
            {
                str4 = str4.Substring(1);
            }
            return str4;
        }

        public static int UBound(Array Array, int Rank = 1)
        {
            if (Array == null)
            {
                throw ExceptionUtils.VbMakeException(new ArgumentNullException(Utils.GetResourceString("Argument_InvalidNullValue1", new string[] { "Array" })), 9);
            }
            if ((Rank < 1) || (Rank > Array.Rank))
            {
                throw new RankException(Utils.GetResourceString("Argument_InvalidRank1", new string[] { "Rank" }));
            }
            return Array.GetUpperBound(Rank - 1);
        }

        public static VariantType VarType(object VarName)
        {
            if (VarName == null)
            {
                return VariantType.Object;
            }
            return VarTypeFromComType(VarName.GetType());
        }

        internal static VariantType VarTypeFromComType(Type typ)
        {
            if (typ != null)
            {
                if (typ.IsArray)
                {
                    typ = typ.GetElementType();
                    if (typ.IsArray)
                    {
                        return (VariantType.Array | VariantType.Object);
                    }
                    VariantType type2 = VarTypeFromComType(typ);
                    if ((type2 & VariantType.Array) != VariantType.Empty)
                    {
                        return (VariantType.Array | VariantType.Object);
                    }
                    return (type2 | VariantType.Array);
                }
                if (typ.IsEnum)
                {
                    typ = Enum.GetUnderlyingType(typ);
                }
                if (typ == null)
                {
                    return VariantType.Empty;
                }
                switch (Type.GetTypeCode(typ))
                {
                    case TypeCode.DBNull:
                        return VariantType.Null;

                    case TypeCode.Boolean:
                        return VariantType.Boolean;

                    case TypeCode.Char:
                        return VariantType.Char;

                    case TypeCode.Byte:
                        return VariantType.Byte;

                    case TypeCode.Int16:
                        return VariantType.Short;

                    case TypeCode.Int32:
                        return VariantType.Integer;

                    case TypeCode.Int64:
                        return VariantType.Long;

                    case TypeCode.Single:
                        return VariantType.Single;

                    case TypeCode.Double:
                        return VariantType.Double;

                    case TypeCode.Decimal:
                        return VariantType.Decimal;

                    case TypeCode.DateTime:
                        return VariantType.Date;

                    case TypeCode.String:
                        return VariantType.String;
                }
                if (((typ == typeof(Missing)) || (typ == typeof(Exception))) || typ.IsSubclassOf(typeof(Exception)))
                {
                    return VariantType.Error;
                }
                if (typ.IsValueType)
                {
                    return VariantType.UserDefinedType;
                }
            }
            return VariantType.Object;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static string VbTypeName(string UrtName)
        {
            return OldVbTypeName(UrtName);
        }
    }
}

