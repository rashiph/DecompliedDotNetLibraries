namespace System.Workflow.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Runtime;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;

    [SRDescription("FaultHandlerActivityDescription"), ToolboxItem(typeof(ActivityToolboxItem)), ToolboxBitmap(typeof(FaultHandlerActivity), "Resources.Exception.png"), ActivityValidator(typeof(FaultHandlerActivityValidator)), SRCategory("Standard"), Designer(typeof(FaultHandlerActivityDesigner), typeof(IDesigner))]
    public sealed class FaultHandlerActivity : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>, ITypeFilterProvider, IDynamicPropertyTypeProvider
    {
        internal static readonly DependencyProperty FaultProperty = DependencyProperty.Register("Fault", typeof(Exception), typeof(FaultHandlerActivity));
        public static readonly DependencyProperty FaultTypeProperty = DependencyProperty.Register("FaultType", typeof(Type), typeof(FaultHandlerActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata));

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public FaultHandlerActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public FaultHandlerActivity(string name) : base(name)
        {
        }

        protected internal override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            return SequenceHelper.Cancel(this, executionContext);
        }

        protected internal override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            return SequenceHelper.Execute(this, executionContext);
        }

        protected internal override void Initialize(IServiceProvider provider)
        {
            if (base.Parent == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_MustHaveParent"));
            }
            base.Initialize(provider);
        }

        protected internal override void OnActivityChangeRemove(ActivityExecutionContext executionContext, Activity removedActivity)
        {
            SequenceHelper.OnActivityChangeRemove(this, executionContext, removedActivity);
        }

        protected internal override void OnWorkflowChangesCompleted(ActivityExecutionContext executionContext)
        {
            SequenceHelper.OnWorkflowChangesCompleted(this, executionContext);
        }

        internal void SetException(Exception e)
        {
            base.SetValue(FaultProperty, e);
        }

        bool ITypeFilterProvider.CanFilterType(Type type, bool throwOnError)
        {
            bool flag = TypeProvider.IsAssignable(typeof(Exception), type);
            if (throwOnError && !flag)
            {
                throw new Exception(SR.GetString("Error_ExceptionTypeNotException", new object[] { type, "Type" }));
            }
            return flag;
        }

        void IActivityEventListener<ActivityExecutionStatusChangedEventArgs>.OnEvent(object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            SequenceHelper.OnEvent(this, sender, e);
        }

        AccessTypes IDynamicPropertyTypeProvider.GetAccessType(IServiceProvider serviceProvider, string propertyName)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }
            if (propertyName.Equals("Fault", StringComparison.Ordinal))
            {
                return AccessTypes.Write;
            }
            return AccessTypes.Read;
        }

        Type IDynamicPropertyTypeProvider.GetPropertyType(IServiceProvider serviceProvider, string propertyName)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }
            Type faultType = null;
            if (string.Equals(propertyName, "Fault", StringComparison.Ordinal))
            {
                faultType = this.FaultType;
                if (faultType == null)
                {
                    faultType = typeof(Exception);
                }
            }
            return faultType;
        }

        [SRDescription("FaultDescription"), ReadOnly(true), MergableProperty(false)]
        public Exception Fault
        {
            get
            {
                return (base.GetValue(FaultProperty) as Exception);
            }
        }

        [SRDescription("ExceptionTypeDescr"), MergableProperty(false), Editor(typeof(TypeBrowserEditor), typeof(UITypeEditor))]
        public Type FaultType
        {
            get
            {
                return (Type) base.GetValue(FaultTypeProperty);
            }
            set
            {
                base.SetValue(FaultTypeProperty, value);
            }
        }

        string ITypeFilterProvider.FilterDescription
        {
            get
            {
                return SR.GetString("FilterDescription_FaultHandlerActivity");
            }
        }
    }
}

