namespace System.Diagnostics
{
    using System;
    using System.Configuration;

    internal class SwitchesDictionarySectionHandler : DictionarySectionHandler
    {
        protected override string KeyAttributeName
        {
            get
            {
                return "name";
            }
        }

        internal override bool ValueRequired
        {
            get
            {
                return true;
            }
        }
    }
}

