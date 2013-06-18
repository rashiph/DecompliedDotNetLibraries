namespace System.Windows.Forms.ButtonInternal
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class CheckBoxStandardAdapter : CheckBoxBaseAdapter
    {
        internal CheckBoxStandardAdapter(ButtonBase control) : base(control)
        {
        }

        protected override ButtonBaseAdapter CreateButtonAdapter()
        {
            return new ButtonStandardAdapter(base.Control);
        }

        internal override Size GetPreferredSizeCore(Size proposedSize)
        {
            Size preferredSizeCore;
            if (base.Control.Appearance == Appearance.Button)
            {
                ButtonStandardAdapter adapter = new ButtonStandardAdapter(base.Control);
                return adapter.GetPreferredSizeCore(proposedSize);
            }
            using (Graphics graphics = WindowsFormsUtils.CreateMeasurementGraphics())
            {
                using (PaintEventArgs args = new PaintEventArgs(graphics, new Rectangle()))
                {
                    preferredSizeCore = this.Layout(args).GetPreferredSizeCore(proposedSize);
                }
            }
            return preferredSizeCore;
        }

        protected override ButtonBaseAdapter.LayoutOptions Layout(PaintEventArgs e)
        {
            ButtonBaseAdapter.LayoutOptions options = this.CommonLayout();
            options.checkPaddingSize = 1;
            options.everettButtonCompat = !Application.RenderWithVisualStyles;
            if (Application.RenderWithVisualStyles)
            {
                using (Graphics graphics = WindowsFormsUtils.CreateMeasurementGraphics())
                {
                    options.checkSize = CheckBoxRenderer.GetGlyphSize(graphics, CheckBoxRenderer.ConvertFromButtonState(base.GetState(), true, base.Control.MouseIsOver)).Width;
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
                this.ButtonAdapter.PaintDown(e, base.Control.CheckState);
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
                this.ButtonAdapter.PaintOver(e, base.Control.CheckState);
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
                this.ButtonAdapter.PaintUp(e, base.Control.CheckState);
            }
            else
            {
                ButtonBaseAdapter.ColorData colors = base.PaintRender(e.Graphics).Calculate();
                ButtonBaseAdapter.LayoutData layout = this.Layout(e).Layout();
                base.PaintButtonBackground(e, base.Control.ClientRectangle, null);
                int num = layout.focus.X & 1;
                if (!Application.RenderWithVisualStyles)
                {
                    num = 1 - num;
                }
                if (!layout.options.everettButtonCompat)
                {
                    layout.textBounds.Offset(-1, -1);
                }
                layout.imageBounds.Offset(-1, -1);
                layout.focus.Offset(-(num + 1), -2);
                layout.focus.Width = (layout.textBounds.Width + layout.imageBounds.Width) - 1;
                layout.focus.Intersect(layout.textBounds);
                if (((layout.options.textAlign != (ContentAlignment.BottomLeft | ContentAlignment.MiddleLeft | ContentAlignment.TopLeft)) && layout.options.useCompatibleTextRendering) && layout.options.font.Italic)
                {
                    layout.focus.Width += 2;
                }
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

