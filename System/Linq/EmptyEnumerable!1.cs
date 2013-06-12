namespace System.Linq
{
    using System;
    using System.Collections.Generic;

    internal class EmptyEnumerable<TElement>
    {
        private static TElement[] instance;

        public static IEnumerable<TElement> Instance
        {
            get
            {
                if (EmptyEnumerable<TElement>.instance == null)
                {
                    EmptyEnumerable<TElement>.instance = new TElement[0];
                }
                return EmptyEnumerable<TElement>.instance;
            }
        }
    }
}

