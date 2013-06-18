namespace Microsoft.Build.Framework.XamlTypes
{
    using System;
    using System.Collections.Generic;

    public interface IProjectSchemaNode
    {
        IEnumerable<object> GetSchemaObjects(Type type);
        IEnumerable<Type> GetSchemaObjectTypes();
    }
}

