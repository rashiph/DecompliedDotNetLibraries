namespace System.Xml.Schema
{
    using System;
    using System.Xml;

    internal sealed class SchemaEntity : IDtdEntityInfo
    {
        private string baseURI;
        private string declaredURI;
        private bool isDeclaredInExternal;
        private bool isExternal;
        private bool isParameter;
        private int lineNumber;
        private int linePosition;
        private XmlQualifiedName ndata = XmlQualifiedName.Empty;
        private bool parsingInProgress;
        private string pubid;
        private XmlQualifiedName qname;
        private string text;
        private string url;

        internal SchemaEntity(XmlQualifiedName qname, bool isParameter)
        {
            this.qname = qname;
            this.isParameter = isParameter;
        }

        internal static bool IsPredefinedEntity(string n)
        {
            if ((!(n == "lt") && !(n == "gt")) && (!(n == "amp") && !(n == "apos")))
            {
                return (n == "quot");
            }
            return true;
        }

        internal string BaseURI
        {
            get
            {
                if (this.baseURI != null)
                {
                    return this.baseURI;
                }
                return string.Empty;
            }
            set
            {
                this.baseURI = value;
            }
        }

        internal bool DeclaredInExternal
        {
            get
            {
                return this.isDeclaredInExternal;
            }
            set
            {
                this.isDeclaredInExternal = value;
            }
        }

        internal string DeclaredURI
        {
            get
            {
                if (this.declaredURI != null)
                {
                    return this.declaredURI;
                }
                return string.Empty;
            }
            set
            {
                this.declaredURI = value;
            }
        }

        internal bool IsExternal
        {
            get
            {
                return this.isExternal;
            }
            set
            {
                this.isExternal = value;
            }
        }

        internal int Line
        {
            get
            {
                return this.lineNumber;
            }
            set
            {
                this.lineNumber = value;
            }
        }

        internal XmlQualifiedName Name
        {
            get
            {
                return this.qname;
            }
        }

        internal XmlQualifiedName NData
        {
            get
            {
                return this.ndata;
            }
            set
            {
                this.ndata = value;
            }
        }

        internal bool ParsingInProgress
        {
            get
            {
                return this.parsingInProgress;
            }
            set
            {
                this.parsingInProgress = value;
            }
        }

        internal int Pos
        {
            get
            {
                return this.linePosition;
            }
            set
            {
                this.linePosition = value;
            }
        }

        internal string Pubid
        {
            get
            {
                return this.pubid;
            }
            set
            {
                this.pubid = value;
            }
        }

        string IDtdEntityInfo.BaseUriString
        {
            get
            {
                return this.BaseURI;
            }
        }

        string IDtdEntityInfo.DeclaredUriString
        {
            get
            {
                return this.DeclaredURI;
            }
        }

        bool IDtdEntityInfo.IsDeclaredInExternal
        {
            get
            {
                return this.DeclaredInExternal;
            }
        }

        bool IDtdEntityInfo.IsExternal
        {
            get
            {
                return this.IsExternal;
            }
        }

        bool IDtdEntityInfo.IsParameterEntity
        {
            get
            {
                return this.isParameter;
            }
        }

        bool IDtdEntityInfo.IsUnparsedEntity
        {
            get
            {
                return !this.NData.IsEmpty;
            }
        }

        int IDtdEntityInfo.LineNumber
        {
            get
            {
                return this.Line;
            }
        }

        int IDtdEntityInfo.LinePosition
        {
            get
            {
                return this.Pos;
            }
        }

        string IDtdEntityInfo.Name
        {
            get
            {
                return this.Name.Name;
            }
        }

        string IDtdEntityInfo.PublicId
        {
            get
            {
                return this.Pubid;
            }
        }

        string IDtdEntityInfo.SystemId
        {
            get
            {
                return this.Url;
            }
        }

        string IDtdEntityInfo.Text
        {
            get
            {
                return this.Text;
            }
        }

        internal string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                this.text = value;
                this.isExternal = false;
            }
        }

        internal string Url
        {
            get
            {
                return this.url;
            }
            set
            {
                this.url = value;
                this.isExternal = true;
            }
        }
    }
}

