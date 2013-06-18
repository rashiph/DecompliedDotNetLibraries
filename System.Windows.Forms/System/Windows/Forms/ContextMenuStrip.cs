namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true), System.Windows.Forms.SRDescription("DescriptionContextMenuStrip"), DefaultEvent("Opening")]
    public class ContextMenuStrip : ToolStripDropDownMenu
    {
        public ContextMenuStrip()
        {
        }

        public ContextMenuStrip(IContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            container.Add(this);
        }

        internal ContextMenuStrip Clone()
        {
            ContextMenuStrip strip = new ContextMenuStrip();
            strip.Events.AddHandlers(base.Events);
            strip.AutoClose = base.AutoClose;
            strip.AutoSize = this.AutoSize;
            strip.Bounds = base.Bounds;
            strip.ImageList = base.ImageList;
            strip.ShowCheckMargin = base.ShowCheckMargin;
            strip.ShowImageMargin = base.ShowImageMargin;
            for (int i = 0; i < this.Items.Count; i++)
            {
                ToolStripItem item = this.Items[i];
                if (item is ToolStripSeparator)
                {
                    strip.Items.Add(new ToolStripSeparator());
                }
                else if (item is ToolStripMenuItem)
                {
                    ToolStripMenuItem item2 = item as ToolStripMenuItem;
                    strip.Items.Add(item2.Clone());
                }
            }
            return strip;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected override void SetVisibleCore(bool visible)
        {
            if (!visible)
            {
                base.WorkingAreaConstrained = true;
            }
            base.SetVisibleCore(visible);
        }

        internal void ShowInTaskbar(int x, int y)
        {
            base.WorkingAreaConstrained = false;
            Rectangle rect = base.CalculateDropDownLocation(new Point(x, y), ToolStripDropDownDirection.AboveLeft);
            Rectangle bounds = Screen.FromRectangle(rect).Bounds;
            if (rect.Y < bounds.Y)
            {
                rect = base.CalculateDropDownLocation(new Point(x, y), ToolStripDropDownDirection.BelowLeft);
            }
            else if (rect.X < bounds.X)
            {
                rect = base.CalculateDropDownLocation(new Point(x, y), ToolStripDropDownDirection.AboveRight);
            }
            rect = WindowsFormsUtils.ConstrainToBounds(bounds, rect);
            base.Show(rect.X, rect.Y);
        }

        internal void ShowInternal(Control source, Point location, bool isKeyboardActivated)
        {
            base.Show(source, location);
            if (isKeyboardActivated)
            {
                ToolStripManager.ModalMenuFilter.Instance.ShowUnderlines = true;
            }
        }

        [System.Windows.Forms.SRDescription("ContextMenuStripSourceControlDescr"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control SourceControl
        {
            [UIPermission(SecurityAction.Demand, Window=UIPermissionWindow.AllWindows)]
            get
            {
                return base.SourceControlInternal;
            }
        }
    }
}

