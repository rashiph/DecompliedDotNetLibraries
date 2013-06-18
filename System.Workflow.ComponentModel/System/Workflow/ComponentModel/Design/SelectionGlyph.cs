namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Runtime;

    public abstract class SelectionGlyph : DesignerGlyph
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected SelectionGlyph()
        {
        }

        public override Rectangle GetBounds(ActivityDesigner designer, bool activated)
        {
            if (designer == null)
            {
                throw new ArgumentNullException("designer");
            }
            Rectangle bounds = designer.Bounds;
            bounds.Inflate(WorkflowTheme.CurrentTheme.AmbientTheme.SelectionSize.Width / 2, WorkflowTheme.CurrentTheme.AmbientTheme.SelectionSize.Height / 2);
            return bounds;
        }

        public virtual Rectangle[] GetGrabHandles(ActivityDesigner designer)
        {
            Size selectionSize = WorkflowTheme.CurrentTheme.AmbientTheme.SelectionSize;
            Size size = new Size(selectionSize.Width, selectionSize.Height);
            Rectangle bounds = this.GetBounds(designer, false);
            bounds.Inflate(selectionSize.Width, selectionSize.Height);
            ActivityDesigner parentDesigner = designer.ParentDesigner;
            if ((parentDesigner != null) && (parentDesigner is FreeformActivityDesigner))
            {
                return new Rectangle[] { new Rectangle(bounds.Location, size), new Rectangle(new Point(bounds.Left + ((bounds.Width - size.Width) / 2), bounds.Top), size), new Rectangle(bounds.Right - size.Width, bounds.Top, size.Width, size.Height), new Rectangle(new Point(bounds.Right - size.Width, bounds.Top + ((bounds.Height - size.Height) / 2)), size), new Rectangle(bounds.Right - size.Width, bounds.Bottom - size.Height, size.Width, size.Height), new Rectangle(new Point(bounds.Left + ((bounds.Width - size.Width) / 2), bounds.Bottom - size.Height), size), new Rectangle(bounds.Left, bounds.Bottom - size.Height, size.Width, size.Height), new Rectangle(new Point(bounds.Left, bounds.Top + ((bounds.Height - size.Height) / 2)), size) };
            }
            return new Rectangle[] { new Rectangle(bounds.Location, size) };
        }

        protected override void OnPaint(Graphics graphics, bool activated, AmbientTheme ambientTheme, ActivityDesigner designer)
        {
            ActivityDesignerPaint.DrawSelection(graphics, this.GetBounds(designer, activated), this.IsPrimarySelection, WorkflowTheme.CurrentTheme.AmbientTheme.SelectionSize, this.GetGrabHandles(designer));
        }

        public abstract bool IsPrimarySelection { get; }

        public override int Priority
        {
            get
            {
                return 4;
            }
        }
    }
}

