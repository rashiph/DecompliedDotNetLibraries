namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Runtime;

    internal sealed class ConnectionPointHitTestInfo : HitTestInfo
    {
        private System.Workflow.ComponentModel.Design.ConnectionPoint connectionPoint;

        internal ConnectionPointHitTestInfo(System.Workflow.ComponentModel.Design.ConnectionPoint connectionPoint) : base(connectionPoint.AssociatedDesigner, HitTestLocations.Connector | HitTestLocations.Designer)
        {
            this.connectionPoint = connectionPoint;
        }

        internal System.Workflow.ComponentModel.Design.ConnectionPoint ConnectionPoint
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.connectionPoint;
            }
        }
    }
}

