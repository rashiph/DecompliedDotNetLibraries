namespace System.Runtime.Serialization
{
    using System;
    using System.Security;
    using System.Xml;

    internal class Attributes
    {
        internal int ArraySZSize;
        internal string ClrAssembly;
        internal string ClrType;
        internal string FactoryTypeName;
        internal string FactoryTypeNamespace;
        internal string FactoryTypePrefix;
        internal string Id;
        internal string Ref;
        [SecurityCritical]
        private static XmlDictionaryString[] schemaInstanceLocalNames = new XmlDictionaryString[] { DictionaryGlobals.XsiNilLocalName, DictionaryGlobals.XsiTypeLocalName };
        [SecurityCritical]
        private static XmlDictionaryString[] serializationLocalNames = new XmlDictionaryString[] { DictionaryGlobals.IdLocalName, DictionaryGlobals.ArraySizeLocalName, DictionaryGlobals.RefLocalName, DictionaryGlobals.ClrTypeLocalName, DictionaryGlobals.ClrAssemblyLocalName, DictionaryGlobals.ISerializableFactoryTypeLocalName };
        internal bool UnrecognizedAttributesFound;
        internal bool XsiNil;
        internal string XsiTypeName;
        internal string XsiTypeNamespace;
        internal string XsiTypePrefix;

        [SecuritySafeCritical]
        internal void Read(XmlReaderDelegator reader)
        {
            this.Reset();
            while (reader.MoveToNextAttribute())
            {
                switch (reader.IndexOfLocalName(serializationLocalNames, DictionaryGlobals.SerializationNamespace))
                {
                    case 0:
                    {
                        this.ReadId(reader);
                        continue;
                    }
                    case 1:
                    {
                        this.ReadArraySize(reader);
                        continue;
                    }
                    case 2:
                    {
                        this.ReadRef(reader);
                        continue;
                    }
                    case 3:
                    {
                        this.ClrType = reader.Value;
                        continue;
                    }
                    case 4:
                    {
                        this.ClrAssembly = reader.Value;
                        continue;
                    }
                    case 5:
                    {
                        this.ReadFactoryType(reader);
                        continue;
                    }
                }
                switch (reader.IndexOfLocalName(schemaInstanceLocalNames, DictionaryGlobals.SchemaInstanceNamespace))
                {
                    case 0:
                    {
                        this.ReadXsiNil(reader);
                        continue;
                    }
                    case 1:
                    {
                        this.ReadXsiType(reader);
                        continue;
                    }
                }
                if (!reader.IsNamespaceUri(DictionaryGlobals.XmlnsNamespace))
                {
                    this.UnrecognizedAttributesFound = true;
                }
            }
            reader.MoveToElement();
        }

        private void ReadArraySize(XmlReaderDelegator reader)
        {
            this.ArraySZSize = reader.ReadContentAsInt();
            if (this.ArraySZSize < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("InvalidSizeDefinition", new object[] { this.ArraySZSize })));
            }
        }

        private void ReadFactoryType(XmlReaderDelegator reader)
        {
            string qname = reader.Value;
            if ((qname != null) && (qname.Length > 0))
            {
                XmlObjectSerializerReadContext.ParseQualifiedName(qname, reader, out this.FactoryTypeName, out this.FactoryTypeNamespace, out this.FactoryTypePrefix);
            }
        }

        private void ReadId(XmlReaderDelegator reader)
        {
            this.Id = reader.ReadContentAsString();
            if (string.IsNullOrEmpty(this.Id))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("InvalidXsIdDefinition", new object[] { this.Id })));
            }
        }

        private void ReadRef(XmlReaderDelegator reader)
        {
            this.Ref = reader.ReadContentAsString();
            if (string.IsNullOrEmpty(this.Ref))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(System.Runtime.Serialization.SR.GetString("InvalidXsRefDefinition", new object[] { this.Ref })));
            }
        }

        private void ReadXsiNil(XmlReaderDelegator reader)
        {
            this.XsiNil = reader.ReadContentAsBoolean();
        }

        private void ReadXsiType(XmlReaderDelegator reader)
        {
            string qname = reader.Value;
            if ((qname != null) && (qname.Length > 0))
            {
                XmlObjectSerializerReadContext.ParseQualifiedName(qname, reader, out this.XsiTypeName, out this.XsiTypeNamespace, out this.XsiTypePrefix);
            }
        }

        internal void Reset()
        {
            this.Id = Globals.NewObjectId;
            this.Ref = Globals.NewObjectId;
            this.XsiTypeName = null;
            this.XsiTypeNamespace = null;
            this.XsiTypePrefix = null;
            this.XsiNil = false;
            this.ClrAssembly = null;
            this.ClrType = null;
            this.ArraySZSize = -1;
            this.FactoryTypeName = null;
            this.FactoryTypeNamespace = null;
            this.FactoryTypePrefix = null;
            this.UnrecognizedAttributesFound = false;
        }
    }
}

