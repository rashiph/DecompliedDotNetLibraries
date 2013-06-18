namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.Windows.Forms;

    internal class ToolStripItemDataObject : DataObject
    {
        private ArrayList dragComponents;
        private ToolStrip owner;
        private ToolStripItem primarySelection;

        internal ToolStripItemDataObject(ArrayList dragComponents, ToolStripItem primarySelection, ToolStrip owner)
        {
            this.dragComponents = dragComponents;
            this.owner = owner;
            this.primarySelection = primarySelection;
        }

        internal ArrayList DragComponents
        {
            get
            {
                return this.dragComponents;
            }
        }

        internal ToolStrip Owner
        {
            get
            {
                return this.owner;
            }
        }

        internal ToolStripItem PrimarySelection
        {
            get
            {
                return this.primarySelection;
            }
        }
    }
}

