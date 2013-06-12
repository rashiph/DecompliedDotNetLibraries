namespace System.Xml.Schema
{
    using System;

    internal enum ValidatorState
    {
        None,
        Start,
        TopLevelAttribute,
        TopLevelTextOrWS,
        Element,
        Attribute,
        EndOfAttributes,
        Text,
        Whitespace,
        EndElement,
        SkipToEndElement,
        Finish
    }
}

