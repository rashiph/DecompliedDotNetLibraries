namespace System.Xml
{
    using System;

    internal interface IDtdParserAdapterWithValidation : IDtdParserAdapter
    {
        bool DtdValidation { get; }

        IValidationEventHandling ValidationEventHandling { get; }
    }
}

