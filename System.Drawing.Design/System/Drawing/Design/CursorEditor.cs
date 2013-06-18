namespace System.Drawing.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class CursorEditor : UITypeEditor
    {
        private CursorUI cursorUI;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
                if (edSvc == null)
                {
                    return value;
                }
                if (this.cursorUI == null)
                {
                    this.cursorUI = new CursorUI(this);
                }
                this.cursorUI.Start(edSvc, value);
                edSvc.DropDownControl(this.cursorUI);
                value = this.cursorUI.Value;
                this.cursorUI.End();
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override bool IsDropDownResizable
        {
            get
            {
                return true;
            }
        }

        private class CursorUI : ListBox
        {
            private TypeConverter cursorConverter;
            private UITypeEditor editor;
            private IWindowsFormsEditorService edSvc;
            private object value;

            public CursorUI(UITypeEditor editor)
            {
                this.editor = editor;
                base.Height = 310;
                this.ItemHeight = Math.Max(4 + Cursors.Default.Size.Height, this.Font.Height);
                this.DrawMode = DrawMode.OwnerDrawFixed;
                base.BorderStyle = BorderStyle.None;
                this.cursorConverter = TypeDescriptor.GetConverter(typeof(Cursor));
                if (this.cursorConverter.GetStandardValuesSupported())
                {
                    foreach (object obj2 in this.cursorConverter.GetStandardValues())
                    {
                        base.Items.Add(obj2);
                    }
                }
            }

            public void End()
            {
                this.edSvc = null;
                this.value = null;
            }

            protected override void OnClick(EventArgs e)
            {
                base.OnClick(e);
                this.value = base.SelectedItem;
                this.edSvc.CloseDropDown();
            }

            protected override void OnDrawItem(DrawItemEventArgs die)
            {
                base.OnDrawItem(die);
                if (die.Index != -1)
                {
                    Cursor cursor = (Cursor) base.Items[die.Index];
                    string s = this.cursorConverter.ConvertToString(cursor);
                    Font font = die.Font;
                    Brush brush = new SolidBrush(die.ForeColor);
                    die.DrawBackground();
                    die.Graphics.FillRectangle(SystemBrushes.Control, new Rectangle(die.Bounds.X + 2, die.Bounds.Y + 2, 0x20, die.Bounds.Height - 4));
                    die.Graphics.DrawRectangle(SystemPens.WindowText, new Rectangle(die.Bounds.X + 2, die.Bounds.Y + 2, 0x1f, (die.Bounds.Height - 4) - 1));
                    cursor.DrawStretched(die.Graphics, new Rectangle(die.Bounds.X + 2, die.Bounds.Y + 2, 0x20, die.Bounds.Height - 4));
                    die.Graphics.DrawString(s, font, brush, (float) (die.Bounds.X + 0x24), (float) (die.Bounds.Y + ((die.Bounds.Height - font.Height) / 2)));
                    brush.Dispose();
                }
            }

            protected override bool ProcessDialogKey(Keys keyData)
            {
                if (((keyData & Keys.KeyCode) == Keys.Enter) && ((keyData & (Keys.Alt | Keys.Control)) == Keys.None))
                {
                    this.OnClick(EventArgs.Empty);
                    return true;
                }
                return base.ProcessDialogKey(keyData);
            }

            public void Start(IWindowsFormsEditorService edSvc, object value)
            {
                this.edSvc = edSvc;
                this.value = value;
                if (value != null)
                {
                    for (int i = 0; i < base.Items.Count; i++)
                    {
                        if (base.Items[i] == value)
                        {
                            this.SelectedIndex = i;
                            return;
                        }
                    }
                }
            }

            public object Value
            {
                [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
                get
                {
                    return this.value;
                }
            }
        }
    }
}

