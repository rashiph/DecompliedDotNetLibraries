namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Runtime;
    using System.Windows.Forms;

    public class ActivityDragEventArgs : DragEventArgs
    {
        private List<Activity> draggedActivities;
        private Point dragInitiationPoint;
        private Point snapPoint;

        internal ActivityDragEventArgs(DragEventArgs dragEventArgs, Point dragInitiationPoint, Point point, List<Activity> draggedActivities) : base(dragEventArgs.Data, dragEventArgs.KeyState, point.X, point.Y, dragEventArgs.AllowedEffect, dragEventArgs.Effect)
        {
            this.snapPoint = Point.Empty;
            this.dragInitiationPoint = Point.Empty;
            this.dragInitiationPoint = dragInitiationPoint;
            if (draggedActivities == null)
            {
                this.draggedActivities = new List<Activity>();
            }
            else
            {
                this.draggedActivities = new List<Activity>(draggedActivities);
            }
        }

        public ReadOnlyCollection<Activity> Activities
        {
            get
            {
                return this.draggedActivities.AsReadOnly();
            }
        }

        public Point DragImageSnapPoint
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.snapPoint;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.snapPoint = value;
            }
        }

        public Point DragInitiationPoint
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.dragInitiationPoint;
            }
        }
    }
}

