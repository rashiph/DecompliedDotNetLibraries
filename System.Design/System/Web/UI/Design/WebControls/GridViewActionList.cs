namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel.Design;
    using System.Design;

    internal class GridViewActionList : DesignerActionList
    {
        private bool _allowDeleting;
        private bool _allowEditing;
        private bool _allowMoveLeft;
        private bool _allowMoveRight;
        private bool _allowPaging;
        private bool _allowRemoveField;
        private bool _allowSelection;
        private bool _allowSorting;
        private GridViewDesigner _gridViewDesigner;

        public GridViewActionList(GridViewDesigner gridViewDesigner) : base(gridViewDesigner.Component)
        {
            this._gridViewDesigner = gridViewDesigner;
        }

        public void AddNewField()
        {
            this._gridViewDesigner.AddNewField();
        }

        public void EditFields()
        {
            this._gridViewDesigner.EditFields();
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection items = new DesignerActionItemCollection();
            items.Add(new DesignerActionMethodItem(this, "EditFields", System.Design.SR.GetString("GridView_EditFieldsVerb"), "Action", System.Design.SR.GetString("GridView_EditFieldsDesc")));
            items.Add(new DesignerActionMethodItem(this, "AddNewField", System.Design.SR.GetString("GridView_AddNewFieldVerb"), "Action", System.Design.SR.GetString("GridView_AddNewFieldDesc")));
            if (this.AllowMoveLeft)
            {
                items.Add(new DesignerActionMethodItem(this, "MoveFieldLeft", System.Design.SR.GetString("GridView_MoveFieldLeftVerb"), "Action", System.Design.SR.GetString("GridView_MoveFieldLeftDesc")));
            }
            if (this.AllowMoveRight)
            {
                items.Add(new DesignerActionMethodItem(this, "MoveFieldRight", System.Design.SR.GetString("GridView_MoveFieldRightVerb"), "Action", System.Design.SR.GetString("GridView_MoveFieldRightDesc")));
            }
            if (this.AllowRemoveField)
            {
                items.Add(new DesignerActionMethodItem(this, "RemoveField", System.Design.SR.GetString("GridView_RemoveFieldVerb"), "Action", System.Design.SR.GetString("GridView_RemoveFieldDesc")));
            }
            if (this.AllowPaging)
            {
                items.Add(new DesignerActionPropertyItem("EnablePaging", System.Design.SR.GetString("GridView_EnablePaging"), "Behavior", System.Design.SR.GetString("GridView_EnablePagingDesc")));
            }
            if (this.AllowSorting)
            {
                items.Add(new DesignerActionPropertyItem("EnableSorting", System.Design.SR.GetString("GridView_EnableSorting"), "Behavior", System.Design.SR.GetString("GridView_EnableSortingDesc")));
            }
            if (this.AllowEditing)
            {
                items.Add(new DesignerActionPropertyItem("EnableEditing", System.Design.SR.GetString("GridView_EnableEditing"), "Behavior", System.Design.SR.GetString("GridView_EnableEditingDesc")));
            }
            if (this.AllowDeleting)
            {
                items.Add(new DesignerActionPropertyItem("EnableDeleting", System.Design.SR.GetString("GridView_EnableDeleting"), "Behavior", System.Design.SR.GetString("GridView_EnableDeletingDesc")));
            }
            if (this.AllowSelection)
            {
                items.Add(new DesignerActionPropertyItem("EnableSelection", System.Design.SR.GetString("GridView_EnableSelection"), "Behavior", System.Design.SR.GetString("GridView_EnableSelectionDesc")));
            }
            return items;
        }

        public void MoveFieldLeft()
        {
            this._gridViewDesigner.MoveLeft();
        }

        public void MoveFieldRight()
        {
            this._gridViewDesigner.MoveRight();
        }

        public void RemoveField()
        {
            this._gridViewDesigner.RemoveField();
        }

        internal bool AllowDeleting
        {
            get
            {
                return this._allowDeleting;
            }
            set
            {
                this._allowDeleting = value;
            }
        }

        internal bool AllowEditing
        {
            get
            {
                return this._allowEditing;
            }
            set
            {
                this._allowEditing = value;
            }
        }

        internal bool AllowMoveLeft
        {
            get
            {
                return this._allowMoveLeft;
            }
            set
            {
                this._allowMoveLeft = value;
            }
        }

        internal bool AllowMoveRight
        {
            get
            {
                return this._allowMoveRight;
            }
            set
            {
                this._allowMoveRight = value;
            }
        }

        internal bool AllowPaging
        {
            get
            {
                return this._allowPaging;
            }
            set
            {
                this._allowPaging = value;
            }
        }

        internal bool AllowRemoveField
        {
            get
            {
                return this._allowRemoveField;
            }
            set
            {
                this._allowRemoveField = value;
            }
        }

        internal bool AllowSelection
        {
            get
            {
                return this._allowSelection;
            }
            set
            {
                this._allowSelection = value;
            }
        }

        internal bool AllowSorting
        {
            get
            {
                return this._allowSorting;
            }
            set
            {
                this._allowSorting = value;
            }
        }

        public override bool AutoShow
        {
            get
            {
                return true;
            }
            set
            {
            }
        }

        public bool EnableDeleting
        {
            get
            {
                return this._gridViewDesigner.EnableDeleting;
            }
            set
            {
                this._gridViewDesigner.EnableDeleting = value;
            }
        }

        public bool EnableEditing
        {
            get
            {
                return this._gridViewDesigner.EnableEditing;
            }
            set
            {
                this._gridViewDesigner.EnableEditing = value;
            }
        }

        public bool EnablePaging
        {
            get
            {
                return this._gridViewDesigner.EnablePaging;
            }
            set
            {
                this._gridViewDesigner.EnablePaging = value;
            }
        }

        public bool EnableSelection
        {
            get
            {
                return this._gridViewDesigner.EnableSelection;
            }
            set
            {
                this._gridViewDesigner.EnableSelection = value;
            }
        }

        public bool EnableSorting
        {
            get
            {
                return this._gridViewDesigner.EnableSorting;
            }
            set
            {
                this._gridViewDesigner.EnableSorting = value;
            }
        }
    }
}

