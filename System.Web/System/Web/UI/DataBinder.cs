namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Web;

    public sealed class DataBinder
    {
        private static bool enableCaching = true;
        private static readonly char[] expressionPartSeparator = new char[] { '.' };
        private static readonly char[] indexExprEndChars = new char[] { ']', ')' };
        private static readonly char[] indexExprStartChars = new char[] { '[', '(' };
        private static readonly ConcurrentDictionary<Type, PropertyDescriptorCollection> propertyCache = new ConcurrentDictionary<Type, PropertyDescriptorCollection>();

        public static object Eval(object container, string expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            expression = expression.Trim();
            if (expression.Length == 0)
            {
                throw new ArgumentNullException("expression");
            }
            if (container == null)
            {
                return null;
            }
            string[] expressionParts = expression.Split(expressionPartSeparator);
            return Eval(container, expressionParts);
        }

        private static object Eval(object container, string[] expressionParts)
        {
            object propertyValue = container;
            for (int i = 0; (i < expressionParts.Length) && (propertyValue != null); i++)
            {
                string propName = expressionParts[i];
                if (propName.IndexOfAny(indexExprStartChars) < 0)
                {
                    propertyValue = GetPropertyValue(propertyValue, propName);
                }
                else
                {
                    propertyValue = GetIndexedPropertyValue(propertyValue, propName);
                }
            }
            return propertyValue;
        }

        public static string Eval(object container, string expression, string format)
        {
            object obj2 = Eval(container, expression);
            if ((obj2 == null) || (obj2 == DBNull.Value))
            {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(format))
            {
                return obj2.ToString();
            }
            return string.Format(format, obj2);
        }

        public static object GetDataItem(object container)
        {
            bool flag;
            return GetDataItem(container, out flag);
        }

        public static object GetDataItem(object container, out bool foundDataItem)
        {
            if (container == null)
            {
                foundDataItem = false;
                return null;
            }
            IDataItemContainer container2 = container as IDataItemContainer;
            if (container2 != null)
            {
                foundDataItem = true;
                return container2.DataItem;
            }
            string name = "DataItem";
            PropertyInfo property = container.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property == null)
            {
                foundDataItem = false;
                return null;
            }
            foundDataItem = true;
            return property.GetValue(container, null);
        }

        public static object GetIndexedPropertyValue(object container, string expr)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            if (string.IsNullOrEmpty(expr))
            {
                throw new ArgumentNullException("expr");
            }
            object obj2 = null;
            bool flag = false;
            int length = expr.IndexOfAny(indexExprStartChars);
            int num2 = expr.IndexOfAny(indexExprEndChars, length + 1);
            if (((length < 0) || (num2 < 0)) || (num2 == (length + 1)))
            {
                throw new ArgumentException(System.Web.SR.GetString("DataBinder_Invalid_Indexed_Expr", new object[] { expr }));
            }
            string propName = null;
            object obj3 = null;
            string s = expr.Substring(length + 1, (num2 - length) - 1).Trim();
            if (length != 0)
            {
                propName = expr.Substring(0, length);
            }
            if (s.Length != 0)
            {
                if (((s[0] == '"') && (s[s.Length - 1] == '"')) || ((s[0] == '\'') && (s[s.Length - 1] == '\'')))
                {
                    obj3 = s.Substring(1, s.Length - 2);
                }
                else if (char.IsDigit(s[0]))
                {
                    int num3;
                    flag = int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out num3);
                    if (flag)
                    {
                        obj3 = num3;
                    }
                    else
                    {
                        obj3 = s;
                    }
                }
                else
                {
                    obj3 = s;
                }
            }
            if (obj3 == null)
            {
                throw new ArgumentException(System.Web.SR.GetString("DataBinder_Invalid_Indexed_Expr", new object[] { expr }));
            }
            object propertyValue = null;
            if ((propName != null) && (propName.Length != 0))
            {
                propertyValue = GetPropertyValue(container, propName);
            }
            else
            {
                propertyValue = container;
            }
            if (propertyValue == null)
            {
                return obj2;
            }
            Array array = propertyValue as Array;
            if ((array != null) && flag)
            {
                return array.GetValue((int) obj3);
            }
            if ((propertyValue is IList) && flag)
            {
                return ((IList) propertyValue)[(int) obj3];
            }
            PropertyInfo info = propertyValue.GetType().GetProperty("Item", BindingFlags.Public | BindingFlags.Instance, null, null, new Type[] { obj3.GetType() }, null);
            if (info == null)
            {
                throw new ArgumentException(System.Web.SR.GetString("DataBinder_No_Indexed_Accessor", new object[] { propertyValue.GetType().FullName }));
            }
            return info.GetValue(propertyValue, new object[] { obj3 });
        }

        public static string GetIndexedPropertyValue(object container, string propName, string format)
        {
            object indexedPropertyValue = GetIndexedPropertyValue(container, propName);
            if ((indexedPropertyValue == null) || (indexedPropertyValue == DBNull.Value))
            {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(format))
            {
                return indexedPropertyValue.ToString();
            }
            return string.Format(format, indexedPropertyValue);
        }

        internal static PropertyDescriptorCollection GetPropertiesFromCache(object container)
        {
            if (!EnableCaching || (container is ICustomTypeDescriptor))
            {
                return TypeDescriptor.GetProperties(container);
            }
            PropertyDescriptorCollection properties = null;
            Type key = container.GetType();
            if (!propertyCache.TryGetValue(key, out properties))
            {
                properties = TypeDescriptor.GetProperties(key);
                propertyCache.TryAdd(key, properties);
            }
            return properties;
        }

        public static object GetPropertyValue(object container, string propName)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            if (string.IsNullOrEmpty(propName))
            {
                throw new ArgumentNullException("propName");
            }
            PropertyDescriptor descriptor = GetPropertiesFromCache(container).Find(propName, true);
            if (descriptor == null)
            {
                throw new HttpException(System.Web.SR.GetString("DataBinder_Prop_Not_Found", new object[] { container.GetType().FullName, propName }));
            }
            return descriptor.GetValue(container);
        }

        public static string GetPropertyValue(object container, string propName, string format)
        {
            object propertyValue = GetPropertyValue(container, propName);
            if ((propertyValue == null) || (propertyValue == DBNull.Value))
            {
                return string.Empty;
            }
            if (string.IsNullOrEmpty(format))
            {
                return propertyValue.ToString();
            }
            return string.Format(format, propertyValue);
        }

        internal static bool IsNull(object value)
        {
            if ((value != null) && !Convert.IsDBNull(value))
            {
                return false;
            }
            return true;
        }

        public static bool EnableCaching
        {
            get
            {
                return enableCaching;
            }
            set
            {
                enableCaching = value;
                if (!value)
                {
                    propertyCache.Clear();
                }
            }
        }
    }
}

