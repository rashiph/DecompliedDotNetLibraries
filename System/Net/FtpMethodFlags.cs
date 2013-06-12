namespace System.Net
{
    using System;

    [Flags]
    internal enum FtpMethodFlags
    {
        DoesNotTakeParameter = 0x10,
        HasHttpCommand = 0x80,
        IsDownload = 1,
        IsUpload = 2,
        MayTakeParameter = 8,
        MustChangeWorkingDirectoryToPath = 0x100,
        None = 0,
        ParameterIsDirectory = 0x20,
        ShouldParseForResponseUri = 0x40,
        TakesParameter = 4
    }
}

