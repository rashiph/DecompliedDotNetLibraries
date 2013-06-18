namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal delegate XmlQualifiedName GetQualifiedNameHandler(ExtendedPropertyInfo extendedProperty, WorkflowMarkupSerializationManager manager, out string prefix);
}

