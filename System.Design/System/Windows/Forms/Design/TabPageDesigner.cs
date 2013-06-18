namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class TabPageDesigner : PanelDesigner
    {
        public override bool CanBeParentedTo(IDesigner parentDesigner)
        {
            return ((parentDesigner != null) && (parentDesigner.Component is TabControl));
        }

        protected override ControlBodyGlyph GetControlGlyph(GlyphSelectionType selectionType)
        {
            this.OnSetCursor();
            return new ControlBodyGlyph(Rectangle.Empty, Cursor.Current, this.Control, this);
        }

        internal void OnDragDropInternal(DragEventArgs de)
        {
            this.OnDragDrop(de);
        }

        internal void OnDragEnterInternal(DragEventArgs de)
        {
            this.OnDragEnter(de);
        }

        internal void OnDragLeaveInternal(EventArgs e)
        {
            this.OnDragLeave(e);
        }

        internal void OnDragOverInternal(DragEventArgs e)
        {
            this.OnDragOver(e);
        }

        internal void OnGiveFeedbackInternal(GiveFeedbackEventArgs e)
        {
            this.OnGiveFeedback(e);
        }

        public override System.Windows.Forms.Design.SelectionRules SelectionRules
        {
            get
            {
                System.Windows.Forms.Design.SelectionRules selectionRules = base.SelectionRules;
                if (this.Control.Parent is TabControl)
                {
                    selectionRules &= ~System.Windows.Forms.Design.SelectionRules.AllSizeable;
                }
                return selectionRules;
            }
        }
    }
}

