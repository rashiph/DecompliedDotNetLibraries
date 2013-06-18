namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;

    public class BulletedListDesigner : ListControlDesigner
    {
        protected override void PostFilterEvents(IDictionary events)
        {
            base.PostFilterEvents(events);
            events.Remove("SelectedIndexChanged");
        }

        protected override bool UsePreviewControl
        {
            get
            {
                return true;
            }
        }
    }
}

