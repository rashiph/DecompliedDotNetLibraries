namespace System.Workflow.Activities.Common
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime;
    using System.Threading;
    using System.Workflow.ComponentModel;

    internal sealed class Walker
    {
        private bool useEnabledActivities;

        internal event System.Workflow.Activities.Common.WalkerEventHandler FoundActivity;

        internal event System.Workflow.Activities.Common.WalkerEventHandler FoundProperty;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Walker() : this(false)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Walker(bool useEnabledActivities)
        {
            this.useEnabledActivities = useEnabledActivities;
        }

        private static DesignerSerializationVisibility GetSerializationVisibility(PropertyInfo prop)
        {
            if ((prop.DeclaringType == typeof(CompositeActivity)) && string.Equals(prop.Name, "Activities", StringComparison.Ordinal))
            {
                return DesignerSerializationVisibility.Hidden;
            }
            DesignerSerializationVisibility visible = DesignerSerializationVisibility.Visible;
            DesignerSerializationVisibilityAttribute[] customAttributes = (DesignerSerializationVisibilityAttribute[]) prop.GetCustomAttributes(typeof(DesignerSerializationVisibilityAttribute), true);
            if (customAttributes.Length > 0)
            {
                visible = customAttributes[0].Visibility;
            }
            return visible;
        }

        private static bool IsBrowsableType(Type type)
        {
            bool browsable = false;
            BrowsableAttribute[] customAttributes = (BrowsableAttribute[]) type.GetCustomAttributes(typeof(BrowsableAttribute), true);
            if (customAttributes.Length > 0)
            {
                browsable = customAttributes[0].Browsable;
            }
            return browsable;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void Walk(Activity seedActivity)
        {
            this.Walk(seedActivity, true);
        }

        public void Walk(Activity seedActivity, bool walkChildren)
        {
            Queue queue = new Queue();
            queue.Enqueue(seedActivity);
            while (queue.Count > 0)
            {
                Activity currentActivity = queue.Dequeue() as Activity;
                if (this.FoundActivity != null)
                {
                    System.Workflow.Activities.Common.WalkerEventArgs eventArgs = new System.Workflow.Activities.Common.WalkerEventArgs(currentActivity);
                    this.FoundActivity(this, eventArgs);
                    if (eventArgs.Action == System.Workflow.Activities.Common.WalkerAction.Abort)
                    {
                        return;
                    }
                    if (eventArgs.Action == System.Workflow.Activities.Common.WalkerAction.Skip)
                    {
                        continue;
                    }
                }
                if ((this.FoundProperty != null) && !this.WalkProperties(currentActivity))
                {
                    return;
                }
                if (walkChildren && (currentActivity is CompositeActivity))
                {
                    if (this.useEnabledActivities)
                    {
                        foreach (Activity activity2 in Helpers.GetAllEnabledActivities((CompositeActivity) currentActivity))
                        {
                            queue.Enqueue(activity2);
                        }
                    }
                    else
                    {
                        foreach (Activity activity3 in ((CompositeActivity) currentActivity).Activities)
                        {
                            queue.Enqueue(activity3);
                        }
                    }
                }
            }
        }

        private bool WalkProperties(Activity seedActivity)
        {
            return this.WalkProperties(seedActivity, seedActivity);
        }

        public bool WalkProperties(Activity activity, object obj)
        {
            Activity activity2 = obj as Activity;
            foreach (PropertyInfo info in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (((info.GetIndexParameters() == null) || (info.GetIndexParameters().Length <= 0)) && (GetSerializationVisibility(info) != DesignerSerializationVisibility.Hidden))
                {
                    object currentValue = null;
                    DependencyProperty dependencyProperty = DependencyProperty.FromName(info.Name, obj.GetType());
                    if ((dependencyProperty != null) && (activity2 != null))
                    {
                        if (activity2.IsBindingSet(dependencyProperty))
                        {
                            currentValue = activity2.GetBinding(dependencyProperty);
                        }
                        else
                        {
                            currentValue = activity2.GetValue(dependencyProperty);
                        }
                    }
                    else
                    {
                        try
                        {
                            currentValue = info.CanRead ? info.GetValue(obj, null) : null;
                        }
                        catch
                        {
                        }
                    }
                    if (this.FoundProperty != null)
                    {
                        System.Workflow.Activities.Common.WalkerEventArgs eventArgs = new System.Workflow.Activities.Common.WalkerEventArgs(activity, currentValue, info, obj);
                        this.FoundProperty(this, eventArgs);
                        if (eventArgs.Action == System.Workflow.Activities.Common.WalkerAction.Skip)
                        {
                            continue;
                        }
                        if (eventArgs.Action == System.Workflow.Activities.Common.WalkerAction.Abort)
                        {
                            return false;
                        }
                    }
                    if (currentValue is IList)
                    {
                        foreach (object obj3 in (IList) currentValue)
                        {
                            if (this.FoundProperty != null)
                            {
                                System.Workflow.Activities.Common.WalkerEventArgs args2 = new System.Workflow.Activities.Common.WalkerEventArgs(activity, obj3, null, currentValue);
                                this.FoundProperty(this, args2);
                                if (args2.Action == System.Workflow.Activities.Common.WalkerAction.Skip)
                                {
                                    continue;
                                }
                                if (args2.Action == System.Workflow.Activities.Common.WalkerAction.Abort)
                                {
                                    return false;
                                }
                            }
                            if (((obj3 != null) && IsBrowsableType(obj3.GetType())) && !this.WalkProperties(activity, obj3))
                            {
                                return false;
                            }
                        }
                    }
                    else if (((currentValue != null) && IsBrowsableType(currentValue.GetType())) && !this.WalkProperties(activity, currentValue))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}

