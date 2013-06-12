namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Web;

    public sealed class WebPartDescriptionCollection : ReadOnlyCollectionBase
    {
        private HybridDictionary _ids;

        public WebPartDescriptionCollection()
        {
        }

        public WebPartDescriptionCollection(ICollection webPartDescriptions)
        {
            if (webPartDescriptions == null)
            {
                throw new ArgumentNullException("webPartDescriptions");
            }
            this._ids = new HybridDictionary(webPartDescriptions.Count, true);
            foreach (object obj2 in webPartDescriptions)
            {
                if (obj2 == null)
                {
                    throw new ArgumentException(System.Web.SR.GetString("Collection_CantAddNull"), "webPartDescriptions");
                }
                WebPartDescription description = obj2 as WebPartDescription;
                if (description == null)
                {
                    throw new ArgumentException(System.Web.SR.GetString("Collection_InvalidType", new object[] { "WebPartDescription" }), "webPartDescriptions");
                }
                string iD = description.ID;
                if (this._ids.Contains(iD))
                {
                    throw new ArgumentException(System.Web.SR.GetString("WebPart_Collection_DuplicateID", new object[] { "WebPartDescription", iD }), "webPartDescriptions");
                }
                base.InnerList.Add(description);
                this._ids.Add(iD, description);
            }
        }

        public bool Contains(WebPartDescription value)
        {
            return base.InnerList.Contains(value);
        }

        public void CopyTo(WebPartDescription[] array, int index)
        {
            base.InnerList.CopyTo(array, index);
        }

        public int IndexOf(WebPartDescription value)
        {
            return base.InnerList.IndexOf(value);
        }

        public WebPartDescription this[int index]
        {
            get
            {
                return (WebPartDescription) base.InnerList[index];
            }
        }

        public WebPartDescription this[string id]
        {
            get
            {
                if (this._ids == null)
                {
                    return null;
                }
                return (WebPartDescription) this._ids[id];
            }
        }
    }
}

