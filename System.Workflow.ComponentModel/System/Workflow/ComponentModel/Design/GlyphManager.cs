namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal class GlyphManager : WorkflowDesignerMessageFilter, IDesignerGlyphProviderService
    {
        private ActivityDesigner activeDesigner;
        private DesignerGlyph activeGlyph;
        private List<IDesignerGlyphProvider> designerGlyphProviders = new List<IDesignerGlyphProvider>();

        internal GlyphManager()
        {
        }

        protected override void Dispose(bool disposing)
        {
            this.designerGlyphProviders.Clear();
            this.activeGlyph = null;
            this.activeDesigner = null;
            IServiceContainer service = base.GetService(typeof(IServiceContainer)) as IServiceContainer;
            if ((service != null) && (base.GetService(typeof(IDesignerGlyphProviderService)) != null))
            {
                service.RemoveService(typeof(IDesignerGlyphProviderService));
            }
            base.Dispose(disposing);
        }

        internal void DrawDesignerGlyphs(ActivityDesignerPaintEventArgs e, ActivityDesigner designer)
        {
            foreach (DesignerGlyph glyph in this.GetDesignerGlyphs(designer))
            {
                glyph.Draw(e.Graphics, designer);
            }
            if ((this.activeGlyph != null) && (designer == this.activeDesigner))
            {
                this.activeGlyph.DrawActivated(e.Graphics, this.activeDesigner);
            }
        }

        private ActivityDesigner[] GetActivityDesigners(Rectangle logicalViewPort)
        {
            List<ActivityDesigner> list = new List<ActivityDesigner>();
            bool isEmpty = logicalViewPort.IsEmpty;
            ActivityDesigner safeRootDesigner = ActivityDesigner.GetSafeRootDesigner(base.ParentView);
            if (safeRootDesigner != null)
            {
                Stack<object> stack = new Stack<object>();
                stack.Push(safeRootDesigner);
                CompositeActivityDesigner designer2 = safeRootDesigner as CompositeActivityDesigner;
                if ((designer2 != null) && (designer2.ContainedDesigners.Count > 0))
                {
                    stack.Push(designer2.ContainedDesigners);
                }
                while (stack.Count > 0)
                {
                    object obj2 = stack.Pop();
                    ICollection is2 = obj2 as ICollection;
                    if (is2 != null)
                    {
                        foreach (ActivityDesigner designer3 in is2)
                        {
                            if ((isEmpty || logicalViewPort.IntersectsWith(designer3.Bounds)) && designer3.IsVisible)
                            {
                                stack.Push(designer3);
                                designer2 = designer3 as CompositeActivityDesigner;
                                if ((designer2 != null) && (designer2.ContainedDesigners.Count > 0))
                                {
                                    stack.Push(designer2.ContainedDesigners);
                                }
                            }
                        }
                    }
                    else
                    {
                        list.Add((ActivityDesigner) obj2);
                    }
                }
            }
            return list.ToArray();
        }

        internal ActivityDesignerGlyphCollection GetDesignerGlyphs(ActivityDesigner designer)
        {
            ActivityDesignerGlyphCollection glyphs = new ActivityDesignerGlyphCollection();
            if (designer.Glyphs != null)
            {
                glyphs.AddRange(designer.Glyphs);
            }
            foreach (IDesignerGlyphProvider provider in this.designerGlyphProviders)
            {
                ActivityDesignerGlyphCollection collection = provider.GetGlyphs(designer);
                if (collection != null)
                {
                    glyphs.AddRange(collection);
                }
            }
            glyphs.Sort(new Comparison<DesignerGlyph>(DesignerGlyph.OnComparePriority));
            return glyphs;
        }

        private DesignerGlyph GlyphFromPoint(Point point, out ActivityDesigner activityDesigner)
        {
            activityDesigner = null;
            WorkflowView parentView = base.ParentView;
            if (parentView != null)
            {
                RectangleCollection rectangles = new RectangleCollection();
                foreach (ActivityDesigner designer in this.GetActivityDesigners(parentView.ClientRectangleToLogical(new Rectangle(Point.Empty, parentView.ViewPortSize))))
                {
                    if (!rectangles.IsPointInsideAnyRectangle(point))
                    {
                        foreach (DesignerGlyph glyph in this.GetDesignerGlyphs(designer))
                        {
                            if (glyph.GetBounds(designer, false).Contains(point) && glyph.CanBeActivated)
                            {
                                activityDesigner = designer;
                                return glyph;
                            }
                        }
                    }
                    rectangles.AddRectangle(designer.Bounds);
                }
            }
            return null;
        }

        protected override void Initialize(WorkflowView parentView)
        {
            base.Initialize(parentView);
            IServiceContainer service = base.GetService(typeof(IServiceContainer)) as IServiceContainer;
            if (service != null)
            {
                if (base.GetService(typeof(IDesignerGlyphProviderService)) != null)
                {
                    service.RemoveService(typeof(IDesignerGlyphProviderService));
                }
                service.AddService(typeof(IDesignerGlyphProviderService), this);
            }
        }

        protected override bool OnMouseDoubleClick(MouseEventArgs eventArgs)
        {
            if (this.activeGlyph != null)
            {
                this.activeGlyph.Activate(this.activeDesigner);
                return true;
            }
            return false;
        }

        protected override bool OnMouseDown(MouseEventArgs eventArgs)
        {
            if (this.activeGlyph != null)
            {
                this.activeGlyph.Activate(this.activeDesigner);
                return true;
            }
            return false;
        }

        protected override bool OnMouseEnter(MouseEventArgs eventArgs)
        {
            this.RefreshActiveGlyph(base.ParentView.ClientPointToLogical(new Point(eventArgs.X, eventArgs.Y)));
            return false;
        }

        protected override bool OnMouseHover(MouseEventArgs eventArgs)
        {
            this.RefreshActiveGlyph(base.ParentView.ClientPointToLogical(new Point(eventArgs.X, eventArgs.Y)));
            return false;
        }

        protected override bool OnMouseMove(MouseEventArgs eventArgs)
        {
            this.RefreshActiveGlyph(base.ParentView.ClientPointToLogical(new Point(eventArgs.X, eventArgs.Y)));
            return false;
        }

        private void RefreshActiveGlyph(Point point)
        {
            WorkflowView parentView = base.ParentView;
            if (parentView != null)
            {
                DesignerGlyph activeGlyph = this.activeGlyph;
                if ((this.activeGlyph == null) || !this.activeGlyph.GetBounds(this.activeDesigner, true).Contains(point))
                {
                    ActivityDesigner activityDesigner = null;
                    DesignerGlyph glyph2 = this.GlyphFromPoint(point, out activityDesigner);
                    if (this.activeGlyph != null)
                    {
                        parentView.InvalidateLogicalRectangle(this.activeGlyph.GetBounds(this.activeDesigner, true));
                    }
                    this.activeGlyph = glyph2;
                    this.activeDesigner = activityDesigner;
                    if (this.activeGlyph != null)
                    {
                        parentView.InvalidateLogicalRectangle(this.activeGlyph.GetBounds(this.activeDesigner, true));
                    }
                }
                if (activeGlyph != this.activeGlyph)
                {
                    if ((this.activeGlyph != null) && this.activeGlyph.CanBeActivated)
                    {
                        parentView.Cursor = Cursors.Hand;
                    }
                    else if (parentView.Cursor == Cursors.Hand)
                    {
                        parentView.Cursor = Cursors.Default;
                    }
                }
            }
        }

        void IDesignerGlyphProviderService.AddGlyphProvider(IDesignerGlyphProvider glyphProvider)
        {
            if (!this.designerGlyphProviders.Contains(glyphProvider))
            {
                this.designerGlyphProviders.Add(glyphProvider);
                base.ParentView.InvalidateClientRectangle(Rectangle.Empty);
            }
        }

        void IDesignerGlyphProviderService.RemoveGlyphProvider(IDesignerGlyphProvider glyphProvider)
        {
            this.designerGlyphProviders.Remove(glyphProvider);
            base.ParentView.InvalidateClientRectangle(Rectangle.Empty);
        }

        ReadOnlyCollection<IDesignerGlyphProvider> IDesignerGlyphProviderService.GlyphProviders
        {
            get
            {
                return this.designerGlyphProviders.AsReadOnly();
            }
        }

        private class RectangleCollection
        {
            private List<Rectangle> rectangles = new List<Rectangle>();

            internal void AddRectangle(Rectangle rectangle)
            {
                this.rectangles.Add(rectangle);
            }

            internal bool IsPointInsideAnyRectangle(Point p)
            {
                for (int i = 0; i < this.rectangles.Count; i++)
                {
                    Rectangle rectangle = this.rectangles[i];
                    if (rectangle.Contains(p))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}

