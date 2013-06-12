namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web.UI;

    [SupportsEventValidation, ToolboxItem(false)]
    internal class ChildTable : Table
    {
        private string _parentID;
        private bool _parentIDSet;
        private int _parentLevel;

        internal ChildTable() : this(1)
        {
        }

        internal ChildTable(int parentLevel)
        {
            this._parentLevel = parentLevel;
            this._parentIDSet = false;
        }

        internal ChildTable(string parentID)
        {
            this._parentID = parentID;
            this._parentIDSet = true;
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            base.AddAttributesToRender(writer);
            string parentID = this._parentID;
            if (!this._parentIDSet)
            {
                parentID = this.GetParentID();
            }
            if (parentID != null)
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, parentID);
            }
        }

        private string GetParentID()
        {
            if (this.ID == null)
            {
                Control parent = this;
                for (int i = 0; i < this._parentLevel; i++)
                {
                    parent = parent.Parent;
                    if (parent == null)
                    {
                        break;
                    }
                }
                if ((parent != null) && !string.IsNullOrEmpty(parent.ID))
                {
                    return parent.ClientID;
                }
            }
            return null;
        }
    }
}

