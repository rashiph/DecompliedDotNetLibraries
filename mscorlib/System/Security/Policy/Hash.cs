namespace System.Security.Policy
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Util;

    [Serializable, ComVisible(true)]
    public sealed class Hash : EvidenceBase, ISerializable
    {
        private RuntimeAssembly m_assembly;
        private Dictionary<Type, byte[]> m_hashes;
        private WeakReference m_rawData;

        public Hash(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            if (assembly.IsDynamic)
            {
                throw new ArgumentException(Environment.GetResourceString("Security_CannotGenerateHash"), "assembly");
            }
            this.m_hashes = new Dictionary<Type, byte[]>();
            this.m_assembly = assembly as RuntimeAssembly;
            if (this.m_assembly == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeAssembly"), "assembly");
            }
        }

        private Hash(Hash hash)
        {
            this.m_assembly = hash.m_assembly;
            this.m_rawData = hash.m_rawData;
            this.m_hashes = new Dictionary<Type, byte[]>(hash.m_hashes);
        }

        [SecurityCritical]
        internal Hash(SerializationInfo info, StreamingContext context)
        {
            Dictionary<Type, byte[]> valueNoThrow = info.GetValueNoThrow("Hashes", typeof(Dictionary<Type, byte[]>)) as Dictionary<Type, byte[]>;
            if (valueNoThrow != null)
            {
                this.m_hashes = valueNoThrow;
            }
            else
            {
                this.m_hashes = new Dictionary<Type, byte[]>();
                byte[] buffer = info.GetValueNoThrow("Md5", typeof(byte[])) as byte[];
                if (buffer != null)
                {
                    this.m_hashes[typeof(System.Security.Cryptography.MD5)] = buffer;
                }
                byte[] buffer2 = info.GetValueNoThrow("Sha1", typeof(byte[])) as byte[];
                if (buffer2 != null)
                {
                    this.m_hashes[typeof(System.Security.Cryptography.SHA1)] = buffer2;
                }
                byte[] assemblyBytes = info.GetValueNoThrow("RawData", typeof(byte[])) as byte[];
                if (assemblyBytes != null)
                {
                    this.GenerateDefaultHashes(assemblyBytes);
                }
            }
        }

        private Hash(Type hashType, byte[] hashValue)
        {
            this.m_hashes = new Dictionary<Type, byte[]>();
            byte[] destinationArray = new byte[hashValue.Length];
            Array.Copy(hashValue, destinationArray, destinationArray.Length);
            this.m_hashes[hashType] = hashValue;
        }

        public override EvidenceBase Clone()
        {
            return new Hash(this);
        }

        public static Hash CreateMD5(byte[] md5)
        {
            if (md5 == null)
            {
                throw new ArgumentNullException("md5");
            }
            return new Hash(typeof(System.Security.Cryptography.MD5), md5);
        }

        public static Hash CreateSHA1(byte[] sha1)
        {
            if (sha1 == null)
            {
                throw new ArgumentNullException("sha1");
            }
            return new Hash(typeof(System.Security.Cryptography.SHA1), sha1);
        }

        public static Hash CreateSHA256(byte[] sha256)
        {
            if (sha256 == null)
            {
                throw new ArgumentNullException("sha256");
            }
            return new Hash(typeof(System.Security.Cryptography.SHA256), sha256);
        }

        private void GenerateDefaultHashes()
        {
            if (this.m_assembly != null)
            {
                this.GenerateDefaultHashes(this.GetRawData());
            }
        }

        private void GenerateDefaultHashes(byte[] assemblyBytes)
        {
            Type[] typeArray = new Type[] { GetHashIndexType(typeof(System.Security.Cryptography.SHA1)), GetHashIndexType(typeof(System.Security.Cryptography.SHA256)), GetHashIndexType(typeof(System.Security.Cryptography.MD5)) };
            foreach (Type type in typeArray)
            {
                Type defaultHashImplementation = GetDefaultHashImplementation(type);
                if ((defaultHashImplementation != null) && !this.m_hashes.ContainsKey(type))
                {
                    this.m_hashes[type] = GenerateHash(defaultHashImplementation, assemblyBytes);
                }
            }
        }

        public byte[] GenerateHash(HashAlgorithm hashAlg)
        {
            if (hashAlg == null)
            {
                throw new ArgumentNullException("hashAlg");
            }
            byte[] sourceArray = this.GenerateHash(hashAlg.GetType());
            byte[] destinationArray = new byte[sourceArray.Length];
            Array.Copy(sourceArray, destinationArray, destinationArray.Length);
            return destinationArray;
        }

        private byte[] GenerateHash(Type hashType)
        {
            Type hashIndexType = GetHashIndexType(hashType);
            byte[] buffer = null;
            if (!this.m_hashes.TryGetValue(hashIndexType, out buffer))
            {
                if (this.m_assembly == null)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("Security_CannotGenerateHash"));
                }
                buffer = GenerateHash(hashType, this.GetRawData());
                this.m_hashes[hashIndexType] = buffer;
            }
            return buffer;
        }

        private static byte[] GenerateHash(Type hashType, byte[] assemblyBytes)
        {
            using (HashAlgorithm algorithm = HashAlgorithm.Create(hashType.FullName))
            {
                return algorithm.ComputeHash(assemblyBytes);
            }
        }

        private static Type GetDefaultHashImplementation(Type hashAlgorithm)
        {
            if (hashAlgorithm.IsAssignableFrom(typeof(System.Security.Cryptography.MD5)))
            {
                if (!CryptoConfig.AllowOnlyFipsAlgorithms)
                {
                    return typeof(MD5CryptoServiceProvider);
                }
                return null;
            }
            if (!hashAlgorithm.IsAssignableFrom(typeof(System.Security.Cryptography.SHA256)))
            {
                return hashAlgorithm;
            }
            Version version = Environment.OSVersion.Version;
            if (Environment.RunningOnWinNT && ((version.Major > 5) || ((version.Major == 5) && (version.Minor >= 2))))
            {
                return Type.GetType("System.Security.Cryptography.SHA256CryptoServiceProvider, System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            }
            if (!CryptoConfig.AllowOnlyFipsAlgorithms)
            {
                return typeof(SHA256Managed);
            }
            return null;
        }

        private static Type GetDefaultHashImplementationOrFallback(Type hashAlgorithm, Type fallbackImplementation)
        {
            Type defaultHashImplementation = GetDefaultHashImplementation(hashAlgorithm);
            if (defaultHashImplementation == null)
            {
                return fallbackImplementation;
            }
            return defaultHashImplementation;
        }

        private static Type GetHashIndexType(Type hashType)
        {
            Type baseType = hashType;
            while ((baseType != null) && (baseType.BaseType != typeof(HashAlgorithm)))
            {
                baseType = baseType.BaseType;
            }
            if (baseType == null)
            {
                baseType = typeof(HashAlgorithm);
            }
            return baseType;
        }

        [SecurityCritical]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            byte[] buffer;
            byte[] buffer2;
            this.GenerateDefaultHashes();
            if (this.m_hashes.TryGetValue(typeof(System.Security.Cryptography.MD5), out buffer2))
            {
                info.AddValue("Md5", buffer2);
            }
            if (this.m_hashes.TryGetValue(typeof(System.Security.Cryptography.SHA1), out buffer))
            {
                info.AddValue("Sha1", buffer);
            }
            info.AddValue("RawData", null);
            info.AddValue("PEFile", IntPtr.Zero);
            info.AddValue("Hashes", this.m_hashes);
        }

        private byte[] GetRawData()
        {
            byte[] target = null;
            if (this.m_assembly != null)
            {
                if (this.m_rawData != null)
                {
                    target = this.m_rawData.Target as byte[];
                }
                if (target == null)
                {
                    target = this.m_assembly.GetRawBytes();
                    this.m_rawData = new WeakReference(target);
                }
            }
            return target;
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext ctx)
        {
            this.GenerateDefaultHashes();
        }

        public override string ToString()
        {
            return this.ToXml().ToString();
        }

        private SecurityElement ToXml()
        {
            this.GenerateDefaultHashes();
            SecurityElement element = new SecurityElement("System.Security.Policy.Hash");
            element.AddAttribute("version", "2");
            foreach (KeyValuePair<Type, byte[]> pair in this.m_hashes)
            {
                SecurityElement child = new SecurityElement("hash");
                child.AddAttribute("algorithm", pair.Key.Name);
                child.AddAttribute("value", Hex.EncodeHexString(pair.Value));
                element.AddChild(child);
            }
            return element;
        }

        public byte[] MD5
        {
            get
            {
                byte[] buffer = null;
                if (!this.m_hashes.TryGetValue(typeof(System.Security.Cryptography.MD5), out buffer))
                {
                    buffer = this.GenerateHash(GetDefaultHashImplementationOrFallback(typeof(System.Security.Cryptography.MD5), typeof(System.Security.Cryptography.MD5)));
                }
                byte[] destinationArray = new byte[buffer.Length];
                Array.Copy(buffer, destinationArray, destinationArray.Length);
                return destinationArray;
            }
        }

        public byte[] SHA1
        {
            get
            {
                byte[] buffer = null;
                if (!this.m_hashes.TryGetValue(typeof(System.Security.Cryptography.SHA1), out buffer))
                {
                    buffer = this.GenerateHash(GetDefaultHashImplementationOrFallback(typeof(System.Security.Cryptography.SHA1), typeof(System.Security.Cryptography.SHA1)));
                }
                byte[] destinationArray = new byte[buffer.Length];
                Array.Copy(buffer, destinationArray, destinationArray.Length);
                return destinationArray;
            }
        }

        public byte[] SHA256
        {
            get
            {
                byte[] buffer = null;
                if (!this.m_hashes.TryGetValue(typeof(System.Security.Cryptography.SHA256), out buffer))
                {
                    buffer = this.GenerateHash(GetDefaultHashImplementationOrFallback(typeof(System.Security.Cryptography.SHA256), typeof(System.Security.Cryptography.SHA256)));
                }
                byte[] destinationArray = new byte[buffer.Length];
                Array.Copy(buffer, destinationArray, destinationArray.Length);
                return destinationArray;
            }
        }
    }
}

