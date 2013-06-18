namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal class ToolboxItemSnapLineBehavior : System.Windows.Forms.Design.Behavior.Behavior
    {
        private BehaviorService behaviorService;
        private ControlDesigner designer;
        private DragAssistanceManager dragManager;
        private bool isPushed;
        private Point lastOffset;
        private Rectangle lastRectangle;
        private IServiceProvider serviceProvider;
        private StatusCommandUI statusCommandUI;
        private bool targetAllowsDragBox;
        private bool targetAllowsSnapLines;

        public ToolboxItemSnapLineBehavior(IServiceProvider serviceProvider, BehaviorService behaviorService)
        {
            this.serviceProvider = serviceProvider;
            this.behaviorService = behaviorService;
            this.designer = null;
            this.isPushed = false;
            this.lastRectangle = Rectangle.Empty;
            this.lastOffset = Point.Empty;
            this.statusCommandUI = new StatusCommandUI(serviceProvider);
            this.targetAllowsDragBox = true;
            this.targetAllowsSnapLines = true;
        }

        public ToolboxItemSnapLineBehavior(IServiceProvider serviceProvider, BehaviorService behaviorService, ControlDesigner controlDesigner) : this(serviceProvider, behaviorService)
        {
            this.designer = controlDesigner;
            if ((controlDesigner != null) && !controlDesigner.ParticipatesWithSnapLines)
            {
                this.targetAllowsSnapLines = false;
            }
        }

        public ToolboxItemSnapLineBehavior(IServiceProvider serviceProvider, BehaviorService behaviorService, ControlDesigner controlDesigner, bool allowDragBox) : this(serviceProvider, behaviorService, controlDesigner)
        {
            this.designer = controlDesigner;
            this.targetAllowsDragBox = allowDragBox;
        }

        private ToolboxSnapDragDropEventArgs CreateToolboxSnapArgs(DragEventArgs e, Point mouseLoc)
        {
            ToolboxSnapDragDropEventArgs.SnapDirection none = ToolboxSnapDragDropEventArgs.SnapDirection.None;
            Point empty = Point.Empty;
            bool flag = false;
            bool flag2 = false;
            if (this.dragManager != null)
            {
                foreach (DragAssistanceManager.Line line in this.dragManager.GetRecentLines())
                {
                    if (line.LineType == DragAssistanceManager.LineType.Standard)
                    {
                        if (!flag && (line.x1 == line.x2))
                        {
                            if (line.x1 == this.lastRectangle.Left)
                            {
                                none |= ToolboxSnapDragDropEventArgs.SnapDirection.Left;
                                empty.X = this.lastRectangle.Left - mouseLoc.X;
                            }
                            else
                            {
                                none |= ToolboxSnapDragDropEventArgs.SnapDirection.Right;
                                empty.X = this.lastRectangle.Right - mouseLoc.X;
                            }
                            flag = true;
                        }
                        else if (!flag2 && (line.y1 == line.y2))
                        {
                            if (line.y1 == this.lastRectangle.Top)
                            {
                                none |= ToolboxSnapDragDropEventArgs.SnapDirection.Top;
                                empty.Y = this.lastRectangle.Top - mouseLoc.Y;
                            }
                            else if (line.y1 == this.lastRectangle.Bottom)
                            {
                                none |= ToolboxSnapDragDropEventArgs.SnapDirection.Bottom;
                                empty.Y = this.lastRectangle.Bottom - mouseLoc.Y;
                            }
                            flag2 = true;
                        }
                    }
                    else if ((line.LineType == DragAssistanceManager.LineType.Margin) || (line.LineType == DragAssistanceManager.LineType.Padding))
                    {
                        if (!flag2 && (line.x1 == line.x2))
                        {
                            if (Math.Max(line.y1, line.y2) <= this.lastRectangle.Top)
                            {
                                none |= ToolboxSnapDragDropEventArgs.SnapDirection.Top;
                                empty.Y = this.lastRectangle.Top - mouseLoc.Y;
                            }
                            else
                            {
                                none |= ToolboxSnapDragDropEventArgs.SnapDirection.Bottom;
                                empty.Y = this.lastRectangle.Bottom - mouseLoc.Y;
                            }
                            flag2 = true;
                        }
                        else if (!flag && (line.y1 == line.y2))
                        {
                            if (Math.Max(line.x1, line.x2) <= this.lastRectangle.Left)
                            {
                                none |= ToolboxSnapDragDropEventArgs.SnapDirection.Left;
                                empty.X = this.lastRectangle.Left - mouseLoc.X;
                            }
                            else
                            {
                                none |= ToolboxSnapDragDropEventArgs.SnapDirection.Right;
                                empty.X = this.lastRectangle.Right - mouseLoc.X;
                            }
                            flag = true;
                        }
                    }
                    if (flag && flag2)
                    {
                        break;
                    }
                }
            }
            if (!flag)
            {
                none |= ToolboxSnapDragDropEventArgs.SnapDirection.Left;
                empty.X = this.lastRectangle.Left - mouseLoc.X;
            }
            if (!flag2)
            {
                none |= ToolboxSnapDragDropEventArgs.SnapDirection.Top;
                empty.Y = this.lastRectangle.Top - mouseLoc.Y;
            }
            return new ToolboxSnapDragDropEventArgs(none, empty, e);
        }

        private SnapLine[] GenerateNewToolSnapLines(Rectangle r)
        {
            return new SnapLine[] { new SnapLine(SnapLineType.Left, r.Left), new SnapLine(SnapLineType.Right, r.Right), new SnapLine(SnapLineType.Bottom, r.Bottom), new SnapLine(SnapLineType.Top, r.Top), new SnapLine(SnapLineType.Horizontal, r.Top - 4, "Margin.Top", SnapLinePriority.Always), new SnapLine(SnapLineType.Horizontal, r.Bottom + 3, "Margin.Bottom", SnapLinePriority.Always), new SnapLine(SnapLineType.Vertical, r.Left - 4, "Margin.Left", SnapLinePriority.Always), new SnapLine(SnapLineType.Vertical, r.Right + 3, "Margin.Right", SnapLinePriority.Always) };
        }

        public void OnBeginDrag()
        {
            Adorner bodyGlyphAdorner = null;
            SelectionManager service = (SelectionManager) this.serviceProvider.GetService(typeof(SelectionManager));
            if (service != null)
            {
                bodyGlyphAdorner = service.BodyGlyphAdorner;
            }
            ArrayList list = new ArrayList();
            foreach (ControlBodyGlyph glyph in bodyGlyphAdorner.Glyphs)
            {
                Control relatedComponent = glyph.RelatedComponent as Control;
                if ((relatedComponent != null) && !relatedComponent.AllowDrop)
                {
                    list.Add(glyph);
                }
            }
            foreach (Glyph glyph2 in list)
            {
                bodyGlyphAdorner.Glyphs.Remove(glyph2);
            }
        }

        public override void OnDragDrop(Glyph g, DragEventArgs e)
        {
            this.behaviorService.PopBehavior(this);
            try
            {
                Point point = this.behaviorService.AdornerWindowToScreen();
                ToolboxSnapDragDropEventArgs args = this.CreateToolboxSnapArgs(e, new Point(e.X - point.X, e.Y - point.Y));
                base.OnDragDrop(g, args);
            }
            finally
            {
                this.IsPushed = false;
            }
        }

        public override bool OnMouseMove(Glyph g, MouseButtons button, Point mouseLoc)
        {
            bool flag = Control.ModifierKeys == Keys.Alt;
            if (flag && (this.dragManager != null))
            {
                this.dragManager.EraseSnapLines();
            }
            bool flag2 = base.OnMouseMove(g, button, mouseLoc);
            Rectangle dragBounds = new Rectangle(mouseLoc.X - (DesignerUtils.BOXIMAGESIZE / 2), mouseLoc.Y - (DesignerUtils.BOXIMAGESIZE / 2), DesignerUtils.BOXIMAGESIZE, DesignerUtils.BOXIMAGESIZE);
            if (dragBounds != this.lastRectangle)
            {
                if (((this.dragManager != null) && this.targetAllowsSnapLines) && !flag)
                {
                    this.lastOffset = this.dragManager.OnMouseMove(dragBounds, this.GenerateNewToolSnapLines(dragBounds));
                    dragBounds.Offset(this.lastOffset.X, this.lastOffset.Y);
                }
                if (!this.lastRectangle.IsEmpty)
                {
                    using (Region region = new Region(this.lastRectangle))
                    {
                        region.Exclude(dragBounds);
                        this.behaviorService.Invalidate(region);
                    }
                }
                if (this.targetAllowsDragBox)
                {
                    using (Graphics graphics = this.behaviorService.AdornerWindowGraphics)
                    {
                        graphics.DrawImage(DesignerUtils.BoxImage, dragBounds.Location);
                    }
                }
                IDesignerHost service = (IDesignerHost) this.serviceProvider.GetService(typeof(IDesignerHost));
                if (service != null)
                {
                    Control rootComponent = service.RootComponent as Control;
                    if (rootComponent != null)
                    {
                        Point point = this.behaviorService.MapAdornerWindowPoint(rootComponent.Handle, new Point(0, 0));
                        Rectangle bounds = new Rectangle(dragBounds.X - point.X, dragBounds.Y - point.Y, 0, 0);
                        if (this.statusCommandUI != null)
                        {
                            this.statusCommandUI.SetStatusInformation(bounds);
                        }
                    }
                }
                if (((this.dragManager != null) && this.targetAllowsSnapLines) && !flag)
                {
                    this.dragManager.RenderSnapLinesInternal();
                }
                this.lastRectangle = dragBounds;
            }
            return flag2;
        }

        public bool IsPushed
        {
            get
            {
                return this.isPushed;
            }
            set
            {
                this.isPushed = value;
                if (this.isPushed)
                {
                    if (this.dragManager == null)
                    {
                        this.dragManager = new DragAssistanceManager(this.serviceProvider);
                    }
                }
                else
                {
                    if (!this.lastRectangle.IsEmpty)
                    {
                        this.behaviorService.Invalidate(this.lastRectangle);
                    }
                    this.lastOffset = Point.Empty;
                    this.lastRectangle = Rectangle.Empty;
                    if (this.dragManager != null)
                    {
                        this.dragManager.OnMouseUp();
                        this.dragManager = null;
                    }
                }
            }
        }
    }
}

