namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodeConditionStatement : CodeStatement
    {
        private CodeExpression condition;
        private CodeStatementCollection falseStatments;
        private CodeStatementCollection trueStatments;

        public CodeConditionStatement()
        {
            this.trueStatments = new CodeStatementCollection();
            this.falseStatments = new CodeStatementCollection();
        }

        public CodeConditionStatement(CodeExpression condition, params CodeStatement[] trueStatements)
        {
            this.trueStatments = new CodeStatementCollection();
            this.falseStatments = new CodeStatementCollection();
            this.Condition = condition;
            this.TrueStatements.AddRange(trueStatements);
        }

        public CodeConditionStatement(CodeExpression condition, CodeStatement[] trueStatements, CodeStatement[] falseStatements)
        {
            this.trueStatments = new CodeStatementCollection();
            this.falseStatments = new CodeStatementCollection();
            this.Condition = condition;
            this.TrueStatements.AddRange(trueStatements);
            this.FalseStatements.AddRange(falseStatements);
        }

        public CodeExpression Condition
        {
            get
            {
                return this.condition;
            }
            set
            {
                this.condition = value;
            }
        }

        public CodeStatementCollection FalseStatements
        {
            get
            {
                return this.falseStatments;
            }
        }

        public CodeStatementCollection TrueStatements
        {
            get
            {
                return this.trueStatments;
            }
        }
    }
}

