namespace System.Data
{
    using System;

    internal sealed class ParentForeignKeyConstraintEnumerator : ForeignKeyConstraintEnumerator
    {
        private DataTable table;

        public ParentForeignKeyConstraintEnumerator(DataSet dataSet, DataTable inTable) : base(dataSet)
        {
            this.table = inTable;
        }

        protected override bool IsValidCandidate(Constraint constraint)
        {
            return ((constraint is ForeignKeyConstraint) && (((ForeignKeyConstraint) constraint).RelatedTable == this.table));
        }
    }
}

