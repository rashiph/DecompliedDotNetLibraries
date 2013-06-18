namespace System.Windows.Forms.ButtonInternal
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal class CheckBoxPopupAdapter : CheckBoxBaseAdapter
    {
        internal CheckBoxPopupAdapter(ButtonBase control) : base(control)
        {
        }

        protected override ButtonBaseAdapter CreateButtonAdapter()
        {
            return new ButtonPopupAdapter(base.Control);
        }

        protected override ButtonBaseAdapter.LayoutOptions Layout(PaintEventArgs e)
        {
            return this.PaintPopupLayout(e, true);
        }

        internal override void PaintDown(PaintEventArgs e, CheckState state)
        {
            if (base.Control.Appearance == Appearance.Button)
            {
                new ButtonPopupAdapter(base.Control).PaintDown(e, base.Control.CheckState);
            }
            else
            {
                Graphics g = e.Graphics;
                ButtonBaseAdapter.ColorData colors = base.PaintPopupRender(e.Graphics).Calculate();
                ButtonBaseAdapter.LayoutData layout = this.PaintPopupLayout(e, true).Layout();
                Region clip = e.Graphics.Clip;
                base.PaintButtonBackground(e, base.Control.ClientRectangle, null);
                base.PaintImage(e, layout);
                base.DrawCheckBackground(e, layout.checkBounds, colors.windowText, colors.buttonFace, true, colors);
                CheckBoxBaseAdapter.DrawPopupBorder(g, layout.checkBounds, colors);
                base.DrawCheckOnly(e, layout, colors, colors.windowText, colors.buttonFace, true);
                base.PaintField(e, layout, colors, colors.windowText, true);
            }
        }

        internal override void PaintOver(PaintEventArgs e, CheckState state)
        {
            Graphics g = e.Graphics;
            if (base.Control.Appearance == Appearance.Button)
            {
                new ButtonPopupAdapter(base.Control).PaintOver(e, base.Control.CheckState);
            }
            else
            {
                ButtonBaseAdapter.ColorData colors = base.PaintPopupRender(e.Graphics).Calculate();
                ButtonBaseAdapter.LayoutData layout = this.PaintPopupLayout(e, true).Layout();
                Region clip = e.Graphics.Clip;
                base.PaintButtonBackground(e, base.Control.ClientRectangle, null);
                base.PaintImage(e, layout);
                base.DrawCheckBackground(e, layout.checkBounds, colors.windowText, colors.options.highContrast ? colors.buttonFace : colors.highlight, true, colors);
                CheckBoxBaseAdapter.DrawPopupBorder(g, layout.checkBounds, colors);
                base.DrawCheckOnly(e, layout, colors, colors.windowText, colors.highlight, true);
                e.Graphics.Clip = clip;
                e.Graphics.ExcludeClip(layout.checkArea);
                base.PaintField(e, layout, colors, colors.windowText, true);
            }
        }

        private ButtonBaseAdapter.LayoutOptions PaintPopupLayout(PaintEventArgs e, bool show3D)
        {
            ButtonBaseAdapter.LayoutOptions options = this.CommonLayout();
            options.shadowedText = false;
            if (show3D)
            {
                options.checkSize = (int) ((11f * CheckableControlBaseAdapter.GetDpiScaleRatio(e.Graphics)) + 1f);
                return options;
            }
            options.checkSize = (int) (11f * CheckableControlBaseAdapter.GetDpiScaleRatio(e.Graphics));
            options.checkPaddingSize = 1;
            return options;
        }

        internal static ButtonBaseAdapter.LayoutOptions PaintPopupLayout(Graphics g, bool show3D, int checkSize, Rectangle clientRectangle, Padding padding, bool isDefault, Font font, string text, bool enabled, ContentAlignment textAlign, RightToLeft rtl)
        {
            ButtonBaseAdapter.LayoutOptions options = ButtonBaseAdapter.CommonLayout(clientRectangle, padding, isDefault, font, text, enabled, textAlign, rtl);
            options.shadowedText = false;
            if (show3D)
            {
                options.checkSize = (int) ((checkSize * CheckableControlBaseAdapter.GetDpiScaleRatio(g)) + 1f);
                return options;
            }
            options.checkSize = (int) (checkSize * CheckableControlBaseAdapter.GetDpiScaleRatio(g));
            options.checkPaddingSize = 1;
            return options;
        }

        internal override void PaintUp(PaintEventArgs e, CheckState state)
        {
            if (base.Control.Appearance == Appearance.Button)
            {
                new ButtonPopupAdapter(base.Control).PaintUp(e, base.Control.CheckState);
            }
            else
            {
                Graphics graphics = e.Graphics;
                ButtonBaseAdapter.ColorData colors = base.PaintPopupRender(e.Graphics).Calculate();
                ButtonBaseAdapter.LayoutData layout = this.PaintPopupLayout(e, false).Layout();
                Region clip = e.Graphics.Clip;
                base.PaintButtonBackground(e, base.Control.ClientRectangle, null);
                base.PaintImage(e, layout);
                base.DrawCheckBackground(e, layout.checkBounds, colors.windowText, colors.options.highContrast ? colors.buttonFace : colors.highlight, true, colors);
                ButtonBaseAdapter.DrawFlatBorder(e.Graphics, layout.checkBounds, colors.buttonShadow);
                base.DrawCheckOnly(e, layout, colors, colors.windowText, colors.highlight, true);
                base.PaintField(e, layout, colors, colors.windowText, true);
            }
        }
    }
}

