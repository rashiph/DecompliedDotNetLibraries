namespace System.Configuration
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Security.Cryptography.Xml;
    using System.Security.Permissions;
    using System.Xml;

    [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
    public sealed class RsaProtectedConfigurationProvider : ProtectedConfigurationProvider
    {
        private string _CspProviderName;
        private string _KeyContainerName;
        private string _KeyName;
        private bool _UseMachineContainer;
        private bool _UseOAEP;
        private const uint CRYPT_MACHINE_KEYSET = 0x20;
        private const string DefaultRsaKeyContainerName = "NetFrameworkConfigurationKey";
        private const uint PROV_Rsa_FULL = 1;

        public void AddKey(int keySize, bool exportable)
        {
            RSACryptoServiceProvider cryptoServiceProvider = this.GetCryptoServiceProvider(exportable, false);
            cryptoServiceProvider.KeySize = keySize;
            cryptoServiceProvider.PersistKeyInCsp = true;
            cryptoServiceProvider.Clear();
        }

        public override XmlNode Decrypt(XmlNode encryptedNode)
        {
            XmlDocument document = new XmlDocument();
            EncryptedXml xml = null;
            RSACryptoServiceProvider cryptoServiceProvider = this.GetCryptoServiceProvider(false, true);
            document.PreserveWhitespace = true;
            document.LoadXml(encryptedNode.OuterXml);
            xml = new EncryptedXml(document);
            xml.AddKeyNameMapping(this._KeyName, cryptoServiceProvider);
            xml.DecryptDocument();
            cryptoServiceProvider.Clear();
            return document.DocumentElement;
        }

        public void DeleteKey()
        {
            RSACryptoServiceProvider cryptoServiceProvider = this.GetCryptoServiceProvider(false, true);
            cryptoServiceProvider.PersistKeyInCsp = false;
            cryptoServiceProvider.Clear();
        }

        public override XmlNode Encrypt(XmlNode node)
        {
            RSACryptoServiceProvider cryptoServiceProvider = this.GetCryptoServiceProvider(false, false);
            XmlDocument document = new XmlDocument {
                PreserveWhitespace = true
            };
            document.LoadXml("<foo>" + node.OuterXml + "</foo>");
            EncryptedXml xml = new EncryptedXml(document);
            XmlElement documentElement = document.DocumentElement;
            SymmetricAlgorithm symmetricAlgorithm = new TripleDESCryptoServiceProvider();
            byte[] randomKey = this.GetRandomKey();
            symmetricAlgorithm.Key = randomKey;
            symmetricAlgorithm.Mode = CipherMode.ECB;
            symmetricAlgorithm.Padding = PaddingMode.PKCS7;
            byte[] buffer = xml.EncryptData(documentElement, symmetricAlgorithm, true);
            EncryptedData encryptedData = new EncryptedData {
                Type = "http://www.w3.org/2001/04/xmlenc#Element",
                EncryptionMethod = new EncryptionMethod("http://www.w3.org/2001/04/xmlenc#tripledes-cbc"),
                KeyInfo = new KeyInfo()
            };
            EncryptedKey encryptedKey = new EncryptedKey {
                EncryptionMethod = new EncryptionMethod("http://www.w3.org/2001/04/xmlenc#rsa-1_5"),
                KeyInfo = new KeyInfo(),
                CipherData = new CipherData()
            };
            encryptedKey.CipherData.CipherValue = EncryptedXml.EncryptKey(symmetricAlgorithm.Key, cryptoServiceProvider, this.UseOAEP);
            KeyInfoName clause = new KeyInfoName {
                Value = this._KeyName
            };
            encryptedKey.KeyInfo.AddClause(clause);
            KeyInfoEncryptedKey key2 = new KeyInfoEncryptedKey(encryptedKey);
            encryptedData.KeyInfo.AddClause(key2);
            encryptedData.CipherData = new CipherData();
            encryptedData.CipherData.CipherValue = buffer;
            EncryptedXml.ReplaceElement(documentElement, encryptedData, true);
            foreach (XmlNode node2 in document.ChildNodes)
            {
                if (node2.NodeType == XmlNodeType.Element)
                {
                    foreach (XmlNode node3 in node2.ChildNodes)
                    {
                        if (node3.NodeType == XmlNodeType.Element)
                        {
                            return node3;
                        }
                    }
                }
            }
            return null;
        }

        public void ExportKey(string xmlFileName, bool includePrivateParameters)
        {
            RSACryptoServiceProvider cryptoServiceProvider = this.GetCryptoServiceProvider(false, false);
            string contents = cryptoServiceProvider.ToXmlString(includePrivateParameters);
            File.WriteAllText(xmlFileName, contents);
            cryptoServiceProvider.Clear();
        }

        private static bool GetBooleanValue(NameValueCollection configurationValues, string valueName, bool defaultValue)
        {
            string str = configurationValues[valueName];
            if (str == null)
            {
                return defaultValue;
            }
            configurationValues.Remove(valueName);
            if (str == "true")
            {
                return true;
            }
            if (str != "false")
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_invalid_boolean_attribute", new object[] { valueName }));
            }
            return false;
        }

        private RSACryptoServiceProvider GetCryptoServiceProvider(bool exportable, bool keyMustExist)
        {
            RSACryptoServiceProvider provider;
            try
            {
                CspParameters parameters = new CspParameters {
                    KeyContainerName = this.KeyContainerName,
                    KeyNumber = 1,
                    ProviderType = 1
                };
                if ((this.CspProviderName != null) && (this.CspProviderName.Length > 0))
                {
                    parameters.ProviderName = this.CspProviderName;
                }
                if (this.UseMachineContainer)
                {
                    parameters.Flags |= CspProviderFlags.UseMachineKeyStore;
                }
                if (!exportable && !keyMustExist)
                {
                    parameters.Flags |= CspProviderFlags.UseNonExportableKey;
                }
                if (keyMustExist)
                {
                    parameters.Flags |= CspProviderFlags.UseExistingKey;
                }
                provider = new RSACryptoServiceProvider(parameters);
            }
            catch
            {
                this.ThrowBetterException(keyMustExist);
                throw;
            }
            return provider;
        }

        private byte[] GetRandomKey()
        {
            byte[] data = new byte[0x18];
            new RNGCryptoServiceProvider().GetBytes(data);
            return data;
        }

        public void ImportKey(string xmlFileName, bool exportable)
        {
            RSACryptoServiceProvider cryptoServiceProvider = this.GetCryptoServiceProvider(exportable, false);
            cryptoServiceProvider.FromXmlString(File.ReadAllText(xmlFileName));
            cryptoServiceProvider.PersistKeyInCsp = true;
            cryptoServiceProvider.Clear();
        }

        public override void Initialize(string name, NameValueCollection configurationValues)
        {
            base.Initialize(name, configurationValues);
            this._KeyName = "Rsa Key";
            this._KeyContainerName = configurationValues["keyContainerName"];
            configurationValues.Remove("keyContainerName");
            if ((this._KeyContainerName == null) || (this._KeyContainerName.Length < 1))
            {
                this._KeyContainerName = "NetFrameworkConfigurationKey";
            }
            this._CspProviderName = configurationValues["cspProviderName"];
            configurationValues.Remove("cspProviderName");
            this._UseMachineContainer = GetBooleanValue(configurationValues, "useMachineContainer", true);
            this._UseOAEP = GetBooleanValue(configurationValues, "useOAEP", false);
            if (configurationValues.Count > 0)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Unrecognized_initialization_value", new object[] { configurationValues.GetKey(0) }));
            }
        }

        private void ThrowBetterException(bool keyMustExist)
        {
            SafeCryptContextHandle phProv = null;
            try
            {
                if (Microsoft.Win32.UnsafeNativeMethods.CryptAcquireContext(out phProv, this.KeyContainerName, this.CspProviderName, 1, this.UseMachineContainer ? 0x20 : 0) == 0)
                {
                    int errorCode = Marshal.GetHRForLastWin32Error();
                    if ((errorCode != -2146893802) || keyMustExist)
                    {
                        switch (errorCode)
                        {
                            case -2147024891:
                            case -2147024890:
                            case -2146893802:
                                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Key_container_doesnt_exist_or_access_denied"));
                        }
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
            }
            finally
            {
                if ((phProv != null) && !phProv.IsInvalid)
                {
                    phProv.Dispose();
                }
            }
        }

        public string CspProviderName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._CspProviderName;
            }
        }

        public string KeyContainerName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._KeyContainerName;
            }
        }

        public RSAParameters RsaPublicKey
        {
            get
            {
                return this.GetCryptoServiceProvider(false, false).ExportParameters(false);
            }
        }

        public bool UseMachineContainer
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._UseMachineContainer;
            }
        }

        public bool UseOAEP
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._UseOAEP;
            }
        }
    }
}

