namespace System.Web.Services.Discovery
{
    using System;
    using System.Collections;
    using System.Xml;
    using System.Xml.Serialization;

    internal class DiscoveryDocumentSerializationReader : XmlSerializationReader
    {
        private string id1_discovery;
        private string id10_Item;
        private string id11_SoapBinding;
        private string id12_address;
        private string id13_Item;
        private string id14_binding;
        private string id15_SchemaReference;
        private string id16_ref;
        private string id17_targetNamespace;
        private string id18_ContractReference;
        private string id19_docRef;
        private string id2_Item;
        private string id20_DiscoveryDocumentReference;
        private string id3_DiscoveryDocument;
        private string id4_discoveryRef;
        private string id5_contractRef;
        private string id6_Item;
        private string id7_schemaRef;
        private string id8_Item;
        private string id9_soap;

        protected override void InitCallbacks()
        {
        }

        protected override void InitIDs()
        {
            this.id1_discovery = base.Reader.NameTable.Add("discovery");
            this.id4_discoveryRef = base.Reader.NameTable.Add("discoveryRef");
            this.id19_docRef = base.Reader.NameTable.Add("docRef");
            this.id8_Item = base.Reader.NameTable.Add("http://schemas.xmlsoap.org/disco/schema/");
            this.id14_binding = base.Reader.NameTable.Add("binding");
            this.id20_DiscoveryDocumentReference = base.Reader.NameTable.Add("DiscoveryDocumentReference");
            this.id17_targetNamespace = base.Reader.NameTable.Add("targetNamespace");
            this.id5_contractRef = base.Reader.NameTable.Add("contractRef");
            this.id10_Item = base.Reader.NameTable.Add("http://schemas.xmlsoap.org/disco/soap/");
            this.id13_Item = base.Reader.NameTable.Add("");
            this.id7_schemaRef = base.Reader.NameTable.Add("schemaRef");
            this.id3_DiscoveryDocument = base.Reader.NameTable.Add("DiscoveryDocument");
            this.id9_soap = base.Reader.NameTable.Add("soap");
            this.id12_address = base.Reader.NameTable.Add("address");
            this.id16_ref = base.Reader.NameTable.Add("ref");
            this.id11_SoapBinding = base.Reader.NameTable.Add("SoapBinding");
            this.id18_ContractReference = base.Reader.NameTable.Add("ContractReference");
            this.id2_Item = base.Reader.NameTable.Add("http://schemas.xmlsoap.org/disco/");
            this.id15_SchemaReference = base.Reader.NameTable.Add("SchemaReference");
            this.id6_Item = base.Reader.NameTable.Add("http://schemas.xmlsoap.org/disco/scl/");
        }

        public object Read10_discovery()
        {
            base.Reader.MoveToContent();
            if (base.Reader.NodeType == XmlNodeType.Element)
            {
                if ((base.Reader.LocalName != this.id1_discovery) || (base.Reader.NamespaceURI != this.id2_Item))
                {
                    throw base.CreateUnknownNodeException();
                }
                return this.Read9_DiscoveryDocument(true, true);
            }
            base.UnknownNode(null, "http://schemas.xmlsoap.org/disco/:discovery");
            return null;
        }

