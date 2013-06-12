namespace System.Xml.Schema
{
    using System;
    using System.Collections;

    internal class RestrictionFacets
    {
        internal ArrayList Enumeration;
        internal RestrictionFlags FixedFlags;
        internal RestrictionFlags Flags;
        internal int FractionDigits;
        internal int Length;
        internal object MaxExclusive;
        internal object MaxInclusive;
        internal int MaxLength;
        internal object MinExclusive;
        internal object MinInclusive;
        internal int MinLength;
        internal ArrayList Patterns;
        internal int TotalDigits;
        internal XmlSchemaWhiteSpace WhiteSpace;
    }
}

