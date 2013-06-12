namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Threading;
    using System.Web;
    using System.Web.Compilation;
    using System.Xml;

    internal class ConfigUtil
    {
        private ConfigUtil()
        {
        }

        internal static void CheckAssignableType(Type baseType, Type type, ConfigurationElement configElement, string propertyName)
        {
            if (!baseType.IsAssignableFrom(type))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Type_doesnt_inherit_from_type", new object[] { type.FullName, baseType.FullName }), configElement.ElementInformation.Properties[propertyName].Source, configElement.ElementInformation.Properties[propertyName].LineNumber);
            }
        }

        internal static void CheckAssignableType(Type baseType, Type baseType2, Type type, ConfigurationElement configElement, string propertyName)
        {
            if (!baseType.IsAssignableFrom(type) && !baseType2.IsAssignableFrom(type))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Type_doesnt_inherit_from_type", new object[] { type.FullName, baseType.FullName }), configElement.ElementInformation.Properties[propertyName].Source, configElement.ElementInformation.Properties[propertyName].LineNumber);
            }
        }

        internal static void CheckBaseType(Type expectedBaseType, Type userBaseType, string propertyName, ConfigurationElement configElement)
        {
            if (!expectedBaseType.IsAssignableFrom(userBaseType))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_type_to_inherit_from", new object[] { userBaseType.FullName, expectedBaseType.FullName }), configElement.ElementInformation.Properties[propertyName].Source, configElement.ElementInformation.Properties[propertyName].LineNumber);
            }
        }

        internal static Type GetType(string typeName, XmlNode node)
        {
            return GetType(typeName, node, false);
        }

        internal static Type GetType(string typeName, string propertyName, ConfigurationElement configElement)
        {
            return GetType(typeName, propertyName, configElement, true);
        }

        internal static Type GetType(string typeName, XmlNode node, bool ignoreCase)
        {
            return GetType(typeName, null, null, node, true, ignoreCase);
        }

        internal static Type GetType(string typeName, string propertyName, ConfigurationElement configElement, bool checkAptcaBit)
        {
            return GetType(typeName, propertyName, configElement, checkAptcaBit, false);
        }

        internal static Type GetType(string typeName, string propertyName, ConfigurationElement configElement, bool checkAptcaBit, bool ignoreCase)
        {
            return GetType(typeName, propertyName, configElement, null, checkAptcaBit, ignoreCase);
        }

        internal static Type GetType(string typeName, string propertyName, ConfigurationElement configElement, XmlNode node, bool checkAptcaBit, bool ignoreCase)
        {
            Type type;
            try
            {
                type = BuildManager.GetType(typeName, true, ignoreCase);
            }
            catch (Exception exception)
            {
                if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                {
                    throw;
                }
                if (node != null)
                {
                    throw new ConfigurationErrorsException(exception.Message, exception, node);
                }
                if (configElement != null)
                {
                    throw new ConfigurationErrorsException(exception.Message, exception, configElement.ElementInformation.Properties[propertyName].Source, configElement.ElementInformation.Properties[propertyName].LineNumber);
                }
                throw new ConfigurationErrorsException(exception.Message, exception);
            }
            if (checkAptcaBit)
            {
                if (node != null)
                {
                    HttpRuntime.FailIfNoAPTCABit(type, node);
                    return type;
                }
                HttpRuntime.FailIfNoAPTCABit(type, (configElement != null) ? configElement.ElementInformation : null, propertyName);
            }
            return type;
        }

        internal static bool IsTypeHandlerOrFactory(Type t)
        {
            if (!typeof(IHttpHandler).IsAssignableFrom(t))
            {
                return typeof(IHttpHandlerFactory).IsAssignableFrom(t);
            }
            return true;
        }
    }
}

