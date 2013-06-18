namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Bridge;
    using Microsoft.Win32;
    using System;
    using System.IO;
    using System.Security;
    using System.Security.AccessControl;

    internal class RegistryConfigurationProvider : ConfigurationProvider
    {
        private RegistryKey regKey;

        public RegistryConfigurationProvider(RegistryKey rootKey, string subKey)
        {
            try
            {
                if (rootKey != null)
                {
                    this.regKey = rootKey.OpenSubKey(subKey, RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.ExecuteKey);
                }
            }
            catch (SecurityException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationProviderException(Microsoft.Transactions.SR.GetString("RegistryKeyOpenSubKeyFailed", new object[] { subKey, rootKey.Name, exception.Message }), exception));
            }
            catch (IOException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationProviderException(Microsoft.Transactions.SR.GetString("RegistryKeyOpenSubKeyFailed", new object[] { subKey, rootKey.Name, exception2.Message }), exception2));
            }
        }

        public override void Dispose()
        {
            if (this.regKey != null)
            {
                this.regKey.Close();
                this.regKey = null;
            }
        }

        public override ConfigurationProvider OpenKey(string key)
        {
            return new RegistryConfigurationProvider(this.regKey, key);
        }

        public override int ReadInteger(string value, int defaultValue)
        {
            object obj2 = this.ReadValue(value, defaultValue);
            if ((obj2 != null) && (obj2 is int))
            {
                return (int) obj2;
            }
            return defaultValue;
        }

        public override string[] ReadMultiString(string value, string[] defaultValue)
        {
            object obj2 = this.ReadValue(value, defaultValue);
            if (obj2 == null)
            {
                return defaultValue;
            }
            string[] strArray = obj2 as string[];
            if (strArray == null)
            {
                return defaultValue;
            }
            return strArray;
        }

        public override string ReadString(string value, string defaultValue)
        {
            object obj2 = this.ReadValue(value, defaultValue);
            if (obj2 == null)
            {
                return defaultValue;
            }
            string str = obj2 as string;
            if (str == null)
            {
                return defaultValue;
            }
            return str;
        }

        private object ReadValue(string value, object defaultValue)
        {
            object obj2;
            if (this.regKey == null)
            {
                return defaultValue;
            }
            try
            {
                obj2 = this.regKey.GetValue(value, defaultValue);
            }
            catch (SecurityException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationProviderException(Microsoft.Transactions.SR.GetString("RegistryKeyGetValueFailed", new object[] { value, this.regKey.Name, exception.Message }), exception));
            }
            catch (IOException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationProviderException(Microsoft.Transactions.SR.GetString("RegistryKeyGetValueFailed", new object[] { value, this.regKey.Name, exception2.Message }), exception2));
            }
            return obj2;
        }
    }
}

