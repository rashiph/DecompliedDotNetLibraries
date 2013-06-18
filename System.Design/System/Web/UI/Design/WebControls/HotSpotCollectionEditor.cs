namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel.Design;
    using System.Web.UI.WebControls;

    public class HotSpotCollectionEditor : CollectionEditor
    {
        public HotSpotCollectionEditor(Type type) : base(type)
        {
        }

        protected override bool CanSelectMultipleInstances()
        {
            return false;
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new Type[] { typeof(CircleHotSpot), typeof(RectangleHotSpot), typeof(PolygonHotSpot) };
        }

        protected override string HelpTopic
        {
            get
            {
                return "net.Asp.HotSpot.CollectionEditor";
            }
        }
    }
}

