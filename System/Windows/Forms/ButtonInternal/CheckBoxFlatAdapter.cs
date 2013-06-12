namespace System.Windows.Forms.ButtonInternal
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal class CheckBoxFlatAdapter : CheckBoxBaseAdapter
    {
        internal CheckBoxFlatAdapter(ButtonBase control) : base(control)
        {
        }

        protected override ButtonBaseAdapter CreateButtonAdapter()
        {
            return new ButtonFlatAdapter(base.Control);
        }

        protected override ButtonBaseAdapter.LayoutOptions Layout(PaintEventArgs e)
        {
            ButtonBaseAdapter.LayoutOptions options = this.CommonLayout();
            options.checkSize = (int) (11f * CheckableControlBaseAdapter.GetDpiScaleRatio(e.Graphics));
            options.shadowedText = false;
            return options;
        }

        internal override void PaintDown(PaintEventArgs e, CheckState state)
        {
            if (base.Control.Appearance == Appearance.Button)
            {
                this.ButtonAdapter.PaintDown(e, base.Control.CheckState);
            }
            else
            {
                ButtonBaseAdapter.ColorData colors = base.PaintFlatRender(e.Graphics).Calculate();
                if (base.Control.Enabled)
                {
                    this.PaintFlatWorker(e, colors.windowText, colors.highlight, colors.windowFrame, colors);
                }
                else
                {
                    this.PaintFlatWorker(e, colors.buttonShadow, colors.buttonFace, colors.buttonShadow, colors);
                }
            }
        }

        private void PaintFlatWorker(PaintEventArgs e, Color checkColor, Color checkBackground, Color checkBorder, ButtonBaseAdapter.ColorData colors)
        {
            Graphics graphics = e.Graphics;
            ButtonBaseAdapter.LayoutData layout = this.Layout(e).Layout();
            base.PaintButtonBackground(e, base.Control.ClientRectangle, null);
            base.PaintImage(e, layout);
            base.DrawCheckFlat(e, layout, checkColor, colors.options.highContrast ? colors.buttonFace : checkBackground, checkBorder, colors);
            base.PaintField(e, layout, colors, checkColor, true);
        }

        internal override void PaintOver(PaintEventArgs e, CheckState state)
        {
            if (base.Control.Appearance == Appearance.Button)
            {
                this.ButtonAdapter.PaintOver(e, base.Control.CheckState);
            }
            else
            {
                ButtonBaseAdapter.ColorData colors = base.PaintFlatRender(e.Graphics).Calculate();
                if (base.Control.Enabled)
                {
                    this.PaintFlatWorker(e, colors.windowText, colors.lowHighlight, colors.windowFrame, colors);
                }
                else
                {
                    this.PaintFlatWorker(e, colors.buttonShadow, colors.buttonFace, colors.buttonShadow, colors);
                }
            }
        }

        internal override void PaintUp(PaintEventArgs e, CheckState state)
        {
            if (base.Control.Appearance == Appearance.Button)
            {
                this.ButtonAdapter.PaintUp(e, base.Control.CheckState);
            }
            else
            {
                ButtonBaseAdapter.ColorData colors = base.PaintFlatRender(e.Graphics).Calculate();
                if (base.Control.Enabled)
                {
                    this.PaintFlatWorker(e, colors.windowText, colors.highlight, colors.windowFrame, colors);
                }
                else
                {
                    this.PaintFlatWorker(e, colors.buttonShadow, colors.buttonFace, colors.buttonShadow, colors);
                }
            }
        }

        private ButtonFlatAdapter ButtonAdapter
        {
            get
            {
                return (ButtonFlatAdapter) base.ButtonAdapter;
            }
        }
    }
}

