namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class DesignerActionToolStripDropDown : ToolStripDropDown
    {
        private bool _cancelClose;
        private DesignerActionUI _designerActionUI;
        private IWin32Window _mainParentWindow;
        private ToolStripControlHost _panel;
        private Glyph relatedGlyph;

        public DesignerActionToolStripDropDown(DesignerActionUI designerActionUI, IWin32Window mainParentWindow)
        {
            this._mainParentWindow = mainParentWindow;
            this._designerActionUI = designerActionUI;
        }

        public void CheckFocusIsRight()
        {
            if (System.Design.UnsafeNativeMethods.GetFocus() == base.Handle)
            {
                this._panel.Focus();
            }
            IntPtr focus = System.Design.UnsafeNativeMethods.GetFocus();
            if ((this.CurrentPanel != null) && (this.CurrentPanel.Handle == focus))
            {
                this.CurrentPanel.SelectNextControl(null, true, true, true, true);
            }
            focus = System.Design.UnsafeNativeMethods.GetFocus();
        }

        internal static string GetControlInformation(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
            {
                return "Handle is IntPtr.Zero";
            }
            return string.Empty;
        }

        private bool IsWindowEnabled(IntPtr handle)
        {
            int windowLong = (int) System.Design.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, handle), -16);
            return ((windowLong & 0x8000000) == 0);
        }

        protected override void OnClosing(ToolStripDropDownClosingEventArgs e)
        {
            if ((e.CloseReason == ToolStripDropDownCloseReason.AppFocusChange) && this._cancelClose)
            {
                this._cancelClose = false;
                e.Cancel = true;
            }
            else if ((e.CloseReason == ToolStripDropDownCloseReason.AppFocusChange) || (e.CloseReason == ToolStripDropDownCloseReason.AppClicked))
            {
                IntPtr activeWindow = System.Design.UnsafeNativeMethods.GetActiveWindow();
                if ((base.Handle == activeWindow) && (e.CloseReason == ToolStripDropDownCloseReason.AppClicked))
                {
                    e.Cancel = false;
                }
                else if (WindowOwnsWindow(base.Handle, activeWindow))
                {
                    e.Cancel = true;
                }
                else if ((this._mainParentWindow != null) && !WindowOwnsWindow(this._mainParentWindow.Handle, activeWindow))
                {
                    if (this.IsWindowEnabled(this._mainParentWindow.Handle))
                    {
                        e.Cancel = false;
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                    base.OnClosing(e);
                    return;
                }
                IntPtr windowLong = System.Design.UnsafeNativeMethods.GetWindowLong(new HandleRef(this, activeWindow), -8);
                if (!this.IsWindowEnabled(windowLong))
                {
                    e.Cancel = true;
                }
            }
            base.OnClosing(e);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            this.UpdateContainerSize();
        }

        private void PanelResized(object sender, EventArgs e)
        {
            Control control = sender as Control;
            if ((base.Size.Width != control.Size.Width) || (base.Size.Height != control.Size.Height))
            {
                base.SuspendLayout();
                base.Size = control.Size;
                if (this._panel != null)
                {
                    this._panel.Size = control.Size;
                }
                this._designerActionUI.UpdateDAPLocation(null, this.relatedGlyph as DesignerActionGlyph);
                base.ResumeLayout();
            }
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                IButtonControl control2 = Control.FromChildHandle(System.Design.UnsafeNativeMethods.GetFocus()) as IButtonControl;
                if ((control2 != null) && (control2 is Control))
                {
                    control2.PerformClick();
                    return true;
                }
            }
            return base.ProcessDialogKey(keyData);
        }

        public void SetDesignerActionPanel(DesignerActionPanel panel, Glyph relatedGlyph)
        {
            if ((this._panel == null) || (panel != ((DesignerActionPanel) this._panel.Control)))
            {
                this.relatedGlyph = relatedGlyph;
                panel.SizeChanged += new EventHandler(this.PanelResized);
                if (this._panel != null)
                {
                    this.Items.Remove(this._panel);
                    this._panel.Dispose();
                    this._panel = null;
                }
                this._panel = new ToolStripControlHost(panel);
                this._panel.Margin = Padding.Empty;
                this._panel.Size = panel.Size;
                base.SuspendLayout();
                base.Size = panel.Size;
                this.Items.Add(this._panel);
                base.ResumeLayout();
                if (base.Visible)
                {
                    this.CheckFocusIsRight();
                }
            }
        }

        protected override void SetVisibleCore(bool visible)
        {
            base.SetVisibleCore(visible);
            if (visible)
            {
                this.CheckFocusIsRight();
            }
        }

        public void UpdateContainerSize()
        {
            if (this.CurrentPanel != null)
            {
                Size preferredSize = this.CurrentPanel.GetPreferredSize(new Size(150, 0x7fffffff));
                if (this.CurrentPanel.Size == preferredSize)
                {
                    this.CurrentPanel.PerformLayout();
                }
                else
                {
                    this.CurrentPanel.Size = preferredSize;
                }
                base.ClientSize = preferredSize;
            }
        }

        private static bool WindowOwnsWindow(IntPtr hWndOwner, IntPtr hWndDescendant)
        {
            if (!(hWndDescendant == hWndOwner))
            {
                while (hWndDescendant != IntPtr.Zero)
                {
                    hWndDescendant = System.Design.UnsafeNativeMethods.GetWindowLong(new HandleRef(null, hWndDescendant), -8);
                    if (hWndDescendant == IntPtr.Zero)
                    {
                        return false;
                    }
                    if (hWndDescendant == hWndOwner)
                    {
                        return true;
                    }
                }
                return false;
            }
            return true;
        }

        private void WmActivate(ref Message m)
        {
            if (((int) ((long) m.WParam)) == 0)
            {
                IntPtr lParam = m.LParam;
                if (WindowOwnsWindow(base.Handle, lParam))
                {
                    this._cancelClose = true;
                }
                else
                {
                    this._cancelClose = false;
                }
            }
            else
            {
                this._cancelClose = false;
            }
            base.WndProc(ref m);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 6)
            {
                this.WmActivate(ref m);
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        public DesignerActionPanel CurrentPanel
        {
            get
            {
                if (this._panel != null)
                {
                    return (this._panel.Control as DesignerActionPanel);
                }
                return null;
            }
        }

        protected override bool TopMost
        {
            get
            {
                return false;
            }
        }
    }
}

