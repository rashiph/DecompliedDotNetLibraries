namespace System.Xml
{
    using System;

    internal interface IDtdEntityInfo
    {
        string BaseUriString { get; }

        string DeclaredUriString { get; }

        bool IsDeclaredInExternal { get; }

        bool IsExternal { get; }

        bool IsParameterEntity { get; }

        bool IsUnparsedEntity { get; }

        int LineNumber { get; }

        int LinePosition { get; }

        string Name { get; }

        string PublicId { get; }

        string SystemId { get; }

        string Text { get; }
    }
}

