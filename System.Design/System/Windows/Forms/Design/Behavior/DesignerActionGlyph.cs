namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal sealed class DesignerActionGlyph : Glyph
    {
        private Adorner adorner;
        private Rectangle alternativeBounds;
        private Control alternativeParent;
        private Rectangle bounds;
        internal const int CONTROLOVERLAP_X = 5;
        internal const int CONTROLOVERLAP_Y = 2;
        private DockStyle dockStyle;
        private Bitmap glyphImageClosed;
        private Bitmap glyphImageOpened;
        private bool mouseOver;

        public DesignerActionGlyph(DesignerActionBehavior behavior, Adorner adorner) : this(behavior, adorner, Rectangle.Empty, null)
        {
        }

        public DesignerActionGlyph(DesignerActionBehavior behavior, Rectangle alternativeBounds, Control alternativeParent) : this(behavior, null, alternativeBounds, alternativeParent)
        {
        }

        private DesignerActionGlyph(DesignerActionBehavior behavior, Adorner adorner, Rectangle alternativeBounds, Control alternativeParent) : base(behavior)
        {
            this.alternativeBounds = Rectangle.Empty;
            this.adorner = adorner;
            this.alternativeBounds = alternativeBounds;
            this.alternativeParent = alternativeParent;
            this.Invalidate();
        }

        public override Cursor GetHitTest(Point p)
        {
            if (this.bounds.Contains(p))
            {
                this.MouseOver = true;
                return Cursors.Default;
            }
            this.MouseOver = false;
            return null;
        }

        internal void Invalidate()
        {
            IComponent relatedComponent = ((DesignerActionBehavior) this.Behavior).RelatedComponent;
            Point empty = Point.Empty;
            Control c = relatedComponent as Control;
            if (((c != null) && !(relatedComponent is ToolStripDropDown)) && (this.adorner != null))
            {
                empty = this.adorner.BehaviorService.ControlToAdornerWindow(c);
                empty.X += c.Width;
            }
            else
            {
                ComponentTray alternativeParent = this.alternativeParent as ComponentTray;
                if (alternativeParent != null)
                {
                    ComponentTray.TrayControl trayControlFromComponent = alternativeParent.GetTrayControlFromComponent(relatedComponent);
                    if (trayControlFromComponent != null)
                    {
                        this.alternativeBounds = trayControlFromComponent.Bounds;
                    }
                }
                Rectangle boundsForNoResizeSelectionType = DesignerUtils.GetBoundsForNoResizeSelectionType(this.alternativeBounds, SelectionBorderGlyphType.Top);
                empty.X = boundsForNoResizeSelectionType.Right;
                empty.Y = boundsForNoResizeSelectionType.Top;
            }
            empty.X -= this.GlyphImageOpened.Width + 5;
            empty.Y -= this.GlyphImageOpened.Height - 2;
            this.bounds = new Rectangle(empty.X, empty.Y, this.GlyphImageOpened.Width, this.GlyphImageOpened.Height);
        }

        internal void InvalidateOwnerLocation()
        {
            if (this.alternativeParent != null)
            {
                this.alternativeParent.Invalidate(this.bounds);
            }
            else
            {
                this.adorner.Invalidate(this.bounds);
            }
        }

        public override void Paint(PaintEventArgs pe)
        {
            if (this.Behavior is DesignerActionBehavior)
            {
                Image glyphImageOpened;
                IComponent lastPanelComponent = ((DesignerActionBehavior) this.Behavior).ParentUI.LastPanelComponent;
                IComponent relatedComponent = ((DesignerActionBehavior) this.Behavior).RelatedComponent;
                if ((lastPanelComponent != null) && (lastPanelComponent == relatedComponent))
                {
                    glyphImageOpened = this.GlyphImageOpened;
                }
                else
                {
                    glyphImageOpened = this.GlyphImageClosed;
                }
                pe.Graphics.DrawImage(glyphImageOpened, this.bounds.Left, this.bounds.Top);
                if (this.MouseOver || ((lastPanelComponent != null) && (lastPanelComponent == relatedComponent)))
                {
                    pe.Graphics.FillRectangle(DesignerUtils.HoverBrush, Rectangle.Inflate(this.bounds, -1, -1));
                }
            }
        }

        internal void UpdateAlternativeBounds(Rectangle newBounds)
        {
            this.alternativeBounds = newBounds;
            this.Invalidate();
        }

        public override Rectangle Bounds
        {
            get
            {
                return this.bounds;
            }
        }

        public DockStyle DockEdge
        {
            get
            {
                return this.dockStyle;
            }
            set
            {
                if (this.dockStyle != value)
                {
                    this.dockStyle = value;
                }
            }
        }

        private Image GlyphImageClosed
        {
            get
            {
                if (this.glyphImageClosed == null)
                {
                    this.glyphImageClosed = new Bitmap(typeof(DesignerActionGlyph), "Close_left.bmp");
                    this.glyphImageClosed.MakeTransparent(Color.Magenta);
                }
                return this.glyphImageClosed;
            }
        }

        private Image GlyphImageOpened
        {
            get
            {
                if (this.glyphImageOpened == null)
                {
                    this.glyphImageOpened = new Bitmap(typeof(DesignerActionGlyph), "Open_left.bmp");
                    this.glyphImageOpened.MakeTransparent(Color.Magenta);
                }
                return this.glyphImageOpened;
            }
        }

        public bool IsInComponentTray
        {
            get
            {
                return (this.adorner == null);
            }
        }

        private bool MouseOver
        {
            get
            {
                return this.mouseOver;
            }
            set
            {
                if (this.mouseOver != value)
                {
                    this.mouseOver = value;
                    this.InvalidateOwnerLocation();
                }
            }
        }
    }
}

