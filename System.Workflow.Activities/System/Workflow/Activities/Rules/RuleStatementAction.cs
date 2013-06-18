namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Text;
    using System.Workflow.ComponentModel.Compiler;

    [Serializable]
    public class RuleStatementAction : RuleAction
    {
        private CodeStatement codeDomStatement;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleStatementAction()
        {
        }

        public RuleStatementAction(CodeExpression codeDomExpression)
        {
            this.codeDomStatement = new CodeExpressionStatement(codeDomExpression);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleStatementAction(CodeStatement codeDomStatement)
        {
            this.codeDomStatement = codeDomStatement;
        }

        public override RuleAction Clone()
        {
            RuleStatementAction action = (RuleStatementAction) base.MemberwiseClone();
            action.codeDomStatement = CodeDomStatementWalker.Clone(this.codeDomStatement);
            return action;
        }

        public override bool Equals(object obj)
        {
            RuleStatementAction action = obj as RuleStatementAction;
            return ((action != null) && CodeDomStatementWalker.Match(this.CodeDomStatement, action.CodeDomStatement));
        }

        public override void Execute(RuleExecution context)
        {
            if (this.codeDomStatement == null)
            {
                throw new InvalidOperationException(Messages.NullStatement);
            }
            CodeDomStatementWalker.Execute(context, this.codeDomStatement);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override ICollection<string> GetSideEffects(RuleValidation validation)
        {
            RuleAnalysis analysis = new RuleAnalysis(validation, true);
            if (this.codeDomStatement != null)
            {
                CodeDomStatementWalker.AnalyzeUsage(analysis, this.codeDomStatement);
            }
            return analysis.GetSymbols();
        }

        public override string ToString()
        {
            if (this.codeDomStatement == null)
            {
                return "";
            }
            StringBuilder stringBuilder = new StringBuilder();
            CodeDomStatementWalker.Decompile(stringBuilder, this.codeDomStatement);
            return stringBuilder.ToString();
        }

        public override bool Validate(RuleValidation validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException("validator");
            }
            if (this.codeDomStatement == null)
            {
                ValidationError error = new ValidationError(Messages.NullStatement, 0x53d);
                error.UserData["ErrorObject"] = this;
                validator.AddError(error);
                return false;
            }
            return CodeDomStatementWalker.Validate(validator, this.codeDomStatement);
        }

        public CodeStatement CodeDomStatement
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.codeDomStatement;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.codeDomStatement = value;
            }
        }
    }
}

