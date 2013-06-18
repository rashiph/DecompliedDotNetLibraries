namespace System.Web.RegularExpressions
{
    using System;
    using System.Collections;
    using System.Text.RegularExpressions;

    internal class BindParametersRegex : Regex
    {
        public BindParametersRegex()
        {
            base.pattern = "\\s*((\"(?<fieldName>(([\\w\\.]+)|(\\[.+\\])))\")|('(?<fieldName>(([\\w\\.]+)|(\\[.+\\])))'))\\s*(,\\s*((\"(?<formatString>.*)\")|('(?<formatString>.*)'))\\s*)?\\s*\\z";
            base.roptions = RegexOptions.Singleline | RegexOptions.Multiline;
            base.factory = new BindParametersRegexFactory18();
            base.capnames = new Hashtable();
            base.capnames.Add("1", 1);
            base.capnames.Add("2", 2);
            base.capnames.Add("3", 3);
            base.capnames.Add("4", 4);
            base.capnames.Add("5", 5);
            base.capnames.Add("6", 6);
            base.capnames.Add("7", 7);
            base.capnames.Add("fieldName", 14);
            base.capnames.Add("formatString", 15);
            base.capnames.Add("12", 12);
            base.capnames.Add("13", 13);
            base.capnames.Add("8", 8);
            base.capnames.Add("9", 9);
            base.capnames.Add("10", 10);
            base.capnames.Add("11", 11);
            base.capnames.Add("0", 0);
            base.capslist = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "fieldName", "formatString" };
            base.capsize = 0x10;
            base.InitializeReferences();
        }
    }
}

