namespace System.Security.Cryptography
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Security.Util;
    using System.Threading;

    [ComVisible(true)]
    public class CryptoConfig
    {
        private static Dictionary<string, Type> appNameHT = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string> appOidHT = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, object> defaultNameHT = null;
        private static Dictionary<string, string> defaultOidHT = null;
        private const string MachineConfigFilename = "machine.config";
        private static Dictionary<string, string> machineNameHT = null;
        private static Dictionary<string, string> machineOidHT = null;
        private static bool? s_fipsAlgorithmPolicy;
        private static object s_InternalSyncObject;
        private static string version = null;

        [SecurityCritical]
        public static void AddAlgorithm(Type algorithm, params string[] names)
        {
            if (algorithm == null)
            {
                throw new ArgumentNullException("algorithm");
            }
            if (!algorithm.IsVisible)
            {
                throw new ArgumentException(Environment.GetResourceString("Cryptography_AlgorithmTypesMustBeVisible"), "algorithm");
            }
            if (names == null)
            {
                throw new ArgumentNullException("names");
            }
            string[] destinationArray = new string[names.Length];
            Array.Copy(names, destinationArray, destinationArray.Length);
            foreach (string str in destinationArray)
            {
                if (string.IsNullOrEmpty(str))
                {
                    throw new ArgumentException(Environment.GetResourceString("Cryptography_AddNullOrEmptyName"));
                }
            }
            lock (InternalSyncObject)
            {
                foreach (string str2 in destinationArray)
                {
                    appNameHT[str2] = algorithm;
                }
            }
        }

        [SecurityCritical]
        public static void AddOID(string oid, params string[] names)
        {
            if (oid == null)
            {
                throw new ArgumentNullException("oid");
            }
            if (names == null)
            {
                throw new ArgumentNullException("names");
            }
            string[] destinationArray = new string[names.Length];
            Array.Copy(names, destinationArray, destinationArray.Length);
            foreach (string str in destinationArray)
            {
                if (string.IsNullOrEmpty(str))
                {
                    throw new ArgumentException(Environment.GetResourceString("Cryptography_AddNullOrEmptyName"));
                }
            }
            lock (InternalSyncObject)
            {
                foreach (string str2 in destinationArray)
                {
                    appOidHT[str2] = oid;
                }
            }
        }

        [SecuritySafeCritical]
        public static object CreateFromName(string name)
        {
            return CreateFromName(name, null);
        }

        [SecuritySafeCritical]
        public static object CreateFromName(string name, params object[] args)
        {
            object obj4;
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            Type valueOrDefault = null;
            InitializeConfigInfo();
            lock (InternalSyncObject)
            {
                valueOrDefault = appNameHT.GetValueOrDefault(name);
            }
            if (valueOrDefault == null)
            {
                string typeName = machineNameHT.GetValueOrDefault(name);
                if (typeName != null)
                {
                    valueOrDefault = Type.GetType(typeName, false, false);
                    if ((valueOrDefault != null) && !valueOrDefault.IsVisible)
                    {
                        valueOrDefault = null;
                    }
                }
            }
            if (valueOrDefault == null)
            {
                object obj3 = DefaultNameHT.GetValueOrDefault(name);
                if (obj3 != null)
                {
                    if (obj3 is Type)
                    {
                        valueOrDefault = (Type) obj3;
                    }
                    else if (obj3 is string)
                    {
                        valueOrDefault = Type.GetType((string) obj3, false, false);
                        if ((valueOrDefault != null) && !valueOrDefault.IsVisible)
                        {
                            valueOrDefault = null;
                        }
                    }
                }
            }
            if (valueOrDefault == null)
            {
                valueOrDefault = Type.GetType(name, false, false);
                if ((valueOrDefault != null) && !valueOrDefault.IsVisible)
                {
                    valueOrDefault = null;
                }
            }
            if (valueOrDefault == null)
            {
                return null;
            }
            RuntimeType type2 = valueOrDefault as RuntimeType;
            if (type2 == null)
            {
                return null;
            }
            if (args == null)
            {
                args = new object[0];
            }
            MethodBase[] constructors = type2.GetConstructors(BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance);
            if (constructors == null)
            {
                return null;
            }
            List<MethodBase> list = new List<MethodBase>();
            for (int i = 0; i < constructors.Length; i++)
            {
                MethodBase item = constructors[i];
                if (item.GetParameters().Length == args.Length)
                {
                    list.Add(item);
                }
            }
            if (list.Count == 0)
            {
                return null;
            }
            constructors = list.ToArray();
            RuntimeConstructorInfo info = Type.DefaultBinder.BindToMethod(BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance, constructors, ref args, null, null, null, out obj4) as RuntimeConstructorInfo;
            if ((info == null) || typeof(Delegate).IsAssignableFrom(info.DeclaringType))
            {
                return null;
            }
            object obj2 = info.Invoke(BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance, Type.DefaultBinder, args, null);
            if (obj4 != null)
            {
                Type.DefaultBinder.ReorderArgumentArray(ref args, obj4);
            }
            return obj2;
        }

        [SecuritySafeCritical]
        public static byte[] EncodeOID(string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            char[] separator = new char[] { '.' };
            string[] strArray = str.Split(separator);
            uint[] numArray = new uint[strArray.Length];
            for (int i = 0; i < strArray.Length; i++)
            {
                numArray[i] = (uint) int.Parse(strArray[i], CultureInfo.InvariantCulture);
            }
            byte[] destinationArray = new byte[numArray.Length * 5];
            int destinationIndex = 0;
            if (numArray.Length < 2)
            {
                throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_InvalidOID"));
            }
            uint dwValue = (numArray[0] * 40) + numArray[1];
            byte[] sourceArray = EncodeSingleOIDNum(dwValue);
            Array.Copy(sourceArray, 0, destinationArray, destinationIndex, sourceArray.Length);
            destinationIndex += sourceArray.Length;
            for (int j = 2; j < numArray.Length; j++)
            {
                sourceArray = EncodeSingleOIDNum(numArray[j]);
                Buffer.InternalBlockCopy(sourceArray, 0, destinationArray, destinationIndex, sourceArray.Length);
                destinationIndex += sourceArray.Length;
            }
            if (destinationIndex > 0x7f)
            {
                throw new CryptographicUnexpectedOperationException(Environment.GetResourceString("Cryptography_Config_EncodedOIDError"));
            }
            sourceArray = new byte[destinationIndex + 2];
            sourceArray[0] = 6;
            sourceArray[1] = (byte) destinationIndex;
            Buffer.InternalBlockCopy(destinationArray, 0, sourceArray, 2, destinationIndex);
            return sourceArray;
        }

        private static byte[] EncodeSingleOIDNum(uint dwValue)
        {
            if (dwValue < 0x80)
            {
                return new byte[] { ((byte) dwValue) };
            }
            if (dwValue < 0x4000)
            {
                return new byte[] { ((byte) ((dwValue >> 7) | 0x80)), ((byte) (dwValue & 0x7f)) };
            }
            if (dwValue < 0x200000)
            {
                return new byte[] { ((byte) ((dwValue >> 14) | 0x80)), ((byte) ((dwValue >> 7) | 0x80)), ((byte) (dwValue & 0x7f)) };
            }
            if (dwValue < 0x10000000)
            {
                return new byte[] { ((byte) ((dwValue >> 0x15) | 0x80)), ((byte) ((dwValue >> 14) | 0x80)), ((byte) ((dwValue >> 7) | 0x80)), ((byte) (dwValue & 0x7f)) };
            }
            return new byte[] { ((byte) ((dwValue >> 0x1c) | 0x80)), ((byte) ((dwValue >> 0x15) | 0x80)), ((byte) ((dwValue >> 14) | 0x80)), ((byte) ((dwValue >> 7) | 0x80)), ((byte) (dwValue & 0x7f)) };
        }

        [SecurityCritical]
        private static void InitializeConfigInfo()
        {
            if (machineNameHT == null)
            {
                lock (InternalSyncObject)
                {
                    if (machineNameHT == null)
                    {
                        ConfigNode node = OpenCryptoConfig();
                        if (node != null)
                        {
                            foreach (ConfigNode node2 in node.Children)
                            {
                                if ((machineNameHT != null) && (machineOidHT != null))
                                {
                                    break;
                                }
                                if ((machineNameHT == null) && (string.Compare(node2.Name, "cryptoNameMapping", StringComparison.Ordinal) == 0))
                                {
                                    machineNameHT = InitializeNameMappings(node2);
                                }
                                else if ((machineOidHT == null) && (string.Compare(node2.Name, "oidMap", StringComparison.Ordinal) == 0))
                                {
                                    machineOidHT = InitializeOidMappings(node2);
                                }
                            }
                        }
                        if (machineNameHT == null)
                        {
                            machineNameHT = new Dictionary<string, string>();
                        }
                        if (machineOidHT == null)
                        {
                            machineOidHT = new Dictionary<string, string>();
                        }
                    }
                }
            }
        }

        private static Dictionary<string, string> InitializeNameMappings(ConfigNode nameMappingNode)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
            foreach (ConfigNode node in nameMappingNode.Children)
            {
                if (string.Compare(node.Name, "cryptoClasses", StringComparison.Ordinal) == 0)
                {
                    foreach (ConfigNode node2 in node.Children)
                    {
                        if ((string.Compare(node2.Name, "cryptoClass", StringComparison.Ordinal) == 0) && (node2.Attributes.Count > 0))
                        {
                            DictionaryEntry entry = node2.Attributes[0];
                            dictionary2.Add((string) entry.Key, (string) entry.Value);
                        }
                    }
                }
                else if (string.Compare(node.Name, "nameEntry", StringComparison.Ordinal) == 0)
                {
                    string key = null;
                    string str2 = null;
                    foreach (DictionaryEntry entry2 in node.Attributes)
                    {
                        if (string.Compare((string) entry2.Key, "name", StringComparison.Ordinal) == 0)
                        {
                            key = (string) entry2.Value;
                        }
                        else if (string.Compare((string) entry2.Key, "class", StringComparison.Ordinal) == 0)
                        {
                            str2 = (string) entry2.Value;
                        }
                    }
                    if ((key != null) && (str2 != null))
                    {
                        string valueOrDefault = dictionary2.GetValueOrDefault(str2);
                        if (valueOrDefault != null)
                        {
                            dictionary.Add(key, valueOrDefault);
                        }
                    }
                }
            }
            return dictionary;
        }

        private static Dictionary<string, string> InitializeOidMappings(ConfigNode oidMappingNode)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (ConfigNode node in oidMappingNode.Children)
            {
                if (string.Compare(node.Name, "oidEntry", StringComparison.Ordinal) == 0)
                {
                    string str = null;
                    string key = null;
                    foreach (DictionaryEntry entry in node.Attributes)
                    {
                        if (string.Compare((string) entry.Key, "OID", StringComparison.Ordinal) == 0)
                        {
                            str = (string) entry.Value;
                        }
                        else if (string.Compare((string) entry.Key, "name", StringComparison.Ordinal) == 0)
                        {
                            key = (string) entry.Value;
                        }
                    }
                    if ((key != null) && (str != null))
                    {
                        dictionary.Add(key, str);
                    }
                }
            }
            return dictionary;
        }

        public static string MapNameToOID(string name)
        {
            return MapNameToOID(name, OidGroup.AllGroups);
        }

        [SecuritySafeCritical]
        internal static string MapNameToOID(string name, OidGroup group)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            InitializeConfigInfo();
            string valueOrDefault = null;
            lock (InternalSyncObject)
            {
                valueOrDefault = appOidHT.GetValueOrDefault(name);
            }
            if (valueOrDefault == null)
            {
                valueOrDefault = machineOidHT.GetValueOrDefault(name);
            }
            if (valueOrDefault == null)
            {
                valueOrDefault = DefaultOidHT.GetValueOrDefault(name);
            }
            if (valueOrDefault == null)
            {
                valueOrDefault = X509Utils._GetOidFromFriendlyName(name, group);
            }
            return valueOrDefault;
        }

        [SecurityCritical]
        private static ConfigNode OpenCryptoConfig()
        {
            string path = Config.MachineDirectory + "machine.config";
            new FileIOPermission(FileIOPermissionAccess.Read, path).Assert();
            if (File.Exists(path))
            {
                CodeAccessPermission.RevertAssert();
                ConfigNode node = new ConfigTreeParser().Parse(path, "configuration", true);
                if (node == null)
                {
                    return null;
                }
                ConfigNode node2 = null;
                foreach (ConfigNode node3 in node.Children)
                {
                    bool flag = false;
                    if (string.Compare(node3.Name, "mscorlib", StringComparison.Ordinal) == 0)
                    {
                        foreach (DictionaryEntry entry in node3.Attributes)
                        {
                            if (string.Compare((string) entry.Key, "version", StringComparison.Ordinal) == 0)
                            {
                                flag = true;
                                if (string.Compare((string) entry.Value, Version, StringComparison.Ordinal) == 0)
                                {
                                    node2 = node3;
                                    break;
                                }
                            }
                        }
                        if (!flag)
                        {
                            node2 = node3;
                        }
                    }
                    if (node2 != null)
                    {
                        break;
                    }
                }
                if (node2 != null)
                {
                    foreach (ConfigNode node4 in node2.Children)
                    {
                        if (string.Compare(node4.Name, "cryptographySettings", StringComparison.Ordinal) == 0)
                        {
                            return node4;
                        }
                    }
                }
            }
            return null;
        }

        public static bool AllowOnlyFipsAlgorithms
        {
            [SecuritySafeCritical]
            get
            {
                if (!s_fipsAlgorithmPolicy.HasValue)
                {
                    if (Utils._GetEnforceFipsPolicySetting())
                    {
                        if (Environment.OSVersion.Version.Major >= 6)
                        {
                            bool flag;
                            uint num = Win32Native.BCryptGetFipsAlgorithmMode(out flag);
                            s_fipsAlgorithmPolicy = new bool?(((num != 0) && (num != 0xc0000034)) || flag);
                        }
                        else
                        {
                            s_fipsAlgorithmPolicy = new bool?(Utils.ReadLegacyFipsPolicy());
                        }
                    }
                    else
                    {
                        s_fipsAlgorithmPolicy = false;
                    }
                }
                return s_fipsAlgorithmPolicy.Value;
            }
        }

        private static Dictionary<string, object> DefaultNameHT
        {
            get
            {
                if (defaultNameHT == null)
                {
                    Dictionary<string, object> dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    Type type = typeof(SHA1CryptoServiceProvider);
                    Type type2 = typeof(MD5CryptoServiceProvider);
                    Type type3 = typeof(SHA256Managed);
                    Type type4 = typeof(SHA384Managed);
                    Type type5 = typeof(SHA512Managed);
                    Type type6 = typeof(RIPEMD160Managed);
                    Type type7 = typeof(HMACMD5);
                    Type type8 = typeof(HMACRIPEMD160);
                    Type type9 = typeof(HMACSHA1);
                    Type type10 = typeof(HMACSHA256);
                    Type type11 = typeof(HMACSHA384);
                    Type type12 = typeof(HMACSHA512);
                    Type type13 = typeof(MACTripleDES);
                    Type type14 = typeof(RSACryptoServiceProvider);
                    Type type15 = typeof(DSACryptoServiceProvider);
                    Type type16 = typeof(DESCryptoServiceProvider);
                    Type type17 = typeof(TripleDESCryptoServiceProvider);
                    Type type18 = typeof(RC2CryptoServiceProvider);
                    Type type19 = typeof(RijndaelManaged);
                    Type type20 = typeof(DSASignatureDescription);
                    Type type21 = typeof(RSAPKCS1SHA1SignatureDescription);
                    Type type22 = typeof(RNGCryptoServiceProvider);
                    string str = "System.Security.Cryptography.AesCryptoServiceProvider, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                    string str2 = "System.Security.Cryptography.AesManaged, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                    string str3 = "System.Security.Cryptography.ECDiffieHellmanCng, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                    string str4 = "System.Security.Cryptography.ECDsaCng, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                    string str5 = "System.Security.Cryptography.MD5Cng, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                    string str6 = "System.Security.Cryptography.SHA1Cng, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                    string str7 = "System.Security.Cryptography.SHA256Cng, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                    string str8 = "System.Security.Cryptography.SHA256CryptoServiceProvider, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                    string str9 = "System.Security.Cryptography.SHA384Cng, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                    string str10 = "System.Security.Cryptography.SHA384CryptoServiceProvider, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                    string str11 = "System.Security.Cryptography.SHA512Cng, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                    string str12 = "System.Security.Cryptography.SHA512CryptoServiceProvider, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
                    dictionary.Add("RandomNumberGenerator", type22);
                    dictionary.Add("System.Security.Cryptography.RandomNumberGenerator", type22);
                    dictionary.Add("SHA", type);
                    dictionary.Add("SHA1", type);
                    dictionary.Add("System.Security.Cryptography.SHA1", type);
                    dictionary.Add("System.Security.Cryptography.SHA1Cng", str6);
                    dictionary.Add("System.Security.Cryptography.HashAlgorithm", type);
                    dictionary.Add("MD5", type2);
                    dictionary.Add("System.Security.Cryptography.MD5", type2);
                    dictionary.Add("System.Security.Cryptography.MD5Cng", str5);
                    dictionary.Add("SHA256", type3);
                    dictionary.Add("SHA-256", type3);
                    dictionary.Add("System.Security.Cryptography.SHA256", type3);
                    dictionary.Add("System.Security.Cryptography.SHA256Cng", str7);
                    dictionary.Add("System.Security.Cryptography.SHA256CryptoServiceProvider", str8);
                    dictionary.Add("SHA384", type4);
                    dictionary.Add("SHA-384", type4);
                    dictionary.Add("System.Security.Cryptography.SHA384", type4);
                    dictionary.Add("System.Security.Cryptography.SHA384Cng", str9);
                    dictionary.Add("System.Security.Cryptography.SHA384CryptoServiceProvider", str10);
                    dictionary.Add("SHA512", type5);
                    dictionary.Add("SHA-512", type5);
                    dictionary.Add("System.Security.Cryptography.SHA512", type5);
                    dictionary.Add("System.Security.Cryptography.SHA512Cng", str11);
                    dictionary.Add("System.Security.Cryptography.SHA512CryptoServiceProvider", str12);
                    dictionary.Add("RIPEMD160", type6);
                    dictionary.Add("RIPEMD-160", type6);
                    dictionary.Add("System.Security.Cryptography.RIPEMD160", type6);
                    dictionary.Add("System.Security.Cryptography.RIPEMD160Managed", type6);
                    dictionary.Add("System.Security.Cryptography.HMAC", type9);
                    dictionary.Add("System.Security.Cryptography.KeyedHashAlgorithm", type9);
                    dictionary.Add("HMACMD5", type7);
                    dictionary.Add("System.Security.Cryptography.HMACMD5", type7);
                    dictionary.Add("HMACRIPEMD160", type8);
                    dictionary.Add("System.Security.Cryptography.HMACRIPEMD160", type8);
                    dictionary.Add("HMACSHA1", type9);
                    dictionary.Add("System.Security.Cryptography.HMACSHA1", type9);
                    dictionary.Add("HMACSHA256", type10);
                    dictionary.Add("System.Security.Cryptography.HMACSHA256", type10);
                    dictionary.Add("HMACSHA384", type11);
                    dictionary.Add("System.Security.Cryptography.HMACSHA384", type11);
                    dictionary.Add("HMACSHA512", type12);
                    dictionary.Add("System.Security.Cryptography.HMACSHA512", type12);
                    dictionary.Add("MACTripleDES", type13);
                    dictionary.Add("System.Security.Cryptography.MACTripleDES", type13);
                    dictionary.Add("RSA", type14);
                    dictionary.Add("System.Security.Cryptography.RSA", type14);
                    dictionary.Add("System.Security.Cryptography.AsymmetricAlgorithm", type14);
                    dictionary.Add("DSA", type15);
                    dictionary.Add("System.Security.Cryptography.DSA", type15);
                    dictionary.Add("ECDsa", str4);
                    dictionary.Add("ECDsaCng", str4);
                    dictionary.Add("System.Security.Cryptography.ECDsaCng", str4);
                    dictionary.Add("ECDH", str3);
                    dictionary.Add("ECDiffieHellman", str3);
                    dictionary.Add("ECDiffieHellmanCng", str3);
                    dictionary.Add("System.Security.Cryptography.ECDiffieHellmanCng", str3);
                    dictionary.Add("DES", type16);
                    dictionary.Add("System.Security.Cryptography.DES", type16);
                    dictionary.Add("3DES", type17);
                    dictionary.Add("TripleDES", type17);
                    dictionary.Add("Triple DES", type17);
                    dictionary.Add("System.Security.Cryptography.TripleDES", type17);
                    dictionary.Add("RC2", type18);
                    dictionary.Add("System.Security.Cryptography.RC2", type18);
                    dictionary.Add("Rijndael", type19);
                    dictionary.Add("System.Security.Cryptography.Rijndael", type19);
                    dictionary.Add("System.Security.Cryptography.SymmetricAlgorithm", type19);
                    dictionary.Add("AES", str);
                    dictionary.Add("AesCryptoServiceProvider", str);
                    dictionary.Add("System.Security.Cryptography.AesCryptoServiceProvider", str);
                    dictionary.Add("AesManaged", str2);
                    dictionary.Add("System.Security.Cryptography.AesManaged", str2);
                    dictionary.Add("http://www.w3.org/2000/09/xmldsig#dsa-sha1", type20);
                    dictionary.Add("System.Security.Cryptography.DSASignatureDescription", type20);
                    dictionary.Add("http://www.w3.org/2000/09/xmldsig#rsa-sha1", type21);
                    dictionary.Add("System.Security.Cryptography.RSASignatureDescription", type21);
                    dictionary.Add("http://www.w3.org/2000/09/xmldsig#sha1", type);
                    dictionary.Add("http://www.w3.org/2001/04/xmlenc#sha256", type3);
                    dictionary.Add("http://www.w3.org/2001/04/xmlenc#sha512", type5);
                    dictionary.Add("http://www.w3.org/2001/04/xmlenc#ripemd160", type6);
                    dictionary.Add("http://www.w3.org/2001/04/xmlenc#des-cbc", type16);
                    dictionary.Add("http://www.w3.org/2001/04/xmlenc#tripledes-cbc", type17);
                    dictionary.Add("http://www.w3.org/2001/04/xmlenc#kw-tripledes", type17);
                    dictionary.Add("http://www.w3.org/2001/04/xmlenc#aes128-cbc", type19);
                    dictionary.Add("http://www.w3.org/2001/04/xmlenc#kw-aes128", type19);
                    dictionary.Add("http://www.w3.org/2001/04/xmlenc#aes192-cbc", type19);
                    dictionary.Add("http://www.w3.org/2001/04/xmlenc#kw-aes192", type19);
                    dictionary.Add("http://www.w3.org/2001/04/xmlenc#aes256-cbc", type19);
                    dictionary.Add("http://www.w3.org/2001/04/xmlenc#kw-aes256", type19);
                    dictionary.Add("http://www.w3.org/TR/2001/REC-xml-c14n-20010315", "System.Security.Cryptography.Xml.XmlDsigC14NTransform, System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                    dictionary.Add("http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments", "System.Security.Cryptography.Xml.XmlDsigC14NWithCommentsTransform, System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                    dictionary.Add("http://www.w3.org/2001/10/xml-exc-c14n#", "System.Security.Cryptography.Xml.XmlDsigExcC14NTransform, System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                    dictionary.Add("http://www.w3.org/2001/10/xml-exc-c14n#WithComments", "System.Security.Cryptography.Xml.XmlDsigExcC14NWithCommentsTransform, System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                    dictionary.Add("http://www.w3.org/2000/09/xmldsig#base64", "System.Security.Cryptography.Xml.XmlDsigBase64Transform, System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                    dictionary.Add("http://www.w3.org/TR/1999/REC-xpath-19991116", "System.Security.Cryptography.Xml.XmlDsigXPathTransform, System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                    dictionary.Add("http://www.w3.org/TR/1999/REC-xslt-19991116", "System.Security.Cryptography.Xml.XmlDsigXsltTransform, System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                    dictionary.Add("http://www.w3.org/2000/09/xmldsig#enveloped-signature", "System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform, System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                    dictionary.Add("http://www.w3.org/2002/07/decrypt#XML", "System.Security.Cryptography.Xml.XmlDecryptionTransform, System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                    dictionary.Add("urn:mpeg:mpeg21:2003:01-REL-R-NS:licenseTransform", "System.Security.Cryptography.Xml.XmlLicenseTransform, System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                    dictionary.Add("http://www.w3.org/2000/09/xmldsig# X509Data", "System.Security.Cryptography.Xml.KeyInfoX509Data, System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                    dictionary.Add("http://www.w3.org/2000/09/xmldsig# KeyName", "System.Security.Cryptography.Xml.KeyInfoName, System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                    dictionary.Add("http://www.w3.org/2000/09/xmldsig# KeyValue/DSAKeyValue", "System.Security.Cryptography.Xml.DSAKeyValue, System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                    dictionary.Add("http://www.w3.org/2000/09/xmldsig# KeyValue/RSAKeyValue", "System.Security.Cryptography.Xml.RSAKeyValue, System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                    dictionary.Add("http://www.w3.org/2000/09/xmldsig# RetrievalMethod", "System.Security.Cryptography.Xml.KeyInfoRetrievalMethod, System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                    dictionary.Add("http://www.w3.org/2001/04/xmlenc# EncryptedKey", "System.Security.Cryptography.Xml.KeyInfoEncryptedKey, System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                    dictionary.Add("http://www.w3.org/2000/09/xmldsig#hmac-sha1", type9);
                    dictionary.Add("http://www.w3.org/2001/04/xmldsig-more#md5", type2);
                    dictionary.Add("http://www.w3.org/2001/04/xmldsig-more#sha384", type4);
                    dictionary.Add("http://www.w3.org/2001/04/xmldsig-more#hmac-md5", type7);
                    dictionary.Add("http://www.w3.org/2001/04/xmldsig-more#hmac-ripemd160", type8);
                    dictionary.Add("http://www.w3.org/2001/04/xmldsig-more#hmac-sha256", type10);
                    dictionary.Add("http://www.w3.org/2001/04/xmldsig-more#hmac-sha384", type11);
                    dictionary.Add("http://www.w3.org/2001/04/xmldsig-more#hmac-sha512", type12);
                    dictionary.Add("2.5.29.10", "System.Security.Cryptography.X509Certificates.X509BasicConstraintsExtension, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                    dictionary.Add("2.5.29.19", "System.Security.Cryptography.X509Certificates.X509BasicConstraintsExtension, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                    dictionary.Add("2.5.29.14", "System.Security.Cryptography.X509Certificates.X509SubjectKeyIdentifierExtension, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                    dictionary.Add("2.5.29.15", "System.Security.Cryptography.X509Certificates.X509KeyUsageExtension, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                    dictionary.Add("2.5.29.37", "System.Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                    dictionary.Add("X509Chain", "System.Security.Cryptography.X509Certificates.X509Chain, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
                    dictionary.Add("1.2.840.113549.1.9.3", "System.Security.Cryptography.Pkcs.Pkcs9ContentType, System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                    dictionary.Add("1.2.840.113549.1.9.4", "System.Security.Cryptography.Pkcs.Pkcs9MessageDigest, System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                    dictionary.Add("1.2.840.113549.1.9.5", "System.Security.Cryptography.Pkcs.Pkcs9SigningTime, System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                    dictionary.Add("1.3.6.1.4.1.311.88.2.1", "System.Security.Cryptography.Pkcs.Pkcs9DocumentName, System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                    dictionary.Add("1.3.6.1.4.1.311.88.2.2", "System.Security.Cryptography.Pkcs.Pkcs9DocumentDescription, System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                    defaultNameHT = dictionary;
                }
                return defaultNameHT;
            }
        }

        private static Dictionary<string, string> DefaultOidHT
        {
            get
            {
                if (defaultOidHT == null)
                {
                    Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    dictionary.Add("SHA", "1.3.14.3.2.26");
                    dictionary.Add("SHA1", "1.3.14.3.2.26");
                    dictionary.Add("System.Security.Cryptography.SHA1", "1.3.14.3.2.26");
                    dictionary.Add("System.Security.Cryptography.SHA1CryptoServiceProvider", "1.3.14.3.2.26");
                    dictionary.Add("System.Security.Cryptography.SHA1Managed", "1.3.14.3.2.26");
                    dictionary.Add("SHA256", "2.16.840.1.101.3.4.2.1");
                    dictionary.Add("System.Security.Cryptography.SHA256", "2.16.840.1.101.3.4.2.1");
                    dictionary.Add("System.Security.Cryptography.SHA256Managed", "2.16.840.1.101.3.4.2.1");
                    dictionary.Add("SHA384", "2.16.840.1.101.3.4.2.2");
                    dictionary.Add("System.Security.Cryptography.SHA384", "2.16.840.1.101.3.4.2.2");
                    dictionary.Add("System.Security.Cryptography.SHA384Managed", "2.16.840.1.101.3.4.2.2");
                    dictionary.Add("SHA512", "2.16.840.1.101.3.4.2.3");
                    dictionary.Add("System.Security.Cryptography.SHA512", "2.16.840.1.101.3.4.2.3");
                    dictionary.Add("System.Security.Cryptography.SHA512Managed", "2.16.840.1.101.3.4.2.3");
                    dictionary.Add("RIPEMD160", "1.3.36.3.2.1");
                    dictionary.Add("System.Security.Cryptography.RIPEMD160", "1.3.36.3.2.1");
                    dictionary.Add("System.Security.Cryptography.RIPEMD160Managed", "1.3.36.3.2.1");
                    dictionary.Add("MD5", "1.2.840.113549.2.5");
                    dictionary.Add("System.Security.Cryptography.MD5", "1.2.840.113549.2.5");
                    dictionary.Add("System.Security.Cryptography.MD5CryptoServiceProvider", "1.2.840.113549.2.5");
                    dictionary.Add("System.Security.Cryptography.MD5Managed", "1.2.840.113549.2.5");
                    dictionary.Add("TripleDESKeyWrap", "1.2.840.113549.1.9.16.3.6");
                    dictionary.Add("RC2", "1.2.840.113549.3.2");
                    dictionary.Add("System.Security.Cryptography.RC2CryptoServiceProvider", "1.2.840.113549.3.2");
                    dictionary.Add("DES", "1.3.14.3.2.7");
                    dictionary.Add("System.Security.Cryptography.DESCryptoServiceProvider", "1.3.14.3.2.7");
                    dictionary.Add("TripleDES", "1.2.840.113549.3.7");
                    dictionary.Add("System.Security.Cryptography.TripleDESCryptoServiceProvider", "1.2.840.113549.3.7");
                    defaultOidHT = dictionary;
                }
                return defaultOidHT;
            }
        }

        private static object InternalSyncObject
        {
            get
            {
                if (s_InternalSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref s_InternalSyncObject, obj2, null);
                }
                return s_InternalSyncObject;
            }
        }

        private static string Version
        {
            [SecurityCritical]
            get
            {
                if (version == null)
                {
                    version = ((RuntimeType) typeof(CryptoConfig)).GetRuntimeAssembly().GetVersion().ToString();
                }
                return version;
            }
        }
    }
}

