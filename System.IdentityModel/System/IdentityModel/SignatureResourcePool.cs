namespace System.IdentityModel
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml;

    internal sealed class SignatureResourcePool
    {
        private char[] base64Buffer;
        private const int BufferSize = 0x40;
        private CanonicalizationDriver canonicalizationDriver;
        private byte[] encodingBuffer;
        private HashAlgorithm hashAlgorithm;
        private HashStream hashStream;
        private XmlDictionaryWriter utf8Writer;

        public char[] TakeBase64Buffer()
        {
            if (this.base64Buffer == null)
            {
                this.base64Buffer = new char[0x40];
            }
            return this.base64Buffer;
        }

        public CanonicalizationDriver TakeCanonicalizationDriver()
        {
            if (this.canonicalizationDriver == null)
            {
                this.canonicalizationDriver = new CanonicalizationDriver();
            }
            else
            {
                this.canonicalizationDriver.Reset();
            }
            return this.canonicalizationDriver;
        }

        public byte[] TakeEncodingBuffer()
        {
            if (this.encodingBuffer == null)
            {
                this.encodingBuffer = new byte[0x40];
            }
            return this.encodingBuffer;
        }

        public HashAlgorithm TakeHashAlgorithm(string algorithm)
        {
            if (this.hashAlgorithm == null)
            {
                if (string.IsNullOrEmpty(algorithm))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(algorithm, System.IdentityModel.SR.GetString("EmptyOrNullArgumentString", new object[] { "algorithm" }));
                }
                this.hashAlgorithm = CryptoHelper.CreateHashAlgorithm(algorithm);
            }
            else
            {
                this.hashAlgorithm.Initialize();
            }
            return this.hashAlgorithm;
        }

        public HashStream TakeHashStream(HashAlgorithm hash)
        {
            if (this.hashStream == null)
            {
                this.hashStream = new HashStream(hash);
            }
            else
            {
                this.hashStream.Reset(hash);
            }
            return this.hashStream;
        }

        public HashStream TakeHashStream(string algorithm)
        {
            return this.TakeHashStream(this.TakeHashAlgorithm(algorithm));
        }

        public XmlDictionaryWriter TakeUtf8Writer()
        {
            if (this.utf8Writer == null)
            {
                this.utf8Writer = XmlDictionaryWriter.CreateTextWriter(Stream.Null, Encoding.UTF8, false);
            }
            else
            {
                ((IXmlTextWriterInitializer) this.utf8Writer).SetOutput(Stream.Null, Encoding.UTF8, false);
            }
            return this.utf8Writer;
        }
    }
}

