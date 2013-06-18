namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    internal class PictureBoxDesigner : ControlDesigner
    {
        private DesignerActionListCollection _actionLists;

        public PictureBoxDesigner()
        {
            base.AutoResizeHandles = true;
        }

        private void DrawBorder(Graphics graphics)
        {
            Color color;
            Control control = this.Control;
            Rectangle clientRectangle = control.ClientRectangle;
            if (control.BackColor.GetBrightness() < 0.5)
            {
                color = ControlPaint.Light(control.BackColor);
            }
            else
            {
                color = ControlPaint.Dark(control.BackColor);
            }
            Pen pen = new Pen(color) {
                DashStyle = DashStyle.Dash
            };
            clientRectangle.Width--;
            clientRectangle.Height--;
            graphics.DrawRectangle(pen, clientRectangle);
            pen.Dispose();
        }

        protected override void OnPaintAdornments(PaintEventArgs pe)
        {
            PictureBox component = (PictureBox) base.Component;
            if (component.BorderStyle == BorderStyle.None)
            {
                this.DrawBorder(pe.Graphics);
            }
            base.OnPaintAdornments(pe);
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (this._actionLists == null)
                {
                    this._actionLists = new DesignerActionListCollection();
                    this._actionLists.Add(new PictureBoxActionList(this));
                }
                return this._actionLists;
            }
        }

        public override System.Windows.Forms.Design.SelectionRules SelectionRules
        {
            get
            {
                System.Windows.Forms.Design.SelectionRules selectionRules = base.SelectionRules;
                object component = base.Component;
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(base.Component)["SizeMode"];
                if (descriptor != null)
                {
                    PictureBoxSizeMode mode = (PictureBoxSizeMode) descriptor.GetValue(component);
                    if (mode == PictureBoxSizeMode.AutoSize)
                    {
                        selectionRules &= ~System.Windows.Forms.Design.SelectionRules.AllSizeable;
                    }
                }
                return selectionRules;
            }
        }
    }
}

