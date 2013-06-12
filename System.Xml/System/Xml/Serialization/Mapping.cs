namespace System.Xml.Serialization
{
    using System;

    internal abstract class Mapping
    {
        private bool isSoap;

        internal Mapping()
        {
        }

        internal bool IsSoap
        {
            get
            {
                return this.isSoap;
            }
            set
            {
                this.isSoap = value;
            }
        }
    }
}

