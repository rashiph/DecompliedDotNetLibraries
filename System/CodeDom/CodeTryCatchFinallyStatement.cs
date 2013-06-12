namespace System.CodeDom
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class CodeTryCatchFinallyStatement : CodeStatement
    {
        private CodeCatchClauseCollection catchClauses;
        private CodeStatementCollection finallyStatments;
        private CodeStatementCollection tryStatments;

        public CodeTryCatchFinallyStatement()
        {
            this.tryStatments = new CodeStatementCollection();
            this.finallyStatments = new CodeStatementCollection();
            this.catchClauses = new CodeCatchClauseCollection();
        }

        public CodeTryCatchFinallyStatement(CodeStatement[] tryStatements, CodeCatchClause[] catchClauses)
        {
            this.tryStatments = new CodeStatementCollection();
            this.finallyStatments = new CodeStatementCollection();
            this.catchClauses = new CodeCatchClauseCollection();
            this.TryStatements.AddRange(tryStatements);
            this.CatchClauses.AddRange(catchClauses);
        }

        public CodeTryCatchFinallyStatement(CodeStatement[] tryStatements, CodeCatchClause[] catchClauses, CodeStatement[] finallyStatements)
        {
            this.tryStatments = new CodeStatementCollection();
            this.finallyStatments = new CodeStatementCollection();
            this.catchClauses = new CodeCatchClauseCollection();
            this.TryStatements.AddRange(tryStatements);
            this.CatchClauses.AddRange(catchClauses);
            this.FinallyStatements.AddRange(finallyStatements);
        }

        public CodeCatchClauseCollection CatchClauses
        {
            get
            {
                return this.catchClauses;
            }
        }

        public CodeStatementCollection FinallyStatements
        {
            get
            {
                return this.finallyStatments;
            }
        }

        public CodeStatementCollection TryStatements
        {
            get
            {
                return this.tryStatments;
            }
        }
    }
}

