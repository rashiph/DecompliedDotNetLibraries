namespace System.Web.UI
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Web;

    public static class PropertyConverter
    {
        private static readonly Type[] s_parseMethodTypes = new Type[] { typeof(string) };
        private static readonly Type[] s_parseMethodTypesWithSOP = new Type[] { typeof(string), typeof(IServiceProvider) };

        public static object EnumFromString(Type enumType, string value)
        {
            try
            {
                return Enum.Parse(enumType, value, true);
            }
            catch
            {
                return null;
            }
        }

        public static string EnumToString(Type enumType, object enumValue)
        {
            return Enum.Format(enumType, enumValue, "G").Replace('_', '-');
        }

        public static object ObjectFromString(Type objType, MemberInfo propertyInfo, string value)
        {
            if (value == null)
            {
                return null;
            }
            if (objType.Equals(typeof(bool)) && (value.Length == 0))
            {
                return null;
            }
            bool flag = true;
            object obj2 = null;
            try
            {
                if (objType.IsEnum)
                {
                    flag = false;
                    obj2 = EnumFromString(objType, value);
                }
                else if (objType.Equals(typeof(string)))
                {
                    flag = false;
                    obj2 = value;
                }
                else
                {
                    PropertyDescriptor descriptor = null;
                    if (propertyInfo != null)
                    {
                        descriptor = TypeDescriptor.GetProperties(propertyInfo.ReflectedType)[propertyInfo.Name];
                    }
                    if (descriptor != null)
                    {
                        TypeConverter converter = descriptor.Converter;
                        if ((converter != null) && converter.CanConvertFrom(typeof(string)))
                        {
                            flag = false;
                            obj2 = converter.ConvertFromInvariantString(value);
                        }
                    }
                }
            }
            catch
            {
            }
            if (flag)
            {
                MethodInfo method = objType.GetMethod("Parse", s_parseMethodTypesWithSOP);
                if (method != null)
                {
                    object[] parameters = new object[] { value, CultureInfo.InvariantCulture };
                    try
                    {
                        obj2 = Util.InvokeMethod(method, null, parameters);
                        goto Label_011F;
                    }
                    catch
                    {
                        goto Label_011F;
                    }
                }
                method = objType.GetMethod("Parse", s_parseMethodTypes);
                if (method != null)
                {
                    object[] objArray2 = new object[] { value };
                    try
                    {
                        obj2 = Util.InvokeMethod(method, null, objArray2);
                    }
                    catch
                    {
                    }
                }
            }
        Label_011F:
            if (obj2 == null)
            {
                throw new HttpException(System.Web.SR.GetString("Type_not_creatable_from_string", new object[] { objType.FullName, value, propertyInfo.Name }));
            }
            return obj2;
        }
    }
}

