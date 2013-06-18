namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    [Serializable]
    public class RuleSet
    {
        internal RuleChainingBehavior behavior;
        [NonSerialized]
        private RuleEngine cachedEngine;
        [NonSerialized]
        private RuleValidation cachedValidation;
        internal string description;
        internal string name;
        internal List<Rule> rules;
        internal const string RuleSetTrackingKey = "RuleSet.";
        private bool runtimeInitialized;
        private object syncLock;

        public RuleSet()
        {
            this.behavior = RuleChainingBehavior.Full;
            this.syncLock = new object();
            this.rules = new List<Rule>();
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleSet(string name) : this()
        {
            this.name = name;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleSet(string name, string description) : this(name)
        {
            this.description = description;
        }

        public RuleSet Clone()
        {
            RuleSet set = (RuleSet) base.MemberwiseClone();
            set.runtimeInitialized = false;
            if (this.rules != null)
            {
                set.rules = new List<Rule>();
                foreach (Rule rule in this.rules)
                {
                    set.rules.Add(rule.Clone());
                }
            }
            return set;
        }

        public override bool Equals(object obj)
        {
            RuleSet set = obj as RuleSet;
            if (set == null)
            {
                return false;
            }
            if (((this.Name != set.Name) || (this.Description != set.Description)) || ((this.ChainingBehavior != set.ChainingBehavior) || (this.Rules.Count != set.Rules.Count)))
            {
                return false;
            }
            for (int i = 0; i < this.rules.Count; i++)
            {
                if (!this.rules[i].Equals(set.rules[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public void Execute(RuleExecution ruleExecution)
        {
            if (ruleExecution == null)
            {
                throw new ArgumentNullException("ruleExecution");
            }
            if (ruleExecution.Validation == null)
            {
                throw new ArgumentException(SR.GetString("Error_MissingValidationProperty"), "ruleExecution");
            }
            new RuleEngine(this, ruleExecution.Validation, ruleExecution.ActivityExecutionContext).Execute(ruleExecution);
        }

        internal void Execute(Activity activity, ActivityExecutionContext executionContext)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            Type thisType = activity.GetType();
            RuleEngine cachedEngine = null;
            lock (this.syncLock)
            {
                if (((this.cachedEngine == null) || (this.cachedValidation == null)) || (this.cachedValidation.ThisType != thisType))
                {
                    RuleValidation validation = new RuleValidation(thisType, null);
                    cachedEngine = new RuleEngine(this, validation, executionContext);
                    this.cachedValidation = validation;
                    this.cachedEngine = cachedEngine;
                }
                else
                {
                    cachedEngine = this.cachedEngine;
                }
            }
            cachedEngine.Execute(activity, executionContext);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        internal void OnRuntimeInitialized()
        {
            lock (this.syncLock)
            {
                if (!this.runtimeInitialized)
                {
                    foreach (Rule rule in this.rules)
                    {
                        rule.OnRuntimeInitialized();
                    }
                    this.runtimeInitialized = true;
                }
            }
        }

        public bool Validate(RuleValidation validation)
        {
            if (validation == null)
            {
                throw new ArgumentNullException("validation");
            }
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (Rule rule in this.rules)
            {
                if (!string.IsNullOrEmpty(rule.Name))
                {
                    if (dictionary.ContainsKey(rule.Name))
                    {
                        ValidationError error = new ValidationError(Messages.Error_DuplicateRuleName, 0x53f);
                        error.UserData["ErrorObject"] = rule;
                        validation.AddError(error);
                    }
                    else
                    {
                        dictionary.Add(rule.Name, null);
                    }
                }
                rule.Validate(validation);
            }
            if ((validation.Errors != null) && (validation.Errors.Count != 0))
            {
                return false;
            }
            return true;
        }

        public RuleChainingBehavior ChainingBehavior
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.behavior;
            }
            set
            {
                if (this.runtimeInitialized)
                {
                    throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
                }
                this.behavior = value;
            }
        }

        public string Description
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.description;
            }
            set
            {
                if (this.runtimeInitialized)
                {
                    throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
                }
                this.description = value;
            }
        }

        public string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.name;
            }
            set
            {
                if (this.runtimeInitialized)
                {
                    throw new InvalidOperationException(SR.GetString("Error_CanNotChangeAtRuntime"));
                }
                this.name = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ICollection<Rule> Rules
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.rules;
            }
        }
    }
}

