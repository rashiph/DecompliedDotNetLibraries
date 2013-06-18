namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Collections;

    internal class ADAttribute
    {
        public string Name;
        public ArrayList Values = new ArrayList();

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }
}

