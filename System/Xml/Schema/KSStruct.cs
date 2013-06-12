namespace System.Xml.Schema
{
    using System;

    internal class KSStruct
    {
        public int depth;
        public LocatedActiveAxis[] fields;
        public KeySequence ks;

        public KSStruct(KeySequence ks, int dim)
        {
            this.ks = ks;
            this.fields = new LocatedActiveAxis[dim];
        }
    }
}

