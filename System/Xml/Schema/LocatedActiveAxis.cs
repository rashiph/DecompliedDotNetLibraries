namespace System.Xml.Schema
{
    using System;

    internal class LocatedActiveAxis : ActiveAxis
    {
        private int column;
        internal bool isMatched;
        internal KeySequence Ks;

        internal LocatedActiveAxis(Asttree astfield, KeySequence ks, int column) : base(astfield)
        {
            this.Ks = ks;
            this.column = column;
            this.isMatched = false;
        }

        internal void Reactivate(KeySequence ks)
        {
            base.Reactivate();
            this.Ks = ks;
        }

        internal int Column
        {
            get
            {
                return this.column;
            }
        }
    }
}

