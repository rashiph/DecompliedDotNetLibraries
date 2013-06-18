namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Workflow.ComponentModel;

    internal sealed class TransactionScopeActivityDesigner : SequenceDesigner
    {
        public override ReadOnlyCollection<DesignerView> Views
        {
            get
            {
                List<DesignerView> list = new List<DesignerView>();
                foreach (DesignerView view in base.Views)
                {
                    Type c = view.UserData[SecondaryView.UserDataKey_ActivityType] as Type;
                    if (((c != null) && !typeof(CancellationHandlerActivity).IsAssignableFrom(c)) && !typeof(FaultHandlersActivity).IsAssignableFrom(c))
                    {
                        list.Add(view);
                    }
                }
                return new ReadOnlyCollection<DesignerView>(list);
            }
        }
    }
}

