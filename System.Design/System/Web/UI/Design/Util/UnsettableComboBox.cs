namespace System.Web.UI.Design.Util
{
    using System;
    using System.Security.Permissions;
    using System.Windows.Forms;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal sealed class UnsettableComboBox : ComboBox
    {
        private bool internalChange;
        private string notSetText = System.Design.SR.GetString("UnsettableComboBox_NotSet");

        public UnsettableComboBox()
        {
            base.Items.Add(this.notSetText);
        }

        public void AddItem(object item)
        {
            base.Items.Add(item);
        }

        public void EnsureNotSetItem()
        {
            if (base.Items.Count == 0)
            {
                base.Items.Add(this.notSetText);
            }
        }

        public bool IsSet()
        {
            return (this.SelectedIndex > 0);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            if (this.SelectedIndex == 0)
            {
                this.internalChange = true;
                this.SelectedIndex = -1;
                this.internalChange = false;
            }
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            if (!this.internalChange)
            {
                base.OnSelectedIndexChanged(e);
            }
        }

        public string NotSetText
        {
            get
            {
                return this.notSetText;
            }
            set
            {
                this.notSetText = value;
                base.Items.RemoveAt(0);
                base.Items.Insert(0, this.notSetText);
            }
        }

        public override string Text
        {
            get
            {
                if ((this.SelectedIndex != 0) && (this.SelectedIndex != -1))
                {
                    return base.Text;
                }
                return string.Empty;
            }
            set
            {
                base.Text = value;
            }
        }
    }
}

