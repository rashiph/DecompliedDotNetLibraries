namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;

    public class SequenceDesignerAccessibleObject : CompositeDesignerAccessibleObject
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SequenceDesignerAccessibleObject(SequentialActivityDesigner activityDesigner) : base(activityDesigner)
        {
        }

        public override AccessibleObject GetChild(int index)
        {
            SequentialActivityDesigner activityDesigner = base.ActivityDesigner as SequentialActivityDesigner;
            if (activityDesigner.ActiveDesigner != activityDesigner)
            {
                return base.GetChild(index);
            }
            if (((index >= 0) && (index < this.GetChildCount())) && ((index % 2) == 0))
            {
                return new SequentialConnectorAccessibleObject(base.ActivityDesigner as SequentialActivityDesigner, index / 2);
            }
            return base.GetChild(index / 2);
        }

        public override int GetChildCount()
        {
            SequentialActivityDesigner activityDesigner = base.ActivityDesigner as SequentialActivityDesigner;
            if (activityDesigner.ActiveDesigner != activityDesigner)
            {
                return base.GetChildCount();
            }
            if (activityDesigner != null)
            {
                return ((activityDesigner.ContainedDesigners.Count + activityDesigner.ContainedDesigners.Count) + 1);
            }
            return -1;
        }

        public override AccessibleObject Navigate(AccessibleNavigation navdir)
        {
            if (((navdir == AccessibleNavigation.Up) || (navdir == AccessibleNavigation.Previous)) || ((navdir == AccessibleNavigation.Down) || (navdir == AccessibleNavigation.Next)))
            {
                DesignerNavigationDirection down = DesignerNavigationDirection.Down;
                if ((navdir == AccessibleNavigation.Up) || (navdir == AccessibleNavigation.Previous))
                {
                    down = DesignerNavigationDirection.Up;
                }
                else
                {
                    down = DesignerNavigationDirection.Down;
                }
                CompositeActivityDesigner parentDesigner = base.ActivityDesigner.ParentDesigner;
                if (parentDesigner != null)
                {
                    object nextSelectableObject = parentDesigner.GetNextSelectableObject(base.ActivityDesigner.Activity, down);
                    if (nextSelectableObject is ConnectorHitTestInfo)
                    {
                        return this.GetChild(((ConnectorHitTestInfo) nextSelectableObject).MapToIndex());
                    }
                }
            }
            return base.Navigate(navdir);
        }

        private sealed class SequentialConnectorAccessibleObject : AccessibleObject
        {
            private ConnectorHitTestInfo connectorHitInfo;

            internal SequentialConnectorAccessibleObject(SequentialActivityDesigner activityDesigner, int connectorIndex)
            {
                if (activityDesigner == null)
                {
                    throw new ArgumentNullException("activityDesigner");
                }
                this.connectorHitInfo = new ConnectorHitTestInfo(activityDesigner, HitTestLocations.Designer, connectorIndex);
            }

            public override void DoDefaultAction()
            {
                ISelectionService service = this.GetService(typeof(ISelectionService)) as ISelectionService;
                if (service != null)
                {
                    service.SetSelectedComponents(new object[] { this.connectorHitInfo.SelectableObject }, SelectionTypes.Replace);
                }
                else
                {
                    base.DoDefaultAction();
                }
            }

            private object GetService(System.Type serviceType)
            {
                if ((this.connectorHitInfo.AssociatedDesigner != null) && (this.connectorHitInfo.AssociatedDesigner.Activity.Site != null))
                {
                    return this.connectorHitInfo.AssociatedDesigner.Activity.Site.GetService(serviceType);
                }
                return null;
            }

            public override AccessibleObject Navigate(AccessibleNavigation navdir)
            {
                if ((navdir != AccessibleNavigation.FirstChild) && (navdir != AccessibleNavigation.LastChild))
                {
                    DesignerNavigationDirection down = DesignerNavigationDirection.Down;
                    if (navdir == AccessibleNavigation.Left)
                    {
                        down = DesignerNavigationDirection.Left;
                    }
                    else if (navdir == AccessibleNavigation.Right)
                    {
                        down = DesignerNavigationDirection.Right;
                    }
                    else if ((navdir == AccessibleNavigation.Up) || (navdir == AccessibleNavigation.Previous))
                    {
                        down = DesignerNavigationDirection.Up;
                    }
                    else if ((navdir == AccessibleNavigation.Down) || (navdir == AccessibleNavigation.Next))
                    {
                        down = DesignerNavigationDirection.Down;
                    }
                    object nextSelectableObject = ((CompositeActivityDesigner) this.connectorHitInfo.AssociatedDesigner).GetNextSelectableObject(this.connectorHitInfo, down);
                    if (nextSelectableObject is ConnectorHitTestInfo)
                    {
                        ConnectorHitTestInfo info = nextSelectableObject as ConnectorHitTestInfo;
                        return new SequenceDesignerAccessibleObject.SequentialConnectorAccessibleObject(info.AssociatedDesigner as SequentialActivityDesigner, info.MapToIndex());
                    }
                    if (nextSelectableObject is Activity)
                    {
                        ActivityDesigner designer = ActivityDesigner.GetDesigner(nextSelectableObject as Activity);
                        if (designer != null)
                        {
                            return designer.AccessibilityObject;
                        }
                    }
                }
                return base.Navigate(navdir);
            }

            public override void Select(AccessibleSelection flags)
            {
                ISelectionService service = this.GetService(typeof(ISelectionService)) as ISelectionService;
                if (service != null)
                {
                    if (((flags & AccessibleSelection.TakeFocus) > AccessibleSelection.None) || ((flags & AccessibleSelection.TakeSelection) > AccessibleSelection.None))
                    {
                        service.SetSelectedComponents(new object[] { this.connectorHitInfo.SelectableObject }, SelectionTypes.Replace);
                    }
                    else if ((flags & AccessibleSelection.AddSelection) > AccessibleSelection.None)
                    {
                        service.SetSelectedComponents(new object[] { this.connectorHitInfo.SelectableObject }, SelectionTypes.Add);
                    }
                    else if ((flags & AccessibleSelection.RemoveSelection) > AccessibleSelection.None)
                    {
                        service.SetSelectedComponents(new object[] { this.connectorHitInfo.SelectableObject }, SelectionTypes.Remove);
                    }
                }
            }

            public override Rectangle Bounds
            {
                get
                {
                    return ((SequentialActivityDesigner) this.connectorHitInfo.AssociatedDesigner).InternalRectangleToScreen(this.connectorHitInfo.Bounds);
                }
            }

            public override string DefaultAction
            {
                get
                {
                    return DR.GetString("AccessibleAction", new object[0]);
                }
            }

            public override string Description
            {
                get
                {
                    return DR.GetString("ConnectorAccessibleDescription", new object[] { this.connectorHitInfo.GetType().Name });
                }
            }

            public override string Help
            {
                get
                {
                    return DR.GetString("ConnectorAccessibleHelp", new object[] { this.connectorHitInfo.GetType().Name });
                }
            }

            public override string Name
            {
                get
                {
                    return DR.GetString("ConnectorDesc", new object[] { this.connectorHitInfo.MapToIndex().ToString(CultureInfo.InvariantCulture), this.Parent.Name });
                }
                set
                {
                }
            }

            public override AccessibleObject Parent
            {
                get
                {
                    return this.connectorHitInfo.AssociatedDesigner.AccessibilityObject;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.Diagram;
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    AccessibleStates multiSelectable = AccessibleStates.MultiSelectable;
                    if (this.connectorHitInfo.AssociatedDesigner.IsLocked)
                    {
                        multiSelectable |= AccessibleStates.ReadOnly;
                    }
                    if (!this.connectorHitInfo.AssociatedDesigner.IsVisible)
                    {
                        multiSelectable |= AccessibleStates.Invisible;
                    }
                    ISelectionService service = this.GetService(typeof(ISelectionService)) as ISelectionService;
                    if (service != null)
                    {
                        multiSelectable |= service.GetComponentSelected(this.connectorHitInfo.SelectableObject) ? AccessibleStates.Selected : AccessibleStates.Selectable;
                        multiSelectable |= (service.PrimarySelection == this.connectorHitInfo.SelectableObject) ? AccessibleStates.Focused : AccessibleStates.Focusable;
                    }
                    return multiSelectable;
                }
            }
        }
    }
}

