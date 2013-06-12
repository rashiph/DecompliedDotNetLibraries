namespace System.Windows.Forms.ButtonInternal
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal class RadioButtonPopupAdapter : RadioButtonFlatAdapter
    {
        internal RadioButtonPopupAdapter(ButtonBase control) : base(control)
        {
        }

        protected override ButtonBaseAdapter CreateButtonAdapter()
        {
            return new ButtonPopupAdapter(base.Control);
        }

        protected override ButtonBaseAdapter.LayoutOptions Layout(PaintEventArgs e)
        {
            ButtonBaseAdapter.LayoutOptions options = base.Layout(e);
            if (!base.Control.MouseIsDown && !base.Control.MouseIsOver)
            {
                options.shadowedText = true;
            }
            return options;
        }

        internal override void PaintDown(PaintEventArgs e, CheckState state)
        {
            Graphics graphics = e.Graphics;
            if (base.Control.Appearance == Appearance.Button)
            {
                new ButtonPopupAdapter(base.Control).PaintDown(e, base.Control.Checked ? CheckState.Checked : CheckState.Unchecked);
            }
            else
            {
                ButtonBaseAdapter.ColorData colors = base.PaintPopupRender(e.Graphics).Calculate();
                ButtonBaseAdapter.LayoutData layout = this.Layout(e).Layout();
                base.PaintButtonBackground(e, base.Control.ClientRectangle, null);
                base.PaintImage(e, layout);
                base.DrawCheckBackground3DLite(e, layout.checkBounds, colors.windowText, colors.highlight, colors, true);
                base.DrawCheckOnly(e, layout, colors.buttonShadow, colors.highlight, true);
                base.PaintField(e, layout, colors, colors.windowText, true);
            }
        }

        internal override void PaintOver(PaintEventArgs e, CheckState state)
        {
            Graphics graphics = e.Graphics;
            if (base.Control.Appearance == Appearance.Button)
            {
                new ButtonPopupAdapter(base.Control).PaintOver(e, base.Control.Checked ? CheckState.Checked : CheckState.Unchecked);
            }
            else
            {
                ButtonBaseAdapter.ColorData colors = base.PaintPopupRender(e.Graphics).Calculate();
                ButtonBaseAdapter.LayoutData layout = this.Layout(e).Layout();
                base.PaintButtonBackground(e, base.Control.ClientRectangle, null);
                base.PaintImage(e, layout);
                base.DrawCheckBackground3DLite(e, layout.checkBounds, colors.windowText, colors.options.highContrast ? colors.buttonFace : colors.highlight, colors, true);
                base.DrawCheckOnly(e, layout, colors.windowText, colors.highlight, true);
                base.PaintField(e, layout, colors, colors.windowText, true);
            }
        }

        internal override void PaintUp(PaintEventArgs e, CheckState state)
        {
            Graphics graphics = e.Graphics;
            if (base.Control.Appearance == Appearance.Button)
            {
                new ButtonPopupAdapter(base.Control).PaintUp(e, base.Control.Checked ? CheckState.Checked : CheckState.Unchecked);
            }
            else
            {
                ButtonBaseAdapter.ColorData colors = base.PaintPopupRender(e.Graphics).Calculate();
                ButtonBaseAdapter.LayoutData layout = this.Layout(e).Layout();
                base.PaintButtonBackground(e, base.Control.ClientRectangle, null);
                base.PaintImage(e, layout);
                base.DrawCheckBackgroundFlat(e, layout.checkBounds, colors.buttonShadow, colors.options.highContrast ? colors.buttonFace : colors.highlight, true);
                base.DrawCheckOnly(e, layout, colors.windowText, colors.highlight, true);
                base.PaintField(e, layout, colors, colors.windowText, true);
            }
        }
    }
}

