namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Workflow.Activities;
    using System.Workflow.ComponentModel;

    internal class Tracer
    {
        private static string traceCondition = Messages.Condition;
        private static string traceElse = Messages.Else;
        private string tracePrefix;
        private static string traceRuleActions = Messages.TraceRuleActions;
        private static string traceRuleActionSideEffect = Messages.TraceRuleActionSideEffect;
        private static string traceRuleConditionDependency = Messages.TraceRuleConditionDependency;
        private static string traceRuleEvaluate = Messages.TraceRuleEvaluate;
        private static string traceRuleHeader = Messages.TraceRuleHeader;
        private static string traceRuleIdentifier = Messages.TraceRuleIdentifier;
        private static string traceRuleResult = Messages.TraceRuleResult;
        private static string traceRuleSetEvaluate = Messages.TraceRuleSetEvaluate;
        private static string traceRuleTriggers = Messages.TraceRuleTriggers;
        private static string traceThen = Messages.Then;
        private static string traceUpdate = Messages.TraceUpdate;

        internal Tracer(string name, ActivityExecutionContext activityExecutionContext)
        {
            if (activityExecutionContext != null)
            {
                this.tracePrefix = string.Format(CultureInfo.CurrentCulture, traceRuleIdentifier, new object[] { name, activityExecutionContext.ContextGuid.ToString() });
            }
            else
            {
                this.tracePrefix = string.Format(CultureInfo.CurrentCulture, traceRuleHeader, new object[] { name });
            }
        }

        internal void RuleResult(string ruleName, bool result)
        {
            WorkflowActivityTrace.Rules.TraceEvent(TraceEventType.Information, 0, traceRuleResult, new object[] { this.tracePrefix, ruleName, result.ToString() });
        }

        internal void StartActions(string ruleName, bool result)
        {
            WorkflowActivityTrace.Rules.TraceEvent(TraceEventType.Verbose, 0, traceRuleActions, new object[] { this.tracePrefix, result ? traceThen : traceElse, ruleName });
        }

        internal void StartRule(string ruleName)
        {
            WorkflowActivityTrace.Rules.TraceEvent(TraceEventType.Verbose, 0, traceRuleEvaluate, new object[] { this.tracePrefix, ruleName });
        }

        internal void StartRuleSet()
        {
            WorkflowActivityTrace.Rules.TraceEvent(TraceEventType.Information, 0, traceRuleSetEvaluate, new object[] { this.tracePrefix });
        }

        internal void TraceConditionSymbols(string ruleName, ICollection<string> symbols)
        {
            this.TraceRuleSymbols(traceRuleConditionDependency, traceCondition, ruleName, symbols);
        }

        internal void TraceElseSymbols(string ruleName, ICollection<string> symbols)
        {
            this.TraceRuleSymbols(traceRuleActionSideEffect, traceElse, ruleName, symbols);
        }

        internal void TraceElseTriggers(string currentRuleName, ICollection<int> triggeredRules, List<RuleState> ruleStates)
        {
            this.TraceRuleTriggers(traceElse, currentRuleName, triggeredRules, ruleStates);
        }

        private void TraceRuleSymbols(string message, string clause, string ruleName, ICollection<string> symbols)
        {
            foreach (string str in symbols)
            {
                WorkflowActivityTrace.Rules.TraceEvent(TraceEventType.Verbose, 0, message, new object[] { this.tracePrefix, ruleName, clause, str });
            }
        }

        private void TraceRuleTriggers(string thenOrElse, string currentRuleName, ICollection<int> triggeredRules, List<RuleState> ruleStates)
        {
            foreach (int num in triggeredRules)
            {
                WorkflowActivityTrace.Rules.TraceEvent(TraceEventType.Verbose, 0, traceRuleTriggers, new object[] { this.tracePrefix, currentRuleName, thenOrElse, ruleStates[num].Rule.Name });
            }
        }

        internal void TraceThenSymbols(string ruleName, ICollection<string> symbols)
        {
            this.TraceRuleSymbols(traceRuleActionSideEffect, traceThen, ruleName, symbols);
        }

        internal void TraceThenTriggers(string currentRuleName, ICollection<int> triggeredRules, List<RuleState> ruleStates)
        {
            this.TraceRuleTriggers(traceThen, currentRuleName, triggeredRules, ruleStates);
        }

        internal void TraceUpdate(string ruleName, string otherName)
        {
            WorkflowActivityTrace.Rules.TraceEvent(TraceEventType.Verbose, 0, traceUpdate, new object[] { this.tracePrefix, ruleName, otherName });
        }
    }
}

