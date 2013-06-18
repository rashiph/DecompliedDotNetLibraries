namespace Microsoft.Build.Shared
{
    using System;
    using System.Text.RegularExpressions;

    internal static class FileUtilitiesRegex
    {
        internal static readonly Regex DrivePattern = new Regex("^[A-Za-z]:");
        internal static readonly Regex UNCPattern = new Regex(string.Format(CultureInfo.InvariantCulture, @"^[\{0}\{1}][\{0}\{1}][^\{0}\{1}]+[\{0}\{1}][^\{0}\{1}]+", new object[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }));
    }
}

