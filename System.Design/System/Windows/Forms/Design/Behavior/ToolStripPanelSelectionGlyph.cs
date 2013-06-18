namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class ToolStripPanelSelectionGlyph : ControlBodyGlyph
    {
        private Control baseParent;
        private BehaviorService behaviorService;
        private Rectangle glyphBounds;
        private Image image;
        private const int imageHeight = 6;
        private const int imageWidth = 50;
        private bool isExpanded;
        private IServiceProvider provider;
        private ToolStripPanelSelectionBehavior relatedBehavior;
        private ToolStripPanel relatedPanel;

        internal ToolStripPanelSelectionGlyph(Rectangle bounds, Cursor cursor, IComponent relatedComponent, IServiceProvider provider, ToolStripPanelSelectionBehavior behavior) : base(bounds, cursor, relatedComponent, behavior)
        {
            this.relatedBehavior = behavior;
            this.provider = provider;
            this.relatedPanel = relatedComponent as ToolStripPanel;
            this.behaviorService = (BehaviorService) provider.GetService(typeof(BehaviorService));
            if ((this.behaviorService != null) && (((IDesignerHost) provider.GetService(typeof(IDesignerHost))) != null))
            {
                this.UpdateGlyph();
            }
        }

        private void CollapseGlyph(Rectangle bounds)
        {
            DockStyle dock = this.relatedPanel.Dock;
            int num = 0;
            int num2 = 0;
            switch (dock)
            {
                case DockStyle.Top:
                    this.image = new Bitmap(typeof(ToolStripPanelSelectionGlyph), "topopen.bmp");
                    num = (bounds.Width - 50) / 2;
                    if (num <= 0)
                    {
                        break;
                    }
                    this.glyphBounds = new Rectangle(bounds.X + num, bounds.Y + bounds.Height, 50, 6);
                    return;

                case DockStyle.Bottom:
                    this.image = new Bitmap(typeof(ToolStripPanelSelectionGlyph), "bottomopen.bmp");
                    num = (bounds.Width - 50) / 2;
                    if (num <= 0)
                    {
                        break;
                    }
                    this.glyphBounds = new Rectangle(bounds.X + num, bounds.Y - 6, 50, 6);
                    return;

                case DockStyle.Left:
                    this.image = new Bitmap(typeof(ToolStripPanelSelectionGlyph), "leftopen.bmp");
                    num2 = (bounds.Height - 50) / 2;
                    if (num2 <= 0)
                    {
                        break;
                    }
                    this.glyphBounds = new Rectangle(bounds.X + bounds.Width, bounds.Y + num2, 6, 50);
                    return;

                case DockStyle.Right:
                    this.image = new Bitmap(typeof(ToolStripPanelSelectionGlyph), "rightopen.bmp");
                    num2 = (bounds.Height - 50) / 2;
                    if (num2 <= 0)
                    {
                        break;
                    }
                    this.glyphBounds = new Rectangle(bounds.X - 6, bounds.Y + num2, 6, 50);
                    return;

                default:
                    throw new Exception(System.Design.SR.GetString("ToolStripPanelGlyphUnsupportedDock"));
            }
        }

        private void ExpandGlyph(Rectangle bounds)
        {
            DockStyle dock = this.relatedPanel.Dock;
            int num = 0;
            int num2 = 0;
            switch (dock)
            {
                case DockStyle.Top:
                    this.image = new Bitmap(typeof(ToolStripPanelSelectionGlyph), "topclose.bmp");
                    num = (bounds.Width - 50) / 2;
                    if (num <= 0)
                    {
                        break;
                    }
                    this.glyphBounds = new Rectangle(bounds.X + num, bounds.Y + bounds.Height, 50, 6);
                    return;

                case DockStyle.Bottom:
                    this.image = new Bitmap(typeof(ToolStripPanelSelectionGlyph), "bottomclose.bmp");
                    num = (bounds.Width - 50) / 2;
                    if (num <= 0)
                    {
                        break;
                    }
                    this.glyphBounds = new Rectangle(bounds.X + num, bounds.Y - 6, 50, 6);
                    return;

                case DockStyle.Left:
                    this.image = new Bitmap(typeof(ToolStripPanelSelectionGlyph), "leftclose.bmp");
                    num2 = (bounds.Height - 50) / 2;
                    if (num2 <= 0)
                    {
                        break;
                    }
                    this.glyphBounds = new Rectangle(bounds.X + bounds.Width, bounds.Y + num2, 6, 50);
                    return;

                case DockStyle.Right:
                    this.image = new Bitmap(typeof(ToolStripPanelSelectionGlyph), "rightclose.bmp");
                    num2 = (bounds.Height - 50) / 2;
                    if (num2 <= 0)
                    {
                        break;
                    }
                    this.glyphBounds = new Rectangle(bounds.X - 6, bounds.Y + num2, 6, 50);
                    return;

                default:
                    throw new Exception(System.Design.SR.GetString("ToolStripPanelGlyphUnsupportedDock"));
            }
        }

        public override Cursor GetHitTest(Point p)
        {
            if ((this.behaviorService != null) && (this.baseParent != null))
            {
                Rectangle rectangle = this.behaviorService.ControlRectInAdornerWindow(this.baseParent);
                if (((this.glyphBounds != Rectangle.Empty) && rectangle.Contains(this.glyphBounds)) && this.glyphBounds.Contains(p))
                {
                    return Cursors.Hand;
                }
            }
            return null;
        }

        public override void Paint(PaintEventArgs pe)
        {
            if ((this.behaviorService != null) && (this.baseParent != null))
            {
                Rectangle rectangle = this.behaviorService.ControlRectInAdornerWindow(this.baseParent);
                if ((this.relatedPanel.Visible && (this.image != null)) && ((this.glyphBounds != Rectangle.Empty) && rectangle.Contains(this.glyphBounds)))
                {
                    pe.Graphics.DrawImage(this.image, this.glyphBounds.Left, this.glyphBounds.Top);
                }
            }
        }

        public void UpdateGlyph()
        {
            if (this.behaviorService != null)
            {
                Rectangle bounds = this.behaviorService.ControlRectInAdornerWindow(this.relatedPanel);
                this.glyphBounds = Rectangle.Empty;
                ToolStripContainer parent = this.relatedPanel.Parent as ToolStripContainer;
                if (parent != null)
                {
                    this.baseParent = parent.Parent;
                }
                if (!this.isExpanded)
                {
                    this.CollapseGlyph(bounds);
                }
                else
                {
                    this.ExpandGlyph(bounds);
                }
            }
        }

        public override Rectangle Bounds
        {
            get
            {
                return this.glyphBounds;
            }
        }

        public bool IsExpanded
        {
            get
            {
                return this.isExpanded;
            }
            set
            {
                if (value != this.isExpanded)
                {
                    this.isExpanded = value;
                    this.UpdateGlyph();
                }
            }
        }
    }
}

