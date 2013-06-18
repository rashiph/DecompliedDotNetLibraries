namespace System.ComponentModel.Design.Data
{
    using System;
    using System.Collections;

    public interface IDesignerDataSchema
    {
        ICollection GetSchemaItems(DesignerDataSchemaClass schemaClass);
        bool SupportsSchemaClass(DesignerDataSchemaClass schemaClass);
    }
}

