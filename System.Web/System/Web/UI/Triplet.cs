namespace System.Web.UI
{
    using System;

    [Serializable]
    public sealed class Triplet
    {
        public object First;
        public object Second;
        public object Third;

        public Triplet()
        {
        }

        public Triplet(object x, object y)
        {
            this.First = x;
            this.Second = y;
        }

        public Triplet(object x, object y, object z)
        {
            this.First = x;
            this.Second = y;
            this.Third = z;
        }
    }
}

