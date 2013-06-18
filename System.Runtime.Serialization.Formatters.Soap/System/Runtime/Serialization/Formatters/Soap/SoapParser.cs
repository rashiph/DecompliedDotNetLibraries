namespace System.Runtime.Serialization.Formatters.Soap
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Xml;

    internal sealed class SoapParser : ISerParser
    {
        private bool bDebug;
        internal bool bStop;
        private int depth;
        internal ObjectReader objectReader;
        internal SoapHandler soapHandler;
        private TextReader textReader;
        internal XmlTextReader xmlReader;

        internal SoapParser(Stream stream)
        {
            if (this.bDebug)
            {
                this.xmlReader = new XmlTextReader(this.textReader);
            }
            else
            {
                this.xmlReader = new XmlTextReader(stream);
            }
            this.xmlReader.XmlResolver = null;
            this.xmlReader.DtdProcessing = DtdProcessing.Prohibit;
            this.soapHandler = new SoapHandler(this);
        }

        [Conditional("SER_LOGGING")]
        private static void Dump(string name, XmlReader xmlReader)
        {
        }

        internal void Init(ObjectReader objectReader)
        {
            this.objectReader = objectReader;
            this.soapHandler.Init(objectReader);
            this.bStop = false;
            this.depth = 0;
            this.xmlReader.ResetState();
        }

        private void ParseXml()
        {
            while (!this.bStop && this.xmlReader.Read())
            {
                if (this.depth < this.xmlReader.Depth)
                {
                    this.soapHandler.StartChildren();
                    this.depth = this.xmlReader.Depth;
                }
                else if (this.depth > this.xmlReader.Depth)
                {
                    this.soapHandler.FinishChildren(this.xmlReader.Prefix, this.xmlReader.LocalName, this.xmlReader.NamespaceURI);
                    this.depth = this.xmlReader.Depth;
                }
                switch (this.xmlReader.NodeType)
                {
                    case XmlNodeType.None:
                    case XmlNodeType.Attribute:
                    case XmlNodeType.EntityReference:
                    case XmlNodeType.Entity:
                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.Document:
                    case XmlNodeType.DocumentType:
                    case XmlNodeType.DocumentFragment:
                    case XmlNodeType.Notation:
                    case XmlNodeType.EndEntity:
                    {
                        continue;
                    }
                    case XmlNodeType.Element:
                    {
                        this.soapHandler.StartElement(this.xmlReader.Prefix, this.xmlReader.LocalName, this.xmlReader.NamespaceURI);
                        int attributeCount = this.xmlReader.AttributeCount;
                        goto Label_0152;
                    }
                    case XmlNodeType.Text:
                    {
                        this.soapHandler.Text(this.xmlReader.Value);
                        continue;
                    }
                    case XmlNodeType.CDATA:
                    {
                        this.soapHandler.Text(this.xmlReader.Value);
                        continue;
                    }
                    case XmlNodeType.Comment:
                    {
                        this.soapHandler.Comment(this.xmlReader.Value);
                        continue;
                    }
                    case XmlNodeType.Whitespace:
                    {
                        this.soapHandler.Text(this.xmlReader.Value);
                        continue;
                    }
                    case XmlNodeType.SignificantWhitespace:
                    {
                        this.soapHandler.Text(this.xmlReader.Value);
                        continue;
                    }
                    case XmlNodeType.EndElement:
                    {
                        this.soapHandler.EndElement(this.xmlReader.Prefix, this.xmlReader.LocalName, this.xmlReader.NamespaceURI);
                        continue;
                    }
                    default:
                    {
                        continue;
                    }
                }
            Label_011B:
                this.soapHandler.Attribute(this.xmlReader.Prefix, this.xmlReader.LocalName, this.xmlReader.NamespaceURI, this.xmlReader.Value);
            Label_0152:
                if (this.xmlReader.MoveToNextAttribute())
                {
                    goto Label_011B;
                }
                this.xmlReader.MoveToElement();
                if (this.xmlReader.IsEmptyElement)
                {
                    this.soapHandler.EndElement(this.xmlReader.Prefix, this.xmlReader.LocalName, this.xmlReader.NamespaceURI);
                }
            }
        }

        public void Run()
        {
            try
            {
                this.soapHandler.Start(this.xmlReader);
                this.ParseXml();
            }
            catch (EndOfStreamException)
            {
            }
        }

        internal void Stop()
        {
            this.bStop = true;
        }

        [Conditional("_LOGGING")]
        private void TraceStream(Stream stream)
        {
            this.bDebug = true;
            string s = new StreamReader(stream).ReadToEnd();
            this.textReader = new StringReader(s);
        }
    }
}

