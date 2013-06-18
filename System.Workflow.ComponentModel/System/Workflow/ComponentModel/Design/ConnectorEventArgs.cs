namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Runtime;

    public class ConnectorEventArgs : EventArgs
    {
        private System.Workflow.ComponentModel.Design.Connector connector;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ConnectorEventArgs(System.Workflow.ComponentModel.Design.Connector connector)
        {
            this.connector = connector;
        }

        public System.Workflow.ComponentModel.Design.Connector Connector
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.connector;
            }
        }
    }
}

