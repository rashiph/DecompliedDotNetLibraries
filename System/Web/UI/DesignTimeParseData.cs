namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Design;

    public sealed class DesignTimeParseData
    {
        private EventHandler _dataBindingHandler;
        private IDesignerHost _designerHost;
        private string _documentUrl;
        private string _filter;
        private string _parseText;
        private bool _shouldApplyTheme;
        private ICollection _userControlRegisterEntries;

        public DesignTimeParseData(IDesignerHost designerHost, string parseText) : this(designerHost, parseText, string.Empty)
        {
        }

        public DesignTimeParseData(IDesignerHost designerHost, string parseText, string filter)
        {
            if (string.IsNullOrEmpty(parseText))
            {
                throw new ArgumentNullException("parseText");
            }
            this._designerHost = designerHost;
            this._parseText = parseText;
            this._filter = filter;
        }

        internal void SetUserControlRegisterEntries(ICollection userControlRegisterEntries, List<TagNamespaceRegisterEntry> tagRegisterEntries)
        {
            if ((userControlRegisterEntries != null) || (tagRegisterEntries != null))
            {
                List<Triplet> list = new List<Triplet>();
                if (userControlRegisterEntries != null)
                {
                    foreach (UserControlRegisterEntry entry in userControlRegisterEntries)
                    {
                        list.Add(new Triplet(entry.TagPrefix, new Pair(entry.TagName, entry.UserControlSource.ToString()), null));
                    }
                }
                if (tagRegisterEntries != null)
                {
                    foreach (TagNamespaceRegisterEntry entry2 in tagRegisterEntries)
                    {
                        list.Add(new Triplet(entry2.TagPrefix, null, new Pair(entry2.Namespace, entry2.AssemblyName)));
                    }
                }
                this._userControlRegisterEntries = list;
            }
        }

        public EventHandler DataBindingHandler
        {
            get
            {
                return this._dataBindingHandler;
            }
            set
            {
                this._dataBindingHandler = value;
            }
        }

        public IDesignerHost DesignerHost
        {
            get
            {
                return this._designerHost;
            }
        }

        public string DocumentUrl
        {
            get
            {
                if (this._documentUrl == null)
                {
                    return string.Empty;
                }
                return this._documentUrl;
            }
            set
            {
                this._documentUrl = value;
            }
        }

        public string Filter
        {
            get
            {
                if (this._filter == null)
                {
                    return string.Empty;
                }
                return this._filter;
            }
        }

        public string ParseText
        {
            get
            {
                return this._parseText;
            }
        }

        public bool ShouldApplyTheme
        {
            get
            {
                return this._shouldApplyTheme;
            }
            set
            {
                this._shouldApplyTheme = value;
            }
        }

        public ICollection UserControlRegisterEntries
        {
            get
            {
                return this._userControlRegisterEntries;
            }
        }
    }
}

