namespace System.Xml.Schema
{
    using System;

    internal enum AttributeMatchState
    {
        AttributeFound,
        AnyIdAttributeFound,
        UndeclaredElementAndAttribute,
        UndeclaredAttribute,
        AnyAttributeLax,
        AnyAttributeSkip,
        ProhibitedAnyAttribute,
        ProhibitedAttribute,
        AttributeNameMismatch,
        ValidateAttributeInvalidCall
    }
}

