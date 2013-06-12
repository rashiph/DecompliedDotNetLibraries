namespace System.Xml
{
    using System;

    internal interface IDtdParserAdapterV1 : IDtdParserAdapterWithValidation, IDtdParserAdapter
    {
        bool Namespaces { get; }

        bool Normalization { get; }

        bool V1CompatibilityMode { get; }
    }
}

