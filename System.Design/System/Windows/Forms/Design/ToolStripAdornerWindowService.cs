namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.Design;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal sealed class ToolStripAdornerWindowService : IDisposable
    {
        private BehaviorService bs;
        private Adorner dropDownAdorner;
        private ArrayList dropDownCollection;
        private IOverlayService os;
        private IServiceProvider serviceProvider;
        private ToolStripAdornerWindow toolStripAdornerWindow;

        internal ToolStripAdornerWindowService(IServiceProvider serviceProvider, Control windowFrame)
        {
            this.serviceProvider = serviceProvider;
            this.toolStripAdornerWindow = new ToolStripAdornerWindow(windowFrame);
            this.bs = (BehaviorService) serviceProvider.GetService(typeof(BehaviorService));
            int adornerWindowIndex = this.bs.AdornerWindowIndex;
            this.os = (IOverlayService) serviceProvider.GetService(typeof(IOverlayService));
            if (this.os != null)
            {
                this.os.InsertOverlay(this.toolStripAdornerWindow, adornerWindowIndex);
            }
            this.dropDownAdorner = new Adorner();
            int count = this.bs.Adorners.Count;
            if (count > 1)
            {
                this.bs.Adorners.Insert(count - 1, this.dropDownAdorner);
            }
        }

        public Point AdornerWindowPointToScreen(Point p)
        {
            System.Design.NativeMethods.POINT pt = new System.Design.NativeMethods.POINT(p.X, p.Y);
            System.Design.NativeMethods.MapWindowPoints(this.toolStripAdornerWindow.Handle, IntPtr.Zero, pt, 1);
            return new Point(pt.x, pt.y);
        }

        public Point AdornerWindowToScreen()
        {
            Point p = new Point(0, 0);
            return this.AdornerWindowPointToScreen(p);
        }

        public Point ControlToAdornerWindow(Control c)
        {
            if (c.Parent == null)
            {
                return Point.Empty;
            }
            System.Design.NativeMethods.POINT pt = new System.Design.NativeMethods.POINT {
                x = c.Left,
                y = c.Top
            };
            System.Design.NativeMethods.MapWindowPoints(c.Parent.Handle, this.toolStripAdornerWindow.Handle, pt, 1);
            return new Point(pt.x, pt.y);
        }

        public void Dispose()
        {
            if (this.os != null)
            {
                this.os.RemoveOverlay(this.toolStripAdornerWindow);
            }
            this.toolStripAdornerWindow.Dispose();
            if (this.bs != null)
            {
                this.bs.Adorners.Remove(this.dropDownAdorner);
                this.bs = null;
            }
            if (this.dropDownAdorner != null)
            {
                this.dropDownAdorner.Glyphs.Clear();
                this.dropDownAdorner = null;
            }
        }

        public void Invalidate()
        {
            this.toolStripAdornerWindow.InvalidateAdornerWindow();
        }

        public void Invalidate(Rectangle rect)
        {
            this.toolStripAdornerWindow.InvalidateAdornerWindow(rect);
        }

        public void Invalidate(Region r)
        {
            this.toolStripAdornerWindow.InvalidateAdornerWindow(r);
        }

        internal void ProcessPaintMessage(Rectangle paintRect)
        {
            this.toolStripAdornerWindow.Invalidate(paintRect);
        }

        internal Adorner DropDownAdorner
        {
            get
            {
                return this.dropDownAdorner;
            }
        }

        internal ArrayList DropDowns
        {
            get
            {
                return this.dropDownCollection;
            }
            set
            {
                if (this.dropDownCollection == null)
                {
                    this.dropDownCollection = new ArrayList();
                }
            }
        }

        internal Control ToolStripAdornerWindowControl
        {
            get
            {
                return this.toolStripAdornerWindow;
            }
        }

        public Graphics ToolStripAdornerWindowGraphics
        {
            get
            {
                return this.toolStripAdornerWindow.CreateGraphics();
            }
        }

        private class ToolStripAdornerWindow : Control
        {
            private Control designerFrame;

            internal ToolStripAdornerWindow(Control designerFrame)
            {
                this.designerFrame = designerFrame;
                this.Dock = DockStyle.Fill;
                this.AllowDrop = true;
                this.Text = "ToolStripAdornerWindow";
                base.SetStyle(ControlStyles.Opaque, true);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing && (this.designerFrame != null))
                {
                    this.designerFrame = null;
                }
                base.Dispose(disposing);
            }

            internal void InvalidateAdornerWindow()
            {
                if (this.DesignerFrameValid)
                {
                    this.designerFrame.Invalidate(true);
                    this.designerFrame.Update();
                }
            }

            internal void InvalidateAdornerWindow(Rectangle rectangle)
            {
                if (this.DesignerFrameValid)
                {
                    this.designerFrame.Invalidate(rectangle, true);
                    this.designerFrame.Update();
                }
            }

            internal void InvalidateAdornerWindow(Region region)
            {
                if (this.DesignerFrameValid)
                {
                    this.designerFrame.Invalidate(region, true);
                    this.designerFrame.Update();
                }
            }

            protected override void OnHandleCreated(EventArgs e)
            {
                base.OnHandleCreated(e);
            }

            protected override void OnHandleDestroyed(EventArgs e)
            {
                base.OnHandleDestroyed(e);
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == 0x84)
                {
                    m.Result = (IntPtr) (-1);
                }
                else
                {
                    base.WndProc(ref m);
                }
            }

            protected override System.Windows.Forms.CreateParams CreateParams
            {
                get
                {
                    System.Windows.Forms.CreateParams createParams = base.CreateParams;
                    createParams.Style &= -100663297;
                    createParams.ExStyle |= 0x20;
                    return createParams;
                }
            }

            private bool DesignerFrameValid
            {
                get
                {
                    return (((this.designerFrame != null) && !this.designerFrame.IsDisposed) && this.designerFrame.IsHandleCreated);
                }
            }
        }
    }
}

