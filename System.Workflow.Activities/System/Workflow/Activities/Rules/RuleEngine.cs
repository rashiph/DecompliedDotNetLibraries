namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime;
    using System.Workflow.Activities;
    using System.Workflow.ComponentModel;

    public class RuleEngine
    {
        private IList<RuleState> analyzedRules;
        private string name;
        private RuleValidation validation;

        public RuleEngine(RuleSet ruleSet, Type objectType) : this(ruleSet, new RuleValidation(objectType, null), null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleEngine(RuleSet ruleSet, RuleValidation validation) : this(ruleSet, validation, null)
        {
        }

        internal RuleEngine(RuleSet ruleSet, RuleValidation validation, ActivityExecutionContext executionContext)
        {
            if (!ruleSet.Validate(validation))
            {
                throw new RuleSetValidationException(string.Format(CultureInfo.CurrentCulture, Messages.RuleSetValidationFailed, new object[] { ruleSet.name }), validation.Errors);
            }
            this.name = ruleSet.Name;
            this.validation = validation;
            Tracer tracer = null;
            if (WorkflowActivityTrace.Rules.Switch.ShouldTrace(TraceEventType.Information))
            {
                tracer = new Tracer(ruleSet.Name, executionContext);
            }
            this.analyzedRules = Executor.Preprocess(ruleSet.ChainingBehavior, ruleSet.Rules, validation, tracer);
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters", MessageId="0#")]
        public void Execute(object thisObject)
        {
            this.Execute(new RuleExecution(this.validation, thisObject, null));
        }

        internal void Execute(RuleExecution ruleExecution)
        {
            Tracer tracer = null;
            if (WorkflowActivityTrace.Rules.Switch.ShouldTrace(TraceEventType.Information))
            {
                tracer = new Tracer(this.name, ruleExecution.ActivityExecutionContext);
                tracer.StartRuleSet();
            }
            Executor.ExecuteRuleSet(this.analyzedRules, ruleExecution, tracer, "RuleSet." + this.name);
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters", MessageId="0#")]
        public void Execute(object thisObject, ActivityExecutionContext executionContext)
        {
            this.Execute(new RuleExecution(this.validation, thisObject, executionContext));
        }
    }
}

