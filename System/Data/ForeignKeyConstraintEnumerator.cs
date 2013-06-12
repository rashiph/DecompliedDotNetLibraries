namespace System.Data
{
    using System;

    internal class ForeignKeyConstraintEnumerator : ConstraintEnumerator
    {
        public ForeignKeyConstraintEnumerator(DataSet dataSet) : base(dataSet)
        {
        }

        public ForeignKeyConstraint GetForeignKeyConstraint()
        {
            return (ForeignKeyConstraint) base.CurrentObject;
        }

        protected override bool IsValidCandidate(Constraint constraint)
        {
            return (constraint is ForeignKeyConstraint);
        }
    }
}

