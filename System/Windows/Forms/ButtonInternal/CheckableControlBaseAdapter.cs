namespace System.Windows.Forms.ButtonInternal
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal abstract class CheckableControlBaseAdapter : ButtonBaseAdapter
    {
        private ButtonBaseAdapter buttonAdapter;
        private const int standardCheckSize = 13;

        internal CheckableControlBaseAdapter(ButtonBase control) : base(control)
        {
        }

        internal override ButtonBaseAdapter.LayoutOptions CommonLayout()
        {
            ButtonBaseAdapter.LayoutOptions options = base.CommonLayout();
            options.growBorderBy1PxWhenDefault = false;
            options.borderSize = 0;
            options.paddingSize = 0;
            options.maxFocus = false;
            options.focusOddEvenFixup = true;
            options.checkSize = 13;
            return options;
        }

        protected abstract ButtonBaseAdapter CreateButtonAdapter();
        internal static float GetDpiScaleRatio(Graphics g)
        {
            if (g == null)
            {
                return 1f;
            }
            return (g.DpiX / 96f);
        }

        internal override Size GetPreferredSizeCore(Size proposedSize)
        {
            Size preferredSizeCore;
            if (this.Appearance == System.Windows.Forms.Appearance.Button)
            {
                return this.ButtonAdapter.GetPreferredSizeCore(proposedSize);
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

        private System.Windows.Forms.Appearance Appearance
        {
            get
            {
                CheckBox control = base.Control as CheckBox;
                if (control != null)
                {
                    return control.Appearance;
                }
                RadioButton button = base.Control as RadioButton;
                if (button != null)
                {
                    return button.Appearance;
                }
                return System.Windows.Forms.Appearance.Normal;
            }
        }

        protected ButtonBaseAdapter ButtonAdapter
        {
            get
            {
                if (this.buttonAdapter == null)
                {
                    this.buttonAdapter = this.CreateButtonAdapter();
                }
                return this.buttonAdapter;
            }
        }
    }
}

