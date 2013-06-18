namespace System.Windows.Forms.ButtonInternal
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal class RadioButtonStandardAdapter : RadioButtonBaseAdapter
    {
        internal RadioButtonStandardAdapter(ButtonBase control) : base(control)
        {
        }

        protected override ButtonBaseAdapter CreateButtonAdapter()
        {
            return new ButtonStandardAdapter(base.Control);
        }

        protected override ButtonBaseAdapter.LayoutOptions Layout(PaintEventArgs e)
        {
            ButtonBaseAdapter.LayoutOptions options = this.CommonLayout();
            options.hintTextUp = false;
            options.everettButtonCompat = !Application.RenderWithVisualStyles;
            if (Application.RenderWithVisualStyles)
            {
                using (Graphics graphics = WindowsFormsUtils.CreateMeasurementGraphics())
                {
                    options.checkSize = RadioButtonRenderer.GetGlyphSize(graphics, RadioButtonRenderer.ConvertFromButtonState(base.GetState(), base.Control.MouseIsOver)).Width;
                    return options;
                }
            }
            options.checkSize = (int) (options.checkSize * CheckableControlBaseAdapter.GetDpiScaleRatio(e.Graphics));
            return options;
        }

        internal override void PaintDown(PaintEventArgs e, CheckState state)
        {
            if (base.Control.Appearance == Appearance.Button)
            {
                this.ButtonAdapter.PaintDown(e, base.Control.Checked ? CheckState.Checked : CheckState.Unchecked);
            }
            else
            {
                this.PaintUp(e, state);
            }
        }

        internal override void PaintOver(PaintEventArgs e, CheckState state)
        {
            if (base.Control.Appearance == Appearance.Button)
            {
                this.ButtonAdapter.PaintOver(e, base.Control.Checked ? CheckState.Checked : CheckState.Unchecked);
            }
            else
            {
                this.PaintUp(e, state);
            }
        }

        internal override void PaintUp(PaintEventArgs e, CheckState state)
        {
            if (base.Control.Appearance == Appearance.Button)
            {
                this.ButtonAdapter.PaintUp(e, base.Control.Checked ? CheckState.Checked : CheckState.Unchecked);
            }
            else
            {
                ButtonBaseAdapter.ColorData colors = base.PaintRender(e.Graphics).Calculate();
                ButtonBaseAdapter.LayoutData layout = this.Layout(e).Layout();
                base.PaintButtonBackground(e, base.Control.ClientRectangle, null);
                base.PaintImage(e, layout);
                base.DrawCheckBox(e, layout);
                base.PaintField(e, layout, colors, colors.windowText, true);
            }
        }

        private ButtonStandardAdapter ButtonAdapter
        {
            get
            {
                return (ButtonStandardAdapter) base.ButtonAdapter;
            }
        }
    }
}

