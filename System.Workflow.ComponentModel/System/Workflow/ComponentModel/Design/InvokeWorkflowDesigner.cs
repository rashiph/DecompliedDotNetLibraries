namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    [ActivityDesignerTheme(typeof(InvokeWorkflowDesignerTheme))]
    internal sealed class InvokeWorkflowDesigner : ActivityHostDesigner
    {
        internal const string InvokeWorkflowRef = "System.Workflow.Activities.InvokeWorkflowActivity, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
        private static readonly ArrayList ReservedParameterNames = new ArrayList(new string[] { "Name", "Enabled", "Description", "TargetWorkflow", "Invoking", "ParameterBindings" });
        private Type targetWorkflowType;

        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);
            this.HelpText = DR.GetString("SpecifyTargetWorkflow", new object[0]);
            this.RefreshTargetWorkflowType();
        }

        protected override void OnActivityChanged(ActivityChangedEventArgs e)
        {
            base.OnActivityChanged(e);
            if ((e.Member != null) && string.Equals(e.Member.Name, "TargetWorkflow", StringComparison.Ordinal))
            {
                if ((e.OldValue != e.NewValue) && (base.Activity != null))
                {
                    PropertyInfo property = base.Activity.GetType().GetProperty("ParameterBindings", BindingFlags.Public | BindingFlags.Instance);
                    if (property != null)
                    {
                        WorkflowParameterBindingCollection bindings = property.GetValue(base.Activity, null) as WorkflowParameterBindingCollection;
                        if (bindings != null)
                        {
                            bindings.Clear();
                        }
                    }
                }
                this.RefreshTargetWorkflowType();
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            if (this.targetWorkflowType != null)
            {
                try
                {
                    foreach (PropertyInfo info in this.targetWorkflowType.GetProperties())
                    {
                        if (((!info.CanWrite || (info.DeclaringType == typeof(DependencyObject))) || ((info.DeclaringType == typeof(Activity)) || (info.DeclaringType == typeof(CompositeActivity)))) || (((info.DeclaringType == Type.GetType("System.Workflow.Activities.SequentialWorkflowActivity, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")) || (info.DeclaringType == Type.GetType("System.Workflow.Activities.StateMachineWorkflowActivity, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"))) && string.Equals(info.Name, "DynamicUpdateCondition", StringComparison.Ordinal)))
                        {
                            continue;
                        }
                        bool flag = false;
                        Type targetWorkflowType = this.targetWorkflowType;
                        while ((targetWorkflowType != null) && (targetWorkflowType is DesignTimeType))
                        {
                            targetWorkflowType = targetWorkflowType.BaseType;
                        }
                        if (targetWorkflowType != null)
                        {
                            foreach (DependencyProperty property in DependencyProperty.FromType(targetWorkflowType))
                            {
                                if ((property.Name == info.Name) && property.DefaultMetadata.IsMetaProperty)
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }
                        if (!flag)
                        {
                            PropertyDescriptor descriptor = new ParameterInfoBasedPropertyDescriptor(Type.GetType("System.Workflow.Activities.InvokeWorkflowActivity, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"), info.Name, info.PropertyType, ReservedParameterNames.Contains(info.Name), new Attribute[] { DesignOnlyAttribute.Yes });
                            properties[descriptor.Name] = descriptor;
                        }
                    }
                }
                catch (MissingMemberException)
                {
                }
            }
        }

        internal void RefreshTargetWorkflowType()
        {
            if (base.Activity != null)
            {
                ITypeFilterProvider activity = base.Activity as ITypeFilterProvider;
                Type type = base.Activity.GetType().InvokeMember("TargetWorkflow", BindingFlags.ExactBinding | BindingFlags.GetProperty | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, base.Activity, new object[0], CultureInfo.InvariantCulture) as Type;
                if ((type != null) && activity.CanFilterType(type, false))
                {
                    ITypeProvider service = (ITypeProvider) base.GetService(typeof(ITypeProvider));
                    if (service != null)
                    {
                        Type type2 = null;
                        if ((type.Assembly == null) && (service.LocalAssembly != null))
                        {
                            type2 = service.LocalAssembly.GetType(type.FullName);
                        }
                        else
                        {
                            type2 = service.GetType(type.FullName);
                        }
                        if (type2 != null)
                        {
                            type = type2;
                        }
                    }
                }
                else
                {
                    type = null;
                }
                if (this.targetWorkflowType != type)
                {
                    this.targetWorkflowType = type;
                    base.RefreshHostedActivity();
                    if (this.targetWorkflowType is DesignTimeType)
                    {
                        this.HelpText = DR.GetString("BuildTargetWorkflow", new object[0]);
                    }
                    else
                    {
                        this.HelpText = DR.GetString("SpecifyTargetWorkflow", new object[0]);
                    }
                }
                TypeDescriptor.Refresh(base.Activity);
            }
        }

        protected override Activity RootActivity
        {
            get
            {
                if ((this.targetWorkflowType != null) && !(this.targetWorkflowType is DesignTimeType))
                {
                    return (Activator.CreateInstance(this.targetWorkflowType) as Activity);
                }
                return null;
            }
        }
    }
}

