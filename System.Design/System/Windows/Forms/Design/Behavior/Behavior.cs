namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Windows.Forms;

    public abstract class Behavior
    {
        private BehaviorService bhvSvc;
        private bool callParentBehavior;

        protected Behavior()
        {
        }

        protected Behavior(bool callParentBehavior, BehaviorService behaviorService)
        {
            if (callParentBehavior && (behaviorService == null))
            {
                throw new ArgumentException("behaviorService");
            }
            this.callParentBehavior = callParentBehavior;
            this.bhvSvc = behaviorService;
        }

        public virtual MenuCommand FindCommand(CommandID commandId)
        {
            try
            {
                if (this.callParentBehavior && (this.GetNextBehavior != null))
                {
                    return this.GetNextBehavior.FindCommand(commandId);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private bool GlyphIsValid(Glyph g)
        {
            return (((g != null) && (g.Behavior != null)) && (g.Behavior != this));
        }

        public virtual void OnDragDrop(Glyph g, DragEventArgs e)
        {
            if (this.callParentBehavior && (this.GetNextBehavior != null))
            {
                this.GetNextBehavior.OnDragDrop(g, e);
            }
            else if (this.GlyphIsValid(g))
            {
                g.Behavior.OnDragDrop(g, e);
            }
        }

        public virtual void OnDragEnter(Glyph g, DragEventArgs e)
        {
            if (this.callParentBehavior && (this.GetNextBehavior != null))
            {
                this.GetNextBehavior.OnDragEnter(g, e);
            }
            else if (this.GlyphIsValid(g))
            {
                g.Behavior.OnDragEnter(g, e);
            }
        }

        public virtual void OnDragLeave(Glyph g, EventArgs e)
        {
            if (this.callParentBehavior && (this.GetNextBehavior != null))
            {
                this.GetNextBehavior.OnDragLeave(g, e);
            }
            else if (this.GlyphIsValid(g))
            {
                g.Behavior.OnDragLeave(g, e);
            }
        }

        public virtual void OnDragOver(Glyph g, DragEventArgs e)
        {
            if (this.callParentBehavior && (this.GetNextBehavior != null))
            {
                this.GetNextBehavior.OnDragOver(g, e);
            }
            else if (this.GlyphIsValid(g))
            {
                g.Behavior.OnDragOver(g, e);
            }
            else if (e.Effect != DragDropEffects.None)
            {
                e.Effect = (Control.ModifierKeys == Keys.Control) ? DragDropEffects.Copy : DragDropEffects.Move;
            }
        }

        public virtual void OnGiveFeedback(Glyph g, GiveFeedbackEventArgs e)
        {
            if (this.callParentBehavior && (this.GetNextBehavior != null))
            {
                this.GetNextBehavior.OnGiveFeedback(g, e);
            }
            else if (this.GlyphIsValid(g))
            {
                g.Behavior.OnGiveFeedback(g, e);
            }
        }

        public virtual void OnLoseCapture(Glyph g, EventArgs e)
        {
            if (this.callParentBehavior && (this.GetNextBehavior != null))
            {
                this.GetNextBehavior.OnLoseCapture(g, e);
            }
            else if (this.GlyphIsValid(g))
            {
                g.Behavior.OnLoseCapture(g, e);
            }
        }

        public virtual bool OnMouseDoubleClick(Glyph g, MouseButtons button, Point mouseLoc)
        {
            if (this.callParentBehavior && (this.GetNextBehavior != null))
            {
                return this.GetNextBehavior.OnMouseDoubleClick(g, button, mouseLoc);
            }
            return (this.GlyphIsValid(g) && g.Behavior.OnMouseDoubleClick(g, button, mouseLoc));
        }

        public virtual bool OnMouseDown(Glyph g, MouseButtons button, Point mouseLoc)
        {
            if (this.callParentBehavior && (this.GetNextBehavior != null))
            {
                return this.GetNextBehavior.OnMouseDown(g, button, mouseLoc);
            }
            return (this.GlyphIsValid(g) && g.Behavior.OnMouseDown(g, button, mouseLoc));
        }

        public virtual bool OnMouseEnter(Glyph g)
        {
            if (this.callParentBehavior && (this.GetNextBehavior != null))
            {
                return this.GetNextBehavior.OnMouseEnter(g);
            }
            return (this.GlyphIsValid(g) && g.Behavior.OnMouseEnter(g));
        }

        public virtual bool OnMouseHover(Glyph g, Point mouseLoc)
        {
            if (this.callParentBehavior && (this.GetNextBehavior != null))
            {
                return this.GetNextBehavior.OnMouseHover(g, mouseLoc);
            }
            return (this.GlyphIsValid(g) && g.Behavior.OnMouseHover(g, mouseLoc));
        }

        public virtual bool OnMouseLeave(Glyph g)
        {
            if (this.callParentBehavior && (this.GetNextBehavior != null))
            {
                return this.GetNextBehavior.OnMouseLeave(g);
            }
            return (this.GlyphIsValid(g) && g.Behavior.OnMouseLeave(g));
        }

        public virtual bool OnMouseMove(Glyph g, MouseButtons button, Point mouseLoc)
        {
            if (this.callParentBehavior && (this.GetNextBehavior != null))
            {
                return this.GetNextBehavior.OnMouseMove(g, button, mouseLoc);
            }
            return (this.GlyphIsValid(g) && g.Behavior.OnMouseMove(g, button, mouseLoc));
        }

        public virtual bool OnMouseUp(Glyph g, MouseButtons button)
        {
            if (this.callParentBehavior && (this.GetNextBehavior != null))
            {
                return this.GetNextBehavior.OnMouseUp(g, button);
            }
            return (this.GlyphIsValid(g) && g.Behavior.OnMouseUp(g, button));
        }

        public virtual void OnQueryContinueDrag(Glyph g, QueryContinueDragEventArgs e)
        {
            if (this.callParentBehavior && (this.GetNextBehavior != null))
            {
                this.GetNextBehavior.OnQueryContinueDrag(g, e);
            }
            else if (this.GlyphIsValid(g))
            {
                g.Behavior.OnQueryContinueDrag(g, e);
            }
        }

        public virtual System.Windows.Forms.Cursor Cursor
        {
            get
            {
                return Cursors.Default;
            }
        }

        public virtual bool DisableAllCommands
        {
            get
            {
                return ((this.callParentBehavior && (this.GetNextBehavior != null)) && this.GetNextBehavior.DisableAllCommands);
            }
        }

        private System.Windows.Forms.Design.Behavior.Behavior GetNextBehavior
        {
            get
            {
                if (this.bhvSvc != null)
                {
                    return this.bhvSvc.GetNextBehavior(this);
                }
                return null;
            }
        }
    }
}

