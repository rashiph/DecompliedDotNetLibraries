namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Windows.Forms;

    internal class NotifyIconDesigner : ComponentDesigner
    {
        private DesignerActionListCollection _actionLists;

        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            base.InitializeNewComponent(defaultValues);
            NotifyIcon component = (NotifyIcon) base.Component;
            component.Visible = true;
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (this._actionLists == null)
                {
                    this._actionLists = new DesignerActionListCollection();
                    this._actionLists.Add(new NotifyIconActionList(this));
                }
                return this._actionLists;
            }
        }
    }
}

