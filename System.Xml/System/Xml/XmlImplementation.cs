namespace System.Xml
{
    using System;

    public class XmlImplementation
    {
        private XmlNameTable nameTable;

        public XmlImplementation() : this(new System.Xml.NameTable())
        {
        }

        public XmlImplementation(XmlNameTable nt)
        {
            this.nameTable = nt;
        }

        public virtual XmlDocument CreateDocument()
        {
            return new XmlDocument(this);
        }

        public bool HasFeature(string strFeature, string strVersion)
        {
            if ((string.Compare("XML", strFeature, StringComparison.OrdinalIgnoreCase) != 0) || (((strVersion != null) && !(strVersion == "1.0")) && !(strVersion == "2.0")))
            {
                return false;
            }
            return true;
        }

        internal XmlNameTable NameTable
        {
            get
            {
                return this.nameTable;
            }
        }
    }
}

