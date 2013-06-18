namespace System.Web.UI.Design.Util
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal sealed class AutoSizeComboBox : ComboBox
    {
        private bool _dropDownWidthValid;
        private const int MaxDropDownWidth = 600;

        private void AutoSizeComboBoxDropDown()
        {
            int num2;
            int num = 0;
            using (Graphics graphics = Graphics.FromImage(new Bitmap(1, 1)))
            {
                foreach (object obj2 in base.Items)
                {
                    if (obj2 != null)
                    {
                        Size size = graphics.MeasureString(obj2.ToString(), this.Font, 0, new StringFormat(StringFormatFlags.NoWrap | StringFormatFlags.FitBlackBox)).ToSize();
                        num = Math.Max(num, size.Width);
                        if (num >= 600)
                        {
                            num = 600;
                            goto Label_009A;
                        }
                    }
                }
            }
        Label_009A:
            num2 = (num + SystemInformation.VerticalScrollBarWidth) + (2 * SystemInformation.BorderSize.Width);
            base.DropDownWidth = num2 + 1;
            base.DropDownWidth = num2;
        }

        public void InvalidateDropDownWidth()
        {
            this._dropDownWidthValid = false;
        }

        protected override void OnDropDown(EventArgs e)
        {
            if (!this._dropDownWidthValid)
            {
                this.AutoSizeComboBoxDropDown();
                this._dropDownWidthValid = true;
            }
            base.OnDropDown(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this._dropDownWidthValid = false;
        }
    }
}

