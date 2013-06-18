namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class FlowPanelDesigner : PanelDesigner
    {
        internal override void AddChildControl(Control newChild)
        {
            this.Control.Controls.Add(newChild);
        }

        protected override void OnDragDrop(DragEventArgs de)
        {
            base.OnDragDrop(de);
            SelectionManager service = this.GetService(typeof(SelectionManager)) as SelectionManager;
            if (service != null)
            {
                service.Refresh();
            }
        }

        public override bool ParticipatesWithSnapLines
        {
            get
            {
                return false;
            }
        }

        public override IList SnapLines
        {
            get
            {
                ArrayList snapLines = (ArrayList) base.SnapLines;
                ArrayList list2 = new ArrayList(4);
                foreach (SnapLine line in snapLines)
                {
                    if ((line.Filter != null) && line.Filter.Contains("Padding"))
                    {
                        list2.Add(line);
                    }
                }
                foreach (SnapLine line2 in list2)
                {
                    snapLines.Remove(line2);
                }
                return snapLines;
            }
        }
    }
}

