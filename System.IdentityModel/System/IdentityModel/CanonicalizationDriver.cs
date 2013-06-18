namespace System.IdentityModel
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Xml;

    internal sealed class CanonicalizationDriver
    {
        private bool closeReadersAfterProcessing;
        private bool includeComments;
        private string[] inclusivePrefixes;
        private XmlReader reader;

        public byte[] GetBytes()
        {
            return this.GetMemoryStream().ToArray();
        }

        public string[] GetInclusivePrefixes()
        {
            return this.inclusivePrefixes;
        }

        public MemoryStream GetMemoryStream()
        {
            MemoryStream canonicalStream = new MemoryStream();
            this.WriteTo(canonicalStream);
            canonicalStream.Seek(0L, SeekOrigin.Begin);
            return canonicalStream;
        }

        public void Reset()
        {
            this.reader = null;
        }

        public void SetInclusivePrefixes(string[] inclusivePrefixes)
        {
            this.inclusivePrefixes = inclusivePrefixes;
        }

        public void SetInput(Stream stream)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }
            this.reader = XmlReader.Create(stream);
        }

        public void SetInput(XmlReader reader)
        {
            if (reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("reader");
            }
            this.reader = reader;
        }

        public void WriteTo(Stream canonicalStream)
        {
            if (this.reader == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("NoInputIsSetForCanonicalization")));
            }
            XmlDictionaryReader reader = this.reader as XmlDictionaryReader;
            if ((reader != null) && reader.CanCanonicalize)
            {
                reader.MoveToContent();
                reader.StartCanonicalization(canonicalStream, this.includeComments, this.inclusivePrefixes);
                reader.Skip();
                reader.EndCanonicalization();
            }
            else
            {
                XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(Stream.Null);
                if (this.inclusivePrefixes != null)
                {
                    writer.WriteStartElement("a", this.reader.LookupNamespace(string.Empty));
                    for (int i = 0; i < this.inclusivePrefixes.Length; i++)
                    {
                        string namespaceUri = this.reader.LookupNamespace(this.inclusivePrefixes[i]);
                        if (namespaceUri != null)
                        {
                            writer.WriteXmlnsAttribute(this.inclusivePrefixes[i], namespaceUri);
                        }
                    }
                }
                writer.StartCanonicalization(canonicalStream, this.includeComments, this.inclusivePrefixes);
                writer.WriteNode(this.reader, false);
                writer.Flush();
                writer.EndCanonicalization();
                if (this.inclusivePrefixes != null)
                {
                    writer.WriteEndElement();
                }
                writer.Close();
            }
            if (this.closeReadersAfterProcessing)
            {
                this.reader.Close();
            }
            this.reader = null;
        }

        public void WriteTo(HashAlgorithm hashAlgorithm)
        {
            this.WriteTo(new HashStream(hashAlgorithm));
        }

        public bool CloseReadersAfterProcessing
        {
            get
            {
                return this.closeReadersAfterProcessing;
            }
            set
            {
                this.closeReadersAfterProcessing = value;
            }
        }

        public bool IncludeComments
        {
            get
            {
                return this.includeComments;
            }
            set
            {
                this.includeComments = value;
            }
        }
    }
}

