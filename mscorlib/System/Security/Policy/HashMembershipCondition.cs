namespace System.Security.Policy
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Util;
    using System.Threading;

    [Serializable, ComVisible(true)]
    public sealed class HashMembershipCondition : ISerializable, IDeserializationCallback, IReportMatchMembershipCondition, IMembershipCondition, ISecurityEncodable, ISecurityPolicyEncodable
    {
        private SecurityElement m_element;
        private System.Security.Cryptography.HashAlgorithm m_hashAlg;
        private byte[] m_value;
        private object s_InternalSyncObject;
        private const string s_tagHashAlgorithm = "HashAlgorithm";
        private const string s_tagHashValue = "HashValue";

        internal HashMembershipCondition()
        {
        }

        private HashMembershipCondition(SerializationInfo info, StreamingContext context)
        {
            this.m_value = (byte[]) info.GetValue("HashValue", typeof(byte[]));
            string hashName = (string) info.GetValue("HashAlgorithm", typeof(string));
            if (hashName != null)
            {
                this.m_hashAlg = System.Security.Cryptography.HashAlgorithm.Create(hashName);
            }
            else
            {
                this.m_hashAlg = new SHA1Managed();
            }
        }

        public HashMembershipCondition(System.Security.Cryptography.HashAlgorithm hashAlg, byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (hashAlg == null)
            {
                throw new ArgumentNullException("hashAlg");
            }
            this.m_value = new byte[value.Length];
            Array.Copy(value, this.m_value, value.Length);
            this.m_hashAlg = hashAlg;
        }

        public bool Check(Evidence evidence)
        {
            object usedEvidence = null;
            return ((IReportMatchMembershipCondition) this).Check(evidence, out usedEvidence);
        }

        private static bool CompareArrays(byte[] first, byte[] second)
        {
            if (first.Length != second.Length)
            {
                return false;
            }
            int length = first.Length;
            for (int i = 0; i < length; i++)
            {
                if (first[i] != second[i])
                {
                    return false;
                }
            }
            return true;
        }

        [SecuritySafeCritical]
        public IMembershipCondition Copy()
        {
            if ((this.m_value == null) && (this.m_element != null))
            {
                this.ParseHashValue();
            }
            if ((this.m_hashAlg == null) && (this.m_element != null))
            {
                this.ParseHashAlgorithm();
            }
            return new HashMembershipCondition(this.m_hashAlg, this.m_value);
        }

        [SecuritySafeCritical]
        public override bool Equals(object o)
        {
            HashMembershipCondition condition = o as HashMembershipCondition;
            if (condition != null)
            {
                if ((this.m_hashAlg == null) && (this.m_element != null))
                {
                    this.ParseHashAlgorithm();
                }
                if ((condition.m_hashAlg == null) && (condition.m_element != null))
                {
                    condition.ParseHashAlgorithm();
                }
                if (((this.m_hashAlg != null) && (condition.m_hashAlg != null)) && (this.m_hashAlg.GetType() == condition.m_hashAlg.GetType()))
                {
                    if ((this.m_value == null) && (this.m_element != null))
                    {
                        this.ParseHashValue();
                    }
                    if ((condition.m_value == null) && (condition.m_element != null))
                    {
                        condition.ParseHashValue();
                    }
                    if (this.m_value.Length != condition.m_value.Length)
                    {
                        return false;
                    }
                    for (int i = 0; i < this.m_value.Length; i++)
                    {
                        if (this.m_value[i] != condition.m_value[i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public void FromXml(SecurityElement e)
        {
            this.FromXml(e, null);
        }

        public void FromXml(SecurityElement e, PolicyLevel level)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            if (!e.Tag.Equals("IMembershipCondition"))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MembershipConditionElement"));
            }
            lock (this.InternalSyncObject)
            {
                this.m_element = e;
                this.m_value = null;
                this.m_hashAlg = null;
            }
        }

        private static int GetByteArrayHashCode(byte[] baData)
        {
            if (baData == null)
            {
                return 0;
            }
            int num = 0;
            for (int i = 0; i < baData.Length; i++)
            {
                num = ((num << 8) ^ baData[i]) ^ (num >> 0x18);
            }
            return num;
        }

        [SecuritySafeCritical]
        public override int GetHashCode()
        {
            if ((this.m_hashAlg == null) && (this.m_element != null))
            {
                this.ParseHashAlgorithm();
            }
            int num = (this.m_hashAlg != null) ? this.m_hashAlg.GetType().GetHashCode() : 0;
            if ((this.m_value == null) && (this.m_element != null))
            {
                this.ParseHashValue();
            }
            return (num ^ GetByteArrayHashCode(this.m_value));
        }

        private void ParseHashAlgorithm()
        {
            lock (this.InternalSyncObject)
            {
                if (this.m_element != null)
                {
                    string hashName = this.m_element.Attribute("HashAlgorithm");
                    if (hashName != null)
                    {
                        this.m_hashAlg = System.Security.Cryptography.HashAlgorithm.Create(hashName);
                    }
                    else
                    {
                        this.m_hashAlg = new SHA1Managed();
                    }
                    if ((this.m_value != null) && (this.m_hashAlg != null))
                    {
                        this.m_element = null;
                    }
                }
            }
        }

        private void ParseHashValue()
        {
            lock (this.InternalSyncObject)
            {
                if (this.m_element != null)
                {
                    string hexString = this.m_element.Attribute("HashValue");
                    if (hexString == null)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_InvalidXMLElement", new object[] { "HashValue", base.GetType().FullName }));
                    }
                    this.m_value = Hex.DecodeHexString(hexString);
                    if ((this.m_value != null) && (this.m_hashAlg != null))
                    {
                        this.m_element = null;
                    }
                }
            }
        }

        void IDeserializationCallback.OnDeserialization(object sender)
        {
        }

        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("HashValue", this.HashValue);
            info.AddValue("HashAlgorithm", this.HashAlgorithm.ToString());
        }

        bool IReportMatchMembershipCondition.Check(Evidence evidence, out object usedEvidence)
        {
            usedEvidence = null;
            if (evidence != null)
            {
                Hash hostEvidence = evidence.GetHostEvidence<Hash>();
                if (hostEvidence != null)
                {
                    if ((this.m_value == null) && (this.m_element != null))
                    {
                        this.ParseHashValue();
                    }
                    if ((this.m_hashAlg == null) && (this.m_element != null))
                    {
                        this.ParseHashAlgorithm();
                    }
                    byte[] first = null;
                    lock (this.InternalSyncObject)
                    {
                        first = hostEvidence.GenerateHash(this.m_hashAlg);
                    }
                    if ((first != null) && CompareArrays(first, this.m_value))
                    {
                        usedEvidence = hostEvidence;
                        return true;
                    }
                }
            }
            return false;
        }

        [SecuritySafeCritical]
        public override string ToString()
        {
            if (this.m_hashAlg == null)
            {
                this.ParseHashAlgorithm();
            }
            return Environment.GetResourceString("Hash_ToString", new object[] { this.m_hashAlg.GetType().AssemblyQualifiedName, Hex.EncodeHexString(this.HashValue) });
        }

        [SecuritySafeCritical]
        public SecurityElement ToXml()
        {
            return this.ToXml(null);
        }

        [SecuritySafeCritical]
        public SecurityElement ToXml(PolicyLevel level)
        {
            if ((this.m_value == null) && (this.m_element != null))
            {
                this.ParseHashValue();
            }
            if ((this.m_hashAlg == null) && (this.m_element != null))
            {
                this.ParseHashAlgorithm();
            }
            SecurityElement element = new SecurityElement("IMembershipCondition");
            XMLUtil.AddClassAttribute(element, base.GetType(), "System.Security.Policy.HashMembershipCondition");
            element.AddAttribute("version", "1");
            if (this.m_value != null)
            {
                element.AddAttribute("HashValue", Hex.EncodeHexString(this.HashValue));
            }
            if (this.m_hashAlg != null)
            {
                element.AddAttribute("HashAlgorithm", this.HashAlgorithm.GetType().FullName);
            }
            return element;
        }

        public System.Security.Cryptography.HashAlgorithm HashAlgorithm
        {
            [SecuritySafeCritical]
            get
            {
                if ((this.m_hashAlg == null) && (this.m_element != null))
                {
                    this.ParseHashAlgorithm();
                }
                return this.m_hashAlg;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("HashAlgorithm");
                }
                this.m_hashAlg = value;
            }
        }

        public byte[] HashValue
        {
            get
            {
                if ((this.m_value == null) && (this.m_element != null))
                {
                    this.ParseHashValue();
                }
                if (this.m_value == null)
                {
                    return null;
                }
                byte[] destinationArray = new byte[this.m_value.Length];
                Array.Copy(this.m_value, destinationArray, this.m_value.Length);
                return destinationArray;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.m_value = new byte[value.Length];
                Array.Copy(value, this.m_value, value.Length);
            }
        }

        private object InternalSyncObject
        {
            get
            {
                if (this.s_InternalSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref this.s_InternalSyncObject, obj2, null);
                }
                return this.s_InternalSyncObject;
            }
        }
    }
}

