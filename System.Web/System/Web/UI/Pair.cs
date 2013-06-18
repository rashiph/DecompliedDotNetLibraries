namespace System.Web.UI
{
    using System;

    [Serializable]
    public sealed class Pair
    {
        public object First;
        public object Second;

        public Pair()
        {
        }

        public Pair(object x, object y)
        {
            this.First = x;
            this.Second = y;
        }
    }
}

