namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Security.Permissions;
    using System.Text.RegularExpressions;
    using System.Web.UI.Design.Util;
    using System.Windows.Forms;

    [ToolboxItem(false), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class RegexEditorDialog : Form
    {
        private static readonly RegExpEntry[] _entries = new RegExpEntry[] { new RegExpEntry("RegexCanned_SocialSecurity", "RegexCanned_SocialSecurity_Format"), new RegExpEntry("RegexCanned_USPhone", "RegexCanned_USPhone_Format"), new RegExpEntry("RegexCanned_Zip", "RegexCanned_Zip_Format"), new RegExpEntry("RegexCanned_FrZip", "RegexCanned_FrZip_Format"), new RegExpEntry("RegexCanned_FrPhone", "RegexCanned_FrPhone_Format"), new RegExpEntry("RegexCanned_DeZip", "RegexCanned_DeZip_Format"), new RegExpEntry("RegexCanned_DePhone", "RegexCanned_DePhone_Format"), new RegExpEntry("RegexCanned_JpnZip", "RegexCanned_JpnZip_Format"), new RegExpEntry("RegexCanned_JpnPhone", "RegexCanned_JpnPhone_Format"), new RegExpEntry("RegexCanned_PrcZip", "RegexCanned_PrcZip_Format"), new RegExpEntry("RegexCanned_PrcPhone", "RegexCanned_PrcPhone_Format"), new RegExpEntry("RegexCanned_PrcSocialSecurity", "RegexCanned_PrcSocialSecurity_Format") };
        private static object[] cannedExpressions;
        private Button cmdCancel;
        private Button cmdOK;
        private Button cmdTestValidate;
        private Container components;
        private bool firstActivate = true;
        private GroupBox grpExpression;
        private Label lblExpression;
        private Label lblInput;
        private Label lblStandardExpressions;
        private Label lblTestResult;
        private ListBox lstStandardExpressions;
        private string regularExpression;
        private bool settingValue;
        private ISite site;
        private TextBox txtExpression;
        private TextBox txtSampleInput;

        public RegexEditorDialog(ISite site)
        {
            this.site = site;
            this.InitializeComponent();
            this.settingValue = false;
            this.regularExpression = string.Empty;
        }

        protected void cmdHelp_Click(object sender, EventArgs e)
        {
            this.ShowHelp();
        }

        protected void cmdOK_Click(object sender, EventArgs e)
        {
            this.RegularExpression = this.txtExpression.Text;
        }

        protected void cmdTestValidate_Click(object sender, EventArgs args)
        {
            try
            {
                Match match = Regex.Match(this.txtSampleInput.Text, this.txtExpression.Text);
                bool flag = (match.Success && (match.Index == 0)) && (match.Length == this.txtSampleInput.Text.Length);
                if (this.txtSampleInput.Text.Length == 0)
                {
                    flag = true;
                }
                this.lblTestResult.Text = flag ? System.Design.SR.GetString("RegexEditor_InputValid") : System.Design.SR.GetString("RegexEditor_InputInvalid");
                this.lblTestResult.ForeColor = flag ? Color.Black : Color.Red;
            }
            catch
            {
                this.lblTestResult.Text = System.Design.SR.GetString("RegexEditor_BadExpression");
                this.lblTestResult.ForeColor = Color.Red;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void Form_HelpRequested(object sender, HelpEventArgs e)
        {
            this.ShowHelp();
        }

        private void HelpButton_Click(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.ShowHelp();
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this.lblTestResult = new Label();
            this.lstStandardExpressions = new ListBox();
            this.lblStandardExpressions = new Label();
            this.cmdTestValidate = new Button();
            this.txtExpression = new TextBox();
            this.lblInput = new Label();
            this.grpExpression = new GroupBox();
            this.txtSampleInput = new TextBox();
            this.cmdCancel = new Button();
            this.lblExpression = new Label();
            this.cmdOK = new Button();
            Font dialogFont = UIServiceHelper.GetDialogFont(this.site);
            if (dialogFont != null)
            {
                this.Font = dialogFont;
            }
            if (!string.Equals(System.Design.SR.GetString("RTL"), "RTL_False", StringComparison.Ordinal))
            {
                this.RightToLeft = RightToLeft.Yes;
                this.RightToLeftLayout = true;
            }
            base.MinimizeBox = false;
            base.MaximizeBox = false;
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterParent;
            this.Text = System.Design.SR.GetString("RegexEditor_Title");
            base.ImeMode = ImeMode.Disable;
            base.AcceptButton = this.cmdOK;
            base.CancelButton = this.cmdCancel;
            base.Icon = null;
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.ClientSize = new Size(0x15c, 0xd7);
            base.Activated += new EventHandler(this.RegexTypeEditor_Activated);
            base.HelpRequested += new HelpEventHandler(this.Form_HelpRequested);
            base.HelpButton = true;
            base.HelpButtonClicked += new CancelEventHandler(this.HelpButton_Click);
            this.lstStandardExpressions.Location = new Point(12, 30);
            this.lstStandardExpressions.Size = new Size(0x144, 0x54);
            this.lstStandardExpressions.TabIndex = 1;
            this.lstStandardExpressions.SelectedIndexChanged += new EventHandler(this.lstStandardExpressions_SelectedIndexChanged);
            this.lstStandardExpressions.Sorted = true;
            this.lstStandardExpressions.IntegralHeight = true;
            this.lstStandardExpressions.Items.AddRange(this.CannedExpressions);
            this.lblStandardExpressions.Location = new Point(12, 12);
            this.lblStandardExpressions.Text = System.Design.SR.GetString("RegexEditor_StdExp");
            this.lblStandardExpressions.Size = new Size(0x148, 0x10);
            this.lblStandardExpressions.TabIndex = 0;
            this.txtExpression.Location = new Point(12, 140);
            this.txtExpression.TabIndex = 3;
            this.txtExpression.Size = new Size(0x144, 20);
            this.txtExpression.TextChanged += new EventHandler(this.txtExpression_TextChanged);
            this.lblExpression.Location = new Point(12, 0x7a);
            this.lblExpression.Text = System.Design.SR.GetString("RegexEditor_ValidationExpression");
            this.lblExpression.Size = new Size(0x148, 0x10);
            this.lblExpression.TabIndex = 2;
            this.cmdOK.Location = new Point(180, 180);
            this.cmdOK.DialogResult = DialogResult.OK;
            this.cmdOK.Size = new Size(0x4b, 0x17);
            this.cmdOK.TabIndex = 9;
            this.cmdOK.Text = System.Design.SR.GetString("OK");
            this.cmdOK.FlatStyle = FlatStyle.System;
            this.cmdOK.Click += new EventHandler(this.cmdOK_Click);
            this.cmdCancel.Location = new Point(0x105, 180);
            this.cmdCancel.DialogResult = DialogResult.Cancel;
            this.cmdCancel.Size = new Size(0x4b, 0x17);
            this.cmdCancel.TabIndex = 10;
            this.cmdCancel.FlatStyle = FlatStyle.System;
            this.cmdCancel.Text = System.Design.SR.GetString("Cancel");
            this.grpExpression.Location = new Point(8, 280);
            this.grpExpression.ImeMode = ImeMode.Disable;
            this.grpExpression.TabIndex = 4;
            this.grpExpression.TabStop = false;
            this.grpExpression.Text = System.Design.SR.GetString("RegexEditor_TestExpression");
            this.grpExpression.Size = new Size(0x148, 80);
            this.grpExpression.Visible = false;
            this.txtSampleInput.Location = new Point(0x58, 0x18);
            this.txtSampleInput.TabIndex = 6;
            this.txtSampleInput.Size = new Size(160, 20);
            this.grpExpression.Controls.Add(this.lblTestResult);
            this.grpExpression.Controls.Add(this.txtSampleInput);
            this.grpExpression.Controls.Add(this.cmdTestValidate);
            this.grpExpression.Controls.Add(this.lblInput);
            this.cmdTestValidate.Location = new Point(0x100, 0x18);
            this.cmdTestValidate.Size = new Size(0x38, 20);
            this.cmdTestValidate.TabIndex = 7;
            this.cmdTestValidate.Text = System.Design.SR.GetString("RegexEditor_Validate");
            this.cmdTestValidate.FlatStyle = FlatStyle.System;
            this.cmdTestValidate.Click += new EventHandler(this.cmdTestValidate_Click);
            this.lblInput.Location = new Point(8, 0x1c);
            this.lblInput.Text = System.Design.SR.GetString("RegexEditor_SampleInput");
            this.lblInput.Size = new Size(80, 0x10);
            this.lblInput.TabIndex = 5;
            this.lblTestResult.Location = new Point(8, 0x38);
            this.lblTestResult.Size = new Size(0x138, 0x10);
            this.lblTestResult.TabIndex = 8;
            base.Controls.Add(this.txtExpression);
            base.Controls.Add(this.lstStandardExpressions);
            base.Controls.Add(this.lblStandardExpressions);
            base.Controls.Add(this.lblExpression);
            base.Controls.Add(this.grpExpression);
            base.Controls.Add(this.cmdCancel);
            base.Controls.Add(this.cmdOK);
        }

        protected void lstStandardExpressions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this.settingValue && (this.lstStandardExpressions.SelectedIndex >= 1))
            {
                CannedExpression selectedItem = (CannedExpression) this.lstStandardExpressions.SelectedItem;
                this.settingValue = true;
                this.txtExpression.Text = selectedItem.Expression;
                this.settingValue = false;
            }
        }

        protected void RegexTypeEditor_Activated(object sender, EventArgs e)
        {
            if (this.firstActivate)
            {
                this.txtExpression.Text = this.RegularExpression;
                this.UpdateExpressionList();
                this.firstActivate = false;
            }
        }

        private void ShowHelp()
        {
            IHelpService service = (IHelpService) this.site.GetService(typeof(IHelpService));
            if (service != null)
            {
                service.ShowHelpFromKeyword("net.Asp.RegularExpressionEditor");
            }
        }

        protected void txtExpression_TextChanged(object sender, EventArgs e)
        {
            if (!this.settingValue && !this.firstActivate)
            {
                this.lblTestResult.Text = string.Empty;
                this.UpdateExpressionList();
            }
        }

        private void UpdateExpressionList()
        {
            bool flag = false;
            this.settingValue = true;
            string text = this.txtExpression.Text;
            for (int i = 1; i < this.lstStandardExpressions.Items.Count; i++)
            {
                if (text == ((CannedExpression) this.lstStandardExpressions.Items[i]).Expression)
                {
                    this.lstStandardExpressions.SelectedIndex = i;
                    flag = true;
                }
            }
            if (!flag)
            {
                this.lstStandardExpressions.SelectedIndex = 0;
            }
            this.settingValue = false;
        }

        private object[] CannedExpressions
        {
            get
            {
                if (cannedExpressions == null)
                {
                    ArrayList list = new ArrayList();
                    list.Add(System.Design.SR.GetString("RegexCanned_Custom"));
                    list.Add(new CannedExpression(System.Design.SR.GetString("RegexCanned_Email"), @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"));
                    list.Add(new CannedExpression(System.Design.SR.GetString("RegexCanned_URL"), @"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?"));
                    foreach (RegExpEntry entry in _entries)
                    {
                        if (entry.Name.Length > 0)
                        {
                            list.Add(new CannedExpression(System.Design.SR.GetString(entry.Name), System.Design.SR.GetString(entry.Format)));
                        }
                    }
                    cannedExpressions = new object[list.Count];
                    list.CopyTo(cannedExpressions);
                }
                return cannedExpressions;
            }
        }

        public string RegularExpression
        {
            get
            {
                return this.regularExpression;
            }
            set
            {
                this.regularExpression = value;
            }
        }

        private class CannedExpression
        {
            public string Description;
            public string Expression;

            public CannedExpression(string description, string expression)
            {
                this.Description = description;
                this.Expression = expression;
            }

            public override string ToString()
            {
                return this.Description;
            }
        }

        private class RegExpEntry
        {
            public string Format;
            public string Name;

            public RegExpEntry(string name, string format)
            {
                this.Name = name;
                this.Format = format;
            }
        }
    }
}

