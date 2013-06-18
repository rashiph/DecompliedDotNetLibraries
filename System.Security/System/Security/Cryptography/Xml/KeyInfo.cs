namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Collections;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class KeyInfo : IEnumerable
    {
        private string m_id;
        private ArrayList m_KeyInfoClauses = new ArrayList();

        public void AddClause(KeyInfoClause clause)
        {
            this.m_KeyInfoClauses.Add(clause);
        }

        public IEnumerator GetEnumerator()
        {
            return this.m_KeyInfoClauses.GetEnumerator();
        }

        public IEnumerator GetEnumerator(Type requestedObjectType)
        {
            ArrayList list = new ArrayList();
            IEnumerator enumerator = this.m_KeyInfoClauses.GetEnumerator();
            while (enumerator.MoveNext())
            {
                object current = enumerator.Current;
                if (requestedObjectType.Equals(current.GetType()))
                {
                    list.Add(current);
                }
            }
            return list.GetEnumerator();
        }

        public XmlElement GetXml()
        {
            XmlDocument xmlDocument = new XmlDocument {
                PreserveWhitespace = true
            };
            return this.GetXml(xmlDocument);
        }

        internal XmlElement GetXml(XmlDocument xmlDocument)
        {
            XmlElement element = xmlDocument.CreateElement("KeyInfo", "http://www.w3.org/2000/09/xmldsig#");
            if (!string.IsNullOrEmpty(this.m_id))
            {
                element.SetAttribute("Id", this.m_id);
            }
            for (int i = 0; i < this.m_KeyInfoClauses.Count; i++)
            {
                XmlElement xml = ((KeyInfoClause) this.m_KeyInfoClauses[i]).GetXml(xmlDocument);
                if (xml != null)
                {
                    element.AppendChild(xml);
                }
            }
            return element;
        }

        public void LoadXml(XmlElement value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            XmlElement element = value;
            this.m_id = System.Security.Cryptography.Xml.Utils.GetAttribute(element, "Id", "http://www.w3.org/2000/09/xmldsig#");
            for (XmlNode node = element.FirstChild; node != null; node = node.NextSibling)
            {
                XmlElement element2 = node as XmlElement;
                if (element2 != null)
                {
                    string name = element2.NamespaceURI + " " + element2.LocalName;
                    if (name == "http://www.w3.org/2000/09/xmldsig# KeyValue")
                    {
                        foreach (XmlNode node2 in element2.ChildNodes)
                        {
                            XmlElement element3 = node2 as XmlElement;
                            if (element3 != null)
                            {
                                name = name + "/" + element3.LocalName;
                                break;
                            }
                        }
                    }
                    KeyInfoClause clause = (KeyInfoClause) CryptoConfig.CreateFromName(name);
                    if (clause == null)
                    {
                        clause = new KeyInfoNode();
                    }
                    clause.LoadXml(element2);
                    this.AddClause(clause);
                }
            }
        }

        public int Count
        {
            get
            {
                return this.m_KeyInfoClauses.Count;
            }
        }

        public string Id
        {
            get
            {
                return this.m_id;
            }
            set
            {
                this.m_id = value;
            }
        }
    }
}

