namespace System.Data
{
    using System;

    internal sealed class ChildForeignKeyConstraintEnumerator : ForeignKeyConstraintEnumerator
    {
        private DataTable table;

        public ChildForeignKeyConstraintEnumerator(DataSet dataSet, DataTable inTable) : base(dataSet)
        {
            this.table = inTable;
        }

        protected override bool IsValidCandidate(Constraint constraint)
        {
            return ((constraint is ForeignKeyConstraint) && (((ForeignKeyConstraint) constraint).Table == this.table));
        }
    }
}

