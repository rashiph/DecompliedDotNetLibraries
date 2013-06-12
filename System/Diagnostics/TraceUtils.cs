namespace System.Diagnostics
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Reflection;

    internal static class TraceUtils
    {
        private static object ConvertToBaseTypeOrEnum(string value, Type type)
        {
            if (type.IsEnum)
            {
                return Enum.Parse(type, value, false);
            }
            return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }

        internal static object GetRuntimeObject(string className, Type baseType, string initializeData)
        {
            object obj2 = null;
            Type c = null;
            if (className.Length == 0)
            {
                throw new ConfigurationErrorsException(System.SR.GetString("EmptyTypeName_NotAllowed"));
            }
            c = Type.GetType(className);
            if (c == null)
            {
                throw new ConfigurationErrorsException(System.SR.GetString("Could_not_find_type", new object[] { className }));
            }
            if (!baseType.IsAssignableFrom(c))
            {
                throw new ConfigurationErrorsException(System.SR.GetString("Incorrect_base_type", new object[] { className, baseType.FullName }));
            }
            Exception inner = null;
            try
            {
                if (string.IsNullOrEmpty(initializeData))
                {
                    if (IsOwnedTL(c))
                    {
                        throw new ConfigurationErrorsException(System.SR.GetString("TL_InitializeData_NotSpecified"));
                    }
                    ConstructorInfo constructor = c.GetConstructor(new Type[0]);
                    if (constructor == null)
                    {
                        throw new ConfigurationErrorsException(System.SR.GetString("Could_not_get_constructor", new object[] { className }));
                    }
                    obj2 = SecurityUtils.ConstructorInfoInvoke(constructor, new object[0]);
                }
                else
                {
                    ConstructorInfo ctor = c.GetConstructor(new Type[] { typeof(string) });
                    if (ctor != null)
                    {
                        if ((IsOwnedTextWriterTL(c) && (initializeData[0] != Path.DirectorySeparatorChar)) && ((initializeData[0] != Path.AltDirectorySeparatorChar) && !Path.IsPathRooted(initializeData)))
                        {
                            string configFilePath = DiagnosticsConfiguration.ConfigFilePath;
                            if (!string.IsNullOrEmpty(configFilePath))
                            {
                                string directoryName = Path.GetDirectoryName(configFilePath);
                                if (directoryName != null)
                                {
                                    initializeData = Path.Combine(directoryName, initializeData);
                                }
                            }
                        }
                        obj2 = SecurityUtils.ConstructorInfoInvoke(ctor, new object[] { initializeData });
                    }
                    else
                    {
                        ConstructorInfo[] constructors = c.GetConstructors();
                        if (constructors == null)
                        {
                            throw new ConfigurationErrorsException(System.SR.GetString("Could_not_get_constructor", new object[] { className }));
                        }
                        for (int i = 0; i < constructors.Length; i++)
                        {
                            ParameterInfo[] parameters = constructors[i].GetParameters();
                            if (parameters.Length == 1)
                            {
                                Type parameterType = parameters[0].ParameterType;
                                try
                                {
                                    object obj3 = ConvertToBaseTypeOrEnum(initializeData, parameterType);
                                    obj2 = SecurityUtils.ConstructorInfoInvoke(constructors[i], new object[] { obj3 });
                                    goto Label_0223;
                                }
                                catch (TargetInvocationException exception2)
                                {
                                    inner = exception2.InnerException;
                                }
                                catch (Exception exception3)
                                {
                                    inner = exception3;
                                }
                            }
                        }
                    }
                }
            }
            catch (TargetInvocationException exception4)
            {
                inner = exception4.InnerException;
            }
        Label_0223:
            if (obj2 != null)
            {
                return obj2;
            }
            if (inner != null)
            {
                throw new ConfigurationErrorsException(System.SR.GetString("Could_not_create_type_instance", new object[] { className }), inner);
            }
            throw new ConfigurationErrorsException(System.SR.GetString("Could_not_create_type_instance", new object[] { className }));
        }

        internal static bool IsOwnedTextWriterTL(Type type)
        {
            if (!(typeof(XmlWriterTraceListener) == type) && !(typeof(DelimitedListTraceListener) == type))
            {
                return (typeof(TextWriterTraceListener) == type);
            }
            return true;
        }

        internal static bool IsOwnedTL(Type type)
        {
            if (!(typeof(EventLogTraceListener) == type))
            {
                return IsOwnedTextWriterTL(type);
            }
            return true;
        }

        internal static void VerifyAttributes(IDictionary attributes, string[] supportedAttributes, object parent)
        {
            foreach (string str in attributes.Keys)
            {
                bool flag = false;
                if (supportedAttributes != null)
                {
                    for (int i = 0; i < supportedAttributes.Length; i++)
                    {
                        if (supportedAttributes[i] == str)
                        {
                            flag = true;
                        }
                    }
                }
                if (!flag)
                {
                    throw new ConfigurationErrorsException(System.SR.GetString("AttributeNotSupported", new object[] { str, parent.GetType().FullName }));
                }
            }
        }
    }
}

