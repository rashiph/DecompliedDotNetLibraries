namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), HostProtection(SecurityAction.LinkDemand, SharedState=true), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class MenuCommand
    {
        private const int CHECKED = 4;
        private System.ComponentModel.Design.CommandID commandID;
        private const int ENABLED = 2;
        private EventHandler execHandler;
        private const int INVISIBLE = 0x10;
        private IDictionary properties;
        private int status;
        private const int SUPPORTED = 1;

        public event EventHandler CommandChanged;

        public MenuCommand(EventHandler handler, System.ComponentModel.Design.CommandID command)
        {
            this.execHandler = handler;
            this.commandID = command;
            this.status = 3;
        }

        public virtual void Invoke()
        {
            if (this.execHandler != null)
            {
                try
                {
                    this.execHandler(this, EventArgs.Empty);
                }
                catch (CheckoutException exception)
                {
                    if (exception != CheckoutException.Canceled)
                    {
                        throw;
                    }
                }
            }
        }

        public virtual void Invoke(object arg)
        {
            this.Invoke();
        }

        protected virtual void OnCommandChanged(EventArgs e)
        {
            if (this.statusHandler != null)
            {
                this.statusHandler(this, e);
            }
        }

        private void SetStatus(int mask, bool value)
        {
            int status = this.status;
            if (value)
            {
                status |= mask;
            }
            else
            {
                status &= ~mask;
            }
            if (status != this.status)
            {
                this.status = status;
                this.OnCommandChanged(EventArgs.Empty);
            }
        }

        public override string ToString()
        {
            string str = this.CommandID.ToString() + " : ";
            if ((this.status & 1) != 0)
            {
                str = str + "Supported";
            }
            if ((this.status & 2) != 0)
            {
                str = str + "|Enabled";
            }
            if ((this.status & 0x10) == 0)
            {
                str = str + "|Visible";
            }
            if ((this.status & 4) != 0)
            {
                str = str + "|Checked";
            }
            return str;
        }

        public virtual bool Checked
        {
            get
            {
                return ((this.status & 4) != 0);
            }
            set
            {
                this.SetStatus(4, value);
            }
        }

        public virtual System.ComponentModel.Design.CommandID CommandID
        {
            get
            {
                return this.commandID;
            }
        }

        public virtual bool Enabled
        {
            get
            {
                return ((this.status & 2) != 0);
            }
            set
            {
                this.SetStatus(2, value);
            }
        }

        public virtual int OleStatus
        {
            get
            {
                return this.status;
            }
        }

        public virtual IDictionary Properties
        {
            get
            {
                if (this.properties == null)
                {
                    this.properties = new HybridDictionary();
                }
                return this.properties;
            }
        }

        public virtual bool Supported
        {
            get
            {
                return ((this.status & 1) != 0);
            }
            set
            {
                this.SetStatus(1, value);
            }
        }

        public virtual bool Visible
        {
            get
            {
                return ((this.status & 0x10) == 0);
            }
            set
            {
                this.SetStatus(0x10, !value);
            }
        }
    }
}

