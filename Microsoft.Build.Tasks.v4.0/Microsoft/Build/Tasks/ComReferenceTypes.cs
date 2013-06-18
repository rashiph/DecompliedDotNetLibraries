namespace Microsoft.Build.Tasks
{
    using System;

    internal static class ComReferenceTypes
    {
        internal const string aximp = "aximp";
        internal const string primary = "primary";
        internal const string primaryortlbimp = "primaryortlbimp";
        internal const string tlbimp = "tlbimp";

        internal static bool IsAxImp(string refType)
        {
            return (string.Compare(refType, "aximp", StringComparison.OrdinalIgnoreCase) == 0);
        }

        internal static bool IsPia(string refType)
        {
            return (string.Compare(refType, "primary", StringComparison.OrdinalIgnoreCase) == 0);
        }

        internal static bool IsPiaOrTlbImp(string refType)
        {
            return (string.Compare(refType, "primaryortlbimp", StringComparison.OrdinalIgnoreCase) == 0);
        }

        internal static bool IsTlbImp(string refType)
        {
            return (string.Compare(refType, "tlbimp", StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
}

