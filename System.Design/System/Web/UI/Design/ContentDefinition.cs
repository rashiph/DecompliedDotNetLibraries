namespace System.Web.UI.Design
{
    using System;

    public class ContentDefinition
    {
        private string _contentPlaceHolderID;
        private string _defaultContent;
        private string _defaultDesignTimeHTML;

        public ContentDefinition(string id, string content, string designTimeHtml)
        {
            this._contentPlaceHolderID = id;
            this._defaultContent = content;
            this._defaultDesignTimeHTML = designTimeHtml;
        }

        public string ContentPlaceHolderID
        {
            get
            {
                return this._contentPlaceHolderID;
            }
        }

        public string DefaultContent
        {
            get
            {
                return this._defaultContent;
            }
        }

        public string DefaultDesignTimeHtml
        {
            get
            {
                return this._defaultDesignTimeHTML;
            }
        }
    }
}

