namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Xml;

    public class SearchResponse : DirectoryResponse
    {
        private SearchResultEntryCollection entryCollection;
        private SearchResultReferenceCollection referenceCollection;
        internal bool searchDone;

        internal SearchResponse(XmlNode node) : base(node)
        {
            this.referenceCollection = new SearchResultReferenceCollection();
            this.entryCollection = new SearchResultEntryCollection();
        }

        internal SearchResponse(string dn, DirectoryControl[] controls, System.DirectoryServices.Protocols.ResultCode result, string message, Uri[] referral) : base(dn, controls, result, message, referral)
        {
            this.referenceCollection = new SearchResultReferenceCollection();
            this.entryCollection = new SearchResultEntryCollection();
        }

        private SearchResultEntryCollection EntryHelper()
        {
            SearchResultEntryCollection entrys = new SearchResultEntryCollection();
            XmlNodeList list = base.dsmlNode.SelectNodes("dsml:searchResultEntry", base.dsmlNS);
            if (list.Count != 0)
            {
                foreach (XmlNode node in list)
                {
                    SearchResultEntry entry = new SearchResultEntry((XmlElement) node);
                    entrys.Add(entry);
                }
            }
            return entrys;
        }

        private SearchResultReferenceCollection ReferenceHelper()
        {
            SearchResultReferenceCollection references = new SearchResultReferenceCollection();
            XmlNodeList list = base.dsmlNode.SelectNodes("dsml:searchResultReference", base.dsmlNS);
            if (list.Count != 0)
            {
                foreach (XmlNode node in list)
                {
                    SearchResultReference reference = new SearchResultReference((XmlElement) node);
                    references.Add(reference);
                }
            }
            return references;
        }

        internal void SetEntries(SearchResultEntryCollection col)
        {
            this.entryCollection = col;
        }

        internal void SetReferences(SearchResultReferenceCollection col)
        {
            this.referenceCollection = col;
        }

        public override DirectoryControl[] Controls
        {
            get
            {
                DirectoryControl[] controls = null;
                if (base.dsmlRequest && (base.directoryControls == null))
                {
                    base.directoryControls = base.ControlsHelper("dsml:searchResultDone/dsml:control");
                }
                if (base.directoryControls == null)
                {
                    return new DirectoryControl[0];
                }
                controls = new DirectoryControl[base.directoryControls.Length];
                for (int i = 0; i < base.directoryControls.Length; i++)
                {
                    controls[i] = new DirectoryControl(base.directoryControls[i].Type, base.directoryControls[i].GetValue(), base.directoryControls[i].IsCritical, base.directoryControls[i].ServerSide);
                }
                DirectoryControl.TransformControls(controls);
                return controls;
            }
        }

        public SearchResultEntryCollection Entries
        {
            get
            {
                if (base.dsmlRequest && (this.entryCollection.Count == 0))
                {
                    this.entryCollection = this.EntryHelper();
                }
                return this.entryCollection;
            }
        }

        public override string ErrorMessage
        {
            get
            {
                if (base.dsmlRequest && (base.directoryMessage == null))
                {
                    base.directoryMessage = base.ErrorMessageHelper("dsml:searchResultDone/dsml:errorMessage");
                }
                return base.directoryMessage;
            }
        }

        public override string MatchedDN
        {
            get
            {
                if (base.dsmlRequest && (base.dn == null))
                {
                    base.dn = base.MatchedDNHelper("dsml:searchResultDone/@dsml:matchedDN", "dsml:searchResultDone/@matchedDN");
                }
                return base.dn;
            }
        }

        public SearchResultReferenceCollection References
        {
            get
            {
                if (base.dsmlRequest && (this.referenceCollection.Count == 0))
                {
                    this.referenceCollection = this.ReferenceHelper();
                }
                return this.referenceCollection;
            }
        }

        public override Uri[] Referral
        {
            get
            {
                if (base.dsmlRequest && (base.directoryReferral == null))
                {
                    base.directoryReferral = base.ReferralHelper("dsml:searchResultDone/dsml:referral");
                }
                if (base.directoryReferral == null)
                {
                    return new Uri[0];
                }
                Uri[] uriArray = new Uri[base.directoryReferral.Length];
                for (int i = 0; i < base.directoryReferral.Length; i++)
                {
                    uriArray[i] = new Uri(base.directoryReferral[i].AbsoluteUri);
                }
                return uriArray;
            }
        }

        public override System.DirectoryServices.Protocols.ResultCode ResultCode
        {
            get
            {
                if (base.dsmlRequest && (base.result == ~System.DirectoryServices.Protocols.ResultCode.Success))
                {
                    base.result = base.ResultCodeHelper("dsml:searchResultDone/dsml:resultCode/@dsml:code", "dsml:searchResultDone/dsml:resultCode/@code");
                }
                return base.result;
            }
        }
    }
}

