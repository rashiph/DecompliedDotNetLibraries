namespace System.Web.Util
{
    using System;
    using System.Web;

    internal static class ExceptionUtil
    {
        internal static ArgumentException ParameterInvalid(string parameter)
        {
            return new ArgumentException(System.Web.SR.GetString("Parameter_Invalid", new object[] { parameter }), parameter);
        }

        internal static ArgumentException ParameterNullOrEmpty(string parameter)
        {
            return new ArgumentException(System.Web.SR.GetString("Parameter_NullOrEmpty", new object[] { parameter }), parameter);
        }

        internal static ArgumentException PropertyInvalid(string property)
        {
            return new ArgumentException(System.Web.SR.GetString("Property_Invalid", new object[] { property }), property);
        }

        internal static ArgumentException PropertyNullOrEmpty(string property)
        {
            return new ArgumentException(System.Web.SR.GetString("Property_NullOrEmpty", new object[] { property }), property);
        }

        internal static InvalidOperationException UnexpectedError(string methodName)
        {
            return new InvalidOperationException(System.Web.SR.GetString("Unexpected_Error", new object[] { methodName }));
        }
    }
}

