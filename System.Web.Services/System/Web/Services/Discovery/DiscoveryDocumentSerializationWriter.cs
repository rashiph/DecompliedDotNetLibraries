namespace System.Web.Services.Discovery
{
    using System;
    using System.Collections;
    using System.Xml.Serialization;

    internal class DiscoveryDocumentSerializationWriter : XmlSerializationWriter
    {
        protected override void InitCallbacks()
        {
        }

        public void Write10_discovery(object o)
        {
            base.WriteStartDocument();
            if (o == null)
            {
                base.WriteNullTagLiteral("discovery", "http://schemas.xmlsoap.org/disco/");
            }
            else
            {
                base.TopLevelElement();
                this.Write9_DiscoveryDocument("discovery", "http://schemas.xmlsoap.org/disco/", (DiscoveryDocument) o, true, false);
            }
        }

        private void Write3_DiscoveryDocumentReference(string n, string ns, DiscoveryDocumentReference o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(DiscoveryDocumentReference)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("DiscoveryDocumentReference", "http://schemas.xmlsoap.org/disco/");
                }
                base.WriteAttribute("ref", "", o.Ref);
                base.WriteEndElement(o);
            }
        }

        private void Write5_ContractReference(string n, string ns, ContractReference o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(ContractReference)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("ContractReference", "http://schemas.xmlsoap.org/disco/scl/");
                }
                base.WriteAttribute("ref", "", o.Ref);
                base.WriteAttribute("docRef", "", o.DocRef);
                base.WriteEndElement(o);
            }
        }

        private void Write7_SchemaReference(string n, string ns, SchemaReference o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(SchemaReference)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("SchemaReference", "http://schemas.xmlsoap.org/disco/schema/");
                }
                base.WriteAttribute("ref", "", o.Ref);
                base.WriteAttribute("targetNamespace", "", o.TargetNamespace);
                base.WriteEndElement(o);
            }
        }

        private void Write8_SoapBinding(string n, string ns, SoapBinding o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(SoapBinding)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("SoapBinding", "http://schemas.xmlsoap.org/disco/soap/");
                }
                base.WriteAttribute("address", "", o.Address);
                base.WriteAttribute("binding", "", base.FromXmlQualifiedName(o.Binding));
                base.WriteEndElement(o);
            }
        }

        private void Write9_DiscoveryDocument(string n, string ns, DiscoveryDocument o, bool isNullable, bool needType)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    base.WriteNullTagLiteral(n, ns);
                }
            }
            else
            {
                if (!needType && !(o.GetType() == typeof(DiscoveryDocument)))
                {
                    throw base.CreateUnknownTypeException(o);
                }
                base.WriteStartElement(n, ns, o, false, null);
                if (needType)
                {
                    base.WriteXsiType("DiscoveryDocument", "http://schemas.xmlsoap.org/disco/");
                }
                IList references = o.References;
                if (references != null)
                {
                    for (int i = 0; i < references.Count; i++)
                    {
                        object obj2 = references[i];
                        if (obj2 is SchemaReference)
                        {
                            this.Write7_SchemaReference("schemaRef", "http://schemas.xmlsoap.org/disco/schema/", (SchemaReference) obj2, false, false);
                        }
                        else if (obj2 is ContractReference)
                        {
                            this.Write5_ContractReference("contractRef", "http://schemas.xmlsoap.org/disco/scl/", (ContractReference) obj2, false, false);
                        }
                        else if (obj2 is DiscoveryDocumentReference)
                        {
                            this.Write3_DiscoveryDocumentReference("discoveryRef", "http://schemas.xmlsoap.org/disco/", (DiscoveryDocumentReference) obj2, false, false);
                        }
                        else if (obj2 is SoapBinding)
                        {
                            this.Write8_SoapBinding("soap", "http://schemas.xmlsoap.org/disco/soap/", (SoapBinding) obj2, false, false);
                        }
                        else if (obj2 != null)
                        {
                            throw base.CreateUnknownTypeException(obj2);
                        }
                    }
                }
                base.WriteEndElement(o);
            }
        }
    }
}

