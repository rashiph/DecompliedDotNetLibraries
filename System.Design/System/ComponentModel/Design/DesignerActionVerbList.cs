namespace System.ComponentModel.Design
{
    using System;

    internal class DesignerActionVerbList : DesignerActionList
    {
        private DesignerVerb[] _verbs;

        public DesignerActionVerbList(DesignerVerb[] verbs) : base(null)
        {
            this._verbs = verbs;
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection items = new DesignerActionItemCollection();
            for (int i = 0; i < this._verbs.Length; i++)
            {
                if ((this._verbs[i].Visible && this._verbs[i].Enabled) && this._verbs[i].Supported)
                {
                    items.Add(new DesignerActionVerbItem(this._verbs[i]));
                }
            }
            return items;
        }

        public override bool AutoShow
        {
            get
            {
                return false;
            }
        }
    }
}

