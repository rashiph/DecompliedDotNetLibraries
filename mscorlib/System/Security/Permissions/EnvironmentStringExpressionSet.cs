namespace System.Security.Permissions
{
    using System;
    using System.Security.Util;

    [Serializable]
    internal class EnvironmentStringExpressionSet : StringExpressionSet
    {
        public EnvironmentStringExpressionSet() : base(true, null, false)
        {
        }

        public EnvironmentStringExpressionSet(string str) : base(true, str, false)
        {
        }

        protected override StringExpressionSet CreateNewEmpty()
        {
            return new EnvironmentStringExpressionSet();
        }

        protected override string ProcessSingleString(string str)
        {
            return str;
        }

        protected override string ProcessWholeString(string str)
        {
            return str;
        }

        protected override bool StringSubsetString(string left, string right, bool ignoreCase)
        {
            if (!ignoreCase)
            {
                return (string.Compare(left, right, StringComparison.Ordinal) == 0);
            }
            return (string.Compare(left, right, StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
}

