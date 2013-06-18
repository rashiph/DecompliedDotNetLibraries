namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Collections;

    internal class ADSubstringFilter
    {
        public ArrayList Any = new ArrayList();
        public ADValue Final = null;
        public ADValue Initial = null;
        public string Name;
    }
}

