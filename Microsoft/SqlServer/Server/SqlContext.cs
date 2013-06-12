namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Data.SqlClient;
    using System.Data.SqlTypes;
    using System.Security.Principal;

    public sealed class SqlContext
    {
        private SqlPipe _pipe;
        private SmiContext _smiContext;
        private SqlTriggerContext _triggerContext;

        private SqlContext(SmiContext smiContext)
        {
            this._smiContext = smiContext;
            this._smiContext.OutOfScope += new EventHandler(this.OnOutOfScope);
        }

        private void OnOutOfScope(object s, EventArgs e)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.SqlContext.OutOfScope|ADV> SqlContext is out of scope\n");
            }
            if (this._pipe != null)
            {
                this._pipe.OnOutOfScope();
            }
            this._triggerContext = null;
        }

        private static SqlContext CurrentContext
        {
            get
            {
                SmiContext currentContext = SmiContextFactory.Instance.GetCurrentContext();
                SqlContext contextValue = (SqlContext) currentContext.GetContextValue(1);
                if (contextValue == null)
                {
                    contextValue = new SqlContext(currentContext);
                    currentContext.SetContextValue(1, contextValue);
                }
                return contextValue;
            }
        }

        private SqlPipe InstancePipe
        {
            get
            {
                if ((this._pipe == null) && this._smiContext.HasContextPipe)
                {
                    this._pipe = new SqlPipe(this._smiContext);
                }
                return this._pipe;
            }
        }

        private SqlTriggerContext InstanceTriggerContext
        {
            get
            {
                if (this._triggerContext == null)
                {
                    TriggerAction action;
                    SqlXml xml;
                    bool[] flagArray;
                    SmiEventSink_Default eventSink = new SmiEventSink_Default();
                    this._smiContext.GetTriggerInfo(eventSink, out flagArray, out action, out xml);
                    eventSink.ProcessMessagesAndThrow();
                    if (action != TriggerAction.Invalid)
                    {
                        this._triggerContext = new SqlTriggerContext(action, flagArray, xml);
                    }
                }
                return this._triggerContext;
            }
        }

        private System.Security.Principal.WindowsIdentity InstanceWindowsIdentity
        {
            get
            {
                return this._smiContext.WindowsIdentity;
            }
        }

        public static bool IsAvailable
        {
            get
            {
                return InOutOfProcHelper.InProc;
            }
        }

        public static SqlPipe Pipe
        {
            get
            {
                return CurrentContext.InstancePipe;
            }
        }

        public static SqlTriggerContext TriggerContext
        {
            get
            {
                return CurrentContext.InstanceTriggerContext;
            }
        }

        public static System.Security.Principal.WindowsIdentity WindowsIdentity
        {
            get
            {
                return CurrentContext.InstanceWindowsIdentity;
            }
        }
    }
}

