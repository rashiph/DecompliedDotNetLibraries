namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.VisualBasic;
    using System;
    using System.ComponentModel;
    using System.Dynamic;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class Versioned
    {
        private Versioned()
        {
        }

        public static object CallByName(object Instance, string MethodName, CallType UseCallType, params object[] Arguments)
        {
            switch (UseCallType)
            {
                case CallType.Method:
                    return NewLateBinding.LateCall(Instance, null, MethodName, Arguments, null, null, null, false);

                case CallType.Get:
                    return NewLateBinding.LateGet(Instance, null, MethodName, Arguments, null, null, null);

                case CallType.Let:
                case CallType.Set:
                {
                    IDynamicMetaObjectProvider instance = IDOUtils.TryCastToIDMOP(Instance);
                    if (instance == null)
                    {
                        NewLateBinding.LateSet(Instance, null, MethodName, Arguments, null, null, false, false, UseCallType);
                        break;
                    }
                    IDOBinder.IDOSet(instance, MethodName, null, Arguments);
                    break;
                }
                default:
                    throw new ArgumentException(Utils.GetResourceString("Argument_InvalidValue1", new string[] { "CallType" }));
            }
            return null;
        }

        public static bool IsNumeric(object Expression)
        {
            IConvertible convertible = Expression as IConvertible;
            if (convertible != null)
            {
                switch (convertible.GetTypeCode())
                {
                    case TypeCode.Boolean:
                        return true;

                    case TypeCode.Char:
                    case TypeCode.String:
                    {
                        double num;
                        string str = convertible.ToString(null);
                        try
                        {
                            long num2;
                            if (Utils.IsHexOrOctValue(str, ref num2))
                            {
                                return true;
                            }
                        }
                        catch (FormatException)
                        {
                            return false;
                        }
                        return Conversions.TryParseDouble(str, ref num);
                    }
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                        return true;
                }
            }
            return false;
        }

        public static string SystemTypeName(string VbName)
        {
            switch (Strings.Trim(VbName).ToUpperInvariant())
            {
                case "BOOLEAN":
                    return "System.Boolean";

                case "SBYTE":
                    return "System.SByte";

                case "BYTE":
                    return "System.Byte";

                case "SHORT":
                    return "System.Int16";

                case "USHORT":
                    return "System.UInt16";

                case "INTEGER":
                    return "System.Int32";

                case "UINTEGER":
                    return "System.UInt32";

                case "LONG":
                    return "System.Int64";

                case "ULONG":
                    return "System.UInt64";

                case "DECIMAL":
                    return "System.Decimal";

                case "SINGLE":
                    return "System.Single";

                case "DOUBLE":
                    return "System.Double";

                case "DATE":
                    return "System.DateTime";

                case "CHAR":
                    return "System.Char";

                case "STRING":
                    return "System.String";

                case "OBJECT":
                    return "System.Object";
            }
            return null;
        }

        public static string TypeName(object Expression)
        {
            if (Expression == null)
            {
                return "Nothing";
            }
            Type typ = Expression.GetType();
            if (typ.IsCOMObject && (string.CompareOrdinal(typ.Name, "__ComObject") == 0))
            {
                return Information.TypeNameOfCOMObject(Expression, true);
            }
            return Utils.VBFriendlyNameOfType(typ, false);
        }

        public static string VbTypeName(string SystemName)
        {
            SystemName = Strings.Trim(SystemName).ToUpperInvariant();
            if (Strings.Left(SystemName, 7) == "SYSTEM.")
            {
                SystemName = Strings.Mid(SystemName, 8);
            }
            switch (SystemName)
            {
                case "BOOLEAN":
                    return "Boolean";

                case "SBYTE":
                    return "SByte";

                case "BYTE":
                    return "Byte";

                case "INT16":
                    return "Short";

                case "UINT16":
                    return "UShort";

                case "INT32":
                    return "Integer";

                case "UINT32":
                    return "UInteger";

                case "INT64":
                    return "Long";

                case "UINT64":
                    return "ULong";

                case "DECIMAL":
                    return "Decimal";

                case "SINGLE":
                    return "Single";

                case "DOUBLE":
                    return "Double";

                case "DATETIME":
                    return "Date";

                case "CHAR":
                    return "Char";

                case "STRING":
                    return "String";

                case "OBJECT":
                    return "Object";
            }
            return null;
        }
    }
}

