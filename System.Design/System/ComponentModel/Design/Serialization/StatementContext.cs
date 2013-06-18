namespace System.ComponentModel.Design.Serialization
{
    using System;

    public sealed class StatementContext
    {
        private ObjectStatementCollection _statements;

        public ObjectStatementCollection StatementCollection
        {
            get
            {
                if (this._statements == null)
                {
                    this._statements = new ObjectStatementCollection();
                }
                return this._statements;
            }
        }
    }
}

