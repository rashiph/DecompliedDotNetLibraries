namespace System.Xml
{
    using System;

    internal interface IDtdDefaultAttributeInfo : IDtdAttributeInfo
    {
        string DefaultValueExpanded { get; }

        object DefaultValueTyped { get; }

        int ValueLineNumber { get; }

        int ValueLinePosition { get; }
    }
}

