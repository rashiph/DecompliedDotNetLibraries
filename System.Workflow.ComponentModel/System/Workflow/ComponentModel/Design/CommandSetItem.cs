namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.Runtime;

    internal sealed class CommandSetItem : MenuCommand
    {
        private bool immidiateStatusUpdate;
        private EventHandler statusHandler;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CommandSetItem(EventHandler statusHandler, EventHandler invokeHandler, CommandID id) : base(invokeHandler, id)
        {
            this.statusHandler = statusHandler;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CommandSetItem(EventHandler statusHandler, EventHandler invokeHandler, CommandID id, bool immidiateStatusUpdate) : this(statusHandler, invokeHandler, id)
        {
            this.immidiateStatusUpdate = immidiateStatusUpdate;
        }

        public CommandSetItem(EventHandler statusHandler, EventHandler invokeHandler, CommandID id, string text) : this(statusHandler, invokeHandler, id)
        {
            this.Properties["Text"] = text;
        }

        public void UpdateStatus()
        {
            if (this.statusHandler != null)
            {
                try
                {
                    this.statusHandler(this, EventArgs.Empty);
                }
                catch
                {
                }
            }
        }

        public override int OleStatus
        {
            get
            {
                if (this.immidiateStatusUpdate)
                {
                    this.UpdateStatus();
                }
                return base.OleStatus;
            }
        }
    }
}

