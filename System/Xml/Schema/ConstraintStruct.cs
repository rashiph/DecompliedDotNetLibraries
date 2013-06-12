namespace System.Xml.Schema
{
    using System;
    using System.Collections;

    internal sealed class ConstraintStruct
    {
        internal ArrayList axisFields;
        internal SelectorActiveAxis axisSelector;
        internal CompiledIdentityConstraint constraint;
        internal Hashtable keyrefTable;
        internal Hashtable qualifiedTable;
        private int tableDim;

        internal ConstraintStruct(CompiledIdentityConstraint constraint)
        {
            this.constraint = constraint;
            this.tableDim = constraint.Fields.Length;
            this.axisFields = new ArrayList();
            this.axisSelector = new SelectorActiveAxis(constraint.Selector, this);
            if (this.constraint.Role != CompiledIdentityConstraint.ConstraintRole.Keyref)
            {
                this.qualifiedTable = new Hashtable();
            }
        }

        internal int TableDim
        {
            get
            {
                return this.tableDim;
            }
        }
    }
}

