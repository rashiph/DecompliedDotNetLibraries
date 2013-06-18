namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class RuleHaltAction : RuleAction
    {
        public override RuleAction Clone()
        {
            return (RuleAction) base.MemberwiseClone();
        }

        public override bool Equals(object obj)
        {
            return (obj is RuleHaltAction);
        }

        public override void Execute(RuleExecution context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            context.Halted = true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override ICollection<string> GetSideEffects(RuleValidation validation)
        {
            return null;
        }

        public override string ToString()
        {
            return "Halt";
        }

        public override bool Validate(RuleValidation validator)
        {
            return true;
        }
    }
}

