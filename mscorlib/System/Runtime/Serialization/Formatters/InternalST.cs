namespace System.Runtime.Serialization.Formatters
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [ComVisible(true), SecurityCritical]
    public sealed class InternalST
    {
        private InternalST()
        {
        }

        [Conditional("_LOGGING")]
        public static void InfoSoap(params object[] messages)
        {
        }

        public static Assembly LoadAssemblyFromString(string assemblyString)
        {
            return FormatterServices.LoadAssemblyFromString(assemblyString);
        }

        public static void SerializationSetValue(FieldInfo fi, object target, object value)
        {
            if (fi == null)
            {
                throw new ArgumentNullException("fi");
            }
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            FormatterServices.SerializationSetValue(fi, target, value);
        }

        [Conditional("SER_LOGGING")]
        public static void Soap(params object[] messages)
        {
            if (!(messages[0] is string))
            {
                messages[0] = messages[0].GetType().Name + " ";
            }
            else
            {
                messages[0] = messages[0] + " ";
            }
        }

        [Conditional("_DEBUG")]
        public static void SoapAssert(bool condition, string message)
        {
        }

        public static bool SoapCheckEnabled()
        {
            return BCLDebug.CheckEnabled("Soap");
        }
    }
}

