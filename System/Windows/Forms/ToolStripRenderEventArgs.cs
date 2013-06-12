namespace System.Windows.Forms
{
    using System;
    using System.Drawing;

    public class ToolStripRenderEventArgs : EventArgs
    {
        private Rectangle affectedBounds;
        private Color backColor;
        private System.Drawing.Graphics graphics;
        private System.Windows.Forms.ToolStrip toolStrip;

        public ToolStripRenderEventArgs(System.Drawing.Graphics g, System.Windows.Forms.ToolStrip toolStrip)
        {
            this.affectedBounds = Rectangle.Empty;
            this.backColor = Color.Empty;
            this.toolStrip = toolStrip;
            this.graphics = g;
            this.affectedBounds = new Rectangle(Point.Empty, toolStrip.Size);
        }

        public ToolStripRenderEventArgs(System.Drawing.Graphics g, System.Windows.Forms.ToolStrip toolStrip, Rectangle affectedBounds, Color backColor)
        {
            this.affectedBounds = Rectangle.Empty;
            this.backColor = Color.Empty;
            this.toolStrip = toolStrip;
            this.affectedBounds = affectedBounds;
            this.graphics = g;
            this.backColor = backColor;
        }

        public Rectangle AffectedBounds
        {
            get
            {
                return this.affectedBounds;
            }
        }

        public Color BackColor
        {
            get
            {
                if (this.backColor == Color.Empty)
                {
                    this.backColor = this.toolStrip.RawBackColor;
                    if (this.backColor == Color.Empty)
                    {
                        if (this.toolStrip is ToolStripDropDown)
                        {
                            this.backColor = SystemColors.Menu;
                        }
                        else if (this.toolStrip is MenuStrip)
                        {
                            this.backColor = SystemColors.MenuBar;
                        }
                        else
                        {
                            this.backColor = SystemColors.Control;
                        }
                    }
                }
                return this.backColor;
            }
        }

        public Rectangle ConnectedArea
        {
            get
            {
                ToolStripDropDown toolStrip = this.toolStrip as ToolStripDropDown;
                if (toolStrip != null)
                {
                    ToolStripDropDownItem ownerItem = toolStrip.OwnerItem as ToolStripDropDownItem;
                    if (ownerItem is MdiControlStrip.SystemMenuItem)
                    {
                        return Rectangle.Empty;
                    }
                    if (((ownerItem != null) && (ownerItem.ParentInternal != null)) && !ownerItem.IsOnDropDown)
                    {
                        Rectangle rect = new Rectangle(this.toolStrip.PointToClient(ownerItem.TranslatePoint(Point.Empty, ToolStripPointType.ToolStripItemCoords, ToolStripPointType.ScreenCoords)), ownerItem.Size);
                        Rectangle bounds = this.ToolStrip.Bounds;
                        Rectangle clientRectangle = this.ToolStrip.ClientRectangle;
                        clientRectangle.Inflate(1, 1);
                        if (clientRectangle.IntersectsWith(rect))
                        {
                            switch (ownerItem.DropDownDirection)
                            {
                                case ToolStripDropDownDirection.AboveLeft:
                                case ToolStripDropDownDirection.AboveRight:
                                    return Rectangle.Empty;

                                case ToolStripDropDownDirection.BelowLeft:
                                case ToolStripDropDownDirection.BelowRight:
                                    clientRectangle.Intersect(rect);
                                    if (clientRectangle.Height != 2)
                                    {
                                        return Rectangle.Empty;
                                    }
                                    return new Rectangle(rect.X + 1, 0, rect.Width - 2, 2);

                                case ToolStripDropDownDirection.Left:
                                case ToolStripDropDownDirection.Right:
                                    return Rectangle.Empty;
                            }
                        }
                    }
                }
                return Rectangle.Empty;
            }
        }

        public System.Drawing.Graphics Graphics
        {
            get
            {
                return this.graphics;
            }
        }

        public System.Windows.Forms.ToolStrip ToolStrip
        {
            get
            {
                return this.toolStrip;
            }
        }
    }
}

