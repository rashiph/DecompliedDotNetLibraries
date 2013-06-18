namespace System.ServiceModel.Channels
{
    using System;
    using System.Globalization;
    using System.ServiceModel;

    internal static class RedirectionUtility
    {
        public static int ComputeHashCode(string value, string ns)
        {
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            }
            string str = value + value.GetHashCode().ToString(CultureInfo.InvariantCulture);
            if (!string.IsNullOrEmpty(ns))
            {
                str = str + ns;
            }
            return str.GetHashCode();
        }

        public static bool IsNamespaceAndValueMatch(string value1, string namespace1, string value2, string namespace2)
        {
            bool flag = false;
            if (IsNamespaceMatch(namespace1, namespace2))
            {
                flag = string.Equals(value1, value2, StringComparison.Ordinal);
            }
            return flag;
        }

        public static bool IsNamespaceMatch(string namespace1, string namespace2)
        {
            bool flag = false;
            if ((namespace1 == null) && (namespace2 == null))
            {
                return true;
            }
            if ((namespace1 == null) || (namespace2 == null))
            {
                return false;
            }
            if (string.Equals(namespace1, namespace2, StringComparison.Ordinal))
            {
                flag = true;
            }
            return flag;
        }
    }
}

