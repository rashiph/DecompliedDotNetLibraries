namespace System.Web.Util
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Globalization;
    using System.Web;

    internal static class ProviderUtil
    {
        internal const int Infinite = 0x7fffffff;

        internal static void CheckUnrecognizedAttributes(NameValueCollection config, string providerName)
        {
            if (config.Count > 0)
            {
                string key = config.GetKey(0);
                if (!string.IsNullOrEmpty(key))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Unexpected_provider_attribute", new object[] { key, providerName }));
                }
            }
        }

        internal static void GetAndRemoveBooleanAttribute(NameValueCollection config, string attrib, string providerName, ref bool val)
        {
            GetBooleanAttribute(config, attrib, providerName, ref val);
            config.Remove(attrib);
        }

        internal static void GetAndRemoveNonZeroPositiveOrInfiniteAttribute(NameValueCollection config, string attrib, string providerName, ref int val)
        {
            GetNonZeroPositiveOrInfiniteAttribute(config, attrib, providerName, ref val);
            config.Remove(attrib);
        }

        internal static void GetAndRemovePositiveAttribute(NameValueCollection config, string attrib, string providerName, ref int val)
        {
            GetPositiveAttribute(config, attrib, providerName, ref val);
            config.Remove(attrib);
        }

        internal static void GetAndRemovePositiveOrInfiniteAttribute(NameValueCollection config, string attrib, string providerName, ref int val)
        {
            GetPositiveOrInfiniteAttribute(config, attrib, providerName, ref val);
            config.Remove(attrib);
        }

        internal static void GetAndRemoveRequiredNonEmptyStringAttribute(NameValueCollection config, string attrib, string providerName, ref string val)
        {
            GetRequiredNonEmptyStringAttribute(config, attrib, providerName, ref val);
            config.Remove(attrib);
        }

        internal static void GetAndRemoveStringAttribute(NameValueCollection config, string attrib, string providerName, ref string val)
        {
            val = config.Get(attrib);
            config.Remove(attrib);
        }

        internal static void GetBooleanAttribute(NameValueCollection config, string attrib, string providerName, ref bool val)
        {
            string str = config.Get(attrib);
            if (str != null)
            {
                if (str == "true")
                {
                    val = true;
                }
                else
                {
                    if (str != "false")
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_provider_attribute", new object[] { attrib, providerName, str }));
                    }
                    val = false;
                }
            }
        }

        private static void GetNonEmptyStringAttributeInternal(NameValueCollection config, string attrib, string providerName, ref string val, bool required)
        {
            string str = config.Get(attrib);
            if (((str == null) && required) || (str.Length == 0))
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Provider_missing_attribute", new object[] { attrib, providerName }));
            }
            val = str;
        }

        internal static void GetNonZeroPositiveOrInfiniteAttribute(NameValueCollection config, string attrib, string providerName, ref int val)
        {
            string str = config.Get(attrib);
            if (str != null)
            {
                int num;
                if (str == "Infinite")
                {
                    num = 0x7fffffff;
                }
                else
                {
                    try
                    {
                        num = Convert.ToInt32(str, CultureInfo.InvariantCulture);
                    }
                    catch (Exception exception)
                    {
                        if (((exception is ArgumentException) || (exception is FormatException)) || (exception is OverflowException))
                        {
                            throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_provider_non_zero_positive_attributes", new object[] { attrib, providerName }));
                        }
                        throw;
                    }
                    if (num <= 0)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_provider_non_zero_positive_attributes", new object[] { attrib, providerName }));
                    }
                }
                val = num;
            }
        }

        internal static void GetPositiveAttribute(NameValueCollection config, string attrib, string providerName, ref int val)
        {
            string str = config.Get(attrib);
            if (str != null)
            {
                int num;
                try
                {
                    num = Convert.ToInt32(str, CultureInfo.InvariantCulture);
                }
                catch (Exception exception)
                {
                    if (((exception is ArgumentException) || (exception is FormatException)) || (exception is OverflowException))
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_provider_positive_attributes", new object[] { attrib, providerName }));
                    }
                    throw;
                }
                if (num < 0)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_provider_positive_attributes", new object[] { attrib, providerName }));
                }
                val = num;
            }
        }

        internal static void GetPositiveOrInfiniteAttribute(NameValueCollection config, string attrib, string providerName, ref int val)
        {
            string str = config.Get(attrib);
            if (str != null)
            {
                int num;
                if (str == "Infinite")
                {
                    num = 0x7fffffff;
                }
                else
                {
                    try
                    {
                        num = Convert.ToInt32(str, CultureInfo.InvariantCulture);
                    }
                    catch (Exception exception)
                    {
                        if (((exception is ArgumentException) || (exception is FormatException)) || (exception is OverflowException))
                        {
                            throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_provider_positive_attributes", new object[] { attrib, providerName }));
                        }
                        throw;
                    }
                    if (num < 0)
                    {
                        throw new ConfigurationErrorsException(System.Web.SR.GetString("Invalid_provider_positive_attributes", new object[] { attrib, providerName }));
                    }
                }
                val = num;
            }
        }

        internal static void GetRequiredNonEmptyStringAttribute(NameValueCollection config, string attrib, string providerName, ref string val)
        {
            GetNonEmptyStringAttributeInternal(config, attrib, providerName, ref val, true);
        }
    }
}

