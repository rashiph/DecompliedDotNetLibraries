namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Text;
    using System.Workflow.ComponentModel.Compiler;

    internal class ExpressionStatement : RuleCodeDomStatement
    {
        private CodeExpressionStatement exprStatement;

        private ExpressionStatement(CodeExpressionStatement exprStatement)
        {
            this.exprStatement = exprStatement;
        }

        internal override void AnalyzeUsage(RuleAnalysis analysis)
        {
            RuleExpressionWalker.AnalyzeUsage(analysis, this.exprStatement.Expression, false, false, null);
        }

        internal override CodeStatement Clone()
        {
            return new CodeExpressionStatement { Expression = RuleExpressionWalker.Clone(this.exprStatement.Expression) };
        }

        internal static RuleCodeDomStatement Create(CodeStatement statement)
        {
            return new ExpressionStatement((CodeExpressionStatement) statement);
        }

        internal override void Decompile(StringBuilder decompilation)
        {
            if (this.exprStatement.Expression == null)
            {
                RuleEvaluationException exception = new RuleEvaluationException(Messages.InvokeStatementNull);
                exception.Data["ErrorObject"] = this.exprStatement;
                throw exception;
            }
            RuleExpressionWalker.Decompile(decompilation, this.exprStatement.Expression, null);
        }

        internal override void Execute(RuleExecution execution)
        {
            RuleExpressionWalker.Evaluate(execution, this.exprStatement.Expression);
        }

        internal override bool Match(CodeStatement comperand)
        {
            CodeExpressionStatement statement = comperand as CodeExpressionStatement;
            return ((statement != null) && RuleExpressionWalker.Match(this.exprStatement.Expression, statement.Expression));
        }

        internal override bool Validate(RuleValidation validation)
        {
            bool flag = false;
            if (this.exprStatement.Expression == null)
            {
                ValidationError error = new ValidationError(Messages.NullInvokeStatementExpression, 0x53d);
                error.UserData["ErrorObject"] = this.exprStatement;
                validation.Errors.Add(error);
                return flag;
            }
            if (this.exprStatement.Expression is CodeMethodInvokeExpression)
            {
                return (RuleExpressionWalker.Validate(validation, this.exprStatement.Expression, false) != null);
            }
            ValidationError item = new ValidationError(Messages.InvokeNotHandled, 0x548);
            item.UserData["ErrorObject"] = this.exprStatement;
            validation.Errors.Add(item);
            return flag;
        }
    }
}

