namespace System.Reflection
{
    using System;
    using System.Security;

    internal static class MdConstant
    {
        [SecurityCritical]
        public static unsafe object GetValue(MetadataImport scope, int token, RuntimeTypeHandle fieldTypeHandle, bool raw)
        {
            int num2;
            long num4;
            CorElementType end = CorElementType.End;
            long num = 0L;
            string str = scope.GetDefaultValue(token, out num, out num2, out end);
            RuntimeType runtimeType = fieldTypeHandle.GetRuntimeType();
            if (!runtimeType.IsEnum || raw)
            {
                if (runtimeType == typeof(DateTime))
                {
                    num4 = 0L;
                    switch (end)
                    {
                        case CorElementType.I8:
                            num4 = num;
                            goto Label_0129;

                        case CorElementType.U8:
                            num4 = num;
                            goto Label_0129;

                        case CorElementType.Void:
                            return DBNull.Value;
                    }
                    throw new FormatException(Environment.GetResourceString("Arg_BadLiteralFormat"));
                }
                switch (end)
                {
                    case CorElementType.Void:
                        return DBNull.Value;

                    case CorElementType.Boolean:
                        return (*(((int*) &num)) != 0);

                    case CorElementType.Char:
                        return (char) *(((ushort*) &num));

                    case CorElementType.I1:
                        return *(((sbyte*) &num));

                    case CorElementType.U1:
                        return *(((byte*) &num));

                    case CorElementType.I2:
                        return *(((short*) &num));

                    case CorElementType.U2:
                        return *(((ushort*) &num));

                    case CorElementType.I4:
                        return *(((int*) &num));

                    case CorElementType.U4:
                        return *(((uint*) &num));

                    case CorElementType.I8:
                        return num;

                    case CorElementType.U8:
                        return (ulong) num;

                    case CorElementType.R4:
                        return *(((float*) &num));

                    case CorElementType.R8:
                        return *(((double*) &num));

                    case CorElementType.String:
                        if (str == null)
                        {
                            return string.Empty;
                        }
                        return str;

                    case CorElementType.Class:
                        return null;
                }
                throw new FormatException(Environment.GetResourceString("Arg_BadLiteralFormat"));
            }
            long num3 = 0L;
            switch (end)
            {
                case CorElementType.Void:
                    return DBNull.Value;

                case CorElementType.Char:
                    num3 = *((ushort*) &num);
                    break;

                case CorElementType.I1:
                    num3 = *((sbyte*) &num);
                    break;

                case CorElementType.U1:
                    num3 = *((byte*) &num);
                    break;

                case CorElementType.I2:
                    num3 = *((short*) &num);
                    break;

                case CorElementType.U2:
                    num3 = *((ushort*) &num);
                    break;

                case CorElementType.I4:
                    num3 = *((int*) &num);
                    break;

                case CorElementType.U4:
                    num3 = *((uint*) &num);
                    break;

                case CorElementType.I8:
                    num3 = num;
                    break;

                case CorElementType.U8:
                    num3 = num;
                    break;

                default:
                    throw new FormatException(Environment.GetResourceString("Arg_BadLiteralFormat"));
            }
            return RuntimeType.CreateEnum(runtimeType, num3);
        Label_0129:
            return new DateTime(num4);
        }
    }
}

