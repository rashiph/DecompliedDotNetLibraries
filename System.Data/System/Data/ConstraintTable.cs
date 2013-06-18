namespace System.Data
{
    using System;
    using System.Xml.Schema;

    internal sealed class ConstraintTable
    {
        public XmlSchemaIdentityConstraint constraint;
        public DataTable table;

        public ConstraintTable(DataTable t, XmlSchemaIdentityConstraint c)
        {
            this.table = t;
            this.constraint = c;
        }
    }
}

