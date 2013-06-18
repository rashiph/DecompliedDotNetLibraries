namespace System.Configuration
{
    using System;
    using System.Configuration.Internal;
    using System.Xml;

    internal static class ExceptionUtil
    {
        internal static ArgumentException ParameterInvalid(string parameter)
        {
            return new ArgumentException(System.Configuration.SR.GetString("Parameter_Invalid", new object[] { parameter }), parameter);
        }

        internal static ArgumentException ParameterNullOrEmpty(string parameter)
        {
            return new ArgumentException(System.Configuration.SR.GetString("Parameter_NullOrEmpty", new object[] { parameter }), parameter);
        }

        internal static ArgumentException PropertyInvalid(string property)
        {
            return new ArgumentException(System.Configuration.SR.GetString("Property_Invalid", new object[] { property }), property);
        }

        internal static ArgumentException PropertyNullOrEmpty(string property)
        {
            return new ArgumentException(System.Configuration.SR.GetString("Property_NullOrEmpty", new object[] { property }), property);
        }

        internal static InvalidOperationException UnexpectedError(string methodName)
        {
            return new InvalidOperationException(System.Configuration.SR.GetString("Unexpected_Error", new object[] { methodName }));
        }

        internal static ConfigurationErrorsException WrapAsConfigException(string outerMessage, Exception e, IConfigErrorInfo errorInfo)
        {
            if (errorInfo != null)
            {
                return WrapAsConfigException(outerMessage, e, errorInfo.Filename, errorInfo.LineNumber);
            }
            return WrapAsConfigException(outerMessage, e, null, 0);
        }

        internal static ConfigurationErrorsException WrapAsConfigException(string outerMessage, Exception e, string filename, int line)
        {
            ConfigurationErrorsException exception = e as ConfigurationErrorsException;
            if (exception != null)
            {
                return exception;
            }
            ConfigurationException exception2 = e as ConfigurationException;
            if (exception2 != null)
            {
                return new ConfigurationErrorsException(exception2);
            }
            XmlException inner = e as XmlException;
            if (inner != null)
            {
                if (inner.LineNumber != 0)
                {
                    line = inner.LineNumber;
                }
                return new ConfigurationErrorsException(inner.Message, inner, filename, line);
            }
            if (e != null)
            {
                return new ConfigurationErrorsException(System.Configuration.SR.GetString("Wrapped_exception_message", new object[] { outerMessage, e.Message }), e, filename, line);
            }
            return new ConfigurationErrorsException(System.Configuration.SR.GetString("Wrapped_exception_message", new object[] { outerMessage, NoExceptionInformation }), filename, line);
        }

        internal static string NoExceptionInformation
        {
            get
            {
                return System.Configuration.SR.GetString("No_exception_information_available");
            }
        }
    }
}