        private DiscoveryDocumentReference Read3_DiscoveryDocumentReference(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id20_DiscoveryDocumentReference) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            DiscoveryDocumentReference o = new DiscoveryDocumentReference();
            bool[] flagArray = new bool[1];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id16_ref)) && (base.Reader.NamespaceURI == this.id13_Item))
                {
                    o.Ref = base.Reader.Value;
                    flagArray[0] = true;
                }
                else if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o, ":ref");
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(o, "");
                }
                else
                {
                    base.UnknownNode(o, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private ContractReference Read5_ContractReference(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id18_ContractReference) || (type.Namespace != this.id6_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            ContractReference o = new ContractReference();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id16_ref)) && (base.Reader.NamespaceURI == this.id13_Item))
                {
                    o.Ref = base.Reader.Value;
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id19_docRef)) && (base.Reader.NamespaceURI == this.id13_Item))
                    {
                        o.DocRef = base.Reader.Value;
                        flagArray[1] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, ":ref, :docRef");
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(o, "");
                }
                else
                {
                    base.UnknownNode(o, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private SchemaReference Read7_SchemaReference(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id15_SchemaReference) || (type.Namespace != this.id8_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            SchemaReference o = new SchemaReference();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id16_ref)) && (base.Reader.NamespaceURI == this.id13_Item))
                {
                    o.Ref = base.Reader.Value;
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id17_targetNamespace)) && (base.Reader.NamespaceURI == this.id13_Item))
                    {
                        o.TargetNamespace = base.Reader.Value;
                        flagArray[1] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, ":ref, :targetNamespace");
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(o, "");
                }
                else
                {
                    base.UnknownNode(o, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private SoapBinding Read8_SoapBinding(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id11_SoapBinding) || (type.Namespace != this.id10_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            SoapBinding o = new SoapBinding();
            bool[] flagArray = new bool[2];
            while (base.Reader.MoveToNextAttribute())
            {
                if ((!flagArray[0] && (base.Reader.LocalName == this.id12_address)) && (base.Reader.NamespaceURI == this.id13_Item))
                {
                    o.Address = base.Reader.Value;
                    flagArray[0] = true;
                }
                else
                {
                    if ((!flagArray[1] && (base.Reader.LocalName == this.id14_binding)) && (base.Reader.NamespaceURI == this.id13_Item))
                    {
                        o.Binding = base.ToXmlQualifiedName(base.Reader.Value);
                        flagArray[1] = true;
                        continue;
                    }
                    if (!base.IsXmlnsAttribute(base.Reader.Name))
                    {
                        base.UnknownNode(o, ":address, :binding");
                    }
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    base.UnknownNode(o, "");
                }
                else
                {
                    base.UnknownNode(o, "");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }

        private DiscoveryDocument Read9_DiscoveryDocument(bool isNullable, bool checkType)
        {
            XmlQualifiedName type = checkType ? base.GetXsiType() : null;
            bool flag = false;
            if (isNullable)
            {
                flag = base.ReadNull();
            }
            if ((checkType && (type != null)) && ((type.Name != this.id3_DiscoveryDocument) || (type.Namespace != this.id2_Item)))
            {
                throw base.CreateUnknownTypeException(type);
            }
            if (flag)
            {
                return null;
            }
            DiscoveryDocument o = new DiscoveryDocument();
            IList references = o.References;
            while (base.Reader.MoveToNextAttribute())
            {
                if (!base.IsXmlnsAttribute(base.Reader.Name))
                {
                    base.UnknownNode(o);
                }
            }
            base.Reader.MoveToElement();
            if (base.Reader.IsEmptyElement)
            {
                base.Reader.Skip();
                return o;
            }
            base.Reader.ReadStartElement();
            base.Reader.MoveToContent();
            int whileIterations = 0;
            int readerCount = base.ReaderCount;
            while ((base.Reader.NodeType != XmlNodeType.EndElement) && (base.Reader.NodeType != XmlNodeType.None))
            {
                if (base.Reader.NodeType == XmlNodeType.Element)
                {
                    if ((base.Reader.LocalName == this.id4_discoveryRef) && (base.Reader.NamespaceURI == this.id2_Item))
                    {
                        if (references == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            references.Add(this.Read3_DiscoveryDocumentReference(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id5_contractRef) && (base.Reader.NamespaceURI == this.id6_Item))
                    {
                        if (references == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            references.Add(this.Read5_ContractReference(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id7_schemaRef) && (base.Reader.NamespaceURI == this.id8_Item))
                    {
                        if (references == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            references.Add(this.Read7_SchemaReference(false, true));
                        }
                    }
                    else if ((base.Reader.LocalName == this.id9_soap) && (base.Reader.NamespaceURI == this.id10_Item))
                    {
                        if (references == null)
                        {
                            base.Reader.Skip();
                        }
                        else
                        {
                            references.Add(this.Read8_SoapBinding(false, true));
                        }
                    }
                    else
                    {
                        base.UnknownNode(o, "http://schemas.xmlsoap.org/disco/:discoveryRef, http://schemas.xmlsoap.org/disco/scl/:contractRef, http://schemas.xmlsoap.org/disco/schema/:schemaRef, http://schemas.xmlsoap.org/disco/soap/:soap");
                    }
                }
                else
                {
                    base.UnknownNode(o, "http://schemas.xmlsoap.org/disco/:discoveryRef, http://schemas.xmlsoap.org/disco/scl/:contractRef, http://schemas.xmlsoap.org/disco/schema/:schemaRef, http://schemas.xmlsoap.org/disco/soap/:soap");
                }
                base.Reader.MoveToContent();
                base.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            base.ReadEndElement();
            return o;
        }
    }
}

