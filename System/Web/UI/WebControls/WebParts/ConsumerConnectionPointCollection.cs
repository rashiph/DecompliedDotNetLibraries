namespace System.Web.UI.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Web;

    public sealed class ConsumerConnectionPointCollection : ReadOnlyCollectionBase
    {
        private HybridDictionary _ids;

        public ConsumerConnectionPointCollection()
        {
        }

        public ConsumerConnectionPointCollection(ICollection connectionPoints)
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
                ConsumerConnectionPoint point = obj2 as ConsumerConnectionPoint;
                if (point == null)
                {
                    throw new ArgumentException(System.Web.SR.GetString("Collection_InvalidType", new object[] { "ConsumerConnectionPoint" }), "connectionPoints");
                }
                string iD = point.ID;
                if (this._ids.Contains(iD))
                {
                    throw new ArgumentException(System.Web.SR.GetString("WebPart_Collection_DuplicateID", new object[] { "ConsumerConnectionPoint", iD }), "connectionPoints");
                }
                base.InnerList.Add(point);
                this._ids.Add(iD, point);
            }
        }

        public bool Contains(ConsumerConnectionPoint connectionPoint)
        {
            return base.InnerList.Contains(connectionPoint);
        }

        public void CopyTo(ConsumerConnectionPoint[] array, int index)
        {
            base.InnerList.CopyTo(array, index);
        }

        public int IndexOf(ConsumerConnectionPoint connectionPoint)
        {
            return base.InnerList.IndexOf(connectionPoint);
        }

        public ConsumerConnectionPoint Default
        {
            get
            {
                return this[ConnectionPoint.DefaultID];
            }
        }

        public ConsumerConnectionPoint this[int index]
        {
            get
            {
                return (ConsumerConnectionPoint) base.InnerList[index];
            }
        }

        public ConsumerConnectionPoint this[string id]
        {
            get
            {
                if (this._ids == null)
                {
                    return null;
                }
                return (ConsumerConnectionPoint) this._ids[id];
            }
        }
    }
}

