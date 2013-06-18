namespace System.Workflow.Activities.Rules
{
    using System;
    using System.CodeDom;
    using System.Globalization;
    using System.Text;
    using System.Workflow.ComponentModel.Compiler;

    internal class AssignmentStatement : RuleCodeDomStatement
    {
        private CodeAssignStatement assignStatement;

        private AssignmentStatement(CodeAssignStatement assignStatement)
        {
            this.assignStatement = assignStatement;
        }

        internal override void AnalyzeUsage(RuleAnalysis analysis)
        {
            RuleExpressionWalker.AnalyzeUsage(analysis, this.assignStatement.Left, false, true, null);
            RuleExpressionWalker.AnalyzeUsage(analysis, this.assignStatement.Right, true, false, null);
        }

        internal override CodeStatement Clone()
        {
            return new CodeAssignStatement { Left = RuleExpressionWalker.Clone(this.assignStatement.Left), Right = RuleExpressionWalker.Clone(this.assignStatement.Right) };
        }

        internal static RuleCodeDomStatement Create(CodeStatement statement)
        {
            return new AssignmentStatement((CodeAssignStatement) statement);
        }

        internal override void Decompile(StringBuilder decompilation)
        {
            if (this.assignStatement.Right == null)
            {
                RuleEvaluationException exception = new RuleEvaluationException(Messages.AssignRightNull);
                exception.Data["ErrorObject"] = this.assignStatement;
                throw exception;
            }
            if (this.assignStatement.Left == null)
            {
                RuleEvaluationException exception2 = new RuleEvaluationException(Messages.AssignLeftNull);
                exception2.Data["ErrorObject"] = this.assignStatement;
                throw exception2;
            }
            RuleExpressionWalker.Decompile(decompilation, this.assignStatement.Left, null);
            decompilation.Append(" = ");
            RuleExpressionWalker.Decompile(decompilation, this.assignStatement.Right, null);
        }

        internal override void Execute(RuleExecution execution)
        {
            Type expressionType = execution.Validation.ExpressionInfo(this.assignStatement.Left).ExpressionType;
            Type operandType = execution.Validation.ExpressionInfo(this.assignStatement.Right).ExpressionType;
            RuleExpressionResult result = RuleExpressionWalker.Evaluate(execution, this.assignStatement.Left);
            RuleExpressionResult result2 = RuleExpressionWalker.Evaluate(execution, this.assignStatement.Right);
            result.Value = Executor.AdjustType(operandType, result2.Value, expressionType);
        }

        internal override bool Match(CodeStatement comperand)
        {
            CodeAssignStatement statement = comperand as CodeAssignStatement;
            return (((statement != null) && RuleExpressionWalker.Match(this.assignStatement.Left, statement.Left)) && RuleExpressionWalker.Match(this.assignStatement.Right, statement.Right));
        }

        internal override bool Validate(RuleValidation validation)
        {
            bool flag = false;
            RuleExpressionInfo info = null;
            if (this.assignStatement.Left == null)
            {
                ValidationError item = new ValidationError(Messages.NullAssignLeft, 0x541);
                item.UserData["ErrorObject"] = this.assignStatement;
                validation.Errors.Add(item);
            }
            else
            {
                info = validation.ExpressionInfo(this.assignStatement.Left);
                if (info == null)
                {
                    info = RuleExpressionWalker.Validate(validation, this.assignStatement.Left, true);
                }
            }
            RuleExpressionInfo info2 = null;
            if (this.assignStatement.Right == null)
            {
                ValidationError error2 = new ValidationError(Messages.NullAssignRight, 0x543);
                error2.UserData["ErrorObject"] = this.assignStatement;
                validation.Errors.Add(error2);
            }
            else
            {
                info2 = RuleExpressionWalker.Validate(validation, this.assignStatement.Right, false);
            }
            if ((info == null) || (info2 == null))
            {
                return flag;
            }
            Type expressionType = info2.ExpressionType;
            Type lhsType = info.ExpressionType;
            if (lhsType == typeof(NullLiteral))
            {
                ValidationError error3 = new ValidationError(Messages.NullAssignLeft, 0x542);
                error3.UserData["ErrorObject"] = this.assignStatement;
                validation.Errors.Add(error3);
                return false;
            }
            if (lhsType != expressionType)
            {
                ValidationError error = null;
                if (!RuleValidation.TypesAreAssignable(expressionType, lhsType, this.assignStatement.Right, out error))
                {
                    if (error == null)
                    {
                        error = new ValidationError(string.Format(CultureInfo.CurrentCulture, Messages.AssignNotAllowed, new object[] { RuleDecompiler.DecompileType(expressionType), RuleDecompiler.DecompileType(lhsType) }), 0x545);
                    }
                    error.UserData["ErrorObject"] = this.assignStatement;
                    validation.Errors.Add(error);
                    return flag;
                }
            }
            return true;
        }
    }
}

