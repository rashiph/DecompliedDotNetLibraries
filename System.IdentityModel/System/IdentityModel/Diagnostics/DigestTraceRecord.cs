namespace System.IdentityModel.Diagnostics
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Diagnostics;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml;

    internal class DigestTraceRecord : TraceRecord
    {
        private HashAlgorithm _hash;
        private MemoryStream _logStream;
        private string _traceName;
        private const string CanonicalElementString = "CanonicalElementString";
        private const string CanonicalElementStringLength = "CanonicalElementStringLength";
        private const string CanonicalOctets = "CanonicalOctets";
        private const string CanonicalOctetsHash = "CanonicalOctetsHash";
        private const string CanonicalOctetsHashLength = "CanonicalOctetsHashLength";
        private const string CanonicalOctetsLength = "CanonicalOctetsLength";
        private const string Empty = "Empty";
        private const string FirstByte = "FirstByte";
        private const string Key = "Key";
        private const string LastByte = "LastByte";
        private const string Length = "Length";

        internal DigestTraceRecord(string traceName, MemoryStream logStream, HashAlgorithm hash)
        {
            if (string.IsNullOrEmpty(traceName))
            {
                this._traceName = "Empty";
            }
            else
            {
                this._traceName = traceName;
            }
            this._logStream = logStream;
            this._hash = hash;
        }

        internal override void WriteTo(XmlWriter writer)
        {
            base.WriteTo(writer);
            byte[] bytes = this._logStream.GetBuffer();
            string str = Encoding.UTF8.GetString(bytes, 0, (int) this._logStream.Length);
            writer.WriteElementString("CanonicalElementStringLength", str.Length.ToString(CultureInfo.InvariantCulture));
            writer.WriteComment("CanonicalElementString:" + str);
            writer.WriteElementString("CanonicalOctetsLength", bytes.Length.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("CanonicalOctets", Convert.ToBase64String(bytes));
            writer.WriteElementString("CanonicalOctetsHashLength", this._hash.Hash.Length.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("CanonicalOctetsHash", Convert.ToBase64String(this._hash.Hash));
            if (this._hash is KeyedHashAlgorithm)
            {
                KeyedHashAlgorithm algorithm = this._hash as KeyedHashAlgorithm;
                byte[] key = algorithm.Key;
                writer.WriteStartElement("Key");
                writer.WriteElementString("Length", key.Length.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("FirstByte", key[0].ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("LastByte", key[key.Length - 1].ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();
            }
        }

        internal override string EventId
        {
            get
            {
                return ("http://schemas.microsoft.com/2006/08/ServiceModel/" + this._traceName + "TraceRecord");
            }
        }
    }
}

