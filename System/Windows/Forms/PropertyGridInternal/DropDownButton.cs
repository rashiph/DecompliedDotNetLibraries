namespace System.Windows.Forms.PropertyGridInternal
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.ButtonInternal;
    using System.Windows.Forms.VisualStyles;

    internal sealed class DropDownButton : System.Windows.Forms.Button
    {
        private bool ignoreMouse;
        private bool useComboBoxTheme;

        public DropDownButton()
        {
            base.SetStyle(ControlStyles.Selectable, true);
            base.AccessibleName = System.Windows.Forms.SR.GetString("PropertyGridDropDownButtonAccessibleName");
        }

        internal override ButtonBaseAdapter CreateStandardAdapter()
        {
            return new DropDownButtonAdapter(this);
        }

        protected override void OnClick(EventArgs e)
        {
            if (!this.IgnoreMouse)
            {
                base.OnClick(e);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!this.IgnoreMouse)
            {
                base.OnMouseDown(e);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (!this.IgnoreMouse)
            {
                base.OnMouseUp(e);
            }
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            if (Application.RenderWithVisualStyles & this.useComboBoxTheme)
            {
                ComboBoxState normal = ComboBoxState.Normal;
                if (base.MouseIsDown)
                {
                    normal = ComboBoxState.Pressed;
                }
                else if (base.MouseIsOver)
                {
                    normal = ComboBoxState.Hot;
                }
                Rectangle rect = new Rectangle(0, 0, base.Width, base.Height);
                if (normal == ComboBoxState.Normal)
                {
                    pevent.Graphics.FillRectangle(SystemBrushes.Window, rect);
                }
                ComboBoxRenderer.DrawDropDownButton(pevent.Graphics, rect, normal);
            }
        }

        public bool IgnoreMouse
        {
            get
            {
                return this.ignoreMouse;
            }
            set
            {
                this.ignoreMouse = value;
            }
        }

        public bool UseComboBoxTheme
        {
            set
            {
                if (this.useComboBoxTheme != value)
                {
                    this.useComboBoxTheme = value;
                    base.Invalidate();
                }
            }
        }
    }
}

