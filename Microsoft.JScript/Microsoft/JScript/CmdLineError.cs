namespace Microsoft.JScript
{
    using System;

    public enum CmdLineError
    {
        AssemblyNotFound = 0x7d1,
        CannotCreateEngine = 0x7d2,
        CompilerConstant = 0x7d3,
        DuplicateFileAsSourceAndAssembly = 0x7d4,
        DuplicateResourceFile = 0x7d5,
        DuplicateResourceName = 0x7d6,
        DuplicateSourceFile = 0x7d7,
        ErrorSavingCompiledState = 0x7d8,
        IncompatibleTargets = 0x7f6,
        InvalidAssembly = 0x7d9,
        InvalidCharacters = 0x7f4,
        InvalidCodePage = 0x7da,
        InvalidDefinition = 0x7db,
        InvalidForCompilerOptions = 0x7f5,
        InvalidLocaleID = 0x7dc,
        InvalidPlatform = 0x7f7,
        InvalidSourceFile = 0x7de,
        InvalidTarget = 0x7dd,
        InvalidVersion = 0x7ef,
        InvalidWarningLevel = 0x7df,
        LAST = 0x7f7,
        ManagedResourceNotFound = 0x7e6,
        MissingDefineArgument = 0x7e2,
        MissingExtension = 0x7e3,
        MissingLibArgument = 0x7e4,
        MissingReference = 0x7f2,
        MissingVersionInfo = 0x7e5,
        MultipleOutputNames = 0x7e0,
        MultipleTargets = 0x7e1,
        MultipleWin32Resources = 0x7f1,
        NestedResponseFiles = 0x7e7,
        NoCodePage = 0x7e8,
        NoError = 0,
        NoFileName = 0x7e9,
        NoInputSourcesSpecified = 0x7ea,
        NoLocaleID = 0x7eb,
        NoWarningLevel = 0x7ec,
        ResourceNotFound = 0x7ed,
        SourceFileTooBig = 0x7f0,
        SourceNotFound = 0x7f3,
        UnknownOption = 0x7ee,
        Unspecified = 0xbb7
    }
}

