namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;
    using System.Workflow.Runtime.DebugEngine;

    [SRDisplayName("CodeConditionDisplayName"), ToolboxItem(false), ActivityValidator(typeof(CodeCondition.CodeConditionValidator))]
    public class CodeCondition : ActivityCondition
    {
        public static readonly DependencyProperty ConditionEvent = DependencyProperty.Register("Condition", typeof(EventHandler<ConditionalEventArgs>), typeof(CodeCondition));

        [MergableProperty(false), SRDescription("ExpressionDescr"), SRCategory("Handlers")]
        public event EventHandler<ConditionalEventArgs> Condition
        {
            add
            {
                base.AddHandler(ConditionEvent, value);
            }
            remove
            {
                base.RemoveHandler(ConditionEvent, value);
            }
        }

        public override bool Evaluate(Activity ownerActivity, IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            ConditionalEventArgs e = new ConditionalEventArgs();
            EventHandler<ConditionalEventArgs>[] invocationList = base.GetInvocationList<EventHandler<ConditionalEventArgs>>(ConditionEvent);
            IWorkflowDebuggerService service = provider.GetService(typeof(IWorkflowDebuggerService)) as IWorkflowDebuggerService;
            if (invocationList != null)
            {
                foreach (EventHandler<ConditionalEventArgs> handler in invocationList)
                {
                    if (service != null)
                    {
                        service.NotifyHandlerInvoking(handler);
                    }
                    handler(ownerActivity, e);
                    if (service != null)
                    {
                        service.NotifyHandlerInvoked();
                    }
                }
            }
            return e.Result;
        }

        protected override object GetBoundValue(ActivityBind bind, Type targetType)
        {
            if (bind == null)
            {
                throw new ArgumentNullException("bind");
            }
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }
            object runtimeValue = bind;
            Activity parentDependencyObject = base.ParentDependencyObject as Activity;
            if (parentDependencyObject != null)
            {
                runtimeValue = bind.GetRuntimeValue(parentDependencyObject, targetType);
            }
            return runtimeValue;
        }

        private class CodeConditionValidator : ConditionValidator
        {
            public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
            {
                ValidationErrorCollection errors = new ValidationErrorCollection();
                errors.AddRange(base.Validate(manager, obj));
                CodeCondition condition = obj as CodeCondition;
                if (((condition != null) && (condition.GetInvocationList<EventHandler<ConditionalEventArgs>>(CodeCondition.ConditionEvent).Length == 0)) && (condition.GetBinding(CodeCondition.ConditionEvent) == null))
                {
                    Hashtable hashtable = condition.GetValue(WorkflowMarkupSerializer.EventsProperty) as Hashtable;
                    if ((hashtable != null) && (hashtable["Condition"] != null))
                    {
                        return errors;
                    }
                    errors.Add(ValidationError.GetNotSetValidationError(base.GetFullPropertyName(manager) + ".Condition"));
                }
                return errors;
            }
        }
    }
}

