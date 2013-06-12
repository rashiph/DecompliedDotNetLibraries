namespace System
{
    using System.Collections;
    using System.Text;

    internal interface ITuple
    {
        int GetHashCode(IEqualityComparer comparer);
        string ToString(StringBuilder sb);

        int Size { get; }
    }
}

