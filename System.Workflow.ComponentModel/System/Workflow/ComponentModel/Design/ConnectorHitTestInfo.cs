namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;

    public class ConnectorHitTestInfo : HitTestInfo
    {
        private int connector;

        public ConnectorHitTestInfo(CompositeActivityDesigner compositeActivityDesigner, HitTestLocations flags, int connector) : base(compositeActivityDesigner, flags)
        {
            if (this.connector < 0)
            {
                throw new ArgumentException(SR.GetString("Error_InvalidConnectorValue"), "connector");
            }
            this.connector = connector;
        }

        public override bool Equals(object obj)
        {
            ConnectorHitTestInfo info = obj as ConnectorHitTestInfo;
            return (((info != null) && (info.AssociatedDesigner == base.AssociatedDesigner)) && ((info.HitLocation == base.HitLocation) && (info.MapToIndex() == this.MapToIndex())));
        }

        public override int GetHashCode()
        {
            return ((base.GetHashCode() ^ ((base.AssociatedDesigner != null) ? base.AssociatedDesigner.GetHashCode() : 0)) ^ this.MapToIndex().GetHashCode());
        }

        public override int MapToIndex()
        {
            return this.connector;
        }

        public override Rectangle Bounds
        {
            get
            {
                SequentialActivityDesigner associatedDesigner = base.AssociatedDesigner as SequentialActivityDesigner;
                if ((associatedDesigner != null) && associatedDesigner.Expanded)
                {
                    Rectangle[] connectors = associatedDesigner.GetConnectors();
                    if (connectors.Length > 0)
                    {
                        return connectors[this.connector];
                    }
                }
                return Rectangle.Empty;
            }
        }

        public override object SelectableObject
        {
            get
            {
                return this;
            }
        }
    }
}

