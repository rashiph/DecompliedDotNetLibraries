namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Threading;

    public class ApplicationContext : IDisposable
    {
        private Form mainForm;
        private object userData;

        public event EventHandler ThreadExit;

        public ApplicationContext() : this(null)
        {
        }

        public ApplicationContext(Form mainForm)
        {
            this.MainForm = mainForm;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && (this.mainForm != null))
            {
                if (!this.mainForm.IsDisposed)
                {
                    this.mainForm.Dispose();
                }
                this.mainForm = null;
            }
        }

        public void ExitThread()
        {
            this.ExitThreadCore();
        }

        protected virtual void ExitThreadCore()
        {
            if (this.ThreadExit != null)
            {
                this.ThreadExit(this, EventArgs.Empty);
            }
        }

        ~ApplicationContext()
        {
            this.Dispose(false);
        }

        protected virtual void OnMainFormClosed(object sender, EventArgs e)
        {
            this.ExitThreadCore();
        }

        private void OnMainFormDestroy(object sender, EventArgs e)
        {
            Form form = (Form) sender;
            if (!form.RecreatingHandle)
            {
                form.HandleDestroyed -= new EventHandler(this.OnMainFormDestroy);
                this.OnMainFormClosed(sender, e);
            }
        }

        public Form MainForm
        {
            get
            {
                return this.mainForm;
            }
            set
            {
                EventHandler handler = new EventHandler(this.OnMainFormDestroy);
                if (this.mainForm != null)
                {
                    this.mainForm.HandleDestroyed -= handler;
                }
                this.mainForm = value;
                if (this.mainForm != null)
                {
                    this.mainForm.HandleDestroyed += handler;
                }
            }
        }

        [TypeConverter(typeof(StringConverter)), DefaultValue((string) null), System.Windows.Forms.SRDescription("ControlTagDescr"), Bindable(true), Localizable(false), System.Windows.Forms.SRCategory("CatData")]
        public object Tag
        {
            get
            {
                return this.userData;
            }
            set
            {
                this.userData = value;
            }
        }
    }
}

