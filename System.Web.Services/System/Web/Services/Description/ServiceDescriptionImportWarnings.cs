namespace System.Web.Services.Description
{
    using System;

    [Flags]
    public enum ServiceDescriptionImportWarnings
    {
        NoCodeGenerated = 1,
        NoMethodsGenerated = 0x20,
        OptionalExtensionsIgnored = 2,
        RequiredExtensionsIgnored = 4,
        SchemaValidation = 0x40,
        UnsupportedBindingsIgnored = 0x10,
        UnsupportedOperationsIgnored = 8,
        WsiConformance = 0x80
    }
}

