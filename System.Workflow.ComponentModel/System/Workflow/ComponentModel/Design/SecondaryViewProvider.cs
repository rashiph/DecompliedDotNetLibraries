namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Workflow.ComponentModel;

    internal static class SecondaryViewProvider
    {
        private const string EventHandlersRef = "System.Workflow.Activities.EventHandlersActivity, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
        private const string EventHandlingScopeRef = "System.Workflow.Activities.EventHandlingScopeActivity, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

        internal static Activity FindActivity(StructuredCompositeActivityDesigner designer, Type activityType)
        {
            CompositeActivity activity = designer.Activity as CompositeActivity;
            if ((activityType != null) && (activity != null))
            {
                foreach (Activity activity2 in activity.Activities)
                {
                    if (activityType.IsAssignableFrom(activity2.GetType()))
                    {
                        return activity2;
                    }
                }
            }
            return null;
        }

        internal static IList<Type> GetActivityTypes(StructuredCompositeActivityDesigner designer)
        {
            List<Type> list = new List<Type>();
            ReadOnlyCollection<DesignerView> views = designer.Views;
            for (int i = 1; i < views.Count; i++)
            {
                Type item = views[i].UserData[SecondaryView.UserDataKey_ActivityType] as Type;
                list.Add(item);
            }
            return list.AsReadOnly();
        }

        internal static ReadOnlyCollection<DesignerView> GetViews(StructuredCompositeActivityDesigner designer)
        {
            if (designer.Activity == null)
            {
                throw new ArgumentException("Component can not be null!");
            }
            bool flag = !designer.IsEditable;
            List<object[]> list = new List<object[]>();
            string toolboxDisplayName = ActivityToolboxItem.GetToolboxDisplayName(designer.Activity.GetType());
            list.Add(new object[] { designer.Activity.GetType(), DR.GetString("ViewActivity", new object[] { toolboxDisplayName }) });
            if (designer.Activity.Site != null)
            {
                WorkflowDesignerLoader service = designer.Activity.Site.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
                Type c = designer.Activity.GetType();
                if ((service == null) || (typeof(CompositeActivity).IsAssignableFrom(c) && (!flag || (FindActivity(designer, typeof(CancellationHandlerActivity)) != null))))
                {
                    list.Add(new object[] { typeof(CancellationHandlerActivity), DR.GetString("ViewCancelHandler", new object[0]) });
                }
                if ((service == null) || (typeof(CompositeActivity).IsAssignableFrom(c) && (!flag || (FindActivity(designer, typeof(FaultHandlersActivity)) != null))))
                {
                    list.Add(new object[] { typeof(FaultHandlersActivity), DR.GetString("ViewExceptions", new object[0]) });
                }
                if ((service == null) || (((designer.Activity is ICompensatableActivity) && typeof(CompositeActivity).IsAssignableFrom(c)) && (!flag || (FindActivity(designer, typeof(CompensationHandlerActivity)) != null))))
                {
                    list.Add(new object[] { typeof(CompensationHandlerActivity), DR.GetString("ViewCompensation", new object[0]) });
                }
                if ((service == null) || (Type.GetType("System.Workflow.Activities.EventHandlingScopeActivity, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35").IsAssignableFrom(c) && (!flag || (FindActivity(designer, Type.GetType("System.Workflow.Activities.EventHandlersActivity, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")) != null))))
                {
                    list.Add(new object[] { Type.GetType("System.Workflow.Activities.EventHandlersActivity, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"), DR.GetString("ViewEvents", new object[0]) });
                }
            }
            List<DesignerView> list2 = new List<DesignerView>();
            for (int i = 0; i < list.Count; i++)
            {
                Type activityType = list[i][0] as Type;
                DesignerView item = new SecondaryView(designer, i + 1, list[i][1] as string, activityType);
                list2.Add(item);
            }
            return list2.AsReadOnly();
        }

        internal static void OnViewRemoved(StructuredCompositeActivityDesigner designer, Type viewTypeRemoved)
        {
            ReadOnlyCollection<DesignerView> views = designer.Views;
            for (int i = 1; i < views.Count; i++)
            {
                Type type = views[i].UserData[SecondaryView.UserDataKey_ActivityType] as Type;
                if (viewTypeRemoved == type)
                {
                    views[i].UserData[SecondaryView.UserDataKey_Designer] = null;
                }
            }
        }
    }
}

