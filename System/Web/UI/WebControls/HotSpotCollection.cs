namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Reflection;
    using System.Web;
    using System.Web.UI;

    [Editor("System.Web.UI.Design.WebControls.HotSpotCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
    public sealed class HotSpotCollection : StateManagedCollection
    {
        private static readonly Type[] knownTypes = new Type[] { typeof(CircleHotSpot), typeof(RectangleHotSpot), typeof(PolygonHotSpot) };

        public int Add(HotSpot spot)
        {
            return ((IList) this).Add(spot);
        }

        protected override object CreateKnownType(int index)
        {
            switch (index)
            {
                case 0:
                    return new CircleHotSpot();

                case 1:
                    return new RectangleHotSpot();

                case 2:
                    return new PolygonHotSpot();
            }
            throw new ArgumentOutOfRangeException(System.Web.SR.GetString("HotSpotCollection_InvalidTypeIndex"));
        }

        protected override Type[] GetKnownTypes()
        {
            return knownTypes;
        }

        public void Insert(int index, HotSpot spot)
        {
            ((IList) this).Insert(index, spot);
        }

        protected override void OnValidate(object o)
        {
            base.OnValidate(o);
            if (!(o is HotSpot))
            {
                throw new ArgumentException(System.Web.SR.GetString("HotSpotCollection_InvalidType"));
            }
        }

        public void Remove(HotSpot spot)
        {
            ((IList) this).Remove(spot);
        }

        public void RemoveAt(int index)
        {
            ((IList) this).RemoveAt(index);
        }

        protected override void SetDirtyObject(object o)
        {
            ((HotSpot) o).SetDirty();
        }

        public HotSpot this[int index]
        {
            get
            {
                return (HotSpot) this[index];
            }
        }
    }
}

