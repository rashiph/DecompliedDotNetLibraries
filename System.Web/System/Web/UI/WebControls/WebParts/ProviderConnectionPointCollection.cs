namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Web;

    public sealed class ProviderConnectionPointCollection : ReadOnlyCollectionBase
    {
        private HybridDictionary _ids;

        public ProviderConnectionPointCollection()
        {
        }

        public ProviderConnectionPointCollection(ICollection connectionPoints)
        {
            if (connectionPoints == null)
            {
                throw new ArgumentNullException("connectionPoints");
            }
            this._ids = new HybridDictionary(connectionPoints.Count, true);
            foreach (object obj2 in connectionPoints)
            {
                if (obj2 == null)
                {
                    throw new ArgumentException(System.Web.SR.GetString("Collection_CantAddNull"), "connectionPoints");
                }
                ProviderConnectionPoint point = obj2 as ProviderConnectionPoint;
                if (point == null)
                {
                    throw new ArgumentException(System.Web.SR.GetString("Collection_InvalidType", new object[] { "ProviderConnectionPoint" }), "connectionPoints");
                }
                string iD = point.ID;
                if (this._ids.Contains(iD))
                {
                    throw new ArgumentException(System.Web.SR.GetString("WebPart_Collection_DuplicateID", new object[] { "ProviderConnectionPoint", iD }), "connectionPoints");
                }
                base.InnerList.Add(point);
                this._ids.Add(iD, point);
            }
        }

        public bool Contains(ProviderConnectionPoint connectionPoint)
        {
            return base.InnerList.Contains(connectionPoint);
        }

        public void CopyTo(ProviderConnectionPoint[] array, int index)
        {
            base.InnerList.CopyTo(array, index);
        }

        public int IndexOf(ProviderConnectionPoint connectionPoint)
        {
            return base.InnerList.IndexOf(connectionPoint);
        }

        public ProviderConnectionPoint Default
        {
            get
            {
                return this[ConnectionPoint.DefaultID];
            }
        }

        public ProviderConnectionPoint this[int index]
        {
            get
            {
                return (ProviderConnectionPoint) base.InnerList[index];
            }
        }

        public ProviderConnectionPoint this[string id]
        {
            get
            {
                if (this._ids == null)
                {
                    return null;
                }
                return (ProviderConnectionPoint) this._ids[id];
            }
        }
    }
}

