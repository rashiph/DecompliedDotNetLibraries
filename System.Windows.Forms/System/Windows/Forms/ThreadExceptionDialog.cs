namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    [ClassInterface(ClassInterfaceType.AutoDispatch), ComVisible(true), UIPermission(SecurityAction.Assert, Window=UIPermissionWindow.AllWindows), SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class ThreadExceptionDialog : Form
    {
        private Bitmap collapseImage;
        private Button continueButton;
        private TextBox details;
        private Button detailsButton;
        private bool detailsVisible;
        private Bitmap expandImage;
        private Button helpButton;
        private const int MAXHEIGHT = 0x145;
        private const int MAXWIDTH = 440;
        private Label message;
        private PictureBox pictureBox;
        private Button quitButton;

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public event EventHandler AutoSizeChanged
        {
            add
            {
                base.AutoSizeChanged += value;
            }
            remove
            {
                base.AutoSizeChanged -= value;
            }
        }

        public ThreadExceptionDialog(Exception t)
        {
            string str;
            string message;
            Button[] buttonArray;
            this.pictureBox = new PictureBox();
            this.message = new Label();
            this.continueButton = new Button();
            this.quitButton = new Button();
            this.detailsButton = new Button();
            this.helpButton = new Button();
            this.details = new TextBox();
            bool flag = false;
            WarningException exception = t as WarningException;
            if (exception != null)
            {
                str = "ExDlgWarningText";
                message = exception.Message;
                if (exception.HelpUrl == null)
                {
                    buttonArray = new Button[] { this.continueButton };
                }
                else
                {
                    buttonArray = new Button[] { this.continueButton, this.helpButton };
                }
            }
            else
            {
                message = t.Message;
                flag = true;
                if (Application.AllowQuit)
                {
                    if (t is SecurityException)
                    {
                        str = "ExDlgSecurityErrorText";
                    }
                    else
                    {
                        str = "ExDlgErrorText";
                    }
                    buttonArray = new Button[] { this.detailsButton, this.continueButton, this.quitButton };
                }
                else
                {
                    if (t is SecurityException)
                    {
                        str = "ExDlgSecurityContinueErrorText";
                    }
                    else
                    {
                        str = "ExDlgContinueErrorText";
                    }
                    buttonArray = new Button[] { this.detailsButton, this.continueButton };
                }
            }
            if (message.Length == 0)
            {
                message = t.GetType().Name;
            }
            if (t is SecurityException)
            {
                message = System.Windows.Forms.SR.GetString(str, new object[] { t.GetType().Name, Trim(message) });
            }
            else
            {
                message = System.Windows.Forms.SR.GetString(str, new object[] { Trim(message) });
            }
            StringBuilder builder = new StringBuilder();
            string str3 = "\r\n";
            string str4 = System.Windows.Forms.SR.GetString("ExDlgMsgSeperator");
            string format = System.Windows.Forms.SR.GetString("ExDlgMsgSectionSeperator");
            if (Application.CustomThreadExceptionHandlerAttached)
            {
                builder.Append(System.Windows.Forms.SR.GetString("ExDlgMsgHeaderNonSwitchable"));
            }
            else
            {
                builder.Append(System.Windows.Forms.SR.GetString("ExDlgMsgHeaderSwitchable"));
            }
            builder.Append(string.Format(CultureInfo.CurrentCulture, format, new object[] { System.Windows.Forms.SR.GetString("ExDlgMsgExceptionSection") }));
            builder.Append(t.ToString());
            builder.Append(str3);
            builder.Append(str3);
            builder.Append(string.Format(CultureInfo.CurrentCulture, format, new object[] { System.Windows.Forms.SR.GetString("ExDlgMsgLoadedAssembliesSection") }));
            new FileIOPermission(PermissionState.Unrestricted).Assert();
            try
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    AssemblyName name = assembly.GetName();
                    string fileVersion = System.Windows.Forms.SR.GetString("NotAvailable");
                    try
                    {
                        if ((name.EscapedCodeBase != null) && (name.EscapedCodeBase.Length > 0))
                        {
                            Uri uri = new Uri(name.EscapedCodeBase);
                            if (uri.Scheme == "file")
                            {
                                fileVersion = FileVersionInfo.GetVersionInfo(System.Windows.Forms.NativeMethods.GetLocalPath(name.EscapedCodeBase)).FileVersion;
                            }
                        }
                    }
                    catch (FileNotFoundException)
                    {
                    }
                    builder.Append(System.Windows.Forms.SR.GetString("ExDlgMsgLoadedAssembliesEntry", new object[] { name.Name, name.Version, fileVersion, name.EscapedCodeBase }));
                    builder.Append(str4);
                }
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            builder.Append(string.Format(CultureInfo.CurrentCulture, format, new object[] { System.Windows.Forms.SR.GetString("ExDlgMsgJITDebuggingSection") }));
            if (Application.CustomThreadExceptionHandlerAttached)
            {
                builder.Append(System.Windows.Forms.SR.GetString("ExDlgMsgFooterNonSwitchable"));
            }
            else
            {
                builder.Append(System.Windows.Forms.SR.GetString("ExDlgMsgFooterSwitchable"));
            }
            builder.Append(str3);
            builder.Append(str3);
            string str7 = builder.ToString();
            Graphics graphics = this.message.CreateGraphicsInternal();
            Size size = Size.Ceiling(graphics.MeasureString(message, this.Font, 0x164));
            size.Height += 4;
            graphics.Dispose();
            if (size.Width < 180)
            {
                size.Width = 180;
            }
            if (size.Height > 0x145)
            {
                size.Height = 0x145;
            }
            int width = size.Width + 0x54;
            int y = Math.Max(size.Height, 40) + 0x1a;
            System.Windows.Forms.IntSecurity.GetParent.Assert();
            try
            {
                Form activeForm = Form.ActiveForm;
                if ((activeForm == null) || (activeForm.Text.Length == 0))
                {
                    this.Text = System.Windows.Forms.SR.GetString("ExDlgCaption");
                }
                else
                {
                    this.Text = System.Windows.Forms.SR.GetString("ExDlgCaption2", new object[] { activeForm.Text });
                }
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            base.AcceptButton = this.continueButton;
            base.CancelButton = this.continueButton;
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.StartPosition = FormStartPosition.CenterScreen;
            base.Icon = null;
            base.ClientSize = new Size(width, y + 0x1f);
            base.TopMost = true;
            this.pictureBox.Location = new Point(0, 0);
            this.pictureBox.Size = new Size(0x40, 0x40);
            this.pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
            if (t is SecurityException)
            {
                this.pictureBox.Image = SystemIcons.Information.ToBitmap();
            }
            else
            {
                this.pictureBox.Image = SystemIcons.Error.ToBitmap();
            }
            base.Controls.Add(this.pictureBox);
            this.message.SetBounds(0x40, 8 + ((40 - Math.Min(size.Height, 40)) / 2), size.Width, size.Height);
            this.message.Text = message;
            base.Controls.Add(this.message);
            this.continueButton.Text = System.Windows.Forms.SR.GetString("ExDlgContinue");
            this.continueButton.FlatStyle = FlatStyle.Standard;
            this.continueButton.DialogResult = DialogResult.Cancel;
            this.quitButton.Text = System.Windows.Forms.SR.GetString("ExDlgQuit");
            this.quitButton.FlatStyle = FlatStyle.Standard;
            this.quitButton.DialogResult = DialogResult.Abort;
            this.helpButton.Text = System.Windows.Forms.SR.GetString("ExDlgHelp");
            this.helpButton.FlatStyle = FlatStyle.Standard;
            this.helpButton.DialogResult = DialogResult.Yes;
            this.detailsButton.Text = System.Windows.Forms.SR.GetString("ExDlgShowDetails");
            this.detailsButton.FlatStyle = FlatStyle.Standard;
            this.detailsButton.Click += new EventHandler(this.DetailsClick);
            Button detailsButton = null;
            int num3 = 0;
            if (flag)
            {
                detailsButton = this.detailsButton;
                this.expandImage = new Bitmap(base.GetType(), "down.bmp");
                this.expandImage.MakeTransparent();
                this.collapseImage = new Bitmap(base.GetType(), "up.bmp");
                this.collapseImage.MakeTransparent();
                detailsButton.SetBounds(8, y, 100, 0x17);
                detailsButton.Image = this.expandImage;
                detailsButton.ImageAlign = ContentAlignment.MiddleLeft;
                base.Controls.Add(detailsButton);
                num3 = 1;
            }
            int x = (width - 8) - (((buttonArray.Length - num3) * 0x69) - 5);
            for (int i = num3; i < buttonArray.Length; i++)
            {
                detailsButton = buttonArray[i];
                detailsButton.SetBounds(x, y, 100, 0x17);
                base.Controls.Add(detailsButton);
                x += 0x69;
            }
            this.details.Text = str7;
            this.details.ScrollBars = ScrollBars.Both;
            this.details.Multiline = true;
            this.details.ReadOnly = true;
            this.details.WordWrap = false;
            this.details.TabStop = false;
            this.details.AcceptsReturn = false;
            this.details.SetBounds(8, y + 0x1f, width - 0x10, 0x9a);
            base.Controls.Add(this.details);
        }

        private void DetailsClick(object sender, EventArgs eventargs)
        {
            int num = this.details.Height + 8;
            if (this.detailsVisible)
            {
                num = -num;
            }
            base.Height += num;
            this.detailsVisible = !this.detailsVisible;
            this.detailsButton.Image = this.detailsVisible ? this.collapseImage : this.expandImage;
        }

        private static string Trim(string s)
        {
            if (s == null)
            {
                return s;
            }
            int length = s.Length;
            while ((length > 0) && (s[length - 1] == '.'))
            {
                length--;
            }
            return s.Substring(0, length);
        }

        [EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public override bool AutoSize
        {
            get
            {
                return base.AutoSize;
            }
            set
            {
                base.AutoSize = value;
            }
        }
    }
}

