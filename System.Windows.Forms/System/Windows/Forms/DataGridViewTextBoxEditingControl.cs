namespace System.Windows.Forms
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class DataGridViewTextBoxEditingControl : TextBox, IDataGridViewEditingControl
    {
        private static readonly DataGridViewContentAlignment anyCenter = (DataGridViewContentAlignment.BottomCenter | DataGridViewContentAlignment.MiddleCenter | DataGridViewContentAlignment.TopCenter);
        private static readonly DataGridViewContentAlignment anyRight = (DataGridViewContentAlignment.BottomRight | DataGridViewContentAlignment.MiddleRight | DataGridViewContentAlignment.TopRight);
        private static readonly DataGridViewContentAlignment anyTop = (DataGridViewContentAlignment.TopRight | DataGridViewContentAlignment.TopCenter | DataGridViewContentAlignment.TopLeft);
        private DataGridView dataGridView;
        private bool repositionOnValueChange;
        private int rowIndex;
        private bool valueChanged;

        public DataGridViewTextBoxEditingControl()
        {
            base.TabStop = false;
        }

        public virtual void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
        {
            this.Font = dataGridViewCellStyle.Font;
            if (dataGridViewCellStyle.BackColor.A < 0xff)
            {
                Color color = Color.FromArgb(0xff, dataGridViewCellStyle.BackColor);
                this.BackColor = color;
                this.dataGridView.EditingPanel.BackColor = color;
            }
            else
            {
                this.BackColor = dataGridViewCellStyle.BackColor;
            }
            this.ForeColor = dataGridViewCellStyle.ForeColor;
            if (dataGridViewCellStyle.WrapMode == DataGridViewTriState.True)
            {
                base.WordWrap = true;
            }
            base.TextAlign = TranslateAlignment(dataGridViewCellStyle.Alignment);
            this.repositionOnValueChange = (dataGridViewCellStyle.WrapMode == DataGridViewTriState.True) && ((dataGridViewCellStyle.Alignment & anyTop) == DataGridViewContentAlignment.NotSet);
        }

        public virtual bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey)
        {
            switch ((keyData & Keys.KeyCode))
            {
                case Keys.PageUp:
                case Keys.Next:
                    if (!this.valueChanged)
                    {
                        break;
                    }
                    return true;

                case Keys.End:
                case Keys.Home:
                    if (this.SelectionLength == this.Text.Length)
                    {
                        break;
                    }
                    return true;

                case Keys.Left:
                    if (((this.RightToLeft != RightToLeft.No) || ((this.SelectionLength == 0) && (base.SelectionStart == 0))) && ((this.RightToLeft != RightToLeft.Yes) || ((this.SelectionLength == 0) && (base.SelectionStart == this.Text.Length))))
                    {
                        break;
                    }
                    return true;

                case Keys.Up:
                    if ((this.Text.IndexOf("\r\n") < 0) || ((base.SelectionStart + this.SelectionLength) < this.Text.IndexOf("\r\n")))
                    {
                        break;
                    }
                    return true;

                case Keys.Right:
                    if (((this.RightToLeft != RightToLeft.No) || ((this.SelectionLength == 0) && (base.SelectionStart == this.Text.Length))) && ((this.RightToLeft != RightToLeft.Yes) || ((this.SelectionLength == 0) && (base.SelectionStart == 0))))
                    {
                        break;
                    }
                    return true;

                case Keys.Down:
                {
                    int startIndex = base.SelectionStart + this.SelectionLength;
                    if (this.Text.IndexOf("\r\n", startIndex) == -1)
                    {
                        break;
                    }
                    return true;
                }
                case Keys.Delete:
                    if ((this.SelectionLength <= 0) && (base.SelectionStart >= this.Text.Length))
                    {
                        break;
                    }
                    return true;

                case Keys.Enter:
                    if ((((keyData & (Keys.Alt | Keys.Control | Keys.Shift)) == Keys.Shift) && this.Multiline) && base.AcceptsReturn)
                    {
                        return true;
                    }
                    break;
            }
            return !dataGridViewWantsInputKey;
        }

        public virtual object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
        {
            return this.Text;
        }

        private void NotifyDataGridViewOfValueChange()
        {
            this.valueChanged = true;
            this.dataGridView.NotifyCurrentCellDirty(true);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            this.dataGridView.OnMouseWheelInternal(e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            this.NotifyDataGridViewOfValueChange();
        }

        public virtual void PrepareEditingControlForEdit(bool selectAll)
        {
            if (selectAll)
            {
                base.SelectAll();
            }
            else
            {
                base.SelectionStart = this.Text.Length;
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessKeyEventArgs(ref Message m)
        {
            Keys wParam = (Keys) ((int) m.WParam);
            if (wParam != Keys.LineFeed)
            {
                if (wParam == Keys.Enter)
                {
                    if ((m.Msg == 0x102) && (((Control.ModifierKeys != Keys.Shift) || !this.Multiline) || !base.AcceptsReturn))
                    {
                        return true;
                    }
                    goto Label_0094;
                }
                if (wParam != Keys.A)
                {
                    goto Label_0094;
                }
            }
            else
            {
                if (((m.Msg != 0x102) || (Control.ModifierKeys != Keys.Control)) || (!this.Multiline || !base.AcceptsReturn))
                {
                    goto Label_0094;
                }
                return true;
            }
            if ((m.Msg == 0x100) && (Control.ModifierKeys == Keys.Control))
            {
                base.SelectAll();
                return true;
            }
        Label_0094:
            return base.ProcessKeyEventArgs(ref m);
        }

        private static HorizontalAlignment TranslateAlignment(DataGridViewContentAlignment align)
        {
            if ((align & anyRight) != DataGridViewContentAlignment.NotSet)
            {
                return HorizontalAlignment.Right;
            }
            if ((align & anyCenter) != DataGridViewContentAlignment.NotSet)
            {
                return HorizontalAlignment.Center;
            }
            return HorizontalAlignment.Left;
        }

        public virtual DataGridView EditingControlDataGridView
        {
            get
            {
                return this.dataGridView;
            }
            set
            {
                this.dataGridView = value;
            }
        }

        public virtual object EditingControlFormattedValue
        {
            get
            {
                return this.GetEditingControlFormattedValue(DataGridViewDataErrorContexts.Formatting);
            }
            set
            {
                this.Text = (string) value;
            }
        }

        public virtual int EditingControlRowIndex
        {
            get
            {
                return this.rowIndex;
            }
            set
            {
                this.rowIndex = value;
            }
        }

        public virtual bool EditingControlValueChanged
        {
            get
            {
                return this.valueChanged;
            }
            set
            {
                this.valueChanged = value;
            }
        }

        public virtual Cursor EditingPanelCursor
        {
            get
            {
                return Cursors.Default;
            }
        }

        public virtual bool RepositionEditingControlOnValueChange
        {
            get
            {
                return this.repositionOnValueChange;
            }
        }
    }
}

