namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;

    public class ActivityDesignerAccessibleObject : AccessibleObject
    {
        private System.Workflow.ComponentModel.Design.ActivityDesigner activityDesigner;

        public ActivityDesignerAccessibleObject(System.Workflow.ComponentModel.Design.ActivityDesigner activityDesigner)
        {
            if (activityDesigner == null)
            {
                throw new ArgumentNullException("activityDesigner");
            }
            if (activityDesigner.Activity == null)
            {
                throw new ArgumentException(DR.GetString("DesignerNotInitialized", new object[0]), "activityDesigner");
            }
            this.activityDesigner = activityDesigner;
        }

        public override void DoDefaultAction()
        {
            ISelectionService service = this.GetService(typeof(ISelectionService)) as ISelectionService;
            if (service != null)
            {
                service.SetSelectedComponents(new object[] { this.activityDesigner.Activity }, SelectionTypes.Replace);
            }
            else
            {
                base.DoDefaultAction();
            }
        }

        protected object GetService(System.Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }
            if ((this.ActivityDesigner.Activity != null) && (this.ActivityDesigner.Activity.Site != null))
            {
                return this.ActivityDesigner.Activity.Site.GetService(serviceType);
            }
            return null;
        }

        public override AccessibleObject Navigate(AccessibleNavigation navdir)
        {
            if (navdir == AccessibleNavigation.FirstChild)
            {
                return this.GetChild(0);
            }
            if (navdir == AccessibleNavigation.LastChild)
            {
                return this.GetChild(this.GetChildCount() - 1);
            }
            CompositeActivityDesigner parentDesigner = this.activityDesigner.ParentDesigner;
            if (parentDesigner != null)
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
                System.Workflow.ComponentModel.Design.ActivityDesigner designer = System.Workflow.ComponentModel.Design.ActivityDesigner.GetDesigner(parentDesigner.GetNextSelectableObject(this.activityDesigner.Activity, down) as Activity);
                if (designer != null)
                {
                    return designer.AccessibilityObject;
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
                    service.SetSelectedComponents(new object[] { this.activityDesigner.Activity }, SelectionTypes.Replace);
                }
                else if ((flags & AccessibleSelection.AddSelection) > AccessibleSelection.None)
                {
                    service.SetSelectedComponents(new object[] { this.activityDesigner.Activity }, SelectionTypes.Add);
                }
                else if ((flags & AccessibleSelection.RemoveSelection) > AccessibleSelection.None)
                {
                    service.SetSelectedComponents(new object[] { this.activityDesigner.Activity }, SelectionTypes.Remove);
                }
            }
        }

        protected System.Workflow.ComponentModel.Design.ActivityDesigner ActivityDesigner
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.activityDesigner;
            }
        }

        public override Rectangle Bounds
        {
            get
            {
                return this.activityDesigner.InternalRectangleToScreen(this.activityDesigner.Bounds);
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
                return DR.GetString("ActivityDesignerAccessibleDescription", new object[] { this.activityDesigner.Activity.GetType().Name });
            }
        }

        public override string Help
        {
            get
            {
                return DR.GetString("ActivityDesignerAccessibleHelp", new object[] { this.activityDesigner.Activity.GetType().Name });
            }
        }

        public override string Name
        {
            get
            {
                Activity component = this.activityDesigner.Activity;
                if (component == null)
                {
                    return base.Name;
                }
                if (TypeDescriptor.GetProperties(component)["TypeName"] != null)
                {
                    return (TypeDescriptor.GetProperties(component)["TypeName"].GetValue(component) as string);
                }
                if (!string.IsNullOrEmpty(component.QualifiedName))
                {
                    return component.QualifiedName;
                }
                return component.GetType().FullName;
            }
            set
            {
            }
        }

        public override AccessibleObject Parent
        {
            get
            {
                CompositeActivityDesigner parentDesigner = this.activityDesigner.ParentDesigner;
                if (parentDesigner == null)
                {
                    return null;
                }
                return parentDesigner.AccessibilityObject;
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
                AccessibleStates states = this.activityDesigner.IsSelected ? AccessibleStates.Selected : AccessibleStates.Selectable;
                states |= AccessibleStates.MultiSelectable;
                states |= this.activityDesigner.IsPrimarySelection ? AccessibleStates.Focused : AccessibleStates.Focusable;
                if (this.activityDesigner.IsLocked)
                {
                    states |= AccessibleStates.ReadOnly;
                }
                else
                {
                    states |= AccessibleStates.Moveable;
                }
                if (!this.activityDesigner.IsVisible)
                {
                    states |= AccessibleStates.Invisible;
                }
                return states;
            }
        }
    }
}

