namespace System.Runtime.InteropServices.TCEAdapterGen
{
    using System;

    internal static class NameSpaceExtractor
    {
        private static char NameSpaceSeperator = '.';

        public static string ExtractNameSpace(string FullyQualifiedTypeName)
        {
            int length = FullyQualifiedTypeName.LastIndexOf(NameSpaceSeperator);
            if (length == -1)
            {
                return "";
            }
            return FullyQualifiedTypeName.Substring(0, length);
        }
    }
}

