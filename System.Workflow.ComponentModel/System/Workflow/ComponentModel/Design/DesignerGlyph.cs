namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Runtime;

    public abstract class DesignerGlyph
    {
        internal const int CommentPriority = 3;
        internal const int ConfigErrorPriority = 2;
        internal const int ConnectionPointPriority = 1;
        internal const int ConnectorDragDropPriority = 2;
        internal const int FadeGlyphPriority = 3;
        public const int HighestPriority = 0;
        internal const int LockedGlyphPriority = 3;
        public const int LowestPriority = 0xf4240;
        internal const int MoveAnchorPriority = 1;
        internal const int NonExecutionStatePriority = 5;
        public const int NormalPriority = 0x2710;
        internal const int ReadOnlyGlyphPriority = 3;
        internal const int SelectionPriority = 4;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected DesignerGlyph()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal void Activate(ActivityDesigner designer)
        {
            this.OnActivate(designer);
        }

        internal void Draw(Graphics graphics, ActivityDesigner designer)
        {
            this.OnPaint(graphics, false, WorkflowTheme.CurrentTheme.AmbientTheme, designer);
        }

        internal void DrawActivated(Graphics graphics, ActivityDesigner designer)
        {
            this.OnPaint(graphics, true, WorkflowTheme.CurrentTheme.AmbientTheme, designer);
        }

        public virtual Rectangle GetBounds(ActivityDesigner designer, bool activated)
        {
            if (designer == null)
            {
                throw new ArgumentNullException("designer");
            }
            return designer.Bounds;
        }

        protected virtual void OnActivate(ActivityDesigner designer)
        {
        }

        internal static int OnComparePriority(DesignerGlyph x, DesignerGlyph y)
        {
            return (y.Priority - x.Priority);
        }

        protected abstract void OnPaint(Graphics graphics, bool activated, AmbientTheme ambientTheme, ActivityDesigner designer);

        public virtual bool CanBeActivated
        {
            get
            {
                return false;
            }
        }

        public virtual int Priority
        {
            get
            {
                return 0x2710;
            }
        }
    }
}

