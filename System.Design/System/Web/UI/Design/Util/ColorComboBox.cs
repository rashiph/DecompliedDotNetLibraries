namespace System.Web.UI.Design.Util
{
    using System;
    using System.Security.Permissions;
    using System.Windows.Forms;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal sealed class ColorComboBox : ComboBox
    {
        private static readonly string[] COLOR_VALUES = new string[] { "Aqua", "Black", "Blue", "Fuchsia", "Gray", "Green", "Lime", "Maroon", "Navy", "Olive", "Purple", "Red", "Silver", "Teal", "White", "Yellow" };

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!base.DesignMode && !base.RecreatingHandle)
            {
                base.Items.Clear();
                base.Items.AddRange(COLOR_VALUES);
            }
        }

        public string Color
        {
            get
            {
                int selectedIndex = this.SelectedIndex;
                if (selectedIndex != -1)
                {
                    return COLOR_VALUES[selectedIndex];
                }
                return this.Text.Trim();
            }
            set
            {
                this.SelectedIndex = -1;
                this.Text = string.Empty;
                if (value != null)
                {
                    string strB = value.Trim();
                    if (strB.Length != 0)
                    {
                        for (int i = 0; i < COLOR_VALUES.Length; i++)
                        {
                            if (string.Compare(COLOR_VALUES[i], strB, StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                strB = COLOR_VALUES[i];
                                break;
                            }
                        }
                        this.Text = strB;
                    }
                }
            }
        }
    }
}

