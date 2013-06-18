namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [DefaultProperty("GridEditName"), DesignTimeVisible(false), ComVisible(true), ToolboxItem(false), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class DataGridTextBox : TextBox
    {
        private DataGrid dataGrid;
        private bool isInEditOrNavigateMode = true;

        public DataGridTextBox()
        {
            base.TabStop = false;
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if ((((e.KeyChar != ' ') || ((Control.ModifierKeys & Keys.Shift) != Keys.Shift)) && !base.ReadOnly) && (((Control.ModifierKeys & Keys.Control) != Keys.Control) || ((Control.ModifierKeys & Keys.Alt) != Keys.None)))
            {
                this.IsInEditOrNavigateMode = false;
                this.dataGrid.ColumnStartedEditing(base.Bounds);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            this.dataGrid.TextBoxOnMouseWheel(e);
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected internal override bool ProcessKeyMessage(ref Message m)
        {
            Keys wParam = (Keys) ((int) ((long) m.WParam));
            Keys modifierKeys = Control.ModifierKeys;
            if ((((wParam | modifierKeys) == Keys.Enter) || ((wParam | modifierKeys) == Keys.Escape)) || ((wParam | modifierKeys) == (Keys.Control | Keys.Enter)))
            {
                return ((m.Msg == 0x102) || this.ProcessKeyPreview(ref m));
            }
            if (m.Msg == 0x102)
            {
                return ((wParam == Keys.LineFeed) || this.ProcessKeyEventArgs(ref m));
            }
            if (m.Msg == 0x101)
            {
                return true;
            }
            switch ((wParam & Keys.KeyCode))
            {
                case Keys.Space:
                    if (!this.IsInEditOrNavigateMode || ((Control.ModifierKeys & Keys.Shift) != Keys.Shift))
                    {
                        return this.ProcessKeyEventArgs(ref m);
                    }
                    if (m.Msg != 0x102)
                    {
                        return this.ProcessKeyPreview(ref m);
                    }
                    return true;

                case Keys.PageUp:
                case Keys.Next:
                case Keys.Add:
                case Keys.Subtract:
                case Keys.Oemplus:
                case Keys.OemMinus:
                    if (this.IsInEditOrNavigateMode)
                    {
                        return this.ProcessKeyPreview(ref m);
                    }
                    return this.ProcessKeyEventArgs(ref m);

                case Keys.End:
                case Keys.Home:
                    if (this.SelectionLength != this.Text.Length)
                    {
                        return this.ProcessKeyEventArgs(ref m);
                    }
                    return this.ProcessKeyPreview(ref m);

                case Keys.Left:
                    if (((base.SelectionStart + this.SelectionLength) != 0) && (!this.IsInEditOrNavigateMode || (this.SelectionLength != this.Text.Length)))
                    {
                        return this.ProcessKeyEventArgs(ref m);
                    }
                    return this.ProcessKeyPreview(ref m);

                case Keys.Up:
                    if ((this.Text.IndexOf("\r\n") >= 0) && ((base.SelectionStart + this.SelectionLength) >= this.Text.IndexOf("\r\n")))
                    {
                        return this.ProcessKeyEventArgs(ref m);
                    }
                    return this.ProcessKeyPreview(ref m);

                case Keys.Right:
                    if ((base.SelectionStart + this.SelectionLength) != this.Text.Length)
                    {
                        return this.ProcessKeyEventArgs(ref m);
                    }
                    return this.ProcessKeyPreview(ref m);

                case Keys.Down:
                {
                    int startIndex = base.SelectionStart + this.SelectionLength;
                    if (this.Text.IndexOf("\r\n", startIndex) != -1)
                    {
                        return this.ProcessKeyEventArgs(ref m);
                    }
                    return this.ProcessKeyPreview(ref m);
                }
                case Keys.Delete:
                    if (!this.IsInEditOrNavigateMode)
                    {
                        return this.ProcessKeyEventArgs(ref m);
                    }
                    if (!this.ProcessKeyPreview(ref m))
                    {
                        this.IsInEditOrNavigateMode = false;
                        this.dataGrid.ColumnStartedEditing(base.Bounds);
                        return this.ProcessKeyEventArgs(ref m);
                    }
                    return true;

                case Keys.A:
                    if (!this.IsInEditOrNavigateMode || ((Control.ModifierKeys & Keys.Control) != Keys.Control))
                    {
                        return this.ProcessKeyEventArgs(ref m);
                    }
                    return ((m.Msg == 0x102) || this.ProcessKeyPreview(ref m));

                case Keys.Tab:
                    if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                    {
                        return this.ProcessKeyPreview(ref m);
                    }
                    return this.ProcessKeyEventArgs(ref m);

                case Keys.F2:
                    this.IsInEditOrNavigateMode = false;
                    base.SelectionStart = this.Text.Length;
                    return true;
            }
            return this.ProcessKeyEventArgs(ref m);
        }

        public void SetDataGrid(DataGrid parentGrid)
        {
            this.dataGrid = parentGrid;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            if (((m.Msg == 770) || (m.Msg == 0x300)) || (m.Msg == 0x303))
            {
                this.IsInEditOrNavigateMode = false;
                this.dataGrid.ColumnStartedEditing(base.Bounds);
            }
            base.WndProc(ref m);
        }

        public bool IsInEditOrNavigateMode
        {
            get
            {
                return this.isInEditOrNavigateMode;
            }
            set
            {
                this.isInEditOrNavigateMode = value;
                if (value)
                {
                    base.SelectAll();
                }
            }
        }
    }
}

