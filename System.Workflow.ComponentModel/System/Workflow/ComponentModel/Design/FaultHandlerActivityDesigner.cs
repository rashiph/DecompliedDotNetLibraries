namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Workflow.ComponentModel;

    [ActivityDesignerTheme(typeof(FaultHandlerActivityDesignerTheme))]
    internal sealed class FaultHandlerActivityDesigner : SequentialActivityDesigner
    {
        public override bool CanBeParentedTo(CompositeActivityDesigner parentActivityDesigner)
        {
            if (parentActivityDesigner == null)
            {
                throw new ArgumentNullException("parentActivityDesigner");
            }
            return ((parentActivityDesigner.Activity is FaultHandlersActivity) && base.CanBeParentedTo(parentActivityDesigner));
        }

        protected override void OnActivityChanged(ActivityChangedEventArgs e)
        {
            base.OnActivityChanged(e);
            if ((e.Member != null) && string.Equals(e.Member.Name, "FaultType", StringComparison.Ordinal))
            {
                TypeDescriptor.Refresh(e.Activity);
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            if (properties["InitializeField"] == null)
            {
                properties["InitializeField"] = TypeDescriptor.CreateProperty(base.GetType(), "InitializeField", typeof(bool), new Attribute[] { DesignerSerializationVisibilityAttribute.Hidden, BrowsableAttribute.No });
            }
        }

        public override bool CanExpandCollapse
        {
            get
            {
                return false;
            }
        }

        private bool InitializeField
        {
            get
            {
                return false;
            }
        }

        public override ReadOnlyCollection<DesignerView> Views
        {
            get
            {
                List<DesignerView> list = new List<DesignerView>();
                foreach (DesignerView view in base.Views)
                {
                    if (((view.ViewId != 2) && (view.ViewId != 3)) && (view.ViewId != 4))
                    {
                        list.Add(view);
                    }
                }
                return new ReadOnlyCollection<DesignerView>(list);
            }
        }
    }
}

