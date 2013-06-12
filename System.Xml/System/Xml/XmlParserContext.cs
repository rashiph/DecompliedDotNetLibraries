namespace System.Xml
{
    using System;
    using System.Text;

    public class XmlParserContext
    {
        private string _baseURI;
        private string _docTypeName;
        private System.Text.Encoding _encoding;
        private string _internalSubset;
        private XmlNamespaceManager _nsMgr;
        private XmlNameTable _nt;
        private string _pubId;
        private string _sysId;
        private string _xmlLang;
        private System.Xml.XmlSpace _xmlSpace;

        public XmlParserContext(XmlNameTable nt, XmlNamespaceManager nsMgr, string xmlLang, System.Xml.XmlSpace xmlSpace) : this(nt, nsMgr, null, null, null, null, string.Empty, xmlLang, xmlSpace)
        {
        }

        public XmlParserContext(XmlNameTable nt, XmlNamespaceManager nsMgr, string xmlLang, System.Xml.XmlSpace xmlSpace, System.Text.Encoding enc) : this(nt, nsMgr, null, null, null, null, string.Empty, xmlLang, xmlSpace, enc)
        {
        }

        public XmlParserContext(XmlNameTable nt, XmlNamespaceManager nsMgr, string docTypeName, string pubId, string sysId, string internalSubset, string baseURI, string xmlLang, System.Xml.XmlSpace xmlSpace) : this(nt, nsMgr, docTypeName, pubId, sysId, internalSubset, baseURI, xmlLang, xmlSpace, null)
        {
        }

        public XmlParserContext(XmlNameTable nt, XmlNamespaceManager nsMgr, string docTypeName, string pubId, string sysId, string internalSubset, string baseURI, string xmlLang, System.Xml.XmlSpace xmlSpace, System.Text.Encoding enc)
        {
            this._docTypeName = string.Empty;
            this._pubId = string.Empty;
            this._sysId = string.Empty;
            this._internalSubset = string.Empty;
            this._xmlLang = string.Empty;
            this._baseURI = string.Empty;
            if (nsMgr != null)
            {
                if (nt == null)
                {
                    this._nt = nsMgr.NameTable;
                }
                else
                {
                    if (nt != nsMgr.NameTable)
                    {
                        throw new XmlException("Xml_NotSameNametable", string.Empty);
                    }
                    this._nt = nt;
                }
            }
            else
            {
                this._nt = nt;
            }
            this._nsMgr = nsMgr;
            this._docTypeName = (docTypeName == null) ? string.Empty : docTypeName;
            this._pubId = (pubId == null) ? string.Empty : pubId;
            this._sysId = (sysId == null) ? string.Empty : sysId;
            this._internalSubset = (internalSubset == null) ? string.Empty : internalSubset;
            this._baseURI = (baseURI == null) ? string.Empty : baseURI;
            this._xmlLang = (xmlLang == null) ? string.Empty : xmlLang;
            this._xmlSpace = xmlSpace;
            this._encoding = enc;
        }

        public string BaseURI
        {
            get
            {
                return this._baseURI;
            }
            set
            {
                this._baseURI = (value == null) ? string.Empty : value;
            }
        }

        public string DocTypeName
        {
            get
            {
                return this._docTypeName;
            }
            set
            {
                this._docTypeName = (value == null) ? string.Empty : value;
            }
        }

        public System.Text.Encoding Encoding
        {
            get
            {
                return this._encoding;
            }
            set
            {
                this._encoding = value;
            }
        }

        internal bool HasDtdInfo
        {
            get
            {
                if (!(this._internalSubset != string.Empty) && !(this._pubId != string.Empty))
                {
                    return (this._sysId != string.Empty);
                }
                return true;
            }
        }

        public string InternalSubset
        {
            get
            {
                return this._internalSubset;
            }
            set
            {
                this._internalSubset = (value == null) ? string.Empty : value;
            }
        }

        public XmlNamespaceManager NamespaceManager
        {
            get
            {
                return this._nsMgr;
            }
            set
            {
                this._nsMgr = value;
            }
        }

        public XmlNameTable NameTable
        {
            get
            {
                return this._nt;
            }
            set
            {
                this._nt = value;
            }
        }

        public string PublicId
        {
            get
            {
                return this._pubId;
            }
            set
            {
                this._pubId = (value == null) ? string.Empty : value;
            }
        }

        public string SystemId
        {
            get
            {
                return this._sysId;
            }
            set
            {
                this._sysId = (value == null) ? string.Empty : value;
            }
        }

        public string XmlLang
        {
            get
            {
                return this._xmlLang;
            }
            set
            {
                this._xmlLang = (value == null) ? string.Empty : value;
            }
        }

        public System.Xml.XmlSpace XmlSpace
        {
            get
            {
                return this._xmlSpace;
            }
            set
            {
                this._xmlSpace = value;
            }
        }
    }
}

