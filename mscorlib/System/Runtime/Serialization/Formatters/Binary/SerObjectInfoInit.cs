namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Collections;

    internal sealed class SerObjectInfoInit
    {
        internal int objectInfoIdCount = 1;
        internal SerStack oiPool = new SerStack("SerObjectInfo Pool");
        internal Hashtable seenBeforeTable = new Hashtable();
    }
}

