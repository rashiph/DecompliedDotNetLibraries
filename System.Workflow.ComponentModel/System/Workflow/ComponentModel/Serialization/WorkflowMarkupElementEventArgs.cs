namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Runtime;
    using System.Xml;

    internal sealed class WorkflowMarkupElementEventArgs : EventArgs
    {
        private System.Xml.XmlReader reader;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal WorkflowMarkupElementEventArgs(System.Xml.XmlReader reader)
        {
            this.reader = reader;
        }

        public System.Xml.XmlReader XmlReader
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.reader;
            }
        }
    }
}

