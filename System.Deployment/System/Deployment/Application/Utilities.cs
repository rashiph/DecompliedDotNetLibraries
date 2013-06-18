namespace System.Deployment.Application
{
    using System;

    internal static class Utilities
    {
        public static int CompareWithNullEqEmpty(string s1, string s2, StringComparison comparisonType)
        {
            if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2))
            {
                return 0;
            }
            return string.Compare(s1, s2, comparisonType);
        }
    }
}

