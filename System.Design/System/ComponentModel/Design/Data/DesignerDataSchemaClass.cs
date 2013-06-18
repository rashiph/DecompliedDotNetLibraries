namespace System.ComponentModel.Design.Data
{
    using System;

    public sealed class DesignerDataSchemaClass
    {
        public static readonly DesignerDataSchemaClass StoredProcedures = new DesignerDataSchemaClass();
        public static readonly DesignerDataSchemaClass Tables = new DesignerDataSchemaClass();
        public static readonly DesignerDataSchemaClass Views = new DesignerDataSchemaClass();

        private DesignerDataSchemaClass()
        {
        }
    }
}

