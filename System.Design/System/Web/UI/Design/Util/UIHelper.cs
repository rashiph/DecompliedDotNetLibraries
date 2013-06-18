namespace System.Web.UI.Design.Util
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    internal static class UIHelper
    {
        internal static void UpdateFieldsCheckedListBoxColumnWidth(CheckedListBox checkedListBox)
        {
            int num = 0;
            using (Graphics graphics = checkedListBox.CreateGraphics())
            {
                foreach (object obj2 in checkedListBox.Items)
                {
                    string text = obj2.ToString();
                    num = Math.Max(num, (int) graphics.MeasureString(text, checkedListBox.Font).Width);
                }
            }
            num += 50;
            checkedListBox.ColumnWidth = num;
        }
    }
}

