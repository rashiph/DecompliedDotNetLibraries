namespace System.Web.Services.Protocols
{
    using System;
    using System.Globalization;
    using System.Threading;
    using System.Web.Services;

    internal class ScalarFormatter
    {
        private ScalarFormatter()
        {
        }

        private static object EnumFromString(string value, Type type)
        {
            return Enum.Parse(type, value);
        }

        private static string EnumToString(object value)
        {
            return Enum.Format(value.GetType(), value, "G");
        }

        internal static object FromString(string value, Type type)
        {
            object obj2;
            try
            {
                if (type == typeof(string))
                {
                    return value;
                }
                if (type.IsEnum)
                {
                    return EnumFromString(value, type);
                }
                obj2 = Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                throw new ArgumentException(Res.GetString("WebChangeTypeFailed", new object[] { value, type.FullName }), "type", exception);
            }
            return obj2;
        }

        internal static bool IsTypeSupported(Type type)
        {
            if (!type.IsEnum && ((((!(type == typeof(int)) && !(type == typeof(string))) && (!(type == typeof(long)) && !(type == typeof(byte)))) && ((!(type == typeof(sbyte)) && !(type == typeof(short))) && (!(type == typeof(bool)) && !(type == typeof(char))))) && (((!(type == typeof(float)) && !(type == typeof(decimal))) && (!(type == typeof(DateTime)) && !(type == typeof(ushort)))) && (!(type == typeof(uint)) && !(type == typeof(ulong))))))
            {
                return (type == typeof(double));
            }
            return true;
        }

        internal static string ToString(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            if (value is string)
            {
                return (string) value;
            }
            if (value.GetType().IsEnum)
            {
                return EnumToString(value);
            }
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }
    }
}

