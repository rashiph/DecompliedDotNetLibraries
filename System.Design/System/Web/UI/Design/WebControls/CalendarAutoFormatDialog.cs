namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Security.Permissions;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    [Obsolete("Use of this type is not recommended because the AutoFormat dialog is launched by the designer host. The list of available AutoFormats is exposed on the ControlDesigner in the AutoFormats property. http://go.microsoft.com/fwlink/?linkid=14202"), ToolboxItem(false), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class CalendarAutoFormatDialog : Form
    {
        private Calendar calendar;
        private System.Windows.Forms.Button cancelButton;
        private bool firstActivate = true;
        private System.Windows.Forms.Button okButton;
        private bool schemeDirty;
        private System.Windows.Forms.Label schemeNameLabel;
        private System.Windows.Forms.ListBox schemeNameList;
        private MSHTMLHost schemePreview;
        private System.Windows.Forms.Label schemePreviewLabel;

        public CalendarAutoFormatDialog(Calendar calendar)
        {
            this.calendar = calendar;
            this.InitForm();
        }

        protected void DoDelayLoadActions()
        {
            this.schemePreview.CreateTrident();
            this.schemePreview.ActivateTrident();
        }

        private Calendar GetPreviewCalendar()
        {
            Calendar wc = new Calendar {
                ShowTitle = this.calendar.ShowTitle,
                ShowNextPrevMonth = this.calendar.ShowNextPrevMonth,
                ShowDayHeader = this.calendar.ShowDayHeader,
                SelectionMode = this.calendar.SelectionMode
            };
            ((WCScheme) this.schemeNameList.SelectedItem).Apply(wc);
            return wc;
        }

        private void InitForm()
        {
            this.schemeNameLabel = new System.Windows.Forms.Label();
            this.schemeNameList = new System.Windows.Forms.ListBox();
            this.schemePreviewLabel = new System.Windows.Forms.Label();
            this.schemePreview = new MSHTMLHost();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            System.Windows.Forms.Button button = new System.Windows.Forms.Button();
            this.schemeNameLabel.SetBounds(8, 10, 0x9a, 0x10);
            this.schemeNameLabel.Text = System.Design.SR.GetString("CalAFmt_SchemeName");
            this.schemeNameLabel.TabStop = false;
            this.schemeNameLabel.TabIndex = 1;
            this.schemeNameList.TabIndex = 2;
            this.schemeNameList.SetBounds(8, 0x1a, 150, 100);
            this.schemeNameList.UseTabStops = true;
            this.schemeNameList.IntegralHeight = false;
            this.schemeNameList.Items.AddRange(new object[] { new WCSchemeNone(), new WCSchemeStandard(), new WCSchemeProfessional1(), new WCSchemeProfessional2(), new WCSchemeClassic(), new WCSchemeColorful1(), new WCSchemeColorful2() });
            this.schemeNameList.SelectedIndexChanged += new EventHandler(this.OnSelChangedScheme);
            this.schemePreviewLabel.SetBounds(0xa5, 10, 0x5c, 0x10);
            this.schemePreviewLabel.Text = System.Design.SR.GetString("CalAFmt_Preview");
            this.schemePreviewLabel.TabStop = false;
            this.schemePreviewLabel.TabIndex = 3;
            this.schemePreview.SetBounds(0xa5, 0x1a, 270, 240);
            this.schemePreview.TabIndex = 4;
            this.schemePreview.TabStop = false;
            button.Location = new Point(360, 0x114);
            button.Size = new Size(0x4b, 0x17);
            button.TabIndex = 7;
            button.Text = System.Design.SR.GetString("CalAFmt_Help");
            button.FlatStyle = FlatStyle.System;
            button.Click += new EventHandler(this.OnClickHelp);
            this.okButton.Location = new Point(0xc6, 0x114);
            this.okButton.Size = new Size(0x4b, 0x17);
            this.okButton.TabIndex = 5;
            this.okButton.Text = System.Design.SR.GetString("CalAFmt_OK");
            this.okButton.DialogResult = DialogResult.OK;
            this.okButton.FlatStyle = FlatStyle.System;
            this.okButton.Click += new EventHandler(this.OnOKClicked);
            this.cancelButton.Location = new Point(0x117, 0x114);
            this.cancelButton.Size = new Size(0x4b, 0x17);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = System.Design.SR.GetString("CalAFmt_Cancel");
            this.cancelButton.FlatStyle = FlatStyle.System;
            this.cancelButton.DialogResult = DialogResult.Cancel;
            this.Text = System.Design.SR.GetString("CalAFmt_Title");
            base.Size = new Size(450, 0x150);
            base.AcceptButton = this.okButton;
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.CancelButton = this.cancelButton;
            base.Icon = null;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.CenterParent;
            base.Activated += new EventHandler(this.OnActivated);
            base.HelpRequested += new HelpEventHandler(this.OnHelpRequested);
            Font dialogFont = UIServiceHelper.GetDialogFont(this.calendar.Site);
            if (dialogFont != null)
            {
                this.Font = dialogFont;
            }
            base.Controls.Clear();
            base.Controls.AddRange(new Control[] { this.schemePreview, this.schemePreviewLabel, this.schemeNameList, this.schemeNameLabel, this.okButton, this.cancelButton, button });
        }

        protected void OnActivated(object source, EventArgs e)
        {
            if (this.firstActivate)
            {
                this.schemeDirty = false;
                this.DoDelayLoadActions();
                this.schemeNameList.SelectedIndex = 0;
                this.firstActivate = false;
            }
        }

        private void OnClickHelp(object sender, EventArgs e)
        {
            this.ShowHelp();
        }

        private void OnHelpRequested(object sender, HelpEventArgs e)
        {
            this.ShowHelp();
        }

        protected void OnOKClicked(object source, EventArgs e)
        {
            this.SaveComponent();
        }

        protected void OnSelChangedScheme(object source, EventArgs e)
        {
            this.schemeDirty = true;
            this.UpdateSchemePreview();
        }

        protected void SaveComponent()
        {
            if (this.schemeDirty)
            {
                ((WCScheme) this.schemeNameList.SelectedItem).Apply(this.calendar);
                this.schemeDirty = false;
            }
        }

        private void ShowHelp()
        {
            IHelpService service = (IHelpService) this.calendar.Site.GetService(typeof(IHelpService));
            if (service != null)
            {
                service.ShowHelpFromKeyword("net.Asp.Calendar.AutoFormat");
            }
        }

        private void UpdateSchemePreview()
        {
            Calendar previewCalendar = this.GetPreviewCalendar();
            IDesigner designer = TypeDescriptor.CreateDesigner(previewCalendar, typeof(IDesigner));
            designer.Initialize(previewCalendar);
            string designTimeHtml = ((CalendarDesigner) designer).GetDesignTimeHtml();
            this.schemePreview.GetDocument().GetBody().SetInnerHTML(designTimeHtml);
        }

        private abstract class WCScheme
        {
            protected WCScheme()
            {
            }

            public abstract void Apply(Calendar wc);
            public static void ClearCalendar(Calendar wc)
            {
                wc.TitleStyle.Reset();
                wc.NextPrevStyle.Reset();
                wc.DayHeaderStyle.Reset();
                wc.SelectorStyle.Reset();
                wc.DayStyle.Reset();
                wc.OtherMonthDayStyle.Reset();
                wc.WeekendDayStyle.Reset();
                wc.TodayDayStyle.Reset();
                wc.SelectedDayStyle.Reset();
                wc.ControlStyle.Reset();
            }

            public abstract string GetDescription();
            public override string ToString()
            {
                return this.GetDescription();
            }
        }

        private class WCSchemeClassic : CalendarAutoFormatDialog.WCScheme
        {
            public override void Apply(Calendar wc)
            {
                CalendarAutoFormatDialog.WCScheme.ClearCalendar(wc);
                wc.DayNameFormat = DayNameFormat.FirstLetter;
                wc.NextPrevFormat = NextPrevFormat.FullMonth;
                wc.TitleFormat = TitleFormat.Month;
                wc.CellPadding = 2;
                wc.CellSpacing = 0;
                wc.ShowGridLines = false;
                wc.Height = Unit.Pixel(220);
                wc.Width = Unit.Pixel(400);
                wc.BackColor = Color.White;
                wc.BorderColor = Color.Black;
                wc.ForeColor = Color.Black;
                wc.Font.Name = "Times New Roman";
                wc.Font.Size = FontUnit.Point(10);
                wc.TitleStyle.Font.Bold = true;
                wc.TitleStyle.ForeColor = Color.White;
                wc.TitleStyle.BackColor = Color.Black;
                wc.TitleStyle.Font.Size = FontUnit.Point(13);
                wc.TitleStyle.Height = Unit.Point(14);
                wc.NextPrevStyle.ForeColor = Color.White;
                wc.NextPrevStyle.Font.Size = FontUnit.Point(8);
                wc.DayHeaderStyle.Font.Bold = true;
                wc.DayHeaderStyle.Font.Size = FontUnit.Point(7);
                wc.DayHeaderStyle.Font.Name = "Verdana";
                wc.DayHeaderStyle.BackColor = Color.FromArgb(0xcc, 0xcc, 0xcc);
                wc.DayHeaderStyle.ForeColor = Color.FromArgb(0x33, 0x33, 0x33);
                wc.DayHeaderStyle.Height = Unit.Pixel(10);
                wc.SelectorStyle.BackColor = Color.FromArgb(0xcc, 0xcc, 0xcc);
                wc.SelectorStyle.ForeColor = Color.FromArgb(0x33, 0x33, 0x33);
                wc.SelectorStyle.Font.Bold = true;
                wc.SelectorStyle.Font.Size = FontUnit.Point(8);
                wc.SelectorStyle.Font.Name = "Verdana";
                wc.SelectorStyle.Width = Unit.Percentage(1.0);
                wc.DayStyle.Width = Unit.Percentage(14.0);
                wc.TodayDayStyle.BackColor = Color.FromArgb(0xcc, 0xcc, 0x99);
                wc.SelectedDayStyle.BackColor = Color.FromArgb(0xcc, 0x33, 0x33);
                wc.SelectedDayStyle.ForeColor = Color.White;
                wc.OtherMonthDayStyle.ForeColor = Color.FromArgb(0x99, 0x99, 0x99);
            }

            public override string GetDescription()
            {
                return System.Design.SR.GetString("CalAFmt_Scheme_Classic");
            }
        }

        private class WCSchemeColorful1 : CalendarAutoFormatDialog.WCScheme
        {
            public override void Apply(Calendar wc)
            {
                CalendarAutoFormatDialog.WCScheme.ClearCalendar(wc);
                wc.DayNameFormat = DayNameFormat.FirstLetter;
                wc.NextPrevFormat = NextPrevFormat.CustomText;
                wc.TitleFormat = TitleFormat.MonthYear;
                wc.CellPadding = 2;
                wc.CellSpacing = 0;
                wc.ShowGridLines = true;
                wc.Height = Unit.Pixel(200);
                wc.Width = Unit.Pixel(220);
                wc.BackColor = Color.FromArgb(0xff, 0xff, 0xcc);
                wc.BorderColor = Color.FromArgb(0xff, 0xcc, 0x66);
                wc.BorderWidth = Unit.Pixel(1);
                wc.ForeColor = Color.FromArgb(0x66, 0x33, 0x99);
                wc.Font.Name = "Verdana";
                wc.Font.Size = FontUnit.Point(8);
                wc.TitleStyle.Font.Bold = true;
                wc.TitleStyle.Font.Size = FontUnit.Point(9);
                wc.TitleStyle.BackColor = Color.FromArgb(0x99, 0, 0);
                wc.TitleStyle.ForeColor = Color.FromArgb(0xff, 0xff, 0xcc);
                wc.NextPrevStyle.ForeColor = Color.FromArgb(0xff, 0xff, 0xcc);
                wc.NextPrevStyle.Font.Size = FontUnit.Point(9);
                wc.DayHeaderStyle.BackColor = Color.FromArgb(0xff, 0xcc, 0x66);
                wc.DayHeaderStyle.Height = Unit.Pixel(1);
                wc.SelectorStyle.BackColor = Color.FromArgb(0xff, 0xcc, 0x66);
                wc.SelectedDayStyle.BackColor = Color.FromArgb(0xcc, 0xcc, 0xff);
                wc.SelectedDayStyle.Font.Bold = true;
                wc.OtherMonthDayStyle.ForeColor = Color.FromArgb(0xcc, 0x99, 0x66);
                wc.TodayDayStyle.ForeColor = Color.White;
                wc.TodayDayStyle.BackColor = Color.FromArgb(0xff, 0xcc, 0x66);
            }

            public override string GetDescription()
            {
                return System.Design.SR.GetString("CalAFmt_Scheme_Colorful1");
            }
        }

        private class WCSchemeColorful2 : CalendarAutoFormatDialog.WCScheme
        {
            public override void Apply(Calendar wc)
            {
                CalendarAutoFormatDialog.WCScheme.ClearCalendar(wc);
                wc.DayNameFormat = DayNameFormat.FirstLetter;
                wc.NextPrevFormat = NextPrevFormat.CustomText;
                wc.TitleFormat = TitleFormat.MonthYear;
                wc.CellPadding = 1;
                wc.CellSpacing = 0;
                wc.ShowGridLines = false;
                wc.Height = Unit.Pixel(200);
                wc.Width = Unit.Pixel(220);
                wc.BackColor = Color.White;
                wc.BorderColor = Color.FromArgb(0x33, 0x66, 0xcc);
                wc.BorderWidth = Unit.Pixel(1);
                wc.ForeColor = Color.FromArgb(0, 0x33, 0x99);
                wc.Font.Name = "Verdana";
                wc.Font.Size = FontUnit.Point(8);
                wc.TitleStyle.Font.Bold = true;
                wc.TitleStyle.Font.Size = FontUnit.Point(10);
                wc.TitleStyle.BackColor = Color.FromArgb(0, 0x33, 0x99);
                wc.TitleStyle.ForeColor = Color.FromArgb(0xcc, 0xcc, 0xff);
                wc.TitleStyle.BorderColor = Color.FromArgb(0x33, 0x66, 0xcc);
                wc.TitleStyle.BorderStyle = System.Web.UI.WebControls.BorderStyle.Solid;
                wc.TitleStyle.BorderWidth = Unit.Pixel(1);
                wc.TitleStyle.Height = Unit.Pixel(0x19);
                wc.NextPrevStyle.ForeColor = Color.FromArgb(0xcc, 0xcc, 0xff);
                wc.NextPrevStyle.Font.Size = FontUnit.Point(8);
                wc.DayHeaderStyle.BackColor = Color.FromArgb(0x99, 0xcc, 0xcc);
                wc.DayHeaderStyle.ForeColor = Color.FromArgb(0x33, 0x66, 0x66);
                wc.DayHeaderStyle.Height = Unit.Pixel(1);
                wc.SelectorStyle.BackColor = Color.FromArgb(0x99, 0xcc, 0xcc);
                wc.SelectorStyle.ForeColor = Color.FromArgb(0x33, 0x66, 0x66);
                wc.SelectedDayStyle.BackColor = Color.FromArgb(0, 0x99, 0x99);
                wc.SelectedDayStyle.ForeColor = Color.FromArgb(0xcc, 0xff, 0x99);
                wc.SelectedDayStyle.Font.Bold = true;
                wc.OtherMonthDayStyle.ForeColor = Color.FromArgb(0x99, 0x99, 0x99);
                wc.TodayDayStyle.ForeColor = Color.White;
                wc.TodayDayStyle.BackColor = Color.FromArgb(0x99, 0xcc, 0xcc);
                wc.WeekendDayStyle.BackColor = Color.FromArgb(0xcc, 0xcc, 0xff);
            }

            public override string GetDescription()
            {
                return System.Design.SR.GetString("CalAFmt_Scheme_Colorful2");
            }
        }

        private class WCSchemeNone : CalendarAutoFormatDialog.WCScheme
        {
            public override void Apply(Calendar wc)
            {
                CalendarAutoFormatDialog.WCScheme.ClearCalendar(wc);
                wc.DayNameFormat = DayNameFormat.Short;
                wc.NextPrevFormat = NextPrevFormat.CustomText;
                wc.TitleFormat = TitleFormat.MonthYear;
                wc.CellPadding = 2;
                wc.CellSpacing = 0;
                wc.ShowGridLines = false;
            }

            public override string GetDescription()
            {
                return System.Design.SR.GetString("CalAFmt_Scheme_Default");
            }
        }

        private class WCSchemeProfessional1 : CalendarAutoFormatDialog.WCScheme
        {
            public override void Apply(Calendar wc)
            {
                CalendarAutoFormatDialog.WCScheme.ClearCalendar(wc);
                wc.DayNameFormat = DayNameFormat.Short;
                wc.NextPrevFormat = NextPrevFormat.FullMonth;
                wc.TitleFormat = TitleFormat.MonthYear;
                wc.CellPadding = 2;
                wc.CellSpacing = 0;
                wc.ShowGridLines = false;
                wc.Height = Unit.Pixel(190);
                wc.Width = Unit.Pixel(350);
                wc.BorderColor = Color.White;
                wc.BorderWidth = Unit.Pixel(1);
                wc.ForeColor = Color.Black;
                wc.BackColor = Color.White;
                wc.Font.Name = "Verdana";
                wc.Font.Size = FontUnit.Point(9);
                wc.TitleStyle.Font.Bold = true;
                wc.TitleStyle.BorderColor = Color.Black;
                wc.TitleStyle.BorderWidth = Unit.Pixel(4);
                wc.TitleStyle.ForeColor = Color.FromArgb(0x33, 0x33, 0x99);
                wc.TitleStyle.BackColor = Color.White;
                wc.TitleStyle.Font.Size = FontUnit.Point(12);
                wc.NextPrevStyle.Font.Bold = true;
                wc.NextPrevStyle.Font.Size = FontUnit.Point(8);
                wc.NextPrevStyle.VerticalAlign = VerticalAlign.Bottom;
                wc.NextPrevStyle.ForeColor = Color.FromArgb(0x33, 0x33, 0x33);
                wc.DayHeaderStyle.Font.Bold = true;
                wc.DayHeaderStyle.Font.Size = FontUnit.Point(8);
                wc.TodayDayStyle.BackColor = Color.FromArgb(0xcc, 0xcc, 0xcc);
                wc.SelectedDayStyle.BackColor = Color.FromArgb(0x33, 0x33, 0x99);
                wc.SelectedDayStyle.ForeColor = Color.White;
                wc.OtherMonthDayStyle.ForeColor = Color.FromArgb(0x99, 0x99, 0x99);
            }

            public override string GetDescription()
            {
                return System.Design.SR.GetString("CalAFmt_Scheme_Professional1");
            }
        }

        private class WCSchemeProfessional2 : CalendarAutoFormatDialog.WCScheme
        {
            public override void Apply(Calendar wc)
            {
                CalendarAutoFormatDialog.WCScheme.ClearCalendar(wc);
                wc.DayNameFormat = DayNameFormat.Short;
                wc.NextPrevFormat = NextPrevFormat.ShortMonth;
                wc.TitleFormat = TitleFormat.MonthYear;
                wc.CellPadding = 2;
                wc.CellSpacing = 1;
                wc.ShowGridLines = false;
                wc.Height = Unit.Pixel(250);
                wc.Width = Unit.Pixel(330);
                wc.BackColor = Color.White;
                wc.BorderColor = Color.Black;
                wc.BorderStyle = System.Web.UI.WebControls.BorderStyle.Solid;
                wc.ForeColor = Color.Black;
                wc.Font.Name = "Verdana";
                wc.Font.Size = FontUnit.Point(9);
                wc.TitleStyle.Font.Bold = true;
                wc.TitleStyle.ForeColor = Color.White;
                wc.TitleStyle.BackColor = Color.FromArgb(0x33, 0x33, 0x99);
                wc.TitleStyle.Font.Size = FontUnit.Point(12);
                wc.TitleStyle.Height = Unit.Point(12);
                wc.NextPrevStyle.Font.Bold = true;
                wc.NextPrevStyle.Font.Size = FontUnit.Point(8);
                wc.NextPrevStyle.ForeColor = Color.White;
                wc.DayHeaderStyle.ForeColor = Color.FromArgb(0x33, 0x33, 0x33);
                wc.DayHeaderStyle.Font.Bold = true;
                wc.DayHeaderStyle.Font.Size = FontUnit.Point(8);
                wc.DayHeaderStyle.Height = Unit.Point(8);
                wc.DayStyle.BackColor = Color.FromArgb(0xcc, 0xcc, 0xcc);
                wc.TodayDayStyle.BackColor = Color.FromArgb(0x99, 0x99, 0x99);
                wc.TodayDayStyle.ForeColor = Color.White;
                wc.SelectedDayStyle.BackColor = Color.FromArgb(0x33, 0x33, 0x99);
                wc.SelectedDayStyle.ForeColor = Color.White;
                wc.OtherMonthDayStyle.ForeColor = Color.FromArgb(0x99, 0x99, 0x99);
            }

            public override string GetDescription()
            {
                return System.Design.SR.GetString("CalAFmt_Scheme_Professional2");
            }
        }

        private class WCSchemeStandard : CalendarAutoFormatDialog.WCScheme
        {
            public override void Apply(Calendar wc)
            {
                CalendarAutoFormatDialog.WCScheme.ClearCalendar(wc);
                wc.DayNameFormat = DayNameFormat.FirstLetter;
                wc.NextPrevFormat = NextPrevFormat.CustomText;
                wc.TitleFormat = TitleFormat.MonthYear;
                wc.CellPadding = 4;
                wc.CellSpacing = 0;
                wc.ShowGridLines = false;
                wc.Height = Unit.Pixel(180);
                wc.Width = Unit.Pixel(200);
                wc.BorderColor = Color.FromArgb(0x99, 0x99, 0x99);
                wc.ForeColor = Color.Black;
                wc.BackColor = Color.White;
                wc.Font.Name = "Verdana";
                wc.Font.Size = FontUnit.Point(8);
                wc.TitleStyle.Font.Bold = true;
                wc.TitleStyle.BorderColor = Color.Black;
                wc.TitleStyle.BackColor = Color.FromArgb(0x99, 0x99, 0x99);
                wc.NextPrevStyle.VerticalAlign = VerticalAlign.Bottom;
                wc.DayHeaderStyle.Font.Bold = true;
                wc.DayHeaderStyle.Font.Size = FontUnit.Point(7);
                wc.DayHeaderStyle.BackColor = Color.FromArgb(0xcc, 0xcc, 0xcc);
                wc.SelectorStyle.BackColor = Color.FromArgb(0xcc, 0xcc, 0xcc);
                wc.TodayDayStyle.BackColor = Color.FromArgb(0xcc, 0xcc, 0xcc);
                wc.TodayDayStyle.ForeColor = Color.Black;
                wc.SelectedDayStyle.BackColor = Color.FromArgb(0x66, 0x66, 0x66);
                wc.SelectedDayStyle.ForeColor = Color.White;
                wc.SelectedDayStyle.Font.Bold = true;
                wc.OtherMonthDayStyle.ForeColor = Color.FromArgb(0x80, 0x80, 0x80);
                wc.WeekendDayStyle.BackColor = Color.FromArgb(0xff, 0xff, 0xcc);
            }

            public override string GetDescription()
            {
                return System.Design.SR.GetString("CalAFmt_Scheme_Simple");
            }
        }
    }
}

