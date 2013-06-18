namespace System.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    public class DateTimeEditor : UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
                if (edSvc == null)
                {
                    return value;
                }
                using (DateTimeUI eui = new DateTimeUI())
                {
                    eui.Start(edSvc, value);
                    edSvc.DropDownControl(eui);
                    value = eui.Value;
                    eui.End();
                }
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        private class DateTimeUI : Control
        {
            private IWindowsFormsEditorService edSvc;
            private MonthCalendar monthCalendar = new DateTimeMonthCalendar();
            private object value;

            public DateTimeUI()
            {
                this.InitializeComponent();
                base.Size = this.monthCalendar.SingleMonthSize;
                this.monthCalendar.Resize += new EventHandler(this.MonthCalResize);
            }

            public void End()
            {
                this.edSvc = null;
                this.value = null;
            }

            private void InitializeComponent()
            {
                this.monthCalendar.DateSelected += new DateRangeEventHandler(this.OnDateSelected);
                this.monthCalendar.KeyDown += new KeyEventHandler(this.MonthCalKeyDown);
                base.Controls.Add(this.monthCalendar);
            }

            private void MonthCalKeyDown(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    this.OnDateSelected(sender, null);
                }
            }

            private void MonthCalResize(object sender, EventArgs e)
            {
                base.Size = this.monthCalendar.Size;
            }

            private void OnDateSelected(object sender, DateRangeEventArgs e)
            {
                this.value = this.monthCalendar.SelectionStart;
                this.edSvc.CloseDropDown();
            }

            protected override void OnGotFocus(EventArgs e)
            {
                base.OnGotFocus(e);
                this.monthCalendar.Focus();
            }

            public void Start(IWindowsFormsEditorService edSvc, object value)
            {
                this.edSvc = edSvc;
                this.value = value;
                if (value != null)
                {
                    DateTime time = (DateTime) value;
                    this.monthCalendar.SetDate(time.Equals(DateTime.MinValue) ? DateTime.Today : time);
                }
            }

            public object Value
            {
                get
                {
                    return this.value;
                }
            }

            private class DateTimeMonthCalendar : MonthCalendar
            {
                protected override bool IsInputKey(Keys keyData)
                {
                    Keys keys = keyData;
                    return ((keys == Keys.Enter) || base.IsInputKey(keyData));
                }
            }
        }
    }
}

