namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Web;
    using System.Web.UI;

    public sealed class WebPartCollection : ReadOnlyCollectionBase
    {
        public WebPartCollection()
        {
        }

        public WebPartCollection(ICollection webParts)
        {
            if (webParts == null)
            {
                throw new ArgumentNullException("webParts");
            }
            foreach (object obj2 in webParts)
            {
                if (obj2 == null)
                {
                    throw new ArgumentException(System.Web.SR.GetString("Collection_CantAddNull"), "webParts");
                }
                if (!(obj2 is WebPart))
                {
                    throw new ArgumentException(System.Web.SR.GetString("Collection_InvalidType", new object[] { "WebPart" }), "webParts");
                }
                base.InnerList.Add(obj2);
            }
        }

        internal int Add(WebPart value)
        {
            return base.InnerList.Add(value);
        }

        public bool Contains(WebPart value)
        {
            return base.InnerList.Contains(value);
        }

        public void CopyTo(WebPart[] array, int index)
        {
            base.InnerList.CopyTo(array, index);
        }

        public int IndexOf(WebPart value)
        {
            return base.InnerList.IndexOf(value);
        }

        public WebPart this[int index]
        {
            get
            {
                return (WebPart) base.InnerList[index];
            }
        }

        public WebPart this[string id]
        {
            get
            {
                foreach (WebPart part in base.InnerList)
                {
                    if (string.Equals(part.ID, id, StringComparison.OrdinalIgnoreCase))
                    {
                        return part;
                    }
                    GenericWebPart part2 = part as GenericWebPart;
                    if (part2 != null)
                    {
                        Control childControl = part2.ChildControl;
                        if ((childControl != null) && string.Equals(childControl.ID, id, StringComparison.OrdinalIgnoreCase))
                        {
                            return part2;
                        }
                    }
                    ProxyWebPart part3 = part as ProxyWebPart;
                    if ((part3 != null) && (string.Equals(part3.OriginalID, id, StringComparison.OrdinalIgnoreCase) || string.Equals(part3.GenericWebPartID, id, StringComparison.OrdinalIgnoreCase)))
                    {
                        return part3;
                    }
                }
                return null;
            }
        }
    }
}

