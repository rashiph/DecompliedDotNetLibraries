namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    public class SortKey
    {
        private string name;
        private string rule;
        private bool order;
        public SortKey()
        {
            Utility.CheckOSVersion();
        }

        public SortKey(string attributeName, string matchingRule, bool reverseOrder)
        {
            Utility.CheckOSVersion();
            this.AttributeName = attributeName;
            this.rule = matchingRule;
            this.order = reverseOrder;
        }

        public string AttributeName
        {
            get
            {
                return this.name;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.name = value;
            }
        }
        public string MatchingRule
        {
            get
            {
                return this.rule;
            }
            set
            {
                this.rule = value;
            }
        }
        public bool ReverseOrder
        {
            get
            {
                return this.order;
            }
            set
            {
                this.order = value;
            }
        }
    }
}

